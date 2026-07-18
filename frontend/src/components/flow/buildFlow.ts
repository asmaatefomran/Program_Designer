import type { Edge, Node as FlowNode } from "reactflow";
import type { BuilderNode } from "@/builder/tree";
import type { ValidationIssueResponse } from "@/api/types";
import { layoutTree } from "./layoutTree";
import type { ProgramFlowNodeData } from "./ProgramFlowNode";

const errorCodes = new Set([
  "SELF_DEPENDENCY",
  "CIRCULAR_DEPENDENCY",
  "FORWARD_PREREQUISITE",
  "PREREQUISITE_ON_ANCESTOR",
  "MISSING_PREREQUISITE",
  "INCONSISTENT_TEMPLATE",
  "INVALID_TREE",
  "INVALID_PARENT",
  "EMPTY_GROUP",
  "INVALID_REQUIRED_SELECTIONS",
]);

export function buildFlow(
  root: BuilderNode,
  issuesByTemplateId: Map<string, ValidationIssueResponse[]>,
): { nodes: FlowNode<ProgramFlowNodeData>[]; edges: Edge[] } {
  const positions = layoutTree(root);
  const nodes: FlowNode<ProgramFlowNodeData>[] = [];
  const edges: Edge[] = [];
  const keyByTemplateId = new Map<string, string>();

  function collectKeys(node: BuilderNode) {
    keyByTemplateId.set(node.templateId, node.key);
    if (node.kind === "group") node.children.forEach(collectKeys);
  }
  collectKeys(root);

  function visit(node: BuilderNode) {
    const pos = positions.get(node.key)!;
    const issues = issuesByTemplateId.get(node.templateId) ?? [];
    const status: ProgramFlowNodeData["status"] = issues.some((i) => errorCodes.has(i.code))
      ? "error"
      : issues.length > 0
        ? "warning"
        : "ok";

    nodes.push({
      id: node.key,
      type: "programNode",
      position: pos,
      data: {
        label: node.name,
        kind: node.kind,
        groupType: node.kind === "group" ? node.groupType : undefined,
        requiredChoiceCount: node.kind === "group" ? node.requiredChoiceCount : undefined,
        childCount: node.kind === "group" ? node.children.length : undefined,
        status,
      },
    });

    for (const prereqTemplateId of node.prerequisiteTemplateIds) {
      const sourceKey = keyByTemplateId.get(prereqTemplateId);
      if (sourceKey) {
        edges.push({
          id: `prereq-${sourceKey}-${node.key}`,
          source: sourceKey,
          target: node.key,
          style: { stroke: "#f59e0b", strokeDasharray: "4 3" },
          animated: true,
          label: "prerequisite",
          labelStyle: { fontSize: 10, fill: "#f59e0b" },
        });
      }
    }

    if (node.kind === "group") {
      for (const child of node.children) {
        edges.push({
          id: `contains-${node.key}-${child.key}`,
          source: node.key,
          target: child.key,
          style: { stroke: "var(--border)" },
        });
        visit(child);
      }
    }
  }

  visit(root);
  return { nodes, edges };
}
