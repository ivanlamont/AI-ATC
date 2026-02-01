# ⚡ Performance Optimization Guide

Comprehensive guide to optimizing AI-ATC performance for production deployments.

## Performance Baseline

Target performance metrics:

| Metric | Target | Status |
|--------|--------|--------|
| API Response Time (p95) | < 500ms | Baseline |
| API Response Time (p99) | < 1000ms | Baseline |
| Error Rate | < 1% | Baseline |
| Concurrent Users | 100+ | Target |
| Database Query Time (avg) | < 50ms | Target |
| Cache Hit Rate | > 80% | Target |
| CPU Usage | < 70% | Target |
| Memory Usage | < 80% | Target |

## Load Testing

### Running Load Tests

**Basic load test** (10-100 users):
```bash
k6 run k6/load-test-basic.js
```

**Stress test** (100-750 users):
```bash
k6 run k6/load-test-stress.js --vus 500
```

**Soak test** (sustained load):
```bash
k6 run k6/load-test-soak.js --duration 8h
```

### Interpreting Results

**Healthy Results**:
- Error rate < 1%
- p95 latency < 500ms
- p99 latency < 1000ms
- No 503 or 504 errors

**Warning Signs**:
- Error rate 1-5%
- p95 latency > 500ms
- Increasing 429 (rate limit) errors
- Memory gradually increasing

**Critical Issues**:
- Error rate > 5%
- p99 latency > 5000ms
- Connection timeouts
- Memory runaway

## Database Optimization

### Query Optimization

**Identify slow queries**:
```bash
# Enable slow query log
SET GLOBAL slow_query_log = 'ON';
SET GLOBAL long_query_time = 0.5;
```

**Analyze slow queries**:
```bash
mysqldumpslow /var/log/mysql/slow.log | head -20
```

### Index Strategy

**Critical Indexes**:
```sql
-- User queries
CREATE INDEX idx_email ON users(email);
CREATE INDEX idx_username ON users(username);

-- Leaderboard queries
CREATE INDEX idx_score_difficulty ON scores(difficulty, score DESC);
CREATE INDEX idx_user_difficulty ON scores(user_id, difficulty);

-- Session queries
CREATE INDEX idx_session_user_time ON sessions(user_id, created_at DESC);
```

**Check index usage**:
```sql
SELECT * FROM sys.statements_with_full_table_scans;
```

### Query Examples

**Poor (N+1 queries)**:
```python
# BAD: N+1 problem
users = User.all()
for user in users:
    scores = Score.filter_by(user_id=user.id)  # N queries!
```

**Good (Join queries)**:
```python
# GOOD: Single query
users_with_scores = db.session.query(User).outerjoin(Score)
```

### Database Connection Pooling

**Configuration**:
```python
# SQLAlchemy connection pool
engine = create_engine(
    DATABASE_URL,
    poolclass=QueuePool,
    pool_size=20,          # Number of connections to pool
    max_overflow=40,       # Additional connections if needed
    pool_recycle=3600,     # Recycle connections hourly
    echo=False,
)
```

**Connection Pool Monitoring**:
```python
from sqlalchemy import event

@event.listens_for(engine, "connect")
def receive_connect(dbapi_conn, connection_record):
    logger.info(f"New connection, pool size: {engine.pool.checkedout()}")
```

## Redis Caching Optimization

### Cache-Aside Pattern

```python
from redis import Redis
import json

redis = Redis(host='localhost', port=6379, decode_responses=True)

def get_leaderboard(difficulty):
    cache_key = f"leaderboard:{difficulty}"

    # Try cache first
    cached = redis.get(cache_key)
    if cached:
        return json.loads(cached)

    # Cache miss - fetch from database
    data = fetch_from_database(difficulty)

    # Store in cache (1 hour TTL)
    redis.setex(cache_key, 3600, json.dumps(data))

    return data
```

### Cache Warming

