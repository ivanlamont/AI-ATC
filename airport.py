import numpy as np

class Airport:
    def __init__(self, position_nm: np.ndarray, altitude_ft: float = 0.0):
        self.position_nm = position_nm.astype(np.float32)
        self.altitude_ft = altitude_ft