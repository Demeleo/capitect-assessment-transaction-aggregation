# 🏦 Transaction Aggregator

The **Transaction Aggregator** is a .NET 9 application built using **Clean Architecture** principles.  
It aggregates financial transactions from multiple vendors.
I used WireMock to simulate a server for each vendor.

---

## 🚀 Overview

This application was designed to:
- Simulate multiple financial vendors providing customer transaction data.
- Aggregate and store these transactions in a SQL Server database.
- Provide **observability** with **OpenTelemetry** (traces, logs, and metrics).
- Expose a REST API that returns aggregated customer transaction data.
- Handle errors consistently through custom exceptions and ProblemDetails middleware.

---

## 🧱 Architecture

The solution follows a **Clean Architecture** structure:

| Layer | Description |
|-------|--------------|
| **Domain** | Contains core business logic, entities, and domain-specific rules. No external dependencies. |
| **Application** | Defines interfaces, data transfer objects (DTOs), and service contracts used across the system. |
| **Infrastructure** | Contains EF Core configuration, data repositories, vendor clients, and WireMock-based vendor simulations. |
| **API** | ASP.NET Core web project — the entry point exposing endpoints, exception handling, and telemetry setup. |

---

## 🧰 Technologies Used

- **.NET 9 / ASP.NET Core Web API**
- **Entity Framework Core 9** for data persistence
- **SQL Server (Docker container)**
- **WireMock.Net** for vendor API simulation
- **Polly** for HTTP retry and timeout resilience
- **OpenTelemetry** for tracing, logging, and metrics
- **Bogus** for generating realistic mock data

---

## 🧪 Features

- ✅ Clean, testable, and modular architecture  
- ✅ Realistic vendor data generation and simulation  
- ✅ Centralized exception handling with RFC 7807 `ProblemDetails` responses  
- ✅ OpenTelemetry tracing and metrics for observability  
- ✅ Resilient vendor communication via `HttpClient` with retry/timeout policies  
- ✅ Fully containerized setup using Docker and Docker Compose  

---

## 🧩 Running the Application

### 1. Prerequisites

Make sure you have installed:
- [Docker Desktop](https://www.docker.com/products/docker-desktop) or similar
- [.NET 9 SDK](https://dotnet.microsoft.com/)

---

### 2. Running with Docker Compose

The solution includes a ready-to-use `docker-compose.yml` that spins up both SQL Server and the API.

From the solution root, run:

```bash
docker compose up --build -d
```

The api is exposed using swagger and should be available on http://localhost:5000/swagger/index.html





