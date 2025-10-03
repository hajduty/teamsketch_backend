import Redis from "ioredis";
import { closeConnectionByUserAndRoom } from "./utils.js";

const redis = new Redis(process.env.REDIS_URL || "localhost:6379");

// Subscribe to the permission-events channel
redis.subscribe("user:kick", (err, count) => {
  if (err) {
    console.error("Failed to subscribe: ", err);
  } else {
    console.log(`Subscribed to ${count} channel(s). Listening for events...`);
  }
});

redis.on("message", (channel, message) => {
  console.log("RAW MESSAGE:", channel, message);
  if (channel === "user:kick") {
    try {
      const event = JSON.parse(message);

      console.log(
        `Received KICK_USER event for user ${event.UserId} in room ${event.RoomId}, reason: ${event.Reason}`
      );

      const success = closeConnectionByUserAndRoom(event.UserId, event.RoomId);
      if (!success) {
        console.warn(`Could not close connection for user ${event.UserId}`);
      }
    } catch (err) {
      console.error("Failed to process message:", err, "Raw message:", message);
    }
  }
});

redis.on("error", (err) => {
  console.error("Redis error:", err);
});

redis.on("connect", () => {
  console.log("Redis connected!");
});

redis.on("ready", () => {
  console.log("Redis ready!");
});
