# AutoResearch Web Platform

## Vision
Build a cloud-native, web-based platform where anyone can generate conference-ready research papers by entering a prompt. Wraps the AutoResearchClaw CLI into a seamless "OpenHands/Devin" style web experience.

## Key Features
- Split-pane UI: Chat (left) + Live workspace (right)
- Real-time streaming via WebSockets/SSE
- Checkpoints & replay for 23-stage pipeline
- RAG-enhanced citation verification
- Local LLM support (Ollama, vLLM)

## Tech Stack
- Frontend: Next.js 14+, React, TailwindCSS, Shadcn UI, Zustand
- Backend: FastAPI (Python), PostgreSQL, Redis, Celery
- Execution: Docker sandbox (local dev), Modal/AWS (prod)
- LLM: OpenAI, Anthropic, Gemini, Ollama

## Status
- [x] PRD documented (WEB_PLATFORM_PRD.md)
- [x] Architecture designed (ARCHITECTURE_DESIGN.md)
- [x] System prompt created (SYSTEM_PROMPT.md)
- [ ] GSD project initialized
- [ ] MVP scaffolded

## Milestones
1. MVP: Fire-and-forget prompt → stream logs → output PDF
2. v1.0: Full HITL with stage checkpoints, RAG citations