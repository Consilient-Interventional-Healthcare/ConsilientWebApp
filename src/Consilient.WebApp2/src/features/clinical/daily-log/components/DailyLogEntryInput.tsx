import React, { useState } from 'react';

interface DailyLogEntryInputProps {
  onSubmit: (content: string) => void;
}

export function DailyLogEntryInput({ onSubmit }: DailyLogEntryInputProps) {
  const [content, setContent] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (content.trim()) {
      onSubmit(content);
      setContent('');
    }
  };

  return (
    <form onSubmit={handleSubmit} className="px-6 py-4 border-t bg-gray-50 flex gap-2 items-start">
      <div className="flex-1 flex flex-col">
        <textarea
          value={content}
          onChange={e => setContent(e.target.value)}
          className="resize-none rounded-md border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          rows={2}
          placeholder="Write a log entry..."
        />
        <div className="mt-1 text-xs text-gray-400 select-none">
          Press <kbd className="px-1 py-0.5 bg-gray-200 rounded border text-gray-700">Ctrl</kbd> + <kbd className="px-1 py-0.5 bg-gray-200 rounded border text-gray-700">Enter</kbd> to submit
        </div>
      </div>
      <button
        type="submit"
        className="bg-blue-600 text-white px-4 py-2 rounded-md text-sm font-medium hover:bg-blue-700 transition-colors"
      >
        Add Entry
      </button>
    </form>
  );
}
