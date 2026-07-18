import { useMemo, useState } from "react";
import { CheckCircle2, Circle, Lock, Loader2, Play } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { BuilderGroup, BuilderNode } from "@/builder/tree";
import type { SimulateRequest, SimulateResponse } from "@/api/types";

interface ChoiceGroupInfo {
  templateId: string;
  name: string;
  requiredChoiceCount: number;
  options: { templateId: string; name: string }[];
}

interface StepInfo {
  templateId: string;
  name: string;
}

function collectChoiceGroups(node: BuilderNode, out: ChoiceGroupInfo[] = []): ChoiceGroupInfo[] {
  if (node.kind === "group") {
    if (node.groupType === "Choice") {
      out.push({
        templateId: node.templateId,
        name: node.name,
        requiredChoiceCount: node.requiredChoiceCount,
        options: node.children.map((c) => ({ templateId: c.templateId, name: c.name })),
      });
    }
    node.children.forEach((c) => collectChoiceGroups(c, out));
  }
  return out;
}

function collectSteps(node: BuilderNode, out: StepInfo[] = []): StepInfo[] {
  if (node.kind === "step") {
    out.push({ templateId: node.templateId, name: node.name });
  } else {
    node.children.forEach((c) => collectSteps(c, out));
  }
  return out;
}

interface Props {
  root: BuilderGroup;
  result: SimulateResponse | null;
  isPending: boolean;
  onRun: (request: SimulateRequest) => void;
}

export function SimulatePanel({ root, result, isPending, onRun }: Props) {
  const choiceGroups = useMemo(() => collectChoiceGroups(root), [root]);
  const steps = useMemo(() => collectSteps(root), [root]);

  const [choices, setChoices] = useState<Record<string, string[]>>({});
  const [completedStepTemplateIds, setCompletedStepTemplateIds] = useState<string[]>([]);

  function toggleChoice(groupTemplateId: string, optionTemplateId: string, max: number) {
    setChoices((prev) => {
      const current = prev[groupTemplateId] ?? [];
      if (current.includes(optionTemplateId)) {
        return { ...prev, [groupTemplateId]: current.filter((id) => id !== optionTemplateId) };
      }
      if (current.length >= max) return prev; // already at the pick-N limit
      return { ...prev, [groupTemplateId]: [...current, optionTemplateId] };
    });
  }

  function toggleCompleted(templateId: string) {
    setCompletedStepTemplateIds((prev) =>
      prev.includes(templateId) ? prev.filter((id) => id !== templateId) : [...prev, templateId],
    );
  }

  return (
    <div className="flex flex-col gap-3">
      <Card>
        <CardHeader>
          <CardTitle>Choices</CardTitle>
        </CardHeader>
        <CardContent className="flex flex-col gap-3">
          {choiceGroups.length === 0 && (
            <p className="text-xs text-muted-foreground">No choice groups in this program.</p>
          )}
          {choiceGroups.map((group) => (
            <div key={group.templateId}>
              <div className="mb-1 text-xs font-medium">
                {group.name}{" "}
                <span className="text-muted-foreground">
                  (pick {group.requiredChoiceCount} of {group.options.length})
                </span>
              </div>
              <div className="flex flex-wrap gap-1.5">
                {group.options.map((option) => {
                  const selected = (choices[group.templateId] ?? []).includes(option.templateId);
                  return (
                    <button
                      key={option.templateId}
                      type="button"
                      onClick={() =>
                        toggleChoice(group.templateId, option.templateId, group.requiredChoiceCount)
                      }
                      className={`rounded-md border px-2 py-1 text-xs transition-colors ${
                        selected
                          ? "border-primary bg-primary/10 text-primary"
                          : "border-border text-muted-foreground hover:bg-muted"
                      }`}
                    >
                      {option.name}
                    </button>
                  );
                })}
              </div>
            </div>
          ))}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Already completed</CardTitle>
        </CardHeader>
        <CardContent className="flex flex-wrap gap-1.5">
          {steps.map((step) => {
            const selected = completedStepTemplateIds.includes(step.templateId);
            return (
              <button
                key={step.templateId}
                type="button"
                onClick={() => toggleCompleted(step.templateId)}
                className={`rounded-md border px-2 py-1 text-xs transition-colors ${
                  selected
                    ? "border-emerald-500 bg-emerald-500/10 text-emerald-700 dark:text-emerald-400"
                    : "border-border text-muted-foreground hover:bg-muted"
                }`}
              >
                {step.name}
              </button>
            );
          })}
        </CardContent>
      </Card>

      <Button
        onClick={() => onRun({ choices, completedStepTemplateIds })}
        disabled={isPending}
        className="self-start"
      >
        {isPending ? <Loader2 className="size-3.5 animate-spin" /> : <Play className="size-3.5" />}
        Run simulation
      </Button>

      {result && (
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
          <ResultColumn
            title="Complete"
            icon={<CheckCircle2 className="size-3.5 text-emerald-500" />}
            items={result.complete}
          />
          <ResultColumn
            title="Unlocked"
            icon={<Circle className="size-3.5 text-blue-500" />}
            items={result.unlocked}
          />
          <ResultColumn
            title="Blocked"
            icon={<Lock className="size-3.5 text-amber-500" />}
            items={result.blocked}
          />
        </div>
      )}
    </div>
  );
}

function ResultColumn({
  title,
  icon,
  items,
}: {
  title: string;
  icon: React.ReactNode;
  items: { name: string; reason: string | null }[];
}) {
  return (
    <Card>
      <CardHeader className="flex-row items-center gap-1.5">
        {icon}
        <CardTitle>
          {title} ({items.length})
        </CardTitle>
      </CardHeader>
      <CardContent className="flex flex-col gap-1.5">
        {items.length === 0 && <span className="text-xs text-muted-foreground">None</span>}
        {items.map((item, i) => (
          <div key={i} className="text-xs">
            <div className="font-medium">{item.name}</div>
            {item.reason && <div className="text-muted-foreground">{item.reason}</div>}
          </div>
        ))}
      </CardContent>
    </Card>
  );
}
