import { api } from './client';
import type { Comment, PaginatedResponse, VoteResult } from '../types';

export const commentsApi = {
  getByTierListId: async (tierListId: string, page = 1, pageSize = 20): Promise<PaginatedResponse<Comment>> => {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });

    const response = await api.get<PaginatedResponse<Comment>>(
      `/tierlists/${tierListId}/comments?${params.toString()}`
    );
    return response.data;
  },

  create: async (tierListId: string, text: string, parentCommentId?: string): Promise<string> => {
    const response = await api.post<{ id: string }>(`/tierlists/${tierListId}/comments`, {
      text,
      parentCommentId,
    });
    return response.data.id;
  },

  delete: async (id: string): Promise<void> => {
    await api.delete(`/comments/${id}`);
  },

  vote: async (id: string, value: number): Promise<VoteResult> => {
    const response = await api.put<VoteResult>(`/comments/${id}/vote`, { value });
    return response.data;
  },
};
