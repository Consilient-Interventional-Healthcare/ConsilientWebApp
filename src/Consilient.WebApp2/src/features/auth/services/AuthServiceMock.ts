import type {
  IAuthService,
  LinkExternalLoginRequest,
  AuthenticateUserRequest,
  User,
  LoginResults,
  UserClaim,
} from "@/features/auth/auth.types";
import { dataProvider } from "@/data/DataProvider";

export class AuthServiceMock implements IAuthService {
  linkExternalAccount(params: LinkExternalLoginRequest): Promise<void> {
    if (params.providerKey === "fail") {
      throw new Error("Failed to link external account");
    }
    // No-op for mock
    return Promise.resolve();
  }

  authenticate(providerKey: string): Promise<string> {
    if (providerKey === "fail") {
      throw new Error("Authentication failed");
    }
    const users = dataProvider.getTable<User>("users");
    const user = users.find((u) =>
      u.externalProviders?.some((ep) => ep.providerKey === providerKey)
    ) ?? null;
    if (!user) {
      throw new Error("User not found");
    }
    // Return a mock token string
    return Promise.resolve("mock-token");
  }

  login(params: AuthenticateUserRequest): Promise<LoginResults> {
    if (params.username === "fail@example.com") {
      throw new Error("Login failed");
    }
    const [user = null] = dataProvider.query<User>(
      "SELECT * FROM users WHERE email = ?",
      [params.username]
    );
    if (!user) {
      throw new Error("User not found");
    }
    // Return mock claims
    const claims: UserClaim[] = [
      { type: "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", value: user.id.toString() },
      { type: "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", value: user.firstName + " " + user.lastName },
      { type: "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", value: user.email }
    ];
    return Promise.resolve({
      success: true,
      errors: [],
      userClaims: claims
    });
  }

  async logout(): Promise<void> {
    // No-op for mock
  }

    getCurrentUserClaims(): Promise<UserClaim[] | null> {
    // Return a fixed mock user claims for demonstration
    const claims: UserClaim[] = [
      { type: "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", value: "1" },
      { type: "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", value: "Mock User" },
      { type: "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", value: "mockuser@example.com" }
    ];
    return Promise.resolve(claims);
  }
}
