#!/bin/bash

# Performance Testing Runner Script
# Runs various performance tests and generates reports
#
# Usage:
#   ./run-performance-tests.sh              # Run all tests
#   ./run-performance-tests.sh basic        # Run basic load test
#   ./run-performance-tests.sh stress       # Run stress test
#   ./run-performance-tests.sh soak         # Run soak test

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
BASE_URL="${BASE_URL:-http://localhost:5000}"
RESULTS_DIR="performance/results"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)

# Create results directory
mkdir -p "$RESULTS_DIR"

log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

check_k6() {
    if ! command -v k6 &> /dev/null; then
        log_error "k6 not installed. Install from https://k6.io/docs/get-started/installation/"
        exit 1
    fi
}

check_api_health() {
    log_info "Checking API health at $BASE_URL..."
    if ! curl -s "$BASE_URL/health" > /dev/null; then
        log_error "API is not responding at $BASE_URL"
        log_info "Make sure to start the server first"
        exit 1
    fi
    log_info "API is healthy!"
}

run_basic_load_test() {
    log_info "Running basic load test..."
    local report_file="$RESULTS_DIR/basic_load_test_${TIMESTAMP}.json"

    k6 run k6/load-test-basic.js \
        --out json="$report_file" \
        --env BASE_URL="$BASE_URL" \
        || { log_error "Basic load test failed"; exit 1; }

    log_info "Basic load test results saved to $report_file"
    analyze_results "$report_file"
}

run_stress_test() {
    log_info "Running stress test..."
    local report_file="$RESULTS_DIR/stress_test_${TIMESTAMP}.json"

    k6 run k6/load-test-stress.js \
        --out json="$report_file" \
        --env BASE_URL="$BASE_URL" \
        || log_warn "Stress test completed with issues (expected)"

    log_info "Stress test results saved to $report_file"
    analyze_results "$report_file"
}

run_soak_test() {
    log_info "Running soak test (this will take a while)..."
    local report_file="$RESULTS_DIR/soak_test_${TIMESTAMP}.json"
    local duration="${1:-8h}"

    k6 run k6/load-test-soak.js \
        --duration "$duration" \
        --out json="$report_file" \
        --env BASE_URL="$BASE_URL" \
        || log_warn "Soak test completed with issues"

    log_info "Soak test results saved to $report_file"
    analyze_results "$report_file"
}

run_profile_cpu() {
    log_info "Running CPU profiling..."
    python performance/performance_profiler.py --profile cpu
}

run_profile_memory() {
    log_info "Running memory profiling..."
    python performance/performance_profiler.py --profile memory
}

run_monitor() {
    log_info "Running system monitoring (60 seconds)..."
    local report_file="$RESULTS_DIR/monitoring_${TIMESTAMP}.txt"

    python performance/performance_profiler.py \
        --profile monitor \
        --duration 60 \
        --output "$report_file"

    log_info "Monitoring results saved to $report_file"
}

analyze_results() {
    local report_file="$1"

    log_info "Analyzing results from $report_file..."

    # Extract key metrics using jq (if available)
    if command -v jq &> /dev/null; then
        local error_rate=$(jq '.metrics.errors.values.rate // 0' "$report_file")
        local p95=$(jq '.metrics.api_duration.values.p95 // 0' "$report_file")

        log_info "Error Rate: $error_rate"
        log_info "p95 Latency: ${p95}ms"

        if (( $(echo "$error_rate > 0.1" | bc -l) )); then
            log_warn "High error rate detected!"
        fi

        if (( $(echo "$p95 > 500" | bc -l) )); then
            log_warn "High p95 latency detected!"
        fi
    fi
}

generate_comparison_report() {
    log_info "Generating comparison report..."

    local report_file="$RESULTS_DIR/comparison_${TIMESTAMP}.txt"

    cat > "$report_file" << EOF
# Performance Test Comparison Report
Generated: $(date)
API URL: $BASE_URL

## Test Results

### Basic Load Test
- Ramps from 10 to 100 VUs
- 12 minute duration
- Result: $RESULTS_DIR/basic_load_test_*.json

### Stress Test
- Ramps from 100 to 750 VUs
- 13 minute duration
- Result: $RESULTS_DIR/stress_test_*.json

### Soak Test
- Sustained at 50 VUs
- 8 hour duration
- Result: $RESULTS_DIR/soak_test_*.json

## Analysis

Review the JSON result files for detailed metrics.

## Next Steps

1. Review the results in Grafana or k6 Cloud
2. Identify performance bottlenecks
3. Implement optimizations from PERFORMANCE_OPTIMIZATION.md
4. Retest to validate improvements

## Documentation

See docs/PERFORMANCE_OPTIMIZATION.md for detailed optimization strategies.
EOF

    log_info "Comparison report saved to $report_file"
}

show_usage() {
    cat << EOF
Performance Testing Script for AI-ATC

Usage:
    $0 [command] [options]

Commands:
    all             Run all tests (basic, stress, soak)
    basic           Run basic load test
    stress          Run stress test
    soak [duration] Run soak test (default: 8h)
    cpu             Profile CPU usage
    memory          Profile memory usage
    monitor         Monitor system performance
    health          Check API health
    report          Generate comparison report

Environment Variables:
    BASE_URL        API endpoint (default: http://localhost:5000)

Examples:
    $0 basic
    $0 stress
    $0 soak 4h
    $0 monitor
    BASE_URL=http://api.example.com $0 basic

EOF
}

# Main script
main() {
    local command="${1:-all}"

    case "$command" in
        all)
            check_k6
            check_api_health
            run_basic_load_test
            run_stress_test
            generate_comparison_report
            log_info "All tests completed! Check $RESULTS_DIR for results."
            ;;
        basic)
            check_k6
            check_api_health
            run_basic_load_test
            ;;
        stress)
            check_k6
            check_api_health
            run_stress_test
            ;;
        soak)
            check_k6
            check_api_health
            local duration="${2:-8h}"
            run_soak_test "$duration"
            ;;
        cpu)
            run_profile_cpu
            ;;
        memory)
            run_profile_memory
            ;;
        monitor)
            run_monitor
            ;;
        health)
            check_api_health
            ;;
        report)
            generate_comparison_report
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