```python
def warm_cache():
    """Pre-populate cache with frequently accessed data"""
    difficulties = ['beginner', 'intermediate', 'advanced', 'expert']

    for difficulty in difficulties:
        data = fetch_from_database(difficulty)
        redis.setex(
            f"leaderboard:{difficulty}",
            3600,
            json.dumps(data)
        )

    logger.info("Cache warming completed")
```

### Caching Strategy

**What to Cache**:
- Leaderboards (changes infrequently)
- Scenario metadata
- User profiles
- Static configuration

**What NOT to Cache**:
- Real-time game state
- Frequently changing scores
- Personal user data (privacy)

**Cache Invalidation**:

```python
def submit_score(user_id, difficulty, score):
    # Save score
    new_score = Score.create(user_id, difficulty, score)
    db.commit()

    # Invalidate affected caches
    redis.delete(f"leaderboard:{difficulty}")
    redis.delete(f"user_stats:{user_id}")

    return new_score
```

## API Optimization

### Response Compression

```python
# Enable gzip compression
from flask_compress import Compress

app = Flask(__name__)
Compress(app)

# Configure compression
app.config['COMPRESS_MIN_SIZE'] = 1024  # Min 1KB
```

### Pagination

```python
@app.route('/api/leaderboard')
def get_leaderboard():
    page = request.args.get('page', 1, type=int)
    per_page = request.args.get('per_page', 50, type=int)

    # Limit max page size
    per_page = min(per_page, 100)

    scores = Score.query.paginate(page, per_page)
    return jsonify({
        'items': scores.items,
        'total': scores.total,
        'pages': scores.pages,
    })
```

### Request Rate Limiting

```python
from flask_limiter import Limiter
from flask_limiter.util import get_remote_address

limiter = Limiter(
    app,
    key_func=get_remote_address,
    default_limits=["200 per day", "50 per hour"]
)

@app.route('/api/submit-score', methods=['POST'])
@limiter.limit("10 per minute")
def submit_score():
    # Handle score submission
    pass
```

## SignalR Hub Optimization

### Connection Pooling

```csharp
// Configure SignalR options
services.AddSignalR(hubOptions =>
{
    hubOptions.MaximumReceiveMessageSize = 32 * 1024; // 32 KB
    hubOptions.StreamBufferCapacity = 10;
});
```

### Message Batching

```csharp
// Batch multiple updates before sending
private List<ScoreUpdate> _updateBatch = new();
private Timer _batchTimer;

private void OnScoreUpdate(ScoreUpdate update)
{
    _updateBatch.Add(update);

    // Flush batch every 100ms or when full
    if (_updateBatch.Count >= 50)
    {
        FlushBatch();
    }
}

private void FlushBatch()
{
    if (_updateBatch.Count == 0) return;

    _hubContext.Clients.All.SendAsync("BatchUpdate", _updateBatch);
    _updateBatch.Clear();
}
```

### Hub Scaling

```csharp
// Enable scaleout with Redis
services.AddSignalR()
    .AddStackExchangeRedis("localhost:6379", options =>
    {
        options.Configuration.AbortOnConnectFail = false;
    });
```

## AI Model Inference Optimization

### Batch Inference

```python
def predict_batch(game_states):
    """Process multiple states in batch for efficiency"""
    # Instead of single inference
    # states = [env1, env2, env3]
    # predictions = [model.predict(s) for s in states]  # 3 passes

    # Use batch inference
    states_tensor = torch.stack([
        torch.tensor(s.state) for s in game_states
    ])
    predictions = model.predict(states_tensor)  # 1 pass
    return predictions
```

### Model Caching

```python
from functools import lru_cache

@lru_cache(maxsize=10000)
def evaluate_state_cached(state_tuple):
    """Cache model evaluations for repeated states"""
    return model.predict(state_tuple)
```

### GPU Memory Management

```python
import torch

# Use mixed precision for faster inference
with torch.cuda.amp.autocast():
    predictions = model(input_tensor)

# Clear cache periodically
torch.cuda.empty_cache()
```

