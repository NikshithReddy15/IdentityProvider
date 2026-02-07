CREATE TABLE [dbo].[User]
(
    [Id]            INT           IDENTITY PRIMARY KEY,
    [FirstName]     NVARCHAR(MAX) NOT NULL,
    [MiddleName]    NVARCHAR(MAX) NULL,
    [LastName]      NVARCHAR(MAX) NOT NULL,
    [Email]         NVARCHAR(MAX) NOT NULL,
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