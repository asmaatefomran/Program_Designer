import { useMemo } from "react";
import ReactFlow, { Background, Controls, type NodeTypes } from "reactflow";
import "reactflow/dist/style.css";
import type { BuilderNode } from "@/builder/tree";
import type { ValidationIssueResponse } from "@/api/types";
import { buildFlow } from "./buildFlow";
import { ProgramFlowNode } from "./ProgramFlowNode";

const nodeTypes: NodeTypes = { programNode: ProgramFlowNode };

interface Props {
  root: BuilderNode;
  issuesByTemplateId: Map<string, ValidationIssueResponse[]>;
}

export function ProgramFlow({ root, issuesByTemplateId }: Props) {
  // eslint-disable-next-line react-hooks/exhaustive-deps
  const { nodes, edges } = useMemo(() => buildFlow(root, issuesByTemplateId), [root, issuesByTemplateId]);

  return (
    <ReactFlow
      nodes={nodes}
      edges={edges}
      nodeTypes={nodeTypes}
      fitView
      fitViewOptions={{ padding: 0.2 }}
      proOptions={{ hideAttribution: true }}
    >
      <Background gap={20} />
      <Controls />
    </ReactFlow>
  );
}
