import type { GroupRequest, GroupType, NodeRequest, NodeResponse } from "@/api/types";

// The tree the editor works with. Nearly identical to NodeRequest, but every
// node also carries a `key` -- a stable React key independent of templateId,
// since templateId is user-editable text and clones can intentionally repeat
// it (see Node.TemplateId on the backend). The key is UI-only and never sent
// to the API.

export interface BuilderStep {
  kind: "step";
  key: string;
  templateId: string;
  name: string;
  prerequisiteTemplateIds: string[];
}

export interface BuilderGroup {
  kind: "group";
  key: string;
  templateId: string;
  name: string;
  groupType: GroupType;
  requiredChoiceCount: number;
  prerequisiteTemplateIds: string[];
  children: BuilderNode[];
}

export type BuilderNode = BuilderStep | BuilderGroup;

let keyCounter = 0;
function nextKey(): string {
  keyCounter += 1;
  return `node-${keyCounter}-${Date.now().toString(36)}`;
}


export function createStep(name = "New step"): BuilderStep {
  return {
    kind: "step",
    key: nextKey(),
    templateId: "",
    name,
    prerequisiteTemplateIds: [],
  };
}

export function createGroup(name = "New group"): BuilderGroup {
  return {
    kind: "group",
    key: nextKey(),
    templateId: "",
    name,
    groupType: "All",
    requiredChoiceCount: 0,
    prerequisiteTemplateIds: [],
    children: [],
  };
}

// ---- Immutable tree operations, all keyed by the UI-only `key` ----

export function mapNode(
  node: BuilderNode,
  fn: (n: BuilderNode) => BuilderNode,
): BuilderNode {
  const mapped = fn(node);
  if (mapped.kind !== "group") return mapped;
  return { ...mapped, children: mapped.children.map((c) => mapNode(c, fn)) };
}

export function updateNode(
  root: BuilderGroup,
  key: string,
  update: (n: BuilderNode) => BuilderNode,
): BuilderGroup {
  return mapNode(root, (n) => (n.key === key ? update(n) : n)) as BuilderGroup;
}

export function removeNode(root: BuilderGroup, key: string): BuilderGroup {
  function recurse(node: BuilderGroup): BuilderGroup {
    return {
      ...node,
      children: node.children
        .filter((c) => c.key !== key)
        .map((c) => (c.kind === "group" ? recurse(c) : c)),
    };
  }
  return recurse(root);
}

export function addChild(
  root: BuilderGroup,
  parentKey: string,
  child: BuilderNode,
): BuilderGroup {
  function recurse(node: BuilderGroup): BuilderGroup {
    if (node.key === parentKey) {
      return { ...node, children: [...node.children, child] };
    }
    return {
      ...node,
      children: node.children.map((c) =>
        c.kind === "group" ? recurse(c) : c,
      ),
    };
  }
  return recurse(root);
}

export function findNode(root: BuilderNode, key: string): BuilderNode | null {
  if (root.key === key) return root;
  if (root.kind !== "group") return null;
  for (const child of root.children) {
    const found = findNode(child, key);
    if (found) return found;
  }
  return null;
}

/** All (templateId, name) pairs in the tree, in document order -- used to populate prerequisite pickers. */
export function collectTemplates(
  root: BuilderNode,
): { templateId: string; name: string }[] {
  const result: { templateId: string; name: string }[] = [];
  function recurse(node: BuilderNode) {
    result.push({ templateId: node.templateId, name: node.name });
    if (node.kind === "group") {
      node.children.forEach(recurse);
    }
  }
  recurse(root);
  return result;
}

export function toNodeRequest(node: BuilderNode): NodeRequest {
  if (node.kind === "step") {
    return {
      type: "step",
      templateId: node.templateId,
      name: node.name,
      prerequisiteTemplateIds: node.prerequisiteTemplateIds,
    };
  }
  return {
    type: "group",
    templateId: node.templateId,
    name: node.name,
    groupType: node.groupType,
    requiredChoiceCount: node.requiredChoiceCount,
    prerequisiteTemplateIds: node.prerequisiteTemplateIds,
    children: node.children.map(toNodeRequest),
  };
}

export function toGroupRequest(root: BuilderGroup): GroupRequest {
  return toNodeRequest(root) as GroupRequest;
}

/** Converts a fetched NodeResponse tree back into an editable builder tree (used by "load by id"). */
export function responseToBuilderNode(node: NodeResponse): BuilderNode {
  if (node.type === "step") {
    return {
      kind: "step",
      key: node.id,
      templateId: node.templateId,
      name: node.name,
      prerequisiteTemplateIds: node.prerequisiteTemplateIds,
    };
  }
  return {
    kind: "group",
    key: node.id,
    templateId: node.templateId,
    name: node.name,
    groupType: node.groupType,
    requiredChoiceCount: node.requiredSelections ?? 0,
    prerequisiteTemplateIds: node.prerequisiteTemplateIds,
    children: node.children.map(responseToBuilderNode),
  };
}

export interface TemplateIdValidation {
  valid: boolean;
  errors: string[];
}

export function validateTemplateIds(
    root: BuilderNode,
): TemplateIdValidation {
  const errors: string[] = [];

  function visit(node: BuilderNode) {
    if (!node.templateId.trim()) {
      errors.push(`"${node.name}" is missing a Template ID.`);
    }

    if (node.kind === "group") {
      node.children.forEach(visit);
    }
  }

  visit(root);

  return {
    valid: errors.length === 0,
    errors,
  };
}
