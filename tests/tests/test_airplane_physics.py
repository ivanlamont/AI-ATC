from airplane import MAX_VERT_ACCEL

import numpy as np
np.set_printoptions(precision=3, suppress=True)

def test_turn_rate_limit(arrival_plane):
    arrival_plane.heading = 0.0
    arrival_plane.target_heading = np.pi

    arrival_plane.pilot_heading_control(dt=1.0)

    assert abs(arrival_plane.current_turn_rate) <= np.deg2rad(3.0) + 1e-6


def test_vertical_accel_limit(arrival_plane):
    arrival_plane.vert_speed = 0.0
    arrival_plane.altitude = 10000
    arrival_plane.target_altitude = 0

    arrival_plane.pilot_altitude_control(dt=1.0)

    max_vs_delta = MAX_VERT_ACCEL * (1.0 / 60.0)
    assert abs(arrival_plane.vert_speed) <= max_vs_delta + 1e-6

def test_lateral_motion_nm(arrival_plane):
    arrival_plane.heading = 0.0  # east
    arrival_plane.speed = 120.0  # kts

    start = arrival_plane.position_nm.copy()

    # 60 seconds at 120 kts = 2 NM
    arrival_plane.step(dt=60.0)

    dx = arrival_plane.position_nm[0] - start[0]

    assert abs(dx - 2.0) < 0.05, f"Expected ~2 NM, got {dx}"

