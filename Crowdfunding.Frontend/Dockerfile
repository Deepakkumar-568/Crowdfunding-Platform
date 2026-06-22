FROM node:24-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm install --legacy-peer-deps
COPY . .
RUN npm run build

FROM nginx:alpine
# Copy compiled browser assets to nginx html folder.
# Under Angular 17+, the build output defaults to dist/<project-name>/browser
COPY --from=build /app/dist/crowdfunding.frontend/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
