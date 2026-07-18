import { BrowserRouter, Route, Routes } from "react-router-dom";
import { NavBar } from "@/components/NavBar";
import { CreatePage } from "@/pages/CreatePage";
import { ProgramsListPage } from "@/pages/ProgramsListPage";
import { ProgramDetailPage } from "@/pages/ProgramDetailPage";

export default function App() {
  return (
    <BrowserRouter>
      <div className="flex h-screen flex-col bg-background text-foreground">
        <NavBar />
        <Routes>
          <Route path="/" element={<CreatePage />} />
          <Route path="/programs" element={<ProgramsListPage />} />
          <Route path="/programs/:id" element={<ProgramDetailPage />} />
        </Routes>
      </div>
    </BrowserRouter>
  );
}
