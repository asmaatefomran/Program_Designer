import { useState } from "react";
import { Sparkles, PlayCircle, RotateCcw, Search, Loader2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

interface Props {
  programName: string;
  onProgramNameChange: (name: string) => void;
  onLoadExample: () => void;
  onReset: () => void;
  onCreateAndValidate: () => void;
  onLoadById: (id: string) => void;
  isSubmitting: boolean;
  lastProgramId: string | null;
}

export function Toolbar({
  programName,
  onProgramNameChange,
  onLoadExample,
  onReset,
  onCreateAndValidate,
  onLoadById,
  isSubmitting,
  lastProgramId,
}: Props) {
  const [lookupId, setLookupId] = useState("");

  return (
    <div className="flex flex-wrap items-center gap-2 border-b border-border bg-card px-4 py-3">
      <Input
        value={programName}
        onChange={(e) => onProgramNameChange(e.target.value)}
        placeholder="Program name"
        className="w-56"
      />

      <Button variant="outline" size="sm" onClick={onLoadExample}>
        <Sparkles className="size-3.5" /> Load CS example
      </Button>

      <Button variant="ghost" size="sm" onClick={onReset}>
        <RotateCcw className="size-3.5" /> Reset
      </Button>

      <Button size="sm" onClick={onCreateAndValidate} disabled={isSubmitting} className="ml-auto">
        {isSubmitting ? (
          <Loader2 className="size-3.5 animate-spin" />
        ) : (
          <PlayCircle className="size-3.5" />
        )}
        Create &amp; validate
      </Button>

      <div className="flex items-center gap-1.5">
        <Input
          value={lookupId}
          onChange={(e) => setLookupId(e.target.value)}
          placeholder="Program id"
          className="w-40"
        />
        <Button
          variant="outline"
          size="sm"
          onClick={() => lookupId && onLoadById(lookupId)}
          title="Fetch and validate an existing program by id"
        >
          <Search className="size-3.5" />
        </Button>
      </div>

      {lastProgramId && (
        <span className="w-full text-xs text-muted-foreground sm:w-auto">
          Last created: <code className="rounded bg-muted px-1 py-0.5">{lastProgramId}</code>
        </span>
      )}
    </div>
  );
}
