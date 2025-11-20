# Use official Node.js image as the base
FROM mcr.microsoft.com/oss/nginx/nginx:1.9.15-alpine AS build

# Set working directory
WORKDIR /app

# Copy package.json and package-lock.json (if available)
COPY package.json .
COPY package-lock.json .

# Install dependencies
RUN npm install --frozen-lockfile

# Copy the rest of the application code
COPY . .

# Build the application (Vite)
RUN npm run build

# Use a lightweight web server to serve the built files
FROM nginx:alpine AS production

# Copy built files from previous stage
COPY --from=build /app/dist /usr/share/nginx/html

# Copy custom nginx config if needed
# COPY nginx.conf /etc/nginx/nginx.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
