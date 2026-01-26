import numpy as np
import constants

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

VERTICAL_WRONG_DIRECTION_PENALTY = -0.2
VERTICAL_RIGHT_DIRECTION_REWARD = 0.1


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
    ):
        self.id = plane_id

        # -----------------------------
        # State (truth)
        # -----------------------------
        self.position_nm = position_nm.astype(np.float32)     # NM
        self.heading = float(heading_rads)                 # rad
        self.speed = float(speed_kts)                 # knots

        self.altitude = init_altitude_ft                 # ft
        self.vert_speed = 0.0                         # ft/min
        self.current_turn_rate = 0.0                  # rad/sec

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
        self.landed = False
        self.is_arrival = is_arrival
        self.destination = destination_nm  # NM

    # -----------------------------
    # ATC command interface
    # -----------------------------
    def set_targets(self, heading_cmd_norm, speed_cmd_norm, altitude_cmd_norm):
        """
        heading_cmd_norm: [-1,1] → [0, 2pi]
        speed_cmd_norm:   absolute knots
        altitude_cmd_norm: absolute ft
        """

        new_heading = (heading_cmd_norm + 1.0) * np.pi
        new_speed = speed_cmd_norm
        new_altitude = altitude_cmd_norm

        instruction_count = 0

        # Heading
        if abs((self.target_heading - new_heading + np.pi) % (2*np.pi) - np.pi) > np.deg2rad(1.0):
            self.target_heading = new_heading
            instruction_count += 1

        # Speed
        if abs(self.target_speed - new_speed) > 5.0:
            self.target_speed = np.clip(new_speed, self.min_speed, self.max_speed)
            instruction_count += 1

        # Altitude
        if abs(self.target_altitude - new_altitude) > 100.0:
            self.target_altitude = np.clip(new_altitude, MIN_ALTITUDE, MAX_ALTITUDE)
            instruction_count += 1

        return instruction_count

    # -----------------------------
    # Pilot controllers
    # -----------------------------
    def pilot_speed_control(self, dt):
        speed_error = self.target_speed - self.speed
        accel = SPEED_KP * speed_error
        accel = np.clip(accel, -constants.MAX_ACCEL, constants.MAX_ACCEL)
        self.speed += accel
        self.speed = np.clip(self.speed, self.min_speed, self.max_speed)

    def pilot_heading_control(self, dt):
        heading_err = (self.target_heading - self.heading + np.pi) % (2*np.pi) - np.pi
        turn_rate_cmd = HEADING_KP * heading_err

        max_turn = self.max_turn_rate
        self.current_turn_rate = np.clip(turn_rate_cmd, -max_turn, max_turn)
        self.heading_error_deg = np.rad2deg(heading_err)

    def pilot_altitude_control(self, dt):
        alt_error = self.target_altitude - self.altitude

        if abs(alt_error) < ALT_DEADBAND:
            vs_cmd = 0.0
        else:
            vs_cmd = ALTITUDE_KP * alt_error * 60.0  # scale to ft/min

        vs_cmd = np.clip(vs_cmd, -MAX_VERT_SPEED, MAX_VERT_SPEED)

        max_vs_delta = MAX_VERT_ACCEL * (dt / 60.0)
        vs_error = vs_cmd - self.vert_speed
        vs_change = np.clip(vs_error, -max_vs_delta, max_vs_delta)

        self.vert_speed += vs_change

    def apply_control(self, dt):
        self.pilot_speed_control(dt)
        self.pilot_heading_control(dt)
        self.pilot_altitude_control(dt)

    # -----------------------------
    # Physics update
    # -----------------------------
    def step(self, dt: float):
        if self.landed:
            return

        self.apply_control(dt)

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

        self.position_nm += direction * groundspeed_nm_per_sec * dt

    # -----------------------------
    # Pilot-local shaping reward
    # -----------------------------
    def compute_pilot_reward(self, curriculum_stage: int):
        reward = 0.0

        heading_error = abs(self.heading_error_deg)
        altitude_error = abs(self.altitude - self.target_altitude)
        speed_error = abs(self.speed - self.target_speed)
        vs_error = abs(self.vert_speed)
        turn_rate = abs(self.current_turn_rate)
        dist_nm = np.linalg.norm(self.position_nm - self.destination)

        # Arrival vertical intent
        if self.is_arrival:
            if self.target_altitude < self.altitude:
                reward += VERTICAL_RIGHT_DIRECTION_REWARD
            else:
                reward += VERTICAL_WRONG_DIRECTION_PENALTY

        # Stage shaping
        if curriculum_stage >= 0:
            reward += 1.0 * (1.0 - min(heading_error / 30.0, 1.0))

        if curriculum_stage >= 1:
            reward += 1.0 * (1.0 - min(altitude_error / 2000.0, 1.0))

        if curriculum_stage >= 2:
            reward += 0.5 * (1.0 - min(heading_error / 15.0, 1.0))
            reward += 0.5 * (1.0 - min(altitude_error / 1000.0, 1.0))

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

        reward -= 0.01
        return reward

    # -----------------------------
    # Landing check (NM + ft)
    # -----------------------------
    def check_landing(self, airport_pos_nm: np.ndarray, landing_radius_nm: float) -> bool:
        if self.landed:
            return False

        dist_nm = np.linalg.norm(self.position_nm - airport_pos_nm)

        if (
            dist_nm <= landing_radius_nm
            and self.altitude <= 1500.0
            and abs(self.vert_speed) <= 700.0
            and abs(self.current_turn_rate) <= np.deg2rad(3.0)
            and self.speed <= APPROACH_SPEED
        ):
            self.landed = True
            return True

        return False
