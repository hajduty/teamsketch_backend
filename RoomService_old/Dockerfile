FROM node:20-alpine

# Create app directory
RUN mkdir -p /home/node/app && chown -R node:node /home/node/app
WORKDIR /home/node/app

# Copy only package.json & package-lock.json
COPY RoomService/package*.json ./ 
COPY Shared/ ./Shared/

# Install dependencies as root (or you could switch to node later)
RUN npm install --only=production --legacy-peer-deps

# Copy rest of the app code
COPY --chown=node:node RoomService/ ./ 

# Switch to non-root user
USER node

EXPOSE 1234
CMD ["npm", "start"]
