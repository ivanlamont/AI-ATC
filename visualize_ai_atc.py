import numpy as np
import matplotlib.pyplot as plt
from matplotlib.animation import FuncAnimation, FFMpegWriter

from stable_baselines3 import PPO

from ai_atc_env import AIATCEnv, ACTION_NAMES, NO_OP


# -----------------------------
# Config
# -----------------------------
MODEL_PATH = "models/ai_atc_ppo"
MAX_STEPS = 1500
OUTPUT_VIDEO = "ai_atc_demo.mp4"
FPS = 30

AIRPORT_COLOR = "red"
PLANE_COLOR = "blue"
LANDED_COLOR = "green"


# -----------------------------
# Load environment & model
# -----------------------------
env = AIATCEnv(max_planes=3)
model = PPO.load(MODEL_PATH, device="cpu")  # CPU is fine for inference

obs, _ = env.reset()


# -----------------------------
# Matplotlib setup
# -----------------------------
fig, ax = plt.subplots(figsize=(7, 7))
ax.set_aspect("equal", adjustable="box")
ax.set_title("AI-ATC — Trained Policy Visualization")

airport_dot, = ax.plot(
    env.airport[0],
    env.airport[1],
    marker="X",
    markersize=12,
    color=AIRPORT_COLOR,
    label="Airport",
)

plane_dots = []
heading_lines = []

for _ in range(env.max_planes):
    dot, = ax.plot([], [], "o", color=PLANE_COLOR)
    line, = ax.plot([], [], "-", linewidth=1)
    plane_dots.append(dot)
    heading_lines.append(line)

ax.legend(loc="upper right")


# -----------------------------
# Storage for trajectory scaling
# -----------------------------
all_positions = []


# -----------------------------
# Animation update
# -----------------------------
def update(frame_idx):
    global obs

    actions, _ = model.predict(obs, deterministic=True)

    # Debug print issued instructions
    for i, action in enumerate(actions):
        if i < len(env.planes):
            plane = env.planes[i]
            if not plane.landed and action != NO_OP:
                print(f"[ATC] Plane {plane.id}: {ACTION_NAMES[action]}")

    obs, reward, terminated, truncated, _ = env.step(actions)

    all_positions.clear()

    for i, plane in enumerate(env.planes):

        dot = plane_dots[i]
        line = heading_lines[i]

        if plane.landed:
            dot.set_data([plane.pos[0]], [plane.pos[1]])
            dot.set_color(LANDED_COLOR)
            line.set_data([], [])
            continue

        x, y = plane.pos
        all_positions.append((x, y))

        dot.set_data([x], [y])
        dot.set_color(PLANE_COLOR)

        # Heading visualization
        heading_len = 6.0
        hx = x + np.cos(plane.heading) * heading_len
        hy = y + np.sin(plane.heading) * heading_len
        line.set_data([x, hx], [y, hy])

    # Auto-scale view
    if all_positions:
        xs, ys = zip(*all_positions)
        pad = 20
        ax.set_xlim(min(xs) - pad, max(xs) + pad)
        ax.set_ylim(min(ys) - pad, max(ys) + pad)

    if terminated or truncated or frame_idx >= MAX_STEPS or all(plane.landed for plane in env.planes):
        anim.event_source.stop()

    return plane_dots + heading_lines


# -----------------------------
# Run animation
# -----------------------------
anim = FuncAnimation(
    fig,
    update,
    frames=MAX_STEPS,
    interval=1000 / FPS,
    blit=False,
)


# -----------------------------
# Save video
# -----------------------------
writer = FFMpegWriter(fps=FPS)
anim.save(OUTPUT_VIDEO, writer=writer)

print(f"Saved visualization to {OUTPUT_VIDEO}")
