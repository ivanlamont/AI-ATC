import numpy as np

import constants

MAX_VERT_SPEED = 3000.0     # ft/min
MAX_VERT_ACCEL = 1000.0    # ft/min^2 (rate of change of VS)
MIN_ALTITUDE = 0.0         # ft (ground)
MAX_ALTITUDE = 40000.0    # ft (service ceiling)
ALTITUDE_KP = 0.05         # proportional gain for altitude tracking    
HEADING_KP = 0.1           # proportional gain for heading tracking    

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
        self.altitude = init_altitude          # ft
        self.vert_speed = 0.0                  # ft/min
        self.current_turn_rate = 0.0
        self.speed = float(speed)

        self.target_heading = self.heading
        self.target_speed = self.speed
        self.target_altitude = init_altitude   # ft

        self.min_speed = min_speed
        self.max_speed = max_speed
        self.max_turn_rate = max_turn_rate

        self.landed = False

    # def apply_vertical_control(self, dt):
    #     """
    #     Pilot logic: track target_altitude using bounded VS + accel
    #     """

    #     alt_error = self.target_altitude - self.altitude

    #     # Simple proportional controller to desired VS
    #     desired_vs = np.clip(
    #         ALTITUDE_KP * alt_error,   # ft/min
    #         -MAX_VERT_SPEED,
    #         MAX_VERT_SPEED
    #     )

    #     # Vertical acceleration limiting
    #     max_vs_delta = MAX_VERT_ACCEL * (dt / 60.0)

    #     vs_error = desired_vs - self.vert_speed
    #     vs_change = np.clip(vs_error, -max_vs_delta, max_vs_delta)

    #     self.vert_speed += vs_change

    #     # Integrate altitude
    #     self.altitude += (self.vert_speed / 60.0) * dt

    #     # Ground protection
    #     if self.altitude < MIN_ALTITUDE:
    #         self.altitude = MIN_ALTITUDE
    #         self.vert_speed = 0.0

    # def apply_lateral_control(self, desired_turn_rate, desired_accel):
    #     self.commanded_turn_rate = np.clip(
    #         desired_turn_rate,
    #         -self.max_turn_rate,
    #         self.max_turn_rate
    #     )

    #     accel = np.clip(desired_accel, -constants.MAX_ACCEL, constants.MAX_ACCEL)

    #     self.speed += accel
    #     self.speed = np.clip(self.speed, self.min_speed, self.max_speed)


    # def apply_atc_command(self, heading_cmd, speed_cmd, altitude_cmd):
    #     self.target_heading = heading_cmd
    #     self.target_speed = np.clip(speed_cmd, self.min_speed, self.max_speed)
    #     self.target_altitude = altitude_cmd

    def set_targets(self, target_heading_cmd_norm, target_speed_cmd_norm, target_altitude_cmd_norm):
        new_heading = (target_heading_cmd_norm + 1.0) / 2.0 * 2.0 * np.pi
        new_speed = target_speed_cmd_norm
        new_altitude = target_altitude_cmd_norm
    
        instruction_count = 0
        if abs((self.target_heading - new_heading + np.pi) % (2*np.pi) - np.pi) > 0.01:
            self.target_heading = new_heading
            instruction_count += 1
        if abs(self.target_speed - new_speed) > 1.0:
            self.target_speed = np.clip(new_speed, self.min_speed, self.max_speed)
            instruction_count += 1
        if abs(self.target_altitude - new_altitude) > 10.0:
            self.target_altitude = new_altitude
            instruction_count += 1

        return instruction_count
    
    def pilot_speed_control(self, dt):
        speed_error = self.target_speed - self.speed
        desired_accel = ALTITUDE_KP * speed_error
        accel = np.clip(desired_accel, -constants.MAX_ACCEL, constants.MAX_ACCEL)
        self.speed += accel
        self.speed = np.clip(self.speed, self.min_speed, self.max_speed)

    def pilot_heading_control(self, dt):
        # Heading error (wrapped to [-pi, pi])
        heading_err = (self.target_heading - self.heading + np.pi) % (2*np.pi) - np.pi

        # Proportional control
        turn_rate_cmd = HEADING_KP * heading_err

        # Limit to 3 deg/sec
        max_turn = np.deg2rad(3.0)
        self.current_turn_rate = np.clip(turn_rate_cmd, -max_turn, max_turn)

    def pilot_altitude_control(self, dt):
        alt_err = self.target_altitude - self.altitude

        vs_cmd = ALTITUDE_KP * alt_err

        vs_cmd = np.clip(vs_cmd, -MAX_VERT_SPEED, MAX_VERT_SPEED)

        max_vs_delta = MAX_VERT_ACCEL * (dt / 60.0)
        vs_error = vs_cmd - self.vert_speed
        vs_change = np.clip(vs_error, -max_vs_delta, max_vs_delta)

        self.vert_speed += vs_change

    def apply_control(self, dt):
        self.pilot_speed_control(dt)
        self.pilot_heading_control(dt)
        self.pilot_altitude_control(dt)

    # -----------------------------
    # Physics update
    # -----------------------------
    def step(self, dt: float):
        if self.landed:
            return

        self.apply_control(dt)

        # Integrate heading
        self.heading += self.current_turn_rate * dt

        # Integrate altitude
        self.altitude += (self.vert_speed / 60.0) * dt

        # Integrate lateral position
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
        # if dist <= landing_radius and self.altitude <= 1000.0 and abs(self.vert_speed) <= 500.0 and abs(self.current_turn_rate) <= np.deg2rad(5.0) and self.speed <= 170.0:
        if dist <= landing_radius and self.altitude <= 5000.0:
            self.landed = True
            return True

        return False
