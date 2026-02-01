"""
ADSBexchange Integration for Live Aircraft Data

Fetches real-time aircraft positions from ADSBexchange API.

Usage:
    from live_data.adsb_integration import ADSBClient

    client = ADSBClient()
    aircraft = client.get_aircraft_by_region(lat=37.6213, lon=-122.3790, radius=50)
    print(aircraft)
"""

import requests
import logging
import time
from typing import List, Dict, Optional, Tuple
from datetime import datetime, timedelta
from dataclasses import dataclass
import json
import math

logger = logging.getLogger(__name__)


@dataclass
class Aircraft:
    """Live aircraft data"""
    icao: str  # ICAO hex code (unique identifier)
    callsign: str  # Flight callsign (e.g., "AAL456")
    latitude: float
    longitude: float
    altitude_ft: int
    ground_speed_kts: int
    track_deg: float
    vertical_speed_fpm: int
    squawk: str  # Transponder code
    aircraft_type: str  # Aircraft model
    registration: str  # Tail number
    operator: str  # Airline
    destination: str  # IATA code
    origin: str  # IATA code
    last_update_time: datetime

    def to_dict(self) -> Dict:
        """Convert to dictionary"""
        return {
            'icao': self.icao,
            'callsign': self.callsign,
            'latitude': self.latitude,
            'longitude': self.longitude,
            'altitude_ft': self.altitude_ft,
            'ground_speed_kts': self.ground_speed_kts,
            'track_deg': self.track_deg,
            'vertical_speed_fpm': self.vertical_speed_fpm,
            'squawk': self.squawk,
            'aircraft_type': self.aircraft_type,
            'registration': self.registration,
            'operator': self.operator,
            'destination': self.destination,
            'origin': self.origin,
            'last_update_time': self.last_update_time.isoformat(),
        }


