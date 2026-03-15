# Codveda Technologies — .NET Development Internship

**Intern:** Gildas Pacifique Niyonkuru
**ID:** CV/A1/61579
**Role:** .NET Development Intern

---

## Level 3 — Advanced Tasks

### Task 1: Microservices Architecture with .NET
- API Gateway using YARP
- Order Service with RabbitMQ event publishing
- Notification Service consuming RabbitMQ events
- Docker Compose for full stack orchestration

**Run:**
```bash
cd level3/task1_microservices
docker compose up --build
```

**Test:**
```bash
curl -X POST http://localhost:5000/api/orders \
     -H "Content-Type: application/json" \
     -d '{"productId":"PROD-001","quantity":3}'
```

### Task 3: Cloud Integration with Azure
- Azure Blob Storage service
- Azure Functions (HTTP + Timer triggers)
- ASP.NET Core with Azure DI
- Azure DevOps CI/CD pipeline (Blue-Green deployment)

---
#CodvedaJourney #CodvedaExperience #FutureWithCodveda
