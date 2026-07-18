import { Sparkles, Save, PlayCircle, RotateCcw, Loader2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

interface Props {
  programName: string;
  onProgramNameChange: (name: string) => void;
  onLoadExample: () => void;
  onReset: () => void;
  onCreateOnly: () => void;
  onCreateAndValidate: () => void;
  isSubmitting: boolean;
}

export function CreateToolbar({
  programName,
  onProgramNameChange,
  onLoadExample,
  onReset,
  onCreateOnly,
  onCreateAndValidate,
  isSubmitting,
}: Props) {
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

      <div className="ml-auto flex items-center gap-2">
        <Button variant="outline" size="sm" onClick={onCreateOnly} disabled={isSubmitting}>
          {isSubmitting ? <Loader2 className="size-3.5 animate-spin" /> : <Save className="size-3.5" />}
          Create only
        </Button>

        <Button size="sm" onClick={onCreateAndValidate} disabled={isSubmitting}>
          {isSubmitting ? (
            <Loader2 className="size-3.5 animate-spin" />
          ) : (
            <PlayCircle className="size-3.5" />
          )}
          Create &amp; validate
        </Button>
      </div>
    </div>
  );
}
