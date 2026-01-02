// Runtime environment variables for Consilient WebApp
//
// This file is loaded at runtime and sets global variables on window.__ENV.
// Overwrite this file at deployment to inject environment-specific values.
// Example:
// window.__ENV = {
//   APP_API_BASE_URL: "https://your-api-base-url",
//   APP_ENV: "production",
//   APP_ENABLE_DEBUG_MODE: "false",
//   APP_USE_MOCK_SERVICES: "false",
//   APP_ENABLE_EXTERNAL_LOGIN_MOCK: "true",
// };

window.__ENV = {
  APP_API_BASE_URL: "https://localhost:8091",
  APP_ENV: "development",
  APP_ENABLE_DEBUG_MODE: "true",
  APP_USE_MOCK_SERVICES: "true",
  APP_ENABLE_EXTERNAL_LOGIN_MOCK: "false",
};

console.log('window.__ENV loaded:', window.__ENV);