import type { ILogEntryTypeProvider } from "./../dailylog.types";

export class LogEntryTypeProvider implements ILogEntryTypeProvider {
  getLogEntryType(): string {
    return "default";
  }
}

export const logEntryTypeProvider = new LogEntryTypeProvider();