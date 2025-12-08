// Factory method to get the correct AuthService implementation
import { AuthServiceMock } from './AuthServiceMock';
import { AuthService } from './AuthService';
import appSettings from '@/config';

export function getAuthService() {
  return new AuthService();
  // debugger;
  // return appSettings.features.useMockServices
  //   ? new AuthServiceMock()
  //   : new AuthService();
}