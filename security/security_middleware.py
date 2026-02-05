"""
Security Middleware for AI-ATC Application

Implements security headers, CORS policies, and request validation.

Usage:
    from security.security_middleware import setup_security

    app = Flask(__name__)
    setup_security(app)
"""

from flask import request, jsonify
from functools import wraps
import logging
import re
from datetime import datetime, timedelta

logger = logging.getLogger(__name__)


class SecurityHeaders:
    """Security headers configuration"""

    # Strict transport security
    HSTS = "max-age=31536000; includeSubDomains; preload"

    # Content security policy
    CSP = (
        "default-src 'self'; "
        "script-src 'self' 'unsafe-inline'; "
        "style-src 'self' 'unsafe-inline'; "
        "img-src 'self' data:; "
        "font-src 'self'; "
        "connect-src 'self' wss://; "
        "frame-ancestors 'none'; "
        "base-uri 'self'; "
        "form-action 'self'"
    )

    # X-Frame-Options
    X_FRAME_OPTIONS = "DENY"

    # X-Content-Type-Options
    X_CONTENT_TYPE_OPTIONS = "nosniff"

    # X-XSS-Protection
    X_XSS_PROTECTION = "1; mode=block"

    # Referrer-Policy
    REFERRER_POLICY = "strict-origin-when-cross-origin"

    # Permissions-Policy
    PERMISSIONS_POLICY = (
        "geolocation=(), "
        "microphone=(), "
        "camera=(), "
        "magnetometer=(), "
        "gyroscope=(), "
        "accelerometer=()"
    )


def setup_security(app, config=None):
    """Setup security for Flask application"""

    if config is None:
        config = {}

    # Apply security headers
    @app.after_request
    def apply_security_headers(response):
        """Add security headers to all responses"""
        response.headers['Strict-Transport-Security'] = SecurityHeaders.HSTS
        response.headers['Content-Security-Policy'] = SecurityHeaders.CSP
        response.headers['X-Frame-Options'] = SecurityHeaders.X_FRAME_OPTIONS
        response.headers['X-Content-Type-Options'] = SecurityHeaders.X_CONTENT_TYPE_OPTIONS
        response.headers['X-XSS-Protection'] = SecurityHeaders.X_XSS_PROTECTION
        response.headers['Referrer-Policy'] = SecurityHeaders.REFERRER_POLICY
        response.headers['Permissions-Policy'] = SecurityHeaders.PERMISSIONS_POLICY

        # Remove server header
        response.headers.pop('Server', None)

        return response

    # Setup CORS
    setup_cors(app, config)

    # Setup rate limiting
    setup_rate_limiting(app, config)

    # Setup input validation
    setup_input_validation(app)

    logger.info("Security middleware configured")


def setup_cors(app, config):
    """Configure CORS policies"""
    from flask_cors import CORS

    cors_config = {
        "origins": config.get("CORS_ORIGINS", ["http://localhost:3000"]),
        "methods": ["GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS"],
        "allow_headers": ["Content-Type", "Authorization"],
        "expose_headers": ["X-Total-Count", "X-Page-Count"],
        "max_age": 3600,
        "supports_credentials": True,
    }

    CORS(app, resources={r"/api/*": cors_config})
    logger.info("CORS configured")


def setup_rate_limiting(app, config):
    """Configure rate limiting"""
    from flask_limiter import Limiter
    from flask_limiter.util import get_remote_address

    limiter = Limiter(
        app=app,
        key_func=get_remote_address,
        default_limits=config.get("RATE_LIMITS", ["200 per day", "50 per hour"]),
        storage_uri=config.get("REDIS_URL", "memory://"),
    )

    # Specific rate limits for sensitive endpoints
    @app.route('/api/auth/login', methods=['POST'])
    @limiter.limit("5 per minute")
    def login():
        pass

    @app.route('/api/auth/register', methods=['POST'])
    @limiter.limit("3 per minute")
    def register():
        pass

    @app.route('/api/scores', methods=['POST'])
    @limiter.limit("30 per minute")
    def submit_score():
        pass

    logger.info("Rate limiting configured")


def setup_input_validation(app):
    """Setup input validation for all requests"""

    @app.before_request
    def validate_input():
        """Validate all incoming requests"""
        if request.method in ['POST', 'PUT', 'PATCH']:
            # Check Content-Type
            if request.is_json and request.content_type != 'application/json':
                return jsonify({'error': 'Invalid Content-Type'}), 400

            # Validate JSON payload size
            if request.content_length and request.content_length > 1024 * 1024:  # 1MB limit
                return jsonify({'error': 'Payload too large'}), 413

            # Validate request data
            if request.is_json:
                data = request.get_json()
                if not is_valid_json_structure(data):
                    return jsonify({'error': 'Invalid JSON structure'}), 400


