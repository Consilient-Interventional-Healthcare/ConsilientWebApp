# Build stage: use Node.js for installing and building the app
FROM node:20-alpine AS build

WORKDIR /app

# Accept build-time env for Vite
ARG VITE_API_BASE_URL
ENV VITE_API_BASE_URL=${VITE_API_BASE_URL}

# Copy package files first to leverage Docker cache
COPY package.json package-lock.json ./

# Install dependencies
RUN npm ci --silent

# Copy the rest of the application code
COPY . .

# Create an .env file that Vite will read during build
RUN printf "VITE_API_BASE_URL=%s\n" "${VITE_API_BASE_URL}" > .env

# Build the application (Vite)
RUN npm run build

# Production stage: lightweight nginx to serve the built files
FROM nginx:stable-alpine AS production

# Copy built files from previous stage
RUN rm -rf /usr/share/nginx/html/*
COPY --from=build /app/dist /usr/share/nginx/html

# Copy custom nginx config if needed
# COPY nginx.conf /etc/nginx/nginx.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]