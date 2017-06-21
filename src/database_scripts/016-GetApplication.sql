USE [DotNetNancyCentralAuditStore]
GO
/****** Object:  StoredProcedure [dbo].[GetApplication]    Script Date: 12/07/2010 15:11:18 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetApplication]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetApplication]
GO


SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[GetApplication]
@ApplicationName nvarchar(255),
@ApplicationID uniqueidentifier output,
@ApplicationDescription nvarchar(1024) = 'Application Description Not Provided, automated Process'
AS

if exists(
Select ApplicationID
From Application 
where ApplicationName = @ApplicationName)
	
	begin

	set @ApplicationID = (Select ApplicationID
	From Application 
	where ApplicationName = @ApplicationName)
	
	end

else
	begin
		set @ApplicationID = newid();
		insert  Application (ApplicationID, ApplicationName, ApplicationDescription)
		values( @ApplicationID, @ApplicationName, @ApplicationDescription)
		
	end


