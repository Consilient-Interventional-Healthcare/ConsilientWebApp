interface DailyLogPatientSideBarHeaderProps {
  date: string;
  minDate: string;
  maxDate: string;
  providerId: string;
  selectedProvider: string;
  onProviderChange: (providerId: string) => void;
  onDateChange?: (date: string) => void;
  mockProviders: { id: string; name: string }[];
}

export function DailyLogPatientSideBarHeader({
  date,
  minDate,
  maxDate,
  selectedProvider,
  onProviderChange,
  onDateChange,
  mockProviders,
}: DailyLogPatientSideBarHeaderProps) {
  return (
    <div className="px-4 py-4 border-b border-gray-200 space-y-3">
      {/* Date Input */}
      <div className="space-y-2">
        <label htmlFor="date-input" className="text-xs font-medium text-gray-700 uppercase tracking-wide">
          Date
        </label>
        <input
          id="date-input"
          type="date"
          value={date}
          min={minDate}
          max={maxDate}
          onChange={e => {
            const selected = e.target.value;
            if (selected >= minDate && selected <= maxDate) {
              onDateChange?.(selected);
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
          value={selectedProvider}
          onChange={e => onProviderChange(e.target.value)}
          disabled={!date}
          className={
            `w-full px-3 py-2 border border-gray-300 rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white${!date ? ' bg-gray-100 cursor-not-allowed' : ''}`
          }
        >
          <option value="">Select a provider...</option>
          {mockProviders.map(provider => (
            <option key={provider.id} value={provider.id}>
              {provider.name}
            </option>
          ))}
        </select>
      </div>
    </div>
  );
}
