"""
Secrets Management for AI-ATC

Handles secure storage and rotation of secrets.

Usage:
    from security.secrets_manager import SecretsManager

    manager = SecretsManager()
    db_password = manager.get_secret('database_password')
    manager.rotate_secret('database_password')
"""

import os
import json
import logging
from datetime import datetime, timedelta
from typing import Dict, Optional, Any
from cryptography.fernet import Fernet
from pathlib import Path
import hashlib

logger = logging.getLogger(__name__)


class SecretsManager:
    """Manage application secrets securely"""

    def __init__(self, secrets_file: Optional[str] = None, key_file: Optional[str] = None):
        """Initialize secrets manager"""
        self.secrets_file = secrets_file or os.getenv('SECRETS_FILE', '.secrets/secrets.json')
        self.key_file = key_file or os.getenv('SECRETS_KEY_FILE', '.secrets/key')
        self.rotation_days = int(os.getenv('SECRETS_ROTATION_DAYS', 90))

        # Create secrets directory
        Path(self.secrets_file).parent.mkdir(parents=True, exist_ok=True)

        # Load or create encryption key
        self.cipher = self._load_or_create_key()

        # Load secrets
        self.secrets = self._load_secrets()

    def _load_or_create_key(self) -> Fernet:
        """Load encryption key or create if doesn't exist"""
        if os.path.exists(self.key_file):
            with open(self.key_file, 'rb') as f:
                key = f.read()
        else:
            key = Fernet.generate_key()
            Path(self.key_file).parent.mkdir(parents=True, exist_ok=True)
            with open(self.key_file, 'wb') as f:
                f.write(key)
            os.chmod(self.key_file, 0o600)
            logger.info(f"Created new encryption key: {self.key_file}")

        return Fernet(key)

    def _load_secrets(self) -> Dict[str, Any]:
        """Load encrypted secrets from file"""
        if not os.path.exists(self.secrets_file):
            return {}

        try:
            with open(self.secrets_file, 'r') as f:
                data = json.load(f)

            # Decrypt secrets
            decrypted = {}
            for key, encrypted_value in data.items():
                try:
                    decrypted_value = self.cipher.decrypt(encrypted_value.encode()).decode()
                    decrypted[key] = json.loads(decrypted_value)
                except Exception as e:
                    logger.error(f"Failed to decrypt secret {key}: {e}")

            return decrypted
        except Exception as e:
            logger.error(f"Failed to load secrets: {e}")
            return {}

    def _save_secrets(self) -> bool:
        """Save encrypted secrets to file"""
        try:
            # Encrypt secrets
            encrypted = {}
            for key, value in self.secrets.items():
                value_json = json.dumps(value)
                encrypted_value = self.cipher.encrypt(value_json.encode()).decode()
                encrypted[key] = encrypted_value

            # Write to file
            Path(self.secrets_file).parent.mkdir(parents=True, exist_ok=True)
            with open(self.secrets_file, 'w') as f:
                json.dump(encrypted, f)

            os.chmod(self.secrets_file, 0o600)
            logger.info(f"Saved secrets to {self.secrets_file}")
            return True
        except Exception as e:
            logger.error(f"Failed to save secrets: {e}")
            return False

    def get_secret(self, name: str, default: Any = None) -> Any:
        """Get secret value"""
        if name not in self.secrets:
            logger.warning(f"Secret not found: {name}")
            return default

        secret_data = self.secrets[name]

        # Check if secret has expired
        if 'expires_at' in secret_data:
            expiry = datetime.fromisoformat(secret_data['expires_at'])
            if datetime.utcnow() > expiry:
                logger.warning(f"Secret has expired: {name}")
                return None

        return secret_data.get('value')

    def set_secret(self, name: str, value: str, ttl_days: int = 90) -> bool:
        """Set or update secret"""
        expires_at = (datetime.utcnow() + timedelta(days=ttl_days)).isoformat()

        self.secrets[name] = {
            'value': value,
            'created_at': datetime.utcnow().isoformat(),
            'expires_at': expires_at,
            'rotation_due': False,
        }

        logger.info(f"Set secret: {name} (expires in {ttl_days} days)")
        return self._save_secrets()

    def delete_secret(self, name: str) -> bool:
        """Delete secret"""
        if name in self.secrets:
            del self.secrets[name]
            logger.info(f"Deleted secret: {name}")
            return self._save_secrets()
        return False

    def list_secrets(self, include_values: bool = False) -> Dict[str, Any]:
        """List all secrets (metadata only by default)"""
        result = {}
        for name, data in self.secrets.items():
            result[name] = {
                'created_at': data.get('created_at'),
                'expires_at': data.get('expires_at'),
                'rotation_due': self._is_rotation_due(name),
            }
            if include_values:
                result[name]['value'] = data.get('value')
        return result

    def _is_rotation_due(self, name: str) -> bool:
        """Check if secret rotation is due"""
        secret_data = self.secrets.get(name)
        if not secret_data:
            return False

        created_at = datetime.fromisoformat(secret_data['created_at'])
        days_since_creation = (datetime.utcnow() - created_at).days

        return days_since_creation >= self.rotation_days

    def rotate_secret(self, name: str) -> bool:
        """Rotate (regenerate) a secret"""
        if name not in self.secrets:
            logger.warning(f"Cannot rotate non-existent secret: {name}")
            return False

        old_secret = self.secrets[name].get('value')

        # Generate new value based on secret type
        new_value = self._generate_new_secret(name, old_secret)

        if not new_value:
            logger.error(f"Failed to generate new value for secret: {name}")
            return False

        # Archive old secret
        self._archive_secret(name, old_secret)

        # Set new secret
        self.set_secret(name, new_value)

        logger.info(f"Rotated secret: {name}")
        return True

    def _generate_new_secret(self, name: str, old_value: str) -> Optional[str]:
        """Generate new secret value"""
        if 'password' in name.lower():
            return self._generate_password(16)
        elif 'key' in name.lower():
            return Fernet.generate_key().decode()
        elif 'token' in name.lower():
            return self._generate_token(32)
        else:
            # Default: generate random string
            return self._generate_token(32)

    def _generate_password(self, length: int = 16) -> str:
        """Generate secure random password"""
        import secrets
        import string

        alphabet = string.ascii_letters + string.digits + string.punctuation
        return ''.join(secrets.choice(alphabet) for _ in range(length))

    def _generate_token(self, length: int = 32) -> str:
        """Generate secure random token"""
        import secrets
        return secrets.token_hex(length // 2)

    def _archive_secret(self, name: str, value: str) -> bool:
        """Archive old secret for audit purposes"""
        archive_dir = Path('.secrets/archive')
        archive_dir.mkdir(parents=True, exist_ok=True)

        archive_file = archive_dir / f"{name}_{datetime.utcnow().timestamp()}.json"

        archive_data = {
            'name': name,
            'value_hash': hashlib.sha256(value.encode()).hexdigest(),
            'archived_at': datetime.utcnow().isoformat(),
        }

        try:
            with open(archive_file, 'w') as f:
                json.dump(archive_data, f)
            os.chmod(archive_file, 0o600)
            logger.info(f"Archived secret: {name}")
            return True
        except Exception as e:
            logger.error(f"Failed to archive secret: {e}")
            return False

    def check_rotations_due(self) -> Dict[str, bool]:
        """Check which secrets need rotation"""
        result = {}
        for name in self.secrets:
            result[name] = self._is_rotation_due(name)
        return result

    def rotate_all_due(self) -> Dict[str, bool]:
        """Rotate all secrets that are due for rotation"""
        results = {}
        for name, is_due in self.check_rotations_due().items():
            if is_due:
                results[name] = self.rotate_secret(name)
        return results

    def export_for_deployment(self, env_file: str = '.env') -> bool:
        """Export secrets as environment variables"""
        try:
            lines = []
            for name, secret_data in self.secrets.items():
                value = secret_data.get('value')
                lines.append(f"{name}={value}")

            with open(env_file, 'w') as f:
                f.write('\n'.join(lines))

            os.chmod(env_file, 0o600)
            logger.info(f"Exported secrets to {env_file}")
            return True
        except Exception as e:
            logger.error(f"Failed to export secrets: {e}")
            return False


class EnvironmentSecrets:
    """Load secrets from environment with validation"""

    REQUIRED_SECRETS = [
        'DATABASE_PASSWORD',
        'REDIS_PASSWORD',
        'JWT_SECRET',
        'API_KEY',
    ]

    @staticmethod
    def load_and_validate() -> Dict[str, str]:
        """Load all required secrets from environment"""
        secrets = {}

        for name in EnvironmentSecrets.REQUIRED_SECRETS:
            value = os.getenv(name)

            if not value:
                raise ValueError(f"Required secret not set: {name}")

            if len(value) < 16:
                raise ValueError(f"Secret too short (min 16 chars): {name}")

            secrets[name] = value

        logger.info(f"Loaded {len(secrets)} secrets from environment")
        return secrets

    @staticmethod
    def validate_secret(name: str, value: str) -> bool:
        """Validate a secret value"""
        if not value:
            logger.error(f"Secret is empty: {name}")
            return False

        if len(value) < 16:
            logger.error(f"Secret too short: {name}")
            return False

        return True


def create_secrets_template() -> Dict[str, str]:
    """Create template for required secrets"""
    return {
        'DATABASE_PASSWORD': 'your_secure_db_password_here',
        'DATABASE_URL': 'postgresql://user:password@localhost/ai_atc',
        'REDIS_PASSWORD': 'your_secure_redis_password',
        'JWT_SECRET': 'your_secure_jwt_secret_key',
        'API_KEY': 'your_api_key',
        'OAUTH_CLIENT_SECRET': 'your_oauth_secret',
        'ENCRYPTION_KEY': 'your_encryption_key',
    }


if __name__ == '__main__':
    # Example usage
    manager = SecretsManager()

    # Set a secret
    manager.set_secret('api_key', 'secret_value_123', ttl_days=90)

    # Get a secret
    api_key = manager.get_secret('api_key')
    print(f"API Key: {api_key}")

    # List secrets
    print("Secrets:", manager.list_secrets())

    # Check rotations due
    print("Rotations due:", manager.check_rotations_due())

    # Export for deployment
    manager.export_for_deployment()
