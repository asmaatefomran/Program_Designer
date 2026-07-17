# Program Designer API

## Overview

Program Designer API is a .NET 8 REST API for creating, storing and validating hierarchical learning programs.

A learning program consists of nested groups and learning steps. Groups may define selection rules and prerequisite relationships, while the validation engine verifies that the program structure satisfies the business rules.

---

## Features

- Create learning programs
- Retrieve learning programs
- Validate learning programs
- Hierarchical tree structure
- Prerequisite validation
- Group selection rules
- PostgreSQL persistence
- Docker support
- Integration and unit tests

---

## Tech Stack

- ASP.NET Core 8
- Entity Framework Core
- PostgreSQL
- xUnit
- Docker & Docker Compose

---

## Running the application

Start the API and PostgreSQL

```bash
docker compose up --build
```

API Base URL

```text
http://localhost:5000
```

Swagger UI

```text
http://localhost:5000/swagger
```


---


## Running tests

Start the API and PostgreSQL

```bash
docker compose --profile tests run --rm tests
```


---

# API

Instead of pasting the huge JSON, I'd make a nice table.

| Method | Endpoint | Description |
|---------|----------|-------------|
| GET | `/api/Health` | Health check |
| POST | `/programs` | Create a learning program |
| GET | `/programs/{id}` | Retrieve a learning program |
| POST | `/programs/{id}/validate` | Validate a learning program |

---

Then add a **small** request example.

## Create Program

```json
{
  "name": "Computer Science",
  "rootGroup": {
    "type": "group",
    "templateId": "group-root",
    "name": "Computer Science",
    "children": [
      {
        "type": "step",
        "templateId": "step-intro",
        "name": "Introduction to Computing"
      }
    ]
  }
}
```

### Response

```json
{
  "program": {
    "id": "...",
    "name": "Computer Science"
  },
  "validation": {
    "isValid": true,
    "errors": [],
    "warnings": []
  }
}
```

---

## Validation

The validation endpoint analyzes a learning program and returns whether it is valid, along with any detected errors or warnings.

Example response:

```json
{
  "isValid": true,
  "errors": [],
  "warnings": []
}
```

Validation checks include:

- Valid prerequisite references
- Hierarchical structure integrity
- Group selection constraints
- Circular prerequisite detection
- Duplicate template identifiers (if applicable)

---

## Project Structure

```text
.
├── src
│   └── ProgramDesigner.Api
├── tests
│   └── ProgramDesigner.Tests
├── docker-compose.yml
├── README.md
└── .dockerignore
```

---

## Example Program Structure

A learning program is represented as a hierarchical tree:

```text
Program
└── Computer Science
    ├── Foundations
    │   ├── Introduction to Computing
    │   └── Mathematics for Computing
    ├── Major
    │   ├── AI
    │   │   ├── Machine Learning Basics
    │   │   ├── Electives
    │   │   │   ├── Computer Vision
    │   │   │   ├── Natural Language Processing
    │   │   │   └── Robotics
    │   │   └── AI Capstone
    │   ├── IT
    │   └── Programming
    └── Final Capstone
```

---

## Design Decisions

- ASP.NET Core Minimal APIs are used to keep the API lightweight and focused.
- Entity Framework Core provides persistence and database migrations.
- PostgreSQL is used as the relational database.
- The domain is modeled as a recursive tree consisting of groups and learning steps.
- Validation logic is separated from the API layer to keep business rules independent of transport concerns.
- Docker Compose provides a consistent development and testing environment.

---

## Testing

The solution includes automated tests covering:

- Program creation
- Program retrieval
- Validation rules
- API endpoints
- Database persistence

Run the tests with:

```bash
docker compose --profile tests run --rm tests
```

---

## AI Usage

This project was developed with extensive use of AI assistants, primarily ChatGPT, with additional assistance from Claude.

AI was used as a collaborative development tool for:
- Discussing implementation approaches and alternative designs.
- Generating implementation suggestions and code examples.
- Reviewing code and suggesting refactoring opportunities.
- Assisting with debugging, Docker configuration, Entity Framework issues, and documentation.

My responsibilities included:
- Defining the overall solution and driving the implementation.
- Designing the domain model and API behavior.
- Identifying and implementing the required validation rules.
- Creating and expanding the test suite, including edge cases and validation scenarios.
- Evaluating, modifying, and integrating AI-generated suggestions.
- Debugging issues, making architectural decisions, and verifying the correctness of the final solution.

All code included in the final project was reviewed, tested, and validated by me. I take full responsibility for the implementation and understand the design decisions and business logic throughout the codebase


---

## Future Improvements

Given more time, I would consider adding:

- Update and delete endpoints
- API versioning
- Authentication and authorization
- Pagination and filtering
- CI/CD pipeline
- Structured logging and monitoring
- Performance optimizations for very large program trees

---

## License

This project was developed as part of a backend take-home assignment.
