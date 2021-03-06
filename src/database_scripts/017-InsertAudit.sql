USE [DotNetNancyCentralAuditStore]
GO
/****** Object:  StoredProcedure [dbo].[InsertAudit]    Script Date: 12/07/2010 16:41:09 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InsertAudit]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[InsertAudit]
go

USE [DotNetNancyCentralAuditStore]
GO
/****** Object:  StoredProcedure [dbo].[InsertAudit]    Script Date: 12/07/2010 16:41:28 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[InsertAudit]
	@AuditID [uniqueidentifier] OUTPUT ,
	@AuditCategoryID [uniqueidentifier],
	@AuditCategoryName [nvarchar](255),
	@OriginationID [uniqueidentifier],
	@RecordLocator [nvarchar](50),
	@StatusID [uniqueidentifier],
	@DateTimeStamp [datetime],
	@Message [nvarchar](1024),
	@QNumber [int],
	@QCategory [int],
	@PCC [nvarchar](50),
	@ApplicationID [uniqueidentifier]
AS

set @AuditID = newid()

Insert Into Audit 
( AuditID, AuditCategoryID, AuditCategoryName, OriginationID, RecordLocator, StatusID, DateTimeStamp, Message, QNumber, QCategory, PCC, ApplicationID)
Values ( @AuditID, @AuditCategoryID, @AuditCategoryName, @OriginationID, @RecordLocator, @StatusID, @DateTimeStamp, @Message, @QNumber, @QCategory, @PCC, @ApplicationID) 
