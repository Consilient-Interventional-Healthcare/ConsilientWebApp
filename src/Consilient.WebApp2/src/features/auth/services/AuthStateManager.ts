/**
 * Centralized authentication state manager
 * Tracks auth initialization state and notifies listeners when ready
 */
class AuthStateManager {
  private isAuthInitialized = false;
  private authInitPromise: Promise<void> | null = null;
  private resolveAuthInit: (() => void) | null = null;

  constructor() {
    // Create a promise that will be resolved when auth is initialized
    this.authInitPromise = new Promise((resolve) => {
      this.resolveAuthInit = resolve;
    });
  }

  /**
   * Mark authentication as initialized
   * This should be called by AuthProvider when initial auth check completes
   */
  setAuthInitialized(): void {
    if (!this.isAuthInitialized) {
      this.isAuthInitialized = true;
      if (this.resolveAuthInit) {
        this.resolveAuthInit();
      }
    }
  }

  /**
   * Check if authentication has been initialized
   */
  isInitialized(): boolean {
    return this.isAuthInitialized;
  }

  /**
   * Wait for authentication to be initialized
   * Returns immediately if already initialized
   */
  async waitForAuthInit(): Promise<void> {
    if (this.isAuthInitialized) {
      return Promise.resolve();
    }
    return this.authInitPromise!;
  }

  /**
   * Reset the auth state (useful for testing)
   */
  reset(): void {
    this.isAuthInitialized = false;
    this.authInitPromise = new Promise((resolve) => {
      this.resolveAuthInit = resolve;
    });
  }
}

// Export singleton instance
export const authStateManager = new AuthStateManager();
