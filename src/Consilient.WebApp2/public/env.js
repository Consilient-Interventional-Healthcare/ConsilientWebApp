// Runtime environment variables for Consilient WebApp
//
// This file is loaded at runtime and sets global variables on window.__ENV.
// Overwrite this file at deployment to inject environment-specific values.
// Example:
// window.__ENV = {
//   APP_API_BASE_URL: "https://your-api-base-url",
//   APP_ENV: "production",
//   APP_ENABLE_DEBUG_MODE: "false",
//   APP_DISABLE_AUTH: "false",
//   APP_USE_MOCK_SERVICES: "false",
//   APP_MSAL_CLIENT_ID: "",
//   APP_MSAL_TENANT_ID: "",
//   APP_MSAL_AUTHORITY: "",
//   APP_MSAL_REDIRECT_URI: "",
//   APP_MSAL_SCOPES: "User.Read"
// };

window.__ENV = {
  APP_API_BASE_URL: "http://localhost:5000/api",
  APP_ENV: "development",
  APP_ENABLE_DEBUG_MODE: "true",
  APP_DISABLE_AUTH: "false",
  APP_USE_MOCK_SERVICES: "true",
  APP_MSAL_CLIENT_ID: "",
  APP_MSAL_TENANT_ID: "",
  APP_MSAL_AUTHORITY: "",
  APP_MSAL_REDIRECT_URI: "",
  APP_MSAL_SCOPES: "User.Read"
};

console.log('window.__ENV loaded:', window.__ENV);