# Incident Reporting System

[![.NET](https://img.shields.io/badge/.NET-8.0-blueviolet)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Docker](https://img.shields.io/badge/Docker-Containerized-blue)](https://www.docker.com)
[![Microservices](https://img.shields.io/badge/Architecture-Microservices-brightgreen)](https://microservices.io)
[![Consul](https://img.shields.io/badge/Service%20Discovery-Consul-orange)](https://www.consul.io)
[![Ocelot](https://img.shields.io/badge/API%20Gateway-Ocelot-lightgrey)](https://ocelot.readthedocs.io)
[![CI/CD](https://img.shields.io/badge/CI%2FCD-GitHub%20Actions-success)](https://github.com/features/actions)

A full-stack, microservices-based incident reporting platform built with modern cloud-native principles, containerization, service discovery, centralized configuration, API gateway routing, and automated CI/CD.

---

## Project Overview

The **Incident Reporting System** enables citizens to report real-world incidents (traffic problems, public order violations, communal issues) using an interactive map, detailed descriptions, categories, and images.

Reports are stored in a **pending** state until reviewed and approved by moderators. Only approved incidents become publicly visible on a filtered map view.

The system is architected as a set of independent, loosely coupled microservices communicating exclusively through a single API Gateway.

---

## Key Features

### Core User Functionality

* Interactive map (Leaflet.js) for selecting incident location
* Automatic geolocation or manual map click
* Incident categorization (type and subtype)
* Rich description input
* Image upload with client-side preview
* Reports submitted in pending status

---

### Public Anonymous Access

* View approved incidents on a public map
* Filtering by:

  * Time range (today, last week, last month, custom)
  * Incident type
* Popup details with description, timestamp, type, and image

---

### Moderator Dashboard

* List view of pending incidents
* Map view for spatial context
* Approve or reject actions
* Protected routes for Moderator role only

---

### Authentication & Authorization

* JWT-based authentication
* Role-based access control:

  * **User** — report incidents, view approved ones
  * **Moderator** — moderate pending reports
* Automatic role-based redirection after login
* Protected endpoints using JWT Bearer authentication

---

## Advanced Architecture Features

* **Service Discovery** with HashiCorp Consul
* **API Gateway** using Ocelot
* **Centralized Configuration** via Steeltoe Config Server (Git backend)
* **Containerization** with Docker and docker-compose
* **CI/CD** pipeline using GitHub Actions
* **Health Checks** for all services
* **Persistent Storage**

  * PostgreSQL for data
  * Docker volume for images
* **Static File Serving** for incident images

---

## Technology Stack

### Backend

* .NET 8 (ASP.NET Core Web API)
* Entity Framework Core
* PostgreSQL 16
* Ocelot API Gateway
* Steeltoe Config Server & Actuators
* HashiCorp Consul
* JWT Bearer Authentication
* Swagger / OpenAPI

### Frontend

* Blazor WebAssembly
* Leaflet.js
* Bootstrap 5 + custom CSS

### DevOps & Infrastructure

* Docker & Docker Compose
* GitHub Actions
* PostgreSQL
* Consul

---

## System Architecture Overview

1. **Frontend (Blazor WebAssembly)**

   * Runs in the browser
   * Communicates only with API Gateway (`http://localhost:5000`)
   * Uses JWT tokens stored in local storage

2. **API Gateway (Ocelot)**

   * Single public entry point
   * Dynamic routing using Consul
   * Authentication and authorization
   * Static image routing (`/images/incidents/{filename}`)

3. **Microservices**

   * **UserService** — authentication, JWT, roles
   * **IncidentService** — incidents, images, public endpoints
   * **ModerationService** — pending list, approve/reject
   * **ConfigServer** — centralized configuration

4. **Service Discovery & Configuration**

   * Services register in Consul on startup
   * Ocelot resolves services dynamically
   * Configuration pulled from Git via ConfigServer

5. **Image Handling**

   * Stored in `wwwroot/images/incidents`
   * Persisted using Docker volume `incident_images`
   * Served publicly through API Gateway

6. **Database**

   * PostgreSQL container
   * EF Core migrations applied on startup

---

## Project Structure

```
Incident-Reporting-System/
├── ApiGateway/
├── ConfigServer/
├── IncidentService/
│   └── wwwroot/images/incidents/
├── ModerationService/
├── UserService/
├── IncidentFrontend/
├── .github/workflows/
├── docker-compose.yml
└── README.md
```

---

## Setup & Running Locally

### Prerequisites

* Docker Desktop (with Docker Compose)
* Git
* Web browser

---

### Quick Start

1. Clone the repository

```bash
git clone https://github.com/markopreradovic/Incident-Reporting-System.git
cd Incident-Reporting-System
```

2. Start the system

```bash
docker compose up --build
```

3. Wait approximately 60 seconds for all services to become healthy.

---

## Access Points

* Frontend: [http://localhost:7274](http://localhost:7274)
* API Gateway: [http://localhost:5000](http://localhost:5000)
* API Swagger: [http://localhost:5000/swagger](http://localhost:5000/swagger)
* Consul UI: [http://localhost:8500](http://localhost:8500)
* Config Server health: [http://localhost:8888/actuator/health](http://localhost:8888/actuator/health)
* PostgreSQL: localhost:5433

---

## Default Credentials

**Moderator**

* Username: `moderator`
* Password: `moderator1`

**Regular User**

* Username: `string1`
* Password: `string123`

---

## Configuration Management

All configurations (connection strings, JWT secrets, logging levels) are stored in a Git repository and served via ConfigServer at runtime.

Configuration changes do not require rebuilding or restarting containers.

---

## Image Storage & Serving

* Upload endpoint: `/api/incidents/upload-image`
* Stored in `IncidentService/wwwroot/images/incidents`
* Persisted via Docker volume
* Public access:

```
http://localhost:5000/images/incidents/{filename}
```

---

## CI/CD Pipeline

GitHub Actions automatically:

* Builds all services on push to `main`
* Creates Docker images
* Pushes images to Docker Hub:

  ```
  markopreradovic/incidentreportingsystem-*
  ```

---




