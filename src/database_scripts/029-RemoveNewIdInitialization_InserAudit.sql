USE [DotNetNancyCentralAuditStore]
GO

/****** Object:  StoredProcedure [dbo].[InsertAudit]    Script Date: 06/28/2011 09:26:04 ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO

ALTER PROCEDURE [dbo].[InsertAudit]
	@AuditID [uniqueidentifier] OUTPUT ,
	@AuditCategoryID [uniqueidentifier],
	@AuditCategoryName [nvarchar](255),
	@OriginationID [uniqueidentifier],
	@StatusID [uniqueidentifier],
	@DateTimeStamp [datetime],
	@Message [nvarchar](1024),
	@ApplicationID [uniqueidentifier],
	@ReferenceID nvarchar(255) = null,
	@ParameterDictionary varchar(max) = null
AS

Insert Into Audit 
( AuditID, AuditCategoryID, AuditCategoryName, OriginationID,  StatusID, DateTimeStamp, Message,    ApplicationID, ReferenceID, ParameterDictionary)
Values 
( @AuditID, @AuditCategoryID, @AuditCategoryName, @OriginationID,  @StatusID, @DateTimeStamp, @Message, @ApplicationID, @ReferenceID, @ParameterDictionary) 

GO

