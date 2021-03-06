

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
COMMIT
select Has_Perms_By_Name(N'dbo.Application', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Application', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Application', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
COMMIT
select Has_Perms_By_Name(N'dbo.Status', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Status', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Status', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
COMMIT
select Has_Perms_By_Name(N'dbo.Origination', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Origination', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Origination', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
COMMIT
select Has_Perms_By_Name(N'dbo.AuditCategory', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.AuditCategory', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.AuditCategory', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.Audit
	DROP CONSTRAINT DF_Audit_AuditID
GO
CREATE TABLE dbo.Tmp_Audit
	(
	AuditID uniqueidentifier NOT NULL,
	AuditCategoryID uniqueidentifier NOT NULL,
	AuditCategoryName nvarchar(255) NOT NULL,
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
ALTER TABLE dbo.Tmp_Audit ADD CONSTRAINT
	DF_Audit_AuditID DEFAULT (newid()) FOR AuditID
GO
IF EXISTS(SELECT * FROM dbo.Audit)
	 EXEC('INSERT INTO dbo.Tmp_Audit (AuditID, AuditCategoryID, OriginationID, RecordLocator, StatusID, DateTimeStamp, Message, QNumber, QCategory, PCC, ApplicationID)
		SELECT AuditID, AuditCategoryID, OriginationID, RecordLocator, StatusID, DateTimeStamp, Message, QNumber, QCategory, PCC, ApplicationID FROM dbo.Audit WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE dbo.Audit
GO
EXECUTE sp_rename N'dbo.Tmp_Audit', N'Audit', 'OBJECT' 
GO
ALTER TABLE dbo.Audit ADD CONSTRAINT
	PK_Audit PRIMARY KEY CLUSTERED 
	(
	AuditID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.Audit ADD CONSTRAINT
	FK_Audit_AuditCategory FOREIGN KEY
	(
	AuditCategoryID,
	AuditCategoryName
	) REFERENCES dbo.AuditCategory
	(
	AuditCategoryID,
	AuditCategoryName
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Audit ADD CONSTRAINT
	FK_Audit_Origination FOREIGN KEY
	(
	OriginationID
	) REFERENCES dbo.Origination
	(
	OriginationID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Audit ADD CONSTRAINT
	FK_Audit_Status FOREIGN KEY
	(
	StatusID
	) REFERENCES dbo.Status
	(
	StatusID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Audit ADD CONSTRAINT
	FK_Audit_Application FOREIGN KEY
	(
	ApplicationID
	) REFERENCES dbo.Application
	(
	ApplicationID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
select Has_Perms_By_Name(N'dbo.Audit', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Audit', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Audit', 'Object', 'CONTROL') as Contr_Per 