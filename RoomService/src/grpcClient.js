import grpc from "@grpc/grpc-js";
import protoLoader from "@grpc/proto-loader";
import dotenv from "dotenv";
import fs from "fs";

dotenv.config();

// Load the .proto
const packageDef = protoLoader.loadSync("../Shared/Contracts/Protos/permission_service.proto", {
  keepCase: true,
  longs: String,
  enums: String,
  defaults: true,
  oneofs: true,
});

const rootCert = fs.readFileSync("../Shared/Certs/server.crt");
const creds = grpc.credentials.createSsl(rootCert);

// Load package
const grpcObj = grpc.loadPackageDefinition(packageDef);
const permissionPackage = grpcObj.permission;

// Create the client
// @ts-ignore
const client = new permissionPackage.Permission(
  "localhost:7122", // e.g. "localhost:50051"
  creds
);

export async function checkPermissionFromUrl(url) {
  try {
    const parts = url.split("/").filter(Boolean);
    const room = parts[0];
    const token = parts[1];

    if (!room || !token) return null;

    return await new Promise((resolve) => {
      client.CheckPermission({ token, room }, (err, response) => {
        if (err || !response || response.role === "None") {
          // Reject connection
          console.log(response);
          return resolve(null);
        }
        resolve(response);
      });
    });
  } catch (err) {
    console.error("gRPC permission check failed:", err);
    return null;
  }
}