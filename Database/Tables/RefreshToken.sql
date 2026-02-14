CREATE TABLE [dbo].[RefreshToken]
(
    [Id]           UNIQUEIDENTIFIER PRIMARY KEY
        CONSTRAINT DF_RefreshToken_Id DEFAULT NEWID(),
    [UserId]       UNIQUEIDENTIFIER NOT NULL,
    [RefreshToken] NVARCHAR(256)    NOT NULL,
    [ExpiresOn]    DATETIME         NOT NULL,
    [IsRevoked]    BIT              NOT NULL
        CONSTRAINT DF_RefreshToken_IsRevoked DEFAULT 0,
    [RevokedOn]    DATETIME         NULL,
    [DateCreated]  DATETIME         NOT NULL
        CONSTRAINT DF_RefreshToken_DateCreated DEFAULT GETUTCDATE(),

    CONSTRAINT FK_RefreshToken_UserId FOREIGN KEY (UserId) REFERENCES [dbo].[User](Id),
)

CREATE UNIQUE INDEX UQ_IX_RefreshToken_UserId_IsRevoked
    ON [dbo].[RefreshToken](UserId, IsRevoked)
    WHERE IsRevoked = 0

CREATE NONCLUSTERED INDEX IX_RefreshToken_RefreshToken
    ON [dbo].[RefreshToken](RefreshToken)