import numpy as np
import matplotlib.pyplot as plt
from matplotlib.animation import FuncAnimation, FFMpegWriter

from stable_baselines3 import PPO

from ai_atc_env import AIATCEnv
from constants import (
    MAX_PLANE_COUNT,
    MAX_TURN_RATE,
    MAX_ACCEL,
    MAX_VERT_SPEED,
    MODEL_DIR,
)

MAX_STEPS = 1500
OUTPUT_VIDEO = "visualizations/ai_atc_demo.mp4"
FPS = 30

def create_visualization(
    model_path=f"{MODEL_DIR}/ai_atc_ppo",
    interval_ms=200,
    max_trail=100,
):
    """
    Launch interactive visualization of trained ATC model.
    """

    # -----------------------------
    # Load env + model
    # -----------------------------
    env = AIATCEnv(max_planes=MAX_PLANE_COUNT)
    model = PPO.load(model_path)

    obs, _ = env.reset()

    # -----------------------------
    # Matplotlib setup
    # -----------------------------
    fig, ax = plt.subplots(figsize=(8, 8))
    ax.set_title("AI ATC - Continuous Control")

    airport = env.airport
    ax.plot(airport[0], airport[1], "ks", markersize=10, label="Airport")

    plane_dots = []
    text_boxes = []

    for i in range(len(env.planes)):
        dot, = ax.plot([], [], "o", label=f"Plane {i}")
        plane_dots.append(dot)

        txt = ax.text(0, 0, "", fontsize=9)
        text_boxes.append(txt)

    ax.set_aspect("equal")
    ax.legend()

    # -----------------------------
    # History for trails
    # -----------------------------
    pos_hist = [[] for _ in env.planes]

    # -----------------------------
    # Update loop
    # -----------------------------
    def update(frame):
        nonlocal obs

        action, _ = model.predict(obs, deterministic=True)
        obs, reward, terminated, truncated, info = env.step(action)

        for i, plane in enumerate(env.planes):
            if plane.landed:
                continue

            x, y = plane.pos
            pos_hist[i].append((x, y))

            # Trail
            trail = pos_hist[i][-max_trail:]
            xs = [p[0] for p in trail]
            ys = [p[1] for p in trail]

            plane_dots[i].set_data(xs[-1:], ys[-1:])

            # -----------------------------
            # Decode continuous actions
            # -----------------------------
            turn_norm, accel_norm, alt_norm = action[i]

            turn_rate = turn_norm * MAX_TURN_RATE
            accel = accel_norm * MAX_ACCEL
            desired_vs = alt_norm * MAX_VERT_SPEED

            alt_err = plane.target_altitude - plane.altitude

            # -----------------------------
            # Human-readable ATC overlay
            # -----------------------------
            text = (
                f"Plane {i}\n"
                f"Hdg: {np.degrees(plane.heading)%360:6.1f}°\n"
                f"Spd: {plane.speed:6.1f}\n"
                f"Alt: {plane.altitude:6.0f} ft\n"
                f"Tgt: {plane.target_altitude:6.0f} ft\n"
                f"\nCmd:\n"
                f"Turn: {np.degrees(turn_rate):+5.2f}°/s\n"
                f"Accel: {accel:+5.2f}\n"
                f"VS cmd: {desired_vs:+6.0f} fpm\n"
                f"Alt err: {alt_err:+6.0f} ft\n"
            )

            text_boxes[i].set_position((x + 2, y + 2))
            text_boxes[i].set_text(text)

        ax.relim()
        ax.autoscale_view()

        # -----------------------------
        # Episode reset handling
        # -----------------------------
        if terminated or truncated:
            print("Episode finished, resetting visualization.")
            obs, _ = env.reset()
            for hist in pos_hist:
                hist.clear()

        return plane_dots + text_boxes

    # -----------------------------
    # Run animation
    # -----------------------------
    ani = FuncAnimation(
        fig,
        update,
        interval=interval_ms,
        cache_frame_data=False,
    )
    plt.show()

    # -----------------------------
    # Save video
    # -----------------------------
    writer = FFMpegWriter(fps=FPS)
    ani.save(OUTPUT_VIDEO, writer=writer)

    print(f"Saved visualization to {OUTPUT_VIDEO}")

# -----------------------------
# Script entry point
# -----------------------------
if __name__ == "__main__":
    create_visualization()
