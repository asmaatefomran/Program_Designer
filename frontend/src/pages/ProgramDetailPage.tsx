import { useMemo, useState } from "react";
import { useMutation, useQuery } from "@tanstack/react-query";
import { Link, useParams } from "react-router-dom";
import { ArrowLeft, CheckCircle2, Loader2, PlayCircle } from "lucide-react";
import { getProgram, simulateProgram, validateProgram } from "@/api/programs";
import type { SimulateRequest, SimulateResponse, ValidationResponse } from "@/api/types";
import { responseToBuilderNode, type BuilderGroup } from "@/builder/tree";
import { groupIssuesByTemplateId } from "@/builder/mapIssues";
import { ProgramFlow } from "@/components/flow/ProgramFlow";
import { ValidationPanel } from "@/components/ValidationPanel";
import { SimulatePanel } from "@/components/SimulatePanel";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

type Tab = "validate" | "simulate";

export function ProgramDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [tab, setTab] = useState<Tab>("validate");
  const [validation, setValidation] = useState<ValidationResponse | null>(null);
  const [simulation, setSimulation] = useState<SimulateResponse | null>(null);

  const {
    data: program,
    isLoading,
    isError,
    error,
  } = useQuery({
    queryKey: ["program", id],
    queryFn: () => getProgram(id!),
    enabled: !!id,
  });

  const validateMutation = useMutation({
    mutationFn: () => validateProgram(id!),
    onSuccess: setValidation,
  });

  const simulateMutation = useMutation({
    mutationFn: (request: SimulateRequest) => simulateProgram(id!, request),
    onSuccess: setSimulation,
  });

  const root = useMemo(
    () => (program ? (responseToBuilderNode(program.rootGroup) as BuilderGroup) : null),
    [program],
  );

  const issuesByTemplateId = useMemo(() => {
    if (!program || !validation) return new Map();
    return groupIssuesByTemplateId(program.rootGroup, validation);
  }, [program, validation]);

  return (
    <div className="flex min-h-0 flex-1 flex-col">
      <div className="flex items-center gap-3 border-b border-border bg-card px-4 py-3">
        <Link to="/programs">
          <Button variant="ghost" size="sm">
            <ArrowLeft className="size-3.5" /> Back
          </Button>
        </Link>

        {program && <span className="text-sm font-medium">{program.name}</span>}
        {id && <code className="text-xs text-muted-foreground">{id}</code>}

        <div className="ml-auto flex items-center gap-1 rounded-md bg-muted p-0.5">
          <TabButton active={tab === "validate"} onClick={() => setTab("validate")}>
            <CheckCircle2 className="size-3.5" /> Validate
          </TabButton>
          <TabButton active={tab === "simulate"} onClick={() => setTab("simulate")}>
            <PlayCircle className="size-3.5" /> Simulate
          </TabButton>
        </div>

        {tab === "validate" && (
          <Button
            size="sm"
            onClick={() => validateMutation.mutate()}
            disabled={!program || validateMutation.isPending}
          >
            {validateMutation.isPending ? (
              <Loader2 className="size-3.5 animate-spin" />
            ) : (
              <CheckCircle2 className="size-3.5" />
            )}
            Validate
          </Button>
        )}
      </div>

      {isLoading && (
        <div className="flex items-center gap-2 p-4 text-sm text-muted-foreground">
          <Loader2 className="size-4 animate-spin" /> Loading program...
        </div>
      )}

      {isError && (
        <div className="m-4 rounded-md border border-destructive/30 bg-destructive/10 p-3 text-sm text-destructive">
          {error instanceof Error ? error.message : "Program not found."}
        </div>
      )}

      {program && root && tab === "validate" && (
        <div className="grid min-h-0 flex-1 grid-cols-1 lg:grid-cols-[minmax(0,1fr)_320px]">
          <div className="min-h-0 border-r border-border">
            <ProgramFlow root={root} issuesByTemplateId={issuesByTemplateId} />
          </div>
          <div className="min-h-0 overflow-y-auto p-3">
            <ValidationPanel validation={validation} />
          </div>
        </div>
      )}

      {program && root && tab === "simulate" && (
        <div className="min-h-0 flex-1 overflow-y-auto p-4">
          <SimulatePanel
            root={root}
            result={simulation}
            isPending={simulateMutation.isPending}
            onRun={(request) => simulateMutation.mutate(request)}
          />
        </div>
      )}
    </div>
  );
}

function TabButton({
  active,
  onClick,
  children,
}: {
  active: boolean;
  onClick: () => void;
  children: React.ReactNode;
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={cn(
        "flex items-center gap-1.5 rounded-md px-2.5 py-1 text-xs font-medium transition-colors",
        active ? "bg-background shadow-sm" : "text-muted-foreground hover:text-foreground",
      )}
    >
      {children}
    </button>
  );
}
