import { Handle, Position } from "reactflow";
import { FileText, GitBranch, ListOrdered } from "lucide-react";
import { cn } from "@/lib/utils";
import type { GroupType } from "@/api/types";

export interface ProgramFlowNodeData {
  label: string;
  kind: "step" | "group";
  groupType?: GroupType;
  requiredChoiceCount?: number;
  childCount?: number;
  status: "ok" | "warning" | "error";
}

export function ProgramFlowNode({ data }: { data: ProgramFlowNodeData }) {
  const borderClass =
    data.status === "error"
      ? "border-destructive"
      : data.status === "warning"
        ? "border-amber-500"
        : "border-border";

  return (
    <div
      className={cn(
        "min-w-[180px] rounded-lg border-2 bg-card px-3 py-2 shadow-sm",
        borderClass,
      )}
    >
      <Handle type="target" position={Position.Top} className="!bg-muted-foreground" />

      <div className="flex items-center gap-1.5 text-xs font-medium">
        {data.kind === "group" ? (
          data.groupType === "Choice" ? (
            <GitBranch className="size-3.5 text-violet-500" />
          ) : (
            <ListOrdered className="size-3.5 text-blue-500" />
          )
        ) : (
          <FileText className="size-3.5 text-muted-foreground" />
        )}
        <span className="truncate">{data.label}</span>
      </div>

      {data.kind === "group" && (
        <div className="mt-1 text-[10px] text-muted-foreground">
          {data.groupType === "Choice"
            ? `pick ${data.requiredChoiceCount} of ${data.childCount}`
            : "in order"}
        </div>
      )}

      <Handle type="source" position={Position.Bottom} className="!bg-muted-foreground" />
    </div>
  );
}
