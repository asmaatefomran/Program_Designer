# Program Designer

A .NET 8 REST API for creating, storing, and validating hierarchical learning programs.

A learning program is a recursive tree of **groups** (containers with an `All`/`Choice` rule) and **steps** (leaf activities). Any node can declare **prerequisites**, and the validation engine checks the tree for impossible prerequisites (cycles / forward references) and warns about prerequisites that are reachable but not guaranteed for every participant (i.e. they sit inside a `Choice` branch that might not be picked).

---

## Demo Video

📹 *Video walkthrough will be linked here:* **[Add Google Drive link before submitting]**

---

## Table of Contents

- [Getting Started (Clone & Run)](#getting-started-clone--run)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Data Model](#data-model)
- [API Reference](#api-reference)
    - [Request/Response Shapes](#requestresponse-shapes)
    - [POST /programs](#post-programs)
    - [GET /programs](#get-programs)
    - [GET /programs/:id](#get-programsid)
    - [POST /programs/:id/validate](#post-programsidvalidate)
    - [POST /programs/:id/simulate](#post-programsidsimulate-bonus)
- [Validation Rules](#validation-rules)
- [Testing](#testing)
- [Frontend](#frontend)
- [AI Usage](#ai-usage)
- [Design Decisions](#design-decisions)
- [Future Improvements](#future-improvements)

---

## Getting Started (Clone & Run)

Requirements: **Docker** and **Docker Compose** (that's it - no local .NET or Node install needed for this path).

```bash
# 1. Clone the repository
git clone https://github.com/asmaatefomran/Program_Designer.git
cd Program_Designer

# 2. Make sure Docker Engine is running (start it if it's stoped)

# 3. Start everything: Postgres + API + frontend
docker compose up --build
```

That's it. Give it a minute the first time (it builds the API, the frontend, and pulls Postgres).

| Service     | URL                                |
|-------------|-------------------------------------|
| Frontend    | http://localhost:5173               |
| API         | http://localhost:5000               |
| Swagger UI  | http://localhost:5000/swagger       |
| Postgres    | localhost:5432 (`postgres`/`postgres`) |

Database migrations are applied automatically on startup,  there is nothing extra to run.

You should see all the Green words at the end, now the frontend + backend + database are running correctly.
<img width="1380" height="552" alt="image" src="https://github.com/user-attachments/assets/dcc880ce-669e-4cb8-b2d6-42567946fd70" />


To stop everything:

```bash
docker compose down        # add -v to also wipe the postgres volume
```



---

## Tech Stack

- ASP.NET Core 8 (Web API)
- Entity Framework Core + PostgreSQL
- xUnit (unit + integration tests)
- Docker & Docker Compose
- Frontend (optional, included): Vite + React + TypeScript + Tailwind + shadcn/ui + React Flow

---

## Project Structure

```text
.
├── src
│   └── ProgramDesigner.Api
│       ├── Controllers/          # ProgramsController, HealthController
│       ├── DTOs/
│       │   ├── Requests/         # CreateProgramRequest, NodeRequest, GroupRequest, StepRequest, SimulateRequest
│       │   └── Responses/        # ProgramResponse, ValidationResponse, SimulateResponse, ...
│       ├── Domain/
│       │   ├── Entities/         # Program, Node, Group, Step, NodePrerequisite
│       │   └── Enums/            # GroupType (All | Choice)
│       ├── Services/             # ProgramService, ValidationService, SimulationService, ProgramBuilderService
│       ├── Data/                 # ApplicationDbContext + EF Core configurations
│       └── Migrations/
├── tests
│   └── ProgramDesigner.Tests     # Unit + integration tests, includes the CS scenario as test data
├── frontend                      # Vite/React/TS visual builder (optional, not required by the brief)
├── docker-compose.yml
└── README.md
```

---

## Data Model

The domain is a **recursive tree**, so it can represent any program, not just the Computer Science example:

- **`Program`** — top-level wrapper with a `Name` and one root `Group`.
- **`Node`** (abstract) — base type for anything that can appear in the tree. Has an `Id`, a `TemplateId`, a `Name`, and a list of prerequisite references. `Node` is specialized into:
    - **`Step`** — a leaf activity, no children.
    - **`Group`** — a container with:
        - `GroupType`: `All` (in order / everything required) or `Choice` (pick N of M)
        - `RequiredChoiceCount`: the N, only meaningful when `GroupType == Choice`
        - `Children`: a list of `Node` (steps and/or nested groups, to any depth)
- **`TemplateId`** — a designer-facing logical id (e.g. `"step-intro"`). Prerequisites are declared by `TemplateId` rather than database `Id` because the same logical step or group can legitimately appear more than once in the tree — cloned into two or more branches of a `Choice` group — each occurrence getting its own `Id` when persisted. Those occurrences represent the same underlying activity, so a prerequisite should be satisfied by completing *any* of them, not just one specific database row. Declaring the prerequisite against the shared `TemplateId` lets the validator (and the reachability check in particular) treat all occurrences as interchangeable when deciding whether a prerequisite is guaranteed. The API resolves `TemplateId` references into real prerequisite relationships when the tree is persisted.
- **Prerequisites** — any `Step` or `Group` can list `PrerequisiteTemplateIds` pointing at other nodes anywhere in the same program.

---

## API Reference

### Request/Response Shapes

Nodes are polymorphic and discriminated by a `"type"` field (`"step"` or `"group"`), both in requests and responses.

**Node fields (shared by step and group):**

| Field                    | Type       | Notes |
|--------------------------|------------|-------|
| `type`                   | `"step"` \| `"group"` | discriminator |
| `templateId`             | string     | your own logical id, referenced by prerequisites |
| `name`                   | string     | display name |
| `prerequisiteTemplateIds`| string[]   | `templateId`s this node depends on |

**Group-only fields:**

| Field                 | Type              | Notes |
|-----------------------|-------------------|-------|
| `groupType`            | `"All"` \| `"Choice"` | `All` = in order/everything required, `Choice` = pick N of M |
| `requiredChoiceCount`  | number            | the N in "pick N of M" (ignored for `All`) |
| `children`             | Node[]            | steps and/or nested groups |

---

### POST /programs

Creates a program from a full nested tree and returns the created program **plus its initial validation result**.

**Request body** (trimmed example — a group with two ordered steps):

```json
{
  "name": "Computer Science",
  "rootGroup": {
    "type": "group",
    "templateId": "root",
    "name": "Computer Science",
    "groupType": "All",
    "requiredChoiceCount": 0,
    "children": [
      {
        "type": "group",
        "templateId": "foundations",
        "name": "Foundations",
        "groupType": "All",
        "requiredChoiceCount": 0,
        "children": [
          { "type": "step", "templateId": "intro", "name": "Introduction to Computing" },
          { "type": "step", "templateId": "math", "name": "Mathematics for Computing" }
        ]
      },
      {
        "type": "step",
        "templateId": "final-capstone",
        "name": "Final Capstone",
        "prerequisiteTemplateIds": ["foundations"]
      }
    ]
  }
}
```

**Response `201 Created`:**

```json
{
  "program": {
    "id": "3f1b2c...",
    "name": "Computer Science",
    "rootGroup": { "id": "...", "templateId": "root", "type": "group", "...": "full tree, with real ids" }
  },
  "validation": {
    "isValid": true,
    "errors": [],
    "warnings": []
  }
}
```

> The full nested Computer Science scenario from the brief (with the `Major` choice group, `AI`/`IT`/`Programming` branches, and `Electives`) lives in `tests/ProgramDesigner.Tests/TestData/ComputerScienceScenario.cs` — copy its shape if you want the complete payload rather than the trimmed example above.

---

### GET /programs

Lists created programs (paginated), useful for the frontend / for sanity-checking what you've created.

`GET /programs?page=1&pageSize=10`

```json
{
  "items": [
    { "id": "...", "name": "Computer Science", "createdAt": "2026-07-18T12:00:00Z" }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

### GET /programs/:id

Returns the full stored tree for a program.

```json
{
  "id": "3f1b2c...",
  "name": "Computer Science",
  "rootGroup": { "...": "full nested tree" }
}
```

`404` if the id doesn't exist.

### POST /programs/:id/validate

Re-runs validation against the stored program and returns:

```json
{
  "isValid": false,
  "errors": [
    { "code": "IMPOSSIBLE_PREREQUISITE", "nodeId": "ai-capstone", "message": "AI Capstone has a prerequisite on Electives, which is inside AI Capstone's own scope..." }
  ],
  "warnings": [
    { "code": "POTENTIALLY_UNREACHABLE", "nodeId": "final-capstone", "message": "Final Capstone depends on AI Capstone, which only exists if the participant chooses the AI option in Major." }
  ]
}
```

- **`errors`** — impossible prerequisites: self-references, references to a descendant, or forward references. These make `isValid: false`.
- **`warnings`** — a prerequisite that depends on something inside a `Choice` group that isn't guaranteed to be picked. `isValid` can still be `true` when only warnings are present.

`404` if the id doesn't exist.

### POST /programs/:id/simulate (bonus)

Given a participant's choices in each `Choice` group and which steps they've completed, returns what's complete / unlocked / blocked (and why).

**Request:**

```json
{
  "choices": {
    "major": ["ai"]
  },
  "completedStepTemplateIds": ["intro", "math"]
}
```

**Response:**

```json
{
  "complete": [ { "id": "...", "templateId": "intro", "name": "Introduction to Computing", "status": "complete" } ],
  "unlocked": [ { "id": "...", "templateId": "ml-basics", "name": "Machine Learning Basics", "status": "unlocked" } ],
  "blocked":  [ { "id": "...", "templateId": "it-networks", "name": "Networks & Security", "status": "blocked", "reason": "Not part of the chosen Major option" } ]
}
```



---

## Validation Rules

`POST /programs/:id/validate` runs every check below against the stored tree. Each one emits a `ValidationIssueResponse` (`code`, `nodeId`, `message`) into either `errors` (makes `isValid: false`) or `warnings` (valid but worth flagging — `isValid` can still be `true`).

### Errors

| Code | What it catches |
|------|------------------|
| `SELF_DEPENDENCY` | A node's prerequisite points at itself. *(Required scenario: "prerequisite pointing at itself is rejected".)* |
| `PREREQUISITE_ON_ANCESTOR` | A node's prerequisite points at one of its own ancestor groups — i.e. something that contains it. Circular by definition: the ancestor can't be "done" until this node is, so this node can't also be waiting on the ancestor. *(Required scenario: "points at something inside itself".)* |
| `FORWARD_PREREQUISITE` | A prerequisite appears later than the node that depends on it, in tree traversal order. *(Required scenario: "points at something later".)* |
| `CIRCULAR_DEPENDENCY` | General cycle detection across prerequisite chains (A → B → C → A), walked via DFS over template references rather than just direct/adjacent cases. *(Required scenario: "direct prerequisite cycle is rejected".)* |
| `MISSING_PREREQUISITE` | A prerequisite references a `templateId` that doesn't exist anywhere in the program. |
| `INVALID_TREE` | The same node instance appears more than once while walking the tree — a structural corruption, distinct from an intentional template clone. |
| `INVALID_PARENT` | A child's recorded parent group doesn't match the group that actually contains it. |
| `EMPTY_GROUP` | A group has zero children. |
| `INVALID_REQUIRED_SELECTIONS` | A `Choice` group's required-selection count is negative, or greater than its number of children (can't pick 3 of 2). |
| `INCONSISTENT_TEMPLATE` | The same `templateId` is intentionally reused in more than one place (a clone), but the copies disagree — different node type (step vs. group), different name, different prerequisites, or (for groups) different `groupType`/required-selection count. Clones of the same template are expected to be identical in everything except position. |

### Warnings

| Code | What it catches |
|------|------------------|
| `UNREACHABLE_PREREQUISITE` | A prerequisite is only guaranteed along *some* participant paths, not all — it lives inside a `Choice` branch that isn't guaranteed to be picked, with no equivalent clone covering every other branch. *(Required scenario: "prerequisite depends on a path the participant might not take".)* This is the reachability check from Part 2 — it accounts for a template being deliberately cloned into multiple branches (if every branch of the choice contains an occurrence of the prerequisite, or every branch is mandatory, it's still guaranteed and does **not** warn), so it's not simply "is this inside any choice group". |
| `CHOICE_REQUIRES_ALL_CHILDREN` | A `Choice` group's required-selection count equals its total number of children — every option is effectively mandatory, which functionally makes it an `All` group wearing a `Choice` label. Structurally valid, but likely a designer mistake worth flagging. |

---

## Testing

```bash
docker compose --profile tests run --rm tests
# or, without Docker:
cd tests/ProgramDesigner.Tests && dotnet test
```

Covers, at minimum:

- The full Computer Science scenario validates cleanly (no errors, no warnings).
- A direct prerequisite cycle is rejected.
- A self-referencing prerequisite is rejected.
- A prerequisite reachable only through one branch of a `Choice` group produces a warning, not a rejection.

---

## Frontend

`frontend/` is a Vite + React + TypeScript app for building and validating a program visually instead of hand-writing JSON: a recursive tree editor, a live diagram (solid edges = containment, dashed edges = prerequisites), and a validation panel that highlights the exact node an error/warning is about.

---

## AI Usage

This project was developed with extensive use of AI assistants, primarily ChatGPT, with additional assistance from Claude.

AI was used for:
- Discussing implementation approaches and alternative designs.
- Generating implementation suggestions and code examples.
- Reviewing code and suggesting refactoring opportunities.
- Assisting with debugging, Docker configuration, Entity Framework issues, and documentation (including this README).

My own responsibilities:
- Defining the overall solution and driving the implementation.
- Designing the domain model and API behavior.
- Identifying and implementing the required validation rules.
- Writing and expanding the test suite, including edge cases.
- Evaluating, modifying, and integrating AI-generated suggestions.
- Debugging, architectural decisions, and final verification of correctness.

All code in the final project was reviewed, tested, and validated by me. I take full responsibility for the implementation and understand the design decisions and business logic throughout the codebase. The frontend (`frontend/`) was built with Claude directly inside an existing Vite/React/Tailwind/shadcn scaffold, matching the API contract in `DTOs/`.

---

## Design Decisions

- The domain is modeled as a recursive tree of `Group`/`Step`, so it can represent any program structure, not just the Computer Science example.
- Prerequisites are declared by `templateId` in requests (a designer-facing logical id) rather than database `Id`, because the same step or group can be intentionally cloned into multiple branches of a `Choice` group — each clone getting its own `Id` — while still representing the same underlying activity. Keying prerequisites off `templateId` means completing *any* clone of the referenced template satisfies the prerequisite, which is exactly what the reachability check (`UNREACHABLE_PREREQUISITE`) relies on when a prerequisite is cloned into every branch of a choice.
- Validation logic lives in its own service, separate from the API/controllers, so business rules stay independent of transport concerns.
- EF Core + PostgreSQL for persistence, with migrations applied automatically at startup.
- Docker Compose provides one consistent command for the full stack and for tests.

---

## Future Improvements

Given more time:

- Update and delete endpoints for programs.
- API versioning.
- Authentication and authorization.
- Richer pagination/filtering on `GET /programs`.
- CI/CD pipeline.
- Structured logging and monitoring.
- Performance work for very large program trees.

---

## License

Developed as part of a backend take-home assignment.
