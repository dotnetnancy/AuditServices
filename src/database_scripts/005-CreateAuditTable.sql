
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
CREATE TABLE dbo.Audit
	(
	AuditID uniqueidentifier NOT NULL,
	AuditCategoryID uniqueidentifier NOT NULL,
	OriginationID uniqueidentifier NOT NULL,
	RecordLocator nvarchar(50) NOT NULL,
	StatusID uniqueidentifier NOT NULL,
	DateTimeStamp datetime NOT NULL,
	Message nvarchar(1024) NOT NULL,
	QNumber int NULL,
	QCategory int NULL,
	PCC nvarchar(50) NULL,
	ApplicationID uniqueidentifier NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Audit ADD CONSTRAINT
	PK_Audit PRIMARY KEY CLUSTERED 
	(
	AuditID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT
select Has_Perms_By_Name(N'dbo.Audit', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Audit', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Audit', 'Object', 'CONTROL') as Contr_Per 