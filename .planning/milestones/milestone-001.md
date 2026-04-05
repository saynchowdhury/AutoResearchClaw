# Milestone 1: MVP Walking Skeleton

## Goal
Fire-and-forget prompt → Backend spins up Docker container → Streams live logs via WebSockets → Outputs final PDF and ZIP.

## Scope (v0.1)
- Basic FastAPI server with POST /api/runs/start endpoint
- Redis + background worker (Celery/ARQ) for 2-hour task execution
- Next.js frontend with split-pane layout
- WebSocket/SSE event streaming
- Local Docker sandbox for execution
- Project, Runs, Logs database tables

## Phases
- [Phase 1] Infrastructure Setup (Docker Compose, PostgreSQL, Redis)
- [Phase 2] Backend Foundation (FastAPI, WebSocket manager, database models)
- [Phase 3] Frontend Scaffolding (Next.js, split-pane UI, state management)
- [Phase 4] Sandbox Integration (Docker SDK, artifact watching)
- [Phase 5] Event Streaming (agent_thought, terminal_log, stage_change, artifact_update)
- [Phase 6] Integration & Testing (end-to-end run)

## Target
- User enters research prompt
- Backend returns run_id immediately (non-blocking)
- Frontend streams live logs and artifacts
- Final output: PDF + ZIP of LaTeX workspace