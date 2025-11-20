# Build stage: use Node.js for installing and building the app
FROM node:20-alpine AS build

WORKDIR /app


# Copy package files first to leverage Docker cache
COPY package.json package-lock.json ./

# Install dependencies
RUN npm ci --silent

# Copy the rest of the application code
COPY . .


# Build the application (Vite)
RUN npm run build

# Production stage: lightweight nginx to serve the built files
FROM nginx:stable-alpine AS production

# Install bash (not strictly necessary) and gettext for envsubst if needed
RUN apk add --no-cache bash

# Copy built files from previous stage
RUN rm -rf /usr/share/nginx/html/*
COPY --from=build /app/dist /usr/share/nginx/html

# Copy entrypoint that will generate env.js at container start
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

# Copy custom nginx config if needed
COPY nginx.conf /etc/nginx/nginx.conf

EXPOSE 80

ENTRYPOINT ["/docker-entrypoint.sh"]
CMD ["nginx", "-g", "daemon off;"]