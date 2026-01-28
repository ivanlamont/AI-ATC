
# -----------------------------
# Constants
# -----------------------------

import numpy as np

LOG_DIR = "tensorboard"
MODEL_DIR = "models"

# NO_OP = 0
# TURN_LEFT = 1
# TURN_RIGHT = 2
# SPEED_UP = 3
# SLOW_DOWN = 4

# ACTION_NAMES = {
#     NO_OP: "NO_OP",
#     TURN_LEFT: "TURN_LEFT",
#     TURN_RIGHT: "TURN_RIGHT",
#     SPEED_UP: "SPEED_UP",
#     SLOW_DOWN: "SLOW_DOWN",
# }

#0 = hard left, 1 = left, 2 = maintain, 3 = right, 4 = hard right
HEADING_DELTAS_DEG = {
    0: -20,
    1: -10,
    2:   0,
    3: +10,
    4: +20,
}

#0 = slow,  1 = maintain, 2 = fast
SPEED_DELTAS_KTS = {
    0: -10,
    1:   0,
    2: +10,
}

#0 = descend, 1 = maintain, 2 = climb
VERT_SPEED_DELTAS_FPM = {
    0: -1000,
    1:     0,
    2: +1000,
}

CLEARANCE_INTERVAL_S = 15.0

ALTITUDE_STEP_FT = 1000.0

MAX_ACCEL = 5.0  # knots/sec

MAX_PLANE_COUNT = 3
MAX_TURN_RATE = np.deg2rad(3.0)   # 3 deg/sec
MAX_ACCEL = 5.0                  # knots/sec
MAX_VERT_SPEED = 2500.0          # ft/min
MAX_ALTITUDE_CHANGE_PER_STEP = 2500.0  # ft per step

INITIAL_SPACING_NM = 4.0

MAX_SIM_SECONDS = 3600.0  # 1 hour

OUTPUT_VIDEO = "visualizations/ai_atc_demo.mp4"
MODEL_DIR = "models"
MODEL_OUTPUT = f"{MODEL_DIR}/ai_atc_ppo"
