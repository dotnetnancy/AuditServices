
/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.Origination
	(
	OriginationID uniqueidentifier NOT NULL,
	OriginationName nvarchar(50) NOT NULL,
	OriginationDescription nvarchar(1024) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Origination ADD CONSTRAINT
	DF_Origination_OriginationID DEFAULT newid() FOR OriginationID
GO
ALTER TABLE dbo.Origination ADD CONSTRAINT
	PK_Origination PRIMARY KEY CLUSTERED 
	(
	OriginationID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE UNIQUE NONCLUSTERED INDEX IX_Origination ON dbo.Origination
	(
	OriginationName
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
COMMIT
select Has_Perms_By_Name(N'dbo.Origination', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Origination', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Origination', 'Object', 'CONTROL') as Contr_Per 