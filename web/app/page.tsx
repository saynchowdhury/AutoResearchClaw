"use client";

import { useState, useEffect, useRef } from "react";
import { useStore } from "@/lib/store";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Badge } from "@/components/ui/badge";
import { ScrollArea } from "@/components/ui/scroll-area";
import * as signalR from "@microsoft/signalr";
import {
  Terminal,
  MessageSquare,
  FileText,
  Play,
  Pause,
  RotateCcw,
  Settings,
  Send,
  Loader2,
} from "lucide-react";

interface EventMessage {
  type: "AgentThought" | "TerminalLog" | "StageChange" | "ArtifactUpdate";
  runId: string;
  thought?: string;
  log?: string;
  stage?: number;
  stageName?: string;
  filePath?: string;
  action?: string;
  timestamp: string;
}

export default function Home() {
  const {
    projects,
    activeProject,
    setActiveProject,
    runs,
    activeRun,
    setActiveRun,
    messages,
    addMessage,
    isConnected,
    setIsConnected,
  } = useStore();

  const [topic, setTopic] = useState("");
  const [isStarting, setIsStarting] = useState(false);
  const [terminalLogs, setTerminalLogs] = useState<string[]>([]);
  const [agentThoughts, setAgentThoughts] = useState<string[]>([]);
  const [currentStage, setCurrentStage] = useState(0);
  const [artifacts, setArtifacts] = useState<{ path: string; action: string }[]>([]);
  const terminalRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const hubUrl = "http://localhost:5000/events";
    
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect()
      .build();

    connection.on("AgentThought", (data: any) => handleEvent({ type: "AgentThought", thought: data.thought, ...data }));
    connection.on("TerminalLog", (data: any) => handleEvent({ type: "TerminalLog", log: data.log, ...data }));
    connection.on("StageChange", (data: any) => handleEvent({ type: "StageChange", stage: data.stage, stageName: data.stageName, ...data }));
    connection.on("ArtifactUpdate", (data: any) => handleEvent({ type: "ArtifactUpdate", filePath: data.filePath, action: data.action, ...data }));

    connection.start()
      .then(() => setIsConnected(true))
      .catch(err => console.error("SignalR Connection Error: ", err));

    connection.onclose(() => setIsConnected(false));
    connection.onreconnected(() => setIsConnected(true));

    return () => {
      connection.stop();
    };
  }, []);

  useEffect(() => {
    if (terminalRef.current) {
      terminalRef.current.scrollTop = terminalRef.current.scrollHeight;
    }
  }, [terminalLogs]);

  const handleEvent = (data: EventMessage) => {
    switch (data.type) {
      case "AgentThought":
        if (data.thought) setAgentThoughts((prev) => [...prev, data.thought!]);
        break;
      case "TerminalLog":
        if (data.log) setTerminalLogs((prev) => [...prev, data.log!]);
        break;
      case "StageChange":
        setCurrentStage(data.stage || 0);
        break;
      case "ArtifactUpdate":
        if (data.filePath && data.action) {
          setArtifacts((prev) => [...prev, { path: data.filePath!, action: data.action! }]);
        }
        break;
    }
  };

  const startRun = async () => {
    if (!topic.trim() || !activeProject) return;

    setIsStarting(true);
    setTerminalLogs([]);
    setAgentThoughts([]);
    setArtifacts([]);
    setCurrentStage(0);

    try {
      const response = await fetch("http://localhost:5000/api/runs/start", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ projectId: activeProject.id, topic }),
      });

      if (response.ok) {
        const run = await response.json();
        setActiveRun(run);
        addMessage({ role: "user", content: topic });
        setTopic("");
      }
    } catch (error) {
      console.error("Failed to start run:", error);
    } finally {
      setIsStarting(false);
    }
  };

  const stages = [
    "Topic Initialization",
    "Problem Decomposition",
    "Search Strategy",
    "Literature Collect",
    "Literature Screen",
    "Knowledge Extract",
    "Synthesis",
    "Hypothesis Generation",
    "Experiment Design",
    "Code Generation",
    "Resource Planning",
    "Experiment Run",
    "Iterative Refinement",
    "Result Analysis",
    "Research Decision",
    "Paper Outline",
    "Paper Draft",
    "Peer Review",
    "Paper Revision",
    "Quality Gate",
    "Knowledge Archive",
    "Export & Publish",
    "Citation Verify",
  ];

  return (
    <div className="flex h-screen overflow-hidden">
      {/* Left Pane - Chat & Control */}
      <div className="w-1/3 border-r border-border flex flex-col bg-card">
        <div className="p-4 border-b border-border">
          <h1 className="text-xl font-bold flex items-center gap-2">
            <span>🔬</span> AutoResearchClaw
          </h1>
        </div>

        <div className="p-4 border-b border-border">
          <div className="flex items-center justify-between mb-2">
            <span className="text-sm font-medium">Status</span>
            <Badge variant={isConnected ? "default" : "destructive"}>
              {isConnected ? "Connected" : "Disconnected"}
            </Badge>
          </div>
          <div className="flex items-center gap-2">
            <Button variant="outline" size="sm">
              <Settings className="w-4 h-4 mr-2" />
              Settings
            </Button>
          </div>
        </div>

        <div className="p-4 border-b border-border">
          <label className="text-sm font-medium mb-2 block">Research Topic</label>
          <div className="flex gap-2">
            <Input
              placeholder="Enter your research topic..."
              value={topic}
              onChange={(e) => setTopic(e.target.value)}
              onKeyDown={(e) => e.key === "Enter" && startRun()}
            />
            <Button onClick={startRun} disabled={!topic.trim() || isStarting}>
              {isStarting ? <Loader2 className="w-4 h-4 animate-spin" /> : <Play className="w-4 h-4" />}
            </Button>
          </div>
        </div>

        <ScrollArea className="flex-1 p-4">
          <div className="space-y-4">
            {messages.length === 0 && (
              <div className="text-center text-muted-foreground py-8">
                <MessageSquare className="w-8 h-8 mx-auto mb-2 opacity-50" />
                <p className="text-sm">Enter a research topic to start</p>
              </div>
            )}
            {messages.map((msg, i) => (
              <div
                key={i}
                className={`p-3 rounded-lg ${
                  msg.role === "user" ? "bg-primary text-primary-foreground" : "bg-muted"
                }`}
              >
                <p className="text-sm">{msg.content}</p>
              </div>
            ))}
          </div>
        </ScrollArea>
      </div>

      {/* Right Pane - Live Workspace */}
      <div className="w-2/3 flex flex-col">
        <div className="p-4 border-b border-border bg-card">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <span className="text-sm font-medium">Progress</span>
              <div className="flex items-center gap-1">
                {stages.slice(0, Math.min(currentStage + 1, 23)).map((stage, i) => (
                  <div
                    key={i}
                    className={`w-8 h-2 rounded ${
                      i < currentStage ? "bg-primary" : i === currentStage ? "bg-primary animate-pulse" : "bg-muted"
                    }`}
                  />
                ))}
              </div>
              <span className="text-sm text-muted-foreground">
                {currentStage > 0 ? stages[currentStage - 1] : "Queued"} ({currentStage}/23)
              </span>
            </div>
            <div className="flex gap-2">
              <Button variant="outline" size="sm">
                <Pause className="w-4 h-4" />
              </Button>
              <Button variant="outline" size="sm">
                <RotateCcw className="w-4 h-4" />
              </Button>
            </div>
          </div>
        </div>

        <Tabs defaultValue="terminal" className="flex-1 flex flex-col">
          <div className="px-4 pt-2">
            <TabsList>
              <TabsTrigger value="terminal">
                <Terminal className="w-4 h-4 mr-2" />
                Terminal
              </TabsTrigger>
              <TabsTrigger value="thoughts">
                <MessageSquare className="w-4 h-4 mr-2" />
                Thoughts
              </TabsTrigger>
              <TabsTrigger value="artifacts">
                <FileText className="w-4 h-4 mr-2" />
                Artifacts
              </TabsTrigger>
              <TabsTrigger value="skills">
                <Settings className="w-4 h-4 mr-2" />
                Skills
              </TabsTrigger>
            </TabsList>
          </div>

          <TabsContent value="terminal" className="flex-1 m-0">
            <div
              ref={terminalRef}
              className="h-full p-4 bg-black text-green-400 font-mono text-sm overflow-auto"
            >
              {terminalLogs.length === 0 ? (
                <span className="text-gray-600">Waiting for output...</span>
              ) : (
                terminalLogs.map((log, i) => (
                  <div key={i} className="whitespace-pre-wrap">
                    {log}
                  </div>
                ))
              )}
            </div>
          </TabsContent>

          <TabsContent value="thoughts" className="flex-1 m-0">
            <ScrollArea className="h-full p-4">
              <div className="space-y-2">
                {agentThoughts.length === 0 ? (
                  <span className="text-muted-foreground">No thoughts yet...</span>
                ) : (
                  agentThoughts.map((thought, i) => (
                    <div key={i} className="p-3 bg-muted rounded-lg">
                      <p className="text-sm">{thought}</p>
                    </div>
                  ))
                )}
              </div>
            </ScrollArea>
          </TabsContent>

          <TabsContent value="artifacts" className="flex-1 m-0">
            <ScrollArea className="h-full p-4">
              <div className="space-y-2">
                {artifacts.length === 0 ? (
                  <span className="text-muted-foreground">No artifacts yet...</span>
                ) : (
                  artifacts.map((artifact, i) => (
                    <div key={i} className="p-3 bg-muted rounded-lg flex items-center justify-between">
                      <span className="font-mono text-sm">{artifact.path}</span>
                      <Badge variant="outline">{artifact.action}</Badge>
                    </div>
                  ))
                )}
              </div>
            </ScrollArea>
          </TabsContent>

          <TabsContent value="skills" className="flex-1 m-0">
            <ScrollArea className="h-full p-4">
              <div className="space-y-4">
                <div className="p-4 border rounded-lg bg-card">
                  <h3 className="font-bold mb-2">ML & Research Skills</h3>
                  <p className="text-sm text-muted-foreground mb-4">Toggle skills to provide explicit context to the agent.</p>
                  <div className="space-y-2">
                    {["0-autoresearch-skill", "20-ml-paper-writing", "21-research-ideation", "11-evaluation", "16-prompt-engineering"].map((skill) => (
                      <div key={skill} className="flex items-center justify-between p-2 bg-muted rounded">
                        <span className="text-sm font-mono">{skill}</span>
                        <Badge variant="default" className="cursor-pointer">Enabled</Badge>
                      </div>
                    ))}
                  </div>
                </div>
                <div className="p-4 border rounded-lg bg-card">
                  <h3 className="font-bold mb-2">UI/UX Web Skills</h3>
                  <div className="space-y-2">
                    {["nextjs-best-practices", "ui-ux-design-guidelines", "signalr-realtime-patterns"].map((skill) => (
                      <div key={skill} className="flex items-center justify-between p-2 bg-muted rounded">
                        <span className="text-sm font-mono">{skill}</span>
                        <Badge variant="default" className="cursor-pointer">Enabled</Badge>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            </ScrollArea>
          </TabsContent>
        </Tabs>
      </div>
    </div>
  );
}