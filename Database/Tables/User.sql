CREATE TABLE [dbo].[User]
(
    [Id]            INT IDENTITY PRIMARY KEY,
    [FirstName]     NVARCHAR(64)  NOT NULL,
    [MiddleName]    NVARCHAR(64)  NULL,
    [LastName]      NVARCHAR(64)  NOT NULL,
    [Email]         NVARCHAR(64)  NOT NULL,
    [CountryCode]   NVARCHAR(4)   NULL,
    [MobileNumber]  NVARCHAR(17)  NULL,
    [Password]      NVARCHAR(MAX) NOT NULL,
    [AccountStatus] TINYINT       NOT NULL
        CONSTRAINT DF_User_AccountStatus DEFAULT 0,
    [DateCreated]   DATETIME      NOT NULL
        CONSTRAINT DF_User_DateCreated DEFAULT GETUTCDATE(),
    [DateModified]  DATETIME      NOT NULL
        CONSTRAINT DF_User_DateModified DEFAULT GETUTCDATE(),
)

CREATE NONCLUSTERED INDEX IX_User_Email
    ON [dbo].[User](Email)