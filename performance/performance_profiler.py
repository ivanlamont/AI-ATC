"""
Performance Profiling and Analysis Tool for AI-ATC

Analyze application performance, identify bottlenecks, and generate optimization reports.

Usage:
    python performance_profiler.py --profile cpu
    python performance_profiler.py --profile memory
    python performance_profiler.py --analyze logs/
"""

import sys
import time
import psutil
import cProfile
import pstats
import io
import json
import logging
from pathlib import Path
from typing import Dict, List, Tuple
from dataclasses import dataclass, asdict
from datetime import datetime
import argparse

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)


@dataclass
class PerformanceMetric:
    """Performance metric data"""
    timestamp: str
    metric_name: str
    value: float
    unit: str
    status: str  # 'good', 'warning', 'critical'


class PerformanceProfiler:
    """Profile Python application performance"""

    def __init__(self):
        self.metrics: List[PerformanceMetric] = []
        self.process = psutil.Process()

    def profile_cpu(self, func, *args, **kwargs):
        """Profile CPU usage of a function"""
        logger.info("Starting CPU profiling...")

        profiler = cProfile.Profile()
        profiler.enable()

        try:
            result = func(*args, **kwargs)
        finally:
            profiler.disable()

        # Generate report
        s = io.StringIO()
        ps = pstats.Stats(profiler, stream=s).sort_stats('cumulative')
        ps.print_stats(20)  # Top 20 functions

        report = s.getvalue()
        logger.info("CPU Profiling Report:\n" + report)

        return result, report

    def profile_memory(self, func, *args, **kwargs):
        """Profile memory usage of a function"""
        logger.info("Starting memory profiling...")

        # Get initial memory
        import tracemalloc
        tracemalloc.start()

        try:
            result = func(*args, **kwargs)
        finally:
            current, peak = tracemalloc.get_traced_memory()
            tracemalloc.stop()

        memory_used_mb = current / 1024 / 1024
        memory_peak_mb = peak / 1024 / 1024

        logger.info(f"Memory Current: {memory_used_mb:.2f} MB")
        logger.info(f"Memory Peak: {memory_peak_mb:.2f} MB")

        return result, {
            'current_mb': memory_used_mb,
            'peak_mb': memory_peak_mb,
        }

    def monitor_performance(self, duration_seconds=60, interval=1):
        """Monitor system performance over time"""
        logger.info(f"Monitoring performance for {duration_seconds} seconds...")

        start_time = time.time()
        measurements = []

        while time.time() - start_time < duration_seconds:
            measurement = {
                'timestamp': datetime.now().isoformat(),
                'cpu_percent': self.process.cpu_percent(interval=0.1),
                'memory_mb': self.process.memory_info().rss / 1024 / 1024,
                'threads': self.process.num_threads(),
                'fds': self.process.num_fds() if hasattr(self.process, 'num_fds') else 0,
            }
            measurements.append(measurement)
            logger.info(f"CPU: {measurement['cpu_percent']:.1f}%, Memory: {measurement['memory_mb']:.1f} MB")

            time.sleep(interval)

        return measurements

    def analyze_database_query_performance(self, queries: List[Dict]) -> Dict:
        """Analyze database query performance"""
        logger.info(f"Analyzing {len(queries)} queries...")

        analysis = {
            'total_queries': len(queries),
            'slow_queries': [],
            'fast_queries': [],
            'avg_duration_ms': 0.0,
            'p95_duration_ms': 0.0,
            'p99_duration_ms': 0.0,
        }

        durations = [q['duration_ms'] for q in queries if 'duration_ms' in q]

        if durations:
            durations.sort()
            analysis['avg_duration_ms'] = sum(durations) / len(durations)
            analysis['p95_duration_ms'] = durations[int(len(durations) * 0.95)]
            analysis['p99_duration_ms'] = durations[int(len(durations) * 0.99)]

            # Identify slow queries (> 100ms)
            analysis['slow_queries'] = [q for q in queries if q.get('duration_ms', 0) > 100]
            analysis['fast_queries'] = [q for q in queries if q.get('duration_ms', 0) <= 10]

        logger.info(f"Average query time: {analysis['avg_duration_ms']:.2f} ms")
        logger.info(f"P95 query time: {analysis['p95_duration_ms']:.2f} ms")
        logger.info(f"Slow queries (>100ms): {len(analysis['slow_queries'])}")

        return analysis

    def generate_performance_report(self, measurements: List[Dict]) -> str:
        """Generate a performance report"""
        report = []
        report.append("=" * 60)
        report.append("PERFORMANCE REPORT")
        report.append("=" * 60)
        report.append(f"Generated: {datetime.now().isoformat()}\n")

        if measurements:
            cpu_values = [m['cpu_percent'] for m in measurements]
            memory_values = [m['memory_mb'] for m in measurements]

            report.append("CPU Usage:")
            report.append(f"  Average: {sum(cpu_values) / len(cpu_values):.1f}%")
            report.append(f"  Max: {max(cpu_values):.1f}%")
            report.append(f"  Min: {min(cpu_values):.1f}%\n")

            report.append("Memory Usage:")
            report.append(f"  Average: {sum(memory_values) / len(memory_values):.1f} MB")
            report.append(f"  Max: {max(memory_values):.1f} MB")
            report.append(f"  Min: {min(memory_values):.1f} MB\n")

        report.append("=" * 60)
        return "\n".join(report)


