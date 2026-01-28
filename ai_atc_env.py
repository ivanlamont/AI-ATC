import numpy as np
import gymnasium as gym
from gymnasium import spaces

from airplane import MAX_ALTITUDE, MIN_ALTITUDE, Airplane
from runway import Runway
from airport import Airport

from constants import CLEARANCE_INTERVAL_S, INITIAL_SPACING_NM, MAX_ACCEL, MAX_PLANE_COUNT, MAX_TURN_RATE, MAX_VERT_SPEED, MAX_ALTITUDE_CHANGE_PER_STEP, MAX_SIM_SECONDS
      
class AIATCEnv(gym.Env):
    """
    AI-ATC Environment with N airplanes (fixed observation/action size).
    """

    metadata = {"render_modes": []}

    def __init__(
        self,
        max_planes: int = MAX_PLANE_COUNT,
        airport_pos: np.ndarray | None = None,
        dt: float = 1.0,
        max_episode_steps: int = 2000,
        render_mode: str | None = None,
    ):
        super().__init__()

        self.curriculum_stage = 0
        self.render_mode = render_mode
        # -----------------------------
        # Core config
        # -----------------------------
        self.max_planes = max_planes
        self.sim_time = 0.0
        self.step_count = 0
        self.dt = dt
        self.max_episode_steps = max_episode_steps

        self.airport = Airport(
            position_nm=(
                airport_pos.astype(np.float32)
                if airport_pos is not None
                else np.array([0.0, 0.0], dtype=np.float32)
            ),
            altitude_ft=0.0
        )

        self.runway = Runway(
            airport=self.airport,
            runway_heading_deg=270.0,  # example: RWY 27
            faf_distance_nm=6.0,
        )

        # -----------------------------
        # Aircraft dynamics
        # -----------------------------
        self.turn_delta = np.deg2rad(3.0)
        self.max_turn_rate = np.deg2rad(3.0)

        self.speed_delta = 0.5

        self.initial_speed = 220.0   # knots
        self.min_speed = 160.0
        self.max_speed = 260.0

        # -----------------------------
        # Environment constraints
        # -----------------------------

        self.landing_radius = 2.0   # NM (≈ 2-3 miles)
        self.min_separation = 3.0   # NM
        self.max_distance = 250.0  # discard far-away trajectories

        # -----------------------------
        # Rewards
        # -----------------------------
        self.landing_reward = 100.0
        self.all_landed_bonus = 200.0
        self.collision_penalty = 200.0

        self.instruction_cost = 0.5
        self.silence_bonus = 0.1
        self.turn_rate_penalty = 0.1

        # -----------------------------
        # Spaces
        # -----------------------------
        # Per-plane observation:
        # [dx, dy, speed, heading, landed_flag]
        obs_dim_per_plane = 5
        self.observation_space = spaces.Box(
            low=-np.inf,
            high=np.inf,
            shape=(self.max_planes * obs_dim_per_plane,),
            dtype=np.float32,
        )

        # Continuous action space per plane:
        self.single_plane_action_space = gym.spaces.MultiDiscrete([
            5,  # heading clearance
            3,  # speed clearance
            3,  # altitude clearance
        ])

        self.action_space = spaces.Box(
            low=-1.0,
            high=1.0,
            shape=(self.max_planes, 3),     # three axes of control
            dtype=np.float32
        )

        # -----------------------------
        # State
        # -----------------------------
        self.planes: list[Airplane] = []

    # -----------------------------
    # Curriculum based training
    # -----------------------------
    def set_curriculum_stage(self, stage: int):
        self.curriculum_stage = stage

    def spawn_on_final(self, plane_id, distance_nm, altitude_ft, intercept_deg=0.0):
        """
        Spawn aircraft on localizer or with intercept angle.
        """
        loc_dir = self.runway.localizer_dir
        outbound = self.runway.outbound_dir

        # Base position on final
        pos_nm = self.airport.position_nm + outbound * distance_nm

        # Intercept heading offset
        heading = np.arctan2(loc_dir[1], loc_dir[0])
        heading += np.deg2rad(intercept_deg)

        plane = Airplane(
            plane_id=plane_id,
            position_nm=pos_nm,
            destination_nm=self.airport,
            heading_rads=heading,
            speed_kts=self.initial_speed,
            min_speed_kts=self.min_speed,
            max_speed_kts=self.max_speed,
            max_turn_rate_rads=self.max_turn_rate,
            init_altitude_ft=altitude_ft,
            is_arrival=True,
        )

        # Bias targets for realism
        plane.target_heading = np.arctan2(loc_dir[1], loc_dir[0])
        plane.target_altitude = 0.0  # For arrivals, descend to airport altitude
        plane.target_speed = plane.speed

        return plane

    # -----------------------------
    # Reset
    # -----------------------------
    def reset(self, *, seed=None, options=None):
        super().reset(seed=seed)

        self.sim_time = 0.0
        self.step_count = 0
        self.planes = []

        num_planes = self.max_planes
    
        for i in range(num_planes):

            if self.curriculum_stage == 0:
                # -------------------------
                # Stage 0: 10 NM final, 2000 ft, on localizer
                # -------------------------
                plane = self.spawn_on_final(
                    plane_id=i,
                    distance_nm=6.0 + (INITIAL_SPACING_NM * i),
                    altitude_ft=2000.0 + (1000.0 * i),
                    intercept_deg=0.0
                )

            elif self.curriculum_stage == 1:
                # -------------------------
                # Stage 1: 6-18 NM, 5000 ft, 20 deg intercept
                # -------------------------
                good_airplane_location = False
                while not good_airplane_location:
                    d = self.np_random.uniform(6.0, 18.0)
                    intercept = self.np_random.choice([-20.0, 20.0])
                    alt = d * 400
                    plane = self.spawn_on_final(
                        plane_id=i,
                        distance_nm=d,
                        altitude_ft=alt,
                        intercept_deg=intercept
                    )                    
                    good_airplane_location = not self.position_violates_separation(plane)

            elif self.curriculum_stage >= 2:
                # -------------------------
                # Stage 2+: Randomized terminal area
                # -------------------------
                angle = self.np_random.uniform(0, 2 * np.pi)
                radius = self.np_random.uniform(15.0, 30.0)
                alt = self.np_random.uniform(4.0, 12.0) * 1000.0

                pos = self.airport.position_nm + np.array(
                    [np.cos(angle), np.sin(angle)], dtype=np.float32
                ) * radius

                heading = angle + np.pi  # rough inbound

                plane = Airplane(
                    plane_id=i,
                    position_nm=pos,
                    destination_nm=self.airport,
                    heading_rads=heading,
                    speed_kts=self.initial_speed,
                    min_speed_kts=self.min_speed,
                    max_speed_kts=self.max_speed,
                    max_turn_rate_rads=self.max_turn_rate,
                    init_altitude_ft=alt,
                    is_arrival=True,
                )

            plane.target_altitude = 0.0
            plane.prev_dist_nm = np.linalg.norm(
                plane.position_nm - self.airport.position_nm
            )

            self.planes.append(plane)

        return self._get_obs(), {}
    
    def position_violates_separation(self, new_plane) -> bool:
        for plane in self.planes:
            if self.separation_violated(plane, new_plane):
                return True
        return False

    def separation_violated(self, p1, p2) -> bool:
        if abs(p1.altitude - p2.altitude) >= 2000.0:
            return False
        d = np.linalg.norm(p1.position_nm - p2.position_nm)
        return d < self.min_separation

    # -----------------------------
    # Step
    # -----------------------------
    def step(self, actions):
        self.step_count += 1
        self.sim_time += self.dt
        reward = 0.0
        terminated = False
        truncated = False

        instruction_count = 0

        # -----------------------------
        # Apply actions
        # -----------------------------
        for i, plane in enumerate(self.planes):
            if plane.landed:
                reward += +1000.0
                continue

            turn_norm, accel_norm, vs_norm = actions[i]
            if self.sim_time - plane.last_clearance_time < CLEARANCE_INTERVAL_S:
                # ignore non-maintain actions
                turn_norm = 0.0
                accel_norm = 0.0
                vs_norm = 0.0
            else:
                plane.last_clearance_time = self.sim_time

            turn_rate = turn_norm * self.max_turn_rate
            accel = accel_norm * MAX_ACCEL
            vert_speed = vs_norm * MAX_VERT_SPEED

            instruction_count += plane.apply_atc_clearance(
                turn_rate_cmd=turn_rate,
                accel_cmd=accel,
                vert_speed_cmd=vert_speed,
                dt=self.dt
            )

        # -----------------------------
        # Physics update
        # -----------------------------
        for plane in self.planes:
            plane.step(self.dt)
            # Pilot-local shaping
            reward += plane.compute_pilot_reward(self.curriculum_stage)

        # -----------------------------
        # Landing checks
        # -----------------------------
        landed_this_step = 0
        for plane in self.planes:
            if plane.check_landing(self.airport.position_nm, self.landing_radius):
                reward += self.landing_reward
                landed_this_step += 1

        # -----------------------------
        # Separation penalty
        # -----------------------------
        for i in range(len(self.planes)):
            for j in range(i + 1, len(self.planes)):
                p1, p2 = self.planes[i], self.planes[j]
                if p1.landed or p2.landed:
                    continue

                if self.separation_violated(p1, p2):
                    reward -= self.collision_penalty
                    terminated = True

        # -----------------------------
        # Silence / instruction shaping
        # -----------------------------
        active_planes = sum(not p.landed for p in self.planes)

        reward -= instruction_count * self.instruction_cost
        reward += (active_planes - instruction_count) * self.silence_bonus

        # -----------------------------
        # Distance discard
        # -----------------------------
        for plane in self.planes:
            if plane.landed:
                continue
            if np.linalg.norm(plane.position_nm - self.airport.position_nm) > self.max_distance:
                terminated = True
                reward -= 50.0

        # -----------------------------
        # Termination
        # -----------------------------
        if all(p.landed for p in self.planes):
            reward += self.all_landed_bonus
            terminated = True

        if self.step_count >= self.max_episode_steps:
            truncated = True
            
        if self.sim_time >= MAX_SIM_SECONDS:
            truncated = True


        return self._get_obs(), reward, terminated, truncated, {}

    def is_done(self):
        if self.curriculum_stage < 5:
            # Early stages: never require full landing
            return False

        return self.check_landing_conditions() or self.crashed

    # -----------------------------
    # Observation builder
    # -----------------------------
    def _get_obs(self):
        obs = []

        for i in range(self.max_planes):
            if i < len(self.planes):
                p = self.planes[i]
                if p.landed:
                    obs.extend([0, 0, 0, 0, 1])
                else:
                    dx, dy = p.position_nm - self.airport.position_nm
                    obs.extend([dx, dy, p.speed, p.heading, 0])
            else:
                obs.extend([0, 0, 0, 0, 1])

        return np.array(obs, dtype=np.float32)
