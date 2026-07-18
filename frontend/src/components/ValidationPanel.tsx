import { CheckCircle2, AlertCircle, AlertTriangle } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { ValidationResponse } from "@/api/types";

export function ValidationPanel({ validation }: { validation: ValidationResponse | null }) {
  if (!validation) {
    return (
      <Card>
        <CardContent className="pt-4 text-sm text-muted-foreground">
          Create the program to see validation results here.
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader className="flex-row items-center gap-2">
        {validation.isValid ? (
          <CheckCircle2 className="size-4 text-emerald-500" />
        ) : (
          <AlertCircle className="size-4 text-destructive" />
        )}
        <CardTitle>{validation.isValid ? "Valid" : "Invalid"}</CardTitle>
      </CardHeader>
      <CardContent className="flex flex-col gap-3">
        {validation.errors.length > 0 && (
          <div className="flex flex-col gap-1.5">
            <div className="text-xs font-semibold text-destructive">
              Errors ({validation.errors.length})
            </div>
            {validation.errors.map((e, i) => (
              <div key={i} className="flex items-start gap-1.5 text-xs text-destructive">
                <AlertCircle className="mt-0.5 size-3 shrink-0" />
                <span>
                  <code className="rounded bg-destructive/10 px-1 py-0.5">{e.code}</code> {e.message}
                </span>
              </div>
            ))}
          </div>
        )}

        {validation.warnings.length > 0 && (
          <div className="flex flex-col gap-1.5">
            <div className="text-xs font-semibold text-amber-600 dark:text-amber-400">
              Warnings ({validation.warnings.length})
            </div>
            {validation.warnings.map((w, i) => (
              <div
                key={i}
                className="flex items-start gap-1.5 text-xs text-amber-600 dark:text-amber-400"
              >
                <AlertTriangle className="mt-0.5 size-3 shrink-0" />
                <span>
                  <code className="rounded bg-amber-500/10 px-1 py-0.5">{w.code}</code> {w.message}
                </span>
              </div>
            ))}
          </div>
        )}

        {validation.errors.length === 0 && validation.warnings.length === 0 && (
          <div className="text-xs text-muted-foreground">
            No errors or warnings — every prerequisite is guaranteed reachable.
          </div>
        )}
      </CardContent>
    </Card>
  );
}
