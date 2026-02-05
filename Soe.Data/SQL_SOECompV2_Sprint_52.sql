update Employment set DateFrom = CAST('1900-01-01' AS DATETIME)
  where DateFrom is null
  GO


  /*
   den 4 december 201408:17:59
   User: dba
   Server: extra01\xedev
   Database: soecompv_education
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
ALTER TABLE dbo.PayrollGroupPriceType
	DROP CONSTRAINT FK_PayrollGroupPriceType_PayrollPriceType
GO
ALTER TABLE dbo.PayrollPriceType SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.PayrollPriceType', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.PayrollPriceType', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.PayrollPriceType', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.PayrollGroupPriceType
	DROP CONSTRAINT FK_PayrollGroupPriceType_PayrollGroup
GO
ALTER TABLE dbo.PayrollGroup SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.PayrollGroup', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.PayrollGroup', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.PayrollGroup', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.PayrollGroupPriceType
	DROP CONSTRAINT DF_PayrollGroupPriceType_ShowOnEmployee
GO
ALTER TABLE dbo.PayrollGroupPriceType
	DROP CONSTRAINT DF_PayrollGroupPriceType_ReadOnlyOnEmployee
GO
ALTER TABLE dbo.PayrollGroupPriceType
	DROP CONSTRAINT DF_PayrollGroupPriceType_State
GO
CREATE TABLE dbo.Tmp_PayrollGroupPriceType
	(
	PayrollGroupPriceTypeId int NOT NULL IDENTITY (1, 1),
	PayrollGroupId int NOT NULL,
	PayrollPriceTypeId int NOT NULL,
	Sort int NOT NULL,
	ShowOnEmployee bit NOT NULL,
	ReadOnlyOnEmployee bit NOT NULL,
	Created datetime NULL,
	CreatedBy nvarchar(50) NULL,
	Modified datetime NULL,
	ModifiedBy nvarchar(50) NULL,
	State int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_PayrollGroupPriceType SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_PayrollGroupPriceType ADD CONSTRAINT
	DF_PayrollGroupPriceType_Sort DEFAULT 0 FOR Sort
GO
ALTER TABLE dbo.Tmp_PayrollGroupPriceType ADD CONSTRAINT
	DF_PayrollGroupPriceType_ShowOnEmployee DEFAULT ((0)) FOR ShowOnEmployee
GO
ALTER TABLE dbo.Tmp_PayrollGroupPriceType ADD CONSTRAINT
	DF_PayrollGroupPriceType_ReadOnlyOnEmployee DEFAULT ((0)) FOR ReadOnlyOnEmployee
GO
ALTER TABLE dbo.Tmp_PayrollGroupPriceType ADD CONSTRAINT
	DF_PayrollGroupPriceType_State DEFAULT ((0)) FOR State
GO
SET IDENTITY_INSERT dbo.Tmp_PayrollGroupPriceType ON
GO
IF EXISTS(SELECT * FROM dbo.PayrollGroupPriceType)
	 EXEC('INSERT INTO dbo.Tmp_PayrollGroupPriceType (PayrollGroupPriceTypeId, PayrollGroupId, PayrollPriceTypeId, ShowOnEmployee, ReadOnlyOnEmployee, Created, CreatedBy, Modified, ModifiedBy, State)
		SELECT PayrollGroupPriceTypeId, PayrollGroupId, PayrollPriceTypeId, ShowOnEmployee, ReadOnlyOnEmployee, Created, CreatedBy, Modified, ModifiedBy, State FROM dbo.PayrollGroupPriceType WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_PayrollGroupPriceType OFF
GO
ALTER TABLE dbo.PayrollGroupPriceTypePeriod
	DROP CONSTRAINT FK_PayrollGroupPriceTypePeriod_PayrollGroupPriceType
GO
DROP TABLE dbo.PayrollGroupPriceType
GO
EXECUTE sp_rename N'dbo.Tmp_PayrollGroupPriceType', N'PayrollGroupPriceType', 'OBJECT' 
GO
ALTER TABLE dbo.PayrollGroupPriceType ADD CONSTRAINT
	PK_PayrollGroupPriceType PRIMARY KEY CLUSTERED 
	(
	PayrollGroupPriceTypeId
	) WITH( PAD_INDEX = OFF, FILLFACTOR = 90, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX XE_Q_PayrollGroupId_PayrollPriceTypeId_State ON dbo.PayrollGroupPriceType
	(
	PayrollGroupId,
	PayrollPriceTypeId,
	State
	) WITH( PAD_INDEX = OFF, FILLFACTOR = 90, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE dbo.PayrollGroupPriceType ADD CONSTRAINT
	FK_PayrollGroupPriceType_PayrollGroup FOREIGN KEY
	(
	PayrollGroupId
	) REFERENCES dbo.PayrollGroup
	(
	PayrollGroupId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.PayrollGroupPriceType ADD CONSTRAINT
	FK_PayrollGroupPriceType_PayrollPriceType FOREIGN KEY
	(
	PayrollPriceTypeId
	) REFERENCES dbo.PayrollPriceType
	(
	PayrollPriceTypeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
select Has_Perms_By_Name(N'dbo.PayrollGroupPriceType', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.PayrollGroupPriceType', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.PayrollGroupPriceType', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.PayrollGroupPriceTypePeriod ADD CONSTRAINT
	FK_PayrollGroupPriceTypePeriod_PayrollGroupPriceType FOREIGN KEY
	(
	PayrollGroupPriceTypeId
	) REFERENCES dbo.PayrollGroupPriceType
	(
	PayrollGroupPriceTypeId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.PayrollGroupPriceTypePeriod SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.PayrollGroupPriceTypePeriod', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.PayrollGroupPriceTypePeriod', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.PayrollGroupPriceTypePeriod', 'Object', 'CONTROL') as Contr_Per 
GO
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
ALTER TABLE dbo.PayrollProductSetting ADD
	CalculateSupplementCharge bit NOT NULL CONSTRAINT DF_PayrollProductSetting_CalculateSupplementCharge DEFAULT 0
GO
ALTER TABLE dbo.PayrollProductSetting SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.PayrollProductSetting', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.PayrollProductSetting', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.PayrollProductSetting', 'Object', 'CONTROL') as Contr_Per 
GO



/*
   den 5 december 201408:33:39
   User: dba
   Server: extra01\xedev
   Database: soecompv_education
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
ALTER TABLE dbo.Employment ADD
	FixedAccounting bit NOT NULL CONSTRAINT DF_Employment_FixedAccounting DEFAULT 0
GO
ALTER TABLE dbo.Employment SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.Employment', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Employment', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Employment', 'Object', 'CONTROL') as Contr_Per 



UPDATE EmploymentAccountStd SET [Percent] = 0 WHERE [Percent] IS NULL



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
ALTER TABLE dbo.EmploymentAccountStd
	DROP CONSTRAINT FK_EmploymentAccountStd_AccountStd
GO
ALTER TABLE dbo.AccountStd SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.AccountStd', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.AccountStd', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.AccountStd', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.EmploymentAccountStd
	DROP CONSTRAINT FK_EmploymentAccountStd_Employment
GO
ALTER TABLE dbo.Employment SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.Employment', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Employment', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Employment', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.EmploymentAccountStd
	DROP CONSTRAINT DF_EmploymentAccountStd_Percent
GO
CREATE TABLE dbo.Tmp_EmploymentAccountStd
	(
	EmploymentAccountStdId int NOT NULL IDENTITY (1, 1),
	EmploymentId int NOT NULL,
	AccountId int NULL,
	Type int NOT NULL,
	[Percent] decimal(5, 2) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_EmploymentAccountStd SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_EmploymentAccountStd ADD CONSTRAINT
	DF_EmploymentAccountStd_Percent DEFAULT ((0)) FOR [Percent]
GO
SET IDENTITY_INSERT dbo.Tmp_EmploymentAccountStd ON
GO
IF EXISTS(SELECT * FROM dbo.EmploymentAccountStd)
	 EXEC('INSERT INTO dbo.Tmp_EmploymentAccountStd (EmploymentAccountStdId, EmploymentId, AccountId, Type, [Percent])
		SELECT EmploymentAccountStdId, EmploymentId, AccountId, Type, CONVERT(decimal(5, 2), [Percent]) FROM dbo.EmploymentAccountStd WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_EmploymentAccountStd OFF
GO
ALTER TABLE dbo.EmploymentAccountInternal
	DROP CONSTRAINT FK_EmploymentAccountInternal_EmploymentAccountStd
GO
DROP TABLE dbo.EmploymentAccountStd
GO
EXECUTE sp_rename N'dbo.Tmp_EmploymentAccountStd', N'EmploymentAccountStd', 'OBJECT' 
GO
ALTER TABLE dbo.EmploymentAccountStd ADD CONSTRAINT
	PK_EmploymentAccountStd PRIMARY KEY CLUSTERED 
	(
	EmploymentAccountStdId
	) WITH( PAD_INDEX = OFF, FILLFACTOR = 90, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX XE_FK_AccountId ON dbo.EmploymentAccountStd
	(
	AccountId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE dbo.EmploymentAccountStd ADD CONSTRAINT
	FK_EmploymentAccountStd_Employment FOREIGN KEY
	(
	EmploymentId
	) REFERENCES dbo.Employment
	(
	EmploymentId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.EmploymentAccountStd ADD CONSTRAINT
	FK_EmploymentAccountStd_AccountStd FOREIGN KEY
	(
	AccountId
	) REFERENCES dbo.AccountStd
	(
	AccountId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO

select Has_Perms_By_Name(N'dbo.EmploymentAccountStd', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.EmploymentAccountStd', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.EmploymentAccountStd', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.EmploymentAccountInternal ADD CONSTRAINT
	FK_EmploymentAccountInternal_EmploymentAccountStd FOREIGN KEY
	(
	EmploymentAccountStdId
	) REFERENCES dbo.EmploymentAccountStd
	(
	EmploymentAccountStdId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.EmploymentAccountInternal SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.EmploymentAccountInternal', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.EmploymentAccountInternal', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.EmploymentAccountInternal', 'Object', 'CONTROL') as Contr_Per 




/*
   den 5 december 201414:34:19
   User: dba
   Server: extra01\xedev
   Database: soecompv_education
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
ALTER TABLE dbo.PayrollProductSetting
	DROP CONSTRAINT FK_PayrollProductSetting_PayrollGroup
GO
ALTER TABLE dbo.PayrollGroup SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.PayrollGroup', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.PayrollGroup', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.PayrollGroup', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.PayrollProductSetting
	DROP CONSTRAINT FK_PayrollProductSetting_PayrollProduct
GO
ALTER TABLE dbo.PayrollProduct SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.PayrollProduct', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.PayrollProduct', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.PayrollProduct', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.PayrollProductSetting
	DROP CONSTRAINT DF_PayrollProductSetting_CentRoundingType
GO
ALTER TABLE dbo.PayrollProductSetting
	DROP CONSTRAINT DF_PayrollProductSetting_TaxCalculationType
GO
ALTER TABLE dbo.PayrollProductSetting
	DROP CONSTRAINT DF_PayrollProductSetting_PrintOnSalarySpecification
GO
ALTER TABLE dbo.PayrollProductSetting
	DROP CONSTRAINT DF_PayrollProductSetting_PrintDate
GO
ALTER TABLE dbo.PayrollProductSetting
	DROP CONSTRAINT DF_PayrollProductSetting_AccountingPrio
GO
ALTER TABLE dbo.PayrollProductSetting
	DROP CONSTRAINT DF_PayrollProductSetting_PensionCompany
GO
ALTER TABLE dbo.PayrollProductSetting
	DROP CONSTRAINT DF_PayrollProductSetting_Vacation
GO
ALTER TABLE dbo.PayrollProductSetting
	DROP CONSTRAINT DF_PayrollProductSetting_Union
GO
ALTER TABLE dbo.PayrollProductSetting
	DROP CONSTRAINT DF_PayrollProductSetting_State
GO
ALTER TABLE dbo.PayrollProductSetting
	DROP CONSTRAINT DF_PayrollProductSetting_CalculateSupplementCharge
GO
CREATE TABLE dbo.Tmp_PayrollProductSetting
	(
	PayrollProductSettingId int NOT NULL IDENTITY (1, 1),
	ProductId int NOT NULL,
	PayrollGroupId int NULL,
	CentRoundingType int NOT NULL,
	CentRoundingLevel int NOT NULL,
	TaxCalculationType int NOT NULL,
	PrintOnSalarySpecification bit NOT NULL,
	PrintDate bit NOT NULL,
	AccountingPrio nvarchar(50) NOT NULL,
	PensionCompany int NOT NULL,
	VacationSalaryPromoted bit NOT NULL,
	UnionFeePromoted bit NOT NULL,
	Created datetime NULL,
	CreatedBy nvarchar(50) NULL,
	Modified datetime NULL,
	ModifiedBy nvarchar(50) NULL,
	State int NOT NULL,
	CalculateSupplementCharge bit NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_PayrollProductSetting SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_PayrollProductSetting ADD CONSTRAINT
	DF_PayrollProductSetting_CentRoundingType DEFAULT ((0)) FOR CentRoundingType
GO
ALTER TABLE dbo.Tmp_PayrollProductSetting ADD CONSTRAINT
	DF_PayrollProductSetting_CentRoundingLevel DEFAULT 0 FOR CentRoundingLevel
GO
ALTER TABLE dbo.Tmp_PayrollProductSetting ADD CONSTRAINT
	DF_PayrollProductSetting_TaxCalculationType DEFAULT ((0)) FOR TaxCalculationType
GO
ALTER TABLE dbo.Tmp_PayrollProductSetting ADD CONSTRAINT
	DF_PayrollProductSetting_PrintOnSalarySpecification DEFAULT ((0)) FOR PrintOnSalarySpecification
GO
ALTER TABLE dbo.Tmp_PayrollProductSetting ADD CONSTRAINT
	DF_PayrollProductSetting_PrintDate DEFAULT ((0)) FOR PrintDate
GO
ALTER TABLE dbo.Tmp_PayrollProductSetting ADD CONSTRAINT
	DF_PayrollProductSetting_AccountingPrio DEFAULT (N'0,0,0,0,0,0,0') FOR AccountingPrio
GO
ALTER TABLE dbo.Tmp_PayrollProductSetting ADD CONSTRAINT
	DF_PayrollProductSetting_PensionCompany DEFAULT ((0)) FOR PensionCompany
GO
ALTER TABLE dbo.Tmp_PayrollProductSetting ADD CONSTRAINT
	DF_PayrollProductSetting_Vacation DEFAULT ((0)) FOR VacationSalaryPromoted
GO
ALTER TABLE dbo.Tmp_PayrollProductSetting ADD CONSTRAINT
	DF_PayrollProductSetting_Union DEFAULT ((0)) FOR UnionFeePromoted
GO
ALTER TABLE dbo.Tmp_PayrollProductSetting ADD CONSTRAINT
	DF_PayrollProductSetting_State DEFAULT ((0)) FOR State
GO
ALTER TABLE dbo.Tmp_PayrollProductSetting ADD CONSTRAINT
	DF_PayrollProductSetting_CalculateSupplementCharge DEFAULT ((0)) FOR CalculateSupplementCharge
GO
SET IDENTITY_INSERT dbo.Tmp_PayrollProductSetting ON
GO
IF EXISTS(SELECT * FROM dbo.PayrollProductSetting)
	 EXEC('INSERT INTO dbo.Tmp_PayrollProductSetting (PayrollProductSettingId, ProductId, PayrollGroupId, CentRoundingType, TaxCalculationType, PrintOnSalarySpecification, PrintDate, AccountingPrio, PensionCompany, VacationSalaryPromoted, UnionFeePromoted, Created, CreatedBy, Modified, ModifiedBy, State, CalculateSupplementCharge)
		SELECT PayrollProductSettingId, ProductId, PayrollGroupId, CentRoundingType, TaxCalculationType, PrintOnSalarySpecification, PrintDate, AccountingPrio, PensionCompany, VacationSalaryPromoted, UnionFeePromoted, Created, CreatedBy, Modified, ModifiedBy, State, CalculateSupplementCharge FROM dbo.PayrollProductSetting WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_PayrollProductSetting OFF
GO
ALTER TABLE dbo.PayrollProductPriceFormula
	DROP CONSTRAINT FK_PayrollProductPriceFormula_PayrollProductSetting
GO
ALTER TABLE dbo.PayrollProductPriceType
	DROP CONSTRAINT FK_PayrollProductPriceType_PayrollProductSetting
GO
ALTER TABLE dbo.PayrollProductAccountStd
	DROP CONSTRAINT FK_PayrollProductAccountStd_PayrollProductSetting
GO
DROP TABLE dbo.PayrollProductSetting
GO
EXECUTE sp_rename N'dbo.Tmp_PayrollProductSetting', N'PayrollProductSetting', 'OBJECT' 
GO
ALTER TABLE dbo.PayrollProductSetting ADD CONSTRAINT
	PK_PayrollProductSetting PRIMARY KEY CLUSTERED 
	(
	PayrollProductSettingId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.PayrollProductSetting ADD CONSTRAINT
	FK_PayrollProductSetting_PayrollProduct FOREIGN KEY
	(
	ProductId
	) REFERENCES dbo.PayrollProduct
	(
	ProductId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.PayrollProductSetting ADD CONSTRAINT
	FK_PayrollProductSetting_PayrollGroup FOREIGN KEY
	(
	PayrollGroupId
	) REFERENCES dbo.PayrollGroup
	(
	PayrollGroupId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
select Has_Perms_By_Name(N'dbo.PayrollProductSetting', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.PayrollProductSetting', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.PayrollProductSetting', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.PayrollProductAccountStd ADD CONSTRAINT
	FK_PayrollProductAccountStd_PayrollProductSetting FOREIGN KEY
	(
	PayrollProductSettingId
	) REFERENCES dbo.PayrollProductSetting
	(
	PayrollProductSettingId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.PayrollProductAccountStd SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.PayrollProductAccountStd', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.PayrollProductAccountStd', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.PayrollProductAccountStd', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.PayrollProductPriceType ADD CONSTRAINT
	FK_PayrollProductPriceType_PayrollProductSetting FOREIGN KEY
	(
	PayrollProductSettingId
	) REFERENCES dbo.PayrollProductSetting
	(
	PayrollProductSettingId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.PayrollProductPriceType SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.PayrollProductPriceType', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.PayrollProductPriceType', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.PayrollProductPriceType', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.PayrollProductPriceFormula ADD CONSTRAINT
	FK_PayrollProductPriceFormula_PayrollProductSetting FOREIGN KEY
	(
	PayrollProductSettingId
	) REFERENCES dbo.PayrollProductSetting
	(
	PayrollProductSettingId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.PayrollProductPriceFormula SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.PayrollProductPriceFormula', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.PayrollProductPriceFormula', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.PayrollProductPriceFormula', 'Object', 'CONTROL') as Contr_Per 
GO
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
ALTER TABLE dbo.PayrollProductSetting ADD
	ChildProductId int NULL
GO
ALTER TABLE dbo.PayrollProductSetting SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.PayrollProductSetting', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.PayrollProductSetting', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.PayrollProductSetting', 'Object', 'CONTROL') as Contr_Per 
GO



USE [soecompv_education]
GO

/****** Object:  View [dbo].[TimeCodeTransactionBillingView]    Script Date: 2014-12-10 13:05:18 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO















ALTER VIEW [dbo].[TimeCodeTransactionBillingView]
AS

--Transactions from offer/order/invoice
SELECT
	--TimeCodeTransaction
	tct.TimeCodeTransactionId,
	--Origin
	ori.ActorCompanyId,
	ori.Type as HeadType,
	ori.Status as HeadStatus,
	ISNULL((CASE WHEN ori.Type = 5 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END), CAST(0 AS bit)) AS HeadIsOffer, 
	ISNULL((CASE WHEN ori.Type = 6 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END), CAST(0 AS bit)) AS HeadIsOrder, 
	ISNULL((CASE WHEN ori.Type = 2 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END), CAST(0 AS bit)) AS HeadIsInvoice, 
	st_status.Name as HeadStatusName,
	--Invoice
	inv.InvoiceId,
	inv.InvoiceNr as HeadNr,
	inv.InvoiceDate as HeadDate,
	inv.Created as HeadCreated,
	inv.CreatedBy as HeadCreatedBy,
	inv.DefaultDim1AccountId as HeadDim1Account,
	inv.DefaultDim1AccountId as HeadDim2Account,
	inv.DefaultDim3AccountId as HeadDim3Account,
	inv.DefaultDim4AccountId as HeadDim4Account,
	inv.DefaultDim5AccountId as HeadDim5Account,
	inv.DefaultDim6AccountId as HeadDim6Account,
	--CustomerInvoiceRow
	row.CustomerInvoiceRowId,
	row.Quantity as RowQuantity,
	row.Amount as RowAmount,
	row.AmountCurrency as RowAmountCurrency,
	row.AmountLedgerCurrency as RowAmountLedgerCurrency,
	row.AmountEntCurrency as RowAmountEntCurrency,
    row.SumAmount as RowSumAmount,
    row.SumAmountCurrency as RowSumAmountCurrency,
    row.SumAmountLedgerCurrency as RowSumAmountLedgerCurrency,
    row.SumAmountEntCurrency as RowSumAmountEntCurrency,
    row.VatAmount as RowVatAmount,
    row.VatAmountCurrency as RowVatAmountCurrency,
    row.VatAmountLedgerCurrency as RowVatAmountLedgerCurrency,
    row.VatAmountEntCurrency as RowVatAmountEntCurrency,
    row.VatRate as RowVatRate,
    row.IsTimeProjectRow as RowIsTimeProjectRow,
    row.Created as RowCreated,
    row.CreatedBy as RowCreatedBy,
    row.PurchasePrice as RowPurchasePrice,
    row.MarginalIncome as RowMarginalIncome,
    row.MarginalIncomeRatio as RowMarginalIncomeRatio,
    row.DiscountPercent as RowDiscountPercent,
    row.AttestStateId as RowAttestStateId,
    row.TargetRowId as TargetRowId,
    row.IsFreightAmountRow,
    row.IsInvoiceFeeRow,
    row.IsCentRoundingRow,
    row.IsInterestRow,
    row.IsReminderRow,
    --Product
	ISNULL(prod.ProductId, 0) as RowProductId,
	ISNULL(prod.Number, '') as RowProductNumber,
    ISNULL(prod.Name, '') as RowProductName,
    ISNULL(prod.Description, '') as RowProductDescription,
	--InvoiceProduct
	ISNULL(invProd.CalculationType, 0) as RowCalculationType,
    --ProductUnit
    ISNULL(unit.Code, '') as RowProductUnitCode,
    ISNULL(unit.Name, '') as RowProductUnitName,
	--Lang
	st_status.LangId
FROM
	TimeCodeTransaction as tct WITH (NOLOCK) OUTER APPLY
	(
		SELECT		TOP 1 *
		FROM		TimeInvoiceTransaction
		WHERE		TimeCodeTransactionId = tct.TimeCodeTransactionId
		ORDER BY	TimeCodeTransactionId
	) tit INNER JOIN		
	CustomerInvoiceRow as row WITH (NOLOCK) on row.CustomerInvoiceRowId = tit.CustomerInvoiceRowId INNER JOIN
	Product as prod WITH (NOLOCK) on prod.ProductId = row.ProductId INNER JOIN
	InvoiceProduct as invProd WITH(NOLOCK) on invProd.ProductId = prod.ProductId INNER JOIN
	--ProductUnit as unit WITH (NOLOCK) on unit.ProductUnitId = prod.ProductUnitId INNER JOIN
	Invoice as inv WITH (NOLOCK) on inv.InvoiceId = row.InvoiceId INNER JOIN
	Origin as ori WITH (NOLOCK) on ori.OriginId = inv.InvoiceId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_status ON ori.Status = st_status.SysTermId LEFT OUTER JOIN
	ProductUnit as unit WITH (NOLOCK) on unit.ProductUnitId = prod.ProductUnitId
WHERE
	ori.Type IN(2,5,6) AND
	row.State = 0 AND
	inv.State = 0 AND
	(st_status.SysTermGroupId = 30)


GO


---$ Alter table dbo.AttestRole
IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE OBJECT_ID = OBJECT_ID(N'dbo.AttestRole') AND NAME = 'AlsoAttestAdditionsFromTime')
BEGIN
    PRINT 'Add column : dbo.AttestRole.AlsoAttestAdditionsFromTime'
    ALTER TABLE dbo.AttestRole
        ADD AlsoAttestAdditionsFromTime bit NOT NULL DEFAULT (0)
END
GO


/*
   den 19 december 201414:21:15
   User: dba
   Server: extra01\xedev
   Database: soecompv_education
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

	
GO
ALTER TABLE dbo.AccountStd SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.AccountStd', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.AccountStd', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.AccountStd', 'Object', 'CONTROL') as Contr_Per 



USE [soecompv_education]
GO

/****** Object:  StoredProcedure [dbo].[GetTimePayrollTransactionsForEmployees]    Script Date: 2014-12-19 14:44:47 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO








ALTER PROCEDURE [dbo].[GetTimePayrollTransactionsForEmployees]
	@actorCompanyId int,					-- Mandatory
	@dateFrom datetime,						-- Mandatory
	@dateTo datetime,						-- Mandatory
	@employeeIds nvarchar(max),				-- Null or comma separated string
	@preliminary bit						-- Null, 0 or 1 (Null = condition not used, 0 = no preliminary, 1 = only preliminary)


WITH RECOMPILE

AS
BEGIN
	SET NOCOUNT ON;
	 Select
		 tct.TimeCodeId as TimeCodeId,
		 tpt.State as TimePayrollTransactionState,
		 tct.State as TimeCodeTransactionState,
		 p.ProductId as ProductId,
		 tpt.TimePayrollTransactionId as TimePayrollTransactionId,
		 tpt.TimeCodeTransactionId as TimeCodeTransactionId,
		 tbd.Date as [Date],
		 p.Number as PayrollProductNumber,
		 p.Name as PayrollProductName,
		 tpt.Quantity as PayrollProductMinutes,
		 pp.Factor as PayrollProductFactor,
		 pp.PayrollType as PayrollProductType,
		 pp.SysPayrollTypeLevel1 as PayrollProductTypeLevel1,
		 pp.SysPayrollTypeLevel2 as PayrollProductTypeLevel2,
		 pp.SysPayrollTypeLevel3 as PayrollProductTypeLevel3,
		 pp.SysPayrollTypeLevel4 as PayrollProductTypeLevel4,
		 tpt.UnitPrice as UnitPrice,
		 tpt.UnitPriceCurrency as UnitPriceCurrency,
		 tpt.UnitPriceEntCurrency UnitPriceEntCurrency,
		 tpt.Amount as Amount,
		 tpt.AmountCurrency as AmountCurrency,
		 tpt.AmountEntCurrency as AmountEntCurrency,
		 tpt.VatAmount as Vatamount,
		 tpt.VatAmountCurrency as VatAmountCurrency,
		 tpt.VatAmountEntCurrency as VatAmountEntCurrency,
		 tpt.Quantity as Quantity,
		 tpt.SysPayrollTypeLevel1 as PayrollTypeLevel1,
		 tpt.SysPayrollTypeLevel2 as PayrollTypeLevel2,
		 tpt.SysPayrollTypeLevel3 as PayrollTypeLevel3,
		 tpt.SysPayrollTypeLevel4 as PayrollTypeLevel4,
		 pp.Payed as PayedTime,
		 tpte.Formula as Formula,
		 tpte.FormulaExtracted as FormulaExtracted,
		 tpte.FormulaNames as FormulaNames,
		 tpte.FormulaOrigin as FormulaOrigin,
		 tpte.FormulaPlain as FormulaPlain,
		 tb.Comment as Note,
		 a.AccountNr as AccountNr,
		 a.Name as AccountName,
		 ad.AccountDimNr as AccountDimNr,
		 ad.name as AccountDimName,
		 a2.AccountNr as AccountInternalNr,
		 a2.Name as AccountInternalName,
		 tpt.EmployeeId,
		 tpt.TimeBlockDateId,
		 ast.Name as AttestStateName

		 from 
		 TimePayrollTransaction tpt inner join
		 product p on tpt.PayrollProductId = p.ProductId inner join
		 PayrollProduct pp on pp.ProductId  = p.ProductId inner join
		 TimeCodeTransaction tct on tct.TimeCodeTransactionId= tpt.TimeCodeTransactionId inner join
		 Account a on a.AccountId=tpt.AccountId inner join
		 AttestState ast on ast.AttestStateId = tpt.AttestStateId inner join
		 timeblockdate tbd on tbd.TimeBlockDateId = tpt.TimeBlockDateId left outer join
		 timeblock tb on tb.TimeBlockId = tpt.TimeBlockId left outer join
		 TimePayrollTransactionExtended tpte on tpte.TimePayrollTransactionId = tpt.TimePayrollTransactionId left outer join 
		 TimePayrollTransactionAccount tpta on tpta.TimePayrollTransactionId = tpt.TimePayrollTransactionId left outer join
		 account a2 on a2.AccountId=tpta.AccountId left outer join
		 accountDim ad on ad.AccountDimId = a.AccountDimId

         where
		 tbd.Date between @dateFrom and @dateTo AND
		 tpt.EmployeeId IN (SELECT * FROM SplitDelimiterString(@employeeIds, ','))
		 
		
		 ORDER BY 
		 tpt.EmployeeId

END


SET ANSI_NULLS ON





GO

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
ALTER TABLE dbo.PayrollProductSetting ADD
	WorkingTimePromoted bit NOT NULL CONSTRAINT DF_PayrollProductSetting_WorkingTimePromoted DEFAULT 0
GO
ALTER TABLE dbo.PayrollProductSetting SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.PayrollProductSetting', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.PayrollProductSetting', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.PayrollProductSetting', 'Object', 'CONTROL') as Contr_Per 
GO

/*
   7. tammikuuta 201510:05:38
   User: dba
   Server: 192.168.1.162\xedev
   Database: soecompv_education
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
ALTER TABLE dbo.AttestWorkFlowGroup SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.AttestWorkFlowGroup', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.AttestWorkFlowGroup', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.AttestWorkFlowGroup', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.Supplier ADD
	AttestWorkFlowGroupId int NULL
GO
ALTER TABLE dbo.Supplier ADD CONSTRAINT
	FK_Supplier_AttestFlowGroup FOREIGN KEY
	(
	AttestWorkFlowGroupId
	) REFERENCES dbo.AttestWorkFlowGroup
	(
	AttestWorkFlowHeadId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Supplier SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.Supplier', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Supplier', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Supplier', 'Object', 'CONTROL') as Contr_Per 
GO
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
ALTER TABLE dbo.PayrollProductSetting ADD
	TimeUnit int NOT NULL CONSTRAINT DF_PayrollProductSetting_TimeUnit DEFAULT 0
GO
ALTER TABLE dbo.PayrollProductSetting SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.PayrollProductSetting', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.PayrollProductSetting', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.PayrollProductSetting', 'Object', 'CONTROL') as Contr_Per 
GO

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
ALTER TABLE dbo.Report ADD
	FileType int NOT NULL CONSTRAINT DF_Report_ReportFileType DEFAULT ((0))
GO
ALTER TABLE dbo.Report SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

USE [soecompv_education]
GO

/****** Object:  StoredProcedure [dbo].[GetEdiEntriesAll]    Script Date: 9.1.2015 14:12:10 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetEdiEntriesAll]
	/*@langId					INT,*/
	@startingday   		    SmallDateTime,
	@endingday 				SmallDateTime 
	
