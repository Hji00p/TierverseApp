import React, { createContext, useContext, useEffect, useState } from 'react';
import { authApi } from '../api/auth';
import { setAccessToken } from '../api/client';
import type { User } from '../types';

interface AuthContextType {
  user: User | null;
  token: string | null;
  isLoading: boolean;
  login: (credentials: { userName: string; password: string }) => Promise<void>;
  register: (data: { userName: string; email: string; password: string }) => Promise<void>;
  logout: () => Promise<void>;
  isAdmin: boolean;
  isModerator: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);

  // Attempt silent refresh on initial mount
  useEffect(() => {
    let isMounted = true;

    const initializeAuth = async () => {
      try {
        const data = await authApi.refresh();
        if (isMounted) {
          setUser({ id: data.id, userName: data.userName, roles: data.roles });
          setToken(data.accessToken);
        }
      } catch {
        // No active refresh token cookie, user is unauthenticated
        if (isMounted) {
          setUser(null);
          setToken(null);
          setAccessToken(null);
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    };

    initializeAuth();

    return () => {
      isMounted = false;
    };
  }, []);

  const login = async (credentials: { userName: string; password: string }) => {
    const data = await authApi.login(credentials);
    setUser({ id: data.id, userName: data.userName, roles: data.roles });
    setToken(data.accessToken);
  };

  const register = async (data: { userName: string; email: string; password: string }) => {
    const response = await authApi.register(data);
    setUser({ id: response.id, userName: response.userName, roles: response.roles });
    setToken(response.accessToken);
  };

  const logout = async () => {
    try {
      await authApi.logout();
    } finally {
      // Clear all guest/user local drafts so items don't persist across accounts
      try {
        const keysToRemove: string[] = [];
        for (let i = 0; i < localStorage.length; i++) {
          const key = localStorage.key(i);
          if (key && key.startsWith('tierverse_draft_')) {
            keysToRemove.push(key);
          }
        }
        keysToRemove.forEach((k) => localStorage.removeItem(k));
      } catch (e) {
        console.warn('Failed to clear draft keys on logout', e);
      }

      setUser(null);
      setToken(null);
      setAccessToken(null);
    }
  };

  const isAdmin = user?.roles?.includes('Admin') ?? false;
  const isModerator = (user?.roles?.includes('Moderator') || isAdmin) ?? false;

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        isLoading,
        login,
        register,
        logout,
        isAdmin,
        isModerator,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
