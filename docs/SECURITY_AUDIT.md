# 🔒 Security Audit & Hardening Guide

Comprehensive security audit checklist and OWASP Top 10 prevention strategies for AI-ATC.

## Security Audit Checklist

### Authentication & Authorization

- [ ] **Password Requirements**
  - Minimum 12 characters
  - Mix of upper/lower case, numbers, symbols
  - No common patterns or dictionary words
  - Password history prevents reuse

- [ ] **Session Management**
  - Sessions expire after 30 minutes of inactivity
  - Sessions expire after 24 hours absolute
  - Session IDs are cryptographically random (128+ bits)
  - Secure cookie flags: HttpOnly, Secure, SameSite=Strict

- [ ] **Multi-Factor Authentication (MFA)**
  - MFA available for user accounts
  - Fallback codes for MFA recovery
  - Backup authentication method enforced

- [ ] **OAuth2 Implementation**
  - PKCE flow used for public clients
  - Code_challenge_method = S256 (SHA-256)
  - Authorization code expires in 10 minutes
  - Refresh tokens expire in 30 days
  - Scope restrictions enforced

- [ ] **Permission Model**
  - Role-Based Access Control (RBAC) implemented
  - Principle of least privilege enforced
  - Permission checks on every endpoint
  - Admin actions logged and auditable

### API Security

- [ ] **Rate Limiting**
  - Anonymous: 100 requests/minute
  - Authenticated: 1000 requests/minute
  - Admin: 5000 requests/minute
  - Endpoint-specific limits (stricter for auth)

- [ ] **Input Validation**
  - All user inputs validated before processing
  - Email format validation
  - Callsign format validation (alphanumeric only)
  - Numeric inputs checked for range
  - String length limits enforced

- [ ] **Output Encoding**
  - JSON responses properly escaped
  - HTML special characters encoded
  - No raw user input in responses
  - Error messages don't leak sensitive data

- [ ] **API Versioning**
  - Versioning support for backward compatibility
  - Deprecated versions have sunset policy
  - API documentation current

### Data Protection

- [ ] **Encryption in Transit**
  - HTTPS enforced (HTTP → HTTPS redirect)
  - TLS 1.2 minimum (1.3 preferred)
  - Strong cipher suites configured
  - HSTS header set (min-age 31536000)
  - Certificate valid and not self-signed

- [ ] **Encryption at Rest**
  - Database passwords encrypted
  - API keys encrypted in storage
  - Sensitive user data encrypted
  - Backup files encrypted
  - Encryption keys rotated regularly

- [ ] **Data Minimization**
  - Only necessary data collected
  - Data retention policy enforced
  - Old data securely deleted
  - User data deletion requests honored

### Infrastructure Security

- [ ] **Network Security**
  - Firewall configured and active
  - Only necessary ports exposed (80, 443)
  - SSH restricted (key-based only)
  - DDoS protection enabled
  - WAF rules configured

- [ ] **Container Security**
  - Images scanned for vulnerabilities
  - No root user in containers
  - Read-only filesystems where possible
  - Resource limits set
  - Secrets not stored in images

- [ ] **Database Security**
  - Database server not publicly accessible
  - Strong database passwords
  - Connection encryption enabled
  - Query logging enabled
  - Unused features disabled

- [ ] **Secrets Management**
  - Secrets not in source code
  - Environment variables for secrets
  - Secrets rotated regularly (30-90 days)
  - Access to secrets logged
  - Backup secrets stored securely

### Application Security

- [ ] **Security Headers**
  - Content-Security-Policy set
  - X-Frame-Options: DENY
  - X-Content-Type-Options: nosniff
  - X-XSS-Protection: 1; mode=block
  - Strict-Transport-Security set

- [ ] **Error Handling**
  - Stack traces not exposed to users
  - Generic error messages shown
  - Detailed errors logged server-side
  - No sensitive data in logs

- [ ] **Logging & Monitoring**
  - Security events logged
  - Login attempts logged (successes and failures)
  - Admin actions logged
  - Log retention policy (90+ days)
  - Logs protected from tampering

- [ ] **Dependency Management**
  - Dependencies kept up-to-date
  - Security advisories monitored
  - Vulnerable packages removed immediately
  - Dependency scanning in CI/CD

### Testing & Validation

- [ ] **Security Testing**
  - OWASP ZAP scan performed
  - Penetration testing scheduled
  - Code security review completed
  - Dependency audit completed
  - Container scan completed

- [ ] **Incident Response**
  - Incident response plan documented
  - Security contacts identified
  - Breach notification procedure defined
  - Incident logging enabled
  - Regular drills scheduled

---

## OWASP Top 10 Prevention

### 1. Broken Access Control

**Risk**: Users access unauthorized resources

**Prevention**:
```python
# BAD: No authorization check
@app.route('/api/users/<user_id>')
def get_user(user_id):
    return User.query.get(user_id)

# GOOD: Authorization enforced
@app.route('/api/users/<user_id>')
@require_auth
def get_user(user_id):
    current_user = get_current_user()
    if current_user.id != user_id and not current_user.is_admin:
        return jsonify({'error': 'Forbidden'}), 403
    return User.query.get(user_id)
```

