import grpc from "@grpc/grpc-js";
import protoLoader from "@grpc/proto-loader";
import dotenv from "dotenv";
import fs from "fs";

dotenv.config();

var protoPath = process.env.PROTO_PATH ?? "../Shared/Contracts/Protos/permission_service.proto";
var certPath = process.env.CERT_PATH ?? "../Shared/Certs/server.crt";

// Load the .proto
const packageDef = protoLoader.loadSync(protoPath, {
  keepCase: true,
  longs: String,
  enums: String,
  defaults: true,
  oneofs: true,
});

const rootCert = fs.readFileSync(certPath);
const creds = grpc.credentials.createSsl(rootCert);

// Load package
const grpcObj = grpc.loadPackageDefinition(packageDef);
const permissionPackage = grpcObj.permission;

// Create the client
// @ts-ignore
const client = new permissionPackage.Permission(
  process.env.PERMISSION_SERVICE_URL || "localhost:7122", // e.g. "localhost:50051"
  creds
);

console.log(process.env.PERMISSION_SERVICE_URL);

export async function checkPermissionFromUrl(url) {
  try {
    const parts = url.split("/").filter(Boolean);
    const room = parts[0];
    const token = parts[1];

    //console.log("Checking for perms.");

    if (!room || !token) {
      console.log("No token, or roomId");
      return null
     };

    return await new Promise((resolve) => {
      client.CheckPermission({ token, room }, (err, response) => {
        if (err || !response || response.role === "None") {
          // Reject connection
          //console.log("Response: ");
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