CREATE TABLE [dbo].[LoginSession]
(
    [Id]             UNIQUEIDENTIFIER PRIMARY KEY
        CONSTRAINT DF_LoginSession_Id DEFAULT NEWID(),
    [UserId]         UNIQUEIDENTIFIER NOT NULL,
    [DeviceId]       UNIQUEIDENTIFIER NOT NULL,
    [UserAgent]      NVARCHAR(1024)   NOT NULL,
    [IpAddress]      NVARCHAR(45)     NULL,
    [LastSeenOn]     DATETIME         NOT NULL
        CONSTRAINT DF_LoginSession_LastSeenOn DEFAULT GETUTCDATE(),
    [DeactivatedOn]  DATETIME         NULL,
    [DateCreated]    DATETIME         NOT NULL
        CONSTRAINT DF_LoginSession_DateCreated DEFAULT GETUTCDATE(),

    CONSTRAINT FK_LoginSession_User_UserId FOREIGN KEY (UserId) REFERENCES [dbo].[User] (Id),
)
GO

CREATE NONCLUSTERED INDEX IX_LoginSession_UserId
    ON [dbo].[LoginSession](UserId)
