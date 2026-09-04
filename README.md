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
## CI/CD Pipeline

The project includes a Continuous Integration and Continuous Deployment (CI/CD) pipeline to ensure code quality and reliable releases.

A screenshot of the CI/CD pipeline is available in the `docs` folder.

---

## Demonstration Video

The project demonstration video is available through the unlisted YouTube link below.

**YouTube Link:** 
