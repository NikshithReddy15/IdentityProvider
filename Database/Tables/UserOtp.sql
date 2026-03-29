CREATE TABLE [dbo].[UserOtp]
(
    [Id]          UNIQUEIDENTIFIER PRIMARY KEY
        CONSTRAINT DF_UserOtp_Id DEFAULT NEWID(),
    [UserId]      UNIQUEIDENTIFIER NOT NULL,
    [OtpCode]     NVARCHAR(10)     NOT NULL,
    [ExpiresOn]   DATETIME         NOT NULL,
    [IsUsed]      BIT              NOT NULL
        CONSTRAINT DF_UserOtp_IsUsed DEFAULT 0,
    [DateCreated] DATETIME         NOT NULL
        CONSTRAINT DF_UserOtp_DateCreated DEFAULT GETUTCDATE(),

    CONSTRAINT FK_UserOtp_User_UserId FOREIGN KEY (UserId) REFERENCES [dbo].[User] (Id),
)
