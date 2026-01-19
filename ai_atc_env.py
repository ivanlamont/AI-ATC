import numpy as np
import gymnasium as gym
from gymnasium import spaces
import gymnasium as gym
from gymnasium import spaces
import numpy as np
import math

class AIATCEnv(gym.Env):
    metadata = {"render_modes": []}

    def __init__(self):
        super().__init__()

        self.max_turn_rate = math.radians(15)      # deg/sec
        self.max_turn_accel = math.radians(30)     # deg/sec²

        # --- Simulation parameters ---
        self.dt = 1.0
        self.turn_rate = math.radians(5)
        self.speed_delta = 0.05

        self.airport = np.array([0.0, 0.0])
        self.success_radius = 0.5
        self.max_distance = 20.0
        self.min_separation = 1.0

        # --- Action space ---
        self.action_space = spaces.Discrete(25)  # 5 actions per plane

        # --- Observation space ---
        high = np.array([np.inf] * 11, dtype=np.float32)
        self.observation_space = spaces.Box(-high, high, dtype=np.float32)

        self.reset()

    def reset(self, seed=None, options=None):
        super().reset(seed=seed)

        self.planes = []
        for _ in range(2):
            pos = np.random.uniform(-10, 10, size=2)
            heading = np.random.uniform(0, 2 * math.pi)
            speed = np.random.uniform(0.8, 1.2)

            plane = {
                "pos": pos,
                "heading": heading,
                "speed": speed,
                "min_speed": 0.8 * speed,
                "max_speed": 1.2 * speed,
                "current_turn_rate": 0.0,
                "desired_turn_rate": 0.0,
            }

            self.planes.append(plane)

        return self._get_obs(), {}

    def step(self, action):
        a1 = action // 5
        a2 = action % 5
        actions = [a1, a2]
        TURN_COMMAND = {
            0: 0.0,                          # no-op
            1: +self.max_turn_rate,          # turn left
            2: -self.max_turn_rate,          # turn right
        }

        for plane, act in zip(self.planes, actions):
            if act in (1, 2):
                plane["desired_turn_rate"] = TURN_COMMAND[act]
            else:
                plane["desired_turn_rate"] = 0.0


            if act == 3:
                plane["speed"] = min(
                    plane["speed"] + self.speed_delta,
                    plane["max_speed"]
                )
            elif act == 4:
                plane["speed"] = max(
                    plane["speed"] - self.speed_delta,
                    plane["min_speed"]
                )

            # --- Turn dynamics ---
            delta = plane["desired_turn_rate"] - plane["current_turn_rate"]
            delta = np.clip(
                delta,
                -self.max_turn_accel * self.dt,
                self.max_turn_accel * self.dt
            )
            plane["current_turn_rate"] += delta

            plane["current_turn_rate"] = np.clip(
                plane["current_turn_rate"],
                -self.max_turn_rate,
                self.max_turn_rate
            )

            plane["heading"] += plane["current_turn_rate"] * self.dt

            dx = math.cos(plane["heading"]) * plane["speed"] * self.dt
            dy = math.sin(plane["heading"]) * plane["speed"] * self.dt
            plane["pos"] += np.array([dx, dy])

        reward = -1.0
        
        terminated = False

        d1 = np.linalg.norm(self.planes[0]["pos"] - self.airport)
        d2 = np.linalg.norm(self.planes[1]["pos"] - self.airport)
        separation = np.linalg.norm(
            self.planes[0]["pos"] - self.planes[1]["pos"]
        )

        # --- Collision / unsafe proximity ---
        if separation < self.min_separation:
            reward -= 100.0
            terminated = True

        # --- Success ---
        if d1 < self.success_radius and d2 < self.success_radius:
            reward += 200.0
            terminated = True

        # --- Early discard optimization ---
        if d1 > self.max_distance and d2 > self.max_distance:
            reward -= 50.0
            terminated = True

        TURN_PENALTY_WEIGHT = 0.1

        total_turn_penalty = sum(
            abs(plane["current_turn_rate"]) for plane in self.planes
        )

        reward -= TURN_PENALTY_WEIGHT * total_turn_penalty

        return self._get_obs(), reward, terminated, False, {}

    def _get_obs(self):
        p1, p2 = self.planes

        d1 = np.linalg.norm(p1["pos"] - self.airport)
        d2 = np.linalg.norm(p2["pos"] - self.airport)
        sep = np.linalg.norm(p1["pos"] - p2["pos"])

        return np.array([
            p1["pos"][0], p1["pos"][1], p1["heading"], p1["speed"],
            p2["pos"][0], p2["pos"][1], p2["heading"], p2["speed"],
            d1, d2, sep
        ], dtype=np.float32)