AS
BEGIN
	
	/***v.ActorCompanyId, v.BillingType, c.LicenseId, c.Name,  count(v.EdiEntryId) as count **/
	select v.ActorCompanyId, v.Type, c.LicenseId, c.Name, COUNT(v.EdiEntryId) as Items, c.OrgNr
	from dbo.EdiEntry as v, dbo.Company as c
	where /*v.LangId = @langId and 
	*/ 
	v.Created >= @startingday and v.Created < @endingday+1 and c.ActorCompanyId = v.ActorCompanyId 
	group by v.ActorCompanyId, c.LicenseId, c.Name, c.OrgNr, v.Type
	order by v.ActorCompanyId, v.Type
	
END



GO

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
ALTER TABLE dbo.AccountStd ADD
	RowTextStop bit NOT NULL CONSTRAINT DF_AccountStd_RowTextStop DEFAULT ((1))
GO
ALTER TABLE dbo.AccountStd SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.AccountStd', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.AccountStd', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.AccountStd', 'Object', 'CONTROL') as Contr_Per 

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
ALTER TABLE dbo.PaymentInformationRow ADD
	BIC nvarchar(20) NULL,
	ClearingCode nvarchar(50) NULL,
	PaymentCode nvarchar(3) NULL,
	PaymentMethod int NULL,
	PaymentForm int NULL,
	ChargeCode int NULL,
	IntermediaryCode int NULL,
	CurrencyAccount nvarchar(50) NULL
GO
ALTER TABLE dbo.PaymentInformationRow SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.PaymentInformationRow', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.PaymentInformationRow', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.PaymentInformationRow', 'Object', 'CONTROL') as Contr_Per 
GO


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
ALTER TABLE dbo.ReportHeader ADD
	InvertRow bit NOT NULL CONSTRAINT DF_ReportHeader_InvertRow DEFAULT ((0))
GO
ALTER TABLE dbo.ReportHeader SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.ReportHeader', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.ReportHeader', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.ReportHeader', 'Object', 'CONTROL') as Contr_Per 
GO

USE [soecompv_education]
GO

/****** Object:  View [dbo].[ScanningEntryView]    Script Date: 15.1.2015 9:51:18 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/*I(SNULL(edi.ScanningEntryId, 0) > 0)*/
ALTER VIEW [dbo].[ScanningEntryView]
AS
SELECT        edi.EdiEntryId, edi.ActorCompanyId, edi.Type, scanning.Status, ST_EStatus.Name AS StatusName, st_EdiSourceType.Name AS SourceTypeName, 
                         scanning.MessageType, ST_MessageType.Name AS MessageTypeName, edi.BillingType, ST_BillingType.Name AS BillingTypeName, 
                         edi.SysWholesellerId AS WholeSellerId, edi.WholesellerName, edi.BuyerId, edi.BuyerReference, ISNULL((CASE WHEN scanning.Image IS NOT NULL 
                         THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END), CAST(0 AS bit)) AS HasPdf, edi.ErrorCode, edi.Created, edi.State, scanning.ScanningEntryId, scanning.NrOfPages, 
                         scanning.NrOfInvoices, scanning.OperatorMessage, edi.Date, edi.InvoiceDate, edi.DueDate, edi.Sum, edi.SumCurrency, edi.SumVat, edi.SumVatCurrency, 
                         edi.CurrencyId, curr.SysCurrencyId, scurr.Code AS CurrencyCode, edi.CurrencyRate, edi.OrderId, edi.OrderStatus, ST_OStatus.Name AS OrderStatusName, 
                         edi.OrderNr, edi.SellerOrderNr, edi.InvoiceId, edi.InvoiceStatus, ST_IStatus.Name AS InvoiceStatusName, edi.InvoiceNr, edi.SeqNr, 
                         cust.ActorCustomerId AS CustomerId, cust.CustomerNr, cust.Name AS CustomerName, supp.ActorSupplierId AS SupplierId, supp.SupplierNr, 
                         supp.Name AS SupplierName, ST_EStatus.LangId, supp.AttestWorkFlowGroupId
FROM            dbo.EdiEntry AS edi INNER JOIN
                         dbo.ScanningEntry AS scanning ON edi.ScanningEntryInvoiceId = scanning.ScanningEntryId INNER JOIN
                         SOESysV2.dbo.SysTerm AS ST_EStatus ON ST_EStatus.SysTermId = edi.Status AND ST_EStatus.SysTermGroupId = 143 INNER JOIN
                         SOESysV2.dbo.SysTerm AS ST_OStatus ON ST_OStatus.SysTermId = edi.OrderStatus AND ST_OStatus.SysTermGroupId = 114 AND 
                         ST_OStatus.LangId = ST_EStatus.LangId INNER JOIN
                         SOESysV2.dbo.SysTerm AS ST_IStatus ON ST_IStatus.SysTermId = edi.InvoiceStatus AND ST_IStatus.SysTermGroupId = 115 AND 
                         ST_IStatus.LangId = ST_EStatus.LangId INNER JOIN
                         SOESysV2.dbo.SysTerm AS ST_MessageType ON ST_MessageType.SysTermId = 2 AND ST_MessageType.SysTermGroupId = 144 AND 
                         ST_MessageType.LangId = ST_EStatus.LangId INNER JOIN
                         SOESysV2.dbo.SysTerm AS st_EdiSourceType ON st_EdiSourceType.SysTermId = edi.Type AND st_EdiSourceType.SysTermGroupId = 116 AND 
                         st_EdiSourceType.LangId = ST_EStatus.LangId INNER JOIN
                         dbo.Currency AS curr ON curr.CurrencyId = edi.CurrencyId INNER JOIN
                         SOESysV2.dbo.SysCurrency AS scurr ON scurr.SysCurrencyId = curr.SysCurrencyId LEFT OUTER JOIN
                         SOESysV2.dbo.SysTerm AS ST_BillingType ON ST_BillingType.SysTermId = edi.BillingType AND ST_BillingType.SysTermGroupId = 27 AND 
                         ST_BillingType.LangId = ST_EStatus.LangId LEFT OUTER JOIN
                         dbo.Invoice AS i ON edi.OrderId IS NOT NULL AND i.InvoiceId = edi.OrderId LEFT OUTER JOIN
                         dbo.Customer AS cust ON cust.ActorCustomerId = i.ActorId LEFT OUTER JOIN
                         dbo.Supplier AS supp ON supp.ActorSupplierId = edi.ActorSupplierId
WHERE        (edi.Type = 2)

GO



USE [soecompv_education]
GO

/****** Object:  Table [dbo].[ShiftTypeEmployeeStatisticsTarget]    Script Date: 2015-01-15 09:30:56 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ShiftTypeEmployeeStatisticsTarget](
	[ShiftTypeEmployeeStatisticsTargetId] [int] IDENTITY(1,1) NOT NULL,
	[ShiftTypeId] [int] NOT NULL,
	[EmployeeStatisticsType] [int] NOT NULL,
	[TargetValue] [decimal](10, 2) NOT NULL,
	[FromDate] [datetime] NULL,
	[Created] [datetime] NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[Modified] [datetime] NULL,
	[ModifiedBy] [nvarchar](50) NULL,
	[State] [int] NOT NULL,
 CONSTRAINT [PK_ShiftTypeEmployeeStatisticsTarget] PRIMARY KEY CLUSTERED 
(
	[ShiftTypeEmployeeStatisticsTargetId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[ShiftTypeEmployeeStatisticsTarget] ADD  CONSTRAINT [DF_ShiftTypeEmployeeStatisticsTarget_TargetValue]  DEFAULT ((0)) FOR [TargetValue]
GO

ALTER TABLE [dbo].[ShiftTypeEmployeeStatisticsTarget] ADD  CONSTRAINT [DF_ShiftTypeEmployeeStatisticsTarget_State]  DEFAULT ((0)) FOR [State]
GO

ALTER TABLE [dbo].[ShiftTypeEmployeeStatisticsTarget]  WITH CHECK ADD  CONSTRAINT [FK_ShiftTypeEmployeeStatisticsTarget_ShiftType] FOREIGN KEY([ShiftTypeId])
REFERENCES [dbo].[ShiftType] ([ShiftTypeId])
GO

ALTER TABLE [dbo].[ShiftTypeEmployeeStatisticsTarget] CHECK CONSTRAINT [FK_ShiftTypeEmployeeStatisticsTarget_ShiftType]
GO

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
ALTER TABLE dbo.Report ADD
	ShowInAccountingReports bit NOT NULL CONSTRAINT DF_Report_ShowInAccountingReports DEFAULT ((0))
GO
ALTER TABLE dbo.Report SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.Report', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Report', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Report', 'Object', 'CONTROL') as Contr_Per 

GO
USE [soecompv_education]
GO
/****** Object:  StoredProcedure [dbo].[GetTimePayrollTransactionsForEmployee]    Script Date: 2015-01-16 09:52:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



ALTER PROCEDURE [dbo].[GetTimePayrollTransactionsForEmployee]
	@employeeId INT,
	@startDate DateTime = NULL,
	@stopDate DateTime = NULL,
	@timePeriodId INT = NULL
WITH RECOMPILE
AS
BEGIN
	SET NOCOUNT ON;

SELECT 
	--TimePayrollTransaction
	tpt.TimePayrollTransactionId,
	tpt.EmployeeId,
	tpt.TimeCodeTransactionId,
	tpt.AccountId,
	tpt.UnitPrice,
	tpt.UnitPriceCurrency,
	tpt.UnitPriceEntCurrency,
	tpt.Amount as Amount,
	tpt.AmountCurrency AS AmountCurrency,
	tpt.AmountEntCurrency AS AmountEntCurrency,
	tpt.VatAmount as VatAmount,
	tpt.VatAmountCurrency AS VatAmountCurrency,
	tpt.VatAmountEntCurrency AS VatAmountEntCurrency,
	tpt.Quantity,
	tpt.ManuallyAdded,
	ISNULL(tpt.Comment,'') AS TransactionComment,
	tpt.IsPreliminary,
	tpt.ReversedDate,
	tpt.IsReversed,
	tpt.IsAdded,
	tpt.AddedDateFrom,
	tpt.AddedDateTo,
	tpt.IsFixed,
	tpt.IsAdditionOrDeduction,
	tpt.IsSpecifiedUnitPrice,
	tpt.IsCentRounding,
	tpt.IsQuantityRounding,
	tpt.SysPayrollTypeLevel1 as TransactionSysPayrollTypeLevel1,
	tpt.SysPayrollTypeLevel2 as TransactionSysPayrollTypeLevel2,
	tpt.SysPayrollTypeLevel3 as TransactionSysPayrollTypeLevel3,
	tpt.SysPayrollTypeLevel4 as TransactionSysPayrollTypeLevel4,
	tpt.Created,
	tpt.CreatedBy,
	--TimePayrollTransactionExtended
	tpte.PayrollPriceFormulaId,
	tpte.PayrollPriceTypeId,
	tpte.Formula,
	tpte.FormulaPlain,
	tpte.FormulaExtracted,
	tpte.FormulaNames,
	tpte.FormulaOrigin,
	tpte.PayrollCalculationPerformed,
	--TimeBlockDate
	tbd.TimeBlockDateId,
	tbd.Date,
	--TimeBlock
	tb.TimeBlockId,
	tb.TimeScheduleTemplatePeriodId,
	(CASE WHEN ISNULL(tct.TimeCodeTransactionId, 0) > 0 THEN tct.Start ELSE tb.StartTime END) AS StartTime, 
	(CASE WHEN ISNULL(tct.TimeCodeTransactionId, 0) > 0 THEN tct.Stop ELSE tb.StopTime END) AS StopTime, 
	ISNULL(tb.Comment,'') AS DeviationComment,
	--AttestState
	ast.AttestStateId,
	ast.Name as AttestStateName,
	ast.Color as AttestStateColor,
	ast.Initial as AttestStateInitial,
	--PayrollProduct
	pp.ProductId,
	pp.ShortName as ProductShortName,
	pp.Factor as PayrollProductFactor,
	pp.Payed as PayrollProductPayed,
	pp.Export as PayrollProductExport,
	pp.SysPayrollTypeLevel1 as PayrollProductSysPayrollTypeLevel1,
	pp.SysPayrollTypeLevel2 as PayrollProductSysPayrollTypeLevel2,
	pp.SysPayrollTypeLevel3 as PayrollProductSysPayrollTypeLevel3,
	pp.SysPayrollTypeLevel4 as PayrollProductSysPayrollTypeLevel4,
	--Product
	p.Number as ProductNumber,
	p.Name as ProductName,
	--TimeCode
	(select	count(tc2.TimeCodeId) from TimeCode as tc2 inner join TimeCodeWork as tcw on tc.TimeCodeId = tcw.TimeCodeWorkId where tc.TimeCodeId = tc2.TimeCodeId AND tc.Type = 1 AND tcw.IsWorkOutsideSchedule = 1) AS NoOfPresenceWorkOutsideScheduleTime,
	(select	count(tc2.TimeCodeId) from TimeCode as tc2 inner join TimeCodeAbsense as tca on tc.TimeCodeId = tca.TimeCodeAbsenseId where tc.TimeCodeId = tc2.TimeCodeId AND tc.Type = 2 AND tca.IsAbsence = 1) AS NoOfAbsenceAbsenceTime,
	ISNULL(tc.RegistrationType,1) AS TimeCodeRegistrationType,
	ISNULL(tc.Type,0) AS TimeCodeType,
	--TimePeriod
	tpt.TimePeriodId,
	tp.Name as TimePeriodName
FROM	
	TimePayrollTransaction as tpt WITH (NOLOCK) INNER JOIN
	TimeBlockDate as tbd WITH (NOLOCK) on tbd.TimeBlockDateId = tpt.TimeBlockDateId INNER JOIN
	AttestState as ast WITH (NOLOCK) ON ast.AttestStateId = tpt.AttestStateId INNER JOIN
	PayrollProduct as pp WITH (NOLOCK) ON pp.ProductId = tpt.PayrollProductId INNER JOIN
	Product as p WITH (NOLOCK) ON p.ProductId = pp.ProductId LEFT OUTER JOIN
	TimeBlock as tb WITH (NOLOCK) ON tb.TimeBlockId = tpt.TimeBlockId LEFT OUTER JOIN	
	TimeCodeTransaction as tct WITH (NOLOCK) on tct.TimeCodeTransactionId = tpt.TimeCodeTransactionId LEFT OUTER JOIN
	TimeCode as tc WITH (NOLOCK) ON tc.TimeCodeId = tct.TimeCodeId LEFT OUTER JOIN
	TimePeriod as tp WITH (NOLOCK) ON tp.TimePeriodId = tpt.TimePeriodId LEFT OUTER JOIN
	TimePayrollTransactionExtended as tpte on tpte.TimePayrollTransactionId = tpt.TimePayrollTransactionId
WHERE
	(tbd.EmployeeId = @employeeId) AND 
	(tpt.State = 0) AND
	(tb.State IS NULL OR tb.State = 0) AND
	(
	  (@timePeriodId IS NOT NULL AND @timePeriodId = tpt.TimePeriodId) 
	  OR
	  ((tpt.TimePeriodId IS NULL OR @timePeriodId IS NULL) AND (@startDate IS NOT NULL AND @stopDate IS NOT NULL AND tbd.Date BETWEEN @startDate AND @stopDate))
	)
ORDER BY
	tbd.Date, tb.StartTime


END

		/*
		Case	Gui			Periodselector				Rule										Parameters
		1		Attest		For day/week/month/period	Never consider TimePeriodId, only dates		(@timePeriodId is null)
		2		Payroll		For day/week/month			Never consider TimePeriodId, only dates		(@timePeriodId is null)
		3		Payroll		For extra-period			Never consider dates, only TimePeriodId		(@startDate and @stopDate is null)
		4		Payroll		For regular-period			Consider both TimePeriodId and dates
														Can match either TimePeriodId or dates
														
		*/


