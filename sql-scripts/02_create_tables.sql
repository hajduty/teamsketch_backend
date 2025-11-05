-- Create database if it doesn't exist
CREATE DATABASE IF NOT EXISTS TS;

-- Switch to the TS database
USE TS;

-- Create Users table
CREATE TABLE IF NOT EXISTS Users (
    Id CHAR(36) NOT NULL PRIMARY KEY DEFAULT (UUID()),
    Email VARCHAR(255) NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL
);

-- Create Permissions table
CREATE TABLE IF NOT EXISTS Permissions (
    UserId VARCHAR(255) NOT NULL,
    UserEmail VARCHAR(255) NOT NULL,
    Room VARCHAR(255) NOT NULL,
    Role VARCHAR(100) NOT NULL,
    CreatedAt DATETIME(6) NOT NULL DEFAULT (UTC_TIMESTAMP(6)),
    PRIMARY KEY (UserId, Room)
);
