import { api } from './client';
import type {
  TemplateBrief,
  TemplateDetail,
  CreateTemplateRequest,
  PaginatedResponse,
} from '../types';

export const getTemplates = async (params: {
  search?: string;
  page?: number;
  pageSize?: number;
}): Promise<PaginatedResponse<TemplateBrief>> => {
  const { data } = await api.get<PaginatedResponse<TemplateBrief>>('/templates', { params });
  return data;
};

export const getTemplateById = async (id: string): Promise<TemplateDetail> => {
  const { data } = await api.get<TemplateDetail>(`/templates/${id}`);
  return data;
};

export const createTemplate = async (
  req: CreateTemplateRequest
): Promise<{ id: string }> => {
  const { data } = await api.post<{ id: string }>('/templates', req);
  return data;
};

export const updateTemplate = async (
  id: string,
  req: CreateTemplateRequest
): Promise<void> => {
  await api.put(`/templates/${id}`, req);
};

export const deleteTemplate = async (id: string): Promise<void> => {
  await api.delete(`/templates/${id}`);
};
