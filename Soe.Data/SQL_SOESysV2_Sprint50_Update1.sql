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
ALTER TABLE dbo.SysScheduledJob ADD
	Type int NOT NULL CONSTRAINT DF_SysScheduledJob_Type_1 DEFAULT ((0))
GO
ALTER TABLE dbo.SysScheduledJob SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.SysScheduledJob', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.SysScheduledJob', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.SysScheduledJob', 'Object', 'CONTROL') as Contr_Per 

GO



INSERT INTO SysWholesellerEdi (SysWholesellerEdiId, SenderId, SenderName, EdiFolder, FtpUser, FtpPassword, EdiManagerType)
VALUES(20, 'DA', 'Dahl', 'Dahl', 'Dahl', 'BhG¤uRL', 4)