// Mirrors src/ProgramDesigner.Api/DTOs exactly. Keep these in sync with the
// backend by hand -- there's no shared schema between the two projects.

export type GroupType = "All" | "Choice";

// ---- Requests (POST /programs) ----

export interface StepRequest {
  type: "step";
  templateId: string;
  name: string;
  prerequisiteTemplateIds: string[];
}

export interface GroupRequest {
  type: "group";
  templateId: string;
  name: string;
  groupType: GroupType;
  requiredChoiceCount: number;
  prerequisiteTemplateIds: string[];
  children: NodeRequest[];
}

export type NodeRequest = StepRequest | GroupRequest;

export interface CreateProgramRequest {
  name: string;
  rootGroup: GroupRequest;
}

// ---- Responses ----

export interface StepResponse {
  type: "step";
  id: string;
  templateId: string;
  name: string;
  prerequisiteTemplateIds: string[];
}

export interface GroupResponse {
  type: "group";
  id: string;
  templateId: string;
  name: string;
  groupType: GroupType;
  requiredSelections: number | null;
  prerequisiteTemplateIds: string[];
  children: NodeResponse[];
}

export type NodeResponse = StepResponse | GroupResponse;

export interface ProgramResponse {
  id: string;
  name: string;
  rootGroup: GroupResponse;
}

export interface ValidationIssueResponse {
  code: string;
  nodeId: string;
  message: string;
}

export interface ValidationResponse {
  isValid: boolean;
  errors: ValidationIssueResponse[];
  warnings: ValidationIssueResponse[];
}

export interface CreateProgramResponse {
  program: ProgramResponse;
  validation: ValidationResponse;
}

// ---- Programs list (GET /programs?page=&pageSize=) ----

export interface ProgramSummaryResponse {
  id: string;
  name: string;
  createdAt: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// ---- Simulate (POST /programs/:id/simulate) ----

export interface SimulateRequest {
  choices: Record<string, string[]>;
  completedStepTemplateIds: string[];
}

export interface NodeStateResponse {
  id: string;
  templateId: string;
  name: string;
  status: "complete" | "unlocked" | "blocked";
  reason: string | null;
}

export interface SimulateResponse {
  complete: NodeStateResponse[];
  unlocked: NodeStateResponse[];
  blocked: NodeStateResponse[];
}
