# Build stage: use Node.js for installing and building the app
FROM node:20-alpine AS build

WORKDIR /app

# Copy package files first to leverage Docker layer cache
COPY package.json package-lock.json ./

# Install dependencies deterministically in CI
RUN npm ci --silent

# Copy source and build
COPY . .
RUN npm run build

# Production stage: lightweight nginx to serve static files
FROM nginx:stable-alpine AS production

# Clear default content and copy build output
RUN rm -rf /usr/share/nginx/html/*
COPY --from=build /app/dist /usr/share/nginx/html

# Optional: provide SPA fallback for client-side routing
# COPY nginx.conf /etc/nginx/nginx.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
