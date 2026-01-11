import grpc from "@grpc/grpc-js";
import protoLoader from "@grpc/proto-loader";
import dotenv from "dotenv";
// @ts-ignore
import fs from "fs";

dotenv.config();

var protoPath = process.env.PROTO_PATH ?? "../Shared/Contracts/Protos/permission_service.proto";
//var certPath = process.env.CERT_PATH ?? "../Shared/Certs/server.crt";

// Load the .proto
const packageDef = protoLoader.loadSync(protoPath, {
  keepCase: true,
  longs: String,
  enums: String,
  defaults: true,
  oneofs: true,
});

//const rootCert = fs.readFileSync(certPath);
//const creds = grpc.credentials.createSsl(rootCert);

const creds = grpc.credentials.createInsecure();

// Load package
const grpcObj = grpc.loadPackageDefinition(packageDef);
const permissionPackage = grpcObj.permission;

// @ts-ignore
const client = new permissionPackage.Permission(
  process.env.PERMISSION_SERVICE_URL || "localhost:7122", // e.g. "localhost:50051"
  creds
);

/**
 * @param {string} room
 * @param {string} token
 */
export async function checkPermissionFromUrl(room, token) {
  try {
    console.log("Checking for perms.");

    if (!room || !token) {
      console.log("No token, or roomId");
      return null
     };

    return await new Promise((resolve) => {
      // @ts-ignore
      client.CheckPermission({ token, room }, (err, response) => {
        if (err || !response || response.role === "None") {
          // Reject connection
          console.log(token, err);
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
