import { api, setAccessToken } from './client';
import type { AuthResponse } from '../types';

export const authApi = {
  login: async (credentials: { userName: string; password: string }): Promise<AuthResponse> => {
    const response = await api.post<AuthResponse>('/auth/login', credentials);
    setAccessToken(response.data.accessToken);
    return response.data;
  },

  register: async (data: { userName: string; email: string; password: string }): Promise<AuthResponse> => {
    const response = await api.post<AuthResponse>('/auth/register', data);
    setAccessToken(response.data.accessToken);
    return response.data;
  },

  refresh: async (): Promise<AuthResponse> => {
    const response = await api.post<AuthResponse>('/auth/refresh', {});
    setAccessToken(response.data.accessToken);
    return response.data;
  },

  logout: async (): Promise<void> => {
    try {
      await api.post('/auth/logout', {});
    } finally {
      setAccessToken(null);
    }
  },
};
