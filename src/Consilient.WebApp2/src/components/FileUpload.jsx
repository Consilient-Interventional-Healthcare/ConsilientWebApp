import { useState } from 'react';

function FileUpload({ onFileSelect, acceptedFileTypes = "*", maxSizeMB = 10 }) {
  const [selectedFile, setSelectedFile] = useState(null);
  const [error, setError] = useState('');
  const [isDragging, setIsDragging] = useState(false);

  const handleFileChange = (event) => {
    const file = event.target.files[0];
    validateAndSetFile(file);
  };

  const validateAndSetFile = (file) => {
    if (!file) return;

    setError('');
    
    // Check file size
    const maxSizeBytes = maxSizeMB * 1024 * 1024;
    if (file.size > maxSizeBytes) {
      setError(`File size exceeds ${maxSizeMB}MB limit`);
      return;
    }

    setSelectedFile(file);
    if (onFileSelect) {
      onFileSelect(file);
    }
  };

  const handleDragOver = (event) => {
    event.preventDefault();
    setIsDragging(true);
  };

  const handleDragLeave = () => {
    setIsDragging(false);
  };

  const handleDrop = (event) => {
    event.preventDefault();
    setIsDragging(false);
    const file = event.dataTransfer.files[0];
    validateAndSetFile(file);
  };

  const clearFile = () => {
    setSelectedFile(null);
    setError('');
    if (onFileSelect) {
      onFileSelect(null);
    }
  };

  return (
    <div className="w-full max-w-md mx-auto">
      <div
        className={`border-2 border-dashed rounded-lg p-8 text-center transition-colors ${
          isDragging ? 'border-blue-500 bg-blue-50' : 'border-gray-300 hover:border-gray-400'
        }`}
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
        onDrop={handleDrop}
      >
        <input
          type="file"
          id="fileInput"
          className="hidden"
          onChange={handleFileChange}
          accept={acceptedFileTypes}
        />
        
        {!selectedFile ? (
          <label htmlFor="fileInput" className="cursor-pointer">
            <div className="text-gray-600">
              <svg className="mx-auto h-12 w-12 text-gray-400" stroke="currentColor" fill="none" viewBox="0 0 48 48">
                <path d="M28 8H12a4 4 0 00-4 4v20m32-12v8m0 0v8a4 4 0 01-4 4H12a4 4 0 01-4-4v-4m32-4l-3.172-3.172a4 4 0 00-5.656 0L28 28M8 32l9.172-9.172a4 4 0 015.656 0L28 28m0 0l4 4m4-24h8m-4-4v8m-12 4h.02" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
              </svg>
              <p className="mt-2 text-sm">
                <span className="font-semibold text-blue-600 hover:text-blue-500">Click to upload</span> or drag and drop
              </p>
              <p className="text-xs text-gray-500 mt-1">Max size: {maxSizeMB}MB</p>
            </div>
          </label>
        ) : (
          <div className="text-sm">
            <div className="flex items-center justify-between bg-gray-50 p-3 rounded">
              <div className="flex items-center space-x-2">
                <svg className="h-5 w-5 text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M8 4a3 3 0 00-3 3v4a5 5 0 0010 0V7a1 1 0 112 0v4a7 7 0 11-14 0V7a5 5 0 0110 0v4a3 3 0 11-6 0V7a1 1 0 012 0v4a1 1 0 102 0V7a3 3 0 00-3-3z" clipRule="evenodd" />
                </svg>
                <span className="text-gray-700 truncate max-w-xs">{selectedFile.name}</span>
              </div>
              <button
                onClick={clearFile}
                className="text-red-500 hover:text-red-700 font-medium"
              >
                Remove
              </button>
            </div>
            <p className="text-xs text-gray-500 mt-2">
              {(selectedFile.size / 1024 / 1024).toFixed(2)} MB
            </p>
          </div>
        )}
      </div>

      {error && (
        <div className="mt-2 text-sm text-red-600">
          {error}
        </div>
      )}
    </div>
  );
}

export default FileUpload;