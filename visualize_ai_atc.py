import numpy as np
import matplotlib.pyplot as plt
from matplotlib.animation import FuncAnimation
from stable_baselines3 import PPO
from ai_atc_env import SimpleATCEnv

# Load trained model
model = PPO.load("ppo_ai_atc_fast")

# Create environment
env = SimpleATCEnv(max_episode_steps=100)

obs, info = env.reset()

positions = []
done = False

while not done:
    # SB3 expects a batch dimension
    action, _ = model.predict(obs[None, :], deterministic=True)

    obs, reward, terminated, truncated, info = env.step(action.item())
    done = terminated or truncated

    positions.append(env.pos.copy())

positions = np.array(positions)

# ---- Visualization ----
fig, ax = plt.subplots(figsize=(6, 6))
ax.set_title("AI-ATC: Aircraft Guidance Simulation")
ax.set_xlabel("X")
ax.set_ylabel("Y")
ax.set_xlim(-10, 110)
ax.set_ylim(-10, 110)
ax.grid(True)

# Airport
airport = np.array([0.0, 0.0])
ax.scatter(*airport, color="red", s=100, label="Airport")

# Aircraft path
path_line, = ax.plot([], [], "b--", alpha=0.6)
plane_dot, = ax.plot([], [], "bo", label="Aircraft")

ax.legend()

def update(frame):
    x = positions[:frame + 1, 0]
    y = positions[:frame + 1, 1]
    path_line.set_data(x, y)
    plane_dot.set_data([x[-1]], [y[-1]])
    return path_line, plane_dot

ani = FuncAnimation(
    fig,
    update,
    frames=len(positions),
    interval=100,
    blit=True
)

output_file = "ai_atc_demo.mp4"

ani.save(
    output_file,
    writer="ffmpeg",
    fps=10
)

print(f"Saved animation to {output_file}")