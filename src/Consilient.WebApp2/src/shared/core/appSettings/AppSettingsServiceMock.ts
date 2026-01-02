import type { AppSettings, IAppSettingsService } from './appSettings.types'
import appSettings from '@/config';

export class AppSettingsServiceMock implements IAppSettingsService {
  private appSettings: AppSettings | null = null

  public getAppSettings(): Promise<AppSettings> {
    this.appSettings ??= {
      ExternalLoginEnabled: appSettings.features.enableExternalLoginMock ?? false,
    }
    return Promise.resolve(this.appSettings)
  }

  public setAppSettings(settings: AppSettings): void {
    this.appSettings = settings
  }
}