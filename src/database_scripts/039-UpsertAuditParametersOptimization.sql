USE [DotNetNancyCentralAuditStore]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpsertAuditParameters]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpsertAuditParameters]
go

-- =============================================
-- Author:		Pranot Bhosale
-- Created date: 07/18/2011
-- Description:	
-- =============================================
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE  PROCEDURE [dbo].[UpsertAuditParameters]
  @AuditID UNIQUEIDENTIFIER
  , @ParamName varchar(100)
  , @ParamValue varchar(100)
AS
BEGIN
	
	
		INSERT INTO AuditParameters
		(AuditID,ParamName,ParamValue)
		VALUES (@AuditID,@ParamName,@ParamValue)
	
		
	RETURN
END

GO


