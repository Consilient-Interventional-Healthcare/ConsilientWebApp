import { AppSettingsService } from './AppSettingsService'
import { AppSettingsServiceMock } from './AppSettingsServiceMock'
import appSettings from '@/config';

export class AppSettingsServiceFactory {
  static create() {
    return appSettings.features.useMockServices ? new AppSettingsServiceMock() : new AppSettingsService()
  }
}