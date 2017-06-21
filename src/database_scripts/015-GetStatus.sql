USE [DotNetNancyCentralAuditStore]
GO
/****** Object:  StoredProcedure [dbo].[GetStatus]    Script Date: 12/07/2010 15:11:18 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetStatus]
GO


SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[GetStatus]
@StatusName nvarchar(255),
@StatusID uniqueidentifier output,
@StatusDescription nvarchar(1024) = 'Status Description Not Provided, automated Process'
AS

if exists(
Select StatusID
From Status 
where StatusName = @StatusName)
	
	begin

	set @StatusID = (Select StatusID
	From Status 
	where StatusName = @StatusName)
	
	end

else
	begin
		set @StatusID = newid();
		insert  Status (StatusID, StatusName, StatusDescription)
		values( @StatusID, @StatusName, @StatusDescription)
		
	end