GO
USE [soecompv_education]
GO
/****** Object:  StoredProcedure [dbo].[GetTimePayrollTransactionsWithAccIntsForEmployee]    Script Date: 2015-01-16 14:29:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




ALTER PROCEDURE [dbo].[GetTimePayrollTransactionsWithAccIntsForEmployee]
	@employeeId						INT,
	@actorCompanyId					INT,
	@dateFrom						DATETIME,
	@dateTo							DATETIME,
	@timeScheduleTemplatePeriodId	INT
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT 
		--TimePayrollTransaction
		tpt.TimePayrollTransactionId,
		tpt.Quantity,
		tpt.Amount,
		tpt.AmountCurrency,
		tpt.AmountLedgerCurrency,
		tpt.AmountEntCurrency,
		tpt.VatAmount,
		tpt.VatAmountCurrency,
		tpt.VatAmountLedgerCurrency,
		tpt.VatAmountEntCurrency,
		ISNULL(tpt.Comment,'') AS TransactionComment,
		tpt.IsPreliminary,
		tpt.ManuallyAdded,
		tpt.IsAdded,
		tpt.IsFixed,
		tpt.Exported,
		tpt.IsReversed,
		tpt.ReversedDate,
		tpt.SysPayrollTypeLevel1 as TransactionSysPayrollTypeLevel1,
		tpt.SysPayrollTypeLevel2 as TransactionSysPayrollTypeLevel2,
		tpt.SysPayrollTypeLevel3 as TransactionSysPayrollTypeLevel3,
		tpt.SysPayrollTypeLevel4 as TransactionSysPayrollTypeLevel4,
		tpt.IsCentRounding,
		tpt.IsQuantityRounding,
		--TimeCodeTransaction
		tct.TimeCodeTransactionId,
		--Employee
		tpt.EmployeeId,
		--EmployeeChild
		ec.EmployeeChildId as ChildId,
		ec.FirstName as ChildFirstName,
		ec.LastName as ChildLastName,
		ec.BirthDate as ChildBirthDate,
		--Product
		p.ProductId,
		p.Number as ProductNumber,
		p.Name as ProductName,
		--PayrollProduct
		pp.Export as PayrollProductExport,
		pp.SysPayrollTypeLevel1 as PayrollProductSysPayrollTypeLevel1,
		pp.SysPayrollTypeLevel2 as PayrollProductSysPayrollTypeLevel2,
		pp.SysPayrollTypeLevel3 as PayrollProductSysPayrollTypeLevel3,
		pp.SysPayrollTypeLevel4 as PayrollProductSysPayrollTypeLevel4,
		--TimeCode
		tc.TimeCodeId,
		tc.Code as TimeCodeCode,
		tc.Name as TimeCodeName,
		tc.Type as TimeCodeType,
		tc.RegistrationType as TimeCodeRegistrationType,
		--TimeBlock
		ISNULL(tb.TimeBlockId,0) as TimeBlockId,
		ISNULL(tb.Comment,'') as DeviationComment,	
		--TimeBlockDate
		tb.TimeBlockDateId,
		tbd.Date,	
		--AttestState
		ats.AttestStateId,
		ats.Name as AttestStateName,
		--Accounting
		acc.AccountId AS AccountStdId,
		acc.AccountNr AS AccountStdNr,
		acc.Name AS AccountStdName,
		LEFT(o.list,LEN(o.list) - 1) AS AccountInternalsStr
	FROM 
		dbo.TimePayrollTransaction AS tpt WITH (NOLOCK)INNER JOIN 
		dbo.Product AS p WITH (NOLOCK) ON p.ProductId = tpt.PayrollProductId INNER JOIN 
		dbo.PayrollProduct AS pp WITH (NOLOCK) ON pp.ProductId = p.ProductId INNER JOIN 
		dbo.Account AS acc WITH (NOLOCK) ON acc.AccountId = tpt.AccountId INNER JOIN 
		dbo.TimeBlockDate AS tbd WITH (NOLOCK) ON tbd.TimeBlockDateId = tpt.TimeBlockDateId INNER JOIN 
		dbo.AttestState AS ats WITH (NOLOCK) ON ats.AttestStateId = tpt.AttestStateId LEFT OUTER JOIN 
		dbo.TimeCodeTransaction AS tct WITH (NOLOCK) on tct.TimeCodeTransactionId = tpt.TimeCodeTransactionId LEFT OUTER JOIN
		dbo.TimeCode AS tc WITH (NOLOCK) on tc.TimeCodeId = tct.TimeCodeId LEFT OUTER JOIN
		dbo.EmployeeChild as ec  WITH (NOLOCK) on ec.EmployeeChildId = tpt.EmployeeChildId LEFT OUTER JOIN
		dbo.TimeBlock AS tb WITH (NOLOCK) ON tb.TimeBlockId = tpt.TimeBlockId CROSS APPLY 
		(
			SELECT   
				CONVERT(VARCHAR(100),tptad.AccountDimNr) + ':' + CONVERT(VARCHAR(100),tacc.AccountId) + ':' + CONVERT(VARCHAR(100),tacc.AccountNr) + ':' + CONVERT(VARCHAR(100),tacc.Name) + ',' AS [text()]
			FROM
				dbo.TimePayrollTransactionAccount AS ta WITH (NOLOCK) INNER JOIN 
				dbo.Account AS tacc WITH (NOLOCK) ON tacc.AccountId = ta.AccountId INNER JOIN 
				dbo.AccountDim AS tptad WITH (NOLOCK) ON tptad.AccountDimId = tacc.AccountDimId
			WHERE    
				ta.TimePayrollTransactionId = tpt.TimePayrollTransactionId and tptad.AccountDimNr > 1
			ORDER BY
				tptad.AccountDimNr
			FOR XML PATH('')
		 ) o(list)	
	WHERE 
		tbd.EmployeeId = @employeeId AND 
		(tbd.Date BETWEEN @dateFrom AND @dateTo) AND
		tpt.[State] = 0	AND
		ISNULL(tb.State, 0) = 0
	ORDER BY
		tb.StartTime


    END

GO


ALTER PROCEDURE [dbo].[GetChangeStatusGridViewSI]
	@actorCompanyId INT,
	@langId INT,
	@originType INT
AS
BEGIN
	SET NOCOUNT ON;

/*
 ATTENTION:
 Changes should be made in both view [ChangeStatusGridViewSI] and procedure [GetChangeStatusGridViewSI]
 */

SELECT TOP 100 PERCENT 
    --Company
    ISNULL(o.ActorCompanyId, 0) AS OwnerActorId, 
    --Actor
    ISNULL(i.ActorId, 0) AS ActorId,
    s.SupplierNr AS ActorNr, 
    s.Name AS ActorName, 
    --Supplier
    s.BlockPayment AS SupplierBlockPayment, 
    --Customer
    --Origin
    o.OriginId, 
    o.Type AS OriginType, 
    o.Status, 
    st_stat.Name AS StatusName, 
    o.Description AS InternalText,       
    --Invoice
    i.InvoiceId, 
    i.Type AS InvoiceType, 
	i.IsTemplate as IsInvoiceTemplate,
    i.InvoiceNr, 
    i.SeqNr as InvoiceSeqNr, 
    i.BillingType AS BillingTypeId, 
    i.StatusIcon AS StatusIcon, 
	i.VatType as VatType,
    ISNULL(i.ProjectId, 0) AS ProjectId, 
    st_bt.Name AS BillingTypeName, 
    ISNULL(CAST(0 AS decimal(10, 2)), 0) AS RemainingAmount, 
    ISNULL(CAST(0 AS decimal(10, 2)), 0) AS RemainingAmountExVat, 
    --Supplier invoice
    si.BlockPayment, 
    si.MultipleDebtRows, 
    si.AttestStateId AS SupplierInvoiceAttestStateId,
    si.PaymentMethodId AS SupplierInvoicePaymentMethodId, 
    ISNULL(pm.PaymentType, 0) AS SupplierInvoicePaymentMethodType,
    --Payment method
    ISNULL(pm.SysPaymentMethodId, 0) AS SysPaymentMethodId,       
    ISNULL(pm.Name, '') AS PaymentMethodName, 
    --Common
    st_bt.LangId AS LangId, 
    s_cu.Code AS CurrencyCode, 
    i.CurrencyRate, 
    cu.SysCurrencyId, 
    vs.AccountYearId, 
    vs.VoucherSeriesId, 
    ISNULL(CAST(i.VoucherHeadId AS bit), 0) AS HasVoucher, 
    i.InvoiceDate, 
    i.VoucherDate, 
    i.DueDate, 
    ISNULL(pr.PayDate, i.DueDate) AS PayDate, 
    i.TotalAmount AS InvoiceTotalAmount, 
    i.TotalAmountCurrency AS InvoiceTotalAmountCurrency, 
    i.VATAmount, 
	i.VATAmountCurrency,
    i.PaidAmount AS InvoicePaidAmount, 
    i.PaidAmountCurrency AS InvoicePaidAmountCurrency, 
    ISNULL(i.TotalAmount - i.PaidAmount, 0) AS InvoicePayAmount, 
    ISNULL(i.TotalAmountCurrency - i.PaidAmountCurrency, 0) AS InvoicePayAmountCurrency, 
    i.FullyPayed,
    LEFT(ob.list,LEN(ob.list) - 1) AS PaymentStatuses,
    si.CurrentAttestUsers,
	i.PaymentNr
FROM         
    dbo.Origin AS o WITH (NOLOCK) INNER JOIN
    dbo.Invoice AS i WITH (NOLOCK) ON i.InvoiceId = o.OriginId INNER JOIN
    dbo.SupplierInvoice AS si WITH (NOLOCK) ON si.InvoiceId = i.InvoiceId INNER JOIN
    dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i.CurrencyId INNER JOIN
    dbo.Supplier AS s WITH (NOLOCK) ON s.ActorSupplierId = i.ActorId INNER JOIN
    dbo.VoucherSeries AS vs WITH (NOLOCK) ON o.VoucherSeriesId = vs.VoucherSeriesId INNER JOIN
    SoesysV2.dbo.SysTerm AS st_bt WITH (NOLOCK) ON i.BillingType = st_bt.SysTermId INNER JOIN
    SoesysV2.dbo.SysTerm AS st_stat WITH (NOLOCK) ON o.Status = st_stat.SysTermId LEFT OUTER JOIN
    dbo.PaymentMethod AS pm WITH (NOLOCK) ON pm.PaymentMethodId = si.PaymentMethodId INNER JOIN
    SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId OUTER APPLY
    (SELECT TOP 1 PayDate, PaymentNr FROM dbo.PaymentRow WITH (NOLOCK) WHERE InvoiceId = i.InvoiceId ORDER BY PayDate DESC) pr OUTER APPLY 
    (
      SELECT   CONVERT(VARCHAR(2),Status) + ',' AS [text()]
	  FROM     PaymentRow pr
	  WHERE    pr.InvoiceId = i.InvoiceId AND pr.State = 0
	  ORDER BY pr.PaymentRowId
	  FOR XML PATH('')
	 ) ob(list)          
WHERE     
	(o.Type = @originType) AND
	(o.ActorCompanyId = @actorCompanyId) AND
	(st_stat.LangId = @langId) AND
	(st_bt.LangId = @langId) AND
	(i.State = 0) AND 
	(st_bt.SysTermGroupId = 27) AND 
	(st_stat.SysTermGroupId = 30)

END


GO

ALTER VIEW [dbo].[ChangeStatusGridViewSI]
AS

/*
 ATTENTION:
 Changes should be made in both view [ChangeStatusGridViewSI] and procedure [GetChangeStatusGridViewSI]
 */

SELECT TOP 100 PERCENT 
    --Company
    ISNULL(o.ActorCompanyId, 0) AS OwnerActorId, 
    --Actor
    ISNULL(i.ActorId, 0) AS ActorId,
    s.SupplierNr AS ActorNr, 
    s.Name AS ActorName, 
    --Supplier
    s.BlockPayment AS SupplierBlockPayment, 
    --Customer
    --Origin
    o.OriginId, 
    o.Type AS OriginType, 
    o.Status, 
    st_stat.Name AS StatusName, 
    o.Description AS InternalText,       
    --Invoice
    i.InvoiceId, 
    i.Type AS InvoiceType, 
	i.IsTemplate as IsInvoiceTemplate,
    i.InvoiceNr, 
    i.SeqNr as InvoiceSeqNr, 
    i.BillingType AS BillingTypeId, 
    i.StatusIcon AS StatusIcon, 
	i.VatType as VatType,
    ISNULL(i.ProjectId, 0) AS ProjectId, 
    st_bt.Name AS BillingTypeName, 
    ISNULL(CAST(0 AS decimal(10, 2)), 0) AS RemainingAmount, 
    ISNULL(CAST(0 AS decimal(10, 2)), 0) AS RemainingAmountExVat, 
    --Supplier invoice
    si.BlockPayment, 
    si.MultipleDebtRows, 
    si.AttestStateId AS SupplierInvoiceAttestStateId,
    si.PaymentMethodId AS SupplierInvoicePaymentMethodId, 
    ISNULL(pm.PaymentType, 0) AS SupplierInvoicePaymentMethodType,
    --Payment method
    ISNULL(pm.SysPaymentMethodId, 0) AS SysPaymentMethodId,       
    ISNULL(pm.Name, '') AS PaymentMethodName, 
    --Common
    st_bt.LangId AS LangId, 
    s_cu.Code AS CurrencyCode, 
    i.CurrencyRate, 
    cu.SysCurrencyId, 
    vs.AccountYearId, 
    vs.VoucherSeriesId, 
    ISNULL(CAST(i.VoucherHeadId AS bit), 0) AS HasVoucher, 
    i.InvoiceDate, 
    i.VoucherDate, 
    i.DueDate, 
    ISNULL(pr.PayDate, i.DueDate) AS PayDate, 
    i.TotalAmount AS InvoiceTotalAmount, 
    i.TotalAmountCurrency AS InvoiceTotalAmountCurrency, 
    i.VATAmount, 
	i.VATAmountCurrency,
    i.PaidAmount AS InvoicePaidAmount, 
    i.PaidAmountCurrency AS InvoicePaidAmountCurrency, 
    ISNULL(i.TotalAmount - i.PaidAmount, 0) AS InvoicePayAmount, 
    ISNULL(i.TotalAmountCurrency - i.PaidAmountCurrency, 0) AS InvoicePayAmountCurrency, 
    i.FullyPayed,
    LEFT(ob.list,LEN(ob.list) - 1) AS PaymentStatuses,
    si.CurrentAttestUsers,
	i.PaymentNr as PaymentNr
FROM         
    dbo.Origin AS o WITH (NOLOCK) INNER JOIN
    dbo.Invoice AS i WITH (NOLOCK) ON i.InvoiceId = o.OriginId INNER JOIN
    dbo.SupplierInvoice AS si WITH (NOLOCK) ON si.InvoiceId = i.InvoiceId INNER JOIN
    dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i.CurrencyId INNER JOIN
    dbo.Supplier AS s WITH (NOLOCK) ON s.ActorSupplierId = i.ActorId INNER JOIN
    dbo.VoucherSeries AS vs WITH (NOLOCK) ON o.VoucherSeriesId = vs.VoucherSeriesId INNER JOIN
    SoesysV2.dbo.SysTerm AS st_bt WITH (NOLOCK) ON i.BillingType = st_bt.SysTermId INNER JOIN
    SoesysV2.dbo.SysTerm AS st_stat WITH (NOLOCK) ON o.Status = st_stat.SysTermId LEFT OUTER JOIN
    dbo.PaymentMethod AS pm WITH (NOLOCK) ON pm.PaymentMethodId = si.PaymentMethodId INNER JOIN
    SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId OUTER APPLY
    (SELECT TOP 1 PayDate, PaymentNr FROM dbo.PaymentRow WITH (NOLOCK) WHERE InvoiceId = i.InvoiceId ORDER BY PayDate DESC) pr OUTER APPLY 
    (
      SELECT   CONVERT(VARCHAR(2),Status) + ',' AS [text()]
	  FROM     PaymentRow pr
	  WHERE    pr.InvoiceId = i.InvoiceId AND pr.State = 0
	  ORDER BY pr.PaymentRowId
	  FOR XML PATH('')
	 ) ob(list)          
WHERE     
    (st_stat.LangId = st_bt.LangId) AND
    (o.Type = 1) AND 
    (i.State = 0) AND 
    (st_bt.SysTermGroupId = 27) AND 
    (st_stat.SysTermGroupId = 30)


GO


ALTER VIEW [dbo].[ChangeStatusGridViewCI]
AS
SELECT        TOP 100 PERCENT /*Company*/ ISNULL(o.ActorCompanyId, 0) AS OwnerActorId, /*Actor*/ ISNULL(i.ActorId, 0) AS ActorId, c.CustomerNr AS ActorNr, 
                         c.Name AS ActorName, /*Customer*/ c.GracePeriodDays AS CustomerGracePeriodDays, c.BillingTemplate AS DefaultBillingReportTemplate, 
                         ISNULL(CAST(c.PriceListTypeId AS int), 0) AS CustomerPriceListTypeId, /*Origin*/ o.OriginId, o.Type AS OriginType, o.Status, st_stat.Name AS StatusName, 
                         o.Description AS InternalText, /*Invoice*/ i.InvoiceId, i.Type AS InvoiceType, i.IsTemplate AS IsInvoiceTemplate, i.InvoiceNr, i.SeqNr AS InvoiceSeqNr, 
                         i.BillingType AS BillingTypeId, i.StatusIcon AS StatusIcon, ISNULL(i.ProjectId, 0) AS ProjectId, st_bt.Name AS BillingTypeName, 
                         /*Customer invoice*/ ci.InvoiceHeadText, ci.RegistrationType, ci.DeliveryAddressId, ci.BillingAddressId, ci.BillingInvoicePrinted, ci.HasHouseholdTaxDeduction, ci.FixedPriceOrder,
                         ci.InsecureDebt, ci.MultipleAssetRows, ci.NoOfReminders AS NoOfReminders, (SELECT COUNT(*) FROM CustomerInvoicePrintedReminder cipr WHERE cipr.CustomerInvoiceOriginId=i.InvoiceId) AS NoOfPrintedReminders, 
						 ISNULL(i.RemainingAmount, 0) AS RemainingAmount, ISNULL(i.RemainingAmountExVat, 0) 
                         AS RemainingAmountExVat, ci.DeliveryDate, '' AS ContractGroupName, /*cg.Name AS ContractGroupName, */ ci.NextContractPeriodDate, ci.NextContractPeriodYear, 
                         ci.NextContractPeriodValue, /* Shifttype*/ st.Name AS ShiftTypeName, st.Color AS ShiftTypeColor, /*Common*/ st_bt.LangId AS LangId, s_cu.Code AS CurrencyCode, 
                         i.CurrencyRate, cu.SysCurrencyId, vs.AccountYearId, vs.VoucherSeriesId, ISNULL(CAST(i.VoucherHeadId AS bit), 0) AS HasVoucher, i.InvoiceDate, i.VoucherDate, 
                         i.DueDate, ISNULL(pr.PayDate, i.DueDate) AS PayDate, i.TotalAmount AS InvoiceTotalAmount, i.TotalAmountCurrency AS InvoiceTotalAmountCurrency, i.VATAmount, 
                         i.VATAmountCurrency, i.PaidAmount AS InvoicePaidAmount, i.PaidAmountCurrency AS InvoicePaidAmountCurrency, ISNULL(i.TotalAmount - i.PaidAmount, 0) 
                         AS InvoicePayAmount, ISNULL(i.TotalAmountCurrency - i.PaidAmountCurrency, 0) AS InvoicePayAmountCurrency, i.FullyPayed, p.Number AS ProjectNumber, 
                         ISNULL(ci.OrderNumbers, '') AS OrderNumbers, COALESCE (ci.InvoiceDeliveryType, c.InvoiceDeliveryType, 0) AS DeliveryType, 
                         i.ExportStatus AS ExportStatus
FROM            dbo.Origin AS o WITH (NOLOCK) INNER JOIN
                         dbo.Invoice AS i WITH (NOLOCK) ON i.InvoiceId = o.OriginId INNER JOIN
                         dbo.CustomerInvoice AS ci WITH (NOLOCK) ON ci.InvoiceId = i.InvoiceId LEFT OUTER JOIN						 
                         dbo.ShiftType AS st WITH (NOLOCK) ON ci.ShiftTypeId = st.ShiftTypeId INNER JOIN
                         dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i.CurrencyId INNER JOIN
                         dbo.Customer AS c WITH (NOLOCK) ON c.ActorCustomerId = i.ActorId INNER JOIN
                         dbo.VoucherSeries AS vs WITH (NOLOCK) ON o.VoucherSeriesId = vs.VoucherSeriesId INNER JOIN
                         SoesysV2.dbo.SysTerm AS st_bt WITH (NOLOCK) ON i.BillingType = st_bt.SysTermId INNER JOIN
                         SoesysV2.dbo.SysTerm AS st_stat WITH (NOLOCK) ON o.Status = st_stat.SysTermId INNER JOIN
                         SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId LEFT OUTER JOIN
                         dbo.ContractGroup AS cg WITH (NOLOCK) ON ci.ContractGroupId = cg.ContractGroupId LEFT OUTER JOIN
                         dbo.Project AS p WITH (NOLOCK) ON i.ProjectId = p.ProjectId OUTER APPLY
                             (SELECT        TOP 1 PayDate
                               FROM            dbo.PaymentRow WITH (NOLOCK)
                               WHERE        InvoiceId = i.InvoiceId
                               ORDER BY PayDate DESC) pr
WHERE        (st_stat.LangId = st_bt.LangId) AND (o.Type = 2 OR
                         o.Type = 5 OR
                         o.Type = 6 OR
                         o.Type = 7) AND (i.State = 0) AND (st_bt.SysTermGroupId = 27) AND (st_stat.SysTermGroupId = 30)


GO



ALTER VIEW [dbo].[ChangeStatusGridViewCI]
AS
SELECT        TOP 100 PERCENT /*Company*/ ISNULL(o.ActorCompanyId, 0) AS OwnerActorId, /*Actor*/ ISNULL(i.ActorId, 0) AS ActorId, c.CustomerNr AS ActorNr, 
                         c.Name AS ActorName, /*Customer*/ c.GracePeriodDays AS CustomerGracePeriodDays, c.BillingTemplate AS DefaultBillingReportTemplate, 
                         ISNULL(CAST(c.PriceListTypeId AS int), 0) AS CustomerPriceListTypeId, /*Origin*/ o.OriginId, o.Type AS OriginType, o.Status, st_stat.Name AS StatusName, 
                         o.Description AS InternalText, /*Invoice*/ i.InvoiceId, i.Type AS InvoiceType, i.IsTemplate AS IsInvoiceTemplate, i.InvoiceNr, i.SeqNr AS InvoiceSeqNr, 
                         i.BillingType AS BillingTypeId, i.StatusIcon AS StatusIcon, ISNULL(i.ProjectId, 0) AS ProjectId, st_bt.Name AS BillingTypeName, 
                         /*Customer invoice*/ ci.InvoiceHeadText, ci.RegistrationType, ci.DeliveryAddressId, ci.BillingAddressId, ci.BillingInvoicePrinted, ci.HasHouseholdTaxDeduction, ci.FixedPriceOrder, 
                         ci.InsecureDebt, ci.MultipleAssetRows, ci.NoOfReminders AS NoOfReminders, (SELECT COUNT(*) FROM CustomerInvoicePrintedReminder cipr WHERE cipr.CustomerInvoiceOriginId=i.InvoiceId) AS NoOfPrintedReminders, 
						 ISNULL(i.RemainingAmount, 0) AS RemainingAmount, ISNULL(i.RemainingAmountExVat, 0) 
                         AS RemainingAmountExVat, ci.DeliveryDate, '' AS ContractGroupName, /*cg.Name AS ContractGroupName, */ ci.NextContractPeriodDate, ci.NextContractPeriodYear, 
                         ci.NextContractPeriodValue, /* Shifttype*/ st.Name AS ShiftTypeName, st.Color AS ShiftTypeColor, /*Common*/ st_bt.LangId AS LangId, s_cu.Code AS CurrencyCode, 
                         i.CurrencyRate, cu.SysCurrencyId, vs.AccountYearId, vs.VoucherSeriesId, ISNULL(CAST(i.VoucherHeadId AS bit), 0) AS HasVoucher, i.InvoiceDate, i.VoucherDate, 
                         i.DueDate, ISNULL(pr.PayDate, i.DueDate) AS PayDate, i.TotalAmount AS InvoiceTotalAmount, i.TotalAmountCurrency AS InvoiceTotalAmountCurrency, i.VATAmount, 
                         i.VATAmountCurrency, i.PaidAmount AS InvoicePaidAmount, i.PaidAmountCurrency AS InvoicePaidAmountCurrency, ISNULL(i.TotalAmount - i.PaidAmount, 0) 
                         AS InvoicePayAmount, ISNULL(i.TotalAmountCurrency - i.PaidAmountCurrency, 0) AS InvoicePayAmountCurrency, i.FullyPayed, p.Number AS ProjectNumber, 
                         ISNULL(ci.OrderNumbers, '') AS OrderNumbers, COALESCE (ci.InvoiceDeliveryType, c.InvoiceDeliveryType, 0) AS DeliveryType, 
                         i.ExportStatus AS ExportStatus
