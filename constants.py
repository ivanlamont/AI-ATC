
# -----------------------------
# Constants
# -----------------------------

import numpy as np

LOG_DIR = "tensorboard"
MODEL_DIR = "models"

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

MAX_PLANE_COUNT = 3
MAX_TURN_RATE = np.deg2rad(3.0)   # 3 deg/sec
MAX_ACCEL = 5.0                  # knots/sec
MAX_VERT_SPEED = 2500.0          # ft/min
MAX_ALTITUDE_CHANGE_PER_STEP = 2500.0  # ft per step
