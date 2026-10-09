# Loki Deployment & Operations Guide

## Table of Contents
1. [Deployment Options](#deployment-options)
2. [Local Development](#local-development)
3. [Production Setup](#production-setup)
4. [Monitoring Loki](#monitoring-loki)
5. [Troubleshooting](#troubleshooting)
6. [Performance Tuning](#performance-tuning)

---

## Deployment Options

### Option 1: Docker Compose (Development) ⭐ Recommended for Local
```yaml
# docker-compose.yml
loki:
  image: grafana/loki:latest
  ports:
	- "3100:3100"
  volumes:
	- ./loki-config.yml:/etc/loki/local-config.yml
	- loki_data:/loki
```

**Pros:**
- Simple, single command startup
- Perfect for local development
- All services in sync
- Easy to tear down

**Cons:**
- Not suitable for production
- No high availability
- Single instance only

### Option 2: Kubernetes (Production) 
```yaml
# Deploy via Helm or kubectl
helm repo add grafana https://grafana.github.io/helm-charts
helm install loki grafana/loki-stack
```

### Option 3: Cloud-Managed (Grafana Cloud)
- Pre-configured Loki tenant
- Automatically scaled
- No infrastructure management
- Monthly subscription

---

## Local Development Setup

### 1. Start Stack
```powershell
docker-compose up -d
```

Expected output:
```
[+] Running 5/5
  ✓ observability-postgres   
  ✓ observability-prometheus
  ✓ observability-loki       
  ✓ observability-grafana    
```

### 2. Verify Services
```powershell
# Check all containers running
docker ps

# Check Loki health
curl http://localhost:3100/ready
# Should return: "ready"

# Check Loki endpoints
curl http://localhost:3100/metrics | head -20
```

### 3. Confirm Log Ingestion
```powershell
# Make any API request
curl https://localhost:7000/api/orders -k

# Query Loki for logs
curl "http://localhost:3100/loki/api/v1/query" \
  --data-urlencode 'query={app="observability-app"}'
```

---

## Production Setup

### Prerequisites
- .NET 10 runtime
- PostgreSQL 13+
- Docker/container runtime
- 2GB+ free disk space

### Configuration

**1. Update loki-config.yml for Production:**
```yaml
auth_enabled: true  # Require API token

ingester:
  chunk_idle_period: 1m
  max_chunk_age: 30m

limits_config:
  ingestion_rate_mb: 50      # Adjust for your volume
  ingestion_burst_size_mb: 100

storage_config:
  boltdb_shipper:
	shared_store: s3         # Use S3, GCS, or Azure Blob
	cache_location: /loki/cache
```

**2. Update Program.cs for Production:**
```csharp
// Production settings
Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Async(LogEventLevel.Warning)  // Log less in prod
	.Enrich.With(new TraceIdEnricher(...))
	.Enrich.FromLogContext()
	.WriteTo.Async(a => a.Http(
		requestUri: lokiUri,
		queueLimitBytes: 1_000_000,              // Increase queue
		textFormatter: new LokiHttpFormatProvider()
	))
	.CreateLogger();
```

**3. Docker Compose for Production:**
```yaml
loki:
  image: grafana/loki:3.0       # Use specific version
  restart: unless-stopped
  volumes:
	- loki_data:/loki
	- ./loki-config.yml:/etc/loki/local-config.yml
  environment:
	- LOKI_STORAGE_CONFIG_S3_ENDPOINT=s3.aws.com
	- LOKI_STORAGE_CONFIG_S3_BUCKET=my-loki-bucket
  healthcheck:
	test: ["CMD", "curl", "-f", "http://localhost:3100/ready"]
	interval: 30s
	timeout: 10s
	retries: 3
```

---

## Monitoring Loki

### Health Checks

**Loki Readiness:**
```bash
curl http://localhost:3100/ready
# Returns: "ready" if healthy
```

**Loki Liviness:**
```bash
curl http://localhost:3100/metrics | grep loki_up
# Should return: loki_up 1
```

### Key Metrics to Monitor

```
# Ingestion rate
rate(loki_distributor_messages_received_total[5m])

# Ingestion errors
rate(loki_distributor_errors_total[5m])

# Query performance
loki_querier_request_duration_seconds

# Index size
loki_index_db_size_bytes

# Chunk compression ratio
loki_chunk_compression_ratio
```

### Grafana Alerting

**Example Alert: High Error Rate**
```yaml
alert: LokiHighErrorRate
expr: |
  (rate(loki_distributor_errors_total[5m]) / 
   rate(loki_distributor_messages_received_total[5m])) > 0.01
for: 5m
annotations:
  summary: "Loki having >1% error rate"
```

**Example Alert: Disk Space**
```yaml
alert: LokiLowDiskSpace
expr: |
  (disk_used_percent{path="/loki"}) > 80
for: 10m
annotations:
  summary: "Loki disk usage above 80%"
```

---

## Troubleshooting

### Logs Not Appearing in Loki

**Step 1: Verify Application Logging**
```powershell
# Check console output
dotnet run
# Should show: [HH:MM:SS INF] messages

# Check application logs contain trace ID
# Should see: TraceId: xxxxx
```

**Step 2: Check Loki Connectivity**
```powershell
# Test direct HTTP connection to Loki
$result = curl.exe "http://localhost:3100/ready"
if ($result -eq "ready") { Write-Host "✓ Loki is ready" }
else { Write-Host "✗ Loki is not ready" }
```

**Step 3: Check Loki Receiving Logs**
```bash
# View Loki container logs
docker logs observability-loki | grep -E "POST|received|ingestion"

# Should show incoming requests to /api/v1/push
```

**Step 4: Verify Label Configuration**
```bash
# Query available labels in Loki
curl "http://localhost:3100/loki/api/v1/labels"

# Should return:
# ["__name__", "app", "environment", "level", ...]
```

### High Memory Usage

**Check Container Memory:**
```powershell
docker stats observability-loki --no-stream
```

**Solutions:**
1. Reduce chunk retention period
2. Lower ingestion rate limits
3. Enable log sampling
4. Use async batching

### Slow Queries

**Identify Slow Queries:**
```bash
# Check Loki logs for slow queries
docker logs observability-loki | grep -E "slow|took|duration"
```

**Optimize:**
1. Always filter by labels first
2. Reduce time range
3. Use regex sparingly
4. Add index tags for common fields

### Disk Space Issues

**Check Usage:**
```powershell
docker exec observability-loki du -sh /loki/*
```

**Solutions:**
1. Configure retention policy
2. Compress older chunks
3. Export and delete old logs
4. Increase storage volume

---

## Performance Tuning

### For Small Volume (<10GB/day)

**Program.cs:**
```csharp
.WriteTo.Http(
	requestUri: lokiUri,
	queueLimitBytes: 100_000,              // Smaller queue
	textFormatter: new LokiHttpFormatProvider()
)

// Batch settings
Period = TimeSpan.FromSeconds(5)           // Send every 5s
PostMinimumLogEventBatchSize = 10          // Send when 10 logs ready
```

**loki-config.yml:**
```yaml
limits_config:
  ingestion_rate_mb: 3
  ingestion_burst_size_mb: 5

chunk_store_config:
  max_look_back_period: 168h               # 7 days max query
```

### For Medium Volume (10-100GB/day)

**Program.cs:**
```csharp
.WriteTo.Async(a => a.Http(
	requestUri: lokiUri,
	queueLimitBytes: 500_000,
	textFormatter: new LokiHttpFormatProvider()
))

Period = TimeSpan.FromSeconds(10)
PostMinimumLogEventBatchSize = 50
```

**loki-config.yml:**
```yaml
limits_config:
  ingestion_rate_mb: 20
  ingestion_burst_size_mb: 40
```

### For High Volume (>100GB/day)

**Recommendations:**
1. Use Kubernetes cluster for auto-scaling
2. Implement log sampling
3. Separate hot/cold storage tiers
4. Use cloud-managed Loki (Grafana Cloud)
5. Implement log filtering at source

```csharp
// Sample logs: only log 10% of INFO level
.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)

// Filter before sending to Loki
.Filter.ByIncludingOnly(le =>
	le.Level >= LogEventLevel.Warning ||  // Always log warnings+
	(le.Level == LogEventLevel.Information && Random.Next(10) == 0)  // 10% of info
)
```

---

## Backup & Recovery

### Backup Loki Data

**Docker Volume Backup:**
```bash
# Backup Loki volume
docker run --rm \
  -v observability_loki_data:/loki \
  -v $(pwd):/backup \
  alpine tar czf /backup/loki-backup.tar.gz -C / loki

# Restore
docker run --rm \
  -v observability_loki_data:/loki \
  -v $(pwd):/backup \
  alpine tar xzf /backup/loki-backup.tar.gz -C /
```

**Cloud Storage (S3/GCS):**
```bash
# Configure in loki-config.yml
storage_config:
  boltdb_shipper:
	shared_store: s3
	s3: s3://my-bucket/loki

# Automated daily backup
aws s3 sync /loki s3://my-bucket/loki/daily-$(date +%Y%m%d)
```

---

## Upgrade & Maintenance

### Upgrading Loki

**Steps:**
1. Stop current Loki container
2. Backup data volume
3. Update image version
4. Restart container
5. Verify logs are accessible

```powershell
# Update docker-compose.yml
# Change: image: grafana/loki:latest
# To: image: grafana/loki:3.0.0  # Specific version

# Restart
docker-compose down
docker volume ls | findstr loki  # Verify volume (data persists)
docker-compose up -d
docker logs -f observability-loki
```

### Regular Maintenance

**Daily:**
- Monitor disk usage
- Check error rates

**Weekly:**
- Review query performance
- Audit access logs
- Check backup status

**Monthly:**
- Test recovery procedures
- Review retention policies
- Optimize indexes
- Update dependencies

---

## Grafana Setup for Loki

### Auto-Provisioned (Via Docker Compose)
Already configured! Loki datasource available at http://localhost:3000

### Manual Setup
1. Grafana → Data Sources → Add Data Source
2. Type: Loki
3. URL: http://loki:3100
4. Save & Test

### Creating Dashboards

**Log Volume Dashboard:**
```
Query 1: sum(rate({app="observability-app"}[5m]))
Type: Graph
Title: "Log Volume (logs/sec)"

Query 2: sum by (level) (rate({app="observability-app"}[5m]))
Type: Graph
Title: "Logs by Level"
```

**Error Analysis Dashboard:**
```
Query: {app="observability-app", level="error"} | json | level="error"
Type: Logs
Title: "Error Logs"
```

---

## Security Considerations

### Authentication
```yaml
# loki-config.yml
auth_enabled: true

# Set API key
http_config:
  bearer_token: "your-secret-token"
```

### Network Security
- Restrict Loki to internal network only
- Use reverse proxy for public access
- Enable TLS/SSL

```yaml
server:
  http_listen_port: 3100
  http_tls_cert_file: /etc/loki/tls/cert.pem
  http_tls_key_file: /etc/loki/tls/key.pem
```

### Data Privacy
- Don't log sensitive data (passwords, API keys, PII)
- Use log filtering/masking
- Configure retention policies
- Enable audit logging

---

## Cost Optimization

### How Loki Saves Money
- Doesn't index log contents (unlike ELK)
- Labels-based indexing is efficient
- Compression ratio ~1:10
- Less disk needed than alternatives

### Optimization Tips
1. Use appropriate log levels (WARNING for production)
2. Implement sampling for high-volume services
3. Set retention policies
4. Use local storage for dev, cloud for prod
5. Archive old logs to S3/GCS

### Cost Comparison
| Solution | Monthly Cost | Notes |
|----------|--------------|-------|
| Loki (self-hosted) | $50-200 | Infrastructure costs |
| Loki (Grafana Cloud) | $50-200 | Pay-as-you-go |
| ELK Stack | $200-1000 | Memory intensive |
| Splunk | $500-5000+ | Enterprise pricing |

---

## Compliance & Audit

### Audit Logging
```csharp
// Log all API requests
.Enrich.WithProperty("RequestPath", context.Request.Path)
.Enrich.WithProperty("RequestMethod", context.Request.Method)
.Enrich.WithProperty("UserId", context.User?.Identity?.Name)
```

### Retention Policy
```yaml
# loki-config.yml - Set retention
schema_config:
  configs:
	- from: 2024-01-01
	  store: boltdb-shipper
	  retention_period: 2160h  # 90 days
```

### Data Export
```bash
# Export logs to CSV
curl "http://localhost:3100/loki/api/v1/query_range?query={app=%22observability-app%22}" \
  | jq -r '.data.result[0].values[] | @csv' \
  > logs_export.csv
```

---

## FAQ

**Q: How much disk space does Loki need?**
A: ~10-20% of raw log volume after compression. 100GB/day → ~2-4GB stored.

**Q: Can I query logs across multiple instances?**
A: Yes, use label filters or distributed setup with shared storage.

**Q: How do I delete logs?**
A: Loki doesn't support selective deletion. Use retention policies or export before deletion.

**Q: Is Loki suitable for production?**
A: Yes, it's production-ready. Many companies use it at scale.

**Q: What's better: Loki or ELK?**
A: Loki is simpler, cheaper, cloud-native. ELK is more powerful but complex.

---

## Support Resources

- **Loki Documentation**: https://grafana.com/docs/loki/latest/
- **Grafana Community**: https://community.grafana.com/
- **GitHub Issues**: https://github.com/grafana/loki/issues
- **Slack Community**: https://slack.grafana.com

---

## Summary

Loki provides:
✅ Lightweight log aggregation  
✅ Label-based efficient querying  
✅ Cost-effective storage  
✅ Seamless Grafana integration  
✅ Production-ready reliability  

**Your complete observability stack is ready!** 🚀
