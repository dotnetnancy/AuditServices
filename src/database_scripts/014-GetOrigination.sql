USE [DotNetNancyCentralAuditStore]
GO
/****** Object:  StoredProcedure [dbo].[GetOrigination]    Script Date: 12/07/2010 15:11:18 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetOrigination]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetOrigination]
GO


SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[GetOrigination]
@OriginationName nvarchar(255),
@OriginationID uniqueidentifier output,
@OriginationDescription nvarchar(1024) = 'Origination Description Not Provided, automated Process'
AS

if exists(
Select OriginationID
From Origination 
where OriginationName = @OriginationName)
	
	begin

	set @OriginationID = (Select OriginationID
	From Origination 
	where OriginationName = @OriginationName)
	
	end

else
	begin
		set @OriginationID = newid();
		insert  Origination (OriginationID, OriginationName, OriginationDescription)
		values( @OriginationID, @OriginationName, @OriginationDescription)
		
	end


