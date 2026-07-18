import { Plus, Trash2, ListOrdered, GitBranch, FileText, AlertTriangle, AlertCircle } from "lucide-react";
import { Input } from "@/components/ui/input";
import { Select } from "@/components/ui/select";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { PrerequisitePicker } from "@/components/PrerequisitePicker";
import {
  createGroup,
  createStep,
  type BuilderNode,
} from "@/builder/tree";
import type { ValidationIssueResponse } from "@/api/types";

interface Props {
  node: BuilderNode;
  depth: number;
  allTemplates: { templateId: string; name: string }[];
  issuesByTemplateId: Map<string, ValidationIssueResponse[]>;
  onUpdate: (key: string, patch: Partial<BuilderNode>) => void;
  onRemove: (key: string) => void;
  onAddChild: (parentKey: string, child: BuilderNode) => void;
}

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

export function NodeEditor({
  node,
  depth,
  allTemplates,
  issuesByTemplateId,
  onUpdate,
  onRemove,
  onAddChild,
}: Props) {
  const issues = issuesByTemplateId.get(node.templateId) ?? [];
  const hasError = issues.some((i) => errorCodes.has(i.code));
  const hasWarning = issues.length > 0 && !hasError;

  const availablePrereqs = allTemplates.filter((t) => t.templateId !== node.templateId);

  return (
    <div
      className="rounded-lg border bg-card"
      style={{
        marginLeft: depth === 0 ? 0 : 16,
        borderColor: hasError
          ? "var(--destructive)"
          : hasWarning
            ? "color-mix(in oklch, orange 60%, transparent)"
            : "var(--border)",
      }}
    >
      <div className="flex flex-wrap items-center gap-2 p-2">
        {node.kind === "group" ? (
          <GitBranch className="size-4 shrink-0 text-muted-foreground" />
        ) : (
          <FileText className="size-4 shrink-0 text-muted-foreground" />
        )}

        <Input
          value={node.name}
          onChange={(e) => onUpdate(node.key, { name: e.target.value })}
          className="w-48"
        />

        <Badge variant={node.kind === "group" ? "default" : "outline"}>
          {node.kind === "group" ? node.groupType : "step"}
        </Badge>

        {node.kind === "group" && node.groupType === "Choice" && (
          <label className="flex items-center gap-1 text-xs text-muted-foreground">
            pick
            <Input
              type="number"
              min={0}
              max={node.children.length}
              value={node.requiredChoiceCount}
              onChange={(e) =>
                onUpdate(node.key, { requiredChoiceCount: Number(e.target.value) })
              }
              className="h-7 w-14"
            />
            of {node.children.length}
          </label>
        )}

        {node.kind === "group" && (
          <Select
            value={node.groupType}
            onChange={(e) =>
              onUpdate(node.key, { groupType: e.target.value as "All" | "Choice" })
            }
          >
            <option value="All">In order (All)</option>
            <option value="Choice">Choice</option>
          </Select>
        )}

        <div className="ml-auto flex items-center gap-1">
          {node.kind === "group" && (
            <>
              <Button
                variant="ghost"
                size="sm"
                onClick={() => onAddChild(node.key, createStep())}
                title="Add step"
              >
                <Plus className="size-3.5" /> Step
              </Button>
              <Button
                variant="ghost"
                size="sm"
                onClick={() => onAddChild(node.key, createGroup())}
                title="Add group"
              >
                <ListOrdered className="size-3.5" /> Group
              </Button>
            </>
          )}
          {depth > 0 && (
            <Button
              variant="ghost"
              size="sm"
              onClick={() => onRemove(node.key)}
              title="Delete"
            >
              <Trash2 className="size-3.5 text-destructive" />
            </Button>
          )}
        </div>
      </div>

      <div className="flex items-center gap-2 border-t border-border/60 px-2 py-1.5">
        <span className="shrink-0 text-xs text-muted-foreground">prerequisites:</span>
        <PrerequisitePicker
          value={node.prerequisiteTemplateIds}
          options={availablePrereqs}
          onChange={(next) => onUpdate(node.key, { prerequisiteTemplateIds: next })}
        />
      </div>

      {issues.length > 0 && (
        <div className="flex flex-col gap-1 border-t border-border/60 px-2 py-1.5">
          {issues.map((issue, i) => (
            <div
              key={i}
              className={`flex items-start gap-1.5 text-xs ${
                errorCodes.has(issue.code) ? "text-destructive" : "text-amber-600 dark:text-amber-400"
              }`}
            >
              {errorCodes.has(issue.code) ? (
                <AlertCircle className="mt-0.5 size-3 shrink-0" />
              ) : (
                <AlertTriangle className="mt-0.5 size-3 shrink-0" />
              )}
              {issue.message}
            </div>
          ))}
        </div>
      )}

      {node.kind === "group" && node.children.length > 0 && (
        <div className="flex flex-col gap-2 border-t border-border/60 p-2">
          {node.children.map((child) => (
            <NodeEditor
              key={child.key}
              node={child}
              depth={depth + 1}
              allTemplates={allTemplates}
              issuesByTemplateId={issuesByTemplateId}
              onUpdate={onUpdate}
              onRemove={onRemove}
              onAddChild={onAddChild}
            />
          ))}
        </div>
      )}

      {node.kind === "group" && node.children.length === 0 && (
        <div className="border-t border-border/60 p-2 text-xs text-muted-foreground">
          Empty group — add a step or group above.
        </div>
      )}
    </div>
  );
}