**Verification**:
- [ ] Permission checks on all endpoints
- [ ] User can only access own data (except admins)
- [ ] Admin permissions verified on admin endpoints

### 2. Cryptographic Failures

**Risk**: Sensitive data exposed

**Prevention**:
```python
# Database encryption
from cryptography.fernet import Fernet

def encrypt_sensitive_data(data, key):
    f = Fernet(key)
    return f.encrypt(data.encode())

def decrypt_sensitive_data(encrypted_data, key):
    f = Fernet(key)
    return f.decrypt(encrypted_data).decode()

# Use HTTPS only
app.config['SESSION_COOKIE_SECURE'] = True
app.config['SESSION_COOKIE_HTTPONLY'] = True
app.config['SESSION_COOKIE_SAMESITE'] = 'Strict'
```

**Verification**:
- [ ] HTTPS enforced
- [ ] Sensitive data encrypted
- [ ] No hardcoded secrets
- [ ] Keys rotated regularly

### 3. Injection

**Risk**: Attackers inject malicious code

**Prevention**:
```python
# BAD: SQL injection
user = User.query.filter(f"email='{email}'")

# GOOD: Parameterized query
user = User.query.filter(User.email == email)

# BAD: Command injection
os.system(f"convert {user_file}")

# GOOD: Safe subprocess
subprocess.run(['convert', user_file], check=True)

# BAD: XSS
return render_template('profile.html', name=user_input)

# GOOD: Escape output
from markupsafe import escape
return render_template('profile.html', name=escape(user_input))
```

**Verification**:
- [ ] All inputs validated
- [ ] Parameterized queries used
- [ ] No string concatenation in queries
- [ ] Output properly encoded

### 4. Insecure Design

**Risk**: Missing security controls in design

**Prevention**:
- Design threat model and identify risks
- Implement defense in depth
- Fail securely
- Assume zero trust

```python
# Threat model example
"""
Threat: Unauthorized access to leaderboard
Mitigation: Implement rate limiting + session validation
"""

# Implementation
@app.route('/api/leaderboard')
@limiter.limit("100 per minute")
@require_auth
def get_leaderboard():
    return leaderboard_data
```

### 5. Security Misconfiguration

**Risk**: Insecure default settings

**Prevention**:
```python
# Flask secure defaults
app.config.update(
    DEBUG=False,  # Never in production
    SESSION_COOKIE_SECURE=True,
    SESSION_COOKIE_HTTPONLY=True,
    SESSION_COOKIE_SAMESITE='Strict',
    PREFERRED_URL_SCHEME='https',
    SEND_FILE_MAX_AGE_DEFAULT=0,  # No cache for dynamic content
)

# Disable unnecessary features
app.config['JSON_SORT_KEYS'] = False
app.config['RESTFUL_ERROR_404_HELP'] = False
```

### 6. Vulnerable & Outdated Components

**Risk**: Using packages with known vulnerabilities

**Prevention**:
```bash
# Check for vulnerabilities
pip install safety
safety check

# Update dependencies
pip install --upgrade -r requirements.txt

# Use dependency pinning
pip freeze > requirements.txt

# Automated scanning
# In CI/CD: bandit, safety, pip-audit
```

### 7. Authentication Failures

**Risk**: Weak authentication mechanisms

**Prevention**:
```python
# Use strong hashing
from werkzeug.security import generate_password_hash, check_password_hash

password_hash = generate_password_hash(password, method='pbkdf2:sha256:150000')
if check_password_hash(password_hash, submitted_password):
    # Authenticate user
    pass

# Implement lockout after failed attempts
failed_attempts = cache.get(f"login_attempts:{email}")
if failed_attempts > 5:
    return "Account locked. Try again in 15 minutes"

# Log all authentication attempts
logger.info(f"Login attempt for {email}: {'success' if valid else 'failed'}")
```

### 8. Software & Data Integrity Failures

**Risk**: Compromised software or data

**Prevention**:
```python
# Verify dependencies integrity
# Use hash checking in requirements.txt
# requests==2.28.1 --hash=sha256:...

# Sign deployments
# Use GPG signatures for releases
# Verify signatures before deployment

# Implement checksums
import hashlib

def verify_file_integrity(file_path, expected_hash):
    sha256_hash = hashlib.sha256()
    with open(file_path, 'rb') as f:
        for block in iter(lambda: f.read(4096), b''):
            sha256_hash.update(block)
    return sha256_hash.hexdigest() == expected_hash
```

### 9. Logging & Monitoring Failures

**Risk**: Security incidents go undetected

**Prevention**:
```python
import logging

# Setup security logging
security_logger = logging.getLogger('security')
security_logger.setLevel(logging.INFO)

# Log security events
security_logger.info(f"User login: {user_id}")
security_logger.warning(f"Failed login attempt: {email}")
security_logger.error(f"Authorization denied: {user_id} accessing {resource}")

# Monitor for anomalies
if failed_attempts > 5:
    alert("Multiple failed login attempts")
```

