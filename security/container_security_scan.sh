#!/bin/bash

# Container Security Vulnerability Scanning
# Scans Docker images and containers for vulnerabilities
#
# Prerequisites:
#   - Docker installed
#   - Trivy installed: https://github.com/aquasecurity/trivy
#
# Usage:
#   ./container_security_scan.sh scan <image>      # Scan image
#   ./container_security_scan.sh scan-all           # Scan all images
#   ./container_security_scan.sh report             # Generate report

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Configuration
SCAN_DIR="security/scan-results"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
SEVERITY_THRESHOLD="HIGH"

# Create results directory
mkdir -p "$SCAN_DIR"

log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

log_header() {
    echo -e "${BLUE}=== $1 ===${NC}"
}

check_trivy() {
    if ! command -v trivy &> /dev/null; then
        log_error "Trivy not installed"
        echo "Install from: https://github.com/aquasecurity/trivy"
        exit 1
    fi
}

scan_image() {
    local image="$1"

    log_header "Scanning Image: $image"

    local report_file="$SCAN_DIR/trivy_${image//\//_}_${TIMESTAMP}.json"

    # Run Trivy scan
    trivy image \
        --format json \
        --output "$report_file" \
        --severity CRITICAL,HIGH,MEDIUM \
        --exit-code 0 \
        "$image"

    if [ $? -eq 0 ]; then
        log_info "Scan completed: $report_file"
        analyze_report "$report_file" "$image"
    else
        log_warn "Scan found vulnerabilities"
    fi
}

scan_dockerfile() {
    local dockerfile="$1"

    if [ ! -f "$dockerfile" ]; then
        log_error "Dockerfile not found: $dockerfile"
        return 1
    fi

    log_header "Scanning Dockerfile: $dockerfile"

    local report_file="$SCAN_DIR/hadolint_${dockerfile//\//_}_${TIMESTAMP}.txt"

    # Run Hadolint (Dockerfile linter)
    if command -v hadolint &> /dev/null; then
        hadolint "$dockerfile" > "$report_file" 2>&1 || true
        log_info "Dockerfile scan: $report_file"
    else
        log_warn "Hadolint not installed, skipping Dockerfile scan"
    fi
}

analyze_report() {
    local report_file="$1"
    local image="$2"

    if [ ! -f "$report_file" ]; then
        return
    fi

    # Count vulnerabilities
    local critical_count=$(jq '[.Results[]?.Misconfigurations[]? | select(.Severity=="CRITICAL")] | length' "$report_file" 2>/dev/null || echo 0)
    local high_count=$(jq '[.Results[]?.Misconfigurations[]? | select(.Severity=="HIGH")] | length' "$report_file" 2>/dev/null || echo 0)
    local medium_count=$(jq '[.Results[]?.Misconfigurations[]? | select(.Severity=="MEDIUM")] | length' "$report_file" 2>/dev/null || echo 0)

    echo ""
    echo "Vulnerability Summary:"
    echo "  Critical: $critical_count"
    echo "  High: $high_count"
    echo "  Medium: $medium_count"
    echo ""

    # Show critical vulnerabilities
    if [ "$critical_count" -gt 0 ]; then
        log_error "Critical vulnerabilities detected!"
        jq '.Results[]?.Misconfigurations[]? | select(.Severity=="CRITICAL") | {Title, Description}' "$report_file" 2>/dev/null || true
        echo ""
    fi

    # Show high vulnerabilities
    if [ "$high_count" -gt 0 ]; then
        log_warn "High severity vulnerabilities detected"
        jq '.Results[]?.Misconfigurations[]? | select(.Severity=="HIGH") | .Title' "$report_file" 2>/dev/null | head -5
        echo ""
    fi
}

scan_docker_socket() {
    log_header "Scanning Docker Socket Configuration"

    # Check if Docker daemon is running securely
    if [ -e /var/run/docker.sock ]; then
        local perms=$(stat -f '%OLp' /var/run/docker.sock 2>/dev/null || stat -c '%a' /var/run/docker.sock 2>/dev/null)
        log_info "Docker socket permissions: $perms"

        if [ "$perms" != "660" ]; then
            log_warn "Docker socket has non-standard permissions"
        fi
    fi
}

generate_security_report() {
    local report_file="$SCAN_DIR/security_report_${TIMESTAMP}.txt"

    log_header "Generating Security Report"

    cat > "$report_file" << 'EOF'
# Container Security Scan Report

## Summary

This report documents security scan results for AI-ATC container images.

## Scanned Images

EOF

    # Add scan results
    for scan in "$SCAN_DIR"/trivy_*.json; do
        if [ -f "$scan" ]; then
            local image=$(basename "$scan" | sed 's/trivy_//;s/_[0-9]*\.json//')
            echo "- $image" >> "$report_file"
        fi
    done

    cat >> "$report_file" << 'EOF'

## Remediation

### Critical Vulnerabilities

For each critical vulnerability:
1. Review the CVE details
2. Update the base image to a patched version
3. Rebuild and re-scan the image
4. Update deployment

### High Severity Issues

1. Update dependencies to patched versions
2. Remove unused packages from Dockerfile
3. Use multi-stage builds to minimize image size
4. Use specific versions, not latest tags

### Best Practices

1. Scan images regularly (on every build)
2. Use minimal base images (alpine, distroless)
3. Run containers as non-root
4. Implement network policies
5. Use private registries
6. Sign container images

## Next Steps

1. Review critical vulnerabilities
2. Update dependencies
3. Rebuild images
4. Re-run scans
5. Update deployment

EOF

    log_info "Report saved: $report_file"
}

show_usage() {
    cat << EOF
Container Security Scanner

Usage:
    $0 scan <image>         Scan specific image
    $0 scan-all             Scan all local images
    $0 scan-docker          Scan Dockerfile
    $0 socket               Check Docker socket
    $0 report               Generate report
    $0 help                 Show this help

Examples:
    $0 scan ai-atc:latest
    $0 scan-all
    $0 scan-docker Dockerfile

Prerequisites:
    - Docker installed
    - Trivy installed: https://github.com/aquasecurity/trivy

EOF
}

# Main
main() {
    local command="${1:-help}"

    case "$command" in
        scan)
            if [ -z "$2" ]; then
                log_error "Image name required"
                show_usage
                exit 1
            fi
            check_trivy
            scan_image "$2"
            ;;
        scan-all)
            check_trivy
            log_header "Scanning All Local Images"
            docker images --format "{{.Repository}}:{{.Tag}}" | while read image; do
                scan_image "$image"
            done
            ;;
        scan-docker)
            if [ -z "$2" ]; then
                log_error "Dockerfile path required"
                show_usage
                exit 1
            fi
            scan_dockerfile "$2"
            ;;
        socket)
            scan_docker_socket
            ;;
        report)
            generate_security_report
            ;;
        help|--help|-h)
            show_usage
            ;;
        *)
            log_error "Unknown command: $command"
            show_usage
            exit 1
            ;;
    esac
}

main "$@"
