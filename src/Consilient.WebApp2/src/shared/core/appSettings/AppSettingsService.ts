import apiClient from '@/shared/core/api/ApiClient';
import type { AppSettings, IAppSettingsService } from './appSettings.types';

export class AppSettingsService implements IAppSettingsService {
  private appSettings: AppSettings | null = null

  public async getAppSettings(): Promise<AppSettings> {
    if (!this.appSettings) {
      const response = await apiClient.get<AppSettings>('/settings')
      this.appSettings ??= response?.data
    }
    return this.appSettings
  }

  public setAppSettings(settings: AppSettings): void {
    this.appSettings = settings
  }
}