FROM            dbo.Origin AS o WITH (NOLOCK) INNER JOIN
                         dbo.Invoice AS i WITH (NOLOCK) ON i.InvoiceId = o.OriginId INNER JOIN
                         dbo.CustomerInvoice AS ci WITH (NOLOCK) ON ci.InvoiceId = i.InvoiceId LEFT OUTER JOIN						 
                         dbo.ShiftType AS st WITH (NOLOCK) ON ci.ShiftTypeId = st.ShiftTypeId INNER JOIN
                         dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i.CurrencyId INNER JOIN
                         dbo.Customer AS c WITH (NOLOCK) ON c.ActorCustomerId = i.ActorId INNER JOIN
                         dbo.VoucherSeries AS vs WITH (NOLOCK) ON o.VoucherSeriesId = vs.VoucherSeriesId INNER JOIN
                         SoesysV2.dbo.SysTerm AS st_bt WITH (NOLOCK) ON i.BillingType = st_bt.SysTermId INNER JOIN
                         SoesysV2.dbo.SysTerm AS st_stat WITH (NOLOCK) ON o.Status = st_stat.SysTermId INNER JOIN
                         SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId LEFT OUTER JOIN
                         dbo.ContractGroup AS cg WITH (NOLOCK) ON ci.ContractGroupId = cg.ContractGroupId LEFT OUTER JOIN
                         dbo.Project AS p WITH (NOLOCK) ON i.ProjectId = p.ProjectId OUTER APPLY
                             (SELECT        TOP 1 PayDate
                               FROM            dbo.PaymentRow WITH (NOLOCK)
                               WHERE        InvoiceId = i.InvoiceId
                               ORDER BY PayDate DESC) pr
WHERE        (st_stat.LangId = st_bt.LangId) AND (o.Type = 2 OR
                         o.Type = 5 OR
                         o.Type = 6 OR
                         o.Type = 7) AND (i.State = 0) AND (st_bt.SysTermGroupId = 27) AND (st_stat.SysTermGroupId = 30)


GO


ALTER PROCEDURE [dbo].[GetChangeStatusGridViewCI]
	@actorCompanyId INT,
	@langId INT,
	@originType INT
AS
BEGIN
	SET NOCOUNT ON;

/*
 ATTENTION:
 Changes should be made in both view [ChangeStatusGridViewCI] and procedure [GetChangeStatusGridViewCI]
 */

SELECT TOP 100 PERCENT 
	--Company
	ISNULL(o.ActorCompanyId, 0) AS OwnerActorId, 
	--Actor
	ISNULL(i.ActorId, 0) AS ActorId,
	c.CustomerNr AS ActorNr, 
	c.Name AS ActorName, 
	--Customer
	c.GracePeriodDays AS CustomerGracePeriodDays, 
	c.BillingTemplate AS DefaultBillingReportTemplate,
	ISNULL(CAST(c.PriceListTypeId AS int), 0) AS CustomerPriceListTypeId,  
	--Origin
	o.OriginId, 
	o.Type AS OriginType, 
	o.Status, 
	st_stat.Name AS StatusName, 
	o.Description AS InternalText, 
	--Invoice
	i.InvoiceId, 
	i.Type AS InvoiceType, 
	i.IsTemplate as IsInvoiceTemplate,
	i.InvoiceNr, 
	i.SeqNr as InvoiceSeqNr, 
	i.BillingType AS BillingTypeId, 
	i.StatusIcon AS StatusIcon, 
	ISNULL(i.ProjectId, 0) AS ProjectId,
	st_bt.Name AS BillingTypeName, 
	--Customer invoice
	ci.InvoiceHeadText, 
	ci.RegistrationType, 
	ci.DeliveryAddressId, 
	ci.BillingAddressId,
	ci.BillingInvoicePrinted, 
	ci.HasHouseholdTaxDeduction,
	ci.FixedPriceOrder,
	ci.InsecureDebt,
	ci.MultipleAssetRows, 
	ci.NoOfReminders,	
	ISNULL(i.RemainingAmount, 0) AS RemainingAmount, 
	ISNULL(i.RemainingAmountExVat, 0) AS RemainingAmountExVat,
	ci.DeliveryDate,
	--Contract
	'' AS ContractGroupName,--cg.Name AS ContractGroupName, 
	ci.NextContractPeriodDate, 
	ci.NextContractPeriodYear, 
	ci.NextContractPeriodValue, 
	-- Shifttype
	st.Name as ShiftTypeName,
	st.Color as ShiftTypeColor,
	--Common
	st_bt.LangId AS LangId, 
	s_cu.Code AS CurrencyCode, 
	i.CurrencyRate, 
	cu.SysCurrencyId, 
	vs.AccountYearId, 
	vs.VoucherSeriesId, 
	ISNULL(CAST(i.VoucherHeadId AS bit), 0) AS HasVoucher, 
	i.InvoiceDate, 
	i.VoucherDate, 
	i.DueDate, 
	ISNULL(pr.PayDate, i.DueDate) AS PayDate, 
	i.TotalAmount AS InvoiceTotalAmount, 
	i.TotalAmountCurrency AS InvoiceTotalAmountCurrency, 
	i.VATAmount, 
	i.VATAmountCurrency, 
	i.PaidAmount AS InvoicePaidAmount, 
	i.PaidAmountCurrency AS InvoicePaidAmountCurrency, 
	ISNULL(i.TotalAmount - i.PaidAmount, 0) AS InvoicePayAmount, 
	ISNULL(i.TotalAmountCurrency - i.PaidAmountCurrency, 0) AS InvoicePayAmountCurrency, 
	i.FullyPayed,
	p.Number AS ProjectNumber,
	ISNULL(ci.OrderNumbers, '') AS OrderNumbers,
	COALESCE (ci.InvoiceDeliveryType, c.InvoiceDeliveryType, 0) AS DeliveryType,
	i.ExportStatus,
	(SELECT COUNT(*) FROM CustomerInvoicePrintedReminder cipr WHERE cipr.CustomerInvoiceOriginId=i.InvoiceId) AS NoOfPrintedReminders
FROM
	dbo.Origin AS o WITH (NOLOCK) INNER JOIN
	dbo.Invoice AS i WITH (NOLOCK) ON i.InvoiceId = o.OriginId INNER JOIN
	dbo.CustomerInvoice AS ci WITH (NOLOCK) ON ci.InvoiceId = i.InvoiceId LEFT OUTER JOIN
	dbo.ShiftType AS st WITH (NOLOCK) ON ci.ShiftTypeId = st.ShiftTypeId INNER JOIN
	dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i.CurrencyId INNER JOIN
	dbo.Customer AS c WITH (NOLOCK) ON c.ActorCustomerId = i.ActorId INNER JOIN
	dbo.VoucherSeries AS vs WITH (NOLOCK) ON o.VoucherSeriesId = vs.VoucherSeriesId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_bt WITH (NOLOCK) ON i.BillingType = st_bt.SysTermId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_stat WITH (NOLOCK) ON o.Status = st_stat.SysTermId INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId LEFT OUTER JOIN
	dbo.ContractGroup AS cg WITH (NOLOCK) ON ci.ContractGroupId = cg.ContractGroupId LEFT OUTER JOIN
	dbo.Project AS p WITH(NOLOCK) ON i.ProjectId = p.ProjectId OUTER APPLY
	(SELECT TOP 1 PayDate FROM dbo.PaymentRow WITH (NOLOCK) WHERE InvoiceId = i.InvoiceId ORDER BY PayDate DESC) pr
WHERE     
	(o.Type = @originType) AND
	(o.ActorCompanyId = @actorCompanyId) AND
	(st_stat.LangId = @langId) AND
	(st_bt.LangId = @langId) AND
	(i.State = 0) AND 
	(st_bt.SysTermGroupId = 27) AND 
	(st_stat.SysTermGroupId = 30)

END

GO

ALTER PROCEDURE [dbo].[GetChangeStatusGridViewCI]
	@actorCompanyId INT,
	@langId INT,
	@originType INT
AS
BEGIN
	SET NOCOUNT ON;

/*
 ATTENTION:
 Changes should be made in both view [ChangeStatusGridViewCI] and procedure [GetChangeStatusGridViewCI]
 */

SELECT TOP 100 PERCENT 
	--Company
	ISNULL(o.ActorCompanyId, 0) AS OwnerActorId, 
	--Actor
	ISNULL(i.ActorId, 0) AS ActorId,
	c.CustomerNr AS ActorNr, 
	c.Name AS ActorName, 
	--Customer
	c.GracePeriodDays AS CustomerGracePeriodDays, 
	c.BillingTemplate AS DefaultBillingReportTemplate,
	ISNULL(CAST(c.PriceListTypeId AS int), 0) AS CustomerPriceListTypeId,  
	--Origin
	o.OriginId, 
	o.Type AS OriginType, 
	o.Status, 
	st_stat.Name AS StatusName, 
	o.Description AS InternalText, 
	--Invoice
	i.InvoiceId, 
	i.Type AS InvoiceType, 
	i.IsTemplate as IsInvoiceTemplate,
	i.InvoiceNr, 
	i.SeqNr as InvoiceSeqNr, 
	i.BillingType AS BillingTypeId, 
	i.StatusIcon AS StatusIcon, 
	ISNULL(i.ProjectId, 0) AS ProjectId,
	st_bt.Name AS BillingTypeName, 
	--Customer invoice
	ci.InvoiceHeadText, 
	ci.RegistrationType, 
	ci.DeliveryAddressId, 
	ci.BillingAddressId,
	ci.BillingInvoicePrinted, 
	ci.HasHouseholdTaxDeduction,
	ci.FixedPriceOrder,
	ci.InsecureDebt,
	ci.MultipleAssetRows, 
	ci.NoOfReminders,	
	ISNULL(i.RemainingAmount, 0) AS RemainingAmount, 
	ISNULL(i.RemainingAmountExVat, 0) AS RemainingAmountExVat,
	ci.DeliveryDate,
	--Contract
	'' AS ContractGroupName,--cg.Name AS ContractGroupName, 
	ci.NextContractPeriodDate, 
	ci.NextContractPeriodYear, 
	ci.NextContractPeriodValue, 
	st_fpo.Name AS FixedPriceOrderName,
	-- Shifttype
	st.Name as ShiftTypeName,
	st.Color as ShiftTypeColor,
	--Common
	st_bt.LangId AS LangId, 
	s_cu.Code AS CurrencyCode, 
	i.CurrencyRate, 
	cu.SysCurrencyId, 
	vs.AccountYearId, 
	vs.VoucherSeriesId, 
	ISNULL(CAST(i.VoucherHeadId AS bit), 0) AS HasVoucher, 
	i.InvoiceDate, 
	i.VoucherDate, 
	i.DueDate, 
	ISNULL(pr.PayDate, i.DueDate) AS PayDate, 
	i.TotalAmount AS InvoiceTotalAmount, 
	i.TotalAmountCurrency AS InvoiceTotalAmountCurrency, 
	i.VATAmount, 
	i.VATAmountCurrency, 
	i.PaidAmount AS InvoicePaidAmount, 
	i.PaidAmountCurrency AS InvoicePaidAmountCurrency, 
	ISNULL(i.TotalAmount - i.PaidAmount, 0) AS InvoicePayAmount, 
	ISNULL(i.TotalAmountCurrency - i.PaidAmountCurrency, 0) AS InvoicePayAmountCurrency, 
	i.FullyPayed,
	p.Number AS ProjectNumber,
	ISNULL(ci.OrderNumbers, '') AS OrderNumbers,
	COALESCE (ci.InvoiceDeliveryType, c.InvoiceDeliveryType, 0) AS DeliveryType,
	i.ExportStatus,
	(SELECT COUNT(*) FROM CustomerInvoicePrintedReminder cipr WHERE cipr.CustomerInvoiceOriginId=i.InvoiceId) AS NoOfPrintedReminders
FROM
	dbo.Origin AS o WITH (NOLOCK) INNER JOIN
	dbo.Invoice AS i WITH (NOLOCK) ON i.InvoiceId = o.OriginId INNER JOIN
	dbo.CustomerInvoice AS ci WITH (NOLOCK) ON ci.InvoiceId = i.InvoiceId LEFT OUTER JOIN
	dbo.ShiftType AS st WITH (NOLOCK) ON ci.ShiftTypeId = st.ShiftTypeId INNER JOIN
	dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i.CurrencyId INNER JOIN
	dbo.Customer AS c WITH (NOLOCK) ON c.ActorCustomerId = i.ActorId INNER JOIN
	dbo.VoucherSeries AS vs WITH (NOLOCK) ON o.VoucherSeriesId = vs.VoucherSeriesId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_bt WITH (NOLOCK) ON i.BillingType = st_bt.SysTermId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_stat WITH (NOLOCK) ON o.Status = st_stat.SysTermId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_fpo WITH (NOLOCK) ON ci.FixedPriceOrder = st_fpo.SysTermId INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId LEFT OUTER JOIN
	dbo.ContractGroup AS cg WITH (NOLOCK) ON ci.ContractGroupId = cg.ContractGroupId LEFT OUTER JOIN
	dbo.Project AS p WITH(NOLOCK) ON i.ProjectId = p.ProjectId OUTER APPLY
	(SELECT TOP 1 PayDate FROM dbo.PaymentRow WITH (NOLOCK) WHERE InvoiceId = i.InvoiceId ORDER BY PayDate DESC) pr
WHERE     
	(o.Type = @originType) AND
	(o.ActorCompanyId = @actorCompanyId) AND
	(st_stat.LangId = @langId) AND
	(st_bt.LangId = @langId) AND
	(st_fpo.LangId = @langId) AND
	(i.State = 0) AND 
	(st_bt.SysTermGroupId = 27) AND 
	(st_stat.SysTermGroupId = 30)
	AND (st_fpo.SysTermGroupId = 443)

END


GO

USE [SOECompv2]
GO

/****** Object:  View [dbo].[ChangeStatusGridViewCI]    Script Date: 2015-01-23 11:35:49 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER VIEW [dbo].[ChangeStatusGridViewCI]
AS
SELECT        TOP 100 PERCENT /*Company*/ ISNULL(o.ActorCompanyId, 0) AS OwnerActorId, /*Actor*/ ISNULL(i.ActorId, 0) AS ActorId, c.CustomerNr AS ActorNr, 
                         c.Name AS ActorName, /*Customer*/ c.GracePeriodDays AS CustomerGracePeriodDays, c.BillingTemplate AS DefaultBillingReportTemplate, 
                         ISNULL(CAST(c.PriceListTypeId AS int), 0) AS CustomerPriceListTypeId, /*Origin*/ o.OriginId, o.Type AS OriginType, o.Status, st_stat.Name AS StatusName, 
                         o.Description AS InternalText, /*Invoice*/ i.InvoiceId, i.Type AS InvoiceType, i.IsTemplate AS IsInvoiceTemplate, i.InvoiceNr, i.SeqNr AS InvoiceSeqNr, 
                         i.BillingType AS BillingTypeId, i.StatusIcon AS StatusIcon, ISNULL(i.ProjectId, 0) AS ProjectId, st_bt.Name AS BillingTypeName, 
                         /*Customer invoice*/ ci.InvoiceHeadText, ci.RegistrationType, ci.DeliveryAddressId, ci.BillingAddressId, ci.BillingInvoicePrinted, ci.HasHouseholdTaxDeduction, ci.FixedPriceOrder, 
                         ci.InsecureDebt, ci.MultipleAssetRows, ci.NoOfReminders AS NoOfReminders, st_fpo.Name AS FixedPriceOrderName , (SELECT COUNT(*) FROM CustomerInvoicePrintedReminder cipr WHERE cipr.CustomerInvoiceOriginId=i.InvoiceId) AS NoOfPrintedReminders, 
						 ISNULL(i.RemainingAmount, 0) AS RemainingAmount, ISNULL(i.RemainingAmountExVat, 0) 
                         AS RemainingAmountExVat, ci.DeliveryDate, '' AS ContractGroupName, /*cg.Name AS ContractGroupName, */ ci.NextContractPeriodDate, ci.NextContractPeriodYear, 
                         ci.NextContractPeriodValue, /* Shifttype*/ st.Name AS ShiftTypeName, st.Color AS ShiftTypeColor, /*Common*/ st_bt.LangId AS LangId, s_cu.Code AS CurrencyCode, 
                         i.CurrencyRate, cu.SysCurrencyId, vs.AccountYearId, vs.VoucherSeriesId, ISNULL(CAST(i.VoucherHeadId AS bit), 0) AS HasVoucher, i.InvoiceDate, i.VoucherDate, 
                         i.DueDate, ISNULL(pr.PayDate, i.DueDate) AS PayDate, i.TotalAmount AS InvoiceTotalAmount, i.TotalAmountCurrency AS InvoiceTotalAmountCurrency, i.VATAmount, 
                         i.VATAmountCurrency, i.PaidAmount AS InvoicePaidAmount, i.PaidAmountCurrency AS InvoicePaidAmountCurrency, ISNULL(i.TotalAmount - i.PaidAmount, 0) 
                         AS InvoicePayAmount, ISNULL(i.TotalAmountCurrency - i.PaidAmountCurrency, 0) AS InvoicePayAmountCurrency, i.FullyPayed, p.Number AS ProjectNumber, 
                         ISNULL(ci.OrderNumbers, '') AS OrderNumbers, COALESCE (ci.InvoiceDeliveryType, c.InvoiceDeliveryType, 0) AS DeliveryType, 
                         i.ExportStatus AS ExportStatus
FROM            dbo.Origin AS o WITH (NOLOCK) INNER JOIN
                         dbo.Invoice AS i WITH (NOLOCK) ON i.InvoiceId = o.OriginId INNER JOIN
                         dbo.CustomerInvoice AS ci WITH (NOLOCK) ON ci.InvoiceId = i.InvoiceId LEFT OUTER JOIN						 
                         dbo.ShiftType AS st WITH (NOLOCK) ON ci.ShiftTypeId = st.ShiftTypeId INNER JOIN
                         dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i.CurrencyId INNER JOIN
                         dbo.Customer AS c WITH (NOLOCK) ON c.ActorCustomerId = i.ActorId INNER JOIN
                         dbo.VoucherSeries AS vs WITH (NOLOCK) ON o.VoucherSeriesId = vs.VoucherSeriesId INNER JOIN
                         SoesysV2.dbo.SysTerm AS st_bt WITH (NOLOCK) ON i.BillingType = st_bt.SysTermId INNER JOIN
                         SoesysV2.dbo.SysTerm AS st_stat WITH (NOLOCK) ON o.Status = st_stat.SysTermId INNER JOIN
						 SoesysV2.dbo.SysTerm AS st_fpo WITH (NOLOCK) ON ci.FixedPriceOrder = st_fpo.SysTermId INNER JOIN
                         SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId LEFT OUTER JOIN
                         dbo.ContractGroup AS cg WITH (NOLOCK) ON ci.ContractGroupId = cg.ContractGroupId LEFT OUTER JOIN
                         dbo.Project AS p WITH (NOLOCK) ON i.ProjectId = p.ProjectId OUTER APPLY
                             (SELECT        TOP 1 PayDate
                               FROM            dbo.PaymentRow WITH (NOLOCK)
                               WHERE        InvoiceId = i.InvoiceId
                               ORDER BY PayDate DESC) pr
WHERE        (st_stat.LangId = st_bt.LangId) AND (o.Type = 2 OR
                         o.Type = 5 OR
                         o.Type = 6 OR
                         o.Type = 7) AND (i.State = 0) AND (st_bt.SysTermGroupId = 27) AND (st_stat.SysTermGroupId = 30) AND (st_fpo.SysTermGroupId = 443)



GO

USE [soecompv_education]
GO

