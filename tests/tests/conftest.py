import numpy as np
import pytest
from airplane import Airplane

@pytest.fixture
def arrival_plane():
    return Airplane(
        plane_id=1,
        position_nm=np.array([10.0, 0.0], dtype=np.float32),  # 10 NM out
        destination_nm=np.array([0.0, 0.0], dtype=np.float32),
        heading_rads=0.0,
        speed_kts=180.0,
        min_speed_kts=120.0,
        max_speed_kts=250.0,
        max_turn_rate_rads=np.deg2rad(3.0),
        init_altitude_ft=5000.0,
        is_arrival=True,
    )
