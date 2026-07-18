import { NavLink } from "react-router-dom";
import { cn } from "@/lib/utils";

const linkClass = ({ isActive }: { isActive: boolean }) =>
  cn(
    "rounded-md px-3 py-1.5 text-sm font-medium transition-colors",
    isActive
      ? "bg-secondary text-secondary-foreground"
      : "text-muted-foreground hover:bg-muted hover:text-foreground",
  );

export function NavBar() {
  return (
    <header className="flex items-center gap-4 border-b border-border px-4 py-3">
      <div>
        <h1 className="text-lg font-semibold leading-none">Program Designer</h1>
        <p className="text-xs text-muted-foreground">
          Build, save, and validate learning programs.
        </p>
      </div>

      <nav className="ml-auto flex items-center gap-1">
        <NavLink to="/" end className={linkClass}>
          Create
        </NavLink>
        <NavLink to="/programs" className={linkClass}>
          All programs
        </NavLink>
      </nav>
    </header>
  );
}
