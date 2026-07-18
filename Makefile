.PHONY: up down build logs test frontend-dev

## Start the full stack (Postgres + API + frontend), building images as needed
up:
	docker compose up --build

## Same as `up`, but detached
up-d:
	docker compose up --build -d

down:
	docker compose down

## Follow API logs
logs:
	docker compose logs -f backend

## Run the backend test suite in its own container
test:
	docker compose --profile tests run --rm tests

## Run the frontend dev server locally (not in Docker) against a stack already up
frontend-dev:
	cd frontend-new && npm install && npm run dev
