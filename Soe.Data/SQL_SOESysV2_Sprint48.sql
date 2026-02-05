USE [soesysv2]
GO

INSERT INTO SysVatAccount (SysVatAccountId, VatNr1, VatNr2, Name)
VALUES (121, 923, 1, 'Marginaaliveron peruste 24%')

INSERT INTO SysVatRate (SysVatAccountId, VatRate, Date, IsActive)
VALUES (121, 24, getdate(), 1)


/*
   den 5 maj 201408:10:15
   User: dba
   Server: extra01\xedev
   Database: soesysv2
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
ALTER TABLE dbo.SysTerm
	DROP CONSTRAINT FK__Ledtext_LedTyp
GO
ALTER TABLE dbo.SysTermGroup SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.SysTermGroup', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.SysTermGroup', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.SysTermGroup', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
CREATE TABLE dbo.Tmp_SysTerm
	(
	SysTermId int NOT NULL,
	SysTermGroupId int NOT NULL,
	LangId int NOT NULL,
	Name nvarchar(MAX) NOT NULL,
	Tooltip nvarchar(255) NULL,
	Description nvarchar(255) NULL,
	Created datetime NULL,
	CreatedBy nvarchar(50) NULL,
	Modified datetime NULL,
	ModifiedBy nvarchar(50) NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_SysTerm SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM dbo.SysTerm)
	 EXEC('INSERT INTO dbo.Tmp_SysTerm (SysTermId, SysTermGroupId, LangId, Name, Tooltip, Description, Created, CreatedBy, Modified, ModifiedBy)
		SELECT SysTermId, SysTermGroupId, LangId, CONVERT(nvarchar(MAX), Name), Tooltip, Description, Created, CreatedBy, Modified, ModifiedBy FROM dbo.SysTerm WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE dbo.SysTerm
GO
EXECUTE sp_rename N'dbo.Tmp_SysTerm', N'SysTerm', 'OBJECT' 
GO
ALTER TABLE dbo.SysTerm ADD CONSTRAINT
	PK_SysTerm PRIMARY KEY NONCLUSTERED 
	(
	SysTermId,
	SysTermGroupId,
	LangId
	) WITH( PAD_INDEX = OFF, FILLFACTOR = 70, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE CLUSTERED INDEX PK_SysTermId_SysTermGroupId_LangId ON dbo.SysTerm
	(
	SysTermId,
	SysTermGroupId,
	LangId
	) WITH( PAD_INDEX = OFF, FILLFACTOR = 70, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX XE_Q_SysTermGroupId_LangId ON dbo.SysTerm
	(
	SysTermGroupId,
	LangId
	) WITH( PAD_INDEX = OFF, FILLFACTOR = 70, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX XE_FK_SysTermGroupId ON dbo.SysTerm
	(
	SysTermGroupId
	) INCLUDE (SysTermId, LangId, Name) 
 WITH( PAD_INDEX = OFF, FILLFACTOR = 70, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX XE_SysTermGroupId_LangId ON dbo.SysTerm
	(
	SysTermGroupId,
	LangId
	) INCLUDE (SysTermId, Name, Tooltip, Description, Created, CreatedBy, Modified, ModifiedBy) 
 WITH( PAD_INDEX = OFF, FILLFACTOR = 70, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE dbo.SysTerm ADD CONSTRAINT
	FK__Ledtext_LedTyp FOREIGN KEY
	(
	SysTermGroupId
	) REFERENCES dbo.SysTermGroup
	(
	SysTermGroupId
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
CREATE TRIGGER [dbo].[SetCreated] 
   ON  dbo.SysTerm 
   AFTER INSERT
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    UPDATE SysTerm
    SET Created = GETDATE()
    WHERE EXISTS (SELECT 1 FROM inserted
				  WHERE SysTermId = SysTerm.SysTermId AND
				  SysTermGroupId = SysTerm.SysTermGroupId AND
				  LangId = SysTerm.LangId AND
				  SysTerm.Created IS NULL)
END
GO
CREATE TRIGGER [dbo].[SetModified] 
   ON  dbo.SysTerm 
   AFTER UPDATE
AS
IF NOT UPDATE(Created) AND NOT UPDATE(Modified)
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    UPDATE SysTerm
    SET Modified = GETDATE()
    WHERE EXISTS (SELECT 1 FROM inserted
				  WHERE SysTermId = SysTerm.SysTermId AND
				  SysTermGroupId = SysTerm.SysTermGroupId AND
				  LangId = SysTerm.LangId)
END
GO
COMMIT
select Has_Perms_By_Name(N'dbo.SysTerm', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.SysTerm', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.SysTerm', 'Object', 'CONTROL') as Contr_Per 