## Container Optimization

### Resource Requests and Limits

```yaml
# Kubernetes deployment
resources:
  requests:
    memory: "512Mi"
    cpu: "250m"
  limits:
    memory: "1Gi"
    cpu: "500m"
```

### Horizontal Scaling

```bash
# Auto-scale based on CPU usage
kubectl autoscale deployment ai-atc-api \
  --min=3 \
  --max=10 \
  --cpu-percent=70
```

### Health Checks

```python
@app.route('/health')
def health_check():
    return jsonify({
        'status': 'healthy',
        'timestamp': datetime.now().isoformat(),
        'database': check_database(),
        'cache': check_redis(),
    })
```

## Monitoring and Alerting

### Prometheus Metrics

```python
from prometheus_client import Counter, Histogram, Gauge

request_count = Counter('http_requests_total', 'Total HTTP requests')
request_duration = Histogram('http_request_duration_seconds', 'HTTP request duration')
active_connections = Gauge('active_connections', 'Active connections')

@app.before_request
def before_request():
    request.start_time = time.time()

@app.after_request
def after_request(response):
    duration = time.time() - request.start_time
    request_count.inc()
    request_duration.observe(duration)
    return response
```

### Performance SLAs

**Service Level Objectives**:

| SLA | Target | Monitor |
|-----|--------|---------|
| Availability | 99.5% | Uptime |
| Error Rate | < 0.5% | Error tracking |
| API Latency (p95) | < 500ms | APM |
| Database Latency (p95) | < 50ms | Query logs |
| Cache Hit Rate | > 85% | Redis metrics |

## Optimization Checklist

- [ ] Database indexes created
- [ ] Slow queries identified and optimized
- [ ] Connection pooling configured
- [ ] Redis caching implemented
- [ ] Cache warming on startup
- [ ] Cache invalidation strategy
- [ ] API response compression enabled
- [ ] Pagination implemented
- [ ] Rate limiting configured
- [ ] SignalR batching enabled
- [ ] Model inference batching
- [ ] GPU memory management
- [ ] Container resource limits set
- [ ] Auto-scaling configured
- [ ] Health checks implemented
- [ ] Monitoring and alerting setup
- [ ] Load tests passing (100+ users)
- [ ] Stress tests passing (500+ users)
- [ ] Soak tests passing (8+ hours)

## Common Bottlenecks

| Issue | Symptoms | Solution |
|-------|----------|----------|
| N+1 Queries | High DB CPU | Use JOINs, eager loading |
| Missing Indexes | Slow queries | Analyze and add indexes |
| Low Cache Hit Rate | High DB load | Increase TTL, cache more |
| Connection Pool Exhaustion | Timeouts | Increase pool size |
| Memory Leak | Increasing memory | Profile and fix leaks |
| CPU Saturation | High latency | Add more servers |
| Network Latency | Slow API | Check network, CDN |

## Performance Testing Workflow

1. **Establish Baseline**
   - Run basic load test
   - Document current metrics

2. **Identify Bottlenecks**
   - Profile CPU and memory
   - Analyze slow queries
   - Check cache hit rates

3. **Implement Optimizations**
   - Add missing indexes
   - Implement caching
   - Optimize queries

4. **Validate Improvements**
   - Run load tests again
   - Compare to baseline
   - Verify SLAs met

5. **Monitor Production**
   - Set up alerts
   - Track metrics continuously
   - Plan next optimization

## Resources

- [k6 Documentation](https://k6.io/docs/)
- [PostgreSQL Optimization](https://www.postgresql.org/docs/)
- [Redis Performance](https://redis.io/topics/optimization)
- [Prometheus Monitoring](https://prometheus.io/docs/)
- [Flask Performance](https://flask.palletsprojects.com/en/2.3.x/)

## See Also

- [Getting Started](GETTING_STARTED.md)
- [Troubleshooting](TROUBLESHOOTING.md)
- [Deployment Guide](../IMPLEMENTATION_ROADMAP.md)
