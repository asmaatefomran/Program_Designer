import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { ChevronLeft, ChevronRight, FileStack, Loader2 } from "lucide-react";
import { getPrograms } from "@/api/programs";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

const PAGE_SIZE = 10;

export function ProgramsListPage() {
  const [page, setPage] = useState(1);

  const { data, isLoading, isError, error } = useQuery({
    queryKey: ["programs", page],
    queryFn: () => getPrograms(page, PAGE_SIZE),
  });

  return (
    <div className="mx-auto w-full max-w-2xl flex-1 p-6">
      <div className="mb-4 flex items-center gap-2">
        <FileStack className="size-5 text-muted-foreground" />
        <h2 className="text-base font-semibold">All programs</h2>
      </div>

      {isLoading && (
        <div className="flex items-center gap-2 text-sm text-muted-foreground">
          <Loader2 className="size-4 animate-spin" /> Loading...
        </div>
      )}

      {isError && (
        <div className="rounded-md border border-destructive/30 bg-destructive/10 p-3 text-sm text-destructive">
          {error instanceof Error ? error.message : "Failed to load programs."}
        </div>
      )}

      {data && data.items.length === 0 && (
        <Card>
          <CardContent className="pt-4 text-sm text-muted-foreground">
            No programs yet — create one on the Create page.
          </CardContent>
        </Card>
      )}

      {data && data.items.length > 0 && (
        <div className="flex flex-col gap-2">
          {data.items.map((program) => (
            <Link key={program.id} to={`/programs/${program.id}`}>
              <Card className="transition-colors hover:bg-muted/50">
                <CardContent className="flex items-center justify-between gap-2 py-3">
                  <div className="min-w-0">
                    <div className="truncate text-sm font-medium">{program.name}</div>
                    <div className="text-xs text-muted-foreground">
                      {new Date(program.createdAt).toLocaleString()}
                    </div>
                  </div>
                  <code className="shrink-0 rounded bg-muted px-1.5 py-0.5 text-[10px] text-muted-foreground">
                    {program.id.slice(0, 8)}
                  </code>
                </CardContent>
              </Card>
            </Link>
          ))}
        </div>
      )}

      {data && data.totalPages > 1 && (
        <div className="mt-4 flex items-center justify-center gap-3">
          <Button
            variant="outline"
            size="sm"
            disabled={page <= 1}
            onClick={() => setPage((p) => p - 1)}
          >
            <ChevronLeft className="size-3.5" /> Prev
          </Button>
          <span className="text-xs text-muted-foreground">
            Page {data.page} of {data.totalPages} ({data.totalCount} total)
          </span>
          <Button
            variant="outline"
            size="sm"
            disabled={page >= data.totalPages}
            onClick={() => setPage((p) => p + 1)}
          >
            Next <ChevronRight className="size-3.5" />
          </Button>
        </div>
      )}
    </div>
  );
}
