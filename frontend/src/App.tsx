import { useMemo, useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { createProgram, getProgram, validateProgram } from "@/api/programs";
import type { CreateProgramResponse, ValidationIssueResponse } from "@/api/types";
import {
  addChild,
  collectTemplates,
  removeNode,
  responseToBuilderNode,
  toGroupRequest,
  updateNode,
  type BuilderGroup,
  type BuilderNode,
} from "@/builder/tree";
import { groupIssuesByTemplateId } from "@/builder/mapIssues";
import { buildComputerScienceScenario } from "@/data/computerScienceScenario";
import { NodeEditor } from "@/components/NodeEditor";
import { ProgramFlow } from "@/components/flow/ProgramFlow";
import { ValidationPanel } from "@/components/ValidationPanel";
import { Toolbar } from "@/components/Toolbar";

function emptyRoot(): BuilderGroup {
  return {
    kind: "group",
    key: "root",
    templateId: "root",
    name: "Root",
    groupType: "All",
    requiredChoiceCount: 0,
    prerequisiteTemplateIds: [],
    children: [],
  };
}

export default function App() {
  const [programName, setProgramName] = useState("New Program");
  const [root, setRoot] = useState<BuilderGroup>(emptyRoot);
  const [lastResult, setLastResult] = useState<CreateProgramResponse | null>(null);

  const createMutation = useMutation({
    mutationFn: createProgram,
    onSuccess: (result) => {
      setLastResult(result);
      localStorage.setItem("lastProgramId", result.program.id);
    },
  });

  const loadMutation = useMutation({
    mutationFn: async (id: string) => {
      const [program, validation] = await Promise.all([
        getProgram(id),
        validateProgram(id),
      ]);
      return { program, validation };
    },
    onSuccess: (result) => {
      setLastResult(result);
      setProgramName(result.program.name);
      setRoot(responseToBuilderNode(result.program.rootGroup) as BuilderGroup);
    },
  });

  const allTemplates = useMemo(() => collectTemplates(root), [root]);

  const issuesByTemplateId = useMemo(() => {
    if (!lastResult) return new Map<string, ValidationIssueResponse[]>();
    return groupIssuesByTemplateId(lastResult.program.rootGroup, lastResult.validation);
  }, [lastResult]);

  function handleUpdate(key: string, patch: Partial<BuilderNode>) {
    setRoot(updateNode(root, key, (n) => ({ ...n, ...patch }) as BuilderNode));
  }

  function handleRemove(key: string) {
    setRoot(removeNode(root, key));
  }

  function handleAddChild(parentKey: string, child: BuilderNode) {
    setRoot(addChild(root, parentKey, child));
  }

  function handleLoadExample() {
    setRoot(buildComputerScienceScenario());
    setProgramName("Computer Science");
    setLastResult(null);
  }

  function handleReset() {
    setRoot(emptyRoot());
    setProgramName("New Program");
    setLastResult(null);
  }

  function handleCreateAndValidate() {
    createMutation.mutate({ name: programName, rootGroup: toGroupRequest(root) });
  }

  const error = createMutation.error ?? loadMutation.error;

  return (
    <div className="flex h-screen flex-col bg-background text-foreground">
      <header className="border-b border-border px-4 py-3">
        <h1 className="text-lg font-semibold">Program Designer</h1>
        <p className="text-xs text-muted-foreground">
          Build a learning program, then create &amp; validate it against the API.
        </p>
      </header>

      <Toolbar
        programName={programName}
        onProgramNameChange={setProgramName}
        onLoadExample={handleLoadExample}
        onReset={handleReset}
        onCreateAndValidate={handleCreateAndValidate}
        onLoadById={(id) => loadMutation.mutate(id)}
        isSubmitting={createMutation.isPending || loadMutation.isPending}
        lastProgramId={lastResult?.program.id ?? null}
      />

      {error && (
        <div className="border-b border-destructive/30 bg-destructive/10 px-4 py-2 text-xs text-destructive">
          {error instanceof Error ? error.message : "Something went wrong talking to the API."}
        </div>
      )}

      <div className="grid min-h-0 flex-1 grid-cols-1 gap-0 lg:grid-cols-[minmax(0,1fr)_minmax(0,1fr)_320px]">
        <div className="min-h-0 overflow-y-auto border-r border-border p-3">
          <NodeEditor
            node={root}
            depth={0}
            allTemplates={allTemplates}
            issuesByTemplateId={issuesByTemplateId}
            onUpdate={handleUpdate}
            onRemove={handleRemove}
            onAddChild={handleAddChild}
          />
        </div>

        <div className="min-h-0 border-r border-border">
          <ProgramFlow root={root} issuesByTemplateId={issuesByTemplateId} />
        </div>

        <div className="min-h-0 overflow-y-auto p-3">
          <ValidationPanel validation={lastResult?.validation ?? null} />
        </div>
      </div>
    </div>
  );
}
