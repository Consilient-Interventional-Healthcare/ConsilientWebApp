export interface ProviderOption {
  id: number;
  firstName: string;
  lastName: string;
}
interface DailyLogVisitFiltersDateProviderProps {
  date: string;
  minDate: string;
  maxDate: string;
  providerId?: number | null;
  selectedProvider: number | null;
  onProviderChange: (providerId: number | null) => void;
  onDateChange?: (date: string) => void;
  providers: ProviderOption[];
}

export function DailyLogVisitFiltersDateProvider(props: DailyLogVisitFiltersDateProviderProps) {
  return (
    <div className="px-4 py-4 border-b border-gray-200 space-y-3">
      <div className="space-y-2">
        <label htmlFor="date-input" className="text-xs font-medium text-gray-700 uppercase tracking-wide">
          Date
        </label>
        <input
          id="date-input"
          type="date"
          value={props.date}
          min={props.minDate}
          max={props.maxDate}
          onChange={e => {
            const selected = e.target.value;
            if (selected >= props.minDate && selected <= props.maxDate) {
              props.onDateChange?.(selected);
            }
          }}
          className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm bg-white focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        />
      </div>
      {/* Provider Dropdown */}
      <div className="space-y-2">
        <label htmlFor="provider-select" className="text-xs font-medium text-gray-700 uppercase tracking-wide">
          Provider
        </label>
        <select
          id="provider-select"
          value={props.selectedProvider ?? ''}
          onChange={e => props.onProviderChange(e.target.value === '' ? null : Number(e.target.value))}
          disabled={!props.date}
          className={
            `w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white${!props.date ? ' bg-gray-100 cursor-not-allowed' : ''}`
          }
        >
          <option value="">Select a provider...</option>
          {props.providers.map(provider => (
            <option key={provider.id} value={provider.id}>
              {provider.lastName}, {provider.firstName}
            </option>
          ))}
        </select>
      </div>
    </div>
  );
}