class ADSBClient:
    """ADSBexchange API client for live aircraft data"""

    # ADSBexchange endpoints
    BASE_URL = "https://api.adsbexchange.com/v2"
    AIRCRAFT_ENDPOINT = f"{BASE_URL}/json/aircraft/json"
    AIRCRAFT_BY_LAT_LON = f"{BASE_URL}/json/lat/{{lat}}/lon/{{lon}}/dist/{{dist}}"

    # Rate limiting
    RATE_LIMIT_REQUESTS = 100
    RATE_LIMIT_WINDOW_SECONDS = 3600
    MIN_REQUEST_INTERVAL = 2  # Minimum seconds between requests

    # Cache configuration
    CACHE_DURATION_SECONDS = 60
    STALE_DATA_THRESHOLD_SECONDS = 120

    def __init__(self, api_key: Optional[str] = None):
        """Initialize ADSBexchange client"""
        self.api_key = api_key
        self.session = requests.Session()
        self.session.timeout = 10

        # Rate limiting
        self.request_times = []

        # Caching
        self.cache = {}
        self.cache_timestamps = {}

        logger.info("ADSBexchange client initialized")

    def get_aircraft_by_region(
        self,
        lat: float,
        lon: float,
        radius_nm: float = 50,
        min_altitude_ft: int = 1000,
        max_altitude_ft: int = 50000,
    ) -> List[Aircraft]:
        """
        Get live aircraft in geographic region

        Args:
            lat: Center latitude
            lon: Center longitude
            radius_nm: Search radius in nautical miles
            min_altitude_ft: Minimum altitude filter
            max_altitude_ft: Maximum altitude filter

        Returns:
            List of Aircraft objects
        """
        cache_key = f"region_{lat}_{lon}_{radius_nm}"

        # Check cache
        if self._is_cache_valid(cache_key):
            cached_data = self.cache.get(cache_key, [])
            logger.debug(f"Returning {len(cached_data)} aircraft from cache")
            return cached_data

        try:
            # Check rate limit
            if not self._check_rate_limit():
                logger.warning("Rate limit exceeded, using cached data")
                return self.cache.get(cache_key, [])

            # Fetch data from API
            aircraft_data = self._fetch_aircraft_data(lat, lon, radius_nm)

            # Filter by altitude
            aircraft = []
            for data in aircraft_data:
                alt = data.get('alt_baro', data.get('alt_geom', 0))
                if min_altitude_ft <= alt <= max_altitude_ft:
                    aircraft.append(self._parse_aircraft(data))

            # Cache results
            self.cache[cache_key] = aircraft
            self.cache_timestamps[cache_key] = datetime.utcnow()

            logger.info(f"Fetched {len(aircraft)} aircraft near {lat}, {lon}")
            return aircraft

        except Exception as e:
            logger.error(f"Error fetching aircraft data: {e}")
            # Return cached data if available
            return self.cache.get(cache_key, [])

    def get_aircraft_by_airport(
        self,
        airport_code: str,
        radius_nm: float = 30,
    ) -> List[Aircraft]:
        """Get aircraft approaching or departing airport"""
        # Airport coordinates (would ideally come from a database)
        airport_coords = {
            'SFO': (37.6213, -122.3790),
            'LAX': (33.9425, -118.4081),
            'JFK': (40.6413, -73.7781),
            'ORD': (41.9742, -87.9073),
            'ATL': (33.6407, -84.4277),
        }

        if airport_code not in airport_coords:
            logger.warning(f"Unknown airport: {airport_code}")
            return []

        lat, lon = airport_coords[airport_code]
        return self.get_aircraft_by_region(lat, lon, radius_nm)

    def _fetch_aircraft_data(self, lat: float, lon: float, radius_nm: float) -> List[Dict]:
        """Fetch raw aircraft data from ADSBexchange"""
        # Convert NM to km (1 NM = 1.852 km)
        radius_km = radius_nm * 1.852

        url = f"{self.AIRCRAFT_BY_LAT_LON.format(lat=lat, lon=lon, dist=int(radius_km))}"

        headers = {}
        if self.api_key:
            headers['api-auth'] = self.api_key

        try:
            response = self.session.get(url, headers=headers, timeout=10)
            response.raise_for_status()

            data = response.json()
            aircraft_list = data.get('aircraft', [])

            logger.debug(f"API returned {len(aircraft_list)} aircraft")
            return aircraft_list

        except requests.exceptions.RequestException as e:
            logger.error(f"API request failed: {e}")
            raise

    def _parse_aircraft(self, data: Dict) -> Aircraft:
        """Parse raw API data into Aircraft object"""
        return Aircraft(
            icao=data.get('icao', ''),
            callsign=data.get('flight', '').strip() or f"N{data.get('icao', '')}",
            latitude=data.get('lat', 0.0),
            longitude=data.get('lon', 0.0),
            altitude_ft=int(data.get('alt_baro', data.get('alt_geom', 0))),
            ground_speed_kts=int(data.get('gs', 0)),
            track_deg=float(data.get('track', 0)),
            vertical_speed_fpm=int(data.get('baro_rate', 0)),
            squawk=data.get('squawk', '0000'),
            aircraft_type=data.get('t', 'Unknown'),
            registration=data.get('r', ''),
            operator=self._get_operator_from_callsign(data.get('flight', '')),
            destination=data.get('dest', ''),
            origin=data.get('orig', ''),
            last_update_time=datetime.utcnow(),
        )

    def _get_operator_from_callsign(self, callsign: str) -> str:
        """Extract airline operator from flight callsign"""
        if not callsign or len(callsign) < 2:
            return "Unknown"

        # Extract airline prefix (first 3 letters)
        prefix = callsign[:3].upper()

        # Common airline callsigns
        airline_map = {
            'AAL': 'American',
            'UAL': 'United',
            'DAL': 'Delta',
            'SWR': 'Southwest',
            'BAW': 'British Airways',
            'AFR': 'Air France',
            'BAE': 'British Aerospace',
            'KLM': 'KLM',
            'LUF': 'Lufthansa',
            'EZY': 'EasyJet',
            'RYR': 'Ryanair',
        }

        return airline_map.get(prefix, prefix)

    def _check_rate_limit(self) -> bool:
        """Check if rate limit is exceeded"""
        now = time.time()

        # Remove old request times
        self.request_times = [t for t in self.request_times
                              if now - t < self.RATE_LIMIT_WINDOW_SECONDS]

        # Check if limit exceeded
        if len(self.request_times) >= self.RATE_LIMIT_REQUESTS:
            logger.warning("Rate limit nearly exceeded")
            return False

        # Check minimum interval between requests
        if self.request_times and now - self.request_times[-1] < self.MIN_REQUEST_INTERVAL:
            logger.debug("Minimum request interval not met, skipping request")
            return False

        # Record this request
        self.request_times.append(now)
        return True

    def _is_cache_valid(self, cache_key: str) -> bool:
        """Check if cached data is still valid"""
        if cache_key not in self.cache_timestamps:
            return False

        age = (datetime.utcnow() - self.cache_timestamps[cache_key]).total_seconds()
        return age < self.CACHE_DURATION_SECONDS

    def get_cache_status(self) -> Dict:
        """Get cache status information"""
        return {
            'cached_queries': len(self.cache),
            'total_cached_aircraft': sum(len(v) for v in self.cache.values()),
            'cache_ages_seconds': {
                k: (datetime.utcnow() - self.cache_timestamps[k]).total_seconds()
                for k in self.cache.keys()
            },
            'requests_in_window': len(self.request_times),
            'rate_limit_remaining': self.RATE_LIMIT_REQUESTS - len(self.request_times),
        }

    def clear_cache(self) -> None:
        """Clear all cached data"""
        self.cache.clear()
        self.cache_timestamps.clear()
        logger.info("Cache cleared")


