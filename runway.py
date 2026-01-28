import numpy as np
from airport import Airport

class Runway:
    def __init__(
        self,
        airport: Airport,
        runway_heading_deg: float,
        faf_distance_nm: float = 6.0,
    ):
        """
        runway_heading_deg: direction aircraft fly TOWARD runway (inbound)
        """
        self.airport = airport
        self.airport_latlon = airport.position_nm
        self.runway_heading_rad = np.deg2rad(runway_heading_deg)
        self.faf_distance_nm = faf_distance_nm

        # Unit vectors in ENU/NM frame
        self.localizer_dir = np.array([
            np.cos(self.runway_heading_rad),
            np.sin(self.runway_heading_rad)
        ], dtype=np.float32)

        # Opposite direction = outbound
        self.outbound_dir = -self.localizer_dir

    def localizer_point_nm(self, distance_nm: float):
        """
        Point along localizer at given distance FROM runway
        (positive = inbound final)
        """
        return self.outbound_dir * distance_nm

    def faf_point_nm(self):
        return self.localizer_point_nm(self.faf_distance_nm)

