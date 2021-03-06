

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
CREATE TABLE dbo.AuditParameters
	(
	AuditID uniqueidentifier NOT NULL,
	ParamName varchar(100) NOT NULL,
	ParamValue varchar(100) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.AuditParameters ADD CONSTRAINT
	PK_AuditParameters PRIMARY KEY CLUSTERED 
	(
	AuditID,
	ParamName
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.AuditParameters SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.AuditParameters', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.AuditParameters', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.AuditParameters', 'Object', 'CONTROL') as Contr_Per 