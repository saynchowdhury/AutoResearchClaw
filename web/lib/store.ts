import { create } from "zustand";

interface Project {
  id: string;
  name: string;
  description: string;
  globalContext: string;
}

interface Run {
  id: string;
  projectId: string;
  topic: string;
  status: string;
  currentStage: number;
}

interface Message {
  role: "user" | "assistant";
  content: string;
}

interface Store {
  projects: Project[];
  activeProject: Project | null;
  setActiveProject: (project: Project | null) => void;
  runs: Run[];
  activeRun: Run | null;
  setActiveRun: (run: Run | null) => void;
  messages: Message[];
  addMessage: (message: Message) => void;
  clearMessages: () => void;
  isConnected: boolean;
  setIsConnected: (connected: boolean) => void;
}

export const useStore = create<Store>((set) => ({
  projects: [
    {
      id: "default",
      name: "Default Project",
      description: "AutoResearchClaw default project",
      globalContext: "",
    },
  ],
  activeProject: {
    id: "default",
    name: "Default Project",
    description: "AutoResearchClaw default project",
    globalContext: "",
  },
  setActiveProject: (project) => set({ activeProject: project }),
  runs: [],
  activeRun: null,
  setActiveRun: (run) => set({ activeRun: run }),
  messages: [],
  addMessage: (message) => set((state) => ({ messages: [...state.messages, message] })),
  clearMessages: () => set({ messages: [] }),
  isConnected: false,
  setIsConnected: (connected) => set({ isConnected: connected }),
}));