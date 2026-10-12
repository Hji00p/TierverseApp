import { api } from './client';
import type {
  CreateTierListRequest,
  PaginatedResponse,
  SaveLayoutRequest,
  TierListBrief,
  TierListDetail,
  VoteResult,
} from '../types';

export const tierListsApi = {
  getAll: async (search?: string, page = 1, pageSize = 10): Promise<PaginatedResponse<TierListBrief>> => {
    const params = new URLSearchParams();
    if (search) params.append('search', search);
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());

    const response = await api.get<PaginatedResponse<TierListBrief>>(`/tierlists?${params.toString()}`);
    return response.data;
  },

  getById: async (id: string): Promise<TierListDetail> => {
    const response = await api.get<TierListDetail>(`/tierlists/${id}`);
    return response.data;
  },

  create: async (data: CreateTierListRequest): Promise<string> => {
    const response = await api.post<{ id: string } | string>('/tierlists', data);
    if (typeof response.data === 'string') {
      return response.data;
    }
    return (response.data as { id: string }).id;
  },

  update: async (id: string, data: { title: string; description?: string }): Promise<void> => {
    await api.put(`/tierlists/${id}`, data);
  },

  saveLayout: async (id: string, layout: SaveLayoutRequest): Promise<void> => {
    await api.put(`/tierlists/${id}/layout`, layout);
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/tierlists/${id}`);
  },

  vote: async (id: string, value: number): Promise<VoteResult> => {
    const response = await api.put<VoteResult>(`/tierlists/${id}/vote`, { value });
    return response.data;
  },
};
