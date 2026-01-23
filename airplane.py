import numpy as np

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
    ):
        self.id = plane_id
        self.pos = position.astype(np.float32)
        self.heading = float(heading)
        self.speed = float(speed)

        self.min_speed = min_speed
        self.max_speed = max_speed
        self.max_turn_rate = max_turn_rate

        self.current_turn_rate = 0.0
        self.landed = False

    # -----------------------------
    # Action interface
    # -----------------------------
    def apply_action(self, action: int, turn_delta: float, speed_delta: float):
        if self.landed:
            return False  # no instruction issued

        instruction_issued = False

        if action == 1:  # TURN_LEFT
            self.current_turn_rate = -turn_delta
            instruction_issued = True
        elif action == 2:  # TURN_RIGHT
            self.current_turn_rate = turn_delta
            instruction_issued = True
        elif action == 3:  # SPEED_UP
            self.speed = min(self.speed + speed_delta, self.max_speed)
            instruction_issued = True
        elif action == 4:  # SLOW_DOWN
            self.speed = max(self.speed - speed_delta, self.min_speed)
            instruction_issued = True
        else:  # NO_OP
            self.current_turn_rate = 0.0

        return instruction_issued

    # -----------------------------
    # Physics update
    # -----------------------------
    def step(self, dt: float):
        if self.landed:
            return

        # Apply bounded turn rate
        self.current_turn_rate = np.clip(
            self.current_turn_rate,
            -self.max_turn_rate,
            self.max_turn_rate,
        )

        self.heading += self.current_turn_rate

        direction = np.array([
            np.cos(self.heading),
            np.sin(self.heading)
        ], dtype=np.float32)

        self.pos += direction * self.speed * dt

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