/****** Object:  StoredProcedure [dbo].[GetTimeStampEntrysEmployeeSummary]    Script Date: 2015-01-23 10:21:24 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





ALTER PROCEDURE [dbo].[GetTimeStampEntrysEmployeeSummary]
	  @employeeId			INT,
	  @startDate			DATETIME,
	  @stopDate				DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	select 
		tse.TimeBlockDateId, 
		COUNT(tse.EmployeeId) as NrOfTimeStamps,
		COUNT(case tse.EmployeeManuallyAdjusted when 1 then 1 else null end) as NrOfEmployeeManuallyAdjustedTimeStamps
	from 
		TimeStampEntry as tse WITH (NOLOCK) INNER JOIN
		TimeBlockDate as tbd on tbd.TimeBlockDateId = tse.TimeBlockDateId
	where
		tse.EmployeeId = @employeeId AND
		tbd.Date BETWEEN @startDate and @stopDate AND
		tse.Status <> 4 AND
    tse.State = 0
    group by
    tse.TimeBlockDateId
    order by
    tse.TimeBlockDateId

    END



GO


/*
   den 27 januari 201509:09:43
   User: 
   Server: HAKANE1
   Database: soecompv_education
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
EXECUTE sp_rename N'dbo.EmployeeTaxSE.AdjustmentLimit', N'Tmp_SchoolYouthLimitInitial', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.EmployeeTaxSE.Tmp_SchoolYouthLimitInitial', N'SchoolYouthLimitInitial', 'COLUMN' 
GO
ALTER TABLE dbo.EmployeeTaxSE SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.EmployeeTaxSE', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.EmployeeTaxSE', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.EmployeeTaxSE', 'Object', 'CONTROL') as Contr_Per 


/*
   den 29 januari 201513:56:16
   User: dba
   Server: EXTRA01\XEDEV
   Database: soecompv_education
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
ALTER TABLE dbo.BudgetHead ADD
	UseDim2 bit NULL,
	UseDim3 bit NULL
GO
ALTER TABLE dbo.BudgetHead SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.BudgetHead', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.BudgetHead', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.BudgetHead', 'Object', 'CONTROL') as Contr_Per 


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
ALTER TABLE dbo.TimeStampEntry ADD
	IsBreak bit NOT NULL CONSTRAINT DF_TimeStampEntry_IsBreak DEFAULT 0
GO
ALTER TABLE dbo.TimeStampEntry SET (LOCK_ESCALATION = TABLE)
GO
COMMIT


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
ALTER TABLE dbo.TimeStampEntryRaw ADD
	IsBreak bit NOT NULL CONSTRAINT DF_TimeStampEntryRaw_IsBreak DEFAULT 0
GO
ALTER TABLE dbo.TimeStampEntryRaw SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

USE [soecompv_education]
GO

/****** Object:  View [dbo].[TimeStampAttendanceView]    Script Date: 2015-01-30 15:53:33 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER VIEW [dbo].[TimeStampAttendanceView]
AS
Select
	tse.ActorCompanyId,
	tse.EmployeeId,
	e.EmployeeNr,
	[Time],
	cp.FirstName,
	cp.LastName,
	tse.Type,
	tt.Name as TimeTerminalName,
	tdc.Name as TimeDeviationCauseName,
	a.Name as AccountName,
	tse.Created,
	tse.IsBreak
	from TimeStampEntry tse left outer join
	TimeTerminal tt on tt.TimeTerminalId = tse.TimeTerminalId left outer join
	Employee e on e.EmployeeId = tse.EmployeeId inner join 
	ContactPerson cp on cp.ActorContactPersonId = e.ContactPersonId	left outer join
	TimeDeviationCause tdc on tdc.TimeDeviationCauseId= tse.TimeDeviationCauseId left outer join
	account a on a.AccountId = tse.AccountId 
	where Time > GETDATE()-1 and Time < GETDATE() AND TSE.State = 0

Union All
 
Select
	tser.ActorCompanyRecordId as ActorCompanyId,
 	e.EmployeeId,
	e.EmployeeNr,
	[Time],
	cp.FirstName,
	cp.LastName,
	tser.Type,
	tt.Name as TimeTerminalName,
	tdc.Name as TimeDeviationCauseName,
	a.Name as AccountName,
	tser.Created,
	tser.IsBreak
	from TimeStampEntryRaw tser left outer join
	TimeTerminal tt on tt.TimeTerminalId = tser.TimeTerminalRecordId left outer join
	Employee e on e.EmployeeNr=tser.EmployeeNr and e.ActorCompanyId = tser.ActorCompanyRecordId inner join 
	ContactPerson cp on cp.ActorContactPersonId = e.ContactPersonId	left outer join
	TimeDeviationCause tdc on tdc.TimeDeviationCauseId= tser.TimeDeviationCauseRecordId left outer join
	account a on a.AccountId=tser.AccountRecordId 
	where Time > GETDATE()-1 and Time < GETDATE() and tser.TimeStampEntryid is null AND TSER.Status=0

GO

/*
   den 8 januari 201516:23:15
   User: dba
   Server: extra01\xedev
   Database: SOECompv2
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
ALTER TABLE dbo.Company SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.Company', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Company', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Company', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.PaymentRow
	DROP CONSTRAINT FK_PaymentRow_PaymentImport
GO
ALTER TABLE dbo.PaymentImport ADD
	ActorCompanyId int NULL,
	BatchId int NOT NULL CONSTRAINT DF_PaymentImport_BatchId DEFAULT 0,
	SysPaymentTypeId int NOT NULL CONSTRAINT DF_PaymentImport_SysPaymentTypeId DEFAULT 0,
	Type int NOT NULL CONSTRAINT DF_PaymentImport_Type DEFAULT 0,
	TotalAmount decimal(10, 2) NOT NULL CONSTRAINT DF_PaymentImport_TotalAmount DEFAULT 0,
	NumberOfPayments int NOT NULL CONSTRAINT DF_PaymentImport_NumberOfPayments DEFAULT 0,
	Modified datetime NULL,
	ModifiedBy nvarchar(50) NULL,
	State int NOT NULL CONSTRAINT DF_PaymentImport_State DEFAULT 0
GO
ALTER TABLE dbo.PaymentImport ADD CONSTRAINT
	FK_PaymentImport_PaymentImport FOREIGN KEY
	(
	PaymentImportId
	) REFERENCES dbo.PaymentImport
	(
	PaymentImportId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.PaymentImport ADD CONSTRAINT
	FK_PaymentImport_Company FOREIGN KEY
	(
	ActorCompanyId
	) REFERENCES dbo.Company
	(
	ActorCompanyId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.PaymentImport SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.PaymentImport', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.PaymentImport', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.PaymentImport', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.PaymentRow SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.PaymentRow', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.PaymentRow', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.PaymentRow', 'Object', 'CONTROL') as Contr_Per 


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
ALTER TABLE dbo.PaymentImport
	DROP CONSTRAINT FK_PaymentImport_PaymentImport
GO
ALTER TABLE dbo.PaymentImport SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.PaymentImport', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.PaymentImport', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.PaymentImport', 'Object', 'CONTROL') as Contr_Per 

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
ALTER TABLE dbo.Invoice SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.Invoice', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Invoice', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Invoice', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.PaymentImport SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.PaymentImport', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.PaymentImport', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.PaymentImport', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
CREATE TABLE dbo.PaymentImportInvoiceMapping
	(
	PaymentImportInvoiceMappingId int NOT NULL IDENTITY (1, 1),
	PaymentImportId int NOT NULL,
	InvoiceId int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.PaymentImportInvoiceMapping ADD CONSTRAINT
	PK_PaymentImportInvoiceMapping PRIMARY KEY CLUSTERED 
	(
	PaymentImportInvoiceMappingId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.PaymentImportInvoiceMapping ADD CONSTRAINT
	FK_PaymentImportInvoiceMapping_PaymentImportInvoiceMapping FOREIGN KEY
	(
	PaymentImportInvoiceMappingId
	) REFERENCES dbo.PaymentImportInvoiceMapping
	(
	PaymentImportInvoiceMappingId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.PaymentImportInvoiceMapping ADD CONSTRAINT
	FK_PaymentImportInvoiceMapping_PaymentImport FOREIGN KEY
	(
	PaymentImportId
	) REFERENCES dbo.PaymentImport
	(
	PaymentImportId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.PaymentImportInvoiceMapping ADD CONSTRAINT
	FK_PaymentImportInvoiceMapping_Invoice FOREIGN KEY
	(
	InvoiceId
	) REFERENCES dbo.Invoice
	(
	InvoiceId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.PaymentImportInvoiceMapping SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.PaymentImportInvoiceMapping', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.PaymentImportInvoiceMapping', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.PaymentImportInvoiceMapping', 'Object', 'CONTROL') as Contr_Per 

/*
   den 21 januari 201512:23:36
   User: dba
   Server: extra01\xedev
   Database: SOECompv2
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
CREATE TABLE dbo.PaymentImportIO
	(
	PaymentImportIOId int NOT NULL IDENTITY (1, 1),
	BatchNr int NOT NULL,
	Type int NOT NULL,
	CustomerId int NULL,
	Customer nvarchar(50) NULL,
	InvoiceId int NULL,
	InvoiceNr nvarchar(50) NOT NULL,
	InvoiceAmount decimal(10, 2) NULL,
	RestAmount decimal(10, 2) NULL,
	PaidAmount decimal(10, 2) NULL,
	Currency nvarchar(50) NULL,
	InvoiceDate datetime NULL,
	PaidDate datetime NULL,
	MatchCodeId int NULL,
	Status int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.PaymentImportIO ADD CONSTRAINT
	PK_PaymentImportIO PRIMARY KEY CLUSTERED 
	(
	PaymentImportIOId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.PaymentImportIO SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.PaymentImportIO', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.PaymentImportIO', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.PaymentImportIO', 'Object', 'CONTROL') as Contr_Per 

/*
   den 22 januari 201509:43:34
   User: dba
   Server: extra01\xedev
   Database: SOECompv2
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
ALTER TABLE dbo.PaymentImportIO
	DROP CONSTRAINT FK_PaymentImportIO_PaymentImportIO
GO
ALTER TABLE dbo.PaymentImportIO SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.PaymentImportIO', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.PaymentImportIO', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.PaymentImportIO', 'Object', 'CONTROL') as Contr_Per 

/*
   den 27 januari 201509:40:10
   User: dba
   Server: extra01\xedev
   Database: SOECompv2
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
ALTER TABLE dbo.PaymentImportIO ADD
	State int NOT NULL CONSTRAINT DF_PaymentImportIO_State DEFAULT 0
GO
ALTER TABLE dbo.PaymentImportIO SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.PaymentImportIO', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.PaymentImportIO', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.PaymentImportIO', 'Object', 'CONTROL') as Contr_Per 





/*
   7. helmikuuta 20158:02:57
   User: dba
   Server: dell-db14.i.softone.se\release
   Database: soecompv_education
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
ALTER TABLE dbo.Stock ADD
	AccIn int NOT NULL CONSTRAINT DF_Stock_AccIn DEFAULT 0,
	AccInCh int NOT NULL CONSTRAINT DF_Stock_AccInCh DEFAULT 0,
	AccOut int NOT NULL CONSTRAINT DF_Stock_AccOut DEFAULT 0,
	AccOutCh int NOT NULL CONSTRAINT DF_Stock_AccOutCh DEFAULT 0,
	AccInv int NOT NULL CONSTRAINT DF_Stock_AccInv DEFAULT 0,
	AccInvCh int NOT NULL CONSTRAINT DF_Stock_AccInvCh DEFAULT 0,
	AccLoss int NOT NULL CONSTRAINT DF_Stock_AccLoss DEFAULT 0,
	AccLossCh int NOT NULL CONSTRAINT DF_Stock_AccLossCh DEFAULT 0
GO
ALTER TABLE dbo.Stock SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.Stock', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Stock', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Stock', 'Object', 'CONTROL') as Contr_Per 

/*
   7. helmikuuta 20157:54:24
   User: dba
   Server: dell-db14.i.softone.se\release
   Database: soecompv_education
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
ALTER TABLE dbo.StockTransaction ADD
	VoucherId int NOT NULL CONSTRAINT DF_StockTransaction_VoucherId DEFAULT 0
GO
ALTER TABLE dbo.StockTransaction SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.StockTransaction', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.StockTransaction', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.StockTransaction', 'Object', 'CONTROL') as Contr_Per 


USE [soecompv_education]
GO

/****** Object:  UserDefinedFunction [dbo].[Split]    Script Date: 2015-02-12 13:29:32 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER Function [dbo].[Split] 
(
	@IDs NVARCHAR(MAX),
	@SplitOn nvarchar(5)
)
Returns @Tbl_IDs Table  (ID Int)  As  

Begin 
 -- Append comma
 Set @IDs =  @IDs + @SplitOn
 
 -- Indexes to keep the position of searching
 Declare @Pos1 Int
 Declare @pos2 Int
 
 -- Start from first character 
 Set @Pos1=1
 Set @Pos2=1

 While @Pos1<Len(@IDs)
 Begin
  Set @Pos1 = CharIndex(@SplitOn,@IDs,@Pos1)
  Insert @Tbl_IDs Select  Cast(Substring(@IDs,@Pos2,@Pos1-@Pos2) As Int)  
  
  -- Go to next non comma character
  Set @Pos2=@Pos1+1
  
  -- Search from the next charcater
  Set @Pos1 = @Pos1+1
 End 
 Return
End


GO


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
ALTER TABLE dbo.SupplierInvoice ADD
	AttestGroupId int NULL
GO
ALTER TABLE dbo.SupplierInvoice SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.SupplierInvoice', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.SupplierInvoice', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.SupplierInvoice', 'Object', 'CONTROL') as Contr_Per 


USE [soecompv_education]
GO

/****** Object:  StoredProcedure [dbo].[GetEdiEntriesAllPerLicenseId]    Script Date: 26.2.2015 13:26:22 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[GetEdiEntriesAllPerLicenseId]
	/*@langId					INT,*/
	@startingday   		    SmallDateTime,
	@endingday 				SmallDateTime 
	
AS
BEGIN
    select v.Type, l.LicenseId, l.Name, COUNT(v.EdiEntryId) as Items, l.OrgNr
	from dbo.EdiEntry as v, dbo.Company as c, dbo.License as l
	where 
	v.Created >= @startingday and v.Created < @endingday+1
	and c.ActorCompanyId = v.ActorCompanyId 
	and l.LicenseId = c.LicenseId
	group by l.LicenseId, l.Name, l.OrgNr, v.Type
	order by l.LicenseId, v.Type

END


GO

------------------------------------------------------------
-- This script is generated by SQLDBDiff V4.0
-- http://www.sqldbtools.com 
-- 2015-03-05 21:25:14
-- Note : 
--   Script generated by SQLDBDiff might need some adjustment, please review and test the code against test environements before deployement in production. 
------------------------------------------------------------

USE [soecompv_education]
GO

---$ Alter table dbo.License
IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE OBJECT_ID = OBJECT_ID(N'dbo.License') AND NAME = 'IsAccountingOffice')
BEGIN
    PRINT 'Add column : dbo.License.IsAccountingOffice'
    ALTER TABLE dbo.License
        ADD IsAccountingOffice bit NOT NULL DEFAULT (0)
END
GO

IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE OBJECT_ID = OBJECT_ID(N'dbo.License') AND NAME = 'AccountingOfficeId')
BEGIN
    PRINT 'Add column : dbo.License.AccountingOfficeId'
    ALTER TABLE dbo.License
        ADD AccountingOfficeId INT NOT NULL DEFAULT (0)
END
GO

IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE OBJECT_ID = OBJECT_ID(N'dbo.License') AND NAME = 'AccountingOfficeName')
BEGIN
    PRINT 'Add column : dbo.License.AccountingOfficeName'
    ALTER TABLE dbo.License
        ADD AccountingOfficeName NVARCHAR(100) NOT NULL DEFAULT ('')
END
GO


---$ Alter table dbo.PaymentImport
IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE OBJECT_ID = OBJECT_ID(N'dbo.PaymentImport') AND NAME = 'ActorCompanyId')
BEGIN
    PRINT 'Add column : dbo.PaymentImport.ActorCompanyId'
    ALTER TABLE dbo.PaymentImport
        ADD ActorCompanyId INT NULL
END
GO

IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE OBJECT_ID = OBJECT_ID(N'dbo.PaymentImport') AND NAME = 'BatchId')
BEGIN
    PRINT 'Add column : dbo.PaymentImport.BatchId'
    ALTER TABLE dbo.PaymentImport
        ADD BatchId INT NOT NULL DEFAULT (0)
END
GO

IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE OBJECT_ID = OBJECT_ID(N'dbo.PaymentImport') AND NAME = 'SysPaymentTypeId')
BEGIN
    PRINT 'Add column : dbo.PaymentImport.SysPaymentTypeId'
    ALTER TABLE dbo.PaymentImport
        ADD SysPaymentTypeId INT NOT NULL DEFAULT (0)
END
GO

IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE OBJECT_ID = OBJECT_ID(N'dbo.PaymentImport') AND NAME = 'Type')
BEGIN
    PRINT 'Add column : dbo.PaymentImport.Type'
    ALTER TABLE dbo.PaymentImport
        ADD [Type] INT NOT NULL DEFAULT (0)
END
GO

IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE OBJECT_ID = OBJECT_ID(N'dbo.PaymentImport') AND NAME = 'TotalAmount')
BEGIN
    PRINT 'Add column : dbo.PaymentImport.TotalAmount'
    ALTER TABLE dbo.PaymentImport
        ADD TotalAmount DECIMAL(10,2) NOT NULL DEFAULT (0)
END
GO

IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE OBJECT_ID = OBJECT_ID(N'dbo.PaymentImport') AND NAME = 'NumberOfPayments')
BEGIN
    PRINT 'Add column : dbo.PaymentImport.NumberOfPayments'
    ALTER TABLE dbo.PaymentImport
        ADD NumberOfPayments INT NOT NULL DEFAULT (0)
END
GO

IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE OBJECT_ID = OBJECT_ID(N'dbo.PaymentImport') AND NAME = 'Modified')
BEGIN
    PRINT 'Add column : dbo.PaymentImport.Modified'
    ALTER TABLE dbo.PaymentImport
        ADD Modified datetime NULL
END
GO

IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE OBJECT_ID = OBJECT_ID(N'dbo.PaymentImport') AND NAME = 'ModifiedBy')
BEGIN
    PRINT 'Add column : dbo.PaymentImport.ModifiedBy'
    ALTER TABLE dbo.PaymentImport
        ADD ModifiedBy NVARCHAR(50) NULL
END
GO

IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE OBJECT_ID = OBJECT_ID(N'dbo.PaymentImport') AND NAME = 'State')
BEGIN
    PRINT 'Add column : dbo.PaymentImport.State'
    ALTER TABLE dbo.PaymentImport
        ADD [State] INT NOT NULL DEFAULT (0)
END
GO


---$ Create table dbo.PaymentImportInvoiceMapping
IF OBJECT_ID(N'dbo.PaymentImportInvoiceMapping') IS NULL
BEGIN
    PRINT 'Create table dbo.PaymentImportInvoiceMapping'
    CREATE TABLE dbo.PaymentImportInvoiceMapping
    (
        PaymentImportInvoiceMappingId INT IDENTITY(1,1) NOT NULL,
        PaymentImportId INT NOT NULL,
        InvoiceId INT NOT NULL
    )
    END
GO


---$ Create table dbo.PaymentImportIO
IF OBJECT_ID(N'dbo.PaymentImportIO') IS NULL
BEGIN
    PRINT 'Create table dbo.PaymentImportIO'
    CREATE TABLE dbo.PaymentImportIO
    (
        PaymentImportIOId INT IDENTITY(1,1) NOT NULL,
        BatchNr INT NOT NULL,
        [Type] INT NOT NULL,
        CustomerId INT NULL,
        Customer NVARCHAR(50) NULL,
        InvoiceId INT NULL,
        InvoiceNr NVARCHAR(50) NOT NULL,
        InvoiceAmount DECIMAL(10,2) NULL,
        RestAmount DECIMAL(10,2) NULL,
        PaidAmount DECIMAL(10,2) NULL,
        Currency NVARCHAR(50) NULL,
        InvoiceDate datetime NULL,
        PaidDate datetime NULL,
        MatchCodeId INT NULL,
        [Status] INT NOT NULL,
        [State] INT NOT NULL DEFAULT (0),
        ActorCompanyId INT NOT NULL DEFAULT (0)
    )
    END
GO


---$ Alter table dbo.ProjectIO
PRINT 'Alter table dbo.ProjectIO alter column ProjectIOId'
ALTER TABLE dbo.ProjectIO
    ALTER COLUMN ProjectIOId INT IDENTITY(1,1) NOT NULL
GO

---$ Alter table dbo.StockTransaction
-- Diff on PK, related script will be generated if Indexes/PK is selected in Generate sync scripts form 
 GO

---$ Alter table dbo.TimePayrollTransaction
IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE OBJECT_ID = OBJECT_ID(N'dbo.TimePayrollTransaction') AND NAME = 'IsCentRounding')
BEGIN
    PRINT 'Add column : dbo.TimePayrollTransaction.IsCentRounding'
    ALTER TABLE dbo.TimePayrollTransaction
        ADD IsCentRounding bit NOT NULL DEFAULT (0)
END
GO

IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE OBJECT_ID = OBJECT_ID(N'dbo.TimePayrollTransaction') AND NAME = 'IsQuantityRounding')
BEGIN
    PRINT 'Add column : dbo.TimePayrollTransaction.IsQuantityRounding'
    ALTER TABLE dbo.TimePayrollTransaction
        ADD IsQuantityRounding bit NOT NULL DEFAULT (0)
END
GO


---$ Create table dbo.Tmp_ScanningEntryRow
IF OBJECT_ID(N'dbo.Tmp_ScanningEntryRow') IS NULL
BEGIN
    PRINT 'Create table dbo.Tmp_ScanningEntryRow'
    CREATE TABLE dbo.Tmp_ScanningEntryRow
    (
        ScanningEntryRowId INT IDENTITY(1,1) NOT NULL,
        ScanningEntryId INT NOT NULL,
        [Type] INT NOT NULL,
        TypeName NVARCHAR(100) NOT NULL DEFAULT (''),
        [Name] NVARCHAR(100) NOT NULL,
        [Text] NVARCHAR(512) NOT NULL,
        Format NVARCHAR(max) NOT NULL,
        ValidationError NVARCHAR(100) NOT NULL,
        Position NVARCHAR(100) NOT NULL,
        PageNumber NVARCHAR(100) NOT NULL,
        NewText NVARCHAR(100) NULL,
        Created datetime NULL,
        CreatedBy NVARCHAR(50) NULL,
        Modified datetime NULL,
        ModifiedBy NVARCHAR(50) NULL,
        [State] INT NOT NULL DEFAULT (0)
    )
    END
GO


---$ Alter View dbo.ChangeStatusGridViewCI 
IF OBJECT_ID(N'dbo.ChangeStatusGridViewCI') IS NULL
BEGIN
    PRINT 'Create View : dbo.ChangeStatusGridViewCI'
    exec('create view dbo.ChangeStatusGridViewCI as select null as Col1') 
END
GO

PRINT 'Alter view : dbo.ChangeStatusGridViewCI'
GO

SET QUOTED_IDENTIFIER ON 
go

SET ANSI_NULLS ON 
go




ALTER VIEW [dbo].[ChangeStatusGridViewCI]
AS
SELECT        TOP 100 PERCENT /*Company*/ ISNULL(o.ActorCompanyId, 0) AS OwnerActorId, /*Actor*/ ISNULL(i.ActorId, 0) AS ActorId, c.CustomerNr AS ActorNr, 
                         c.Name AS ActorName, /*Customer*/ c.GracePeriodDays AS CustomerGracePeriodDays, c.BillingTemplate AS DefaultBillingReportTemplate, 
                         ISNULL(CAST(c.PriceListTypeId AS int), 0) AS CustomerPriceListTypeId, /*Origin*/ o.OriginId, o.Type AS OriginType, o.Status, st_stat.Name AS StatusName, 
                         o.Description AS InternalText, /*Invoice*/ i.InvoiceId, i.Type AS InvoiceType, i.IsTemplate AS IsInvoiceTemplate, i.InvoiceNr, i.SeqNr AS InvoiceSeqNr, 
                         i.BillingType AS BillingTypeId, i.StatusIcon AS StatusIcon, ISNULL(i.ProjectId, 0) AS ProjectId, st_bt.Name AS BillingTypeName, 
                         /*Customer invoice*/ ci.InvoiceHeadText, ci.RegistrationType, ci.DeliveryAddressId, ci.BillingAddressId, ci.BillingInvoicePrinted, ci.HasHouseholdTaxDeduction, ci.FixedPriceOrder, 
                         ci.InsecureDebt, ci.MultipleAssetRows, ci.NoOfReminders AS NoOfReminders, st_fpo.Name AS FixedPriceOrderName , (SELECT COUNT(*) FROM CustomerInvoicePrintedReminder cipr WHERE cipr.CustomerInvoiceOriginId=i.InvoiceId) AS NoOfPrintedReminders, 
						 ISNULL(i.RemainingAmount, 0) AS RemainingAmount, ISNULL(i.RemainingAmountExVat, 0) 
                         AS RemainingAmountExVat, ci.DeliveryDate, '' AS ContractGroupName, /*cg.Name AS ContractGroupName, */ ci.NextContractPeriodDate, ci.NextContractPeriodYear, 
                         ci.NextContractPeriodValue, /* Shifttype*/ st.Name AS ShiftTypeName, st.Color AS ShiftTypeColor, /*Common*/ st_bt.LangId AS LangId, s_cu.Code AS CurrencyCode, 
                         i.CurrencyRate, cu.SysCurrencyId, vs.AccountYearId, vs.VoucherSeriesId, ISNULL(CAST(i.VoucherHeadId AS bit), 0) AS HasVoucher, i.InvoiceDate, i.VoucherDate, 
                         i.DueDate, ISNULL(pr.PayDate, i.DueDate) AS PayDate, i.TotalAmount AS InvoiceTotalAmount, i.TotalAmountCurrency AS InvoiceTotalAmountCurrency, i.VATAmount, 
                         i.VATAmountCurrency, i.PaidAmount AS InvoicePaidAmount, i.PaidAmountCurrency AS InvoicePaidAmountCurrency, ISNULL(i.TotalAmount - i.PaidAmount, 0) 
                         AS InvoicePayAmount, ISNULL(i.TotalAmountCurrency - i.PaidAmountCurrency, 0) AS InvoicePayAmountCurrency, i.FullyPayed, p.Number AS ProjectNumber, 
                         ISNULL(ci.OrderNumbers, '') AS OrderNumbers, COALESCE (ci.InvoiceDeliveryType, c.InvoiceDeliveryType, 0) AS DeliveryType, 
                         i.ExportStatus AS ExportStatus
