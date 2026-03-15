# Codveda Technologies — .NET Development Internship
**Intern:** Gildas Pacifique Niyonkuru | **ID:** CV/A1/61579

---

## 📁 Project Structure

```
codveda-internship/
├── level1/
│   ├── task2_oop/          C# OOP — HR Employee System
│   └── task3_exceptions/   Exception Handling — Bank System
├── level2/
│   ├── task1_console/      Console Task Manager App
│   ├── task2_aspnet/       ASP.NET Core Blog Platform (MVC + API)
│   └── task3_auth/         JWT Authentication & Authorization
├── level3/
│   ├── task1_microservices/ Microservices + RabbitMQ + Docker
│   ├── task2_async/         Async Programming & Multithreading
│   └── task3_azure/         Azure Cloud Integration
└── kubernetes/
    ├── src/                 K8s-ready .NET 8 API
    ├── manifests/           Deployment, Service, HPA, ConfigMap, Secret
    └── KUBERNETES_STUDY_GUIDE.sh
```

## 🚀 Quick Start

### Level 3 Task 1 — Microservices
```bash
cd level3/task1_microservices
docker compose up --build
# Test: curl -X POST http://localhost:5000/api/orders -H "Content-Type: application/json" -d '{"productId":"P001","quantity":2}'
```

### Kubernetes
```bash
minikube start
kubectl apply -f kubernetes/manifests/
kubectl get pods -n codveda
```

## 🛠 Tech Stack
.NET 8 | ASP.NET Core | EF Core | Docker | Kubernetes | RabbitMQ | Azure | JWT | CI/CD

---
#CodvedaJourney #CodvedaExperience #FutureWithCodveda
