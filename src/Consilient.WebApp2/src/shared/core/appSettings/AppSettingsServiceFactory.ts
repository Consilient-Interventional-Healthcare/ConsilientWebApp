import { AppSettingsService } from './AppSettingsService'
import { AppSettingsServiceMock } from './AppSettingsServiceMock'
import appSettings from '@/config';

export class AppSettingsServiceFactory {
  static create() {
    return appSettings.features.mockServices.appSettings
      ? new AppSettingsServiceMock()
      : new AppSettingsService();
  }
}