FROM            dbo.Origin AS o WITH (NOLOCK) INNER JOIN
                         dbo.Invoice AS i WITH (NOLOCK) ON i.InvoiceId = o.OriginId INNER JOIN
                         dbo.CustomerInvoice AS ci WITH (NOLOCK) ON ci.InvoiceId = i.InvoiceId LEFT OUTER JOIN						 
                         dbo.ShiftType AS st WITH (NOLOCK) ON ci.ShiftTypeId = st.ShiftTypeId INNER JOIN
                         dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i.CurrencyId INNER JOIN
                         dbo.Customer AS c WITH (NOLOCK) ON c.ActorCustomerId = i.ActorId INNER JOIN
                         dbo.VoucherSeries AS vs WITH (NOLOCK) ON o.VoucherSeriesId = vs.VoucherSeriesId INNER JOIN
                         SoesysV2.dbo.SysTerm AS st_bt WITH (NOLOCK) ON i.BillingType = st_bt.SysTermId INNER JOIN
                         SoesysV2.dbo.SysTerm AS st_stat WITH (NOLOCK) ON o.Status = st_stat.SysTermId INNER JOIN
						 SoesysV2.dbo.SysTerm AS st_fpo WITH (NOLOCK) ON ci.FixedPriceOrder = st_fpo.SysTermId INNER JOIN
                         SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId LEFT OUTER JOIN
                         dbo.ContractGroup AS cg WITH (NOLOCK) ON ci.ContractGroupId = cg.ContractGroupId LEFT OUTER JOIN
                         dbo.Project AS p WITH (NOLOCK) ON i.ProjectId = p.ProjectId OUTER APPLY
                             (SELECT        TOP 1 PayDate
                               FROM            dbo.PaymentRow WITH (NOLOCK)
                               WHERE        InvoiceId = i.InvoiceId
                               ORDER BY PayDate DESC) pr
WHERE        (st_stat.LangId = st_bt.LangId) AND (o.Type = 2 OR
                         o.Type = 5 OR
                         o.Type = 6 OR
                         o.Type = 7) AND (i.State = 0) AND (st_bt.SysTermGroupId = 27) AND (st_stat.SysTermGroupId = 30) AND (st_fpo.SysTermGroupId = 443)

GO

---$ Alter View dbo.ChangeStatusGridViewSI 
IF OBJECT_ID(N'dbo.ChangeStatusGridViewSI') IS NULL
BEGIN
    PRINT 'Create View : dbo.ChangeStatusGridViewSI'
    exec('create view dbo.ChangeStatusGridViewSI as select null as Col1') 
END
GO

PRINT 'Alter view : dbo.ChangeStatusGridViewSI'
GO

SET QUOTED_IDENTIFIER ON 
go

SET ANSI_NULLS ON 
go



ALTER VIEW [dbo].[ChangeStatusGridViewSI]
AS

/*
 ATTENTION:
 Changes should be made in both view [ChangeStatusGridViewSI] and procedure [GetChangeStatusGridViewSI]
 */

SELECT TOP 100 PERCENT 
    --Company
    ISNULL(o.ActorCompanyId, 0) AS OwnerActorId, 
    --Actor
    ISNULL(i.ActorId, 0) AS ActorId,
    s.SupplierNr AS ActorNr, 
    s.Name AS ActorName, 
    --Supplier
    s.BlockPayment AS SupplierBlockPayment, 
    --Customer
    --Origin
    o.OriginId, 
    o.Type AS OriginType, 
    o.Status, 
    st_stat.Name AS StatusName, 
    o.Description AS InternalText,       
    --Invoice
    i.InvoiceId, 
    i.Type AS InvoiceType, 
	i.IsTemplate as IsInvoiceTemplate,
    i.InvoiceNr, 
    i.SeqNr as InvoiceSeqNr, 
    i.BillingType AS BillingTypeId, 
    i.StatusIcon AS StatusIcon, 
	i.VatType as VatType,
    ISNULL(i.ProjectId, 0) AS ProjectId, 
    st_bt.Name AS BillingTypeName, 
    ISNULL(CAST(0 AS decimal(10, 2)), 0) AS RemainingAmount, 
    ISNULL(CAST(0 AS decimal(10, 2)), 0) AS RemainingAmountExVat, 
    --Supplier invoice
    si.BlockPayment, 
    si.MultipleDebtRows, 
    si.AttestStateId AS SupplierInvoiceAttestStateId,
    si.PaymentMethodId AS SupplierInvoicePaymentMethodId, 
    ISNULL(pm.PaymentType, 0) AS SupplierInvoicePaymentMethodType,
    --Payment method
    ISNULL(pm.SysPaymentMethodId, 0) AS SysPaymentMethodId,       
    ISNULL(pm.Name, '') AS PaymentMethodName, 
    --Common
    st_bt.LangId AS LangId, 
    s_cu.Code AS CurrencyCode, 
    i.CurrencyRate, 
    cu.SysCurrencyId, 
    vs.AccountYearId, 
    vs.VoucherSeriesId, 
    ISNULL(CAST(i.VoucherHeadId AS bit), 0) AS HasVoucher, 
    i.InvoiceDate, 
    i.VoucherDate, 
    i.DueDate, 
    ISNULL(pr.PayDate, i.DueDate) AS PayDate, 
    i.TotalAmount AS InvoiceTotalAmount, 
    i.TotalAmountCurrency AS InvoiceTotalAmountCurrency, 
    i.VATAmount, 
	i.VATAmountCurrency,
    i.PaidAmount AS InvoicePaidAmount, 
    i.PaidAmountCurrency AS InvoicePaidAmountCurrency, 
    ISNULL(i.TotalAmount - i.PaidAmount, 0) AS InvoicePayAmount, 
    ISNULL(i.TotalAmountCurrency - i.PaidAmountCurrency, 0) AS InvoicePayAmountCurrency, 
    i.FullyPayed,
    LEFT(ob.list,LEN(ob.list) - 1) AS PaymentStatuses,
    si.CurrentAttestUsers,
	i.PaymentNr as PaymentNr
FROM         
    dbo.Origin AS o WITH (NOLOCK) INNER JOIN
    dbo.Invoice AS i WITH (NOLOCK) ON i.InvoiceId = o.OriginId INNER JOIN
    dbo.SupplierInvoice AS si WITH (NOLOCK) ON si.InvoiceId = i.InvoiceId INNER JOIN
    dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i.CurrencyId INNER JOIN
    dbo.Supplier AS s WITH (NOLOCK) ON s.ActorSupplierId = i.ActorId INNER JOIN
    dbo.VoucherSeries AS vs WITH (NOLOCK) ON o.VoucherSeriesId = vs.VoucherSeriesId INNER JOIN
    SoesysV2.dbo.SysTerm AS st_bt WITH (NOLOCK) ON i.BillingType = st_bt.SysTermId INNER JOIN
    SoesysV2.dbo.SysTerm AS st_stat WITH (NOLOCK) ON o.Status = st_stat.SysTermId LEFT OUTER JOIN
    dbo.PaymentMethod AS pm WITH (NOLOCK) ON pm.PaymentMethodId = si.PaymentMethodId INNER JOIN
    SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId OUTER APPLY
    (SELECT TOP 1 PayDate, PaymentNr FROM dbo.PaymentRow WITH (NOLOCK) WHERE InvoiceId = i.InvoiceId ORDER BY PayDate DESC) pr OUTER APPLY 
    (
      SELECT   CONVERT(VARCHAR(2),Status) + ',' AS [text()]
	  FROM     PaymentRow pr
	  WHERE    pr.InvoiceId = i.InvoiceId AND pr.State = 0
	  ORDER BY pr.PaymentRowId
	  FOR XML PATH('')
	 ) ob(list)          
WHERE     
    (st_stat.LangId = st_bt.LangId) AND
    (o.Type = 1) AND 
    (i.State = 0) AND 
    (st_bt.SysTermGroupId = 27) AND 
    (st_stat.SysTermGroupId = 30)

GO

---$ Alter View dbo.EdiEntryView 
IF OBJECT_ID(N'dbo.EdiEntryView') IS NULL
BEGIN
    PRINT 'Create View : dbo.EdiEntryView'
    exec('create view dbo.EdiEntryView as select null as Col1') 
END
GO

PRINT 'Alter view : dbo.EdiEntryView'
GO

SET QUOTED_IDENTIFIER ON 
go

SET ANSI_NULLS ON 
go


ALTER VIEW dbo.EdiEntryView
AS
SELECT        edi.EdiEntryId, edi.ActorCompanyId, edi.Type, edi.Status, ST_EStatus.Name AS StatusName, ST_EdiSourceType.Name AS SourceTypeName, edi.MessageType, 
                         ST_MessageType.Name AS MessageTypeName, edi.BillingType, ST_BillingType.Name AS BillingTypeName, edi.SysWholesellerId AS WholeSellerId, 
                         edi.WholesellerName, edi.BuyerId, edi.BuyerReference, ISNULL((CASE WHEN edi.Pdf IS NOT NULL THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END), CAST(0 AS bit)) 
                         AS HasPdf, edi.ErrorCode, edi.Created, edi.State, ISNULL(edi.ScanningEntryInvoiceId, 0) AS ScanningEntryId, 0 AS NrOfPages, 0 AS NrOfInvoices, 
                         '' AS OperatorMessage, edi.Date, edi.InvoiceDate, edi.DueDate, edi.Sum, edi.SumCurrency, edi.SumVat, edi.SumVatCurrency, edi.CurrencyId, curr.SysCurrencyId, 
                         scurr.Code AS CurrencyCode, edi.CurrencyRate, edi.OrderId, edi.OrderStatus, ST_OStatus.Name AS OrderStatusName, edi.OrderNr, edi.SellerOrderNr, edi.InvoiceId, 
                         edi.InvoiceStatus, ST_IStatus.Name AS InvoiceStatusName, edi.InvoiceNr, edi.SeqNr, cust.ActorCustomerId AS CustomerId, cust.CustomerNr, 
                         cust.Name AS CustomerName, supp.ActorSupplierId AS SupplierId, supp.SupplierNr, supp.Name AS SupplierName, ST_EStatus.LangId, 
                         supp.AttestWorkFlowGroupId
FROM            dbo.EdiEntry AS edi INNER JOIN
                         SOESysV2.dbo.SysTerm AS ST_EStatus ON ST_EStatus.SysTermId = edi.Status AND ST_EStatus.SysTermGroupId = 118 INNER JOIN
                         SOESysV2.dbo.SysTerm AS ST_OStatus ON ST_OStatus.SysTermId = edi.OrderStatus AND ST_OStatus.SysTermGroupId = 114 AND 
                         ST_OStatus.LangId = ST_EStatus.LangId INNER JOIN
                         SOESysV2.dbo.SysTerm AS ST_IStatus ON ST_IStatus.SysTermId = edi.InvoiceStatus AND ST_IStatus.SysTermGroupId = 115 AND 
                         ST_IStatus.LangId = ST_EStatus.LangId INNER JOIN
                         SOESysV2.dbo.SysTerm AS ST_MessageType ON ST_MessageType.SysTermId = edi.MessageType AND ST_MessageType.SysTermGroupId = 183 AND 
                         ST_MessageType.LangId = ST_EStatus.LangId INNER JOIN
                         SOESysV2.dbo.SysTerm AS ST_EdiSourceType ON ST_EdiSourceType.SysTermId = edi.Type AND ST_EdiSourceType.SysTermGroupId = 116 AND 
                         ST_EdiSourceType.LangId = ST_EStatus.LangId INNER JOIN
                         dbo.Currency AS curr ON curr.CurrencyId = edi.CurrencyId INNER JOIN
                         SOESysV2.dbo.SysCurrency AS scurr ON scurr.SysCurrencyId = curr.SysCurrencyId LEFT OUTER JOIN
                         SOESysV2.dbo.SysTerm AS ST_BillingType ON ST_BillingType.SysTermId = edi.BillingType AND ST_BillingType.SysTermGroupId = 27 AND 
                         ST_BillingType.LangId = ST_EStatus.LangId LEFT OUTER JOIN
                         dbo.Invoice AS i ON edi.OrderId IS NOT NULL AND i.InvoiceId = edi.OrderId LEFT OUTER JOIN
                         dbo.Customer AS cust ON cust.ActorCustomerId = i.ActorId LEFT OUTER JOIN
                         dbo.Supplier AS supp ON supp.ActorSupplierId = edi.ActorSupplierId
WHERE        (edi.Type = 1)

GO

---$ Alter View dbo.ScanningEntryView 
IF OBJECT_ID(N'dbo.ScanningEntryView') IS NULL
BEGIN
    PRINT 'Create View : dbo.ScanningEntryView'
    exec('create view dbo.ScanningEntryView as select null as Col1') 
END
GO

PRINT 'Alter view : dbo.ScanningEntryView'
GO

SET QUOTED_IDENTIFIER ON 
go

SET ANSI_NULLS ON 
go


/*I(SNULL(edi.ScanningEntryId, 0) > 0)*/
ALTER VIEW dbo.ScanningEntryView
AS
SELECT        edi.EdiEntryId, edi.ActorCompanyId, edi.Type, scanning.Status, ST_EStatus.Name AS StatusName, st_EdiSourceType.Name AS SourceTypeName, 
                         scanning.MessageType, ST_MessageType.Name AS MessageTypeName, edi.BillingType, ST_BillingType.Name AS BillingTypeName, 
                         edi.SysWholesellerId AS WholeSellerId, edi.WholesellerName, edi.BuyerId, edi.BuyerReference, ISNULL((CASE WHEN scanning.Image IS NOT NULL 
                         THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END), CAST(0 AS bit)) AS HasPdf, edi.ErrorCode, edi.Created, edi.State, scanning.ScanningEntryId, scanning.NrOfPages, 
                         scanning.NrOfInvoices, scanning.OperatorMessage, edi.Date, edi.InvoiceDate, edi.DueDate, edi.Sum, edi.SumCurrency, edi.SumVat, edi.SumVatCurrency, 
                         edi.CurrencyId, curr.SysCurrencyId, scurr.Code AS CurrencyCode, edi.CurrencyRate, edi.OrderId, edi.OrderStatus, ST_OStatus.Name AS OrderStatusName, 
                         edi.OrderNr, edi.SellerOrderNr, edi.InvoiceId, edi.InvoiceStatus, ST_IStatus.Name AS InvoiceStatusName, edi.InvoiceNr, edi.SeqNr, 
                         cust.ActorCustomerId AS CustomerId, cust.CustomerNr, cust.Name AS CustomerName, supp.ActorSupplierId AS SupplierId, supp.SupplierNr, 
                         supp.Name AS SupplierName, ST_EStatus.LangId, supp.AttestWorkFlowGroupId
FROM            dbo.EdiEntry AS edi INNER JOIN
                         dbo.ScanningEntry AS scanning ON edi.ScanningEntryInvoiceId = scanning.ScanningEntryId INNER JOIN
                         SOESysV2.dbo.SysTerm AS ST_EStatus ON ST_EStatus.SysTermId = edi.Status AND ST_EStatus.SysTermGroupId = 143 INNER JOIN
                         SOESysV2.dbo.SysTerm AS ST_OStatus ON ST_OStatus.SysTermId = edi.OrderStatus AND ST_OStatus.SysTermGroupId = 114 AND 
                         ST_OStatus.LangId = ST_EStatus.LangId INNER JOIN
                         SOESysV2.dbo.SysTerm AS ST_IStatus ON ST_IStatus.SysTermId = edi.InvoiceStatus AND ST_IStatus.SysTermGroupId = 115 AND 
                         ST_IStatus.LangId = ST_EStatus.LangId INNER JOIN
                         SOESysV2.dbo.SysTerm AS ST_MessageType ON ST_MessageType.SysTermId = 2 AND ST_MessageType.SysTermGroupId = 144 AND 
                         ST_MessageType.LangId = ST_EStatus.LangId INNER JOIN
                         SOESysV2.dbo.SysTerm AS st_EdiSourceType ON st_EdiSourceType.SysTermId = edi.Type AND st_EdiSourceType.SysTermGroupId = 116 AND 
                         st_EdiSourceType.LangId = ST_EStatus.LangId INNER JOIN
                         dbo.Currency AS curr ON curr.CurrencyId = edi.CurrencyId INNER JOIN
                         SOESysV2.dbo.SysCurrency AS scurr ON scurr.SysCurrencyId = curr.SysCurrencyId LEFT OUTER JOIN
                         SOESysV2.dbo.SysTerm AS ST_BillingType ON ST_BillingType.SysTermId = edi.BillingType AND ST_BillingType.SysTermGroupId = 27 AND 
                         ST_BillingType.LangId = ST_EStatus.LangId LEFT OUTER JOIN
                         dbo.Invoice AS i ON edi.OrderId IS NOT NULL AND i.InvoiceId = edi.OrderId LEFT OUTER JOIN
                         dbo.Customer AS cust ON cust.ActorCustomerId = i.ActorId LEFT OUTER JOIN
                         dbo.Supplier AS supp ON supp.ActorSupplierId = edi.ActorSupplierId
WHERE        (edi.Type = 2)

GO

---$ Alter View dbo.TimeCodeTransactionBillingView 
IF OBJECT_ID(N'dbo.TimeCodeTransactionBillingView') IS NULL
BEGIN
    PRINT 'Create View : dbo.TimeCodeTransactionBillingView'
    exec('create view dbo.TimeCodeTransactionBillingView as select null as Col1') 
END
GO

PRINT 'Alter view : dbo.TimeCodeTransactionBillingView'
GO

SET QUOTED_IDENTIFIER ON 
go

SET ANSI_NULLS ON 
go


















ALTER VIEW [dbo].[TimeCodeTransactionBillingView]
AS

--Transactions from offer/order/invoice
SELECT
	--TimeCodeTransaction
	tct.TimeCodeTransactionId,
	--Origin
	ori.ActorCompanyId,
	ori.Type as HeadType,
	ori.Status as HeadStatus,
	ISNULL((CASE WHEN ori.Type = 5 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END), CAST(0 AS bit)) AS HeadIsOffer, 
	ISNULL((CASE WHEN ori.Type = 6 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END), CAST(0 AS bit)) AS HeadIsOrder, 
	ISNULL((CASE WHEN ori.Type = 2 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END), CAST(0 AS bit)) AS HeadIsInvoice, 
	st_status.Name as HeadStatusName,
	--Invoice
	inv.InvoiceId,
	inv.InvoiceNr as HeadNr,
	inv.InvoiceDate as HeadDate,
	inv.Created as HeadCreated,
	inv.CreatedBy as HeadCreatedBy,
	inv.DefaultDim1AccountId as HeadDim1Account,
	inv.DefaultDim2AccountId as HeadDim2Account,
	inv.DefaultDim3AccountId as HeadDim3Account,
	inv.DefaultDim4AccountId as HeadDim4Account,
	inv.DefaultDim5AccountId as HeadDim5Account,
	inv.DefaultDim6AccountId as HeadDim6Account,
	--CustomerInvoiceRow
	row.CustomerInvoiceRowId,
	row.Quantity as RowQuantity,
	row.Amount as RowAmount,
	row.AmountCurrency as RowAmountCurrency,
	row.AmountLedgerCurrency as RowAmountLedgerCurrency,
	row.AmountEntCurrency as RowAmountEntCurrency,
    row.SumAmount as RowSumAmount,
    row.SumAmountCurrency as RowSumAmountCurrency,
    row.SumAmountLedgerCurrency as RowSumAmountLedgerCurrency,
    row.SumAmountEntCurrency as RowSumAmountEntCurrency,
    row.VatAmount as RowVatAmount,
    row.VatAmountCurrency as RowVatAmountCurrency,
    row.VatAmountLedgerCurrency as RowVatAmountLedgerCurrency,
    row.VatAmountEntCurrency as RowVatAmountEntCurrency,
    row.VatRate as RowVatRate,
    row.IsTimeProjectRow as RowIsTimeProjectRow,
    row.Created as RowCreated,
    row.CreatedBy as RowCreatedBy,
    row.PurchasePrice as RowPurchasePrice,
    row.MarginalIncome as RowMarginalIncome,
    row.MarginalIncomeRatio as RowMarginalIncomeRatio,
    row.DiscountPercent as RowDiscountPercent,
    row.AttestStateId as RowAttestStateId,
    row.TargetRowId as TargetRowId,
    row.IsFreightAmountRow,
    row.IsInvoiceFeeRow,
    row.IsCentRoundingRow,
    row.IsInterestRow,
    row.IsReminderRow,
    --Product
	ISNULL(prod.ProductId, 0) as RowProductId,
	ISNULL(prod.Number, '') as RowProductNumber,
    ISNULL(prod.Name, '') as RowProductName,
    ISNULL(prod.Description, '') as RowProductDescription,
	--InvoiceProduct
	ISNULL(invProd.CalculationType, 0) as RowCalculationType,
    --ProductUnit
    ISNULL(unit.Code, '') as RowProductUnitCode,
    ISNULL(unit.Name, '') as RowProductUnitName,
	--Lang
	st_status.LangId
FROM
	TimeCodeTransaction as tct WITH (NOLOCK) OUTER APPLY
	(
		SELECT		TOP 1 *
		FROM		TimeInvoiceTransaction
		WHERE		TimeCodeTransactionId = tct.TimeCodeTransactionId
		ORDER BY	TimeCodeTransactionId
	) tit INNER JOIN		
	CustomerInvoiceRow as row WITH (NOLOCK) on row.CustomerInvoiceRowId = tit.CustomerInvoiceRowId INNER JOIN
	Product as prod WITH (NOLOCK) on prod.ProductId = row.ProductId INNER JOIN
	InvoiceProduct as invProd WITH(NOLOCK) on invProd.ProductId = prod.ProductId INNER JOIN
	--ProductUnit as unit WITH (NOLOCK) on unit.ProductUnitId = prod.ProductUnitId INNER JOIN
	Invoice as inv WITH (NOLOCK) on inv.InvoiceId = row.InvoiceId INNER JOIN
	Origin as ori WITH (NOLOCK) on ori.OriginId = inv.InvoiceId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_status ON ori.Status = st_status.SysTermId LEFT OUTER JOIN
	ProductUnit as unit WITH (NOLOCK) on unit.ProductUnitId = prod.ProductUnitId
WHERE
	ori.Type IN(2,5,6) AND
	row.State = 0 AND
	inv.State = 0 AND
	(st_status.SysTermGroupId = 30)

GO

---$ Alter View dbo.TimeCodeTransactionExtTransactionsView 
IF OBJECT_ID(N'dbo.TimeCodeTransactionExtTransactionsView') IS NULL
BEGIN
    PRINT 'Create View : dbo.TimeCodeTransactionExtTransactionsView'
    exec('create view dbo.TimeCodeTransactionExtTransactionsView as select null as Col1') 
END
GO

PRINT 'Alter view : dbo.TimeCodeTransactionExtTransactionsView'
GO

SET QUOTED_IDENTIFIER ON 
go

SET ANSI_NULLS ON 
go






ALTER VIEW [dbo].[TimeCodeTransactionExtTransactionsView]
AS

