# Build stage
FROM node:20-alpine AS build

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci

# Copy source code
COPY . .

# Build the application
RUN npm run build

# Production stage
FROM nginx:alpine AS final

# Copy custom nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy built assets from build stage
COPY --from=build /app/dist /usr/share/nginx/html

# Create a non-root user
RUN adduser -D -H -s /sbin/nologin webuser && \
    chown -R webuser:webuser /usr/share/nginx/html

# Switch to non-root user
USER webuser

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
