import { useState } from 'react';
import { logger } from '@/shared/core/logging/Logger';
import { Button } from '@/shared/components/ui/button';

export default function LoggingTest() {
  const [logCount, setLogCount] = useState(0);
  const [currentLevel, setCurrentLevel] = useState<string>(
    ['trace', 'debug', 'info', 'warn', 'error', 'silent'][logger.getLevel()] ?? 'info'
  );

  const testTrace = () => {
    logger.trace('This is a TRACE message', {
      component: 'LoggingTest',
      action: 'test_trace',
      timestamp: Date.now(),
    });
    setLogCount(c => c + 1);
  };

  const testDebug = () => {
    logger.debug('This is a DEBUG message', {
      component: 'LoggingTest',
      action: 'test_debug',
      data: { foo: 'bar', count: 42 },
    });
    setLogCount(c => c + 1);
  };

  const testInfo = () => {
    logger.info('This is an INFO message', {
      component: 'LoggingTest',
      action: 'test_info',
      userId: 12345,
    });
    setLogCount(c => c + 1);
  };

  const testWarn = () => {
    logger.warn('This is a WARNING message', {
      component: 'LoggingTest',
      action: 'test_warn',
      severity: 'medium',
    });
    setLogCount(c => c + 1);
  };

  const testError = () => {
    const testError = new Error('This is a test error');
    testError.stack = 'Error: This is a test error\n    at testError (LoggingTest.tsx:50:25)';
    logger.error('This is an ERROR message', testError, {
      component: 'LoggingTest',
      action: 'test_error',
      userId: 12345,
    });
    setLogCount(c => c + 1);
  };

  const testAll = () => {
    testTrace();
    testDebug();
    testInfo();
    testWarn();
    testError();
  };

  const changeLogLevel = (level: 'trace' | 'debug' | 'info' | 'warn' | 'error' | 'silent') => {
    logger.setLevel(level);
    setCurrentLevel(level);
  };

  return (
    <div className="container mx-auto p-6 max-w-4xl">
      <h1 className="text-3xl font-bold mb-2">Logging System Test</h1>
      <p className="text-muted-foreground mb-6">
        Test the logging system and check your browser console for output
      </p>

      {/* Status */}
      <div className="bg-muted p-4 rounded-lg mb-6">
        <div className="grid grid-cols-2 gap-4">
          <div>
            <span className="font-semibold">Current Log Level:</span>{' '}
            <span className="font-mono text-primary">{currentLevel.toUpperCase()}</span>
          </div>
          <div>
            <span className="font-semibold">Logs Generated:</span>{' '}
            <span className="font-mono text-primary">{logCount}</span>
          </div>
        </div>
      </div>

      {/* Log Level Controls */}
      <div className="mb-6">
        <h2 className="text-xl font-semibold mb-3">Set Log Level</h2>
        <div className="flex gap-2 flex-wrap">
          <Button onClick={() => changeLogLevel('trace')} variant={currentLevel === 'trace' ? 'default' : 'outline'}>
            TRACE
          </Button>
          <Button onClick={() => changeLogLevel('debug')} variant={currentLevel === 'debug' ? 'default' : 'outline'}>
            DEBUG
          </Button>
          <Button onClick={() => changeLogLevel('info')} variant={currentLevel === 'info' ? 'default' : 'outline'}>
            INFO
          </Button>
          <Button onClick={() => changeLogLevel('warn')} variant={currentLevel === 'warn' ? 'default' : 'outline'}>
            WARN
          </Button>
          <Button onClick={() => changeLogLevel('error')} variant={currentLevel === 'error' ? 'default' : 'outline'}>
            ERROR
          </Button>
          <Button onClick={() => changeLogLevel('silent')} variant={currentLevel === 'silent' ? 'default' : 'outline'}>
            SILENT
          </Button>
        </div>
        <p className="text-sm text-muted-foreground mt-2">
          {currentLevel === 'trace' && '📝 All logs visible (TRACE, DEBUG, INFO, WARN, ERROR)'}
          {currentLevel === 'debug' && '🔍 DEBUG, INFO, WARN, ERROR visible'}
          {currentLevel === 'info' && 'ℹ️ INFO, WARN, ERROR visible'}
          {currentLevel === 'warn' && '⚠️ Only WARN and ERROR visible'}
          {currentLevel === 'error' && '❌ Only ERROR visible'}
          {currentLevel === 'silent' && '🔇 No logs visible'}
        </p>
      </div>

      {/* Test Buttons */}
      <div className="mb-6">
        <h2 className="text-xl font-semibold mb-3">Test Individual Log Levels</h2>
        <div className="flex gap-2 flex-wrap">
          <Button onClick={testTrace} variant="outline">
            Test TRACE
          </Button>
          <Button onClick={testDebug} variant="outline">
            Test DEBUG
          </Button>
          <Button onClick={testInfo} variant="outline">
            Test INFO
          </Button>
          <Button onClick={testWarn} variant="outline">
            Test WARN
          </Button>
          <Button onClick={testError} variant="outline">
            Test ERROR
          </Button>
        </div>
      </div>

      <div className="mb-6">
        <Button onClick={testAll} size="lg" className="w-full">
          🚀 Test All Log Levels
        </Button>
      </div>

      {/* Instructions */}
      <div className="border rounded-lg p-4 bg-card">
        <h3 className="font-semibold mb-2">📋 Instructions</h3>
        <ol className="list-decimal list-inside space-y-2 text-sm">
          <li>Open your browser's Developer Console (F12)</li>
          <li>Click the buttons above to generate logs</li>
          <li>Watch the console for formatted log output</li>
          <li>Try changing the log level to filter messages</li>
          <li>
            Check for context metadata:{' '}
            <code className="bg-muted px-1 rounded">component, action, userId</code>
          </li>
        </ol>
      </div>

      {/* Remote Logging Testing */}
      <div className="border rounded-lg p-4 bg-card mt-6">
        <h3 className="font-semibold mb-2">🔗 Remote Logging via Backend API</h3>
        <div className="text-sm space-y-2">
          <p>To test remote logging:</p>
          <ol className="list-decimal list-inside space-y-1 ml-2">
            <li>Set <code className="bg-muted px-1 rounded">ENABLE_REMOTE_LOGGING=true</code> in your .env file</li>
            <li>Restart dev server: <code className="bg-muted px-1 rounded">npm run dev</code></li>
            <li>Click any test button above</li>
            <li>Check browser Network tab for <code className="bg-muted px-1 rounded">POST /api/logs</code> requests</li>
            <li>Backend will store/process logs as configured</li>
          </ol>
          <p className="text-muted-foreground mt-2">
            💡 Logs are sent to your backend API at <code className="bg-muted px-1 rounded">/api/logs</code>.
          </p>
          <p className="text-muted-foreground">
            ⚙️ Backend needs to implement the <code className="bg-muted px-1 rounded">POST /api/logs</code> endpoint.
          </p>
          <p className="text-muted-foreground">
            📊 In production, remote logging is always enabled. In development, use the flag above.
          </p>
        </div>
      </div>
    </div>
  );
}
