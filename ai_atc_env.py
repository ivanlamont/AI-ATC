import numpy as np
import gymnasium as gym
from gymnasium import spaces

from airplane import MAX_ALTITUDE, MIN_ALTITUDE, Airplane

from constants import MAX_ACCEL, MAX_PLANE_COUNT, MAX_TURN_RATE, MAX_VERT_SPEED, MAX_ALTITUDE_CHANGE_PER_STEP

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

        self.render_mode = render_mode
        # -----------------------------
        # Core config
        # -----------------------------
        self.max_planes = max_planes
        self.dt = dt
        self.max_episode_steps = max_episode_steps

        self.airport = (
            airport_pos.astype(np.float32)
            if airport_pos is not None
            else np.array([0.0, 0.0], dtype=np.float32)
        )

        # -----------------------------
        # Aircraft dynamics
        # -----------------------------
        self.turn_delta = np.deg2rad(3.0)
        self.max_turn_rate = np.deg2rad(5.0)

        self.speed_delta = 0.5

        self.initial_speed = 5.0
        self.min_speed = 0.8 * self.initial_speed
        self.max_speed = 1.2 * self.initial_speed

        # -----------------------------
        # Environment constraints
        # -----------------------------
        self.landing_radius = 5.0
        self.min_separation = 8.0
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
        self.single_plane_action_space = spaces.Box(
            low=np.array([-1.0, -1.0, -1.0], dtype=np.float32),
            high=np.array([1.0, 1.0, 1.0], dtype=np.float32),
            dtype=np.float32
        )

        self.action_space = spaces.Box(
            low=-1.0,
            high=1.0,
            shape=(self.max_planes, MAX_PLANE_COUNT),
            dtype=np.float32
        )

        # -----------------------------
        # State
        # -----------------------------
        self.planes: list[Airplane] = []
        self.step_count = 0

    # -----------------------------
    # Reset
    # -----------------------------
    def reset(self, *, seed=None, options=None):
        super().reset(seed=seed)

        self.step_count = 0
        self.planes = []

        num_planes = self.max_planes
    
        for i in range(num_planes):
            angle = self.np_random.uniform(0, 2 * np.pi)
            radius = self.np_random.uniform(60.0, 120.0)
            alt = self.np_random.uniform(12000.0, 24000.0)

            pos = self.airport + np.array(
                [np.cos(angle), np.sin(angle)], dtype=np.float32
            ) * radius

            heading = angle + np.pi  # roughly toward airport

            plane = Airplane(
                plane_id=i,
                position=pos,
                heading=heading,
                speed=self.initial_speed,
                min_speed=self.min_speed,
                max_speed=self.max_speed,
                max_turn_rate=self.max_turn_rate,
                init_altitude=alt,
            )

            self.planes.append(plane)

        return self._get_obs(), {}

    # -----------------------------
    # Step
    # -----------------------------
    def step(self, actions):
        self.step_count += 1
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

            heading_cmd_norm, speed_cmd_norm, altitude_cmd_norm = actions[i]

            instruction_count += plane.set_targets(heading_cmd_norm, speed_cmd_norm, altitude_cmd_norm)

            # desired_turn_rate = heading_cmd_norm * MAX_TURN_RATE
            # desired_accel = speed_cmd_norm * MAX_ACCEL
            # target_alt_delta = altitude_cmd_norm * MAX_ALTITUDE_CHANGE_PER_STEP
            # plane.target_altitude += target_alt_delta
            # plane.target_altitude = np.clip(
            #     plane.target_altitude,
            #     MIN_ALTITUDE,
            #     MAX_ALTITUDE
            # )

            # plane.apply_control(
            #     desired_turn_rate,
            #     desired_accel,
            #     dt=self.dt
            # )
            # plane.pilot_heading_control(self.dt)
            # plane.pilot_altitude_control(self.dt)

        # -----------------------------
        # Physics update
        # -----------------------------
        for plane in self.planes:
            plane.step(self.dt)
            reward -= self.turn_rate_penalty * abs(plane.current_turn_rate)
            reward -= 0.1* abs((plane.altitude - plane.target_altitude) / 1000.0)

        # -----------------------------
        # Landing checks
        # -----------------------------
        landed_this_step = 0
        for plane in self.planes:
            if plane.check_landing(self.airport, self.landing_radius):
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

                d = np.linalg.norm(p1.pos - p2.pos)
                if d < self.min_separation:
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
            if np.linalg.norm(plane.pos - self.airport) > self.max_distance:
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

        return self._get_obs(), reward, terminated, truncated, {}

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
                    dx, dy = p.pos - self.airport
                    obs.extend([dx, dy, p.speed, p.heading, 0])
            else:
                obs.extend([0, 0, 0, 0, 1])

        return np.array(obs, dtype=np.float32)
