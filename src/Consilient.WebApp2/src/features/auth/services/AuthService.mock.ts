import type { LinkExternalLoginRequest, AuthenticateUserRequest, User } from '@/features/auth/auth.types';
import { JwtService } from '@/features/auth/services/JwtService';

// Import mock data from db.json
import dbRaw from '@/data/db.json';
import * as z from 'zod';

// Define Zod schemas that match your types
const UserSchema = z.object({
  id: z.string(),
  email: z.string().email(),
  firstName: z.string(),
  lastName: z.string(),
  name: z.string().optional(),
  role: z.string().optional(),
});

const DbSchema = z.object({
  users: z.array(UserSchema),
  mockExternalAccounts: z.array(z.any()).optional(),
});

// Validate and parse
const db = DbSchema.parse(dbRaw);

function createMockJwt(payload: object): string {
  const header = { alg: 'HS256', typ: 'JWT' };
  const encode = (obj: object) => btoa(JSON.stringify(obj));
  // No signature for mock
  return `${encode(header)}.${encode(payload)}.`;
}

export class AuthServiceMock {
  async linkExternalAccount(params: LinkExternalLoginRequest): Promise<void> {
    // Example: Use db.mockExternalAccounts if needed
    if (params.providerKey === 'fail') {
      return Promise.reject(new Error('Failed to link external account'));
    }
    return Promise.resolve();
  }

  async authenticate(providerKey: string): Promise<string> {
    if (providerKey === 'fail') {
      return Promise.reject(new Error('Authentication failed'));
    }
    // Use mock user from db.json if available
    const user = db.users[0];
    if (!user) {
      return Promise.reject(new Error('User not found'));
    }
    const token = createMockJwt(user);
    JwtService.store(token);
    return Promise.resolve(token);
  }

  async login(params: AuthenticateUserRequest): Promise<string> {
    if (params.email === 'fail@example.com') {
      return Promise.reject(new Error('Login failed'));
    }
    // Use mock user from db.json if available
      const user = db.users.find(u => u.email === params.email);
      if (!user) {
        return Promise.reject(new Error('User not found'));
      }
      // Map db user to User type
      const mappedUser: User = {
        id: user.id as User['id'],
        email: user.email,
        firstName: user.firstName,
        lastName: user.lastName,
        name: user.name ?? '', // Ensure 'name' is always a string
        role: user.role ?? ''
      };
      const token = createMockJwt(mappedUser);
    JwtService.store(token);
    return Promise.resolve(token);
  }

  logout(): void {
    JwtService.remove();
  }
}