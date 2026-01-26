import numpy as np

EARTH_RADIUS_M = 6378137.0

def latlon_to_enu(lat, lon, lat0, lon0):
    dlat = np.deg2rad(lat - lat0)
    dlon = np.deg2rad(lon - lon0)

    x_east  = EARTH_RADIUS_M * dlon * np.cos(np.deg2rad(lat0))
    y_north = EARTH_RADIUS_M * dlat

    return x_east, y_north

def enu_to_latlon(x_east, y_north, lat0, lon0):
    dlat = y_north / EARTH_RADIUS_M
    dlon = x_east / (EARTH_RADIUS_M * np.cos(np.deg2rad(lat0)))

    lat = lat0 + np.rad2deg(dlat)
    lon = lon0 + np.rad2deg(dlon)

    return lat, lon
