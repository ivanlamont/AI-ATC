import numpy as np
import matplotlib.pyplot as plt
from matplotlib import cm
from matplotlib.animation import FuncAnimation, FFMpegWriter

from stable_baselines3 import PPO

from ai_atc_env import AIATCEnv
from airplane import MIN_ALTITUDE, MAX_ALTITUDE
from constants import (
    MAX_PLANE_COUNT,
    MAX_TURN_RATE,
    MAX_ACCEL,
    MAX_VERT_SPEED,
    MODEL_DIR,
    MODEL_OUTPUT,
    OUTPUT_VIDEO,
)
import warnings

warnings.filterwarnings(
    "ignore",
    message=".*Protobuf gencode version.*"
)

MAX_STEPS = 1500
FPS = 30

from matplotlib.animation import FFMpegWriter

def create_visualization(
    model_path=MODEL_OUTPUT,
    output_path=OUTPUT_VIDEO,
    max_steps=600,
    fps=10,
):
    env = AIATCEnv(max_planes=MAX_PLANE_COUNT, render_mode=None)
    model = PPO.load(model_path)

    obs, _ = env.reset()

    fig, ax = plt.subplots(figsize=(8, 8))
    ax.set_title("AI ATC - Episode Playback")

    airport = env.airport.position_nm
    ax.plot(airport[0], airport[1], "ks", markersize=10, label="Airport")

    plane_dots = []
    text_boxes = []

    for i in range(len(env.planes)):
        dot, = ax.plot([], [], "o", label=f"Plane {i}")
        plane_dots.append(dot)
        text_boxes.append(ax.text(0, 0, "", fontsize=9))

    # Find the highest absolute position to set dynamic limits
    max_pos = 0
    for plane in env.planes:
        x, y = plane.position_nm
        max_pos = max(max_pos, abs(x), abs(y))

    ax.set_xlim(-max_pos - 10, max_pos + 10)
    ax.set_ylim(-max_pos - 10, max_pos + 10)
    # ax.relim()
    # ax.autoscale_view()

    ax.set_aspect("equal")
    ax.legend()

    # Add colorbar for altitude
    sm = cm.ScalarMappable(cmap='viridis', norm=plt.Normalize(vmin=MIN_ALTITUDE, vmax=MAX_ALTITUDE))
    sm.set_array([])
    cbar = plt.colorbar(sm, ax=ax)
    cbar.set_label('Altitude (ft)')

    writer = FFMpegWriter(fps=fps)

    with writer.saving(fig, output_path, dpi=150):
        for step in range(max_steps):
            action, _ = model.predict(obs, deterministic=True)
            obs, reward, terminated, truncated, _ = env.step(action)

            for i, plane in enumerate(env.planes):
                if plane.landed:
                    continue

                x, y = plane.position_nm
                alt_norm = (plane.altitude - MIN_ALTITUDE) / (MAX_ALTITUDE - MIN_ALTITUDE)
                color = cm.viridis(alt_norm)
                plane_dots[i].set_data([x], [y])
                plane_dots[i].set_color(color)

                text_boxes[i].set_position((x + 2, y + 2))
                text_boxes[i].set_text(
                    f"P{i} Alt:{plane.altitude:.0f} "
                    f"reward:{reward:.0f}"
                    f"Tgt:{plane.target_altitude:.0f}"
                )

            ax.relim()
            ax.autoscale_view()

            writer.grab_frame()

            if terminated or truncated:
                print(f"Episode finished at step {step}")
                break

    print(f"Saved video to {output_path}")


# -----------------------------
# Script entry point
# -----------------------------
if __name__ == "__main__":
    create_visualization()