--TimeInvoiceTransactions
SELECT
	--TimeCodeTransaction
	tct.TimeCodeTransactionId,
	ISNULL(CAST (1 as bit), 0) as IsInvoiceTransaction,
	ISNULL(CAST (0 as bit), 0) as IsPayrollTransaction,
	tct.CustomerInvoiceRowId as ExternalCustomerInvoiceRowId,
	--TimeInvoiceTransaction
	ext.TimeInvoiceTransactionId as ExternalTransactionId,
	ext.Quantity as ExternalTransactionQuantity,
	ext.InvoiceQuantity as ExternalTransactionInvoiceQuantity,
	ext.Amount as ExternalTransactionAmount,
	ext.AmountCurrency as ExternalTransactionAmountCurrency,
	ext.AmountLedgerCurrency as ExternalTransactionAmountLedgerCurrency,
	ext.AmountEntCurrency as ExternalTransactionAmountEntCurrency,
	ext.VatAmount as ExternalTransactionVatAmount,
	ext.VatAmountCurrency as ExternalTransactionVatAmountCurrency,
	ext.VatAmountLedgerCurrency as ExternalTransactionVatAmountLedgerCurrency,
	ext.VatAmountEntCurrency as ExternalTransactionVatAmountEntCurrency,
	'' as ExternalTransactionComment,	
	--Product
	prod.Number as ExternalTransactionCode,
	prod.Name as ExternalTransactionName, 
	--TimeBlockDate
	tbd.TimeBlockDateId,
	tbd.Status as TimeBlockDateStatus,
	tbd.Date as ExternalTransactionDate,
	--Employee
	emp.EmployeeId,
	emp.EmployeeNr,
	emp.CalculatedCostPerHour,
	--ContactPerson
	cp.FirstName as EmployeeFirstName,
	cp.LastName as EmployeeLastName,
	--Invoice
	ISNULL(inv.DefaultDim1AccountId, 0) as Dim1AccountId,
	ISNULL(inv.DefaultDim2AccountId, 0) as Dim2AccountId,
	ISNULL(inv.DefaultDim3AccountId, 0) as Dim3AccountId,
	ISNULL(inv.DefaultDim4AccountId, 0) as Dim4AccountId,
	ISNULL(inv.DefaultDim5AccountId, 0) as Dim5AccountId,
	ISNULL(inv.DefaultDim6AccountId, 0) as Dim6AccountId
FROM
	TimeCodeTransaction as tct WITH (NOLOCK) INNER JOIN
	TimeInvoiceTransaction as ext WITH (NOLOCK) on ext.TimeCodeTransactionId = tct.TimeCodeTransactionId INNER JOIN
	Product as prod WITH (NOLOCK) on prod.ProductId = ext.InvoiceProductId INNER JOIN
	TimeBlockDate as tbd WITH (NOLOCK) on tbd.TimeBlockDateId = ext.TimeBlockDateId INNER JOIN
	Employee as emp WITH (NOLOCK) on emp.EmployeeId = tbd.EmployeeId INNER JOIN
	ContactPerson as cp WITH (NOLOCK) on cp.ActorContactPersonId = emp.ContactPersonId LEFT OUTER JOIN
	CustomerInvoiceRow as cir WITH (NOLOCK) on ext.CustomerInvoiceRowId = cir.CustomerInvoiceRowId INNER JOIN
	Invoice as inv WITH (NOLOCK) on cir.InvoiceId = inv.InvoiceId
	
WHERE
	ext.State = 0
	
UNION

--TimePayrolllTransactions
SELECT 
	--TimeCodeTransaction
	tct.TimeCodeTransactionId,
	ISNULL(CAST (0 as bit), 0) as IsInvoiceTransaction,
	ISNULL(CAST (1 as bit), 0) as IsPayrollTransaction,
	tct.CustomerInvoiceRowId as ExternalCustomerInvoiceRowId,
	--TimePayrollTransactions
	ext.TimePayrollTransactionId as ExternalTransactionId,
	ext.Quantity as ExternalTransactionQuantity,
	0 as ExternalTransactionInvoiceQuantity,
	ext.Amount as ExternalTransactionAmount,
	ext.AmountCurrency as ExternalTransactionAmountCurrency,
	ext.AmountLedgerCurrency as ExternalTransactionAmountLedgerCurrency,
	ext.AmountEntCurrency as ExternalTransactionAmountEntCurrency,
	ext.VatAmount as ExternalTransactionVatAmount,
	ext.VatAmountCurrency as ExternalTransactionVatAmountCurrency,
	ext.VatAmountLedgerCurrency as ExternalTransactionVatAmountLedgerCurrency,
	ext.VatAmountEntCurrency as ExternalTransactionVatAmountEntCurrency,
	ext.Comment as ExternalTransactionComment,	
	--Product
	prod.Number as ExternalTransactionCode,
	prod.Name as ExternalTransactionName, 
	--TimeBlockDate
	tbd.TimeBlockDateId,
	tbd.Status as TimeBlockDateStatus,
	tbd.Date as ExternalTransactionDate,
	--Employee
	emp.EmployeeId,
	emp.EmployeeNr,
	emp.CalculatedCostPerHour,
	--ContactPerson
	cp.FirstName as EmployeeFirstName,
	cp.LastName as EmployeeLastName,
	--Invoice
	0 as Dim1AccountId,
	0 as Dim2AccountId,
	0 as Dim3AccountId,
	0 as Dim4AccountId,
	0 as Dim5AccountId,
	0 as Dim6AccountId
FROM
	TimeCodeTransaction as tct WITH (NOLOCK) INNER JOIN
	TimePayrollTransaction as ext WITH (NOLOCK) on ext.TimeCodeTransactionId = tct.TimeCodeTransactionId INNER JOIN
	Product as prod WITH (NOLOCK) on prod.ProductId = ext.PayrollProductId INNER JOIN
	TimeBlockDate as tbd WITH (NOLOCK) on tbd.TimeBlockDateId = ext.TimeBlockDateId INNER JOIN
	Employee as emp WITH (NOLOCK) on emp.EmployeeId = tbd.EmployeeId INNER JOIN
	ContactPerson as cp WITH (NOLOCK) on cp.ActorContactPersonId = emp.ContactPersonId
WHERE
	ext.State = 0

GO

---$ Alter Procedure dbo.GetChangeStatusGridViewCI 
IF OBJECT_ID(N'dbo.GetChangeStatusGridViewCI') IS NULL
BEGIN
    PRINT 'Create procedure : dbo.GetChangeStatusGridViewCI'
    EXECUTE('CREATE PROCEDURE dbo.GetChangeStatusGridViewCI AS RETURN 0') 
END
GO

PRINT 'Alter procedure : dbo.GetChangeStatusGridViewCI'
GO

SET QUOTED_IDENTIFIER ON 
go

SET ANSI_NULLS ON 
go



ALTER PROCEDURE [dbo].[GetChangeStatusGridViewCI]
	@actorCompanyId INT,
	@langId INT,
	@originType INT
AS
BEGIN
	SET NOCOUNT ON;

/*
 ATTENTION:
 Changes should be made in both view [ChangeStatusGridViewCI] and procedure [GetChangeStatusGridViewCI]
 */

SELECT TOP 100 PERCENT 
	--Company
	ISNULL(o.ActorCompanyId, 0) AS OwnerActorId, 
	--Actor
	ISNULL(i.ActorId, 0) AS ActorId,
	c.CustomerNr AS ActorNr, 
	c.Name AS ActorName, 
	--Customer
	c.GracePeriodDays AS CustomerGracePeriodDays, 
	c.BillingTemplate AS DefaultBillingReportTemplate,
	ISNULL(CAST(c.PriceListTypeId AS int), 0) AS CustomerPriceListTypeId,  
	--Origin
	o.OriginId, 
	o.Type AS OriginType, 
	o.Status, 
	st_stat.Name AS StatusName, 
	o.Description AS InternalText, 
	--Invoice
	i.InvoiceId, 
	i.Type AS InvoiceType, 
	i.IsTemplate as IsInvoiceTemplate,
	i.InvoiceNr, 
	i.SeqNr as InvoiceSeqNr, 
	i.BillingType AS BillingTypeId, 
	i.StatusIcon AS StatusIcon, 
	ISNULL(i.ProjectId, 0) AS ProjectId,
	st_bt.Name AS BillingTypeName, 
	--Customer invoice
	ci.InvoiceHeadText, 
	ci.RegistrationType, 
	ci.DeliveryAddressId, 
	ci.BillingAddressId,
	ci.BillingInvoicePrinted, 
	ci.HasHouseholdTaxDeduction,
	ci.FixedPriceOrder,
	ci.InsecureDebt,
	ci.MultipleAssetRows, 
	ci.NoOfReminders,	
	ISNULL(i.RemainingAmount, 0) AS RemainingAmount, 
	ISNULL(i.RemainingAmountExVat, 0) AS RemainingAmountExVat,
	ci.DeliveryDate,
	--Contract
	'' AS ContractGroupName,--cg.Name AS ContractGroupName, 
	ci.NextContractPeriodDate, 
	ci.NextContractPeriodYear, 
	ci.NextContractPeriodValue, 
	st_fpo.Name AS FixedPriceOrderName,
	-- Shifttype
	st.Name as ShiftTypeName,
	st.Color as ShiftTypeColor,
	--Common
	st_bt.LangId AS LangId, 
	s_cu.Code AS CurrencyCode, 
	i.CurrencyRate, 
	cu.SysCurrencyId, 
	vs.AccountYearId, 
	vs.VoucherSeriesId, 
	ISNULL(CAST(i.VoucherHeadId AS bit), 0) AS HasVoucher, 
	i.InvoiceDate, 
	i.VoucherDate, 
	i.DueDate, 
	ISNULL(pr.PayDate, i.DueDate) AS PayDate, 
	i.TotalAmount AS InvoiceTotalAmount, 
	i.TotalAmountCurrency AS InvoiceTotalAmountCurrency, 
	i.VATAmount, 
	i.VATAmountCurrency, 
	i.PaidAmount AS InvoicePaidAmount, 
	i.PaidAmountCurrency AS InvoicePaidAmountCurrency, 
	ISNULL(i.TotalAmount - i.PaidAmount, 0) AS InvoicePayAmount, 
	ISNULL(i.TotalAmountCurrency - i.PaidAmountCurrency, 0) AS InvoicePayAmountCurrency, 
	i.FullyPayed,
	p.Number AS ProjectNumber,
	ISNULL(ci.OrderNumbers, '') AS OrderNumbers,
	COALESCE (ci.InvoiceDeliveryType, c.InvoiceDeliveryType, 0) AS DeliveryType,
	i.ExportStatus,
	(SELECT COUNT(*) FROM CustomerInvoicePrintedReminder cipr WHERE cipr.CustomerInvoiceOriginId=i.InvoiceId) AS NoOfPrintedReminders
FROM
	dbo.Origin AS o WITH (NOLOCK) INNER JOIN
	dbo.Invoice AS i WITH (NOLOCK) ON i.InvoiceId = o.OriginId INNER JOIN
	dbo.CustomerInvoice AS ci WITH (NOLOCK) ON ci.InvoiceId = i.InvoiceId LEFT OUTER JOIN
	dbo.ShiftType AS st WITH (NOLOCK) ON ci.ShiftTypeId = st.ShiftTypeId INNER JOIN
	dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i.CurrencyId INNER JOIN
	dbo.Customer AS c WITH (NOLOCK) ON c.ActorCustomerId = i.ActorId INNER JOIN
	dbo.VoucherSeries AS vs WITH (NOLOCK) ON o.VoucherSeriesId = vs.VoucherSeriesId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_bt WITH (NOLOCK) ON i.BillingType = st_bt.SysTermId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_stat WITH (NOLOCK) ON o.Status = st_stat.SysTermId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_fpo WITH (NOLOCK) ON ci.FixedPriceOrder = st_fpo.SysTermId INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId LEFT OUTER JOIN
	dbo.ContractGroup AS cg WITH (NOLOCK) ON ci.ContractGroupId = cg.ContractGroupId LEFT OUTER JOIN
	dbo.Project AS p WITH(NOLOCK) ON i.ProjectId = p.ProjectId OUTER APPLY
	(SELECT TOP 1 PayDate FROM dbo.PaymentRow WITH (NOLOCK) WHERE InvoiceId = i.InvoiceId ORDER BY PayDate DESC) pr
WHERE     
	(o.Type = @originType) AND
	(o.ActorCompanyId = @actorCompanyId) AND
	(st_stat.LangId = @langId) AND
	(st_bt.LangId = @langId) AND
	(st_fpo.LangId = @langId) AND
	(i.State = 0) AND 
	(st_bt.SysTermGroupId = 27) AND 
	(st_stat.SysTermGroupId = 30)
	AND (st_fpo.SysTermGroupId = 443)

END

GO

---$ Alter Procedure dbo.GetChangeStatusGridViewSI 
IF OBJECT_ID(N'dbo.GetChangeStatusGridViewSI') IS NULL
BEGIN
    PRINT 'Create procedure : dbo.GetChangeStatusGridViewSI'
    EXECUTE('CREATE PROCEDURE dbo.GetChangeStatusGridViewSI AS RETURN 0') 
END
GO

PRINT 'Alter procedure : dbo.GetChangeStatusGridViewSI'
GO

SET QUOTED_IDENTIFIER ON 
go

SET ANSI_NULLS ON 
go




ALTER PROCEDURE [dbo].[GetChangeStatusGridViewSI]
	@actorCompanyId INT,
	@langId INT,
	@originType INT
AS
BEGIN
	SET NOCOUNT ON;

/*
 ATTENTION:
 Changes should be made in both view [ChangeStatusGridViewSI] and procedure [GetChangeStatusGridViewSI]
 */

SELECT TOP 100 PERCENT 
    --Company
    ISNULL(o.ActorCompanyId, 0) AS OwnerActorId, 
    --Actor
    ISNULL(i.ActorId, 0) AS ActorId,
    s.SupplierNr AS ActorNr, 
    s.Name AS ActorName, 
    --Supplier
    s.BlockPayment AS SupplierBlockPayment, 
    --Customer
    --Origin
    o.OriginId, 
    o.Type AS OriginType, 
    o.Status, 
    st_stat.Name AS StatusName, 
    o.Description AS InternalText,       
    --Invoice
    i.InvoiceId, 
    i.Type AS InvoiceType, 
	i.IsTemplate as IsInvoiceTemplate,
    i.InvoiceNr, 
    i.SeqNr as InvoiceSeqNr, 
    i.BillingType AS BillingTypeId, 
    i.StatusIcon AS StatusIcon, 
	i.VatType as VatType,
    ISNULL(i.ProjectId, 0) AS ProjectId, 
    st_bt.Name AS BillingTypeName, 
    ISNULL(CAST(0 AS decimal(10, 2)), 0) AS RemainingAmount, 
    ISNULL(CAST(0 AS decimal(10, 2)), 0) AS RemainingAmountExVat, 
    --Supplier invoice
    si.BlockPayment, 
    si.MultipleDebtRows, 
    si.AttestStateId AS SupplierInvoiceAttestStateId,
    si.PaymentMethodId AS SupplierInvoicePaymentMethodId, 
    ISNULL(pm.PaymentType, 0) AS SupplierInvoicePaymentMethodType,
    --Payment method
    ISNULL(pm.SysPaymentMethodId, 0) AS SysPaymentMethodId,       
    ISNULL(pm.Name, '') AS PaymentMethodName, 
    --Common
    st_bt.LangId AS LangId, 
    s_cu.Code AS CurrencyCode, 
    i.CurrencyRate, 
    cu.SysCurrencyId, 
    vs.AccountYearId, 
    vs.VoucherSeriesId, 
    ISNULL(CAST(i.VoucherHeadId AS bit), 0) AS HasVoucher, 
    i.InvoiceDate, 
    i.VoucherDate, 
    i.DueDate, 
    ISNULL(pr.PayDate, i.DueDate) AS PayDate, 
    i.TotalAmount AS InvoiceTotalAmount, 
    i.TotalAmountCurrency AS InvoiceTotalAmountCurrency, 
    i.VATAmount, 
	i.VATAmountCurrency,
    i.PaidAmount AS InvoicePaidAmount, 
    i.PaidAmountCurrency AS InvoicePaidAmountCurrency, 
    ISNULL(i.TotalAmount - i.PaidAmount, 0) AS InvoicePayAmount, 
    ISNULL(i.TotalAmountCurrency - i.PaidAmountCurrency, 0) AS InvoicePayAmountCurrency, 
    i.FullyPayed,
    LEFT(ob.list,LEN(ob.list) - 1) AS PaymentStatuses,
    si.CurrentAttestUsers,
	i.PaymentNr
FROM         
    dbo.Origin AS o WITH (NOLOCK) INNER JOIN
    dbo.Invoice AS i WITH (NOLOCK) ON i.InvoiceId = o.OriginId INNER JOIN
    dbo.SupplierInvoice AS si WITH (NOLOCK) ON si.InvoiceId = i.InvoiceId INNER JOIN
    dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i.CurrencyId INNER JOIN
    dbo.Supplier AS s WITH (NOLOCK) ON s.ActorSupplierId = i.ActorId INNER JOIN
    dbo.VoucherSeries AS vs WITH (NOLOCK) ON o.VoucherSeriesId = vs.VoucherSeriesId INNER JOIN
    SoesysV2.dbo.SysTerm AS st_bt WITH (NOLOCK) ON i.BillingType = st_bt.SysTermId INNER JOIN
    SoesysV2.dbo.SysTerm AS st_stat WITH (NOLOCK) ON o.Status = st_stat.SysTermId LEFT OUTER JOIN
    dbo.PaymentMethod AS pm WITH (NOLOCK) ON pm.PaymentMethodId = si.PaymentMethodId INNER JOIN
    SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId OUTER APPLY
    (SELECT TOP 1 PayDate, PaymentNr FROM dbo.PaymentRow WITH (NOLOCK) WHERE InvoiceId = i.InvoiceId ORDER BY PayDate DESC) pr OUTER APPLY 
    (
      SELECT   CONVERT(VARCHAR(2),Status) + ',' AS [text()]
	  FROM     PaymentRow pr
	  WHERE    pr.InvoiceId = i.InvoiceId AND pr.State = 0
	  ORDER BY pr.PaymentRowId
	  FOR XML PATH('')
	 ) ob(list)          
WHERE     
	(o.Type = @originType) AND
	(o.ActorCompanyId = @actorCompanyId) AND
	(st_stat.LangId = @langId) AND
	(st_bt.LangId = @langId) AND
	(i.State = 0) AND 
	(st_bt.SysTermGroupId = 27) AND 
	(st_stat.SysTermGroupId = 30)

END

GO

---$ Alter Procedure dbo.GetTimeCodeTransactionsForProject 
IF OBJECT_ID(N'dbo.GetTimeCodeTransactionsForProject') IS NULL
BEGIN
    PRINT 'Create procedure : dbo.GetTimeCodeTransactionsForProject'
    EXECUTE('CREATE PROCEDURE dbo.GetTimeCodeTransactionsForProject AS RETURN 0') 
END
GO

PRINT 'Alter procedure : dbo.GetTimeCodeTransactionsForProject'
GO

SET QUOTED_IDENTIFIER ON 
go

SET ANSI_NULLS ON 
go





ALTER PROCEDURE [dbo].[GetTimeCodeTransactionsForProject]
	@projectId			INT,
	@actorCompanyId		INT
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT 
		--TimeCodeTransaction
		tct.TimeCodeTransactionId,
		tct.Start,
		tct.Stop,
		tct.Quantity,	
		--TimeCode
		tc.TimeCodeId,
		tc.Type as TimeCodeType,
		tc.RegistrationType as TimeCodeRegistrationType,
		tc.Code AS TimeCodeCode,
		tc.Name AS TimeCodeName,
		--TimeBlock
		tb.TimeBlockId,
		--TimeRule
		tr.TimeRuleId,
		tr.Name AS TimeRuleName,
		tr.Sort AS TimeRuleSort,
		tct.SupplierInvoiceId
	FROM 
		dbo.TimeCodeTransaction AS tct WITH (NOLOCK) INNER JOIN 
		dbo.TimeCode AS tc WITH (NOLOCK) ON tc.TimeCodeId = tct.TimeCodeId LEFT OUTER JOIN 
		dbo.TimeBlock AS tb WITH (NOLOCK) ON tb.timeBlockId = tct.timeBlockId LEFT OUTER JOIN 
		dbo.TimeBlockDate AS tbd WITH (NOLOCK) ON tbd.timeBlockDateId = tb.TimeBlockDateId LEFT OUTER JOIN
		dbo.TimeRule as tr WITH (NOLOCK) ON tr.TimeRuleId = tct.TimeRuleId
	WHERE 
		tct.ProjectId = @projectId AND 
		tct.[State] = 0			
	 
END

GO

---$ Alter Procedure dbo.GetTimePayrollTransactionsForEmployee 
IF OBJECT_ID(N'dbo.GetTimePayrollTransactionsForEmployee') IS NULL
BEGIN
    PRINT 'Create procedure : dbo.GetTimePayrollTransactionsForEmployee'
    EXECUTE('CREATE PROCEDURE dbo.GetTimePayrollTransactionsForEmployee AS RETURN 0') 
END
GO

PRINT 'Alter procedure : dbo.GetTimePayrollTransactionsForEmployee'
GO

SET QUOTED_IDENTIFIER ON 
go

SET ANSI_NULLS ON 
go





ALTER PROCEDURE [dbo].[GetTimePayrollTransactionsForEmployee]
	@employeeId INT,
	@startDate DateTime = NULL,
	@stopDate DateTime = NULL,
	@timePeriodId INT = NULL
WITH RECOMPILE
AS
BEGIN
	SET NOCOUNT ON;

SELECT 
	--TimePayrollTransaction
	tpt.TimePayrollTransactionId,
	tpt.EmployeeId,
	tpt.TimeCodeTransactionId,
	tpt.AccountId,
	tpt.UnitPrice,
	tpt.UnitPriceCurrency,
	tpt.UnitPriceEntCurrency,
	tpt.Amount as Amount,
	tpt.AmountCurrency AS AmountCurrency,
	tpt.AmountEntCurrency AS AmountEntCurrency,
	tpt.VatAmount as VatAmount,
	tpt.VatAmountCurrency AS VatAmountCurrency,
	tpt.VatAmountEntCurrency AS VatAmountEntCurrency,
	tpt.Quantity,
	tpt.ManuallyAdded,
	ISNULL(tpt.Comment,'') AS TransactionComment,
	tpt.IsPreliminary,
	tpt.ReversedDate,
	tpt.IsReversed,
	tpt.IsAdded,
	tpt.AddedDateFrom,
	tpt.AddedDateTo,
	tpt.IsFixed,
	tpt.IsAdditionOrDeduction,
	tpt.IsSpecifiedUnitPrice,
	tpt.IsCentRounding,
	tpt.IsQuantityRounding,
	tpt.SysPayrollTypeLevel1 as TransactionSysPayrollTypeLevel1,
	tpt.SysPayrollTypeLevel2 as TransactionSysPayrollTypeLevel2,
	tpt.SysPayrollTypeLevel3 as TransactionSysPayrollTypeLevel3,
	tpt.SysPayrollTypeLevel4 as TransactionSysPayrollTypeLevel4,
	tpt.Created,
	tpt.CreatedBy,
	--TimePayrollTransactionExtended
	tpte.PayrollPriceFormulaId,
	tpte.PayrollPriceTypeId,
	tpte.Formula,
	tpte.FormulaPlain,
	tpte.FormulaExtracted,
	tpte.FormulaNames,
	tpte.FormulaOrigin,
	tpte.PayrollCalculationPerformed,
	--TimeBlockDate
	tbd.TimeBlockDateId,
	tbd.Date,
	--TimeBlock
	tb.TimeBlockId,
	tb.TimeScheduleTemplatePeriodId,
	(CASE WHEN ISNULL(tct.TimeCodeTransactionId, 0) > 0 THEN tct.Start ELSE tb.StartTime END) AS StartTime, 
	(CASE WHEN ISNULL(tct.TimeCodeTransactionId, 0) > 0 THEN tct.Stop ELSE tb.StopTime END) AS StopTime, 
	ISNULL(tb.Comment,'') AS DeviationComment,
	--AttestState
	ast.AttestStateId,
	ast.Name as AttestStateName,
	ast.Color as AttestStateColor,
	ast.Initial as AttestStateInitial,
	--PayrollProduct
	pp.ProductId,
	pp.ShortName as ProductShortName,
	pp.Factor as PayrollProductFactor,
	pp.Payed as PayrollProductPayed,
	pp.Export as PayrollProductExport,
	pp.SysPayrollTypeLevel1 as PayrollProductSysPayrollTypeLevel1,
	pp.SysPayrollTypeLevel2 as PayrollProductSysPayrollTypeLevel2,
	pp.SysPayrollTypeLevel3 as PayrollProductSysPayrollTypeLevel3,
	pp.SysPayrollTypeLevel4 as PayrollProductSysPayrollTypeLevel4,
	--Product
	p.Number as ProductNumber,
	p.Name as ProductName,
	--TimeCode
	(select	count(tc2.TimeCodeId) from TimeCode as tc2 inner join TimeCodeWork as tcw on tc.TimeCodeId = tcw.TimeCodeWorkId where tc.TimeCodeId = tc2.TimeCodeId AND tc.Type = 1 AND tcw.IsWorkOutsideSchedule = 1) AS NoOfPresenceWorkOutsideScheduleTime,
	(select	count(tc2.TimeCodeId) from TimeCode as tc2 inner join TimeCodeAbsense as tca on tc.TimeCodeId = tca.TimeCodeAbsenseId where tc.TimeCodeId = tc2.TimeCodeId AND tc.Type = 2 AND tca.IsAbsence = 1) AS NoOfAbsenceAbsenceTime,
	ISNULL(tc.RegistrationType,1) AS TimeCodeRegistrationType,
	ISNULL(tc.Type,0) AS TimeCodeType,
	--TimePeriod
	tpt.TimePeriodId,
	tp.Name as TimePeriodName
