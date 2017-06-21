USE [DotNetNancyCentralAuditStore]
GO

/****** Object:  StoredProcedure [dbo].[PurgeAuditNAuditParams]    Script Date: 06/27/2011 15:03:39 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[PurgeAuditNAuditParams]  
AS
BEGIN
	SET NOCOUNT ON  
	
	DELETE FROM 
		dbo.AuditParameters
	DELETE FROM
		dbo.Audit
END

GO

