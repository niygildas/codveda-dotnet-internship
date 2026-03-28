# Codveda Technologies — .NET Development Internship

**Intern:** Gildas Pacifique Niyonkuru
**ID:** CV/A1/61579
**Role:** .NET Development Intern
**Organization:** Codveda Technologies
**Period:** March 2026

---

## 📹 Video Demos (LinkedIn)

| Task | Video |
|------|-------|
| Level 1 Task 2 — C# OOP | [Watch on LinkedIn](https://www.linkedin.com/in/gildas-niyonkuru-488480325) |
| Level 1 Task 3 — Exception Handling | [Watch on LinkedIn](https://www.linkedin.com/in/gildas-niyonkuru-488480325) |
| Level 2 Task 1 — Console App | [Watch on LinkedIn](https://www.linkedin.com/in/gildas-niyonkuru-488480325) |
| Level 2 Task 2 — ASP.NET Core | [Watch on LinkedIn](https://www.linkedin.com/in/gildas-niyonkuru-488480325) |
| Level 2 Task 3 — JWT Auth | [Watch on LinkedIn](https://www.linkedin.com/in/gildas-niyonkuru-488480325) |
| Level 3 Task 1 — Microservices | [Watch on LinkedIn](https://www.linkedin.com/in/gildas-niyonkuru-488480325) |
| Level 3 Task 2 — Async Programming | [Watch on LinkedIn](https://www.linkedin.com/in/gildas-niyonkuru-488480325) |
| Level 3 Task 3 — Azure Cloud | [Watch on LinkedIn](https://www.linkedin.com/in/gildas-niyonkuru-488480325) |
| Bonus — Kubernetes | [Watch on LinkedIn](https://www.linkedin.com/in/gildas-niyonkuru-488480325) |

---

## 📁 Project Structure
```
codveda-dotnet-internship/
├── level1/
│   ├── task2_oop/              C# OOP — HR Employee System
│   └── task3_exceptions/       Exception Handling — Bank System
├── level2/
│   ├── task1_console/          Console Task Manager App
│   ├── task2_aspnet/           ASP.NET Core Blog Platform
│   └── task3_auth/             JWT Authentication & Authorization
├── level3/
│   ├── task1_microservices/    Microservices + RabbitMQ + Docker
│   ├── task2_async/            Async Programming & Multithreading
│   └── task3_azure/            Azure Cloud Integration
└── kubernetes/
    ├── src/                    K8s-ready .NET 8 Products API
    ├── manifests/              Deployment, Service, HPA, ConfigMap, Secret
    └── KUBERNETES_STUDY_GUIDE.sh
```

---

## 🚀 Quick Start

### Level 1 — OOP
```bash
cd level1/task2_oop
dotnet run
```

### Level 1 — Exception Handling
```bash
cd level1/task3_exceptions
dotnet run
```

### Level 2 — Console App
```bash
cd level2/task1_console
dotnet run
```

### Level 2 — ASP.NET Core Blog
```bash
cd level2/task2_aspnet
dotnet run
# Open: http://localhost:5000/swagger
```

### Level 2 — JWT Authentication
```bash
cd level2/task3_auth
dotnet run
# Open: http://localhost:5000/swagger
# Test: Register → Login → Copy token → Authorize → Call protected endpoints
```

### Level 3 — Microservices
```bash
cd level3/task1_microservices
docker compose up --build
# API Gateway:  http://localhost:5000
# RabbitMQ UI:  http://localhost:15672 (guest/guest)
# Test order:
curl -X POST http://localhost:5000/api/orders \
     -H "Content-Type: application/json" \
     -d '{"productId":"PROD-001","quantity":3}'
```

### Level 3 — Async Programming
```bash
cd level3/task2_async
dotnet run
```

### Level 3 — Azure Cloud
```bash
cd level3/task3_azure
dotnet run
# Open: http://localhost:5000/swagger
```

### Kubernetes
```bash
cd kubernetes/src
dotnet run
# Open: http://localhost:5000/swagger
# Health: http://localhost:5000/health/live
# Health: http://localhost:5000/health/ready
```

---

## 🛠️ Tech Stack

| Area | Technologies |
|------|-------------|
| Language | C# / .NET 8 |
| Web Framework | ASP.NET Core 8 |
| Database | Entity Framework Core + SQLite |
| API Gateway | YARP (Microsoft.ReverseProxy) |
| Messaging | RabbitMQ |
| Containers | Docker + Docker Compose |
| Cloud | Microsoft Azure |
| Serverless | Azure Functions v4 |
| Storage | Azure Blob Storage |
| CI/CD | Azure DevOps Pipelines |
| Auth | JWT + Refresh Tokens |
| Orchestration | Kubernetes |
| Logging | Serilog |

---

## 🌐 Connect

- **LinkedIn:** [Gildas Niyonkuru](https://www.linkedin.com/in/gildas-niyonkuru-488480325)
- **GitHub:** [niygildas](https://github.com/niygildas)
- **Email:** niygildas@gmail.com

---

#CodvedaJourney #CodvedaExperience #FutureWithCodveda #CodvedaAchievements