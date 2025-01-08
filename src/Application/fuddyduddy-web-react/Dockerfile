# Build stage
FROM node:20-alpine AS build

WORKDIR /app

# Copy package files
COPY src/Application/fuddyduddy-web-react/package*.json ./

# Install dependencies
RUN npm ci

# Copy source code
COPY src/Application/fuddyduddy-web-react/ .

# Build the application
RUN npm run build

# Production stage
FROM nginx:alpine AS final

# Copy custom nginx config
COPY src/Application/fuddyduddy-web-react/nginx.conf /etc/nginx/conf.d/default.conf

# Copy built assets from build stage
COPY --from=build /app/dist /usr/share/nginx/html

# Create required directories and set permissions
RUN mkdir -p /var/cache/nginx && \
    mkdir -p /var/log/nginx && \
    mkdir -p /var/run && \
    chown -R nginx:nginx /var/cache/nginx /var/log/nginx /var/run /usr/share/nginx/html && \
    chmod -R 755 /var/cache/nginx /var/log/nginx /var/run /usr/share/nginx/html

# Use the 'nginx' user (already exists in nginx:alpine)
USER nginx

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
