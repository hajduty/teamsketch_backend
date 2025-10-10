IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Users')
BEGIN
    CREATE TABLE [dbo].[Users] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        [Email] NVARCHAR(255) NOT NULL,
        [PasswordHash] NVARCHAR(255) NOT NULL
    );
END;

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Permissions')
BEGIN
    CREATE TABLE [dbo].[Permissions] (
        [UserId] NVARCHAR(255) NOT NULL,
        [UserEmail] NVARCHAR(255) NOT NULL,
        [Room] NVARCHAR(255) NOT NULL,
        [Role] NVARCHAR(100) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT [PK_Permissions] PRIMARY KEY ([UserId], [Room])
    );
END;