import { appSettings } from '@/config/index';
import { DailyLogService } from './DailyLogService';
import { DailyLogServiceMock } from './DailyLogServiceMock';

export const getDailyLogService = () => appSettings.features.useMockServices 
  ? new DailyLogServiceMock() 
  : new DailyLogService();
