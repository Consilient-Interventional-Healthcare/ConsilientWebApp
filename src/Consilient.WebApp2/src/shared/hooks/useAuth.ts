import { useContext } from 'react';
import { AuthContext } from '@/features/auth/services/AuthContext';
import type { AuthContextType } from '@/features/auth/auth.types';

export function useAuth(): AuthContextType {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
