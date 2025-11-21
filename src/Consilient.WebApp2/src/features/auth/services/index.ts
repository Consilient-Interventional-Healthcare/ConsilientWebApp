import { AuthService } from './AuthService';
import { AuthServiceMock } from './AuthService.mock';
import appSettings from '@/config';

export const Auth = appSettings.features.useMockServices
  ? new AuthServiceMock()
  : new AuthService();