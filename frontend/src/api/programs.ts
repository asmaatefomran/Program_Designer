import axios from "axios";
import type {
  CreateProgramRequest,
  CreateProgramResponse,
  PagedResult,
  ProgramResponse,
  ProgramSummaryResponse,
  SimulateRequest,
  SimulateResponse,
  ValidationResponse,
} from "./types";

const baseURL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5000";

export const apiClient = axios.create({ baseURL });

export async function createProgram(
  request: CreateProgramRequest,
): Promise<CreateProgramResponse> {
  const { data } = await apiClient.post<CreateProgramResponse>(
    "/programs",
    request,
  );
  return data;
}

export async function getProgram(id: string): Promise<ProgramResponse> {
  const { data } = await apiClient.get<ProgramResponse>(`/programs/${id}`);
  return data;
}

export async function validateProgram(id: string): Promise<ValidationResponse> {
  const { data } = await apiClient.post<ValidationResponse>(
    `/programs/${id}/validate`,
  );
  return data;
}

export async function getPrograms(
  page: number,
  pageSize: number,
): Promise<PagedResult<ProgramSummaryResponse>> {
  const { data } = await apiClient.get<PagedResult<ProgramSummaryResponse>>(
    "/programs",
    { params: { page, pageSize } },
  );
  return data;
}

export async function simulateProgram(
  id: string,
  request: SimulateRequest,
): Promise<SimulateResponse> {
  const { data } = await apiClient.post<SimulateResponse>(
    `/programs/${id}/simulate`,
    request,
  );
  return data;
}