FROM	
	TimePayrollTransaction as tpt WITH (NOLOCK) INNER JOIN
	TimeBlockDate as tbd WITH (NOLOCK) on tbd.TimeBlockDateId = tpt.TimeBlockDateId INNER JOIN
	AttestState as ast WITH (NOLOCK) ON ast.AttestStateId = tpt.AttestStateId INNER JOIN
	PayrollProduct as pp WITH (NOLOCK) ON pp.ProductId = tpt.PayrollProductId INNER JOIN
	Product as p WITH (NOLOCK) ON p.ProductId = pp.ProductId LEFT OUTER JOIN
	TimeBlock as tb WITH (NOLOCK) ON tb.TimeBlockId = tpt.TimeBlockId LEFT OUTER JOIN	
	TimeCodeTransaction as tct WITH (NOLOCK) on tct.TimeCodeTransactionId = tpt.TimeCodeTransactionId LEFT OUTER JOIN
	TimeCode as tc WITH (NOLOCK) ON tc.TimeCodeId = tct.TimeCodeId LEFT OUTER JOIN
	TimePeriod as tp WITH (NOLOCK) ON tp.TimePeriodId = tpt.TimePeriodId LEFT OUTER JOIN
	TimePayrollTransactionExtended as tpte on tpte.TimePayrollTransactionId = tpt.TimePayrollTransactionId
WHERE
	(tbd.EmployeeId = @employeeId) AND 
	(tpt.State = 0) AND
	(tb.State IS NULL OR tb.State = 0) AND
	(
	  (@timePeriodId IS NOT NULL AND @timePeriodId = tpt.TimePeriodId) 
	  OR
	  ((tpt.TimePeriodId IS NULL OR @timePeriodId IS NULL) AND (@startDate IS NOT NULL AND @stopDate IS NOT NULL AND tbd.Date BETWEEN @startDate AND @stopDate))
	)
ORDER BY
	tbd.Date, tb.StartTime


END

		/*
		Case	Gui			Periodselector				Rule										Parameters
		1		Attest		For day/week/month/period	Never consider TimePeriodId, only dates		(@timePeriodId is null)
		2		Payroll		For day/week/month			Never consider TimePeriodId, only dates		(@timePeriodId is null)
		3		Payroll		For extra-period			Never consider dates, only TimePeriodId		(@startDate and @stopDate is null)
		4		Payroll		For regular-period			Consider both TimePeriodId and dates
														Can match either TimePeriodId or dates
														
		*/

GO

---$ Alter Procedure dbo.GetTimePayrollTransactionsForEmployees 
IF OBJECT_ID(N'dbo.GetTimePayrollTransactionsForEmployees') IS NULL
BEGIN
    PRINT 'Create procedure : dbo.GetTimePayrollTransactionsForEmployees'
    EXECUTE('CREATE PROCEDURE dbo.GetTimePayrollTransactionsForEmployees AS RETURN 0') 
END
GO

PRINT 'Alter procedure : dbo.GetTimePayrollTransactionsForEmployees'
GO

SET QUOTED_IDENTIFIER ON 
go

SET ANSI_NULLS ON 
go










ALTER PROCEDURE [dbo].[GetTimePayrollTransactionsForEmployees]
	@actorCompanyId int,					-- Mandatory
	@dateFrom datetime,						-- Mandatory
	@dateTo datetime,						-- Mandatory
	@employeeIds nvarchar(max),				-- Null or comma separated string
	@preliminary bit						-- Null, 0 or 1 (Null = condition not used, 0 = no preliminary, 1 = only preliminary)


WITH RECOMPILE

AS
BEGIN
	SET NOCOUNT ON;
	 Select
		 tct.TimeCodeId as TimeCodeId,
		 tpt.State as TimePayrollTransactionState,
		 tct.State as TimeCodeTransactionState,
		 p.ProductId as ProductId,
		 tpt.TimePayrollTransactionId as TimePayrollTransactionId,
		 tpt.TimeCodeTransactionId as TimeCodeTransactionId,
		 tbd.Date as [Date],
		 p.Number as PayrollProductNumber,
		 p.Name as PayrollProductName,
		 tpt.Quantity as PayrollProductMinutes,
		 pp.Factor as PayrollProductFactor,
		 pp.PayrollType as PayrollProductType,
		 pp.SysPayrollTypeLevel1 as PayrollProductTypeLevel1,
		 pp.SysPayrollTypeLevel2 as PayrollProductTypeLevel2,
		 pp.SysPayrollTypeLevel3 as PayrollProductTypeLevel3,
		 pp.SysPayrollTypeLevel4 as PayrollProductTypeLevel4,
		 tpt.UnitPrice as UnitPrice,
		 tpt.UnitPriceCurrency as UnitPriceCurrency,
		 tpt.UnitPriceEntCurrency UnitPriceEntCurrency,
		 tpt.Amount as Amount,
		 tpt.AmountCurrency as AmountCurrency,
		 tpt.AmountEntCurrency as AmountEntCurrency,
		 tpt.VatAmount as Vatamount,
		 tpt.VatAmountCurrency as VatAmountCurrency,
		 tpt.VatAmountEntCurrency as VatAmountEntCurrency,
		 tpt.Quantity as Quantity,
		 tpt.SysPayrollTypeLevel1 as PayrollTypeLevel1,
		 tpt.SysPayrollTypeLevel2 as PayrollTypeLevel2,
		 tpt.SysPayrollTypeLevel3 as PayrollTypeLevel3,
		 tpt.SysPayrollTypeLevel4 as PayrollTypeLevel4,
		 pp.Payed as PayedTime,
		 tpte.Formula as Formula,
		 tpte.FormulaExtracted as FormulaExtracted,
		 tpte.FormulaNames as FormulaNames,
		 tpte.FormulaOrigin as FormulaOrigin,
		 tpte.FormulaPlain as FormulaPlain,
		 tb.Comment as Note,
		 a.AccountNr as AccountNr,
		 a.Name as AccountName,
		 ad.AccountDimNr as AccountDimNr,
		 ad.name as AccountDimName,
		 a2.AccountNr as AccountInternalNr,
		 a2.Name as AccountInternalName,
		 tpt.EmployeeId,
		 tpt.TimeBlockDateId,
		 ast.Name as AttestStateName

		 from 
		 TimePayrollTransaction tpt inner join
		 product p on tpt.PayrollProductId = p.ProductId inner join
		 PayrollProduct pp on pp.ProductId  = p.ProductId inner join
		 TimeCodeTransaction tct on tct.TimeCodeTransactionId= tpt.TimeCodeTransactionId inner join
		 Account a on a.AccountId=tpt.AccountId inner join
		 AttestState ast on ast.AttestStateId = tpt.AttestStateId inner join
		 timeblockdate tbd on tbd.TimeBlockDateId = tpt.TimeBlockDateId left outer join
		 timeblock tb on tb.TimeBlockId = tpt.TimeBlockId left outer join
		 TimePayrollTransactionExtended tpte on tpte.TimePayrollTransactionId = tpt.TimePayrollTransactionId left outer join 
		 TimePayrollTransactionAccount tpta on tpta.TimePayrollTransactionId = tpt.TimePayrollTransactionId left outer join
		 account a2 on a2.AccountId=tpta.AccountId left outer join
		 accountDim ad on ad.AccountDimId = a.AccountDimId

         where
		 tpt.ActorCompanyId = @actorCompanyId AND
		 tbd.Date between @dateFrom and @dateTo AND
		 tpt.EmployeeId IN (SELECT ID FROM Split(@employeeIds, ',')) AND
		 tpt.state = 0
		 
		
		 ORDER BY 
		 tpt.EmployeeId

END


SET ANSI_NULLS ON

GO

---$ Alter Procedure dbo.GetTimePayrollTransactionsWithAccIntsForEmployee 
IF OBJECT_ID(N'dbo.GetTimePayrollTransactionsWithAccIntsForEmployee') IS NULL
BEGIN
    PRINT 'Create procedure : dbo.GetTimePayrollTransactionsWithAccIntsForEmployee'
    EXECUTE('CREATE PROCEDURE dbo.GetTimePayrollTransactionsWithAccIntsForEmployee AS RETURN 0') 
END
GO

PRINT 'Alter procedure : dbo.GetTimePayrollTransactionsWithAccIntsForEmployee'
GO

SET QUOTED_IDENTIFIER ON 
go

SET ANSI_NULLS ON 
go






ALTER PROCEDURE [dbo].[GetTimePayrollTransactionsWithAccIntsForEmployee]
	@employeeId						INT,
	@actorCompanyId					INT,
	@dateFrom						DATETIME,
	@dateTo							DATETIME,
	@timeScheduleTemplatePeriodId	INT
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT 
		--TimePayrollTransaction
		tpt.TimePayrollTransactionId,
		tpt.Quantity,
		tpt.Amount,
		tpt.AmountCurrency,
		tpt.AmountLedgerCurrency,
		tpt.AmountEntCurrency,
		tpt.VatAmount,
		tpt.VatAmountCurrency,
		tpt.VatAmountLedgerCurrency,
		tpt.VatAmountEntCurrency,
		ISNULL(tpt.Comment,'') AS TransactionComment,
		tpt.IsPreliminary,
		tpt.ManuallyAdded,
		tpt.IsAdded,
		tpt.IsFixed,
		tpt.Exported,
		tpt.IsReversed,
		tpt.ReversedDate,
		tpt.SysPayrollTypeLevel1 as TransactionSysPayrollTypeLevel1,
		tpt.SysPayrollTypeLevel2 as TransactionSysPayrollTypeLevel2,
		tpt.SysPayrollTypeLevel3 as TransactionSysPayrollTypeLevel3,
		tpt.SysPayrollTypeLevel4 as TransactionSysPayrollTypeLevel4,
		tpt.IsCentRounding,
		tpt.IsQuantityRounding,
		--TimeCodeTransaction
		tct.TimeCodeTransactionId,
		--Employee
		tpt.EmployeeId,
		--EmployeeChild
		ec.EmployeeChildId as ChildId,
		ec.FirstName as ChildFirstName,
		ec.LastName as ChildLastName,
		ec.BirthDate as ChildBirthDate,
		--Product
		p.ProductId,
		p.Number as ProductNumber,
		p.Name as ProductName,
		--PayrollProduct
		pp.Export as PayrollProductExport,
		pp.SysPayrollTypeLevel1 as PayrollProductSysPayrollTypeLevel1,
		pp.SysPayrollTypeLevel2 as PayrollProductSysPayrollTypeLevel2,
		pp.SysPayrollTypeLevel3 as PayrollProductSysPayrollTypeLevel3,
		pp.SysPayrollTypeLevel4 as PayrollProductSysPayrollTypeLevel4,
		--TimeCode
		tc.TimeCodeId,
		tc.Code as TimeCodeCode,
		tc.Name as TimeCodeName,
		tc.Type as TimeCodeType,
		tc.RegistrationType as TimeCodeRegistrationType,
		--TimeBlock
		ISNULL(tb.TimeBlockId,0) as TimeBlockId,
		ISNULL(tb.Comment,'') as DeviationComment,	
		--TimeBlockDate
		tb.TimeBlockDateId,
		tbd.Date,	
		--AttestState
		ats.AttestStateId,
		ats.Name as AttestStateName,
		--Accounting
		acc.AccountId AS AccountStdId,
		acc.AccountNr AS AccountStdNr,
		acc.Name AS AccountStdName,
		LEFT(o.list,LEN(o.list) - 1) AS AccountInternalsStr
	FROM 
		dbo.TimePayrollTransaction AS tpt WITH (NOLOCK)INNER JOIN 
		dbo.Product AS p WITH (NOLOCK) ON p.ProductId = tpt.PayrollProductId INNER JOIN 
		dbo.PayrollProduct AS pp WITH (NOLOCK) ON pp.ProductId = p.ProductId INNER JOIN 
		dbo.Account AS acc WITH (NOLOCK) ON acc.AccountId = tpt.AccountId INNER JOIN 
		dbo.TimeBlockDate AS tbd WITH (NOLOCK) ON tbd.TimeBlockDateId = tpt.TimeBlockDateId INNER JOIN 
		dbo.AttestState AS ats WITH (NOLOCK) ON ats.AttestStateId = tpt.AttestStateId LEFT OUTER JOIN 
		dbo.TimeCodeTransaction AS tct WITH (NOLOCK) on tct.TimeCodeTransactionId = tpt.TimeCodeTransactionId LEFT OUTER JOIN
		dbo.TimeCode AS tc WITH (NOLOCK) on tc.TimeCodeId = tct.TimeCodeId LEFT OUTER JOIN
		dbo.EmployeeChild as ec  WITH (NOLOCK) on ec.EmployeeChildId = tpt.EmployeeChildId LEFT OUTER JOIN
		dbo.TimeBlock AS tb WITH (NOLOCK) ON tb.TimeBlockId = tpt.TimeBlockId CROSS APPLY 
		(
			SELECT   
				CONVERT(VARCHAR(100),tptad.AccountDimNr) + ':' + CONVERT(VARCHAR(100),tacc.AccountId) + ':' + CONVERT(VARCHAR(100),tacc.AccountNr) + ':' + CONVERT(VARCHAR(100),tacc.Name) + ',' AS [text()]
			FROM
				dbo.TimePayrollTransactionAccount AS ta WITH (NOLOCK) INNER JOIN 
				dbo.Account AS tacc WITH (NOLOCK) ON tacc.AccountId = ta.AccountId INNER JOIN 
				dbo.AccountDim AS tptad WITH (NOLOCK) ON tptad.AccountDimId = tacc.AccountDimId
			WHERE    
				ta.TimePayrollTransactionId = tpt.TimePayrollTransactionId and tptad.AccountDimNr > 1
			ORDER BY
				tptad.AccountDimNr
			FOR XML PATH('')
		 ) o(list)	
	WHERE 
		tbd.EmployeeId = @employeeId AND 
		(tbd.Date BETWEEN @dateFrom AND @dateTo) AND
		tpt.[State] = 0	AND
		ISNULL(tb.State, 0) = 0
	ORDER BY
		tb.StartTime


    END

GO

---$ Alter Procedure dbo.GetTimeSchedulePlanningShifts 
IF OBJECT_ID(N'dbo.GetTimeSchedulePlanningShifts') IS NULL
BEGIN
    PRINT 'Create procedure : dbo.GetTimeSchedulePlanningShifts'
    EXECUTE('CREATE PROCEDURE dbo.GetTimeSchedulePlanningShifts AS RETURN 0') 
END
GO

PRINT 'Alter procedure : dbo.GetTimeSchedulePlanningShifts'
GO

SET QUOTED_IDENTIFIER ON 
go

SET ANSI_NULLS ON 
go





ALTER PROCEDURE [dbo].[GetTimeSchedulePlanningShifts]
	@actorCompanyId int,					-- Mandatory
	@dateFrom datetime,						-- Mandatory
	@dateTo datetime,						-- Mandatory
	@employeeIds nvarchar(max),				-- Null or comma separated string
	@shiftTypeIds nvarchar(max),			-- Null or comma separated string
	@preliminary bit,						-- Null, 0 or 1 (Null = condition not used, 0 = no preliminary, 1 = only preliminary)
	@loadBreaks bit,						-- 0 or 1 (0 = do not load break blocks, 1 = load break blocks)
	@shiftStatus int,						-- Null or a shift status
	@employeeIdInQueue int,					-- Null or employee ID of employee in queue (condition)
	@currentEmployeeId int					-- Null or employee ID of employee in queue (just to set IamInQueue)

WITH RECOMPILE

AS
BEGIN
	SET NOCOUNT ON;

-- DISTINCT needed when multiple employees in queue
  SELECT DISTINCT
        b.TimeScheduleTemplateBlockId,
		ISNULL(p.TimeScheduleTemplateHeadId, 0) AS TimeScheduleTemplateHeadId,
        b.TimeScheduleTemplatePeriodId,
        b.TimeScheduleEmployeePeriodId,
        b.TimeCodeId,
        b.EmployeeId,
        b.ShiftTypeId,
        b.StartTime,
        b.StopTime,
        b.Date,
        b.Description,
        b.ShiftStatus,
        b.ShiftUserStatus,
        b.NbrOfWantedInQueue,
        b.TimeDeviationCauseId,
        b.BreakType,
        b.Link,
        b.TimeScheduleTypeId,
		ISNULL(tst.Code, '') AS TimeScheduleTypeCode,
		ISNULL(tst.Name, '') AS TimeScheduleTypeName,
		ISNULL(tst.IsNotScheduleTime, 0) AS TimeScheduleTypeIsNotScheduleTime,
        b.CustomerInvoiceId,
        b.Type,
		CASE WHEN b.Type = 0 THEN 1 ELSE 0 END AS IsSchedule,
		e.UserId,
        e.Hidden,
        e.Vacant,
        ISNULL(ep.IsPreliminary, 0) AS IsPreliminary,
        cp.FirstName + ' ' + cp.LastName AS EmployeeName,
		e.EmployeeNr + '. ' + cp.FirstName + ' ' + cp.LastName AS EmployeeInfo,
        st.Name AS ShiftTypeName,
        st.Description AS ShiftTypeDescription,
        st.Color AS ShiftTypeColor,
        tdc.Name AS TimeDeviationCauseName,
		(SELECT per.DayNumber FROM TimeScheduleTemplatePeriod per WHERE b.TimeScheduleTemplatePeriodId = per.TimeScheduleTemplatePeriodId) AS DayNumber,
		ISNULL(CAST(h.NoOfDays / 7 AS int), 0) AS TemplateNbrOfWeeks,
		ISNULL(CASE WHEN EXISTS (SELECT * FROM TimeScheduleTemplateBlockQueue subq WHERE @currentEmployeeId IS NOT NULL AND @currentEmployeeId <> 0 AND subq.TimeScheduleTemplateBlockId = b.TimeScheduleTemplateBlockId AND subq.Type = 1 AND subq.EmployeeId = @currentEmployeeId) THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END, 0) AS IamInQueue,
        b.StaffingNeedsRowId,
		ISNULL(CASE WHEN snr.Name IS NULL OR snh.Name IS NULL THEN '' ELSE snh.Name + ' - ' + snr.Name END, '') AS StaffingNeedsOrigin,
		snh.Weekday AS StaffingNeedsWeekday,
		snh.DayTypeId AS StaffingNeedsDayTypeId,
		snh.Date AS StaffingNeedsDate
        FROM TimeScheduleTemplateBlock AS b
        INNER JOIN Employee AS e ON b.EmployeeId = e.EmployeeId
        INNER JOIN ContactPerson AS cp ON e.ContactPersonId = cp.ActorContactPersonId
		INNER JOIN TimeScheduleTemplatePeriod AS p ON b.TimeScheduleTemplatePeriodId = p.TimeScheduleTemplatePeriodId
		INNER JOIN TimeScheduleTemplateHead AS h ON p.TimeScheduleTemplateHeadId = h.TimeScheduleTemplateHeadId
        INNER JOIN TimeScheduleEmployeePeriod AS ep ON b.TimeScheduleEmployeePeriodId = ep.TimeScheduleEmployeePeriodId
        LEFT OUTER JOIN ShiftType AS st ON b.ShiftTypeId = st.ShiftTypeId
		LEFT OUTER JOIN TimeScheduleType tst on b.TimeScheduleTypeId = tst.TimeScheduleTypeId
        LEFT OUTER JOIN TimeDeviationCause AS tdc ON b.TimeDeviationCauseId = tdc.TimeDeviationCauseId
		LEFT OUTER JOIN TimeScheduleTemplateBlockQueue AS q ON b.TimeScheduleTemplateBlockId = q.TimeScheduleTemplateBlockId
		LEFT OUTER JOIN StaffingNeedsRow snr on b.StaffingNeedsRowId = snr.StaffingNeedsRowId
		LEFT OUTER JOIN StaffingNeedsHead snh on snr.StaffingNeedsHeadId = snh.StaffingNeedsHeadId
        WHERE b.State = 0 AND
		 (@employeeIds IS NULL OR b.EmployeeId IN (SELECT * FROM SplitDelimiterString(@employeeIds, ','))) AND
		 (@shiftTypeIds IS NULL OR b.ShiftTypeId IS NULL OR (b.ShiftTypeId IN (SELECT * FROM SplitDelimiterString(@shiftTypeIds, ',')))) AND
		 (b.Date IS NOT NULL AND b.Date <= @dateTo AND (DATEADD(day, DATEDIFF(day, b.StartTime, b.StopTime), b.Date) >= @dateFrom)) AND
		 ((@loadBreaks = 0 AND b.BreakType = 0) OR (@loadBreaks = 1)) AND
		 (((b.TimeScheduleEmployeePeriodId IS NOT NULL) AND (b.StartTime <> b.StopTime) AND (b.TimeDeviationCauseId IS NULL)) OR (b.TimeDeviationCauseId IS NOT NULL)) AND
		 (@preliminary IS NULL OR (ep.EmployeeId = b.EmployeeId AND ep.IsPreliminary = @preliminary)) AND
		 (@shiftStatus IS NULL OR (b.ShiftStatus = @shiftStatus)) AND
		 (@employeeIdInQueue IS NULL OR (q.Type = 1 AND q.EmployeeId = @employeeIdInQueue)) AND
		 e.ActorCompanyId = @actorCompanyId AND
		 e.State = 0
		ORDER BY b.Date, e.Hidden DESC, e.Vacant DESC, IsSchedule DESC, b.StartTime, b.StopTime, st.Name

END


SET ANSI_NULLS ON

GO




/*
   den 10 mars 201514:34:43
   User: dba
   Server: extra02\release
   Database: SOECompv2
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
ALTER TABLE dbo.Employee ADD
	ExternalCode nvarchar(100) NULL
GO
ALTER TABLE dbo.Employee SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.Employee', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Employee', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Employee', 'Object', 'CONTROL') as Contr_Per 


