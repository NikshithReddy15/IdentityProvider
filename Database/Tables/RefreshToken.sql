CREATE TABLE [dbo].[RefreshToken]
(
    [Id]           UNIQUEIDENTIFIER PRIMARY KEY
        CONSTRAINT DF_RefreshToken_Id DEFAULT NEWID(),
    [UserId]       UNIQUEIDENTIFIER NOT NULL,
    [SessionId]    UNIQUEIDENTIFIER NOT NULL,
    [RefreshToken] NVARCHAR(256)    NOT NULL,
    [ExpiresOn]    DATETIME         NOT NULL,
    [RevokedOn]    DATETIME         NULL,
    [DateCreated]  DATETIME         NOT NULL
        CONSTRAINT DF_RefreshToken_DateCreated DEFAULT GETUTCDATE(),

    CONSTRAINT FK_RefreshToken_User_UserId FOREIGN KEY (UserId) REFERENCES [dbo].[User](Id),
    CONSTRAINT FK_RefreshToken_LoginSession_SessionId FOREIGN KEY (SessionId) REFERENCES [dbo].[LoginSession](Id),
)
GO

CREATE UNIQUE NONCLUSTERED INDEX UQ_IX_RefreshToken_UserId_SessionId_Active
    ON [dbo].[RefreshToken](UserId, SessionId)
    WHERE RevokedOn IS NULL
GO

CREATE NONCLUSTERED INDEX IX_RefreshToken_UserId_SessionId
    ON [dbo].[RefreshToken](UserId, SessionId)
GO

CREATE NONCLUSTERED INDEX IX_RefreshToken_RefreshToken
    ON [dbo].[RefreshToken](RefreshToken)
