# Incident Reporting System: A Microservices-Based Platform for Real-World Incident Management

[![.NET](https://img.shields.io/badge/.NET-8.0-blueviolet)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Docker](https://img.shields.io/badge/Docker-Containerized-blue)](https://www.docker.com)
[![Microservices](https://img.shields.io/badge/Architecture-Microservices-brightgreen)](https://microservices.io)
[![Consul](https://img.shields.io/badge/Service%20Discovery-Consul-orange)](https://www.consul.io)
[![Ocelot](https://img.shields.io/badge/API%20Gateway-Ocelot-lightgrey)](https://ocelot.readthedocs.io)
[![CI/CD](https://img.shields.io/badge/CI%2FCD-GitHub%20Actions-success)](https://github.com/features/actions)

A full-stack, microservices-based incident reporting platform engineered with modern cloud-native principles, featuring containerization, dynamic service discovery, centralized configuration management, robust API gateway routing, and an automated Continuous Integration/Continuous Deployment (CI/CD) pipeline.

---

## 1. Executive Summary and Project Overview

The **Incident Reporting System** is designed to provide a comprehensive digital solution for citizens to report real-world incidents, such as traffic issues, public order violations, or communal problems, using an intuitive interface. The platform facilitates the submission of detailed reports, including location data via an interactive map, categorical information, rich descriptions, and image uploads.

A core principle of the system is the controlled dissemination of information. All submitted reports are initially held in a **pending** state, requiring review and approval by designated moderators. Only incidents that have been formally approved become publicly visible on the filtered map view, ensuring data quality and relevance for the general public.

The system's architecture is fundamentally based on a set of independent, loosely coupled microservices. This design choice enhances scalability, maintainability, and fault isolation. All external communication is strictly managed through a single, unified API Gateway.

---

## 2. System Architecture and Microservices Breakdown

The platform adheres to a microservices architectural pattern, where each service is responsible for a distinct business capability. This structure allows for independent deployment and technology selection for each component.

### 2.1. Microservices Overview

The system comprises five primary microservices, each registered with the Consul service discovery agent and exposed via the Ocelot API Gateway.

| Microservice | Primary Functionality | Key Technologies |
| :--- | :--- | :--- |
| **UserService** | User authentication, authorization (RBAC), user management, and role assignment. | .NET 8, JWT, PostgreSQL |
| **IncidentService** | Incident submission, storage, image handling, and public incident retrieval. | .NET 8, EF Core, PostgreSQL |
| **ModerationService** | Management of pending incidents, approval/rejection logic. | .NET 8, EF Core, PostgreSQL |
| **AnalyticsService** | Data analysis and visualization of incident metrics. | .NET 8, Data Visualization Libraries |
| **ConfigServer** | Centralized configuration management using a Git backend. | Steeltoe Config Server |

### 2.2. Data Flow and Infrastructure

1.  **Frontend (Blazor WebAssembly)**: Runs client-side, communicating exclusively with the API Gateway. It includes the **Admin Page** for user management.
2.  **API Gateway (Ocelot)**: Acts as the single entry point, handling request routing, authentication, and authorization enforcement based on dynamic service resolution via Consul.
3.  **Service Discovery (Consul)**: All microservices register themselves upon startup, enabling the API Gateway to dynamically locate and route requests without hardcoding service addresses.
4.  **Persistent Storage**: PostgreSQL is used for structured data storage across all services, while a Docker volume (`incident_images`) ensures persistence for uploaded image files.

---

## 3. Advanced Features and Capabilities

The system incorporates several advanced features to ensure robust security, comprehensive data analysis, and efficient administration.

### 3.1. Incident Analytics Service 

The newly introduced `analytics-service` is dedicated to providing fundamental data analysis and visualization capabilities for incident data. This service processes incident records to generate actionable insights, supporting data-driven decision-making for administrators and moderators.

The service provides analysis across three key dimensions:

*   **Temporal Analysis (Time)**: Visualization of incident reporting trends, including daily, weekly, and monthly submission rates. This helps identify peak reporting periods and temporal patterns.
*   **Geospatial Analysis (Location)**: Mapping the geographical distribution of incidents. This includes generating heatmaps or cluster visualizations to highlight high-incidence areas.
*   **Categorical Analysis (Type)**: Breakdown of incidents by category and subcategory, allowing for a clear understanding of the most prevalent types of reported issues.

Access to the analytics dashboard is restricted to users with the **Admin** and **Moderator** roles.

### 3.2. Advanced Authentication and Authorization 

The system employs a robust, JWT-based authentication mechanism coupled with an advanced Role-Based Access Control (RBAC) model, managed by the `UserService`. This expansion allows for granular control over system access.

#### Role Definitions

The system defines three distinct user roles, each with specific responsibilities and access privileges:

| Role | Primary Responsibilities | Access Privileges |
| :--- | :--- | :--- |
| **Admin** | Full system management, user administration, analytics access, and moderation. | All endpoints, including user creation/deletion and role modification. |
| **Moderator** | Incident review and approval/rejection, analytics access. | Moderation endpoints and read-only access to analytics data. |
| **User** | Incident reporting and viewing of approved public incidents. | Incident submission endpoints and public read endpoints. |

#### Endpoint Access Control

The `UserService` is responsible for defining and managing the mapping between user roles and access rights to individual API endpoints. This ensures that sensitive operations, such as user deletion or role changes, are strictly limited to the **Admin** role, thereby enforcing the principle of least privilege.

### 3.3. Admin Dashboard

A dedicated section within the Frontend application, accessible only to the **Admin** role, provides essential administrative tools for user lifecycle management:

*   **User Listing**: Display of all registered users within the system.
*   **Role Modification**: Capability to change the role of any existing user (e.g., changing a 'User' to a 'Moderator' or 'Admin').
*   **User Deletion**: Functionality to permanently remove user accounts from the system.

---

## 4. Technology Stack

The platform leverages a modern, open-source technology stack, focusing on performance, scalability, and developer productivity.

| Component | Technology | Purpose |
| :--- | :--- | :--- |
| **Backend Framework** | .NET 8 (ASP.NET Core Web API) | Core application logic and microservices development. |
| **Database** | PostgreSQL 16 | Primary relational data store. |
| **ORM** | Entity Framework Core | Object-Relational Mapping for data access. |
| **API Gateway** | Ocelot | Request routing, authentication, and service aggregation. |
| **Service Discovery** | HashiCorp Consul | Dynamic service registration and lookup. |
| **Configuration** | Steeltoe Config Server | Centralized configuration management (Git-backed). |
| **Frontend** | Blazor WebAssembly | Client-side web application framework. |
| **Mapping** | Leaflet.js | Interactive map visualization and location selection. |
| **Containerization** | Docker & Docker Compose | Environment packaging and orchestration. |
| **CI/CD** | GitHub Actions | Automated build, test, and deployment pipeline. |

---

## 5. Project Structure

The repository is organized to clearly separate the concerns of each microservice and infrastructure component. The new `AnalyticsService` is integrated at the root level.

```
Incident-Reporting-System/
├── ApiGateway/
├── AnalyticsService/  <-- NEW MICROSERVICE
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

## 6. Setup and Running Locally

### 6.1. Prerequisites

*   Docker Desktop (with Docker Compose)
*   Git
*   Web browser

### 6.2. Quick Start

The entire system can be launched using a single command, leveraging Docker Compose to build and orchestrate all microservices, the database, and infrastructure components.

1.  Clone the repository:

```bash
git clone https://github.com/markopreradovic/Incident-Reporting-System.git
cd Incident-Reporting-System
```

2.  Start the system:

```bash
docker compose up --build
```

3.  Allow approximately 60 seconds for all services to initialize, register with Consul, and for the database migrations to complete.

---

## 7. Access Points and Default Credentials

### 7.1. Access Points

| Component | URL | Description |
| :--- | :--- | :--- |
| **Frontend Application** | `http://localhost:7274` | Main user interface for reporting and viewing incidents. |
| **API Gateway** | `http://localhost:5000` | Unified API endpoint for all microservices. |
| **API Documentation** | `http://localhost:5000/swagger` | Swagger UI for exploring all exposed API endpoints. |
| **Service Discovery UI** | `http://localhost:8500` | HashiCorp Consul web interface. |
| **Config Server Health** | `http://localhost:8888/actuator/health` | Health check endpoint for the Centralized Configuration Server. |
| **PostgreSQL** | `localhost:5433` | Database connection port. |

### 7.2. Default Credentials

The system is pre-populated with default users for testing the three defined roles:

| Role | Username | Password | Notes |
| :--- | :--- | :--- | :--- |
| **Admin** | `string3` | `string123` | Full administrative access, including user management. |
| **Moderator** | `moderator` | `moderator1` | Access to the Moderation Dashboard and Analytics. |
| **Regular User** | `string1` | `string123` | Standard user for incident reporting. |

---

## 8. Configuration and Image Management

### 8.1. Centralized Configuration Management

All critical system configurations, including database connection strings, JWT secrets, and logging levels, are externalized. They are stored in a Git repository and dynamically served to all microservices via the **ConfigServer** at runtime. This design allows for configuration changes without requiring service rebuilds or container restarts.

### 8.2. Image Storage and Serving

Incident images are handled through a dedicated process:

*   **Upload Endpoint**: Images are submitted via the `/api/incidents/upload-image` endpoint.
*   **Storage**: Files are stored on the file system within the `IncidentService/wwwroot/images/incidents` directory.
*   **Persistence**: A Docker volume ensures that image data persists across container lifecycles.
*   **Public Access**: Images are served publicly and securely through the API Gateway via the route: `http://localhost:5000/images/incidents/{filename}`.

---

## 9. CI/CD Pipeline

The Continuous Integration and Continuous Deployment pipeline is automated using GitHub Actions. The pipeline is configured to:

*   Automatically build all microservices upon a push to the `main` branch.
*   Create optimized Docker images for each service.
*   Push the resulting images to Docker Hub under the namespace: `markopreradovic/incidentreportingsystem-*`.