def is_valid_json_structure(data, max_depth=10):
    """Validate JSON structure for security"""
    if max_depth <= 0:
        return False

    if isinstance(data, dict):
        if len(data) > 100:  # Max 100 keys
            return False
        return all(
            isinstance(k, str) and
            len(k) < 256 and
            is_valid_json_structure(v, max_depth - 1)
            for k, v in data.items()
        )

    if isinstance(data, list):
        if len(data) > 1000:  # Max 1000 items
            return False
        return all(is_valid_json_structure(item, max_depth - 1) for item in data)

    if isinstance(data, str):
        return len(data) < 10000  # Max 10k chars

    return isinstance(data, (int, float, bool, type(None)))


class InputSanitizer:
    """Sanitize user input to prevent injection attacks"""

    # Regex patterns for validation
    EMAIL_PATTERN = re.compile(r'^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$')
    USERNAME_PATTERN = re.compile(r'^[a-zA-Z0-9_-]{3,32}$')
    CALLSIGN_PATTERN = re.compile(r'^[A-Z0-9]{1,6}$')
    NUMERIC_PATTERN = re.compile(r'^-?\d+\.?\d*$')

    # Dangerous patterns to block
    DANGEROUS_PATTERNS = [
        r'<script',
        r'javascript:',
        r'onclick=',
        r'onerror=',
        r'eval\(',
        r'exec\(',
        r'__proto__',
        r'constructor',
        r'prototype',
        r'__dirname',
        r'__filename',
        r'require\(',
        r'import\(',
    ]

    @staticmethod
    def sanitize_email(email):
        """Validate and sanitize email"""
        if not email or not isinstance(email, str):
            return None

        email = email.strip().lower()

        if not InputSanitizer.EMAIL_PATTERN.match(email):
            return None

        return email

    @staticmethod
    def sanitize_username(username):
        """Validate and sanitize username"""
        if not username or not isinstance(username, str):
            return None

        username = username.strip()

        if not InputSanitizer.USERNAME_PATTERN.match(username):
            return None

        return username

    @staticmethod
    def sanitize_callsign(callsign):
        """Validate and sanitize aircraft callsign"""
        if not callsign or not isinstance(callsign, str):
            return None

        callsign = callsign.strip().upper()

        if not InputSanitizer.CALLSIGN_PATTERN.match(callsign):
            return None

        return callsign

    @staticmethod
    def sanitize_string(value, max_length=1000):
        """Sanitize general string input"""
        if not value or not isinstance(value, str):
            return None

        value = value.strip()

        # Check length
        if len(value) > max_length:
            return None

        # Check for dangerous patterns
        value_lower = value.lower()
        for pattern in InputSanitizer.DANGEROUS_PATTERNS:
            if re.search(pattern, value_lower, re.IGNORECASE):
                logger.warning(f"Dangerous pattern detected in input: {pattern}")
                return None

        return value

    @staticmethod
    def sanitize_number(value, min_val=None, max_val=None):
        """Validate and sanitize numeric input"""
        try:
            num = float(value)

            if min_val is not None and num < min_val:
                return None

            if max_val is not None and num > max_val:
                return None

            return num
        except (ValueError, TypeError):
            return None


def require_auth(f):
    """Decorator to require authentication"""
    @wraps(f)
    def decorated_function(*args, **kwargs):
        from flask import request
        from flask_jwt_extended import verify_jwt_in_request, get_jwt_identity

        try:
            verify_jwt_in_request()
            return f(*args, **kwargs)
        except Exception as e:
            logger.warning(f"Authentication failed: {e}")
            return jsonify({'error': 'Unauthorized'}), 401

    return decorated_function


def require_admin(f):
    """Decorator to require admin role"""
    @wraps(f)
    def decorated_function(*args, **kwargs):
        from flask import request
        from flask_jwt_extended import verify_jwt_in_request, get_jwt

        try:
            verify_jwt_in_request()
            claims = get_jwt()

            if 'Administrator' not in claims.get('roles', []):
                logger.warning(f"Admin access denied for user {claims.get('sub')}")
                return jsonify({'error': 'Forbidden'}), 403

            return f(*args, **kwargs)
        except Exception as e:
            logger.warning(f"Admin check failed: {e}")
            return jsonify({'error': 'Unauthorized'}), 401

    return decorated_function


class AuditLogger:
    """Audit logging for sensitive operations"""

    @staticmethod
    def log_auth_event(event_type, user_id, status, details=None):
        """Log authentication event"""
        log_entry = {
            'timestamp': datetime.utcnow().isoformat(),
            'event_type': event_type,
            'user_id': user_id,
            'status': status,
            'details': details or {},
        }
        logger.info(f"AUDIT: {log_entry}")

    @staticmethod
    def log_admin_action(admin_id, action, resource, resource_id, status):
        """Log admin action"""
        log_entry = {
            'timestamp': datetime.utcnow().isoformat(),
            'admin_id': admin_id,
            'action': action,
            'resource': resource,
            'resource_id': resource_id,
            'status': status,
        }
        logger.info(f"AUDIT_ADMIN: {log_entry}")

    @staticmethod
    def log_security_event(event_type, severity, details):
        """Log security event"""
        log_entry = {
            'timestamp': datetime.utcnow().isoformat(),
            'event_type': event_type,
            'severity': severity,
            'details': details,
            'ip_address': request.remote_addr if request else 'N/A',
        }
        logger.warning(f"SECURITY: {log_entry}")
