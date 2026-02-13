CREATE TABLE [dbo].[RevokedToken]
(
    [Id]          UNIQUEIDENTIFIER PRIMARY KEY
        CONSTRAINT DF_RevokedToken_Id DEFAULT NEWID(),
    [HashedToken] NVARCHAR(256) NOT NULL,
    [GeneratedOn] DATETIME      NOT NULL
        CONSTRAINT DF_RevokedToken_GeneratedOn DEFAULT GETUTCDATE(),
    [ExpiresOn]   DATETIME      NOT NULL,
)

CREATE NONCLUSTERED INDEX IX_RevokedToken_HashedToken_ExpiresOn
    ON [dbo].[RevokedToken](HashedToken, ExpiresOn)