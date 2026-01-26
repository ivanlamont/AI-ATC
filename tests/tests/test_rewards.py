import numpy as np
import pytest
from airplane import MAX_VERT_ACCEL, Airplane
from conftest import arrival_plane

AIRPORT = np.array([0.0, 0.0], dtype=np.float32)

def test_terminal_reward_increases_when_closer(arrival_plane):
    arrival_plane.position_nm = np.array([20.0, 0.0])
    r_far = arrival_plane.compute_pilot_reward(curriculum_stage=2)

    arrival_plane.position_nm = np.array([5.0, 0.0])
    r_near = arrival_plane.compute_pilot_reward(curriculum_stage=2)

    assert r_near > r_far

def test_arrival_descent_reward(arrival_plane):
    arrival_plane.altitude = 8000
    arrival_plane.target_altitude = 3000

    r = arrival_plane.compute_pilot_reward(curriculum_stage=1)

    assert r > 0

def test_arrival_climb_penalty(arrival_plane):
    baseline = arrival_plane.compute_pilot_reward(curriculum_stage=1)

    arrival_plane.altitude = 3000
    arrival_plane.target_altitude = 8000

    climbwhenlanding = arrival_plane.compute_pilot_reward(curriculum_stage=1)

    assert climbwhenlanding < baseline

def test_glide_slope_reasonable(arrival_plane):
    arrival_plane.speed = 150.0        # kts
    arrival_plane.vert_speed = -750.0  # fpm (≈ 3° glide slope)

    # Horizontal speed in ft/min:
    horiz_fpm = arrival_plane.speed * 6076.12 / 60.0

    slope = abs(arrival_plane.vert_speed) / horiz_fpm

    # tan(3°) ≈ 0.052
    assert 0.04 < slope < 0.07
