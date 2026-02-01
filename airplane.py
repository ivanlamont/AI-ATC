import numpy as np
import constants
from conversion import wrap_angle

# -----------------------------
# Units:
#  pos        = NM (nautical miles)
#  speed      = knots (NM/hour)
#  vert_speed = ft/min
#  altitude   = ft
#  heading    = radians
#  turn_rate  = rad/sec
#  dt         = seconds
# -----------------------------

MAX_VERT_SPEED = 3000.0     # ft/min
MAX_VERT_ACCEL = 1000.0    # ft/min^2
MIN_ALTITUDE = 0.0         # ft
MAX_ALTITUDE = 40000.0     # ft

ALTITUDE_KP = 0.002        # tuned for ft
HEADING_KP = 0.8           # tuned for radians
SPEED_KP = 0.02            # knots control

ALT_DEADBAND = 100.0       # ft
APPROACH_SPEED = 150.0     # knots

TERMINAL_RADIUS = 20.0     # NM

VERTICAL_WRONG_DIRECTION_PENALTY = -1.0
VERTICAL_RIGHT_DIRECTION_REWARD = 1.0

class Airplane:
    def __init__(
        self,
        plane_id: int,
        position_nm: np.ndarray,
        destination_nm: np.ndarray,
        heading_rads: float,
        speed_kts: float,
        min_speed_kts: float,
        max_speed_kts: float,
        max_turn_rate_rads: float,   # rad/sec
        init_altitude_ft: float = 1000.0,
        is_arrival: bool = True,
        is_vfr: bool = False,
    ):
        self.id = plane_id
        self.last_clearance_time = 0.0

        # Flight rules type
        self.is_vfr = is_vfr  # True for VFR, False for IFR

        # VFR-specific constraints
        if is_vfr:
            self.max_altitude = 10000.0  # VFR typically below 10,000 ft
            self.min_alt_for_controlled_airspace = 500.0  # VFR minimum altitudes
        else:
            self.max_altitude = MAX_ALTITUDE  # IFR can go higher
            self.min_alt_for_controlled_airspace = 0.0

        # -----------------------------
        # State (truth)
        # -----------------------------
        self.position_nm = position_nm.astype(np.float32)     # NM
        self.heading = float(heading_rads)                 # rad
        self.speed = float(speed_kts)                 # knots

        self.altitude = init_altitude_ft                 # ft
        self.vert_speed = 0.0                         # ft/min
        self.current_turn_rate = 0.0                  # rad/sec
        self.accel = 0.0                              # knots/sec

        # VFR flight following indicator
        self.has_flight_following = False  # VFR can request flight following
        self.on_radar = False  # Whether ATC has positive control

        # -----------------------------
        # Targets (ATC commands)
        # -----------------------------
        self.target_heading = self.heading
        self.target_speed = self.speed
        self.target_altitude = init_altitude_ft

        # -----------------------------
        # Limits
        # -----------------------------
        self.min_speed = min_speed_kts
        self.max_speed = max_speed_kts
        self.max_turn_rate = max_turn_rate_rads

        # -----------------------------
        # Meta
        # -----------------------------
        self.heading_error_deg = 0.0
        self.dist_to_faf = np.inf
        self.prev_dist_nm = np.inf
        self.landed = False
        self.is_arrival = is_arrival
        self.destination = destination_nm  # NM


    def apply_atc_clearance(self, turn_rate_cmd, accel_cmd, vert_speed_cmd, dt):
        self.current_turn_rate = np.clip(turn_rate_cmd, -self.max_turn_rate, self.max_turn_rate)
        self.vert_speed = np.clip(vert_speed_cmd, -MAX_VERT_SPEED, MAX_VERT_SPEED)
        self.accel = np.clip(accel_cmd, -constants.MAX_ACCEL, constants.MAX_ACCEL)
        return 1

    # -----------------------------
    # Physics update
    # -----------------------------
    def step(self, dt: float):
        if self.landed:
            return

        # Speed integration
        self.speed += self.accel * dt
        self.speed = np.clip(self.speed, self.min_speed, self.max_speed)

        self.dist_to_faf = np.inf  # Set to inf if no env

        # Heading integration
        self.heading += self.current_turn_rate * dt

        # Altitude integration (ft)
        self.altitude += (self.vert_speed / 60.0) * dt
        self.altitude = np.clip(self.altitude, MIN_ALTITUDE, MAX_ALTITUDE)

        # Lateral integration (NM)
        # knots → NM/sec
        groundspeed_nm_per_sec = self.speed / 3600.0

        direction = np.array([
            np.cos(self.heading),
            np.sin(self.heading)
        ], dtype=np.float32)

        dist_travelled = groundspeed_nm_per_sec * dt
        self.position_nm += direction * dist_travelled

    # -----------------------------
    # Pilot-local shaping reward
    # -----------------------------
    def compute_pilot_reward(self, curriculum_stage: int = 0):
        reward = 0.0

        heading_error = abs(self.heading_error_deg)
        altitude_error = abs(self.altitude - self.target_altitude)
        speed_error = abs(self.speed - self.target_speed)
        vs_error = abs(self.vert_speed)
        turn_rate = abs(self.current_turn_rate)
        dist_nm = np.linalg.norm(self.position_nm - self.destination.position_nm)

        # Glide path logic
        if self.is_arrival:
            ideal_alt_ft = self.destination.altitude_ft + dist_nm * 318
            alt_error = self.altitude - ideal_alt_ft

            # Only care if ABOVE glide path
            if self.is_arrival:
                ideal_alt_ft = self.destination.altitude_ft + dist_nm * 318
                above_glide_ft = self.altitude - ideal_alt_ft

                if above_glide_ft > 0:
                    # Strong penalty for being high
                    above_glide_norm = above_glide_ft / 3000.0   # 3000 ft ~ very bad
                    above_glide_norm = np.clip(above_glide_norm, 0.0, 2.0)

                    reward -= 2.0 * above_glide_norm


        # ----- Vertical speed shaping -----
        vs_norm = abs(self.vert_speed) / 1500.0   # 1500 fpm ~ aggressive
        vs_norm = np.clip(vs_norm, 0.0, 2.0)

        reward -= 0.5 * vs_norm

        if self.is_arrival and self.vert_speed > 0:
            reward -= 0.01 * self.vert_speed
        if self.is_arrival and self.vert_speed < -300:
            reward += 0.2

        # ----- Distance progress (CRITICAL) -----
        progress_nm = self.prev_dist_nm - dist_nm
        if not np.isfinite(progress_nm):
            progress_nm = 0.0
        reward += 5.0 * progress_nm
        self.prev_dist_nm = dist_nm

        # explicit “too high near runway” penalty when we are training early stages on the localizer
        if curriculum_stage < 2:
            if self.is_arrival and dist_nm < 8.0 and self.altitude > 3000:
                reward -= 50.0

        # Stage shaping
        if curriculum_stage >= 0:
            reward += 1.0 * (1.0 - min(heading_error / 30.0, 1.0))

        if curriculum_stage >= 2:
            reward += 0.5 * (1.0 - min(heading_error / 15.0, 1.0))
            if dist_nm < TERMINAL_RADIUS:
                reward += 1.0 * (1.0 - dist_nm / TERMINAL_RADIUS)

        if curriculum_stage >= 3:
            reward += 0.5 * (1.0 - min(speed_error / 40.0, 1.0))

        if curriculum_stage >= 4:
            reward += 0.3 * (1.0 - min(vs_error / 500.0, 1.0))
            reward += 0.3 * (1.0 - min(turn_rate / np.deg2rad(3.0), 1.0))

        if curriculum_stage >= 5:
            if self.landed:
                reward += 50.0
            if dist_nm < TERMINAL_RADIUS * 2:
                if self.altitude < 2000:
                    reward += 5.0
                if self.speed < APPROACH_SPEED:
                    reward += 5.0

        if self.dist_to_faf < 1.0:
            reward += 2.0  # crossed FAF correctly

        stage_scale = {
            0: 1.0,
            1: 1.0,
            2: 1.0,
            3: 0.7,
            4: 0.5,
            5: 0.3,
        }.get(curriculum_stage, 0.3)

        reward *= stage_scale

        reward = np.clip(reward-0.01, -10.0, 10.0)
        return reward

    # -----------------------------
    # Landing check (NM + ft)
    # Different criteria for VFR vs IFR
    # -----------------------------
    def check_landing(self, airport_pos_nm: np.ndarray, landing_radius_nm: float) -> bool:
        if self.landed:
            return False

        dist_nm = np.linalg.norm(self.position_nm - airport_pos_nm)

        # VFR aircraft have slightly more relaxed landing criteria
        if self.is_vfr:
            altitude_threshold = 1000.0  # VFR can land at lower altitude
            vs_threshold = 800.0  # More relaxed vertical speed
            turn_rate_threshold = np.deg2rad(5.0)  # More relaxed turn rate
            approach_speed = APPROACH_SPEED + 10.0  # Slightly higher approach speed
        else:
            altitude_threshold = 1500.0  # IFR stricter
            vs_threshold = 700.0
            turn_rate_threshold = np.deg2rad(3.0)
            approach_speed = APPROACH_SPEED

        if (
            dist_nm <= landing_radius_nm
            and self.altitude <= altitude_threshold
            and abs(self.vert_speed) <= vs_threshold
            and abs(self.current_turn_rate) <= turn_rate_threshold
            and self.speed <= approach_speed
        ):
            self.landed = True
            return True

        return False
