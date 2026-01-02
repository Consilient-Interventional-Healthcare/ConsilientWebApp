export interface AppSettings {
  externalLoginEnabled: boolean
  // Add other shared app settings properties here
  // Example:
  // ApiTimeout: number
  // FeatureXEnabled: boolean
}

export interface IAppSettingsService {
  getAppSettings(): Promise<AppSettings>
  setAppSettings(settings: AppSettings): void
}