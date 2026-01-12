FROM node:20-trixie

WORKDIR /home/node/app

COPY RoomService/package*.json ./
COPY Shared/ ./Shared/

RUN npm ci --omit=dev

COPY RoomService/ ./

USER node

EXPOSE 3002

