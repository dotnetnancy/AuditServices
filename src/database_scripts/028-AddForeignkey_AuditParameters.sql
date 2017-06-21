USE [DotNetNancyCentralAuditStore]
GO

ALTER TABLE [dbo].[AuditParameters]  WITH NOCHECK ADD  CONSTRAINT [FK_AuditParameters_Audit] FOREIGN KEY([AuditID])
REFERENCES [dbo].[Audit] ([AuditID])
GO

ALTER TABLE [dbo].[AuditParameters] CHECK CONSTRAINT [FK_AuditParameters_Audit]
GO

