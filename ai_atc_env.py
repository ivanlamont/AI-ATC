import numpy as np
import gymnasium as gym
from gymnasium import spaces

from airplane import Airplane


# -----------------------------
# Constants
# -----------------------------
NO_OP = 0
TURN_LEFT = 1
TURN_RIGHT = 2
SPEED_UP = 3
SLOW_DOWN = 4

ACTION_NAMES = {
    NO_OP: "NO_OP",
    TURN_LEFT: "TURN_LEFT",
    TURN_RIGHT: "TURN_RIGHT",
    SPEED_UP: "SPEED_UP",
    SLOW_DOWN: "SLOW_DOWN",
}


class AIATCEnv(gym.Env):
    """
    AI-ATC Environment with N airplanes (fixed observation/action size).
    """

    metadata = {"render_modes": []}

    def __init__(
        self,
        max_planes: int = 3,
        airport_pos: np.ndarray | None = None,
        dt: float = 1.0,
        max_episode_steps: int = 2000,
    ):
        super().__init__()

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

        # One discrete action per plane
        self.action_space = spaces.MultiDiscrete([5] * self.max_planes)

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
        for i, action in enumerate(actions):
            if i >= len(self.planes):
                continue

            plane = self.planes[i]
            issued = plane.apply_action(
                action=action,
                turn_delta=self.turn_delta,
                speed_delta=self.speed_delta,
            )

            if issued:
                instruction_count += 1

        # -----------------------------
        # Physics update
        # -----------------------------
        for plane in self.planes:
            plane.step(self.dt)
            reward -= self.turn_rate_penalty * abs(plane.current_turn_rate)

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
