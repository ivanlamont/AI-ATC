from airplane import MAX_VERT_ACCEL, MAX_VERT_SPEED

import numpy as np
np.set_printoptions(precision=3, suppress=True)

def test_turn_rate_limit(arrival_plane):
    arrival_plane.apply_atc_clearance(turn_rate_cmd=np.deg2rad(10.0), accel_cmd=0.0, vert_speed_cmd=0.0, dt=1.0)

    assert abs(arrival_plane.current_turn_rate) <= np.deg2rad(3.0) + 1e-6


def test_vertical_speed_limit(arrival_plane):
    arrival_plane.apply_atc_clearance(turn_rate_cmd=0.0, accel_cmd=0.0, vert_speed_cmd=4000.0, dt=1.0)

    assert abs(arrival_plane.vert_speed) <= MAX_VERT_SPEED + 1e-6


def test_accel_limit(arrival_plane):
    initial_speed = arrival_plane.speed
    arrival_plane.apply_atc_clearance(turn_rate_cmd=0.0, accel_cmd=10.0, vert_speed_cmd=0.0, dt=1.0)
    arrival_plane.step(dt=1.0)
    speed_change = arrival_plane.speed - initial_speed
    assert abs(speed_change) <= 5.0 + 1e-6


def test_lateral_motion_nm(arrival_plane):
    arrival_plane.heading = 0.0  # east
    arrival_plane.speed = 120.0  # kts

    start = arrival_plane.position_nm.copy()

    # 60 seconds at 120 kts = 2 NM
    arrival_plane.step(dt=60.0)

    dx = arrival_plane.position_nm[0] - start[0]

    assert abs(dx - 2.0) < 0.05, f"Expected ~2 NM, got {dx}"


def test_descent(arrival_plane):
    initial_altitude = arrival_plane.altitude
    arrival_plane.apply_atc_clearance(turn_rate_cmd=0.0, accel_cmd=0.0, vert_speed_cmd=-1000.0, dt=1.0)

    # Step for 60 seconds (1 minute) at -1000 ft/min should drop 1000 ft
    arrival_plane.step(dt=60.0)

    altitude_drop = initial_altitude - arrival_plane.altitude
    assert abs(altitude_drop - 1000.0) < 1.0, f"Expected ~1000 ft drop, got {altitude_drop}"

