import { useMemo, useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { createProgram } from "@/api/programs";
import type { CreateProgramResponse, ValidationIssueResponse } from "@/api/types";
import {
  addChild,
  collectTemplates,
  removeNode,
  toGroupRequest,
  updateNode,
  validateTemplateIds,
  type BuilderGroup,
  type BuilderNode,
} from "@/builder/tree";
import { groupIssuesByTemplateId } from "@/builder/mapIssues";
import { buildComputerScienceScenario } from "@/data/computerScienceScenario";
import { NodeEditor } from "@/components/NodeEditor";
import { ProgramFlow } from "@/components/flow/ProgramFlow";
import { ValidationPanel } from "@/components/ValidationPanel";
import { CreateToolbar } from "@/components/CreateToolbar";

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

export function CreatePage() {
  const navigate = useNavigate();
  const [programName, setProgramName] = useState("New Program");
  const [root, setRoot] = useState<BuilderGroup>(emptyRoot);
  const [lastResult, setLastResult] = useState<CreateProgramResponse | null>(null);

  // Both buttons call the exact same POST /programs endpoint -- the backend
  // always computes and returns validation together in one response, so
  // there's no separate "create without validating" call to make. The two
  // buttons differ only in what happens after: "Create only" is for someone
  // who just wants to save and move on (redirects to the programs list),
  // "Create & validate" is for someone actively iterating on the tree and
  // wants to see the result right here.
  const createMutation = useMutation({ mutationFn: createProgram });

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

  function validateBeforeSubmit(): boolean {
    const validation = validateTemplateIds(root);
    if (!validation.valid) {
      alert(validation.errors.join("\n"));
      return false;
    }
    return true;
  }

  async function handleCreateOnly() {
    if (!validateBeforeSubmit()) return;

    await createMutation.mutateAsync({
      name: programName,
      rootGroup: toGroupRequest(root),
    });
    navigate("/programs");
  }

  async function handleCreateAndValidate() {
    if (!validateBeforeSubmit()) return;

    const result = await createMutation.mutateAsync({
      name: programName,
      rootGroup: toGroupRequest(root),
    });
    setLastResult(result);
  }

  return (
    <div className="flex min-h-0 flex-1 flex-col">
      <CreateToolbar
        programName={programName}
        onProgramNameChange={setProgramName}
        onLoadExample={handleLoadExample}
        onReset={handleReset}
        onCreateOnly={handleCreateOnly}
        onCreateAndValidate={handleCreateAndValidate}
        isSubmitting={createMutation.isPending}
      />

      {createMutation.error && (
        <div className="border-b border-destructive/30 bg-destructive/10 px-4 py-2 text-xs text-destructive">
          {createMutation.error instanceof Error
            ? createMutation.error.message
            : "Something went wrong talking to the API."}
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
