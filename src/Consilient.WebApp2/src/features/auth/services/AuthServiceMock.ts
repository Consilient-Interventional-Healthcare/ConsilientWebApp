import type { IAuthService, MockUser } from "@/features/auth/auth.types";
import type { Auth } from "@/types/api.generated";
import { dataProvider } from "@/data/DataProvider";

export class AuthServiceMock implements IAuthService {
  initiateMicrosoftLogin(_returnUrl?: string): void {
    throw new Error("Method not implemented.");
  }
  linkExternalAccount(params: Auth.LinkExternalLoginRequest): Promise<void> {
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
    const users = dataProvider.getTable<MockUser>("users");
    const user = users.find((u) =>
      u.externalProviders?.some((ep) => ep.providerKey === providerKey)
    ) ?? null;
    if (!user) {
      throw new Error("User not found");
    }
    // Return a mock token string
    return Promise.resolve("mock-token");
  }

  login(params: Auth.AuthenticateUserRequest): Promise<Auth.AuthenticateUserApiResponse> {
    if (params.userName === "fail@example.com") {
      throw new Error("Login failed");
    }
    const [user = null] = dataProvider.query<MockUser>(
      "SELECT * FROM users WHERE email = ?",
      [params.userName]
    );
    if (!user) {
      throw new Error("User not found");
    }
    if (user.password !== params.password) {
      throw new Error("Invalid password");
    }
    // Return mock claims
    const claims: Auth.ClaimDto[] = [
      { type: "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", value: user.id.toString() },
      { type: "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", value: user.firstName + " " + user.lastName },
      { type: "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", value: user.email }
    ];
    return Promise.resolve({
      succeeded: true,
      errors: [],
      userClaims: claims
    });
  }

  async logout(): Promise<void> {
    // No-op for mock
  }

    getCurrentUserClaims(): Promise<Auth.ClaimDto[] | null> {
    // Return a fixed mock user claims for demonstration
    const claims: Auth.ClaimDto[] = [
      { type: "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", value: "1" },
      { type: "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", value: "Mock User" },
      { type: "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", value: "mockuser@example.com" }
    ];
    return Promise.resolve(claims);
  }
}
