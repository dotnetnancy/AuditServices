

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
CREATE TABLE dbo.AuditCategory
	(
	AuditCategoryID uniqueidentifier NOT NULL,
	AuditCategoryName nvarchar(255) NOT NULL,
	AuditCategoryDescription nvarchar(1024) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.AuditCategory ADD CONSTRAINT
	DF_AuditCategory_AuditCategoryID DEFAULT newid() FOR AuditCategoryID
GO
COMMIT
select Has_Perms_By_Name(N'dbo.AuditCategory', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.AuditCategory', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.AuditCategory', 'Object', 'CONTROL') as Contr_Per 