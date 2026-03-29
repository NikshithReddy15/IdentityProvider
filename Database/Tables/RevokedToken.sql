CREATE TABLE [dbo].[RevokedToken]
(
    [Id]          UNIQUEIDENTIFIER PRIMARY KEY
        CONSTRAINT DF_RevokedToken_Id DEFAULT NEWID(),
    [HashedToken] NVARCHAR(256) NOT NULL,
    [ExpiresOn]   DATETIME      NOT NULL,
    [DateCreated] DATETIME      NOT NULL
        CONSTRAINT DF_RevokedToken_DateCreated DEFAULT GETUTCDATE(),
)
GO

CREATE NONCLUSTERED INDEX IX_RevokedToken_HashedToken_ExpiresOn
    ON [dbo].[RevokedToken](HashedToken, ExpiresOn)