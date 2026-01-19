import numpy as np
import gymnasium as gym
from gymnasium import spaces

class SimpleATCEnv(gym.Env):
    """
    Single airplane, single airport, 2D only.
    Actions: 0=turn left, 1=turn right, 2=speed up, 3=slow down
    State: [x, y, vx, vy]
    """
    metadata = {"render_modes": ["human"]}

    def __init__(self, max_episode_steps=100):
        super().__init__()

        self.action_space = spaces.Discrete(4)
        self.observation_space = spaces.Box(
            low=-np.inf, high=np.inf, shape=(4,), dtype=np.float32
        )

        self.dt = 1.0
        self.max_speed = 20.0
        self.min_speed = 1.0
        self.turn_angle = np.pi / 18
        self.target = np.array([0.0, 0.0], dtype=np.float32)

        self.max_episode_steps = max_episode_steps
        self.step_count = 0

    def reset(self, seed=None, options=None):
        super().reset(seed=seed)

        self.pos = np.array([100.0, 100.0], dtype=np.float32)
        self.vel = np.array([-1.0, -1.0], dtype=np.float32)
        self.step_count = 0
        self.last_distance = np.linalg.norm(self.pos - self.target)

        return self._get_state(), {}

    def _get_state(self):
        return np.concatenate([self.pos, self.vel])

    def step(self, action):
        self.step_count += 1

        theta = np.arctan2(self.vel[1], self.vel[0])
        speed = np.linalg.norm(self.vel)

        if action == 0:
            theta += self.turn_angle
        elif action == 1:
            theta -= self.turn_angle
        elif action == 2:
            speed = min(speed * 1.1, self.max_speed)
        elif action == 3:
            speed = max(speed * 0.9, self.min_speed)

        self.vel = speed * np.array([np.cos(theta), np.sin(theta)])
        self.pos += self.vel * self.dt

        distance = np.linalg.norm(self.pos - self.target)

        reward = (self.last_distance - distance) * 10.0
        reward -= 0.1
        self.last_distance = distance

        terminated = False
        truncated = False

        if distance < 1.0:
            reward += 100.0
            terminated = True
        elif self.step_count >= self.max_episode_steps:
            truncated = True
        elif distance > 150.0:
            truncated = True

        return self._get_state(), reward, terminated, truncated, {}

    def render(self):
        print(f"Step {self.step_count} | Pos={self.pos} Vel={self.vel}")
