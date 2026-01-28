import numpy as np
import pytest
from ai_atc_env import AIATCEnv
from airplane import MAX_VERT_ACCEL, Airplane
from conftest import arrival_plane

AIRPORT = np.array([0.0, 0.0], dtype=np.float32)

def test_terminal_reward_increases_when_closer(arrival_plane):
    arrival_plane.position_nm = np.array([20.0, 0.0])
    arrival_plane.altitude = 20.0 * 318  # on glide path
    r_far = arrival_plane.compute_pilot_reward(curriculum_stage=2)

    arrival_plane.position_nm = np.array([5.0, 0.0])
    arrival_plane.altitude = 5.0 * 318  # on glide path
    r_near = arrival_plane.compute_pilot_reward(curriculum_stage=2)

    assert r_near > r_far

def test_arrival_descent_reward(arrival_plane):
    arrival_plane.position_nm = np.array([10.0, 0.0])
    arrival_plane.altitude = 3180  # on glide path

    r = arrival_plane.compute_pilot_reward(curriculum_stage=1)

    assert r > 0

def test_arrival_climb_penalty(arrival_plane):
    arrival_plane.position_nm = np.array([10.0, 0.0])
    baseline = arrival_plane.compute_pilot_reward(curriculum_stage=1)  # alt=5000, above glide

    arrival_plane.altitude = 2000  # below glide

    low_alt = arrival_plane.compute_pilot_reward(curriculum_stage=1)

    assert low_alt > baseline

def test_glide_slope_reasonable(arrival_plane):
    arrival_plane.speed = 150.0        # kts
    arrival_plane.vert_speed = -750.0  # fpm (≈ 3° glide slope)

    # Horizontal speed in ft/min:
    horiz_fpm = arrival_plane.speed * 6076.12 / 60.0

    slope = abs(arrival_plane.vert_speed) / horiz_fpm

    # tan(3°) ≈ 0.052
    assert 0.04 < slope < 0.07

def test_descent_better_than_climb_near_runway():

    env = AIATCEnv(max_planes=1, render_mode=None)

    plane = env.spawn_on_final(plane_id=0, distance_nm=8, altitude_ft=3000, intercept_deg=0.0)
    
    plane.vert_speed = -500
    r_descend = plane.compute_pilot_reward()

    plane.vert_speed = 500
    r_climb   = plane.compute_pilot_reward()

    assert r_descend > r_climb
