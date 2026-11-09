# RaceDay

## Project Overview

RaceDay is a web-based event management system designed for South African road running, walking, and cycling events.

The system enables event organisers to create and manage races, define participant categories, and record race results. Participants can browse available events, enroll in races, and track their performance history.

RaceDay streamlines the complete event lifecycle—from event creation through result recording—using role-based access control and real-time data management.

---

## User Roles

### Event Organiser

An Event Organiser is responsible for creating and managing road events.

#### Capabilities

- Create new events (races, walks, and cycling events)
- Define event categories such as age groups and distance types
- View all participants enrolled in their events
- Record participant finish times and positions
- View event routes and participant enrollments
- Update and delete event details

#### Example

Sarah Mokoena organises the Comrades Marathon and records Lebo's finish time as **05:47:30**, placing him in **1st position**.

---

### Participant

A Participant is a user competing in road events.

#### Capabilities

- Browse all available events
- View event details and categories
- Enroll in events and select a category
- View personal enrollment history
- Track race results and performance history
- View event routes and prepare for races

#### Restrictions

Participants cannot create events or record race results.

#### Example

Lebo Khumalo browses upcoming marathons, enrolls in the Comrades 87 km category, and checks his finish time after the race.

---

## Database Schema

The database consists of **8 entities** that support the RaceDay platform.

### Entities

| Entity | Description |
|--------|-------------|
| USER | Base user information |
| EVENT_ORGANISER | Organiser-specific role |
| PARTICIPANT | Participant-specific role |
| EVENT | Race events |
| EVENT_CATEGORIES | Event divisions such as age groups and distance types |
| EVENT_ENROLMENT | Participant sign-ups (three-way junction table) |
| ROUTE | Race route information |
| RESULTS | Participant finish times and positions |

### Key Relationships

- One **User** can be either one **Event Organiser** or one **Participant**.
- One **Event Organiser** can create multiple **Events**.
- One **Event** has one **Route**.
- One **Event** contains multiple **Event Categories**.
- Multiple **Participants** can enroll in multiple **Events** through the **EVENT_ENROLMENT** junction table.

---

## API Endpoints

The REST API consists of **28 endpoints** organised into seven functional areas.

| Functional Area | Endpoints |
|-----------------|-----------|
| Authentication | 3 |
| User Profile | 3 |
| Events | 6 |
| Categories | 4 |
| Enrolments | 4 |
| Results | 4 |
| Route | 4 |

**Total Endpoints:** 28
---

## Docker Deployment

RaceDay MVC is containerised using Docker to provide a consistent environment for running the web application.

The Docker configuration uses the official .NET 8 SDK image to build the application and the .NET 8 ASP.NET runtime image to run it.

### Prerequisites

Before running RaceDay through Docker, ensure that the following are installed and running:

- Docker Desktop
- .NET 8 SDK
- SQL Server / SQL Server LocalDB
- Azurite (Azure Blob Storage emulator)

### Docker Configuration

The MVC project contains the following Docker configuration files:

| File | Purpose |
|------|---------|
| `Dockerfile` | Builds and publishes the MVC application into a Docker image. |
| `.dockerignore` | Excludes unnecessary files and folders from the Docker build context. |

### Running the Application

**1. Start Azurite**

Start the existing Azurite container:

```powershell
docker start raceday-azurite
```

**2. Start the REST API**

Navigate to the `RaceDay.API` project directory and run:

```powershell
dotnet run --launch-profile docker-host
```

The API listens on:

`http://localhost:5298`

Keep the API running while using the MVC application.

**3. Build the MVC Docker Image**

Navigate to the `RaceDay.MVC` project directory and execute:

```powershell
docker build -t raceday-mvc .
```

This builds the MVC application and creates a Docker image named `raceday-mvc`.

**4. Run the MVC Container**

Execute:

```powershell
docker run -d -p 8080:8080 -e ApiSettings__BaseUrl=http://host.docker.internal:5298/ --name raceday-mvc-container raceday-mvc
```

This starts the MVC application inside a Docker container and connects it to the REST API running on the host machine.

**5. Access RaceDay**

Open a browser and navigate to:

`http://localhost:8080`

The RaceDay MVC application should now be accessible through Docker.

### Stopping the Application

To stop the MVC container:

```powershell
docker stop raceday-mvc-container
```

To remove the stopped container:

```powershell
docker rm raceday-mvc-container
```

To stop the API, press `Ctrl + C` in its terminal.

Azurite can be stopped separately using:

```powershell
docker stop raceday-azurite
```

### Image Storage

The REST API handles image uploads and retrieval through the Blob Storage service. Azurite is used as a local Azure Blob Storage emulator during development.

The MVC application communicates with the API for image handling rather than connecting directly to Blob Storage.

This allows uploaded event banners and profile pictures to be displayed while the MVC application runs inside Docker.

---
---
## CI/CD Pipeline

The project includes a Continuous Integration and Continuous Deployment (CI/CD) pipeline to ensure code quality and reliable releases.

A screenshot of the CI/CD :

<img width="1920" height="1020" alt="image" src="https://github.com/user-attachments/assets/34bfa9ce-938f-4047-8307-728475dc1c37" />

---

## Demonstration Video

The project demonstration video is available through the unlisted YouTube link below.

**YouTube Link:** https://youtu.be/mfCToZkuEH0