### 10. Server-Side Request Forgery (SSRF)

**Risk**: Application makes unintended requests

**Prevention**:
```python
# BAD: SSRF vulnerability
import requests
url = request.args.get('url')
response = requests.get(url)  # Can access internal services!

# GOOD: Whitelist URLs
ALLOWED_HOSTS = ['api.example.com', 'cdn.example.com']
url = request.args.get('url')
parsed = urlparse(url)
if parsed.netloc not in ALLOWED_HOSTS:
    return "Invalid URL", 400
response = requests.get(url)

# GOOD: Disable internal redirects
requests.get(url, allow_redirects=False)

# GOOD: Validate schemes
if not url.startswith(('https://', 'http://')):
    return "Invalid URL", 400
```

---

## Security Testing Tools

### Static Analysis

```bash
# Python code security issues
pip install bandit
bandit -r . -ll  # Only high/medium severity

# Dependency vulnerabilities
pip install safety
safety check

# Code quality and security
pip install pylint
pylint your_module.py
```

### Dynamic Analysis

```bash
# Web application testing
docker run -it owasp/zap:stable \
  -t http://localhost:5000 \
  -r report.html

# API fuzzing
pip install atheris
atheris fuzz_target.py
```

### Container Security

```bash
# Scan images
trivy image ai-atc:latest

# Lint Dockerfile
hadolint Dockerfile

# Runtime security
docker run --rm -v /var/run/docker.sock:/var/run/docker.sock \
  aquasec/trivy image --severity HIGH ai-atc:latest
```

---

## Incident Response Procedure

### 1. Detection & Analysis
- Monitor security alerts
- Verify the incident
- Determine scope and impact
- Document initial findings

### 2. Containment
- Isolate affected systems
- Stop the attack
- Preserve evidence
- Notify stakeholders

### 3. Eradication
- Remove malicious code
- Patch vulnerabilities
- Reset credentials
- Update systems

### 4. Recovery
- Restore from clean backups
- Verify system integrity
- Monitor for re-compromise
- Gradually bring online

### 5. Lessons Learned
- Review incident handling
- Update procedures
- Improve controls
- Update security training

---

## Security Configuration Files

### WAF Rules (example)

```
# Block SQL injection attempts
SecRule ARGS:email "@rx (\sunion\s|\sand\s|\swhere\s)" "id:1000,deny"

# Block path traversal
SecRule REQUEST_URI "@contains ../" "id:2000,deny"

# Block common XSS
SecRule ARGS "@rx (<script|javascript:|onerror=)" "id:3000,deny"

# Rate limiting
SecAction "id:4000,phase:1,initcol:ip=%{REMOTE_ADDR},pass"
SecRule IP:requests "@gt 100" "id:4001,deny,status:429"
```

### Secrets Rotation

```python
# Rotate secrets every 90 days
SECRETS_ROTATION_DAYS = 90

def should_rotate_secrets():
    last_rotation = get_last_rotation_date()
    return (datetime.now() - last_rotation).days > SECRETS_ROTATION_DAYS

def rotate_all_secrets():
    rotate_database_password()
    rotate_api_keys()
    rotate_encryption_keys()
    log_rotation_event()
```

---

## Compliance Checklist

### GDPR (if applicable)

- [ ] Privacy policy updated
- [ ] Data processing agreements in place
- [ ] User consent collection mechanism
- [ ] Right to be forgotten implemented
- [ ] Data breach notification plan
- [ ] Privacy impact assessment completed
- [ ] DPO (Data Protection Officer) appointed (if required)

### SOC 2 Type II (if required)

- [ ] Security policies documented
- [ ] Access controls implemented
- [ ] Monitoring enabled
- [ ] Incident response plan
- [ ] Change management procedures
- [ ] Availability monitoring
- [ ] Encryption implemented
- [ ] Annual audit scheduled

---

## Monitoring & Alerting

### Key Security Metrics

- Failed login attempts per user (alert if > 5 in 10 minutes)
- Failed API requests (alert if > 10% error rate)
- Unusual API endpoint access patterns
- High memory/CPU usage (potential attack)
- Rate limit violations per IP

### Alert Rules

```yaml
- name: HighFailedLogins
  condition: failed_logins > 5 in 10m
  action: lock_account, notify_admin

- name: HighErrorRate
  condition: error_rate > 10%
  action: page_oncall, create_incident

- name: UnusualTraffic
  condition: requests_per_second > 1000
  action: enable_ddos_protection, alert
```

---

## Resources

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [Security Checklist](https://github.com/goldbergyoni/nodebestpractices#6-security-best-practices)
- [CWE/SANS Top 25](https://cwe.mitre.org/top25/)
- [NIST Cybersecurity Framework](https://www.nist.gov/cyberframework)

## See Also

- [Getting Started](GETTING_STARTED.md)
- [Performance Optimization](PERFORMANCE_OPTIMIZATION.md)
- [Troubleshooting](TROUBLESHOOTING.md)

---

**Last Updated**: 2024-01-31
**Status**: Ready for Security Audit
