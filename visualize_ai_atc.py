import numpy as np
import matplotlib
matplotlib.use("Agg")  # headless-safe
import matplotlib.pyplot as plt
from matplotlib.animation import FuncAnimation, FFMpegWriter

from stable_baselines3 import PPO
from stable_baselines3.common.vec_env import DummyVecEnv, VecNormalize

from ai_atc_env import AIATCEnv


MODEL_PATH = "models/ai_atc_ppo"
VECNORM_PATH = "models/vecnormalize.pkl"
OUTPUT_VIDEO = "ai_atc_two_aircraft.mp4"

MAX_STEPS = 500


def make_env():
    return AIATCEnv()


# --- Load env + normalization ---
env = DummyVecEnv([make_env])
env = VecNormalize.load(VECNORM_PATH, env)
env.training = False
env.norm_reward = False

model = PPO.load(MODEL_PATH)

obs = env.reset()

# --- Storage for trajectories ---
p1_x, p1_y = [], []
p2_x, p2_y = [], []

airport_x, airport_y = 0.0, 0.0

done = False
step = 0

while not done and step < MAX_STEPS:
    action, _ = model.predict(obs, deterministic=True)
    obs, reward, done, info = env.step(action)

    # unwrap env
    base_env = env.envs[0]

    p1 = base_env.planes[0]["pos"]
    p2 = base_env.planes[1]["pos"]

    p1_x.append(p1[0])
    p1_y.append(p1[1])
    p2_x.append(p2[0])
    p2_y.append(p2[1])

    step += 1

env.close()

# --- Plot setup ---
fig, ax = plt.subplots(figsize=(6, 6))

ax.set_title("AI-ATC: Two-Aircraft Approach")
ax.set_xlabel("X position")
ax.set_ylabel("Y position")

ax.scatter(airport_x, airport_y, c="red", marker="X", s=100, label="Airport")

p1_line, = ax.plot([], [], "b-", label="Aircraft 1 Path")
p2_line, = ax.plot([], [], "g-", label="Aircraft 2 Path")

p1_dot, = ax.plot([], [], "bo")
p2_dot, = ax.plot([], [], "go")

ax.legend()
ax.set_aspect("equal")

# Auto-scale
all_x = p1_x + p2_x
all_y = p1_y + p2_y
margin = 1.0

ax.set_xlim(min(all_x) - margin, max(all_x) + margin)
ax.set_ylim(min(all_y) - margin, max(all_y) + margin)


def update(frame):
    p1_line.set_data(p1_x[:frame], p1_y[:frame])
    p2_line.set_data(p2_x[:frame], p2_y[:frame])

    p1_dot.set_data([p1_x[frame - 1]], [p1_y[frame - 1]])
    p2_dot.set_data([p2_x[frame - 1]], [p2_y[frame - 1]])

    return p1_line, p2_line, p1_dot, p2_dot


ani = FuncAnimation(
    fig,
    update,
    frames=len(p1_x),
    interval=50,
    blit=True
)

writer = FFMpegWriter(fps=20)
ani.save(OUTPUT_VIDEO, writer=writer)

print(f"Saved visualization to {OUTPUT_VIDEO}")
