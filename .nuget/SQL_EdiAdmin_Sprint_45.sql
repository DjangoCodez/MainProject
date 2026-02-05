/*
den 12 december 201314:29:09
User: dba
Server: dell-db14\xedev
Database: ediadmin
Application:
*/

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
ALTER TABLE dbo.EdiCustomer ADD
Type int NOT NULL CONSTRAINT DF_EdiCustomer_Type DEFAULT 0
GO
ALTER TABLE dbo.EdiCustomer SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.EdiCustomer', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.EdiCustomer', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.EdiCustomer', 'Object', 'CONTROL') as Contr_Per

GO