class DatabaseOptimizationAnalyzer:
    """Analyze and suggest database optimizations"""

    @staticmethod
    def analyze_slow_queries(queries: List[Dict]) -> List[str]:
        """Suggest optimizations for slow queries"""
        suggestions = []

        for query in queries:
            if query.get('duration_ms', 0) > 100:
                query_text = query.get('query', '').lower()

                if 'select *' in query_text:
                    suggestions.append(f"Query '{query[:50]}...' uses SELECT *. Specify needed columns.")

                if query_text.count('join') > 2:
                    suggestions.append(f"Query '{query[:50]}...' has many joins. Consider denormalization.")

                if 'like' in query_text and query_text.startswith("like '%"):
                    suggestions.append(f"Query '{query[:50]}...' has leading wildcard. Add index on column.")

        return suggestions

    @staticmethod
    def suggest_indexes(queries: List[Dict]) -> List[Dict]:
        """Suggest missing indexes based on query patterns"""
        suggestions = []
        column_frequencies = {}

        # Analyze WHERE clauses
        for query in queries:
            query_text = query.get('query', '')
            if 'where' in query_text.lower():
                # Extract column names (simplified)
                parts = query_text.split()
                for i, part in enumerate(parts):
                    if parts[i-1].lower() == 'where' or (i > 0 and parts[i-1].lower() in ['and', 'or']):
                        column = part.split('=')[0].strip()
                        column_frequencies[column] = column_frequencies.get(column, 0) + 1

        # Suggest indexes for frequently used columns
        for column, freq in sorted(column_frequencies.items(), key=lambda x: x[1], reverse=True)[:5]:
            if freq >= 5:  # Used in at least 5 queries
                suggestions.append({
                    'column': column,
                    'frequency': freq,
                    'recommendation': f"CREATE INDEX idx_{column} ON table_{column} ({column})"
                })

        return suggestions


class CachingOptimizationAnalyzer:
    """Analyze and suggest caching optimizations"""

    @staticmethod
    def analyze_cache_hit_rate(cache_stats: Dict) -> Dict:
        """Analyze cache effectiveness"""
        total_requests = cache_stats.get('hits', 0) + cache_stats.get('misses', 0)

        if total_requests == 0:
            hit_rate = 0.0
        else:
            hit_rate = cache_stats.get('hits', 0) / total_requests * 100

        return {
            'hit_rate_percent': hit_rate,
            'recommendation': 'Hit rate is good' if hit_rate > 80 else 'Consider caching more data',
            'memory_used_mb': cache_stats.get('memory_mb', 0),
            'evictions': cache_stats.get('evictions', 0),
        }

    @staticmethod
    def suggest_cache_strategies(cache_analysis: Dict) -> List[str]:
        """Suggest Redis caching strategies"""
        suggestions = []

        if cache_analysis['hit_rate_percent'] < 80:
            suggestions.append("Increase cache TTL or cached data size")

        if cache_analysis['evictions'] > 0:
            suggestions.append("Increase Redis maxmemory or implement cache warming")

        suggestions.append("Consider using cache-aside pattern for expensive computations")
        suggestions.append("Implement cache invalidation on data updates")

        return suggestions


def main():
    """Main entry point"""
    parser = argparse.ArgumentParser(description="Performance Analysis Tool for AI-ATC")
    parser.add_argument('--profile', choices=['cpu', 'memory', 'monitor'], help="Type of profiling")
    parser.add_argument('--duration', type=int, default=60, help="Monitoring duration (seconds)")
    parser.add_argument('--analyze', type=str, help="Path to log file for analysis")
    parser.add_argument('--output', type=str, help="Output file for report")

    args = parser.parse_args()

    if args.profile == 'monitor':
        profiler = PerformanceProfiler()
        measurements = profiler.monitor_performance(duration_seconds=args.duration)
        report = profiler.generate_performance_report(measurements)
        print(report)

        if args.output:
            with open(args.output, 'w') as f:
                f.write(report)
            logger.info(f"Report written to {args.output}")

    elif args.analyze:
        # Load and analyze query log
        with open(args.analyze, 'r') as f:
            queries = [json.loads(line) for line in f if line.strip()]

        analyzer = DatabaseOptimizationAnalyzer()
        suggestions = analyzer.analyze_slow_queries(queries)
        index_suggestions = analyzer.suggest_indexes(queries)

        print("\nOptimization Suggestions:")
        for suggestion in suggestions:
            print(f"  - {suggestion}")

        print("\nSuggested Indexes:")
        for idx_sug in index_suggestions:
            print(f"  - {idx_sug['recommendation']} (used {idx_sug['frequency']} times)")

    else:
        print("Usage: python performance_profiler.py --profile [cpu|memory|monitor]")
        print("       python performance_profiler.py --analyze <logfile> [--output <report>]")


if __name__ == '__main__':
    main()
