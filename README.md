# RaceDay

## Project Overview

RaceDay is a web-based event management system designed for South African road running, walking, and cycling events.

The system enables event organisers to create and manage races, define participant categories, and record race results. Participants can browse available events, enroll in races, and track their performance history.

RaceDay streamlines the complete event lifecycle—from event creation through result recording—using role-based access control and real-time data management.

---
## Setup and Run Instructions

### Prerequisites

Before running RaceDay, ensure that the following software is installed:

-   Visual Studio 2022 or later with the ASP.NET and web development
    workload.
-   .NET 8 SDK.
-   SQL Server Express LocalDB.
-   Git.

### 1. Clone the Repository

Clone the repository from GitHub:

``` bash
git clone https://github.com/spyy-dahh/RaceDay.git
cd RaceDay
```

Switch to the `Part2` branch if it is the branch being submitted:

``` bash
git checkout Part2
```

Alternatively, download the repository from GitHub and select the
`Part2` branch before downloading.

### 2. Configure the Database Connection

RaceDay uses SQL Server LocalDB. The database connection string is
configured in `appsettings.json` in the API project.

Before running the application:

1.  Ensure SQL Server LocalDB is installed.
2.  Open `RaceDay.API/appsettings.json`.
3.  Check that the connection string points to the intended database and
    LocalDB instance.
4.  Ensure that the database exists and contains the required tables and
    data.

If the database has not been created, use the project's database
creation SQL script, if provided, or the database setup procedure
documented in the repository.

Do not commit database passwords or other sensitive credentials to
GitHub.

### 3. Restore Dependencies

Open a terminal in the repository root and run:

``` bash
dotnet restore RaceDay.Api.Tests/RaceDay.Api.Tests.csproj
```

This restores the NuGet packages required by the test project and its
referenced API project.

### 4. Run the API

Open the RaceDay solution in Visual Studio and set `RaceDay.API` as the
startup project.

Run the application using the Run button in Visual Studio.

Alternatively, run the API from the repository root:

``` bash
dotnet run --project RaceDay.API/RaceDay.API.csproj
```

Use the application URL displayed in the terminal. If Swagger is
enabled, open the Swagger URL to explore and test the available API
endpoints.

### 5. Run the Automated Tests

From the repository root, run:

``` bash
dotnet test RaceDay.Api.Tests/RaceDay.Api.Tests.csproj
```

This command builds the required projects and executes the automated
tests.

The project also uses GitHub Actions to build and test the application
automatically when changes are pushed to the repository.

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
## CI/CD Pipeline PART 1

The project includes a Continuous Integration and Continuous Deployment (CI/CD) pipeline to ensure code quality and reliable releases.

A screenshot of the CI/CD :

<img width="1920" height="1080" alt="Screenshot 2026-09-04 132215" src="https://github.com/user-attachments/assets/79aa6d90-45da-44c3-b951-83985e86a122" />

## CI/CD Pipeline PART 2

<img width="1920" height="1020" alt="image" src="https://github.com/user-attachments/assets/61bc8f20-a21d-4bdf-b5cd-d2b7f2140746" />

---

## Demonstration Video

The project demonstration video is available through the unlisted YouTube link below.

## PART 1
**YouTube Link:** https://youtu.be/iyjOG4kLHJY

## PART 2