class LiveDataCache:
    """Advanced caching for live aircraft data"""

    def __init__(self, ttl_seconds: int = 60):
        """Initialize cache"""
        self.ttl = ttl_seconds
        self.cache = {}

    def get(self, key: str) -> Optional[List[Aircraft]]:
        """Get cached aircraft"""
        if key not in self.cache:
            return None

        data, timestamp = self.cache[key]

        # Check if expired
        if (datetime.utcnow() - timestamp).total_seconds() > self.ttl:
            del self.cache[key]
            return None

        return data

    def set(self, key: str, aircraft: List[Aircraft]) -> None:
        """Cache aircraft data"""
        self.cache[key] = (aircraft, datetime.utcnow())

    def invalidate(self, pattern: str = None) -> None:
        """Invalidate cache entries matching pattern"""
        if pattern is None:
            self.cache.clear()
        else:
            keys_to_delete = [k for k in self.cache.keys() if pattern in k]
            for k in keys_to_delete:
                del self.cache[k]

    def get_stats(self) -> Dict:
        """Get cache statistics"""
        return {
            'cached_keys': len(self.cache),
            'total_aircraft': sum(len(v[0]) for v in self.cache.values()),
            'ttl_seconds': self.ttl,
        }


def filter_aircraft_for_approach(
    aircraft: List[Aircraft],
    target_airport_lat: float,
    target_airport_lon: float,
    max_distance_nm: float = 30,
) -> List[Aircraft]:
    """Filter aircraft likely to be approaching airport"""
    approaching = []

    for ac in aircraft:
        # Calculate distance to airport
        distance = _calculate_distance(
            ac.latitude, ac.longitude,
            target_airport_lat, target_airport_lon
        )

        # Only include if within reasonable approach distance
        if distance <= max_distance_nm:
            # Check if heading toward airport
            bearing_to_airport = _calculate_bearing(
                ac.latitude, ac.longitude,
                target_airport_lat, target_airport_lon
            )

            # Aircraft heading within 45° of airport bearing
            heading_diff = abs(ac.track_deg - bearing_to_airport)
            if heading_diff > 180:
                heading_diff = 360 - heading_diff

            if heading_diff <= 45:
                approaching.append(ac)

    return approaching


def _calculate_distance(lat1: float, lon1: float, lat2: float, lon2: float) -> float:
    """Calculate distance between two points in nautical miles"""
    # Haversine formula
    R = 3440.07  # Earth's radius in nautical miles

    lat1_rad = math.radians(lat1)
    lat2_rad = math.radians(lat2)
    delta_lat = math.radians(lat2 - lat1)
    delta_lon = math.radians(lon2 - lon1)

    a = math.sin(delta_lat / 2) ** 2 + \
        math.cos(lat1_rad) * math.cos(lat2_rad) * math.sin(delta_lon / 2) ** 2
    c = 2 * math.asin(math.sqrt(a))

    return R * c


def _calculate_bearing(lat1: float, lon1: float, lat2: float, lon2: float) -> float:
    """Calculate bearing from point 1 to point 2 in degrees"""
    lat1_rad = math.radians(lat1)
    lat2_rad = math.radians(lat2)
    delta_lon = math.radians(lon2 - lon1)

    y = math.sin(delta_lon) * math.cos(lat2_rad)
    x = math.cos(lat1_rad) * math.sin(lat2_rad) - \
        math.sin(lat1_rad) * math.cos(lat2_rad) * math.cos(delta_lon)

    bearing = math.atan2(y, x)
    bearing_deg = math.degrees(bearing)

    # Normalize to 0-360
    return (bearing_deg + 360) % 360


if __name__ == '__main__':
    # Example usage
    logging.basicConfig(level=logging.INFO)

    client = ADSBClient()

    # Get aircraft near SFO
    print("Fetching aircraft near SFO...")
    aircraft = client.get_aircraft_by_airport('SFO', radius_nm=30)

    print(f"Found {len(aircraft)} aircraft")
    for ac in aircraft[:5]:
        print(f"  {ac.callsign}: {ac.aircraft_type} at {ac.altitude_ft} ft")

    # Filter for approaching aircraft
    print("\nFiltering for approaching aircraft...")
    approaching = filter_aircraft_for_approach(
        aircraft,
        target_airport_lat=37.6213,
        target_airport_lon=-122.3790,
    )

    print(f"Found {len(approaching)} approaching aircraft")
    for ac in approaching:
        print(f"  {ac.callsign}: {ac.altitude_ft} ft, {ac.ground_speed_kts} kt")

    # Cache status
    print("\nCache Status:")
    print(client.get_cache_status())
