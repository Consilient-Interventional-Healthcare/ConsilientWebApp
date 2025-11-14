import { createContext } from 'react';
import type { AuthContextType } from '@/types/auth';

const AuthContext = createContext<AuthContextType | null>(null);

// Export the context so it can be used by useAuth hook
export { AuthContext };
