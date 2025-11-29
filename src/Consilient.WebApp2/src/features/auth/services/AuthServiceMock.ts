import type {
  IAuthService,
  LinkExternalLoginRequest,
  AuthenticateUserRequest,
  User,
} from "@/features/auth/auth.types";
import { jwtService } from "@/features/auth/services/JwtService";
import { dataProvider } from "@/data/DataProvider";

function createMockJwt(payload: object): string {
  const header = { alg: "HS256", typ: "JWT" };
  const encode = (obj: object) => btoa(JSON.stringify(obj));
  // No signature for mock
  return `${encode(header)}.${encode(payload)}.`;
}

export class AuthServiceMock implements IAuthService {
  async linkExternalAccount(params: LinkExternalLoginRequest): Promise<void> {
    if (params.providerKey === "fail") {
      return Promise.reject(new Error("Failed to link external account"));
    }
    return Promise.resolve();
  }

  async authenticate(providerKey: string): Promise<string> {
    if (providerKey === "fail") {
      return Promise.reject(new Error("Authentication failed"));
    }
    const users = dataProvider.getTable<User>("users");
    const user =
      users.find((u) =>
        u.externalProviders?.some((ep) => ep.providerKey === providerKey)
      ) ?? null;
    if (!user) {
      return Promise.reject(new Error("User not found"));
    }
    const token = createMockJwt(user);
    jwtService.store(token);
    return Promise.resolve(token);
  }

  async login(params: AuthenticateUserRequest): Promise<string> {
    if (params.email === "fail@example.com") {
      return Promise.reject(new Error("Login failed"));
    }
    const [user = null] = dataProvider.query<User>(
      "SELECT * FROM users WHERE email = ?",
      [params.email]
    );
    if (!user) {
      return Promise.reject(new Error("User not found"));
    }
    const token = createMockJwt(user);
    jwtService.store(token);
    return Promise.resolve(token);
  }

  logout(): void {
    jwtService.remove();
  }
}
