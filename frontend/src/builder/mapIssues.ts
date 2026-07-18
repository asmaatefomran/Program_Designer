import type { NodeResponse, ValidationIssueResponse, ValidationResponse } from "@/api/types";

/** Builds a physical-id -> templateId map by walking a response tree. */
function collectIdToTemplateId(root: NodeResponse): Map<string, string> {
  const map = new Map<string, string>();
  function visit(node: NodeResponse) {
    map.set(node.id, node.templateId);
    if (node.type === "group") node.children.forEach(visit);
  }
  visit(root);
  return map;
}

/**
 * Groups a ValidationResponse's errors + warnings by templateId, so the
 * builder UI (which only knows templateId, not the server-assigned physical
 * id) can highlight the right node.
 */
export function groupIssuesByTemplateId(
  rootGroup: NodeResponse,
  validation: ValidationResponse,
): Map<string, ValidationIssueResponse[]> {
  const idToTemplateId = collectIdToTemplateId(rootGroup);
  const grouped = new Map<string, ValidationIssueResponse[]>();

  for (const issue of [...validation.errors, ...validation.warnings]) {
    const templateId = idToTemplateId.get(issue.nodeId);
    if (!templateId) continue;
    const existing = grouped.get(templateId) ?? [];
    existing.push(issue);
    grouped.set(templateId, existing);
  }

  return grouped;
}
