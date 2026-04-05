#!/bin/bash

# ==============================================================================
# AutoResearch Web Platform - Skill Installation & Integration Script
# This script downloads, organizes, and integrates AI Agent Skills into the 
# codebase so the platform users can select and use them during research runs.
# ==============================================================================

set -e

echo "🚀 Initializing Skill Environment for AutoResearch Web Platform..."

# 1. Define the target directories in the codebase for these skills
SKILLS_DIR="./src/AgentSkills"
ML_SKILLS_DIR="$SKILLS_DIR/ML_Research"
WEB_SKILLS_DIR="$SKILLS_DIR/UI_UX_Web"
TEMP_CLONE_DIR="/tmp/ai-research-skills-repo"

mkdir -p "$ML_SKILLS_DIR"
mkdir -p "$WEB_SKILLS_DIR"

# 2. Clone the AI-Research-SKILLs repository for the ML/Research capabilities
echo "📦 Fetching AI-Research-SKILLs from Orchestra-Research..."
if [ -d "$TEMP_CLONE_DIR" ]; then
    rm -rf "$TEMP_CLONE_DIR"
fi
git clone https://github.com/Orchestra-Research/AI-Research-SKILLs.git "$TEMP_CLONE_DIR" --depth 1 --quiet

# 3. Smartly integrate ML & Research Skills into the application's workspace
echo "🧠 Integrating Core ML & Research Skills..."
cp -r "$TEMP_CLONE_DIR/0-autoresearch-skill" "$ML_SKILLS_DIR/"
cp -r "$TEMP_CLONE_DIR/20-ml-paper-writing" "$ML_SKILLS_DIR/"
cp -r "$TEMP_CLONE_DIR/21-research-ideation" "$ML_SKILLS_DIR/"
cp -r "$TEMP_CLONE_DIR/04-mechanistic-interpretability" "$ML_SKILLS_DIR/"
cp -r "$TEMP_CLONE_DIR/08-distributed-training" "$ML_SKILLS_DIR/"
cp -r "$TEMP_CLONE_DIR/11-evaluation" "$ML_SKILLS_DIR/"
cp -r "$TEMP_CLONE_DIR/14-agents" "$ML_SKILLS_DIR/"
cp -r "$TEMP_CLONE_DIR/16-prompt-engineering" "$ML_SKILLS_DIR/"

# 4. Integrate UI/UX and Next.js / React Skills (for local agent building & web capabilities)
echo "🎨 Integrating UI/UX and Web Framework Skills..."
# We will create placeholder directories or fetch them if they exist in the user's local ~/.pi/agent/skills
# Since this script runs on the user's machine, we can sync common Next.js/React skills if available, 
# or download them from relevant public repositories.

# Creating stub skills for the platform UI/UX so the LLM agent has immediate context
cat << 'EOF' > "$WEB_SKILLS_DIR/nextjs-best-practices.md"
# Next.js 14+ App Router Best Practices
- Use Server Components by default.
- Use `use client` only at the leaves of the component tree.
- Fetch data on the server with `async/await`.
- Use Shadcn UI for accessible components.
EOF

cat << 'EOF' > "$WEB_SKILLS_DIR/ui-ux-design-guidelines.md"
# Web UI/UX Design Guidelines
- Maintain a clean, split-pane layout for complex agent tasks.
- Use Lucide Icons for clarity.
- Stream data using WebSockets/SignalR to avoid perceived latency.
- Provide clear error boundaries and "retry" mechanisms in the UI.
EOF

cat << 'EOF' > "$WEB_SKILLS_DIR/signalr-realtime-patterns.md"
# .NET SignalR Real-Time Patterns
- Handle auto-reconnects gracefully on the Next.js client.
- Push state changes rather than polling.
- Group WebSocket connections by `run_id`.
EOF

# 5. Generate a JSON Manifest for the Web Platform to read these skills dynamically
echo "📜 Generating skills_manifest.json for the Next.js frontend & .NET backend..."
cat << 'EOF' > "$SKILLS_DIR/skills_manifest.json"
{
  "version": "1.0",
  "categories": {
    "ML_Research": {
      "description": "Core autonomous research and ML skills",
      "path": "ML_Research/"
    },
    "UI_UX_Web": {
      "description": "Frontend, UI/UX, and real-time streaming skills",
      "path": "UI_UX_Web/"
    }
  }
}
EOF

# Clean up
rm -rf "$TEMP_CLONE_DIR"

echo "✅ All skills have been downloaded and smartly packaged into '$SKILLS_DIR'."
echo "💡 The .NET Orchestrator and Next.js frontend can now load 'skills_manifest.json' to offer these skills directly in the application UI!"
