import numpy as np

import constants

MAX_VERT_SPEED = 3000.0     # ft/min
MAX_VERT_ACCEL = 1000.0    # ft/min^2 (rate of change of VS)
MIN_ALTITUDE = 0.0         # ft (ground)
MAX_ALTITUDE = 40000.0    # ft (service ceiling)
ALTITUDE_KP = 0.05         # proportional gain for altitude tracking    

class Airplane:
    def __init__(
        self,
        plane_id: int,
        position: np.ndarray,
        heading: float,
        speed: float,
        min_speed: float,
        max_speed: float,
        max_turn_rate: float,
        init_altitude: float = 1000.0,  # Default initial altitude in feet
    ):
        self.id = plane_id
        self.pos = position.astype(np.float32)
        self.heading = float(heading)
        self.speed = float(speed)
        self.altitude = init_altitude          # ft
        self.target_altitude = init_altitude   # ft
        self.vert_speed = 0.0                  # ft/min

        self.min_speed = min_speed
        self.max_speed = max_speed
        self.max_turn_rate = max_turn_rate

        self.current_turn_rate = 0.0
        self.landed = False

    def apply_vertical_control(self, dt):
        """
        Pilot logic: track target_altitude using bounded VS + accel
        """

        alt_error = self.target_altitude - self.altitude

        # Simple proportional controller to desired VS
        desired_vs = np.clip(
            ALTITUDE_KP * alt_error,   # ft/min
            -MAX_VERT_SPEED,
            MAX_VERT_SPEED
        )

        # Vertical acceleration limiting
        max_vs_delta = MAX_VERT_ACCEL * (dt / 60.0)

        vs_error = desired_vs - self.vert_speed
        vs_change = np.clip(vs_error, -max_vs_delta, max_vs_delta)

        self.vert_speed += vs_change

        # Integrate altitude
        self.altitude += (self.vert_speed / 60.0) * dt

        # Ground protection
        if self.altitude < MIN_ALTITUDE:
            self.altitude = MIN_ALTITUDE
            self.vert_speed = 0.0

    def apply_lateral_control(self, desired_turn_rate, desired_accel):
        self.commanded_turn_rate = np.clip(
            desired_turn_rate,
            -self.max_turn_rate,
            self.max_turn_rate
        )

        accel = np.clip(desired_accel, -constants.MAX_ACCEL, constants.MAX_ACCEL)

        self.speed += accel
        self.speed = np.clip(self.speed, self.min_speed, self.max_speed)


    def apply_control(self, desired_turn_rate, desired_accel,  dt):
        self.apply_vertical_control(dt)
        self.apply_lateral_control(desired_turn_rate, desired_accel)


    # -----------------------------
    # Physics update
    # -----------------------------
    def step(self, dt: float):
        if self.landed:
            return

        # Integrate heading
        self.heading += self.commanded_turn_rate * dt

        # Integrate lateral position
        direction = np.array([
            np.cos(self.heading),
            np.sin(self.heading)
        ], dtype=np.float32)

        self.pos += direction * self.speed * dt

        # Vertical pilot
        self.apply_vertical_control(dt)


    # -----------------------------
    # Landing check
    # -----------------------------
    def check_landing(self, airport_pos: np.ndarray, landing_radius: float) -> bool:
        if self.landed:
            return False

        dist = np.linalg.norm(self.pos - airport_pos)
        if dist <= landing_radius:
            self.landed = True
            return True

        return False
