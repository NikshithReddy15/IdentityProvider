CREATE TABLE [dbo].[LoginSession]
(
    [Id]           UNIQUEIDENTIFIER PRIMARY KEY,
    [UserId]       UNIQUEIDENTIFIER NOT NULL,
    [DeviceId]     UNIQUEIDENTIFIER NOT NULL,
    [UserAgent]    NVARCHAR(1024)   NOT NULL,
    [IpAddress]    NVARCHAR(45)     NULL,
    [IsActive]     BIT              NOT NULL
        CONSTRAINT DF_LoginSession_IsActive DEFAULT 1,
    [LastSeenOn]   DATETIME         NOT NULL
        CONSTRAINT DF_LoginSession_LastSeenOn DEFAULT GETUTCDATE(),
    [DateCreated]  DATETIME         NOT NULL
        CONSTRAINT DF_LoginSession_DateCreated DEFAULT GETUTCDATE(),

    CONSTRAINT FK_LoginSession_User_UserId FOREIGN KEY (UserId) REFERENCES [dbo].[User] (Id),
)