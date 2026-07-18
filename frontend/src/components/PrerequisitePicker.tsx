import { X } from "lucide-react";
import { Select } from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";

interface Option {
  templateId: string;
  name: string;
}

interface Props {
  value: string[];
  options: Option[];
  onChange: (next: string[]) => void;
}

export function PrerequisitePicker({ value, options, onChange }: Props) {
  const nameFor = (templateId: string) =>
    options.find((o) => o.templateId === templateId)?.name ?? templateId;

  const available = options.filter((o) => !value.includes(o.templateId));

  return (
    <div className="flex flex-wrap items-center gap-1.5">
      {value.map((templateId) => (
        <Badge key={templateId} variant="outline" className="gap-1 pr-1">
          {nameFor(templateId)}
          <button
            type="button"
            onClick={() => onChange(value.filter((v) => v !== templateId))}
            className="rounded-sm hover:bg-muted"
            aria-label={`Remove prerequisite ${nameFor(templateId)}`}
          >
            <X className="size-3" />
          </button>
        </Badge>
      ))}

      {available.length > 0 && (
        <Select
          value=""
          onChange={(e) => {
            if (e.target.value) onChange([...value, e.target.value]);
          }}
          className="h-6 w-auto text-xs"
        >
          <option value="">+ prerequisite</option>
          {available.map((o) => (
            <option key={o.templateId} value={o.templateId}>
              {o.name}
            </option>
          ))}
        </Select>
      )}
    </div>
  );
}
