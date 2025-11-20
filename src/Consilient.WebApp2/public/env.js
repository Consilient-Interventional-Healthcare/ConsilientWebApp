// Runtime environment variables for Consilient WebApp
//
// This file is loaded at runtime and sets global variables on window.__ENV.
// Overwrite this file at deployment to inject environment-specific values.
// Example:
// window.__ENV = {
//   API_BASE_URL: "https://your-api-base-url",
//   APP_ENV: "production",
//   ENABLE_DEBUG_MODE: "false",
//   DISABLE_AUTH: "false",
//   USE_MOCK_SERVICES: "false",
//   MSAL_CLIENT_ID: "",
//   MSAL_TENANT_ID: "",
//   MSAL_AUTHORITY: "",
//   MSAL_REDIRECT_URI: "",
//   MSAL_SCOPES: "User.Read"
// };

window.__ENV = {
  API_BASE_URL: "http://localhost:5000/api",
  APP_ENV: "development",
  ENABLE_DEBUG_MODE: "true",
  DISABLE_AUTH: "true",
  USE_MOCK_SERVICES: "true",
  MSAL_CLIENT_ID: "",
  MSAL_TENANT_ID: "",
  MSAL_AUTHORITY: "",
  MSAL_REDIRECT_URI: "",
  MSAL_SCOPES: "User.Read"
};

console.log('window.__ENV loaded:', window.__ENV);