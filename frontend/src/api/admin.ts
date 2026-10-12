import { api } from './client';
import type { PaginatedResponse, UserRoleInfo } from '../types';

export const adminApi = {
  getUsers: async (search?: string, page = 1, pageSize = 20): Promise<PaginatedResponse<UserRoleInfo>> => {
    const params = new URLSearchParams();
    if (search) params.append('search', search);
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());

    const response = await api.get<PaginatedResponse<UserRoleInfo>>(`/admin/users?${params.toString()}`);
    return response.data;
  },

  updateUserRole: async (userId: string, role: string): Promise<void> => {
    await api.put(`/admin/users/${userId}/role`, { role });
  },
};
