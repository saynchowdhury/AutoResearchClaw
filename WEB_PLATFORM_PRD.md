# Product Requirements Document (PRD): AutoResearch Web Platform

## 1. Product Vision & Overview
**Vision**: Democratize academic research by providing a cloud-native, web-based platform where anyone—from hobbyists to enterprise R&D teams—can generate rigorous, conference-ready research papers simply by entering a prompt.

**Overview**: The platform wraps the open-source **AutoResearchClaw** pipeline into a seamless, accessible UI. Similar to long-running autonomous agents (like Devin or OpenHands), a single prompt kicks off a 1-to-2-hour autonomous loop in an isolated cloud environment. Users can chat with the agent in real-time, provide global context, and steer the research direction while the system handles literature review, coding, experimentation, and LaTeX generation.

---

## 2. User Personas
1. **The Independent Researcher / Hobbyist**: Has brilliant ideas but lacks the GPU compute, coding skills, or time to run empirical experiments and format LaTeX.
2. **The R&D Professional**: Needs rapid prototyping and literature reviews to validate hypotheses before assigning human engineers to a project.
3. **The Academic Student**: Wants a "co-pilot" to help structure papers, find missing citations, and generate baseline experimental code.

---

## 3. The User Journey & Pipeline
1. **Ideation & Prompting**: The user logs into the web app and enters a research prompt (e.g., "Investigate the impact of rotary embeddings on small vision transformers").
2. **Global Context Initialization**: The user can upload context files (PDFs, local datasets, API keys) or specify constraints.
3. **Agentic Execution (1-2 Hours)**: 
   - The platform provisions an isolated cloud sandbox (Docker container).
   - The AutoResearchClaw pipeline begins.
   - The user sees real-time updates: terminal logs, agent thoughts, generated code, and intermediate charts.
4. **Human-in-the-Loop (HITL) Steering**: The user can pause the run, chat with the agent to correct its course, or approve/reject pivots.
5. **Final Deliverable**: The platform presents a compile-ready LaTeX workspace, a generated PDF, and downloadable experimental logs.

---

## 4. Current Problems in AutoResearchClaw & Proposed Platform Solutions

To improve the success rate (reduction of errors/hallucinations) and provide a flawless cloud experience, we must address current limitations in the CLI-based pipeline:

| Current Problem | Proposed Platform Solution |
| :--- | :--- |
| **Silent Failures in Long Runs** | **State Checkpointing & Replays**: Snapshot the container state after every major stage (e.g., Literature Review -> Method -> Experiment). If an experiment fails, the user can "rewind" to a previous stage, modify the prompt, and replay without starting from scratch. |
| **Citation Hallucinations** | **RAG-Enhanced Citation Verifier**: Move beyond basic metadata checking. The platform will fetch actual open-access PDFs, chunk them into a Vector DB, and force the LLM to ground its claims using semantic search (RAG) before inserting a citation. |
| **Rigid Autonomous Execution** | **Interactive Chat & Global Context**: Introduce a sticky global context window. If the agent struggles during coding, the user can use the chat interface to intervene: *"Use PyTorch Lightning instead of raw PyTorch here."* The agent dynamically updates the pipeline based on chat. |
| **Sandbox Security & Resource Exhaustion** | **Ephemeral Cloud Compute**: Run experiments in strictly isolated, time-bound environments (e.g., Modal, Fly.io, or AWS ECS) with network egress controls to prevent malicious code execution while allowing API access to valid academic databases. |
| **Complex Error Handling** | **Intelligent Retries with LLM Diagnostics**: When code fails, instead of naive retries, an independent "Critic Agent" reads the stack trace, proposes 3 potential fixes, and asks the user for approval (or auto-applies the highest confidence fix). |

---

## 5. Proposed Tech Stack

To build a highly responsive, scalable cloud application:

### **Frontend (Web App)**
* **Framework**: Next.js 14+ (React) with App Router.
* **UI Components**: Tailwind CSS + Shadcn UI (for a clean, professional workspace look).
* **State Management**: Zustand (for local UI state) & React Query (for server state).
* **Real-Time Communication**: WebSockets / Server-Sent Events (SSE) to stream agent thoughts, terminal logs, and markdown generation in real-time.

### **Backend (Orchestration & API)**
* **Core API**: FastAPI (Python) or Node.js. Python is preferred to natively interface with the existing AutoResearchClaw codebase.
* **Database**: PostgreSQL (via Supabase or Neon) for user management, global context storage, and project metadata.
* **Cache/Queue**: Redis & Celery/BullMQ for managing long-running tasks (1-2 hours) asynchronously.
* **Storage**: AWS S3 or Cloudflare R2 for storing generated PDFs, LaTeX artifacts, and datasets.

### **Cloud Execution Engine (The Sandbox)**
* **Compute Platform**: **Modal** (highly recommended for serverless GPU/CPU Python execution) OR **AWS ECS / EKS** for container orchestration.
* **Containerization**: Docker. Each run spins up a fresh container with predefined limits (e.g., 4 vCPUs, 16GB RAM, optional GPU attachment).

---

## 6. Next Steps
1. **Review & Refine this PRD**: Do these solutions align with your vision for the "Open Matters"-style platform?
2. **System Architecture Design**: Map out the WebSocket events and database schema.
3. **UI/UX Mockups**: Design the chat interface, the real-time terminal/log view, and the artifact workspace.
4. **Proof of Concept (PoC)**: Wrap the current CLI in a basic FastAPI server + React frontend to trigger a run via the web.
