USE [DotNetNancyCentralAuditStore]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE  PROCEDURE [dbo].[UpsertAuditParameters]
  @AuditID uniqueidentifier,
  @ParamName varchar(100) = NULL,
  @ParamValue varchar(100) = NULL
AS
IF (SELECT COUNT(*) FROM AuditParameters WHERE AuditID = @AuditID and ParamName = @ParamName) = 0
	BEGIN
		INSERT INTO AuditParameters
		(AuditID,ParamName,ParamValue)
		VALUES (@AuditID,@ParamName,@ParamValue)
	END
ELSE
	BEGIN
		UPDATE AuditParameters
		SET  ParamValue = @ParamValue
		WHERE AuditID = @AuditID
		AND ParamName = @ParamName
	END
	
	
	RETURN


GO


