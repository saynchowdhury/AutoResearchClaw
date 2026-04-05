# Architecture & Pre-Build Checklist (MVP Scope)

Before initiating the first build (especially with a modern stack like Next.js App Router), we need to define the **"Walking Skeleton"**—the absolute minimum architecture required to get a 2-hour autonomous agent running reliably in a web browser without timeouts or silent failures.

Here are the 5 critical technical pillars we must define before writing the first line of code:

## 1. The MVP Scope (v0.1 vs v1.0)
Building the full Human-in-the-Loop (HITL) rewind/replay system on Day 1 is too complex. 
* **MVP (v0.1)**: Fire-and-forget prompt -> Backend spins up Docker -> Streams logs via WebSockets -> Outputs final PDF and ZIP. The user can watch it live and type in the chat to add global context, but no complex "stage rewinding" yet.
* **v1.0**: Full interactive stage checkpoints, RAG citation verifier, and the visual Diff editor.

## 2. Event-Driven Communication Protocol (WebSockets / SSE)
A standard HTTP request will timeout after 30-60 seconds. A 2-hour run requires an asynchronous event stream. We need to define the payload structure for our WebSockets or Server-Sent Events (SSE).
* **`agent_thought`**: Streaming the LLM's reasoning (e.g., "I need to search OpenAlex for related work on rotary embeddings...").
* **`terminal_log`**: Streaming stdout/stderr from the Python execution sandbox.
* **`stage_change`**: Emitted when AutoResearchClaw moves from Stage 2 (Lit Review) to Stage 3 (Methodology).
* **`artifact_update`**: Emitted when a file is created or modified (e.g., `draft.tex` updated), triggering the frontend UI to refresh the file tree.

## 3. Database Schema (The State Machine)
We need a robust way to resume runs if the Next.js server restarts or the user closes their laptop and returns an hour later.
* **`Users` Table**: Auth (Supabase / Clerk).
* **`Projects` Table**: Represents a research topic and holds the **Global Context** (system prompts, user constraints).
* **`Runs` Table**: Represents a specific 2-hour execution. Tracks `status` (queued, running, paused, failed, completed), `current_stage` (1-23), and `container_id`.
* **`Logs` Table**: Time-series storage for terminal output and agent thoughts (so if a user refreshes the page, the frontend fetches past logs from the DB before subscribing to the live WebSocket).

## 4. The Sandbox Strategy for Local Dev
Before paying for cloud compute like Modal or AWS ECS, the first build should run locally.
* **Dev Sandbox**: The FastAPI backend will use the Python `docker` SDK to spin up a local container for the agent. We will mount a specific volume `/artifacts` which the frontend will watch to display generated files.
* **Production Sandbox**: Later, we swap the local Docker API calls for Modal API calls or AWS ECS Task execution, keeping the rest of the application agnostic to *where* the code runs.

## 5. Background Task Queue (Celery / Redis)
FastAPI cannot run a 2-hour Python script in its main thread or event loop.
* We must set up **Redis** + a background worker (like **Celery** or **ARQ**).
* When Next.js calls `POST /api/research/start`, FastAPI immediately returns `202 Accepted` with a `run_id`.
* The background worker actually executes the AutoResearchClaw pipeline, emitting WebSocket events to a Redis Pub/Sub channel, which FastAPI then broadcasts to the Next.js frontend.
