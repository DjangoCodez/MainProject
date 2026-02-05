USE [SOECompv2]
GO

/****** Object:  Table [dbo].[CustomerInvoicePrintedReminder]    Script Date: 2014-12-10 12:22:45 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CustomerInvoicePrintedReminder](
	[CustomerInvoicePrintedReminderId] [int] IDENTITY(1,1) NOT NULL,
	[ActorCompanyId] [int] NOT NULL,
	[CustomerInvoiceOriginId] [int] NOT NULL,
	[InvoiceNr] [nvarchar](50) NOT NULL,
	[Amount] [decimal](10, 2) NOT NULL,
	[ReminderDate] [datetime] NOT NULL,
	[DueDate] [datetime] NOT NULL,
	[NoOfReminder] [int] NOT NULL,
	[Created] [datetime] NULL,
	[CreatedBy] [nvarchar](50) NULL,
 CONSTRAINT [PK_CustomerInvoicePrintedReminder] PRIMARY KEY CLUSTERED 
(
	[CustomerInvoicePrintedReminderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[CustomerInvoicePrintedReminder] ADD  DEFAULT ((0)) FOR [Amount]
GO

ALTER TABLE [dbo].[CustomerInvoicePrintedReminder] ADD  DEFAULT ((1)) FOR [NoOfReminder]
GO

ALTER TABLE [dbo].[CustomerInvoicePrintedReminder]  WITH CHECK ADD  CONSTRAINT [FK_CustomerInvoicePrintedReminder_Company] FOREIGN KEY([ActorCompanyId])
REFERENCES [dbo].[Company] ([ActorCompanyId])
GO

ALTER TABLE [dbo].[CustomerInvoicePrintedReminder] CHECK CONSTRAINT [FK_CustomerInvoicePrintedReminder_Company]
GO

/* Adding better edi description inside order and invoice */
/****** Object:  View [dbo].[OrderTraceView]    Script Date: 2014-10-29 12:49:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

    ALTER VIEW [dbo].[OrderTraceView]
    AS
    /*Contract*/ SELECT TOP 100 PERCENT /*Order*/ i_ord.InvoiceId AS OrderId, /*Contract*/ ISNULL(CAST(1 AS bit), 1) AS IsContract, o_con.OriginId AS ContractId,
    /*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, /*Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS InvoiceId, /*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi,
    0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, /*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, /*SupplierInvoice*/ ISNULL(CAST(0 AS bit), 0) AS IsSupplierInvoice,
    0 AS SupplierInvoiceId,  /*Origin*/ o_con.Description,
    o_con.Type AS OriginType, st_oType.Name AS OriginTypeName, o_con.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, /*Common*/ i_con.BillingType,
    st_iBillingType.Name AS BillingTypeName, i_con.InvoiceNr AS Number, ISNULL(CAST(i_con.TotalAmount AS decimal(10, 2)), 0) AS Amount,
    ISNULL(CAST(i_con.TotalAmountCurrency AS decimal(10, 2)), 0) AS AmountCurrency, i_con.VATAmount AS VatAmount, i_con.VATAmountCurrency AS VatAmountCurrency,
    i_con.InvoiceDate AS Date, i_con.State, /*Currency*/ s_cu.Code AS CurrencyCode, i_ord.CurrencyRate, cu.SysCurrencyId, /*Lang*/ st_oType.LangId AS LangId
    FROM         dbo.Invoice AS i_ord WITH (NOLOCK) INNER JOIN
    dbo.Origin AS o_ord WITH (NOLOCK) ON o_ord.OriginId = i_ord.InvoiceId INNER JOIN
    dbo.OriginInvoiceMapping AS orm WITH (NOLOCK) ON orm.InvoiceId = i_ord.InvoiceId INNER JOIN
    dbo.Origin AS o_con WITH (NOLOCK) ON o_con.OriginId = orm.OriginId INNER JOIN
    dbo.Invoice AS i_con WITH (NOLOCK) ON i_con.InvoiceId = o_con.OriginId INNER JOIN
    dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_ord.CurrencyId INNER JOIN
    SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o_con.Type INNER JOIN
    SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o_con.Status INNER JOIN
    SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i_con.BillingType INNER JOIN
    SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
    WHERE     i_ord.State = 0 AND o_ord.Type = 6 AND i_con.Type = 7 AND st_oStatus.SysTermGroupId = 30 AND st_oType.SysTermGroupId = 31 AND
    st_iBillingType.SysTermGroupId = 27 AND st_oStatus.LangId = st_oType.LangId AND st_iBillingType.LangId = st_oType.LangId
    UNION
    /*Offer*/ SELECT TOP 100 PERCENT /*Order*/ i_ord.InvoiceId AS OrderId, /*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, /*Offer*/ ISNULL(CAST(1 AS bit), 1)
    AS IsOffer, orm.OriginId AS OfferId, /*Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS InvoiceId, /*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId,
    ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, /*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId,  /*SupplierInvoice*/ ISNULL(CAST(0 AS bit), 0) AS IsSupplierInvoice,
    0 AS SupplierInvoiceId, /*Origin*/ o_off.Description, o_off.Type AS OriginType,
    st_oType.Name AS OriginTypeName, o_off.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, /*Common*/ i_off.BillingType,
    st_iBillingType.Name AS BillingTypeName, i_off.InvoiceNr AS Number, ISNULL(CAST(i_off.TotalAmount AS decimal(10, 2)), 0) AS Amount,
    ISNULL(CAST(i_off.TotalAmountCurrency AS decimal(10, 2)), 0) AS AmountCurrency, i_off.VATAmount AS VatAmount, i_off.VATAmountCurrency AS VatAmountCurrency,
    i_off.InvoiceDate AS Date, i_off.State, /*Currency*/ s_cu.Code AS CurrencyCode, i_off.CurrencyRate, cu.SysCurrencyId, /*Lang*/ st_oType.LangId AS LangId
    FROM         dbo.Invoice AS i_ord WITH (NOLOCK) INNER JOIN
    dbo.Origin AS o_ord WITH (NOLOCK) ON o_ord.OriginId = i_ord.InvoiceId INNER JOIN
    dbo.OriginInvoiceMapping AS orm WITH (NOLOCK) ON orm.InvoiceId = o_ord.OriginId INNER JOIN
    dbo.Origin AS o_off WITH (NOLOCK) ON o_off.OriginId = orm.OriginId INNER JOIN
    dbo.Invoice AS i_off WITH (NOLOCK) ON i_off.InvoiceId = o_off.OriginId INNER JOIN
    dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_ord.CurrencyId INNER JOIN
    SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o_off.Type INNER JOIN
    SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o_off.Status INNER JOIN
    SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i_off.BillingType INNER JOIN
    SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
    WHERE     i_ord.State = 0 AND o_ord.Type = 6 AND o_off.Type = 5 AND st_oStatus.SysTermGroupId = 30 AND st_oType.SysTermGroupId = 31 AND
    st_iBillingType.SysTermGroupId = 27 AND st_oStatus.LangId = st_oType.LangId AND st_iBillingType.LangId = st_oType.LangId
    UNION
    /*Invoice*/ SELECT TOP 100 PERCENT /*Order*/ i_ord.InvoiceId AS OrderId, /*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, /*Offer*/ ISNULL(CAST(0 AS bit),
    0) AS IsOffer, 0 AS OfferId, /*Invoice*/ ISNULL(CAST(1 AS bit), 1) AS IsInvoice, orm.InvoiceId AS InvoiceId, /*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId,
    ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, /*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId,  /*SupplierInvoice*/ ISNULL(CAST(0 AS bit), 0) AS IsSupplierInvoice,
    0 AS SupplierInvoiceId, /*Origin*/ o_inv.Description, o_inv.Type AS OriginType,
    st_oType.Name AS OriginTypeName, o_inv.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, /*Common*/ i_inv.BillingType,
    st_iBillingType.Name AS BillingTypeName, i_inv.InvoiceNr AS Number, ISNULL(CAST(i_inv.TotalAmount AS decimal(10, 2)), 0) AS Amount,
    ISNULL(CAST(i_inv.TotalAmountCurrency AS decimal(10, 2)), 0) AS AmountCurrency, i_inv.VATAmount AS VatAmount, i_inv.VATAmountCurrency AS VatAmountCurrency,
    i_inv.InvoiceDate AS Date, i_inv.State, /*Currency*/ s_cu.Code AS CurrencyCode, i_ord.CurrencyRate, cu.SysCurrencyId, /*Lang*/ st_oType.LangId AS LangId
    FROM         dbo.Invoice AS i_ord WITH (NOLOCK) INNER JOIN
    dbo.Origin AS o_ord WITH (NOLOCK) ON o_ord.OriginId = i_ord.InvoiceId INNER JOIN
    dbo.OriginInvoiceMapping AS orm WITH (NOLOCK) ON orm.OriginId = o_ord.OriginId INNER JOIN
    dbo.Origin AS o_inv WITH (NOLOCK) ON o_inv.OriginId = orm.InvoiceId INNER JOIN
    dbo.Invoice AS i_inv WITH (NOLOCK) ON i_inv.InvoiceId = o_inv.OriginId INNER JOIN
    dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_ord.CurrencyId INNER JOIN
    SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o_inv.Type INNER JOIN
    SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o_inv.Status INNER JOIN
    SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i_inv.BillingType INNER JOIN
    SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
    WHERE     i_ord.State = 0 AND o_ord.Type = 6 AND o_inv.Type = 2 AND st_oStatus.SysTermGroupId = 30 AND st_oType.SysTermGroupId = 31 AND
    st_iBillingType.SysTermGroupId = 27 AND st_oStatus.LangId = st_oType.LangId AND st_iBillingType.LangId = st_oType.LangId
    UNION
    /*EDI*/ SELECT TOP 100 PERCENT /*Order*/ i_ord.InvoiceId AS OrderId, /*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, /*Offer*/ ISNULL(CAST(0 AS bit), 0)
    AS IsOffer, 0 AS OfferId, /*Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS InvoiceId, /*Edi*/ ISNULL(CAST(1 AS bit), 1) AS IsEdi, ISNULL(edi_ord.EdiEntryId, 0)
    AS EdiEntryId, edi_ord.HasPdf AS EdiHasPdf, /*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId,  /*SupplierInvoice*/ ISNULL(CAST(0 AS bit), 0) AS IsSupplierInvoice,
    0 AS SupplierInvoiceId, (ISNULL(edi_ord.WholesellerName, '') + ' ' + ISNULL(edi_ord.SellerOrderNr, '')) AS Description, 0 AS OriginType,
    'EDI' AS OriginTypeName, edi_ord.OrderStatus AS OriginStatus, edi_ord.OrderStatusName AS OriginStatusName, /*Common*/ 0 AS BillingType, '' AS BillingTypeName,
    edi_ord.OrderNr AS Number, edi_ord.Sum AS Amount, edi_ord.SumCurrency AS AmountCurrency, edi_ord.SumVat AS VatAmount,
    edi_ord.SumVatCurrency AS VatAmountCurrency, ISNULL(edi_ord.Date, i_ord.InvoiceDate) AS Date, edi_ord.State, /*Currency*/ edi_ord.CurrencyCode,
    edi_ord.CurrencyRate, edi_ord.SysCurrencyId, /*Lang*/ edi_ord.LangId
    FROM         dbo.Invoice AS i_ord WITH (NOLOCK) INNER JOIN
    dbo.Origin AS o_ord WITH (NOLOCK) ON o_ord.OriginId = i_ord.InvoiceId INNER JOIN
    dbo.EdiEntryView AS edi_ord WITH (NOLOCK) ON edi_ord.OrderId = i_ord.InvoiceId
    WHERE     i_ord.State = 0 AND o_ord.Type = 6 AND edi_ord.Type = 1
    UNION
    /*Project*/ SELECT TOP 100 PERCENT /*Order*/ i_ord.InvoiceId AS OrderId, /*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, /*Offer*/ ISNULL(CAST(0 AS bit),
    0) AS IsOffer, 0 AS OfferId, /*Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS InvoiceId, /*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId,
    ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, /*Project*/ ISNULL(CAST(1 AS bit), 1) AS IsProject, proj.ProjectId,  /*SupplierInvoice*/ ISNULL(CAST(0 AS bit), 0) AS IsSupplierInvoice,
    0 AS SupplierInvoiceId, /*Origin*/ proj.Description AS Description,
    proj.Type AS OriginType, st_pType.Name AS OriginTypeName, proj.Status AS OriginStatus, st_pStatus.Name AS OriginStatusName, /*Common*/ 0 AS BillingType,
    '' AS BillingTypeName, cast(proj.Number AS nvarchar(100)) AS Number, 0 AS Amount, 0 AS AmountCurrency, 0 AS VatAmount, 0 AS VatAmountCurrency,
    proj.StartDate AS Date, proj.State, /*Currency*/ s_cu.Code AS CurrencyCode, 0 AS CurrencyRate, cu.SysCurrencyId, /*Lang*/ st_pType.LangId AS LangId
    FROM         dbo.Invoice AS i_ord WITH (NOLOCK) INNER JOIN
    dbo.Origin AS o_ord WITH (NOLOCK) ON o_ord.OriginId = i_ord.InvoiceId INNER JOIN
    dbo.Project AS proj WITH (NOLOCK) ON proj.ProjectId = i_ord.ProjectId INNER JOIN
    dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_ord.CurrencyId INNER JOIN
    SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId INNER JOIN
    SoesysV2.dbo.SysTerm AS st_pType WITH (NOLOCK) ON st_pType.SysTermId = proj.Type INNER JOIN
    SoesysV2.dbo.SysTerm AS st_pStatus WITH (NOLOCK) ON st_pStatus.SysTermId = proj.Status
    WHERE     i_ord.State = 0 AND o_ord.Type = 6 AND st_pType.SysTermGroupId = 297 AND st_pStatus.SysTermGroupId = 287 AND st_pType.LangId = st_pStatus.LangId
    UNION
    /*Supplier invoice*/ SELECT TOP 100 PERCENT /*Order*/ i_ord.InvoiceId AS OrderId, /*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, /*Offer*/ ISNULL(CAST(0 AS bit),
    0) AS IsOffer, 0 AS OfferId, /*Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS InvoiceId, /*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId,
    ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, /*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, /*SupplierInvoice*/ ISNULL(CAST(1 AS bit), 1) AS IsSupplierInvoice,
    i_inv.InvoiceId AS SupplierInvoiceId, /*Origin*/ o_inv.Description, o_inv.Type AS OriginType,
    st_oType.Name AS OriginTypeName, o_inv.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, /*Common*/ i_inv.BillingType,
    st_iBillingType.Name AS BillingTypeName, i_inv.InvoiceNr AS Number, ISNULL(CAST(i_inv.TotalAmount AS decimal(10, 2)), 0) AS Amount,
    ISNULL(CAST(i_inv.TotalAmountCurrency AS decimal(10, 2)), 0) AS AmountCurrency, i_inv.VATAmount AS VatAmount, i_inv.VATAmountCurrency AS VatAmountCurrency,
    i_inv.InvoiceDate AS Date, i_inv.State, /*Currency*/ s_cu.Code AS CurrencyCode, i_ord.CurrencyRate, cu.SysCurrencyId, /*Lang*/ st_oType.LangId AS LangId
    FROM         dbo.Invoice AS i_ord WITH (NOLOCK) INNER JOIN
    dbo.Origin AS o_ord WITH (NOLOCK) ON o_ord.OriginId = i_ord.InvoiceId INNER JOIN
    dbo.OriginInvoiceMapping AS orm WITH (NOLOCK) ON orm.OriginId = o_ord.OriginId INNER JOIN
    dbo.Origin AS o_inv WITH (NOLOCK) ON o_inv.OriginId = orm.InvoiceId INNER JOIN
    dbo.Invoice AS i_inv WITH (NOLOCK) ON i_inv.InvoiceId = o_inv.OriginId INNER JOIN
    dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_ord.CurrencyId INNER JOIN
    SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o_inv.Type INNER JOIN
    SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o_inv.Status INNER JOIN
    SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i_inv.BillingType INNER JOIN
    SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
    WHERE     i_ord.State = 0 AND o_ord.Type = 6 AND o_inv.Type = 1 AND st_oStatus.SysTermGroupId = 30 AND st_oType.SysTermGroupId = 31 AND
    st_iBillingType.SysTermGroupId = 27 AND st_oStatus.LangId = st_oType.LangId AND st_iBillingType.LangId = st_oType.LangId



GO


/****** Object:  View [dbo].[InvoiceTraceView]    Script Date: 2014-10-29 11:02:47 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER VIEW [dbo].[InvoiceTraceView]
AS

/*Contract*/ 
SELECT TOP 100 PERCENT 
	/*Invoice*/ i_inv.InvoiceId, 
	/*Contract*/ ISNULL(CAST(1 AS bit), 1) AS IsContract, o_con.OriginId AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS MappedInvoiceId, 
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, CAST('' AS nvarchar) AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName, '' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ o_con.Description, o_con.Type AS OriginType, st_oType.Name AS OriginTypeName, o_con.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, 
	/*Common*/ i_con.BillingType, st_iBillingType.Name AS BillingTypeName, i_con.InvoiceNr AS Number, ISNULL(CAST(i_con.TotalAmount AS decimal(10, 2)), 0) AS Amount, ISNULL(CAST(i_con.TotalAmountCurrency AS decimal(10, 2)), 0) AS AmountCurrency, i_con.VATAmount AS VatAmount, i_con.VATAmountCurrency AS VatAmountCurrency, i_con.InvoiceDate AS Date, i_con.State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, i_con.CurrencyRate, cu.SysCurrencyId, 
	/*Lang*/ st_oType.LangId AS LangId
FROM         
	dbo.Invoice AS i_inv WITH (NOLOCK) INNER JOIN
	dbo.Origin AS o_inv WITH (NOLOCK) ON o_inv.OriginId = i_inv.InvoiceId INNER JOIN
	dbo.OriginInvoiceMapping AS orm_inv WITH (NOLOCK) ON orm_inv.InvoiceId = i_inv.InvoiceId INNER JOIN
	dbo.Origin AS o_con WITH (NOLOCK) ON o_con.OriginId = orm_inv.OriginId INNER JOIN
	dbo.Invoice AS i_con WITH (NOLOCK) ON i_con.InvoiceId = o_con.OriginId INNER JOIN
	dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_inv.CurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o_con.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o_con.Status INNER JOIN
	SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i_con.BillingType INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
WHERE     
	i_inv.State = 0 AND 
	o_inv.Type = 2 AND 
	o_con.Type = 7 AND 
	st_oStatus.SysTermGroupId = 30 AND 
	st_oType.SysTermGroupId = 31 AND 
	st_iBillingType.SysTermGroupId = 27 AND 
	st_oStatus.LangId = st_oType.LangId AND 
	st_iBillingType.LangId = st_oType.LangId
	
UNION

/*Offer*/ 
SELECT TOP 100 PERCENT 
	/*Invoice*/ i_inv.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(1 AS bit), 1) AS IsOffer,orm_inv.OriginId AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS MappedInvoiceId, 
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, CAST('' AS nvarchar) AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName, '' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ o_off.Description, o_off.Type AS OriginType, st_oType.Name AS OriginTypeName, o_off.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, 
	/*Common*/ i_off.BillingType, st_iBillingType.Name AS BillingTypeName, i_off.InvoiceNr AS Number, ISNULL(CAST(i_off.TotalAmount AS decimal(10, 2)), 0) AS Amount, ISNULL(CAST(i_off.TotalAmount AS decimal(10, 2)), 0) AS AmountCurrency, i_off.VATAmount AS VatAmount, i_off.VATAmountCurrency AS VatAmountCurrency, i_off.InvoiceDate AS Date, i_off.State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, i_off.CurrencyRate, cu.SysCurrencyId, 
	/*Lang*/ st_oType.LangId AS LangId
FROM        
	dbo.Invoice AS i_inv WITH (NOLOCK) INNER JOIN
	dbo.Origin AS o_inv WITH (NOLOCK) ON o_inv.OriginId = i_inv.InvoiceId INNER JOIN
	dbo.OriginInvoiceMapping AS orm_inv WITH (NOLOCK) ON orm_inv.InvoiceId = i_inv.InvoiceId INNER JOIN
	dbo.Origin AS o_off WITH (NOLOCK) ON o_off.OriginId = orm_inv.OriginId INNER JOIN
	dbo.Invoice AS i_off WITH (NOLOCK) ON i_off.InvoiceId = o_off.OriginId INNER JOIN
	dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_inv.CurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o_off.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o_off.Status INNER JOIN
	SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i_off.BillingType INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
WHERE     
	i_inv.State = 0 AND 
	o_inv.Type = 2 AND 
	o_off.Type = 5 AND 
	st_oStatus.SysTermGroupId = 30 AND 
	st_oType.SysTermGroupId = 31 AND 
	st_iBillingType.SysTermGroupId = 27 AND 
	st_oStatus.LangId = st_oType.LangId AND 
	st_iBillingType.LangId = st_oType.LangId
	
UNION

/*Order*/ 
SELECT TOP 100 PERCENT 
	/*Invoice*/ i_inv.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(1 AS bit), 1) AS IsOrder, orm_inv.OriginId AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS MappedInvoiceId, 
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId,
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, '' AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName, '' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ o_ord.Description, o_ord.Type AS OriginType, st_oType.Name AS OriginTypeName, o_ord.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, 
	/*Common*/ i_ord.BillingType, st_iBillingType.Name AS BillingTypeName, i_ord.InvoiceNr AS Number, ISNULL(CAST(i_ord.TotalAmount AS decimal(10, 2)), 0) AS Amount, ISNULL(CAST(i_ord.TotalAmountCurrency AS decimal(10, 2)), 0) AS AmountCurrency, i_ord.VATAmount AS VatAmount, i_ord.VATAmountCurrency AS VatAmountCurrency, i_ord.InvoiceDate AS Date, i_ord.State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, i_ord.CurrencyRate, cu.SysCurrencyId, /*Lang*/ st_oType.LangId AS LangId
FROM         
	dbo.Invoice AS i_inv WITH (NOLOCK) INNER JOIN
	dbo.Origin AS o_inv WITH (NOLOCK) ON o_inv.OriginId = i_inv.InvoiceId INNER JOIN
	dbo.OriginInvoiceMapping AS orm_inv WITH (NOLOCK) ON orm_inv.InvoiceId = i_inv.InvoiceId INNER JOIN
	dbo.Origin AS o_ord WITH (NOLOCK) ON o_ord.OriginId = orm_inv.OriginId INNER JOIN
	dbo.Invoice AS i_ord WITH (NOLOCK) ON i_ord.InvoiceId = o_ord.OriginId INNER JOIN
	dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_inv.CurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o_ord.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o_ord.Status INNER JOIN
	SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i_ord.BillingType INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
WHERE     
	i_inv.State = 0 AND 
	o_inv.Type = 2 AND 
	o_ord.Type = 6 AND 
	st_oStatus.SysTermGroupId = 30 AND 
	st_oType.SysTermGroupId = 31 AND 
	st_iBillingType.SysTermGroupId = 27 AND 
	st_oStatus.LangId = st_oType.LangId AND 
	st_iBillingType.LangId = st_oType.LangId
	
UNION

/*Mapped Invoice*/ 
SELECT TOP 100 PERCENT 
	/*Invoice*/ i_inv.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit),  0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(1 AS bit), 1) AS IsInvoice, orm_inv.OriginId AS MappedInvoiceId, 
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, '' AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName,'' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ o_mapinv.Description, o_mapinv.Type AS OriginType, st_oType.Name AS OriginTypeName, o_mapinv.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, 
	/*Common*/ i_mapinv.BillingType, st_iBillingType.Name AS BillingTypeName, i_mapinv.InvoiceNr AS Number, ISNULL(CAST(i_mapinv.TotalAmount AS decimal(10, 2)), 0) AS Amount, ISNULL(CAST(i_mapinv.TotalAmountCurrency AS decimal(10, 2)), 0) AS AmountCurrency, i_mapinv.VATAmount AS VatAmount, i_mapinv.VATAmountCurrency AS VatAmountCurrency, i_mapinv.InvoiceDate AS Date, i_mapinv.State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, i_mapinv.CurrencyRate, cu.SysCurrencyId, 
	/*Lang*/ st_oType.LangId AS LangId
FROM         
	dbo.Invoice AS i_inv WITH (NOLOCK) INNER JOIN
	dbo.Origin AS o_inv WITH (NOLOCK) ON o_inv.OriginId = i_inv.InvoiceId INNER JOIN
	dbo.OriginInvoiceMapping AS orm_inv WITH (NOLOCK) ON orm_inv.InvoiceId = i_inv.InvoiceId INNER JOIN
	dbo.Origin AS o_mapinv WITH (NOLOCK) ON o_mapinv.OriginId = orm_inv.OriginId INNER JOIN
	dbo.Invoice AS i_mapinv WITH (NOLOCK) ON i_mapinv.InvoiceId = o_mapinv.OriginId INNER JOIN
	dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_inv.CurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o_mapinv.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o_mapinv.Status INNER JOIN
	SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i_mapinv.BillingType INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
WHERE     
	i_inv.State = 0 AND 
	o_inv.Type = 2 AND 
	o_mapinv.Type = 2 AND 
	st_oStatus.SysTermGroupId = 30 AND 
	st_oType.SysTermGroupId = 31 AND 
	st_iBillingType.SysTermGroupId = 27 AND 
	st_oStatus.LangId = st_oType.LangId AND 
	st_iBillingType.LangId = st_oType.LangId
	
UNION

/*Mapped Invoice reversed*/ 
SELECT TOP 100 PERCENT 
	/*Invoice*/ i_inv.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(1 AS bit), 1) AS IsInvoice, orm_inv.InvoiceId AS MappedInvoiceId, 
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, '' AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName,'' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ o_mapinv.Description, o_mapinv.Type AS OriginType, st_oType.Name AS OriginTypeName, o_mapinv.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, 
	/*Common*/ i_mapinv.BillingType, st_iBillingType.Name AS BillingTypeName, i_mapinv.InvoiceNr AS Number, ISNULL(CAST(i_mapinv.TotalAmount AS decimal(10, 2)), 0) AS Amount, ISNULL(CAST(i_mapinv.TotalAmountCurrency AS decimal(10, 2)), 0) AS AmountCurrency, i_mapinv.VATAmount AS VatAmount, i_mapinv.VATAmountCurrency AS VatAmountCurrency, i_mapinv.InvoiceDate AS Date, i_mapinv.State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, i_mapinv.CurrencyRate, cu.SysCurrencyId, 
	/*Lang*/ st_oType.LangId AS LangId
FROM         
	dbo.Invoice AS i_inv WITH (NOLOCK) INNER JOIN
	dbo.Origin AS o_inv WITH (NOLOCK) ON o_inv.OriginId = i_inv.InvoiceId INNER JOIN
	dbo.OriginInvoiceMapping AS orm_inv WITH (NOLOCK) ON orm_inv.OriginId = i_inv.InvoiceId INNER JOIN
	dbo.Origin AS o_mapinv WITH (NOLOCK) ON o_mapinv.OriginId = orm_inv.InvoiceId INNER JOIN
	dbo.Invoice AS i_mapinv WITH (NOLOCK) ON i_mapinv.InvoiceId = o_mapinv.OriginId INNER JOIN
	dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_inv.CurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o_mapinv.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o_mapinv.Status INNER JOIN
	SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i_mapinv.BillingType INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
WHERE     
	i_inv.State = 0 AND 
	o_inv.Type = 2 AND 
	o_mapinv.Type = 2 AND 
	st_oStatus.SysTermGroupId = 30 
	AND st_oType.SysTermGroupId = 31 AND 
	st_iBillingType.SysTermGroupId = 27 AND 
	st_oStatus.LangId = st_oType.LangId AND 
	st_iBillingType.LangId = st_oType.LangId
	
UNION

/*Orginal invoices from CustomerInvoiceReminder*/ 
SELECT 
	/*Invoice*/ i_rem.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(1 AS bit), 1) AS IsInvoice, i_origin.InvoiceId AS MappedInvoiceId, 
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, '' AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName,'' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ o_origin.Description, o_origin.Type AS OriginType, st_oType.Name AS OriginTypeName, o_origin.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, 
	/*Common*/ i_origin.BillingType, st_iBillingType.Name AS BillingTypeName, i_origin.InvoiceNr AS Number, ISNULL(CAST(i_origin.TotalAmount AS decimal(10, 2)), 0) AS Amount, ISNULL(CAST(i_origin.TotalAmountCurrency AS decimal(10, 2)), 0) AS AmountCurrency, i_origin.VATAmount AS VatAmount, i_origin.VATAmountCurrency AS VatAmountCurrency, i_origin.InvoiceDate AS Date, i_origin.State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, i_origin.CurrencyRate, cu.SysCurrencyId, 
	/*Lang*/ st_oType.LangId AS LangId
FROM         
	Invoice AS i_rem WITH (NOLOCK) INNER JOIN
	CustomerInvoice AS ci_rem WITH (NOLOCK) ON ci_rem.InvoiceId = i_rem.InvoiceId INNER JOIN
	Origin AS o_rem WITH (NOLOCK) ON o_rem.OriginId = i_rem.InvoiceId INNER JOIN
	CustomerInvoiceRow AS cir_rem WITH (NOLOCK) ON cir_rem.InvoiceId = ci_rem.InvoiceId INNER JOIN
	CustomerInvoiceReminder AS rem WITH (NOLOCK) ON rem.CustomerInvoiceReminderId = cir_rem.CustomerInvoiceReminderId INNER JOIN
	Invoice AS i_origin WITH (NOLOCK) ON i_origin.InvoiceId = rem.CustomerInvoiceOriginId INNER JOIN
	Origin AS o_origin WITH (NOLOCK) ON o_origin.OriginId = i_origin.InvoiceId INNER JOIN
	Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_origin.CurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o_origin.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o_origin.Status INNER JOIN
	SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i_origin.BillingType INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
WHERE     
	(i_rem.BillingType = 4 OR i_origin.BillingType = 4) AND 
	i_rem.State = 0 AND 
	o_origin.Type = 2 AND 
	st_oStatus.SysTermGroupId = 30 AND 
	st_oType.SysTermGroupId = 31 AND 
	st_iBillingType.SysTermGroupId = 27 AND 
	st_oStatus.LangId = st_oType.LangId AND 
	st_iBillingType.LangId = st_oType.LangId
	
UNION

/*Orginal invoices from CustomerInvoiceInterest*/ 
SELECT 
	/*Invoice*/ i_intr.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(1 AS bit), 1) AS IsInvoice, i_origin.InvoiceId AS MappedInvoiceId, 
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, '' AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName,'' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ o_origin.Description, o_origin.Type AS OriginType, st_oType.Name AS OriginTypeName, o_origin.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, 
	/*Common*/ i_origin.BillingType, st_iBillingType.Name AS BillingTypeName, i_origin.InvoiceNr AS Number, ISNULL(CAST(i_origin.TotalAmount AS decimal(10, 2)), 0) AS Amount, ISNULL(CAST(i_origin.TotalAmountCurrency AS decimal(10, 2)), 0) AS AmountCurrency, i_origin.VATAmount AS VatAmount, i_origin.VATAmountCurrency AS VatAmountCurrency, i_origin.InvoiceDate AS Date, i_origin.State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, i_origin.CurrencyRate, cu.SysCurrencyId, 
	/*Lang*/ st_oType.LangId AS LangId
FROM         
	Invoice AS i_intr WITH (NOLOCK) INNER JOIN
	CustomerInvoiceRow AS cir_intr WITH (NOLOCK) ON cir_intr.InvoiceId = i_intr.InvoiceId INNER JOIN
	CustomerInvoiceInterest AS intr WITH (NOLOCK) ON intr.CustomerInvoiceInterestId = cir_intr.CustomerInvoiceInterestId INNER JOIN
	Invoice AS i_origin WITH (NOLOCK) ON i_origin.InvoiceId = intr.CustomerInvoiceOriginId INNER JOIN
	Origin AS o_origin WITH (NOLOCK) ON o_origin.OriginId = i_origin.InvoiceId INNER JOIN
	Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_origin.CurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o_origin.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o_origin.Status INNER JOIN
	SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i_origin.BillingType INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
WHERE     
	i_intr.BillingType = 3 AND 
	i_intr.State = 0 AND 
	o_origin.Type = 2 AND 
	st_oStatus.SysTermGroupId = 30 AND 
	st_oType.SysTermGroupId = 31 AND 
	st_iBillingType.SysTermGroupId = 27 AND 
	st_oStatus.LangId = st_oType.LangId AND 
	st_iBillingType.LangId = st_oType.LangId
	
UNION

/*CustomerInvoiceReminder*/ 
SELECT 
	/*Invoice*/ i_origin.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS MappedInvoiceId,
	/*Reminder Invoice*/ ISNULL(CAST(1 AS bit), 1) AS IsReminderInvoice, i_rem.InvoiceId AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, '' AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName, '' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ o_rem.Description, o_rem.Type AS OriginType, st_oType.Name AS OriginTypeName, o_rem.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, 
	/*Common*/ i_rem.BillingType, st_iBillingType.Name AS BillingTypeName, i_rem.InvoiceNr AS Number, ISNULL(CAST(i_rem.TotalAmount AS decimal(10, 2)), 0) AS Amount, ISNULL(CAST(i_rem.TotalAmountCurrency AS decimal(10, 2)), 0) AS AmountCurrency, i_rem.VATAmount AS VatAmount, i_rem.VATAmountCurrency AS VatAmountCurrency, i_rem.InvoiceDate AS Date, i_rem.State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, i_rem.CurrencyRate, cu.SysCurrencyId, 
	/*Lang*/ st_oType.LangId AS LangId
FROM         
	Invoice AS i_origin INNER JOIN
	CustomerInvoiceReminder AS rem WITH (NOLOCK) ON rem.CustomerInvoiceOriginId = i_origin.InvoiceId INNER JOIN
	CustomerInvoiceRow AS cir_rem WITH (NOLOCK) ON cir_rem.CustomerInvoiceReminderId = rem.CustomerInvoiceReminderId INNER JOIN
	Invoice AS i_rem WITH (NOLOCK) ON i_rem.InvoiceId = cir_rem.InvoiceId INNER JOIN
	Origin AS o_rem WITH (NOLOCK) ON o_rem.OriginId = i_rem.InvoiceId INNER JOIN
	Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_rem.CurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o_rem.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o_rem.Status INNER JOIN
	SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i_rem.BillingType INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
WHERE     
	i_rem.BillingType = 4 AND 
	i_origin.State = 0 AND 
	o_rem.Type = 2 AND 
	st_oStatus.SysTermGroupId = 30 AND 
	st_oType.SysTermGroupId = 31 AND 
	st_iBillingType.SysTermGroupId = 27 AND 
	st_oStatus.LangId = st_oType.LangId AND 
	st_iBillingType.LangId = st_oType.LangId
	
UNION

/*CustomerInvoiceInterest*/ 
SELECT 
	/*Invoice*/ i_origin.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS MappedInvoiceId,
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(1 AS bit), 1) AS IsInterestInvoice, i_intr.InvoiceId AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, '' AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName, '' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ o_intr.Description, o_intr.Type AS OriginType, st_oType.Name AS OriginTypeName, o_intr.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, 
	/*Common*/ i_intr.BillingType, st_iBillingType.Name AS BillingTypeName, i_intr.InvoiceNr AS Number, ISNULL(CAST(i_intr.TotalAmount AS decimal(10, 2)), 0) AS Amount, ISNULL(CAST(i_intr.TotalAmountCurrency AS decimal(10, 2)), 0) AS AmountCurrency, i_intr.VATAmount AS VatAmount,  i_intr.VATAmountCurrency AS VatAmountCurrency, i_intr.InvoiceDate AS Date, i_intr.State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, i_intr.CurrencyRate,  cu.SysCurrencyId, 
	/*Lang*/ st_oType.LangId AS LangId
FROM         
	Invoice AS i_origin WITH (NOLOCK) INNER JOIN
	CustomerInvoice AS ci_origin WITH (NOLOCK) ON ci_origin.InvoiceId = i_origin.InvoiceId INNER JOIN
	Origin AS o_origin WITH (NOLOCK) ON o_origin.OriginId = i_origin.InvoiceId INNER JOIN
	CustomerInvoiceInterest AS intr WITH (NOLOCK) ON intr.CustomerInvoiceOriginId = ci_origin.InvoiceId INNER JOIN
	CustomerInvoiceRow AS cir_intr WITH (NOLOCK) ON cir_intr.CustomerInvoiceInterestId = intr.CustomerInvoiceInterestId INNER JOIN
	Invoice AS i_intr WITH (NOLOCK) ON i_intr.InvoiceId = cir_intr.InvoiceId INNER JOIN
	Origin AS o_intr WITH (NOLOCK) ON o_intr.OriginId = i_intr.InvoiceId INNER JOIN
	Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_intr.CurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o_intr.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o_intr.Status INNER JOIN
	SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i_intr.BillingType INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
WHERE     
	i_intr.BillingType = 3 AND 
	i_origin.State = 0 AND 
	o_intr.Type = 2 AND 
	st_oStatus.SysTermGroupId = 30 AND 
	st_oType.SysTermGroupId = 31 AND 
	st_iBillingType.SysTermGroupId = 27 AND 
	st_oStatus.LangId = st_oType.LangId AND 
	st_iBillingType.LangId = st_oType.LangId
	
UNION

/*Payment*/ 
SELECT TOP 100 PERCENT 
	/*Invoice*/ i.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS MappedInvoiceId, 
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(1 AS bit), 1) AS IsPayment, pr.PaymentRowId, pr.Status AS PaymentStatusId, st_pStatus.Name AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName,'' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ o.Description, o.Type AS OriginType, st_oType.Name AS OriginTypeName, o.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, 
	/*Common*/ i.BillingType, st_iBillingType.Name AS BillingTypeName, cast(pr.SeqNr AS nvarchar(100)) AS Number, pr.Amount AS Amount, pr.AmountCurrency AS AmountCurrency, 0 AS VatAmount, 0 AS VatAmountCurrency, pr.PayDate AS Date, pr.State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, pr.CurrencyRate, cu.SysCurrencyId, 
	/*Lang*/ st_oType.LangId AS LangId
FROM         
	dbo.Invoice AS i INNER JOIN
	dbo.PaymentRow AS pr WITH (NOLOCK) ON pr.InvoiceId = i.InvoiceId INNER JOIN
	dbo.Payment AS p WITH (NOLOCK) ON p.PaymentId = pr.PaymentId INNER JOIN
	dbo.Origin AS o WITH (NOLOCK) ON p.PaymentId = o.OriginId INNER JOIN
	dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i.CurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o.Status INNER JOIN
	SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i.BillingType INNER JOIN
	SoesysV2.dbo.SysTerm AS st_pStatus WITH (NOLOCK) ON st_pStatus.SysTermId = pr.Status INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
WHERE     
	i.State = 0 AND 
	ISNULL(pr.PaymentRowId, 0) > 0 AND 
	st_oStatus.SysTermGroupId = 30 AND 
	st_oType.SysTermGroupId = 31 AND 
	st_iBillingType.SysTermGroupId = 27 AND 
	st_pStatus.SysTermGroupId = 35 AND 
	st_oStatus.LangId = st_oType.LangId AND 
	st_iBillingType.LangId = st_oType.LangId AND 
	st_pStatus.LangId = st_oType.LangId
	
UNION

/*EDI*/ 
SELECT TOP 100 PERCENT 
	/*Invoice*/ i_inv.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS MappedInvoiceId, 
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, '' AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(1 AS bit), 1) AS IsEdi, ISNULL(i_edi.EdiEntryId, 0) AS EdiEntryId, i_edi.HasPdf AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName, '' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ (ISNULL(i_edi.WholesellerName, '') + ' ' + ISNULL(i_edi.SellerOrderNr, '')) AS Description, 0 AS OriginType, 'EDI' AS OriginTypeName, i_edi.InvoiceStatus AS OriginStatus, i_edi.InvoiceStatusName AS OriginStatusName,
	/*Common*/ 0 AS BillingType, '' AS BillingTypeName, i_edi.InvoiceNr AS Number, i_edi.Sum AS Amount, i_edi.SumCurrency AS AmountCurrency, i_edi.SumVat AS VatAmount, i_edi.SumVatCurrency AS VatAmountCurrency, ISNULL(i_edi.Date, i_inv.InvoiceDate) AS Date, i_edi.State, 
	/*Currency*/ i_edi.CurrencyCode, i_edi.CurrencyRate,  i_edi.SysCurrencyId,
	/*Lang*/ i_edi.LangId
FROM         
	dbo.Invoice AS i_inv WITH (NOLOCK) INNER JOIN
	dbo.Origin AS o_inv WITH (NOLOCK) ON o_inv.OriginId = i_inv.InvoiceId INNER JOIN
	dbo.EdiEntryView AS i_edi WITH (NOLOCK) ON i_edi.InvoiceId = i_inv.InvoiceId
WHERE     
	i_inv.State = 0 AND 
	o_inv.Type = 1 AND 
	i_edi.Type = 1
	
UNION

/*Scanning*/ 
SELECT TOP 100 PERCENT 
	/*Invoice*/ i_inv.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS MappedInvoiceId, 
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, '' AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(1 AS bit), 1) AS IsEdi, ISNULL(i_edi.EdiEntryId, 0) AS EdiEntryId, i_edi.HasPdf AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName, '' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ '' AS Description, 0 AS OriginType, 'Scanning' AS OriginTypeName, i_edi.InvoiceStatus AS OriginStatus, i_edi.InvoiceStatusName AS OriginStatusName,
	/*Common*/ 0 AS BillingType, '' AS BillingTypeName, i_edi.InvoiceNr AS Number, i_edi.Sum AS Amount, i_edi.SumCurrency AS AmountCurrency, i_edi.SumVat AS VatAmount, i_edi.SumVatCurrency AS VatAmountCurrency, ISNULL(i_edi.Date, i_inv.InvoiceDate) AS Date, i_edi.State, 
	/*Currency*/ i_edi.CurrencyCode, i_edi.CurrencyRate,  i_edi.SysCurrencyId,
	/*Lang*/ i_edi.LangId
FROM         
	dbo.Invoice AS i_inv WITH (NOLOCK) INNER JOIN
	dbo.Origin AS o_inv WITH (NOLOCK) ON o_inv.OriginId = i_inv.InvoiceId INNER JOIN
	dbo.EdiEntryView AS i_edi WITH (NOLOCK) ON i_edi.InvoiceId = i_inv.InvoiceId INNER JOIN
	dbo.ScanningEntryView as i_sca WITH (NOLOCK) ON i_sca.ScanningEntryId = i_edi.ScanningEntryId
WHERE     
	i_inv.State = 0 AND 
	o_inv.Type = 1 AND 
	i_edi.Type = 2
	
UNION

/*Voucher*/ 
SELECT TOP 100 PERCENT 
	/*Invoice*/ i.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS MappedInvoiceId, 
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, '' AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(1 AS bit), 1) AS IsVoucher, ISNULL(vh.VoucherHeadId, 0) AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName, '' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ o.Description, o.Type AS OriginType, st_oType.Name AS OriginTypeName, o.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, 	
	/*Common*/ 0 AS BillingType, '' AS BillingTypeName, ISNULL(CAST(vh.VoucherNr AS nvarchar), 0) AS Number, CAST
	((SELECT     sum(Amount)
	  FROM         VoucherRow AS vr
	  WHERE     vr.Amount > 0 AND vr.VoucherHeadId = vh.VoucherHeadId) AS decimal(10, 2)) AS Amount, CAST
	((SELECT     sum(Amount) * i.CurrencyRate
	  FROM         VoucherRow AS vr
	  WHERE     vr.Amount > 0 AND vr.VoucherHeadId = vh.VoucherHeadId) AS decimal(10, 2)) AS AmountCurrency, 0 AS VatAmount, 0 AS VatAmountCurrency, 
	vh.Date AS Date, 0 AS State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, i.CurrencyRate, cu.SysCurrencyId, /*Lang*/ st_oStatus.LangId AS LangId
FROM         
	dbo.Invoice AS i WITH (NOLOCK) INNER JOIN
	dbo.Origin AS o WITH (NOLOCK) ON o.OriginId = i.InvoiceId INNER JOIN
	dbo.VoucherHead AS vh WITH (NOLOCK) ON i.VoucherHeadId = vh.VoucherHeadId INNER JOIN
	dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i.CurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o.Status INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
WHERE     
	(i.State = 0) AND 
	(o.Type = 1 OR o.Type = 2) AND 
	(ISNULL(i.VoucherHeadId, 0) > 0) AND 
	st_oStatus.SysTermGroupId = 30 AND 
	st_oType.SysTermGroupId = 31 AND
	st_oStatus.LangId = st_oType.LangId 
	
UNION

/*Voucher2*/ 
SELECT TOP 100 PERCENT 
	/*Invoice*/ i.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS MappedInvoiceId, 
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, '' AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(1 AS bit), 1) AS IsVoucher, ISNULL(vh.VoucherHeadId, 0) AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName, '' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ o.Description, o.Type AS OriginType, st_oType.Name AS OriginTypeName, o.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, 	
	/*Common*/ 0 AS BillingType, '' AS BillingTypeName, ISNULL(CAST(vh.VoucherNr AS nvarchar), 0) AS Number, CAST
	  ((SELECT     sum(Amount)
		  FROM         VoucherRow AS vr
		  WHERE     vr.Amount > 0 AND vr.VoucherHeadId = vh.VoucherHeadId) AS decimal(10, 2)) AS Amount, CAST
	  ((SELECT     sum(Amount) * i.CurrencyRate
		  FROM         VoucherRow AS vr
		  WHERE     vr.Amount > 0 AND vr.VoucherHeadId = vh.VoucherHeadId) AS decimal(10, 2)) AS AmountCurrency, 0 AS VatAmount, 0 AS VatAmountCurrency, 
      vh.Date AS Date, 0 AS State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, i.CurrencyRate, cu.SysCurrencyId, 
	/*Lang*/ st_oStatus.LangId AS LangId
FROM         
	dbo.Invoice AS i WITH (NOLOCK) INNER JOIN
	dbo.Origin AS o WITH (NOLOCK) ON o.OriginId = i.InvoiceId INNER JOIN
	dbo.VoucherHead AS vh WITH (NOLOCK) ON i.VoucherHead2Id = vh.VoucherHeadId INNER JOIN
	dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i.CurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o.Status INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
WHERE     
	(i.State = 0) AND 
	(o.Type = 1 OR o.Type = 2) AND 
	(ISNULL(i.VoucherHead2Id, 0) > 0) AND 
	st_oStatus.SysTermGroupId = 30 AND 
	st_oType.SysTermGroupId = 31 AND
	st_oStatus.LangId = st_oType.LangId 
	
UNION

/*Inventory*/ 
SELECT TOP 100 PERCENT 
	/*invoice*/ i.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS MappedInvoiceId,
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, '' AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(1 AS bit), 1) AS IsInventory, inv.InventoryId AS InventoryId, inv.Name AS InventoryName, inv.Description AS InventoryDescription, stg_tracing.Name AS InventoryTypeName, inv.Status AS InventoryStatusId, st_invStatus.Name AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ o.Description, o.Type AS OriginType, st_oType.Name AS OriginTypeName, o.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, 
	/*Common*/ i.BillingType, st_iBillingType.Name AS BillingTypeName, inv.InventoryNr AS Number, inv.PurchaseAmount AS Amount, ISNULL(CAST(inv.PurchaseAmount AS decimal(10, 2)), 0) AS AmountCurrency, 0 AS VatAmount, 0 AS VatAmountCurrency, inv.PurchaseDate AS Date, inv.State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, i.CurrencyRate, cu.SysCurrencyId, 
	/*Lang*/ st_oType.LangId AS LangId
FROM         
	dbo.Invoice AS i WITH (NOLOCK) INNER JOIN
	dbo.Origin AS o WITH (NOLOCK) ON i.InvoiceId = o.OriginId INNER JOIN
	dbo.InventoryLog AS invl WITH (NOLOCK) ON invl.InvoiceId = i.InvoiceId INNER JOIN
	dbo.Inventory AS inv WITH (NOLOCK) ON inv.InventoryId = invl.InventoryId AND inv.SupplierInvoiceId = invl.InvoiceId INNER JOIN
	dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i.CurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS stg_tracing WITH (NOLOCK) ON stg_tracing.SysTermId = 24 INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o.Status INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i.BillingType INNER JOIN
	SoesysV2.dbo.SysTerm AS st_invStatus WITH (NOLOCK) ON st_invStatus.SysTermId = inv.Status INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
WHERE     
	ISNULL(invl.InventoryLogId, 0) > 0 AND 
	invl.Type = 1 /* = Purchase*/ AND 
	st_oStatus.SysTermGroupId = 30 AND 
	st_oType.SysTermGroupId = 31 AND 
	st_iBillingType.SysTermGroupId = 27 AND 
	stg_tracing.SysTermGroupId = 54 AND 
	st_invStatus.SysTermGroupId = 151 AND 
	st_oStatus.LangId = st_oType.LangId AND 
	stg_tracing.LangId = st_oType.LangId AND 
	st_invStatus.LangId = st_oType.LangId AND 
	st_iBillingType.LangId = st_oType.LangId
UNION

/*Project*/ 
SELECT TOP 100 PERCENT 
	/*Invoice*/ i_inv.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS MappedInvoiceId,
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, '' AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName, '' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(1 AS bit), 1) AS IsProject, proj.ProjectId, 
	/*Origin*/ proj.Description AS Description, proj.Type AS OriginType, st_pType.Name AS OriginTypeName, proj.Status AS OriginStatus, st_pStatus.Name AS OriginStatusName, 
	/*Common*/ 0 AS BillingType, '' AS BillingTypeName, CAST(proj.Number AS nvarchar(100)) AS Number, 0 AS Amount, 0 AS AmountCurrency, 0 AS VatAmount, 0 AS VatAmountCurrency, proj.StartDate AS Date, proj.State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, 0 AS CurrencyRate, cu.SysCurrencyId, 
	/*Lang*/ st_pType.LangId AS LangId
FROM         
	dbo.Invoice AS i_inv WITH (NOLOCK) INNER JOIN
	dbo.Origin AS o_inv WITH (NOLOCK) ON o_inv.OriginId = i_inv.InvoiceId INNER JOIN
	dbo.Project AS proj WITH (NOLOCK) ON proj.ProjectId = i_inv.ProjectId INNER JOIN
	dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_inv.CurrencyId INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_pType WITH (NOLOCK) ON st_pType.SysTermId = proj.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_pStatus WITH (NOLOCK) ON st_pStatus.SysTermId = proj.Status
WHERE     
	i_inv.State = 0 AND 
	o_inv.Type = 2 AND 
	st_pType.SysTermGroupId = 297 AND 
	st_pStatus.SysTermGroupId = 287 AND 
	st_pType.LangId = st_pStatus.LangId

UNION

/*SupplierInvoiceProject*/ 
SELECT TOP 100 PERCENT 
	/*Invoice*/ i_inv.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS MappedInvoiceId,
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, '' AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName, '' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(1 AS bit), 1) AS IsProject, proj.ProjectId, 
	/*Origin*/ proj.Description AS Description, proj.Type AS OriginType, st_pType.Name AS OriginTypeName, proj.Status AS OriginStatus, st_pStatus.Name AS OriginStatusName, 
	/*Common*/ 0 AS BillingType, '' AS BillingTypeName, CAST(proj.Number AS nvarchar(100)) AS Number, 0 AS Amount, 0 AS AmountCurrency, 0 AS VatAmount, 0 AS VatAmountCurrency, proj.StartDate AS Date, proj.State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, 0 AS CurrencyRate, cu.SysCurrencyId, 
	/*Lang*/ st_pType.LangId AS LangId
FROM         
	dbo.Invoice AS i_inv WITH (NOLOCK) INNER JOIN
	dbo.Origin AS o_inv WITH (NOLOCK) ON o_inv.OriginId = i_inv.InvoiceId INNER JOIN
	dbo.TimeCodeTransaction AS tct WITH (NOLOCK) ON tct.SupplierInvoiceId = i_inv.InvoiceId INNER JOIN
	dbo.Project as proj WITH (NOLOCK) ON proj.ProjectId = tct.ProjectId INNER JOIN
	--dbo.Project AS proj WITH (NOLOCK) ON proj.ProjectId = i_inv.ProjectId INNER JOIN
	dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_inv.CurrencyId INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_pType WITH (NOLOCK) ON st_pType.SysTermId = proj.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_pStatus WITH (NOLOCK) ON st_pStatus.SysTermId = proj.Status
WHERE     
	i_inv.State = 0 AND 
	o_inv.Type = 1 AND 
	st_pType.SysTermGroupId = 297 AND 
	st_pStatus.SysTermGroupId = 287 AND 
	st_pType.LangId = st_pStatus.LangId


GO

USE [soecompv2]
GO

/****** Object:  StoredProcedure [dbo].[GetTimePayrollTransactionAccountsForCompany]    Script Date: 2014-10-29 14:46:16 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




    CREATE PROCEDURE [dbo].[GetTimePayrollTransactionAccountsForCompany]
		@actorCompanyId INT,
		@startDate DateTime = NULL,
		@stopDate DateTime = NULL,
		@timePeriodId INT = NULL
    AS
    BEGIN
    SET NOCOUNT ON;

    SELECT
		tpt.ActorCompanyId,
		tpt.EmployeeId,
		tpt.TimePeriodId,
		tptacc.TimePayrollTransactionId,
		tptacc.AccountId,		
		acc.AccountNr,
		acc.Name,
		ad.AccountDimId,
		ad.AccountDimNr,
		ad.Name as AccountDimName
    FROM
		TimePayrollTransactionAccount as tptacc WITH (NOLOCK) INNER JOIN
		TimePayrollTransaction as tpt WITH (NOLOCK) ON tptacc.TimePayrollTransactionId = tpt.TimePayrollTransactionId INNER JOIN
		TimeBlockDate as tbd WITH (NOLOCK) ON tpt.TimeBlockDateId = tbd.TimeBlockDateId INNER JOIN
		Account as acc WITH (NOLOCK) ON tptacc.AccountId = acc.AccountId INNER JOIN
		AccountDim as ad WITH (NOLOCK) ON acc.AccountDimId = ad.AccountDimId
    WHERE
		(tpt.ActorCompanyId = @actorCompanyId) AND
		(tpt.State = 0) AND
		(
		(@timePeriodId IS NOT NULL AND @timePeriodId = tpt.TimePeriodId)
		OR
		((tpt.TimePeriodId IS NULL OR @timePeriodId IS NULL) AND (@startDate IS NOT NULL AND @stopDate IS NOT NULL AND tbd.Date BETWEEN @startDate AND @stopDate))
		)
    ORDER BY
		tptacc.TimePayrollTransactionId, tptacc.AccountId

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


USE [soecompv2]
GO

/****** Object:  StoredProcedure [dbo].[GetTimePayrollTransactionAccountsForEmployee]    Script Date: 2014-10-29 14:48:30 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




    ALTER PROCEDURE [dbo].[GetTimePayrollTransactionAccountsForEmployee]
		@employeeId INT,
		@startDate DateTime = NULL,
		@stopDate DateTime = NULL,
		@timePeriodId INT = NULL
    AS
    BEGIN
    SET NOCOUNT ON;

    SELECT
		tpt.ActorCompanyId,
		tpt.EmployeeId,
		tpt.TimePeriodId,
		tptacc.TimePayrollTransactionId,
		tptacc.AccountId,		
		acc.AccountNr,
		acc.Name,
		ad.AccountDimId,
		ad.AccountDimNr,
		ad.Name as AccountDimName
    FROM
		TimePayrollTransactionAccount as tptacc WITH (NOLOCK) INNER JOIN
		TimePayrollTransaction as tpt WITH (NOLOCK) ON tptacc.TimePayrollTransactionId = tpt.TimePayrollTransactionId INNER JOIN
		TimeBlockDate as tbd WITH (NOLOCK) ON tpt.TimeBlockDateId = tbd.TimeBlockDateId INNER JOIN
		Account as acc WITH (NOLOCK) ON tptacc.AccountId = acc.AccountId INNER JOIN
		AccountDim as ad WITH (NOLOCK) ON acc.AccountDimId = ad.AccountDimId
    WHERE
		(tbd.EmployeeId = @employeeId) AND
		(tpt.State = 0) AND
		(
		(@timePeriodId IS NOT NULL AND @timePeriodId = tpt.TimePeriodId)
		OR
		((tpt.TimePeriodId IS NULL OR @timePeriodId IS NULL) AND (@startDate IS NOT NULL AND @stopDate IS NOT NULL AND tbd.Date BETWEEN @startDate AND @stopDate))
		)
    ORDER BY
		tptacc.TimePayrollTransactionId, tptacc.AccountId

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


USE [soecompv2]
GO

/****** Object:  StoredProcedure [dbo].[GetTimePayrollTransactionsForCompany]    Script Date: 2014-10-29 14:50:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO








ALTER PROCEDURE [dbo].[GetTimePayrollTransactionsForCompany]
	@actorCompanyId INT,
	@employees NVARCHAR(MAX),
	@startDate DateTime = NULL,
	@stopDate DateTime = NULL,
	@timePeriodId INT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		tpt.EmployeeId,
		tpt.TimePayrollTransactionId,
		tpt.TimePeriodId,
		tbd.Date,
		tb.TimeBlockId,
		tbd.TimeBlockDateId,
		ast.AttestStateId,
		ast.Name as AttestStateName,
		ast.Color as AttestStateColor,
		ast.Initial as AttestStateInitial
	FROM 
		TimePayrollTransaction as tpt WITH (NOLOCK) INNER JOIN
		AttestState as ast WITH (NOLOCK) ON tpt.AttestStateId = ast.AttestStateId INNER JOIN
		TimeBlockDate as tbd WITH (NOLOCK) on tpt.TimeBlockDateId = tbd.TimeBlockDateId LEFT OUTER JOIN
		TimeBlock as tb WITH (NOLOCK) on tpt.TimeBlockId = tb.TimeBlockId
	WHERE
		(tpt.ActorCompanyId = @actorCompanyId) AND 
		(@employees IS NULL OR LEN(@employees) = 0 OR tpt.EmployeeId IN (Select id from dbo.Split(@employees,','))) AND
		(tb.State IS NULL OR tb.State = 0) AND
		(tpt.State = 0) AND
		(
		  (@timePeriodId IS NOT NULL AND @timePeriodId = tpt.TimePeriodId) 
		  OR
		  ((tpt.TimePeriodId IS NULL OR @timePeriodId IS NULL) AND (@startDate IS NOT NULL AND @stopDate IS NOT NULL AND tbd.Date BETWEEN @startDate AND @stopDate))
		)
	ORDER BY
		tbd.Date


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


USE [soecompv2]
GO

ALTER TABLE [dbo].[TimeInvoiceTransactionAccount] CHECK CONSTRAINT [FK_TimeInvoiceTransactionAccount_AccountInternal]
GO

ALTER TABLE [dbo].[TimeInvoiceTransactionAccount] CHECK CONSTRAINT [FK_TimeInvoiceTransactionAccount_TimeInvoiceTransaction]
GO

USE [soecompv2]
GO

/****** Object:  Table [dbo].[EndReason]    Script Date: 2014-11-03 10:19:17 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[EndReason](
	[EndReasonId] [int] IDENTITY(1,1) NOT NULL,
	[ActorCompanyId] [int] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Created] [datetime] NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[Modified] [datetime] NULL,
	[ModifiedBy] [nvarchar](50) NULL,
	[State] [int] NOT NULL,
 CONSTRAINT [PK_EndReason] PRIMARY KEY CLUSTERED 
(
	[EndReasonId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[EndReason] ADD  CONSTRAINT [DF_EndReason_State]  DEFAULT ((0)) FOR [State]
GO

ALTER TABLE [dbo].[EndReason]  WITH CHECK ADD  CONSTRAINT [FK_EndReason_Company] FOREIGN KEY([ActorCompanyId])
REFERENCES [dbo].[Company] ([ActorCompanyId])
GO

ALTER TABLE [dbo].[EndReason] CHECK CONSTRAINT [FK_EndReason_Company]
GO

USE [soecompv2]
GO

---$ Drop FK : FK_TimeStampEntry_EmployeeChild
IF OBJECT_ID(N'dbo.FK_TimeStampEntry_EmployeeChild') IS NOT NULL
BEGIN
    PRINT 'Drop constraint FK_TimeStampEntry_EmployeeChild'
    ALTER TABLE dbo.TimeStampEntry
        DROP CONSTRAINT FK_TimeStampEntry_EmployeeChild
END
GO

---$ Alter table dbo.TimeStampEntry
IF EXISTS(SELECT * FROM SYS.COLUMNS WHERE OBJECT_ID = OBJECT_ID(N'dbo.TimeStampEntry') AND NAME = 'EmployeeChildId')
BEGIN
    PRINT 'Drop column : dbo.TimeStampEntry.EmployeeChildId'
    ALTER TABLE dbo.TimeStampEntry
        DROP COLUMN EmployeeChildId
END
GO


---$ Alter table dbo.TimeStampEntryRaw
IF EXISTS(SELECT * FROM SYS.COLUMNS WHERE OBJECT_ID = OBJECT_ID(N'dbo.TimeStampEntryRaw') AND NAME = 'EmployeeChildRecordId')
BEGIN
    PRINT 'Drop column : dbo.TimeStampEntryRaw.EmployeeChildRecordId'
    ALTER TABLE dbo.TimeStampEntryRaw
        DROP COLUMN EmployeeChildRecordId
END
GO






USE [soecompv2]
GO

/****** Object:  Table [dbo].[TimePayrollScheduleTransaction]    Script Date: 2014-11-03 10:30:10 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TimePayrollScheduleTransaction](
	[TimePayrollScheduleTransactionId] [int] IDENTITY(1,1) NOT NULL,
	[ActorCompanyId] [int] NOT NULL,
	[EmployeeId] [int] NOT NULL,
	[TimeBlockDateId] [int] NOT NULL,
	[ProductId] [int] NOT NULL,
	[AccountId] [int] NOT NULL,
	[TimeScheduleTemplatePeriodId] [int] NULL,
	[TimeScheduleTemplateBlockId] [int] NULL,
	[PayrollPriceFormulaId] [int] NULL,
	[PayrollPriceTypeId] [int] NULL,
	[Type] [int] NULL,
	[Quantity] [decimal](10, 2) NOT NULL,
	[Amount] [decimal](10, 2) NULL,
	[AmountCurrency] [decimal](10, 2) NULL,
	[AmountLedgerCurrency] [decimal](10, 2) NULL,
	[AmountEntCurrency] [decimal](10, 2) NULL,
	[VatAmount] [decimal](10, 2) NULL,
	[VatAmountCurrency] [decimal](10, 2) NULL,
	[VatAmountLedgerCurrency] [decimal](10, 2) NULL,
	[VatAmountEntCurrency] [decimal](10, 2) NULL,
	[UnitPrice] [decimal](10, 2) NULL,
	[UnitPriceCurrency] [decimal](10, 2) NULL,
	[UnitPriceLedgerCurrency] [decimal](10, 2) NULL,
	[UnitPriceEntCurrency] [decimal](10, 2) NULL,
	[SysPayrollTypeLevel1] [int] NULL,
	[SysPayrollTypeLevel2] [int] NULL,
	[SysPayrollTypeLevel3] [int] NULL,
	[SysPayrollTypeLevel4] [int] NULL,
	[TimeBlockStartTime] [datetime] NULL,
	[TimeBlockStopTime] [datetime] NULL,
	[Formula] [nvarchar](max) NULL,
	[FormulaPlain] [nvarchar](max) NULL,
	[FormulaExtracted] [nvarchar](max) NULL,
	[FormulaNames] [nvarchar](max) NULL,
	[FormulaOrigin] [nvarchar](max) NULL,
	[Created] [datetime] NULL,
	[CreatedBy] [nvarchar](50) NULL,
	[Modified] [datetime] NULL,
	[ModifiedBy] [nvarchar](50) NULL,
	[State] [int] NOT NULL,
 CONSTRAINT [PK_TimePayrollScheduleTransaction] PRIMARY KEY CLUSTERED 
(
	[TimePayrollScheduleTransactionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

ALTER TABLE [dbo].[TimePayrollScheduleTransaction] ADD  CONSTRAINT [DF_TimePayrollScheduleTransaction_ActorCompanyId]  DEFAULT ((0)) FOR [ActorCompanyId]
GO

ALTER TABLE [dbo].[TimePayrollScheduleTransaction] ADD  CONSTRAINT [DF_TimePayrollScheduleTransaction_Quantity]  DEFAULT ((0)) FOR [Quantity]
GO

ALTER TABLE [dbo].[TimePayrollScheduleTransaction] ADD  CONSTRAINT [DF_TimePayrollScheduleTransaction_State]  DEFAULT ((0)) FOR [State]
GO

ALTER TABLE [dbo].[TimePayrollScheduleTransaction]  WITH CHECK ADD  CONSTRAINT [FK_TimePayrollScheduleTransaction_AccountStd] FOREIGN KEY([AccountId])
REFERENCES [dbo].[AccountStd] ([AccountId])
GO

ALTER TABLE [dbo].[TimePayrollScheduleTransaction] CHECK CONSTRAINT [FK_TimePayrollScheduleTransaction_AccountStd]
GO

ALTER TABLE [dbo].[TimePayrollScheduleTransaction]  WITH CHECK ADD  CONSTRAINT [FK_TimePayrollScheduleTransaction_Company] FOREIGN KEY([ActorCompanyId])
REFERENCES [dbo].[Company] ([ActorCompanyId])
GO

ALTER TABLE [dbo].[TimePayrollScheduleTransaction] CHECK CONSTRAINT [FK_TimePayrollScheduleTransaction_Company]
GO

ALTER TABLE [dbo].[TimePayrollScheduleTransaction]  WITH CHECK ADD  CONSTRAINT [FK_TimePayrollScheduleTransaction_Employee] FOREIGN KEY([EmployeeId])
REFERENCES [dbo].[Employee] ([EmployeeId])
GO

ALTER TABLE [dbo].[TimePayrollScheduleTransaction] CHECK CONSTRAINT [FK_TimePayrollScheduleTransaction_Employee]
GO

ALTER TABLE [dbo].[TimePayrollScheduleTransaction]  WITH CHECK ADD  CONSTRAINT [FK_TimePayrollScheduleTransaction_PayrollPriceFormula] FOREIGN KEY([PayrollPriceFormulaId])
REFERENCES [dbo].[PayrollPriceFormula] ([PayrollPriceFormulaId])
GO

ALTER TABLE [dbo].[TimePayrollScheduleTransaction] CHECK CONSTRAINT [FK_TimePayrollScheduleTransaction_PayrollPriceFormula]
GO

ALTER TABLE [dbo].[TimePayrollScheduleTransaction]  WITH CHECK ADD  CONSTRAINT [FK_TimePayrollScheduleTransaction_PayrollPriceType] FOREIGN KEY([PayrollPriceTypeId])
REFERENCES [dbo].[PayrollPriceType] ([PayrollPriceTypeId])
GO

ALTER TABLE [dbo].[TimePayrollScheduleTransaction] CHECK CONSTRAINT [FK_TimePayrollScheduleTransaction_PayrollPriceType]
GO

ALTER TABLE [dbo].[TimePayrollScheduleTransaction]  WITH CHECK ADD  CONSTRAINT [FK_TimePayrollScheduleTransaction_PayrollProduct] FOREIGN KEY([ProductId])
REFERENCES [dbo].[PayrollProduct] ([ProductId])
GO

ALTER TABLE [dbo].[TimePayrollScheduleTransaction] CHECK CONSTRAINT [FK_TimePayrollScheduleTransaction_PayrollProduct]
GO

ALTER TABLE [dbo].[TimePayrollScheduleTransaction]  WITH CHECK ADD  CONSTRAINT [FK_TimePayrollScheduleTransaction_TimeBlockDate] FOREIGN KEY([TimeBlockDateId])
REFERENCES [dbo].[TimeBlockDate] ([TimeBlockDateId])
GO

ALTER TABLE [dbo].[TimePayrollScheduleTransaction] CHECK CONSTRAINT [FK_TimePayrollScheduleTransaction_TimeBlockDate]
GO

ALTER TABLE [dbo].[TimePayrollScheduleTransaction]  WITH CHECK ADD  CONSTRAINT [FK_TimePayrollScheduleTransaction_TimeScheduleTemplateBlock] FOREIGN KEY([TimeScheduleTemplateBlockId])
REFERENCES [dbo].[TimeScheduleTemplateBlock] ([TimeScheduleTemplateBlockId])
GO

ALTER TABLE [dbo].[TimePayrollScheduleTransaction] CHECK CONSTRAINT [FK_TimePayrollScheduleTransaction_TimeScheduleTemplateBlock]
GO

ALTER TABLE [dbo].[TimePayrollScheduleTransaction]  WITH CHECK ADD  CONSTRAINT [FK_TimePayrollScheduleTransaction_TimeScheduleTemplatePeriod] FOREIGN KEY([TimeScheduleTemplatePeriodId])
REFERENCES [dbo].[TimeScheduleTemplatePeriod] ([TimeScheduleTemplatePeriodId])
GO

ALTER TABLE [dbo].[TimePayrollScheduleTransaction] CHECK CONSTRAINT [FK_TimePayrollScheduleTransaction_TimeScheduleTemplatePeriod]
GO

USE [soecompv2]
GO

/****** Object:  Table [dbo].[TimePayrollScheduleTransactionAccount]    Script Date: 2014-11-03 10:30:41 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TimePayrollScheduleTransactionAccount](
	[TimePayrollScheduleTransactionId] [int] NOT NULL,
	[AccountId] [int] NOT NULL,
 CONSTRAINT [PK_TimePayrollScheduleTransactionAccount] PRIMARY KEY CLUSTERED 
(
	[TimePayrollScheduleTransactionId] ASC,
	[AccountId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[TimePayrollScheduleTransactionAccount]  WITH CHECK ADD  CONSTRAINT [FK_TimePayrollScheduleTransactionAccount_AccountInternal] FOREIGN KEY([AccountId])
REFERENCES [dbo].[AccountInternal] ([AccountId])
GO

ALTER TABLE [dbo].[TimePayrollScheduleTransactionAccount] CHECK CONSTRAINT [FK_TimePayrollScheduleTransactionAccount_AccountInternal]
GO

ALTER TABLE [dbo].[TimePayrollScheduleTransactionAccount]  WITH CHECK ADD  CONSTRAINT [FK_TimePayrollScheduleTransactionAccount_TimePayrollScheduleTransaction] FOREIGN KEY([TimePayrollScheduleTransactionId])
REFERENCES [dbo].[TimePayrollScheduleTransaction] ([TimePayrollScheduleTransactionId])
GO

ALTER TABLE [dbo].[TimePayrollScheduleTransactionAccount] CHECK CONSTRAINT [FK_TimePayrollScheduleTransactionAccount_TimePayrollScheduleTransaction]
GO





USE [soecompv2]
GO

/****** Object:  Index [Q_Type_EmployeeId_TimeBlockDateId]    Script Date: 2014-11-04 09:11:48 ******/
CREATE NONCLUSTERED INDEX [Q_Type_EmployeeId_TimeBlockDateId] ON [dbo].[TimePayrollScheduleTransaction]
(
	[Type] ASC,
	[EmployeeId] ASC,
	[TimeBlockDateId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

USE [soecompv2]
GO



GO


USE [soecompv2]
GO

/****** Object:  StoredProcedure [dbo].[GetTimePayrollScheduleTransactionsForEmployee]    Script Date: 2014-11-04 11:36:07 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO







CREATE PROCEDURE [dbo].[GetTimePayrollScheduleTransactionsForEmployee]
	@employeeId INT,
	@type int,
	@startDate DateTime = NULL,
	@stopDate DateTime = NULL
WITH RECOMPILE
AS
BEGIN
	SET NOCOUNT ON;

SELECT 
	--TimePayrollTransaction
	tpst.TimePayrollScheduleTransactionId,
	tpst.ActorCompanyId,
	tpst.EmployeeId,		
	tpst.TimeBlockDateId,
	tpst.ProductId,
	tpst.AccountId,
	tpst.TimeScheduleTemplatePeriodId,
	tpst.TimeScheduleTemplateBlockId,
	tpst.PayrollPriceFormulaId,
	tpst.PayrollPriceTypeId,
	tpst.Type,
	tpst.Quantity,
	tpst.Amount,
	tpst.AmountCurrency,
	tpst.AmountLedgerCurrency,
	tpst.AmountEntCurrency,
	tpst.VatAmount,
	tpst.VatAmountCurrency,
	tpst.VatAmountLedgerCurrency,
	tpst.VatAmountEntCurrency,
	tpst.UnitPrice,
	tpst.UnitPriceCurrency,
	tpst.UnitPriceLedgerCurrency,
	tpst.UnitPriceEntCurrency,
	tpst.SysPayrollTypeLevel1 as TransactionSysPayrollTypeLevel1,
	tpst.SysPayrollTypeLevel2 as TransactionSysPayrollTypeLevel2,
	tpst.SysPayrollTypeLevel3 as TransactionSysPayrollTypeLevel3,
	tpst.SysPayrollTypeLevel4 as TransactionSysPayrollTypeLevel4,
	tpst.TimeBlockStartTime,
	tpst.TimeBlockStopTime,
	tpst.Formula,
	tpst.FormulaPlain,
	tpst.FormulaExtracted,
	tpst.FormulaNames,
	tpst.FormulaOrigin,
	tpst.Created,
	tpst.CreatedBy,
	--TimeBlockDate
	tbd.Date,
	--PayrollProduct
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
	p.Name as ProductName
FROM	
	TimePayrollScheduleTransaction as tpst WITH (NOLOCK) INNER JOIN
	TimeBlockDate as tbd WITH (NOLOCK) on tbd.TimeBlockDateId = tpst.TimeBlockDateId INNER JOIN
	PayrollProduct as pp WITH (NOLOCK) ON pp.ProductId = tpst.ProductId INNER JOIN
	Product as p WITH (NOLOCK) ON p.ProductId = pp.ProductId
WHERE
	(tbd.EmployeeId = @employeeId) AND 
	(@type is null OR tpst.Type = @type) AND
	(tbd.Date BETWEEN @startDate AND @stopDate) AND
	(tpst.State = 0)
ORDER BY
	tbd.Date

END


GO



USE [soecompv2]
GO

/****** Object:  StoredProcedure [dbo].[GetTimePayrollScheduleTransactionAccountsForEmployee]    Script Date: 2014-11-04 11:11:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






    CREATE PROCEDURE [dbo].[GetTimePayrollScheduleTransactionAccountsForEmployee]
		@employeeId INT,
		@type int,
		@startDate DateTime = NULL,
		@stopDate DateTime = NULL
	WITH RECOMPILE
    AS
    BEGIN
    SET NOCOUNT ON;

    SELECT
		--TimePayrollScheduleTransaction
		tpst.ActorCompanyId,
		tpst.EmployeeId,
		--TimePayrollTransactionScheduleAccount
		tpstacc.TimePayrollScheduleTransactionId,
		tpstacc.AccountId,		
		--Account
		acc.AccountNr,
		acc.Name,
		--AccountDim
		ad.AccountDimId,
		ad.AccountDimNr,
		ad.Name as AccountDimName
    FROM
		TimePayrollScheduleTransactionAccount as tpstacc WITH (NOLOCK) INNER JOIN
		TimePayrollScheduleTransaction as tpst WITH (NOLOCK) ON tpstacc.TimePayrollScheduleTransactionId = tpst.TimePayrollScheduleTransactionId INNER JOIN
		TimeBlockDate as tbd WITH (NOLOCK) ON tpst.TimeBlockDateId = tbd.TimeBlockDateId INNER JOIN
		Account as acc WITH (NOLOCK) ON tpstacc.AccountId = acc.AccountId INNER JOIN
		AccountDim as ad WITH (NOLOCK) ON acc.AccountDimId = ad.AccountDimId
    WHERE
		(tbd.EmployeeId = @employeeId) AND
		(@type is null OR tpst.Type = @type) AND
		(tbd.Date BETWEEN @startDate AND @stopDate) AND
		(tpst.State = 0)
    ORDER BY
		tpstacc.TimePayrollScheduleTransactionId, tpstacc.AccountId

    END



GO





USE [soecompv2]
GO

/****** Object:  StoredProcedure [dbo].[GetTimePayrollTransactionsForCompany]    Script Date: 2014-11-04 10:00:42 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER PROCEDURE [dbo].[GetTimePayrollTransactionsForCompany]
	@actorCompanyId INT,
	@employees NVARCHAR(MAX),
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
		tpt.TimePeriodId,
		--TimeBlockDate
		tbd.Date,
		tbd.TimeBlockDateId,
		--TimeBlock
		tb.TimeBlockId,
		--AttestState
		ast.AttestStateId,
		ast.Name as AttestStateName,
		ast.Color as AttestStateColor,
		ast.Initial as AttestStateInitial
	FROM 
		TimePayrollTransaction as tpt WITH (NOLOCK) INNER JOIN
		AttestState as ast WITH (NOLOCK) ON tpt.AttestStateId = ast.AttestStateId INNER JOIN
		TimeBlockDate as tbd WITH (NOLOCK) on tpt.TimeBlockDateId = tbd.TimeBlockDateId LEFT OUTER JOIN
		TimeBlock as tb WITH (NOLOCK) on tpt.TimeBlockId = tb.TimeBlockId
	WHERE
		(tpt.ActorCompanyId = @actorCompanyId) AND 
		(@employees IS NULL OR LEN(@employees) = 0 OR tpt.EmployeeId IN (Select id from dbo.Split(@employees,','))) AND
		(tb.State IS NULL OR tb.State = 0) AND
		(tpt.State = 0) AND
		(
		  (@timePeriodId IS NOT NULL AND @timePeriodId = tpt.TimePeriodId) 
		  OR
		  ((tpt.TimePeriodId IS NULL OR @timePeriodId IS NULL) AND (@startDate IS NOT NULL AND @stopDate IS NOT NULL AND tbd.Date BETWEEN @startDate AND @stopDate))
		)
	ORDER BY
		tbd.Date


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


USE [soecompv2]
GO

/****** Object:  StoredProcedure [dbo].[GetTimePayrollTransactionsForEmployee]    Script Date: 2014-11-04 10:01:14 ******/
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

USE [soecompv2]
GO

/****** Object:  StoredProcedure [dbo].[GetTimePayrollTransactionAccountsForCompany]    Script Date: 2014-11-04 10:10:03 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


    ALTER PROCEDURE [dbo].[GetTimePayrollTransactionAccountsForCompany]
		@actorCompanyId INT,
		@startDate DateTime = NULL,
		@stopDate DateTime = NULL,
		@timePeriodId INT = NULL
	WITH RECOMPILE
    AS
    BEGIN
    SET NOCOUNT ON;

    SELECT
		--TimePayrollTransaction
		tpt.ActorCompanyId,
		tpt.EmployeeId,
		tpt.TimePeriodId,
		--TimePayrollTransactionAccount
		tptacc.TimePayrollTransactionId,
		tptacc.AccountId,	
		--	Account
		acc.AccountNr,
		acc.Name,
		--AccountDim
		ad.AccountDimId,
		ad.AccountDimNr,
		ad.Name as AccountDimName
    FROM
		TimePayrollTransactionAccount as tptacc WITH (NOLOCK) INNER JOIN
		TimePayrollTransaction as tpt WITH (NOLOCK) ON tptacc.TimePayrollTransactionId = tpt.TimePayrollTransactionId INNER JOIN
		TimeBlockDate as tbd WITH (NOLOCK) ON tpt.TimeBlockDateId = tbd.TimeBlockDateId INNER JOIN
		Account as acc WITH (NOLOCK) ON tptacc.AccountId = acc.AccountId INNER JOIN
		AccountDim as ad WITH (NOLOCK) ON acc.AccountDimId = ad.AccountDimId
    WHERE
		(tpt.ActorCompanyId = @actorCompanyId) AND
		(tpt.State = 0) AND
		(
		(@timePeriodId IS NOT NULL AND @timePeriodId = tpt.TimePeriodId)
		OR
		((tpt.TimePeriodId IS NULL OR @timePeriodId IS NULL) AND (@startDate IS NOT NULL AND @stopDate IS NOT NULL AND tbd.Date BETWEEN @startDate AND @stopDate))
		)
    ORDER BY
		tptacc.TimePayrollTransactionId, tptacc.AccountId

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


USE [soecompv2]
GO

/****** Object:  StoredProcedure [dbo].[GetTimePayrollTransactionAccountsForEmployee]    Script Date: 2014-11-04 10:10:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


    ALTER PROCEDURE [dbo].[GetTimePayrollTransactionAccountsForEmployee]
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
		tpt.ActorCompanyId,
		tpt.EmployeeId,
		tpt.TimePeriodId,
		--TimePayrollTransactionAccount
		tptacc.TimePayrollTransactionId,
		tptacc.AccountId,
		--Account
		acc.AccountNr,
		acc.Name,
		--AccountDim
		ad.AccountDimId,
		ad.AccountDimNr,
		ad.Name as AccountDimName
    FROM
		TimePayrollTransactionAccount as tptacc WITH (NOLOCK) INNER JOIN
		TimePayrollTransaction as tpt WITH (NOLOCK) ON tptacc.TimePayrollTransactionId = tpt.TimePayrollTransactionId INNER JOIN
		TimeBlockDate as tbd WITH (NOLOCK) ON tpt.TimeBlockDateId = tbd.TimeBlockDateId INNER JOIN
		Account as acc WITH (NOLOCK) ON tptacc.AccountId = acc.AccountId INNER JOIN
		AccountDim as ad WITH (NOLOCK) ON acc.AccountDimId = ad.AccountDimId
    WHERE
		(tbd.EmployeeId = @employeeId) AND
		(tpt.State = 0) AND
		(
		(@timePeriodId IS NOT NULL AND @timePeriodId = tpt.TimePeriodId)
		OR
		((tpt.TimePeriodId IS NULL OR @timePeriodId IS NULL) AND (@startDate IS NOT NULL AND @stopDate IS NOT NULL AND tbd.Date BETWEEN @startDate AND @stopDate))
		)
    ORDER BY
		tptacc.TimePayrollTransactionId, tptacc.AccountId

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



USE [soecompv2]
GO
/****** Object:  StoredProcedure [dbo].[GetTimePayrollTransactionsWithAccIntsForEmployee]    Script Date: 2014-11-05 08:28:55 ******/
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



USE [soecompv2]
GO

/****** Object:  StoredProcedure [dbo].[GetTimePayrollScheduleTransactionsWithAccIntsForEmployee]    Script Date: 2014-11-04 16:30:36 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE PROCEDURE [dbo].[GetTimePayrollScheduleTransactionsWithAccIntsForEmployee]
	@employeeId						INT,
	@actorCompanyId					INT,
	@type							INT,
	@dateFrom						DATETIME,
	@dateTo							DATETIME
WITH RECOMPILE
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT 
		--TimePayrollScheduleTransaction
		tpst.TimePayrollScheduleTransactionId,
		tpst.ActorCompanyId,
		tpst.EmployeeId,		
		tpst.TimeBlockDateId,
		tpst.ProductId,
		tpst.AccountId,
		tpst.TimeScheduleTemplatePeriodId,
		tpst.TimeScheduleTemplateBlockId,
		tpst.PayrollPriceFormulaId,
		tpst.PayrollPriceTypeId,
		tpst.Type,
		tpst.Quantity,
		tpst.Amount,
		tpst.AmountCurrency,
		tpst.AmountLedgerCurrency,
		tpst.AmountEntCurrency,
		tpst.VatAmount,
		tpst.VatAmountCurrency,
		tpst.VatAmountLedgerCurrency,
		tpst.VatAmountEntCurrency,
		tpst.UnitPrice,
		tpst.UnitPriceCurrency,
		tpst.UnitPriceLedgerCurrency,
		tpst.UnitPriceEntCurrency,
		tpst.SysPayrollTypeLevel1 as TransactionSysPayrollTypeLevel1,
		tpst.SysPayrollTypeLevel2 as TransactionSysPayrollTypeLevel2,
		tpst.SysPayrollTypeLevel3 as TransactionSysPayrollTypeLevel3,
		tpst.SysPayrollTypeLevel4 as TransactionSysPayrollTypeLevel4,
		tpst.TimeBlockStartTime,
		tpst.TimeBlockStopTime,
		tpst.Formula,
		tpst.FormulaPlain,
		tpst.FormulaExtracted,
		tpst.FormulaNames,
		tpst.FormulaOrigin,
		tpst.Created,
		tpst.CreatedBy,
		--Product
		p.Number as ProductNumber,
		p.Name as ProductName,
		--PayrollProduct
		pp.Export as PayrollProductExport,
		pp.SysPayrollTypeLevel1 as PayrollProductSysPayrollTypeLevel1,
		pp.SysPayrollTypeLevel2 as PayrollProductSysPayrollTypeLevel2,
		pp.SysPayrollTypeLevel3 as PayrollProductSysPayrollTypeLevel3,
		pp.SysPayrollTypeLevel4 as PayrollProductSysPayrollTypeLevel4,
		--Accounting
		acc.AccountId AS AccountStdId,
		acc.AccountNr AS AccountStdNr,
		acc.Name AS AccountStdName,
		LEFT(o.list,LEN(o.list) - 1) AS AccountInternalsStr,
		--TimeBlockDate
		tbd.Date
	FROM 
		dbo.TimePayrollScheduleTransaction AS tpst WITH (NOLOCK)INNER JOIN 
		dbo.Product AS p WITH (NOLOCK) ON p.ProductId = tpst.ProductId INNER JOIN 
		dbo.PayrollProduct AS pp WITH (NOLOCK) ON pp.ProductId = p.ProductId INNER JOIN 
		dbo.Account AS acc WITH (NOLOCK) ON acc.AccountId = tpst.AccountId INNER JOIN 
		dbo.TimeBlockDate AS tbd WITH (NOLOCK) ON tbd.TimeBlockDateId = tpst.TimeBlockDateId CROSS APPLY 
		(
			SELECT   
				CONVERT(VARCHAR(100),tptad.AccountDimNr) + ':' + CONVERT(VARCHAR(100),tacc.AccountId) + ':' + CONVERT(VARCHAR(100),tacc.AccountNr) + ':' + CONVERT(VARCHAR(100),tacc.Name) + ',' AS [text()]
			FROM
				dbo.TimePayrollScheduleTransactionAccount AS ta WITH (NOLOCK) INNER JOIN 
				dbo.Account AS tacc WITH (NOLOCK) ON tacc.AccountId = ta.AccountId INNER JOIN 
				dbo.AccountDim AS tptad WITH (NOLOCK) ON tptad.AccountDimId = tacc.AccountDimId
			WHERE    
				ta.TimePayrollScheduleTransactionId = tpst.TimePayrollScheduleTransactionId and tptad.AccountDimNr > 1
			ORDER BY
				tptad.AccountDimNr
			FOR XML PATH('')
		 ) o(list)	
	WHERE 
		tbd.EmployeeId = @employeeId AND 
		(@type is null OR tpst.Type = @type) AND
		(tbd.Date BETWEEN @dateFrom AND @dateTo) AND
		tpst.[State] = 0
	ORDER BY
		tbd.Date

    END





GO


USE [soecompv2]
GO
/****** Object:  StoredProcedure [dbo].[GetTimePayrollTransactionsWithAccIntsForEmployee]    Script Date: 2014-11-06 08:21:02 ******/
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
		tbd.[Date] >= @dateFrom AND 
		tbd.[Date] <= @dateTo AND
		tpt.[State] = 0	AND
		ISNULL(tb.State, 0) = 0
		ORDER BY
		tb.StartTime


    END
GO


/****** Object:  StoredProcedure [dbo].[GetTimeSchedulePlanningShifts]    Script Date: 2014-11-20 07:36:59 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE PROCEDURE [dbo].[GetTimeSchedulePlanningShifts]
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
		LEFT OUTER JOIN TimeScheduleTemplatePeriod AS p ON b.TimeScheduleTemplatePeriodId = p.TimeScheduleTemplateHeadId
		LEFT OUTER JOIN TimeScheduleTemplateHead AS h ON p.TimeScheduleTemplateHeadId = h.TimeScheduleTemplateHeadId
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



/****** Object:  Table [dbo].[AttestWorkFlowGroup]    Script Date: 2014-11-17 08:39:04 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[AttestWorkFlowGroup](
	[AttestWorkFlowHeadId] [int] NOT NULL,
	[Code] [nvarchar](50) NULL,
	[Name] [varchar](100) NULL,
 CONSTRAINT [PK_AttestWorkFlowGroup] PRIMARY KEY CLUSTERED 
(
	[AttestWorkFlowHeadId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[AttestWorkFlowGroup]  WITH CHECK ADD  CONSTRAINT [FK_AttestWorkFlowGroup_AttestWorkFlowHead] FOREIGN KEY([AttestWorkFlowHeadId])
REFERENCES [dbo].[AttestWorkFlowHead] ([AttestWorkFlowHeadId])
GO

ALTER TABLE [dbo].[AttestWorkFlowGroup] CHECK CONSTRAINT [FK_AttestWorkFlowGroup_AttestWorkFlowHead]
GO


/****** Added NoOfPrintedReminders (Task 13882) - Mattias Eriksson  ******/


GO
/****** Object:  StoredProcedure [dbo].[GetChangeStatusGridViewCI]    Script Date: 2014-11-18 17:54:01 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
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

/****** Object:  StoredProcedure [dbo].[GetDayOfIllnessNumber]    Script Date: 2014-11-21 09:23:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetDayOfIllnessNumber]
	@date				datetime,
	@maxDays			int,
	@interval			int,
	@timeCodeId			int,
	@employeeId			int,
	@daysUntilQualify	int,
	@dayOfAbsenceNumber	int output
AS
BEGIN

	DECLARE @currentDate datetime
	SET @currentDate = @date
	DECLARE @foundDate datetime
	SET @foundDate = @date

	-- When the number of qualifying days of illness is known and passed as parameter,
	-- add these days to maxDays (subtracted from the result)
	IF @daysUntilQualify IS NOT NULL
		SET @maxDays = @maxDays + @daysUntilQualify

	WHILE @foundDate IS NOT NULL
		BEGIN
			-- Check for TimeCodeTransactions with specified TimeCode for specified Employee
			SET @foundDate = NULL
			SELECT TOP 1 @foundDate = tbd.[Date]
			FROM 
				TimeCodeTransaction tct INNER JOIN 
				TimeBlock tb ON tct.TimeBlockId = tb.TimeBlockId INNER JOIN 
				TimeBlockDate tbd ON tb.TimeBlockDateId = tbd.TimeBlockDateId
			GROUP BY 
				tb.EmployeeId, 
				tct.TimeCodeId, 
				tct.[State], 
				tbd.[Date]
			HAVING 
				tb.EmployeeId = @employeeId AND
				tct.TimeCodeId = @timeCodeId AND
				tct.[State] = 0 AND
				tbd.[Date] BETWEEN (@currentDate - @interval) AND (@currentDate - 1)
			ORDER BY 
				tbd.[Date] ASC

			-- If a date was found, check again from that date and backwards (@interval number of days)
			-- Otherwise exit and use previously found date
			IF @foundDate IS NOT NULL
				SET @currentDate = @foundDate
			ELSE
				BREAK

			-- If we pass max number of days back, exit
			IF (SELECT COUNT(DISTINCT tbd.[Date])
				FROM 
					TimeCodeTransaction tct INNER JOIN 
					TimeBlock tb ON tct.TimeBlockId = tb.TimeBlockId INNER JOIN 
					TimeBlockDate tbd ON tb.TimeBlockDateId = tbd.TimeBlockDateId
				WHERE
					tb.EmployeeId = @employeeId AND
					tct.TimeCodeId = @timeCodeId AND
					tct.[State] = 0 AND
					tbd.[Date] BETWEEN (@currentDate) AND (@date - 1)) > @maxDays
				BREAK
		END

	IF @currentDate = @date
		SET @dayOfAbsenceNumber = 1
	ELSE
		BEGIN
			SET @dayOfAbsenceNumber = (SELECT COUNT(DISTINCT tbd.[Date]) + 1
				FROM 
					TimeCodeTransaction tct INNER JOIN 
					TimeBlock tb ON tct.TimeBlockId = tb.TimeBlockId INNER JOIN 
					TimeBlockDate tbd ON tb.TimeBlockDateId = tbd.TimeBlockDateId
				WHERE
					tb.EmployeeId = @employeeId AND
					tct.TimeCodeId = @timeCodeId AND
					tct.[State] = 0 AND
					tbd.[Date] BETWEEN (@currentDate) AND (@date - 1))
		END

	-- Check if absence starts on a 'non schedule' day
	-- In that case set @daysUntilQualify and recursively call procedure again
	IF @daysUntilQualify IS NOT NULL
		SET @dayOfAbsenceNumber = @dayOfAbsenceNumber - @daysUntilQualify
	ELSE
		BEGIN
			SET @daysUntilQualify = 0
			-- Loop until a schedule day is found (template blocks with different start and stop times)
			WHILE @currentDate <= @date
				BEGIN
					IF (SELECT COUNT(*)
					FROM
						TimeScheduleTemplateBlock b
					WHERE
						b.EmployeeId = @employeeId AND
						b.Date = @currentDate AND
						b.StartTime <> b.StopTime AND
						b.State = 0) > 0
					BREAK

					SET @daysUntilQualify = @daysUntilQualify + 1
					SET @currentDate = DATEADD(day, 1, @currentDate)
				END

			IF @daysUntilQualify > 0
				EXEC GetDayOfIllnessNumber @date, @maxDays, @interval, @timeCodeId, @employeeId, @daysUntilQualify, @dayOfAbsenceNumber
		END

	SELECT @dayOfAbsenceNumber
END



/*
   den 24 november 201409:33:53
   User: dba
   Server: extra01\xedev
   Database: soecompv2
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
ALTER TABLE dbo.Category
	DROP CONSTRAINT FK_Category_Company
GO
ALTER TABLE dbo.Company SET (LOCK_ESCALATION = TABLE)
GO

select Has_Perms_By_Name(N'dbo.Company', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Company', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Company', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.Category
	DROP CONSTRAINT DF_Category_State
GO
CREATE TABLE dbo.Tmp_Category
	(
	CategoryId int NOT NULL IDENTITY (1, 1),
	ActorCompanyId int NOT NULL,
	ParentId int NULL,
	Type int NOT NULL,
	Code nvarchar(50) NULL,
	Name nvarchar(100) NOT NULL,
	Created datetime NULL,
	CreatedBy nvarchar(50) NULL,
	Modified datetime NULL,
	ModifiedBy nvarchar(50) NULL,
	State int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_Category SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_Category ADD CONSTRAINT
	DF_Category_State DEFAULT ((0)) FOR State
GO
SET IDENTITY_INSERT dbo.Tmp_Category ON
GO
IF EXISTS(SELECT * FROM dbo.Category)
	 EXEC('INSERT INTO dbo.Tmp_Category (CategoryId, ActorCompanyId, ParentId, Type, Code, Name, State)
		SELECT CategoryId, ActorCompanyId, ParentId, Type, Code, Name, State FROM dbo.Category WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_Category OFF
GO
ALTER TABLE dbo.CategoryAccount
	DROP CONSTRAINT FK_CategoryAccount_Category
GO
ALTER TABLE dbo.CompanyCategoryRecord
	DROP CONSTRAINT FK_CompanyCategoryRecord_Category
GO
ALTER TABLE dbo.Category
	DROP CONSTRAINT FK_Category_Parent
GO
DROP TABLE dbo.Category
GO
EXECUTE sp_rename N'dbo.Tmp_Category', N'Category', 'OBJECT' 
GO
ALTER TABLE dbo.Category ADD CONSTRAINT
	PK_Category PRIMARY KEY CLUSTERED 
	(
	CategoryId
	) WITH( PAD_INDEX = OFF, FILLFACTOR = 90, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX XE_Q_ActorCompanyId_State ON dbo.Category
	(
	ActorCompanyId,
	State
	) WITH( PAD_INDEX = OFF, FILLFACTOR = 90, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX XE_Q_ActorCompanyId_Type_State ON dbo.Category
	(
	ActorCompanyId,
	Type,
	State
	) WITH( PAD_INDEX = OFF, FILLFACTOR = 90, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX PK_Categoryid ON dbo.Category
	(
	CategoryId
	) WITH( PAD_INDEX = OFF, FILLFACTOR = 90, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX XE_Stats_ParentId ON dbo.Category
	(
	ParentId
	) INCLUDE (CategoryId, Code, Name, Type, ActorCompanyId, State) 
 WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE dbo.Category ADD CONSTRAINT
	FK_Category_Company FOREIGN KEY
	(
	ActorCompanyId
	) REFERENCES dbo.Company
	(
	ActorCompanyId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Category ADD CONSTRAINT
	FK_Category_Parent FOREIGN KEY
	(
	ParentId
	) REFERENCES dbo.Category
	(
	CategoryId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO

select Has_Perms_By_Name(N'dbo.Category', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Category', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Category', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.CompanyCategoryRecord ADD CONSTRAINT
	FK_CompanyCategoryRecord_Category FOREIGN KEY
	(
	CategoryId
	) REFERENCES dbo.Category
	(
	CategoryId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.CompanyCategoryRecord SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.CompanyCategoryRecord', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.CompanyCategoryRecord', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.CompanyCategoryRecord', 'Object', 'CONTROL') as Contr_Per BEGIN TRANSACTION
GO
ALTER TABLE dbo.CategoryAccount ADD CONSTRAINT
	FK_CategoryAccount_Category FOREIGN KEY
	(
	CategoryId
	) REFERENCES dbo.Category
	(
	CategoryId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.CategoryAccount SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.CategoryAccount', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.CategoryAccount', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.CategoryAccount', 'Object', 'CONTROL') as Contr_Per 

------------------------------------------------------------
-- This script is generated by SQLDBDiff V4.0
-- http://www.sqldbtools.com 
-- 2014-11-26 10:24:26
-- Note : 
--   Script generated by SQLDBDiff might need some adjustment, please review and test the code against test environements before deployement in production. 
------------------------------------------------------------

USE [soecompv2]
GO

---$ Alter table dbo.AccountDistributionEntry
IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE OBJECT_ID = OBJECT_ID(N'dbo.AccountDistributionEntry') AND NAME = 'State')
BEGIN
    PRINT 'Add column : dbo.AccountDistributionEntry.State'
    ALTER TABLE dbo.AccountDistributionEntry
        ADD [State] INT NOT NULL DEFAULT (0)
END
GO


---$ Create table dbo.CustomerInvoicePrintedReminder
IF OBJECT_ID(N'dbo.CustomerInvoicePrintedReminder') IS NULL
BEGIN
    PRINT 'Create table dbo.CustomerInvoicePrintedReminder'
    CREATE TABLE dbo.CustomerInvoicePrintedReminder
    (
        CustomerInvoicePrintedReminderId INT IDENTITY(1,1) NOT NULL,
        ActorCompanyId INT NOT NULL,
        CustomerInvoiceOriginId INT NOT NULL,
        InvoiceNr NVARCHAR(50) NOT NULL,
        Amount DECIMAL(10,2) NOT NULL DEFAULT (0),
        ReminderDate datetime NOT NULL,
        DueDate datetime NOT NULL,
        NoOfReminder INT NOT NULL DEFAULT (1),
        Created datetime NULL,
        CreatedBy NVARCHAR(50) NULL
    )
    END
GO


---$ Alter table dbo.TimeStampEntry
IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE OBJECT_ID = OBJECT_ID(N'dbo.TimeStampEntry') AND NAME = 'EmployeeChildId')
BEGIN
    PRINT 'Add column : dbo.TimeStampEntry.EmployeeChildId'
    ALTER TABLE dbo.TimeStampEntry
        ADD EmployeeChildId INT NULL
END
GO


---$ Alter table dbo.TimeStampEntryRaw
IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE OBJECT_ID = OBJECT_ID(N'dbo.TimeStampEntryRaw') AND NAME = 'EmployeeChildRecordId')
BEGIN
    PRINT 'Add column : dbo.TimeStampEntryRaw.EmployeeChildRecordId'
    ALTER TABLE dbo.TimeStampEntryRaw
        ADD EmployeeChildRecordId INT NULL
END
GO


---$ Drop table dbo.StockTransActions
IF OBJECT_ID(N'dbo.StockTransActions') is not null
BEGIN
    PRINT 'Drop table dbo.StockTransActions'
    DROP TABLE dbo.StockTransActions
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
                         /*Customer invoice*/ ci.InvoiceHeadText, ci.RegistrationType, ci.DeliveryAddressId, ci.BillingAddressId, ci.BillingInvoicePrinted, ci.HasHouseholdTaxDeduction, 
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

---$ Alter View dbo.InvoiceTraceView 
IF OBJECT_ID(N'dbo.InvoiceTraceView') IS NULL
BEGIN
    PRINT 'Create View : dbo.InvoiceTraceView'
    exec('create view dbo.InvoiceTraceView as select null as Col1') 
END
GO

PRINT 'Alter view : dbo.InvoiceTraceView'
GO

SET QUOTED_IDENTIFIER ON 
go

SET ANSI_NULLS ON 
go


ALTER VIEW [dbo].[InvoiceTraceView]
AS

/*Contract*/ 
SELECT TOP 100 PERCENT 
	/*Invoice*/ i_inv.InvoiceId, 
	/*Contract*/ ISNULL(CAST(1 AS bit), 1) AS IsContract, o_con.OriginId AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS MappedInvoiceId, 
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, CAST('' AS nvarchar) AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName, '' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ o_con.Description, o_con.Type AS OriginType, st_oType.Name AS OriginTypeName, o_con.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, 
	/*Common*/ i_con.BillingType, st_iBillingType.Name AS BillingTypeName, i_con.InvoiceNr AS Number, ISNULL(CAST(i_con.TotalAmount AS decimal(10, 2)), 0) AS Amount, ISNULL(CAST(i_con.TotalAmountCurrency AS decimal(10, 2)), 0) AS AmountCurrency, i_con.VATAmount AS VatAmount, i_con.VATAmountCurrency AS VatAmountCurrency, i_con.InvoiceDate AS Date, i_con.State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, i_con.CurrencyRate, cu.SysCurrencyId, 
	/*Lang*/ st_oType.LangId AS LangId
FROM         
	dbo.Invoice AS i_inv WITH (NOLOCK) INNER JOIN
	dbo.Origin AS o_inv WITH (NOLOCK) ON o_inv.OriginId = i_inv.InvoiceId INNER JOIN
	dbo.OriginInvoiceMapping AS orm_inv WITH (NOLOCK) ON orm_inv.InvoiceId = i_inv.InvoiceId INNER JOIN
	dbo.Origin AS o_con WITH (NOLOCK) ON o_con.OriginId = orm_inv.OriginId INNER JOIN
	dbo.Invoice AS i_con WITH (NOLOCK) ON i_con.InvoiceId = o_con.OriginId INNER JOIN
	dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_inv.CurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o_con.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o_con.Status INNER JOIN
	SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i_con.BillingType INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
WHERE     
	i_inv.State = 0 AND 
	o_inv.Type = 2 AND 
	o_con.Type = 7 AND 
	st_oStatus.SysTermGroupId = 30 AND 
	st_oType.SysTermGroupId = 31 AND 
	st_iBillingType.SysTermGroupId = 27 AND 
	st_oStatus.LangId = st_oType.LangId AND 
	st_iBillingType.LangId = st_oType.LangId
	
UNION

/*Offer*/ 
SELECT TOP 100 PERCENT 
	/*Invoice*/ i_inv.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(1 AS bit), 1) AS IsOffer,orm_inv.OriginId AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS MappedInvoiceId, 
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, CAST('' AS nvarchar) AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName, '' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ o_off.Description, o_off.Type AS OriginType, st_oType.Name AS OriginTypeName, o_off.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, 
	/*Common*/ i_off.BillingType, st_iBillingType.Name AS BillingTypeName, i_off.InvoiceNr AS Number, ISNULL(CAST(i_off.TotalAmount AS decimal(10, 2)), 0) AS Amount, ISNULL(CAST(i_off.TotalAmount AS decimal(10, 2)), 0) AS AmountCurrency, i_off.VATAmount AS VatAmount, i_off.VATAmountCurrency AS VatAmountCurrency, i_off.InvoiceDate AS Date, i_off.State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, i_off.CurrencyRate, cu.SysCurrencyId, 
	/*Lang*/ st_oType.LangId AS LangId
FROM        
	dbo.Invoice AS i_inv WITH (NOLOCK) INNER JOIN
	dbo.Origin AS o_inv WITH (NOLOCK) ON o_inv.OriginId = i_inv.InvoiceId INNER JOIN
	dbo.OriginInvoiceMapping AS orm_inv WITH (NOLOCK) ON orm_inv.InvoiceId = i_inv.InvoiceId INNER JOIN
	dbo.Origin AS o_off WITH (NOLOCK) ON o_off.OriginId = orm_inv.OriginId INNER JOIN
	dbo.Invoice AS i_off WITH (NOLOCK) ON i_off.InvoiceId = o_off.OriginId INNER JOIN
	dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_inv.CurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o_off.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o_off.Status INNER JOIN
	SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i_off.BillingType INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
WHERE     
	i_inv.State = 0 AND 
	o_inv.Type = 2 AND 
	o_off.Type = 5 AND 
	st_oStatus.SysTermGroupId = 30 AND 
	st_oType.SysTermGroupId = 31 AND 
	st_iBillingType.SysTermGroupId = 27 AND 
	st_oStatus.LangId = st_oType.LangId AND 
	st_iBillingType.LangId = st_oType.LangId
	
UNION

/*Order*/ 
SELECT TOP 100 PERCENT 
	/*Invoice*/ i_inv.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(1 AS bit), 1) AS IsOrder, orm_inv.OriginId AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS MappedInvoiceId, 
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId,
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, '' AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName, '' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ o_ord.Description, o_ord.Type AS OriginType, st_oType.Name AS OriginTypeName, o_ord.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, 
	/*Common*/ i_ord.BillingType, st_iBillingType.Name AS BillingTypeName, i_ord.InvoiceNr AS Number, ISNULL(CAST(i_ord.TotalAmount AS decimal(10, 2)), 0) AS Amount, ISNULL(CAST(i_ord.TotalAmountCurrency AS decimal(10, 2)), 0) AS AmountCurrency, i_ord.VATAmount AS VatAmount, i_ord.VATAmountCurrency AS VatAmountCurrency, i_ord.InvoiceDate AS Date, i_ord.State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, i_ord.CurrencyRate, cu.SysCurrencyId, /*Lang*/ st_oType.LangId AS LangId
FROM         
	dbo.Invoice AS i_inv WITH (NOLOCK) INNER JOIN
	dbo.Origin AS o_inv WITH (NOLOCK) ON o_inv.OriginId = i_inv.InvoiceId INNER JOIN
	dbo.OriginInvoiceMapping AS orm_inv WITH (NOLOCK) ON orm_inv.InvoiceId = i_inv.InvoiceId INNER JOIN
	dbo.Origin AS o_ord WITH (NOLOCK) ON o_ord.OriginId = orm_inv.OriginId INNER JOIN
	dbo.Invoice AS i_ord WITH (NOLOCK) ON i_ord.InvoiceId = o_ord.OriginId INNER JOIN
	dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_inv.CurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o_ord.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o_ord.Status INNER JOIN
	SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i_ord.BillingType INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
WHERE     
	i_inv.State = 0 AND 
	o_inv.Type = 2 AND 
	o_ord.Type = 6 AND 
	st_oStatus.SysTermGroupId = 30 AND 
	st_oType.SysTermGroupId = 31 AND 
	st_iBillingType.SysTermGroupId = 27 AND 
	st_oStatus.LangId = st_oType.LangId AND 
	st_iBillingType.LangId = st_oType.LangId
	
UNION

/*Mapped Invoice*/ 
SELECT TOP 100 PERCENT 
	/*Invoice*/ i_inv.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit),  0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(1 AS bit), 1) AS IsInvoice, orm_inv.OriginId AS MappedInvoiceId, 
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, '' AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName,'' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ o_mapinv.Description, o_mapinv.Type AS OriginType, st_oType.Name AS OriginTypeName, o_mapinv.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, 
	/*Common*/ i_mapinv.BillingType, st_iBillingType.Name AS BillingTypeName, i_mapinv.InvoiceNr AS Number, ISNULL(CAST(i_mapinv.TotalAmount AS decimal(10, 2)), 0) AS Amount, ISNULL(CAST(i_mapinv.TotalAmountCurrency AS decimal(10, 2)), 0) AS AmountCurrency, i_mapinv.VATAmount AS VatAmount, i_mapinv.VATAmountCurrency AS VatAmountCurrency, i_mapinv.InvoiceDate AS Date, i_mapinv.State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, i_mapinv.CurrencyRate, cu.SysCurrencyId, 
	/*Lang*/ st_oType.LangId AS LangId
FROM         
	dbo.Invoice AS i_inv WITH (NOLOCK) INNER JOIN
	dbo.Origin AS o_inv WITH (NOLOCK) ON o_inv.OriginId = i_inv.InvoiceId INNER JOIN
	dbo.OriginInvoiceMapping AS orm_inv WITH (NOLOCK) ON orm_inv.InvoiceId = i_inv.InvoiceId INNER JOIN
	dbo.Origin AS o_mapinv WITH (NOLOCK) ON o_mapinv.OriginId = orm_inv.OriginId INNER JOIN
	dbo.Invoice AS i_mapinv WITH (NOLOCK) ON i_mapinv.InvoiceId = o_mapinv.OriginId INNER JOIN
	dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_inv.CurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o_mapinv.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o_mapinv.Status INNER JOIN
	SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i_mapinv.BillingType INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
WHERE     
	i_inv.State = 0 AND 
	o_inv.Type = 2 AND 
	o_mapinv.Type = 2 AND 
	st_oStatus.SysTermGroupId = 30 AND 
	st_oType.SysTermGroupId = 31 AND 
	st_iBillingType.SysTermGroupId = 27 AND 
	st_oStatus.LangId = st_oType.LangId AND 
	st_iBillingType.LangId = st_oType.LangId
	
UNION

/*Mapped Invoice reversed*/ 
SELECT TOP 100 PERCENT 
	/*Invoice*/ i_inv.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(1 AS bit), 1) AS IsInvoice, orm_inv.InvoiceId AS MappedInvoiceId, 
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, '' AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName,'' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ o_mapinv.Description, o_mapinv.Type AS OriginType, st_oType.Name AS OriginTypeName, o_mapinv.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, 
	/*Common*/ i_mapinv.BillingType, st_iBillingType.Name AS BillingTypeName, i_mapinv.InvoiceNr AS Number, ISNULL(CAST(i_mapinv.TotalAmount AS decimal(10, 2)), 0) AS Amount, ISNULL(CAST(i_mapinv.TotalAmountCurrency AS decimal(10, 2)), 0) AS AmountCurrency, i_mapinv.VATAmount AS VatAmount, i_mapinv.VATAmountCurrency AS VatAmountCurrency, i_mapinv.InvoiceDate AS Date, i_mapinv.State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, i_mapinv.CurrencyRate, cu.SysCurrencyId, 
	/*Lang*/ st_oType.LangId AS LangId
FROM         
	dbo.Invoice AS i_inv WITH (NOLOCK) INNER JOIN
	dbo.Origin AS o_inv WITH (NOLOCK) ON o_inv.OriginId = i_inv.InvoiceId INNER JOIN
	dbo.OriginInvoiceMapping AS orm_inv WITH (NOLOCK) ON orm_inv.OriginId = i_inv.InvoiceId INNER JOIN
	dbo.Origin AS o_mapinv WITH (NOLOCK) ON o_mapinv.OriginId = orm_inv.InvoiceId INNER JOIN
	dbo.Invoice AS i_mapinv WITH (NOLOCK) ON i_mapinv.InvoiceId = o_mapinv.OriginId INNER JOIN
	dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_inv.CurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o_mapinv.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o_mapinv.Status INNER JOIN
	SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i_mapinv.BillingType INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
WHERE     
	i_inv.State = 0 AND 
	o_inv.Type = 2 AND 
	o_mapinv.Type = 2 AND 
	st_oStatus.SysTermGroupId = 30 
	AND st_oType.SysTermGroupId = 31 AND 
	st_iBillingType.SysTermGroupId = 27 AND 
	st_oStatus.LangId = st_oType.LangId AND 
	st_iBillingType.LangId = st_oType.LangId
	
UNION

/*Orginal invoices from CustomerInvoiceReminder*/ 
SELECT 
	/*Invoice*/ i_rem.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(1 AS bit), 1) AS IsInvoice, i_origin.InvoiceId AS MappedInvoiceId, 
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, '' AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName,'' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ o_origin.Description, o_origin.Type AS OriginType, st_oType.Name AS OriginTypeName, o_origin.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, 
	/*Common*/ i_origin.BillingType, st_iBillingType.Name AS BillingTypeName, i_origin.InvoiceNr AS Number, ISNULL(CAST(i_origin.TotalAmount AS decimal(10, 2)), 0) AS Amount, ISNULL(CAST(i_origin.TotalAmountCurrency AS decimal(10, 2)), 0) AS AmountCurrency, i_origin.VATAmount AS VatAmount, i_origin.VATAmountCurrency AS VatAmountCurrency, i_origin.InvoiceDate AS Date, i_origin.State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, i_origin.CurrencyRate, cu.SysCurrencyId, 
	/*Lang*/ st_oType.LangId AS LangId
FROM         
	Invoice AS i_rem WITH (NOLOCK) INNER JOIN
	CustomerInvoice AS ci_rem WITH (NOLOCK) ON ci_rem.InvoiceId = i_rem.InvoiceId INNER JOIN
	Origin AS o_rem WITH (NOLOCK) ON o_rem.OriginId = i_rem.InvoiceId INNER JOIN
	CustomerInvoiceRow AS cir_rem WITH (NOLOCK) ON cir_rem.InvoiceId = ci_rem.InvoiceId INNER JOIN
	CustomerInvoiceReminder AS rem WITH (NOLOCK) ON rem.CustomerInvoiceReminderId = cir_rem.CustomerInvoiceReminderId INNER JOIN
	Invoice AS i_origin WITH (NOLOCK) ON i_origin.InvoiceId = rem.CustomerInvoiceOriginId INNER JOIN
	Origin AS o_origin WITH (NOLOCK) ON o_origin.OriginId = i_origin.InvoiceId INNER JOIN
	Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_origin.CurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o_origin.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o_origin.Status INNER JOIN
	SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i_origin.BillingType INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
WHERE     
	(i_rem.BillingType = 4 OR i_origin.BillingType = 4) AND 
	i_rem.State = 0 AND 
	o_origin.Type = 2 AND 
	st_oStatus.SysTermGroupId = 30 AND 
	st_oType.SysTermGroupId = 31 AND 
	st_iBillingType.SysTermGroupId = 27 AND 
	st_oStatus.LangId = st_oType.LangId AND 
	st_iBillingType.LangId = st_oType.LangId
	
UNION

/*Orginal invoices from CustomerInvoiceInterest*/ 
SELECT 
	/*Invoice*/ i_intr.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(1 AS bit), 1) AS IsInvoice, i_origin.InvoiceId AS MappedInvoiceId, 
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, '' AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName,'' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ o_origin.Description, o_origin.Type AS OriginType, st_oType.Name AS OriginTypeName, o_origin.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, 
	/*Common*/ i_origin.BillingType, st_iBillingType.Name AS BillingTypeName, i_origin.InvoiceNr AS Number, ISNULL(CAST(i_origin.TotalAmount AS decimal(10, 2)), 0) AS Amount, ISNULL(CAST(i_origin.TotalAmountCurrency AS decimal(10, 2)), 0) AS AmountCurrency, i_origin.VATAmount AS VatAmount, i_origin.VATAmountCurrency AS VatAmountCurrency, i_origin.InvoiceDate AS Date, i_origin.State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, i_origin.CurrencyRate, cu.SysCurrencyId, 
	/*Lang*/ st_oType.LangId AS LangId
FROM         
	Invoice AS i_intr WITH (NOLOCK) INNER JOIN
	CustomerInvoiceRow AS cir_intr WITH (NOLOCK) ON cir_intr.InvoiceId = i_intr.InvoiceId INNER JOIN
	CustomerInvoiceInterest AS intr WITH (NOLOCK) ON intr.CustomerInvoiceInterestId = cir_intr.CustomerInvoiceInterestId INNER JOIN
	Invoice AS i_origin WITH (NOLOCK) ON i_origin.InvoiceId = intr.CustomerInvoiceOriginId INNER JOIN
	Origin AS o_origin WITH (NOLOCK) ON o_origin.OriginId = i_origin.InvoiceId INNER JOIN
	Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_origin.CurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o_origin.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o_origin.Status INNER JOIN
	SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i_origin.BillingType INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
WHERE     
	i_intr.BillingType = 3 AND 
	i_intr.State = 0 AND 
	o_origin.Type = 2 AND 
	st_oStatus.SysTermGroupId = 30 AND 
	st_oType.SysTermGroupId = 31 AND 
	st_iBillingType.SysTermGroupId = 27 AND 
	st_oStatus.LangId = st_oType.LangId AND 
	st_iBillingType.LangId = st_oType.LangId
	
UNION

/*CustomerInvoiceReminder*/ 
SELECT 
	/*Invoice*/ i_origin.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS MappedInvoiceId,
	/*Reminder Invoice*/ ISNULL(CAST(1 AS bit), 1) AS IsReminderInvoice, i_rem.InvoiceId AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, '' AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName, '' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ o_rem.Description, o_rem.Type AS OriginType, st_oType.Name AS OriginTypeName, o_rem.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, 
	/*Common*/ i_rem.BillingType, st_iBillingType.Name AS BillingTypeName, i_rem.InvoiceNr AS Number, ISNULL(CAST(i_rem.TotalAmount AS decimal(10, 2)), 0) AS Amount, ISNULL(CAST(i_rem.TotalAmountCurrency AS decimal(10, 2)), 0) AS AmountCurrency, i_rem.VATAmount AS VatAmount, i_rem.VATAmountCurrency AS VatAmountCurrency, i_rem.InvoiceDate AS Date, i_rem.State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, i_rem.CurrencyRate, cu.SysCurrencyId, 
	/*Lang*/ st_oType.LangId AS LangId
FROM         
	Invoice AS i_origin INNER JOIN
	CustomerInvoiceReminder AS rem WITH (NOLOCK) ON rem.CustomerInvoiceOriginId = i_origin.InvoiceId INNER JOIN
	CustomerInvoiceRow AS cir_rem WITH (NOLOCK) ON cir_rem.CustomerInvoiceReminderId = rem.CustomerInvoiceReminderId INNER JOIN
	Invoice AS i_rem WITH (NOLOCK) ON i_rem.InvoiceId = cir_rem.InvoiceId INNER JOIN
	Origin AS o_rem WITH (NOLOCK) ON o_rem.OriginId = i_rem.InvoiceId INNER JOIN
	Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_rem.CurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o_rem.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o_rem.Status INNER JOIN
	SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i_rem.BillingType INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
WHERE     
	i_rem.BillingType = 4 AND 
	i_origin.State = 0 AND 
	o_rem.Type = 2 AND 
	st_oStatus.SysTermGroupId = 30 AND 
	st_oType.SysTermGroupId = 31 AND 
	st_iBillingType.SysTermGroupId = 27 AND 
	st_oStatus.LangId = st_oType.LangId AND 
	st_iBillingType.LangId = st_oType.LangId
	
UNION

/*CustomerInvoiceInterest*/ 
SELECT 
	/*Invoice*/ i_origin.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS MappedInvoiceId,
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(1 AS bit), 1) AS IsInterestInvoice, i_intr.InvoiceId AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, '' AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName, '' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ o_intr.Description, o_intr.Type AS OriginType, st_oType.Name AS OriginTypeName, o_intr.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, 
	/*Common*/ i_intr.BillingType, st_iBillingType.Name AS BillingTypeName, i_intr.InvoiceNr AS Number, ISNULL(CAST(i_intr.TotalAmount AS decimal(10, 2)), 0) AS Amount, ISNULL(CAST(i_intr.TotalAmountCurrency AS decimal(10, 2)), 0) AS AmountCurrency, i_intr.VATAmount AS VatAmount,  i_intr.VATAmountCurrency AS VatAmountCurrency, i_intr.InvoiceDate AS Date, i_intr.State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, i_intr.CurrencyRate,  cu.SysCurrencyId, 
	/*Lang*/ st_oType.LangId AS LangId
FROM         
	Invoice AS i_origin WITH (NOLOCK) INNER JOIN
	CustomerInvoice AS ci_origin WITH (NOLOCK) ON ci_origin.InvoiceId = i_origin.InvoiceId INNER JOIN
	Origin AS o_origin WITH (NOLOCK) ON o_origin.OriginId = i_origin.InvoiceId INNER JOIN
	CustomerInvoiceInterest AS intr WITH (NOLOCK) ON intr.CustomerInvoiceOriginId = ci_origin.InvoiceId INNER JOIN
	CustomerInvoiceRow AS cir_intr WITH (NOLOCK) ON cir_intr.CustomerInvoiceInterestId = intr.CustomerInvoiceInterestId INNER JOIN
	Invoice AS i_intr WITH (NOLOCK) ON i_intr.InvoiceId = cir_intr.InvoiceId INNER JOIN
	Origin AS o_intr WITH (NOLOCK) ON o_intr.OriginId = i_intr.InvoiceId INNER JOIN
	Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_intr.CurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o_intr.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o_intr.Status INNER JOIN
	SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i_intr.BillingType INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
WHERE     
	i_intr.BillingType = 3 AND 
	i_origin.State = 0 AND 
	o_intr.Type = 2 AND 
	st_oStatus.SysTermGroupId = 30 AND 
	st_oType.SysTermGroupId = 31 AND 
	st_iBillingType.SysTermGroupId = 27 AND 
	st_oStatus.LangId = st_oType.LangId AND 
	st_iBillingType.LangId = st_oType.LangId
	
UNION

/*Payment*/ 
SELECT TOP 100 PERCENT 
	/*Invoice*/ i.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS MappedInvoiceId, 
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(1 AS bit), 1) AS IsPayment, pr.PaymentRowId, pr.Status AS PaymentStatusId, st_pStatus.Name AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName,'' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ o.Description, o.Type AS OriginType, st_oType.Name AS OriginTypeName, o.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, 
	/*Common*/ i.BillingType, st_iBillingType.Name AS BillingTypeName, cast(pr.SeqNr AS nvarchar(100)) AS Number, pr.Amount AS Amount, pr.AmountCurrency AS AmountCurrency, 0 AS VatAmount, 0 AS VatAmountCurrency, pr.PayDate AS Date, pr.State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, pr.CurrencyRate, cu.SysCurrencyId, 
	/*Lang*/ st_oType.LangId AS LangId
FROM         
	dbo.Invoice AS i INNER JOIN
	dbo.PaymentRow AS pr WITH (NOLOCK) ON pr.InvoiceId = i.InvoiceId INNER JOIN
	dbo.Payment AS p WITH (NOLOCK) ON p.PaymentId = pr.PaymentId INNER JOIN
	dbo.Origin AS o WITH (NOLOCK) ON p.PaymentId = o.OriginId INNER JOIN
	dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i.CurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o.Status INNER JOIN
	SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i.BillingType INNER JOIN
	SoesysV2.dbo.SysTerm AS st_pStatus WITH (NOLOCK) ON st_pStatus.SysTermId = pr.Status INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
WHERE     
	i.State = 0 AND 
	ISNULL(pr.PaymentRowId, 0) > 0 AND 
	st_oStatus.SysTermGroupId = 30 AND 
	st_oType.SysTermGroupId = 31 AND 
	st_iBillingType.SysTermGroupId = 27 AND 
	st_pStatus.SysTermGroupId = 35 AND 
	st_oStatus.LangId = st_oType.LangId AND 
	st_iBillingType.LangId = st_oType.LangId AND 
	st_pStatus.LangId = st_oType.LangId
	
UNION

/*EDI*/ 
SELECT TOP 100 PERCENT 
	/*Invoice*/ i_inv.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS MappedInvoiceId, 
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, '' AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(1 AS bit), 1) AS IsEdi, ISNULL(i_edi.EdiEntryId, 0) AS EdiEntryId, i_edi.HasPdf AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName, '' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ '' AS Description, 0 AS OriginType, 'EDI' AS OriginTypeName, i_edi.InvoiceStatus AS OriginStatus, i_edi.InvoiceStatusName AS OriginStatusName,
	/*Common*/ 0 AS BillingType, '' AS BillingTypeName, i_edi.InvoiceNr AS Number, i_edi.Sum AS Amount, i_edi.SumCurrency AS AmountCurrency, i_edi.SumVat AS VatAmount, i_edi.SumVatCurrency AS VatAmountCurrency, ISNULL(i_edi.Date, i_inv.InvoiceDate) AS Date, i_edi.State, 
	/*Currency*/ i_edi.CurrencyCode, i_edi.CurrencyRate,  i_edi.SysCurrencyId,
	/*Lang*/ i_edi.LangId
FROM         
	dbo.Invoice AS i_inv WITH (NOLOCK) INNER JOIN
	dbo.Origin AS o_inv WITH (NOLOCK) ON o_inv.OriginId = i_inv.InvoiceId INNER JOIN
	dbo.EdiEntryView AS i_edi WITH (NOLOCK) ON i_edi.InvoiceId = i_inv.InvoiceId
WHERE     
	i_inv.State = 0 AND 
	o_inv.Type = 1 AND 
	i_edi.Type = 1
	
UNION

/*Scanning*/ 
SELECT TOP 100 PERCENT 
	/*Invoice*/ i_inv.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS MappedInvoiceId, 
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, '' AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(1 AS bit), 1) AS IsEdi, ISNULL(i_edi.EdiEntryId, 0) AS EdiEntryId, i_edi.HasPdf AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName, '' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ '' AS Description, 0 AS OriginType, 'Scanning' AS OriginTypeName, i_edi.InvoiceStatus AS OriginStatus, i_edi.InvoiceStatusName AS OriginStatusName,
	/*Common*/ 0 AS BillingType, '' AS BillingTypeName, i_edi.InvoiceNr AS Number, i_edi.Sum AS Amount, i_edi.SumCurrency AS AmountCurrency, i_edi.SumVat AS VatAmount, i_edi.SumVatCurrency AS VatAmountCurrency, ISNULL(i_edi.Date, i_inv.InvoiceDate) AS Date, i_edi.State, 
	/*Currency*/ i_edi.CurrencyCode, i_edi.CurrencyRate,  i_edi.SysCurrencyId,
	/*Lang*/ i_edi.LangId
FROM         
	dbo.Invoice AS i_inv WITH (NOLOCK) INNER JOIN
	dbo.Origin AS o_inv WITH (NOLOCK) ON o_inv.OriginId = i_inv.InvoiceId INNER JOIN
	dbo.EdiEntryView AS i_edi WITH (NOLOCK) ON i_edi.InvoiceId = i_inv.InvoiceId INNER JOIN
	dbo.ScanningEntryView as i_sca WITH (NOLOCK) ON i_sca.ScanningEntryId = i_edi.ScanningEntryId
WHERE     
	i_inv.State = 0 AND 
	o_inv.Type = 1 AND 
	i_edi.Type = 2
	
UNION

/*Voucher*/ 
SELECT TOP 100 PERCENT 
	/*Invoice*/ i.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS MappedInvoiceId, 
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, '' AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(1 AS bit), 1) AS IsVoucher, ISNULL(vh.VoucherHeadId, 0) AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName, '' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ o.Description, o.Type AS OriginType, st_oType.Name AS OriginTypeName, o.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, 	
	/*Common*/ 0 AS BillingType, '' AS BillingTypeName, ISNULL(CAST(vh.VoucherNr AS nvarchar), 0) AS Number, CAST
	((SELECT     sum(Amount)
	  FROM         VoucherRow AS vr
	  WHERE     vr.Amount > 0 AND vr.VoucherHeadId = vh.VoucherHeadId) AS decimal(10, 2)) AS Amount, CAST
	((SELECT     sum(Amount) * i.CurrencyRate
	  FROM         VoucherRow AS vr
	  WHERE     vr.Amount > 0 AND vr.VoucherHeadId = vh.VoucherHeadId) AS decimal(10, 2)) AS AmountCurrency, 0 AS VatAmount, 0 AS VatAmountCurrency, 
	vh.Date AS Date, 0 AS State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, i.CurrencyRate, cu.SysCurrencyId, /*Lang*/ st_oStatus.LangId AS LangId
FROM         
	dbo.Invoice AS i WITH (NOLOCK) INNER JOIN
	dbo.Origin AS o WITH (NOLOCK) ON o.OriginId = i.InvoiceId INNER JOIN
	dbo.VoucherHead AS vh WITH (NOLOCK) ON i.VoucherHeadId = vh.VoucherHeadId INNER JOIN
	dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i.CurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o.Status INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
WHERE     
	(i.State = 0) AND 
	(o.Type = 1 OR o.Type = 2) AND 
	(ISNULL(i.VoucherHeadId, 0) > 0) AND 
	st_oStatus.SysTermGroupId = 30 AND 
	st_oType.SysTermGroupId = 31 AND
	st_oStatus.LangId = st_oType.LangId 
	
UNION

/*Voucher2*/ 
SELECT TOP 100 PERCENT 
	/*Invoice*/ i.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS MappedInvoiceId, 
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, '' AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(1 AS bit), 1) AS IsVoucher, ISNULL(vh.VoucherHeadId, 0) AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName, '' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ o.Description, o.Type AS OriginType, st_oType.Name AS OriginTypeName, o.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, 	
	/*Common*/ 0 AS BillingType, '' AS BillingTypeName, ISNULL(CAST(vh.VoucherNr AS nvarchar), 0) AS Number, CAST
	  ((SELECT     sum(Amount)
		  FROM         VoucherRow AS vr
		  WHERE     vr.Amount > 0 AND vr.VoucherHeadId = vh.VoucherHeadId) AS decimal(10, 2)) AS Amount, CAST
	  ((SELECT     sum(Amount) * i.CurrencyRate
		  FROM         VoucherRow AS vr
		  WHERE     vr.Amount > 0 AND vr.VoucherHeadId = vh.VoucherHeadId) AS decimal(10, 2)) AS AmountCurrency, 0 AS VatAmount, 0 AS VatAmountCurrency, 
      vh.Date AS Date, 0 AS State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, i.CurrencyRate, cu.SysCurrencyId, 
	/*Lang*/ st_oStatus.LangId AS LangId
FROM         
	dbo.Invoice AS i WITH (NOLOCK) INNER JOIN
	dbo.Origin AS o WITH (NOLOCK) ON o.OriginId = i.InvoiceId INNER JOIN
	dbo.VoucherHead AS vh WITH (NOLOCK) ON i.VoucherHead2Id = vh.VoucherHeadId INNER JOIN
	dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i.CurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o.Status INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
WHERE     
	(i.State = 0) AND 
	(o.Type = 1 OR o.Type = 2) AND 
	(ISNULL(i.VoucherHead2Id, 0) > 0) AND 
	st_oStatus.SysTermGroupId = 30 AND 
	st_oType.SysTermGroupId = 31 AND
	st_oStatus.LangId = st_oType.LangId 
	
UNION

/*Inventory*/ 
SELECT TOP 100 PERCENT 
	/*invoice*/ i.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS MappedInvoiceId,
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, '' AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(1 AS bit), 1) AS IsInventory, inv.InventoryId AS InventoryId, inv.Name AS InventoryName, inv.Description AS InventoryDescription, stg_tracing.Name AS InventoryTypeName, inv.Status AS InventoryStatusId, st_invStatus.Name AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, 
	/*Origin*/ o.Description, o.Type AS OriginType, st_oType.Name AS OriginTypeName, o.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, 
	/*Common*/ i.BillingType, st_iBillingType.Name AS BillingTypeName, inv.InventoryNr AS Number, inv.PurchaseAmount AS Amount, ISNULL(CAST(inv.PurchaseAmount AS decimal(10, 2)), 0) AS AmountCurrency, 0 AS VatAmount, 0 AS VatAmountCurrency, inv.PurchaseDate AS Date, inv.State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, i.CurrencyRate, cu.SysCurrencyId, 
	/*Lang*/ st_oType.LangId AS LangId
FROM         
	dbo.Invoice AS i WITH (NOLOCK) INNER JOIN
	dbo.Origin AS o WITH (NOLOCK) ON i.InvoiceId = o.OriginId INNER JOIN
	dbo.InventoryLog AS invl WITH (NOLOCK) ON invl.InvoiceId = i.InvoiceId INNER JOIN
	dbo.Inventory AS inv WITH (NOLOCK) ON inv.InventoryId = invl.InventoryId AND inv.SupplierInvoiceId = invl.InvoiceId INNER JOIN
	dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i.CurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS stg_tracing WITH (NOLOCK) ON stg_tracing.SysTermId = 24 INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o.Status INNER JOIN
	SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i.BillingType INNER JOIN
	SoesysV2.dbo.SysTerm AS st_invStatus WITH (NOLOCK) ON st_invStatus.SysTermId = inv.Status INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
WHERE     
	ISNULL(invl.InventoryLogId, 0) > 0 AND 
	invl.Type = 1 /* = Purchase*/ AND 
	st_oStatus.SysTermGroupId = 30 AND 
	st_oType.SysTermGroupId = 31 AND 
	st_iBillingType.SysTermGroupId = 27 AND 
	stg_tracing.SysTermGroupId = 54 AND 
	st_invStatus.SysTermGroupId = 151 AND 
	st_oStatus.LangId = st_oType.LangId AND 
	stg_tracing.LangId = st_oType.LangId AND 
	st_invStatus.LangId = st_oType.LangId AND 
	st_iBillingType.LangId = st_oType.LangId
UNION

/*Project*/ 
SELECT TOP 100 PERCENT 
	/*Invoice*/ i_inv.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS MappedInvoiceId,
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, '' AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName, '' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(1 AS bit), 1) AS IsProject, proj.ProjectId, 
	/*Origin*/ proj.Description AS Description, proj.Type AS OriginType, st_pType.Name AS OriginTypeName, proj.Status AS OriginStatus, st_pStatus.Name AS OriginStatusName, 
	/*Common*/ 0 AS BillingType, '' AS BillingTypeName, CAST(proj.Number AS nvarchar(100)) AS Number, 0 AS Amount, 0 AS AmountCurrency, 0 AS VatAmount, 0 AS VatAmountCurrency, proj.StartDate AS Date, proj.State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, 0 AS CurrencyRate, cu.SysCurrencyId, 
	/*Lang*/ st_pType.LangId AS LangId
FROM         
	dbo.Invoice AS i_inv WITH (NOLOCK) INNER JOIN
	dbo.Origin AS o_inv WITH (NOLOCK) ON o_inv.OriginId = i_inv.InvoiceId INNER JOIN
	dbo.Project AS proj WITH (NOLOCK) ON proj.ProjectId = i_inv.ProjectId INNER JOIN
	dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_inv.CurrencyId INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_pType WITH (NOLOCK) ON st_pType.SysTermId = proj.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_pStatus WITH (NOLOCK) ON st_pStatus.SysTermId = proj.Status
WHERE     
	i_inv.State = 0 AND 
	o_inv.Type = 2 AND 
	st_pType.SysTermGroupId = 297 AND 
	st_pStatus.SysTermGroupId = 287 AND 
	st_pType.LangId = st_pStatus.LangId

UNION

/*SupplierInvoiceProject*/ 
SELECT TOP 100 PERCENT 
	/*Invoice*/ i_inv.InvoiceId, 
	/*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, 
	/*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, 
	/*Order*/ ISNULL(CAST(0 AS bit), 0) AS IsOrder, 0 AS OrderId, 
	/*Mapped Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS MappedInvoiceId,
	/*Reminder Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsReminderInvoice, 0 AS ReminderInvoiceId, 
	/*Interest Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInterestInvoice, 0 AS InterestInvoiceId, 
	/*Payment*/ ISNULL(CAST(0 AS bit), 0) AS IsPayment, 0 AS PaymentRowId, 0 AS PaymentStatusId, '' AS PaymentStatusName, 
	/*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, 
	/*Voucher*/ ISNULL(CAST(0 AS bit), 0) AS IsVoucher, 0 AS VoucherHeadId, 
	/*Inventory*/ ISNULL(CAST(0 AS bit), 0) AS IsInventory, 0 AS InventoryId, '' AS InventoryName, '' AS InventoryDescription, '' AS InventoryTypeName, 0 AS InventoryStatusId, '' AS InventoryStatusName, 
	/*Project*/ ISNULL(CAST(1 AS bit), 1) AS IsProject, proj.ProjectId, 
	/*Origin*/ proj.Description AS Description, proj.Type AS OriginType, st_pType.Name AS OriginTypeName, proj.Status AS OriginStatus, st_pStatus.Name AS OriginStatusName, 
	/*Common*/ 0 AS BillingType, '' AS BillingTypeName, CAST(proj.Number AS nvarchar(100)) AS Number, 0 AS Amount, 0 AS AmountCurrency, 0 AS VatAmount, 0 AS VatAmountCurrency, proj.StartDate AS Date, proj.State, 
	/*Currency*/ s_cu.Code AS CurrencyCode, 0 AS CurrencyRate, cu.SysCurrencyId, 
	/*Lang*/ st_pType.LangId AS LangId
FROM         
	dbo.Invoice AS i_inv WITH (NOLOCK) INNER JOIN
	dbo.Origin AS o_inv WITH (NOLOCK) ON o_inv.OriginId = i_inv.InvoiceId INNER JOIN
	dbo.TimeCodeTransaction AS tct WITH (NOLOCK) ON tct.SupplierInvoiceId = i_inv.InvoiceId INNER JOIN
	dbo.Project as proj WITH (NOLOCK) ON proj.ProjectId = tct.ProjectId INNER JOIN
	--dbo.Project AS proj WITH (NOLOCK) ON proj.ProjectId = i_inv.ProjectId INNER JOIN
	dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_inv.CurrencyId INNER JOIN
	SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId INNER JOIN
	SoesysV2.dbo.SysTerm AS st_pType WITH (NOLOCK) ON st_pType.SysTermId = proj.Type INNER JOIN
	SoesysV2.dbo.SysTerm AS st_pStatus WITH (NOLOCK) ON st_pStatus.SysTermId = proj.Status
WHERE     
	i_inv.State = 0 AND 
	o_inv.Type = 1 AND 
	st_pType.SysTermGroupId = 297 AND 
	st_pStatus.SysTermGroupId = 287 AND 
	st_pType.LangId = st_pStatus.LangId

GO

---$ Alter View dbo.SysProductSearchView 
IF OBJECT_ID(N'dbo.SysProductSearchView') IS NULL
BEGIN
    PRINT 'Create View : dbo.SysProductSearchView'
    exec('create view dbo.SysProductSearchView as select null as Col1') 
END
GO

PRINT 'Alter view : dbo.SysProductSearchView'
GO

SET QUOTED_IDENTIFIER ON 
go

SET ANSI_NULLS ON 
go



ALTER VIEW [dbo].[SysProductSearchView]
AS

	SELECT DISTINCT
		pl.SysProductId,
		sw.[Type],
		l.ActorCompanyId,
		l.SysPriceListHeadId,
		pl.SysWholesellerId
	FROM
		SoesysV2.dbo.SysPriceList pl WITH (NOLOCK) 
		INNER JOIN CompanyWholesellerPricelist l on l.SysPriceListHeadId=pl.SysPriceListHeadId
		INNER JOIN soesysv2.dbo.SysWholeseller sw ON sw.SysWholesellerId = pl.SysWholesellerId

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
	st_status.Name as HeadStatusName,
	--Invoice
	inv.InvoiceId,
	inv.InvoiceNr as HeadNr,
	inv.InvoiceDate as HeadDate,
	inv.Created as HeadCreated,
	inv.CreatedBy as HeadCreatedBy,
	ISNULL((CASE WHEN ori.Type = 5 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END), CAST(0 AS bit)) AS HeadIsOffer, 
	ISNULL((CASE WHEN ori.Type = 6 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END), CAST(0 AS bit)) AS HeadIsOrder, 
	ISNULL((CASE WHEN ori.Type = 2 THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END), CAST(0 AS bit)) AS HeadIsInvoice, 
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

---$ Alter View dbo.TimeInvoiceTransactionsProjectView 
IF OBJECT_ID(N'dbo.TimeInvoiceTransactionsProjectView') IS NULL
BEGIN
    PRINT 'Create View : dbo.TimeInvoiceTransactionsProjectView'
    exec('create view dbo.TimeInvoiceTransactionsProjectView as select null as Col1') 
END
GO

PRINT 'Alter view : dbo.TimeInvoiceTransactionsProjectView'
GO

SET QUOTED_IDENTIFIER ON 
go

SET ANSI_NULLS ON 
go








ALTER VIEW [dbo].[TimeInvoiceTransactionsProjectView]
AS

SELECT
	--TimeCodeTransaction
	tct.TimeCodeTransactionId,
	tct.Type AS TimeCodeTransactionType,
	--TimeInvoiceTransaction
	tit.TimeInvoiceTransactionId,
	tit.CustomerInvoiceRowId,
	tit.Quantity,
	tit.InvoiceQuantity,
	tit.Amount,
	tit.AmountCurrency,
	tit.AmountLedgerCurrency,
	tit.AmountEntCurrency,
	tit.VatAmount,
	tit.VatAmountCurrency,
	tit.VatAmountLedgerCurrency,
	tit.VatAmountEntCurrency,
	tit.Exported,
	--Invoice
	inv.InvoiceNr,
	--Project
	proj.ActorCompanyId,
	proj.ProjectId,
	proj.Number,
	proj.Name,
	proj.Status,
	--TimeBlockDate
	tbd.TimeBlockDateId,
	tbd.Date,
	--Employee
	emp.EmployeeId,
	emp.EmployeeNr,
	(cp.FirstName + ' ' + cp.LastName) AS EmployeeName,
	emp.CalculatedCostPerHour,
	cir.InvoiceId AS InvoiceId,
	cir.AttestStateId AS AttestStateId
FROM
	TimeInvoiceTransaction as tit INNER JOIN
	CustomerInvoiceRow as cir on cir.CustomerInvoiceRowId = tit.CustomerInvoiceRowId INNER JOIN
	Invoice as inv on inv.InvoiceId = cir.InvoiceId INNER JOIN
	TimeCodeTransaction as tct on tct.TimeCodeTransactionId = tit.TimeCodeTransactionId INNER JOIN
	Project as proj on proj.ProjectId = tct.ProjectId  LEFT OUTER JOIN
	Employee as emp on emp.EmployeeId = tit.EmployeeId LEFT OUTER JOIN
	ContactPerson cp ON emp.ContactPersonId = cp.ActorContactPersonId LEFT OUTER JOIN
	TimeBlockDate as tbd on tbd.TimeBlockDateId = tit.TimeBlockDateId
WHERE
	tit.State = 0 AND
	tct.State = 0 AND
	proj.State = 0 AND
	inv.State = 0 AND
	cir.State = 0 --AND
	--cir.AttestStateId is not null

GO

---$ Create FK : FK_CustomerInvoicePrintedReminder_Company
IF OBJECT_ID(N'dbo.FK_CustomerInvoicePrintedReminder_Company') IS NULL
BEGIN
    PRINT 'Add FK constraint : dbo.CustomerInvoicePrintedReminder.FK_CustomerInvoicePrintedReminder_Company'
    ALTER TABLE dbo.CustomerInvoicePrintedReminder
        ADD CONSTRAINT FK_CustomerInvoicePrintedReminder_Company
            FOREIGN KEY(ActorCompanyId)
                REFERENCES dbo.Company(ActorCompanyId)
                    ON DELETE NO ACTION
                    ON UPDATE NO ACTION
END
GO

---$ Create FK : FK_TimeStampEntry_EmployeeChild
IF OBJECT_ID(N'dbo.FK_TimeStampEntry_EmployeeChild') IS NULL
BEGIN
    PRINT 'Add FK constraint : dbo.TimeStampEntry.FK_TimeStampEntry_EmployeeChild'
    ALTER TABLE dbo.TimeStampEntry
        ADD CONSTRAINT FK_TimeStampEntry_EmployeeChild
            FOREIGN KEY(EmployeeChildId)
                REFERENCES dbo.EmployeeChild(EmployeeChildId)
                    ON DELETE NO ACTION
                    ON UPDATE NO ACTION
END
GO

---$ Alter Procedure dbo.GetDayOfAbsenceNumber 
IF OBJECT_ID(N'dbo.GetDayOfAbsenceNumber') IS NULL
BEGIN
    PRINT 'Create procedure : dbo.GetDayOfAbsenceNumber'
    EXECUTE('CREATE PROCEDURE dbo.GetDayOfAbsenceNumber AS RETURN 0') 
END
GO

PRINT 'Alter procedure : dbo.GetDayOfAbsenceNumber'
GO

SET QUOTED_IDENTIFIER ON 
go

SET ANSI_NULLS ON 
go


ALTER PROCEDURE [dbo].[GetDayOfAbsenceNumber]
    @date					DATETIME,
    @maxDays				INT,
    @interval				INT,
    @sysPayrollTypeLevel3	INT,
    @employeeId			INT

AS
BEGIN

    DECLARE @currentDate DATETIME
    SET @currentDate = @date
    DECLARE @foundDate DATETIME
    SET @foundDate = @date

    WHILE @foundDate IS NOT NULL
    BEGIN
		-- Check for TimePayrollTransactions with specified SysPayrollTypeLevel3 for specified Employee
		SET @foundDate = NULL
		SELECT TOP 1 @foundDate = tbd.[Date]
		FROM
			TimePayrollTransaction tpt INNER JOIN
			TimeBlockDate tbd ON tbd.TimeBlockDateId = tpt.TimeBlockDateId
		GROUP BY
			tpt.EmployeeId,
			tpt.SysPayrollTypeLevel3,
			tpt.[State],
			tbd.[Date]
		HAVING
			tpt.EmployeeId = @employeeId AND
			tpt.SysPayrollTypeLevel3 = @sysPayrollTypeLevel3 AND
			tpt.[State] = 0 AND
			tbd.[Date] BETWEEN (@currentDate - @interval) AND (@currentDate - 1)
		ORDER BY
			tbd.[Date] ASC

		-- If a date was found, check again from that date and backwards (@interval number of days)
		-- Otherwise exit and use previously found date
		IF @foundDate IS NOT NULL
			SET @currentDate = @foundDate
		ELSE
			BREAK

		-- If we pass max number of days back, exit
		IF (SELECT COUNT(DISTINCT tbd.[Date])
			FROM
				TimePayrollTransaction tpt INNER JOIN
				TimeBlockDate tbd ON tbd.TimeBlockDateId = tpt.TimeBlockDateId
			WHERE
				tpt.EmployeeId = @employeeId AND
				tpt.SysPayrollTypeLevel3 = @sysPayrollTypeLevel3 AND
				tpt.[State] = 0 AND
				tbd.[Date] BETWEEN (@currentDate) AND (@date - 1)) > @maxDays
			BREAK
    END

    IF @currentDate = @date
	    SELECT 1
    ELSE
		SELECT COUNT(DISTINCT tbd.[Date]) + 1
		FROM
			TimePayrollTransaction tpt INNER JOIN
			TimeBlockDate tbd ON tbd.TimeBlockDateId = tpt.TimeBlockDateId
		WHERE
			tpt.EmployeeId = @employeeId AND
			tpt.SysPayrollTypeLevel3 = @sysPayrollTypeLevel3 AND
			tpt.[State] = 0 AND
			tbd.[Date] BETWEEN (@currentDate) AND (@date - 1)
END

GO

---$ Alter Procedure dbo.GetDayOfIllnessNumber 
IF OBJECT_ID(N'dbo.GetDayOfIllnessNumber') IS NULL
BEGIN
    PRINT 'Create procedure : dbo.GetDayOfIllnessNumber'
    EXECUTE('CREATE PROCEDURE dbo.GetDayOfIllnessNumber AS RETURN 0') 
END
GO

PRINT 'Alter procedure : dbo.GetDayOfIllnessNumber'
GO

SET QUOTED_IDENTIFIER ON 
go

SET ANSI_NULLS ON 
go

ALTER PROCEDURE [dbo].[GetDayOfIllnessNumber]
	@date				datetime,
	@maxDays			int,
	@interval			int,
	@timeCodeId			int,
	@employeeId			int,
	@daysUntilQualify	int,
	@dayOfAbsenceNumber	int output
AS
BEGIN

	DECLARE @currentDate datetime
	SET @currentDate = @date
	DECLARE @foundDate datetime
	SET @foundDate = @date

	-- When the number of qualifying days of illness is known and passed as parameter,
	-- add these days to maxDays (subtracted from the result)
	IF @daysUntilQualify IS NOT NULL
		SET @maxDays = @maxDays + @daysUntilQualify

	WHILE @foundDate IS NOT NULL
		BEGIN
			-- Check for TimeCodeTransactions with specified TimeCode for specified Employee
			SET @foundDate = NULL
			SELECT TOP 1 @foundDate = tbd.[Date]
			FROM 
				TimeCodeTransaction tct INNER JOIN 
				TimeBlock tb ON tct.TimeBlockId = tb.TimeBlockId INNER JOIN 
				TimeBlockDate tbd ON tb.TimeBlockDateId = tbd.TimeBlockDateId
			GROUP BY 
				tb.EmployeeId, 
				tct.TimeCodeId, 
				tct.[State], 
				tbd.[Date]
			HAVING 
				tb.EmployeeId = @employeeId AND
				tct.TimeCodeId = @timeCodeId AND
				tct.[State] = 0 AND
				tbd.[Date] BETWEEN (@currentDate - @interval) AND (@currentDate - 1)
			ORDER BY 
				tbd.[Date] ASC

			-- If a date was found, check again from that date and backwards (@interval number of days)
			-- Otherwise exit and use previously found date
			IF @foundDate IS NOT NULL
				SET @currentDate = @foundDate
			ELSE
				BREAK

			-- If we pass max number of days back, exit
			IF (SELECT COUNT(DISTINCT tbd.[Date])
				FROM 
					TimeCodeTransaction tct INNER JOIN 
					TimeBlock tb ON tct.TimeBlockId = tb.TimeBlockId INNER JOIN 
					TimeBlockDate tbd ON tb.TimeBlockDateId = tbd.TimeBlockDateId
				WHERE
					tb.EmployeeId = @employeeId AND
					tct.TimeCodeId = @timeCodeId AND
					tct.[State] = 0 AND
					tbd.[Date] BETWEEN (@currentDate) AND (@date - 1)) > @maxDays
				BREAK
		END

	IF @currentDate = @date
		SET @dayOfAbsenceNumber = 1
	ELSE
		BEGIN
			SET @dayOfAbsenceNumber = (SELECT COUNT(DISTINCT tbd.[Date]) + 1
				FROM 
					TimeCodeTransaction tct INNER JOIN 
					TimeBlock tb ON tct.TimeBlockId = tb.TimeBlockId INNER JOIN 
					TimeBlockDate tbd ON tb.TimeBlockDateId = tbd.TimeBlockDateId
				WHERE
					tb.EmployeeId = @employeeId AND
					tct.TimeCodeId = @timeCodeId AND
					tct.[State] = 0 AND
					tbd.[Date] BETWEEN (@currentDate) AND (@date - 1))
		END

	-- Check if absence starts on a 'non schedule' day
	-- In that case set @daysUntilQualify and recursively call procedure again
	IF @daysUntilQualify IS NOT NULL
		SET @dayOfAbsenceNumber = @dayOfAbsenceNumber - @daysUntilQualify
	ELSE
		BEGIN
			SET @daysUntilQualify = 0
			-- Loop until a schedule day is found (template blocks with different start and stop times)
			WHILE @currentDate <= @date
				BEGIN
					IF (SELECT COUNT(*)
					FROM
						TimeScheduleTemplateBlock b
					WHERE
						b.EmployeeId = @employeeId AND
						b.Date = @currentDate AND
						b.StartTime <> b.StopTime AND
						b.State = 0) > 0
					BREAK

					SET @daysUntilQualify = @daysUntilQualify + 1
					SET @currentDate = DATEADD(day, 1, @currentDate)
				END

			IF @daysUntilQualify > 0
				EXEC GetDayOfIllnessNumber @date, @maxDays, @interval, @timeCodeId, @employeeId, @daysUntilQualify, @dayOfAbsenceNumber
		END

	SELECT @dayOfAbsenceNumber
END

GO

---$ Alter Procedure dbo.GetOriginStatusFromCustomerInvoiceRow 
IF OBJECT_ID(N'dbo.GetOriginStatusFromCustomerInvoiceRow') IS NULL
BEGIN
    PRINT 'Create procedure : dbo.GetOriginStatusFromCustomerInvoiceRow'
    EXECUTE('CREATE PROCEDURE dbo.GetOriginStatusFromCustomerInvoiceRow AS RETURN 0') 
END
GO

PRINT 'Alter procedure : dbo.GetOriginStatusFromCustomerInvoiceRow'
GO

SET QUOTED_IDENTIFIER ON 
go

SET ANSI_NULLS ON 
go

-- =============================================
-- Author:		Albert Höglund
-- Create date: 2014-11-20
-- Description:	Gets the origin status of a customerinvoice from a customerinvoicerow
-- =============================================
ALTER PROCEDURE GetOriginStatusFromCustomerInvoiceRow
	-- Add the parameters for the stored procedure here
	@customerinvoicerowid int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT O.Status from CustomerInvoiceRow CR inner join 
	Origin O on CR.InvoiceId = O.OriginId
	where CR.CustomerInvoiceRowId = @customerinvoicerowId
END

GO

---$ Alter Procedure dbo.GetTimeBlocksForEmployees 
IF OBJECT_ID(N'dbo.GetTimeBlocksForEmployees') IS NULL
BEGIN
    PRINT 'Create procedure : dbo.GetTimeBlocksForEmployees'
    EXECUTE('CREATE PROCEDURE dbo.GetTimeBlocksForEmployees AS RETURN 0') 
END
GO

PRINT 'Alter procedure : dbo.GetTimeBlocksForEmployees'
GO

SET QUOTED_IDENTIFIER ON 
go

SET ANSI_NULLS ON 
go




ALTER PROCEDURE [dbo].[GetTimeBlocksForEmployees]
		@employeeIds nvarchar(max),
		@startDate DateTime,
		@stopDate DateTime
	AS
	BEGIN
		SET NOCOUNT ON;

	SELECT 
		tb.EmployeeId,
		tb.TimeBlockId,
		tb.TimeScheduleTemplatePeriodId,
		tbd.TimeBlockDateId,
		tbd.Status,
		tbd.StampingStatus,
		tbd.Date,
		tbd.DiscardedBreakEvaluation,
		tb.StartTime,
		tb.StopTime,
		tb.ManuallyAdjusted,
		(select	count(tc.TimeCodeId) from TimeCode as tc WITH (NOLOCK) inner join TimeBlockCodeMapping as m on m.TimeCodeId = tc.TimeCodeId and m.TimeBlockId = tb.TimeBlockId where tc.Type = 1) AS NoOfPresenceTimeCodes,
		(select	count(tc.TimeCodeId) from TimeCode as tc WITH (NOLOCK) inner join TimeBlockCodeMapping as m on m.TimeCodeId = tc.TimeCodeId and m.TimeBlockId = tb.TimeBlockId where tc.Type = 2) AS NoOfAbsenceTimeCodes,
		(select	count(tc.TimeCodeId) from TimeCode as tc WITH (NOLOCK) inner join TimeBlockCodeMapping as m on m.TimeCodeId = tc.TimeCodeId and m.TimeBlockId = tb.TimeBlockId where tc.Type = 3) AS NoOfBreakTimeCodes
	FROM 
		TimeBlock as tb WITH (NOLOCK) INNER JOIN
		TimeBlockDate as tbd WITH (NOLOCK) on tb.TimeBlockDateId = tbd.TimeBlockDateId
	WHERE
		tbd.EmployeeId IN (SELECT * FROM SplitDelimiterString(@employeeIds, ',')) AND 
		tbd.Date BETWEEN @startDate AND @stopDate AND
		tb.State = 0
	ORDER BY
		tbd.Date

END

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
		 tpt.Comment as Note,
		 a.AccountNr as AccountNr,
		 a.Name as AccountName,
		 ad.AccountDimNr as AccountDimNr,
		 ad.name as AccountDimName,
		 a2.AccountNr as AccountInternalNr,
		 a2.Name as AccountInternalName,
		 tpt.EmployeeId,
		 tpt.TimeBlockDateId

		 from 
		 TimePayrollTransaction tpt inner join
		 product p on tpt.PayrollProductId = p.ProductId inner join
		 PayrollProduct pp on pp.ProductId  = p.ProductId inner join
		 TimeCodeTransaction tct on tct.TimeCodeTransactionId= tpt.TimeCodeTransactionId inner join
		 Account a on a.AccountId=tpt.AccountId inner join
		 timeblockdate tbd on tbd.TimeBlockDateId = tpt.TimeBlockDateId left outer join
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

---$ Alter Procedure dbo.GetTimeSchedulePlanningPeriods 
IF OBJECT_ID(N'dbo.GetTimeSchedulePlanningPeriods') IS NULL
BEGIN
    PRINT 'Create procedure : dbo.GetTimeSchedulePlanningPeriods'
    EXECUTE('CREATE PROCEDURE dbo.GetTimeSchedulePlanningPeriods AS RETURN 0') 
END
GO

PRINT 'Alter procedure : dbo.GetTimeSchedulePlanningPeriods'
GO

SET QUOTED_IDENTIFIER ON 
go

SET ANSI_NULLS ON 
go




ALTER PROCEDURE [dbo].[GetTimeSchedulePlanningPeriods]
	@actorCompanyId int,					-- Mandatory
	@dateFrom datetime,						-- Mandatory
	@dateTo datetime,						-- Mandatory
	@employeeIds nvarchar(max),				-- Null or comma separated string
	@shiftTypeIds nvarchar(max),			-- Null or comma separated string
	@preliminary bit,						-- Null, 0 or 1 (Null = condition not used, 0 = no preliminary, 1 = only preliminary)
	@loadBreaks bit,						-- 0 or 1 (0 = do not load break blocks, 1 = load break blocks)
	@absence bit,							-- Null, 0 or 1 (Null = condition not used, 0 = no absence, 1 = only absence)
	@timeDeviationCauseIds nvarchar(max),	-- Null or a comma separated string (Null = condition not used, string = specified deviation causes)
	@shiftStatus int,						-- Null or a shift status
	@employeeIdInQueue int					-- Null or employee ID of employee in queue

WITH RECOMPILE

AS
BEGIN
	SET NOCOUNT ON;

-- DISTINCT needed when multiple employees in queue
  SELECT DISTINCT
        b.TimeScheduleTemplateBlockId,
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
        b.CustomerInvoiceId,
        b.Type,
        b.StaffingNeedsRowId,
        e.Hidden,
        e.Vacant,
        ep.IsPreliminary,
        cp.FirstName + ' ' + cp.LastName AS EmployeeName, 
        st.Name AS ShiftTypeName,
        st.Description AS ShiftTypeDescription,
        st.Color AS ShiftTypeColor,
        tdc.Name AS TimeDeviationCauseName        
        FROM TimeScheduleTemplateBlock AS b
        INNER JOIN Employee AS e ON b.EmployeeId = e.EmployeeId
        LEFT OUTER JOIN TimeScheduleEmployeePeriod AS ep ON b.TimeScheduleEmployeePeriodId = ep.TimeScheduleEmployeePeriodId
        LEFT OUTER JOIN ContactPerson AS cp ON e.ContactPersonId = cp.ActorContactPersonId
        LEFT OUTER JOIN ShiftType AS st ON b.ShiftTypeId = st.ShiftTypeId
        LEFT OUTER JOIN TimeDeviationCause AS tdc ON b.TimeDeviationCauseId = tdc.TimeDeviationCauseId
		LEFT OUTER JOIN TimeScheduleTemplateBlockQueue AS q ON b.TimeScheduleTemplateBlockId = q.TimeScheduleTemplateBlockId
        WHERE b.State = 0 AND
		 (@employeeIds IS NULL OR b.EmployeeId IN (SELECT * FROM SplitDelimiterString(@employeeIds, ','))) AND
		 (@shiftTypeIds IS NULL OR b.ShiftTypeId IS NULL OR (b.ShiftTypeId IN (SELECT * FROM SplitDelimiterString(@shiftTypeIds, ',')))) AND
		 b.Date BETWEEN @dateFrom AND @dateTo AND
		 ((@loadBreaks = 0 AND b.BreakType = 0) OR (@loadBreaks = 1)) AND
		 (((b.TimeScheduleEmployeePeriodId IS NOT NULL) AND (b.StartTime <> b.StopTime) AND (b.TimeDeviationCauseId IS NULL)) OR (b.TimeDeviationCauseId IS NOT NULL)) AND
     (@preliminary IS NULL OR (ep.EmployeeId = b.EmployeeId AND ep.IsPreliminary = @preliminary)) AND
     (@absence IS NULL OR (@absence = 0 AND b.TimeDeviationCauseId IS NULL) OR (@absence = 1 AND b.TimeDeviationCauseId IS NOT NULL)) AND
     (@timeDeviationCauseIds IS NULL OR (b.TimeDeviationCauseId IN (SELECT * FROM SplitDelimiterString(@timeDeviationCauseIds, ',')))) AND
     (@shiftStatus IS NULL OR (b.ShiftStatus = @shiftStatus)) AND
     (@employeeIdInQueue IS NULL OR (q.Type = 1 AND q.EmployeeId = @employeeIdInQueue)) AND
     e.ActorCompanyId = @actorCompanyId AND
     e.State = 0

     END






     /****** Object:  StoredProcedure [dbo].[GetTimeSchedulePlanningPeriodBreakLengths]    Script Date: 2014-10-31 09:14:55 ******/
     SET ANSI_NULLS ON

     GO

     ---$ drop Procedure dbo.GetTimeScheduleCompanySummary
     IF OBJECT_ID(N'dbo.GetTimeScheduleCompanySummary') IS NOT NULL
     BEGIN
     PRINT 'Drop Procedure : dbo.GetTimeScheduleCompanySummary'
     DROP PROCEDURE dbo.GetTimeScheduleCompanySummary
     END
     GO


     ---$ drop Procedure dbo.SysProductSearchNI
     IF OBJECT_ID(N'dbo.SysProductSearchNI') IS NOT NULL
     BEGIN
     PRINT 'Drop Procedure : dbo.SysProductSearchNI'
     DROP PROCEDURE dbo.SysProductSearchNI
     END
     GO





     USE [soecompv2]
     GO

     /****** Object:  View [dbo].[OrderTraceView]    Script Date: 2014-11-26 14:25:37 ******/
     SET ANSI_NULLS ON
     GO

     SET QUOTED_IDENTIFIER ON
     GO


     ALTER VIEW [dbo].[OrderTraceView]
     AS
     /*Contract*/ SELECT TOP 100 PERCENT /*Order*/ i_ord.InvoiceId AS OrderId, /*Contract*/ ISNULL(CAST(1 AS bit), 1) AS IsContract, o_con.OriginId AS ContractId,
     /*Offer*/ ISNULL(CAST(0 AS bit), 0) AS IsOffer, 0 AS OfferId, /*Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS InvoiceId, /*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi,
     0 AS EdiEntryId, ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, /*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, /*SupplierInvoice*/ ISNULL(CAST(0 AS bit), 0) AS IsSupplierInvoice,
     0 AS SupplierInvoiceId,  /*Origin*/ o_con.Description,
     o_con.Type AS OriginType, st_oType.Name AS OriginTypeName, o_con.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, /*Common*/ i_con.BillingType,
     st_iBillingType.Name AS BillingTypeName, i_con.InvoiceNr AS Number, ISNULL(CAST(i_con.TotalAmount AS decimal(10, 2)), 0) AS Amount,
     ISNULL(CAST(i_con.TotalAmountCurrency AS decimal(10, 2)), 0) AS AmountCurrency, i_con.VATAmount AS VatAmount, i_con.VATAmountCurrency AS VatAmountCurrency,
     i_con.InvoiceDate AS Date, i_con.State, /*Currency*/ s_cu.Code AS CurrencyCode, i_ord.CurrencyRate, cu.SysCurrencyId, /*Lang*/ st_oType.LangId AS LangId
     FROM         dbo.Invoice AS i_ord WITH (NOLOCK) INNER JOIN
     dbo.Origin AS o_ord WITH (NOLOCK) ON o_ord.OriginId = i_ord.InvoiceId INNER JOIN
     dbo.OriginInvoiceMapping AS orm WITH (NOLOCK) ON orm.InvoiceId = i_ord.InvoiceId INNER JOIN
     dbo.Origin AS o_con WITH (NOLOCK) ON o_con.OriginId = orm.OriginId INNER JOIN
     dbo.Invoice AS i_con WITH (NOLOCK) ON i_con.InvoiceId = o_con.OriginId INNER JOIN
     dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_ord.CurrencyId INNER JOIN
     SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o_con.Type INNER JOIN
     SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o_con.Status INNER JOIN
     SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i_con.BillingType INNER JOIN
     SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
     WHERE     i_ord.State = 0 AND o_ord.Type = 6 AND i_con.Type = 7 AND st_oStatus.SysTermGroupId = 30 AND st_oType.SysTermGroupId = 31 AND
     st_iBillingType.SysTermGroupId = 27 AND st_oStatus.LangId = st_oType.LangId AND st_iBillingType.LangId = st_oType.LangId
     UNION
     /*Offer*/ SELECT TOP 100 PERCENT /*Order*/ i_ord.InvoiceId AS OrderId, /*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, /*Offer*/ ISNULL(CAST(1 AS bit), 1)
     AS IsOffer, orm.OriginId AS OfferId, /*Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS InvoiceId, /*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId,
     ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, /*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId,  /*SupplierInvoice*/ ISNULL(CAST(0 AS bit), 0) AS IsSupplierInvoice,
     0 AS SupplierInvoiceId, /*Origin*/ o_off.Description, o_off.Type AS OriginType,
     st_oType.Name AS OriginTypeName, o_off.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, /*Common*/ i_off.BillingType,
     st_iBillingType.Name AS BillingTypeName, i_off.InvoiceNr AS Number, ISNULL(CAST(i_off.TotalAmount AS decimal(10, 2)), 0) AS Amount,
     ISNULL(CAST(i_off.TotalAmountCurrency AS decimal(10, 2)), 0) AS AmountCurrency, i_off.VATAmount AS VatAmount, i_off.VATAmountCurrency AS VatAmountCurrency,
     i_off.InvoiceDate AS Date, i_off.State, /*Currency*/ s_cu.Code AS CurrencyCode, i_off.CurrencyRate, cu.SysCurrencyId, /*Lang*/ st_oType.LangId AS LangId
     FROM         dbo.Invoice AS i_ord WITH (NOLOCK) INNER JOIN
     dbo.Origin AS o_ord WITH (NOLOCK) ON o_ord.OriginId = i_ord.InvoiceId INNER JOIN
     dbo.OriginInvoiceMapping AS orm WITH (NOLOCK) ON orm.InvoiceId = o_ord.OriginId INNER JOIN
     dbo.Origin AS o_off WITH (NOLOCK) ON o_off.OriginId = orm.OriginId INNER JOIN
     dbo.Invoice AS i_off WITH (NOLOCK) ON i_off.InvoiceId = o_off.OriginId INNER JOIN
     dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_ord.CurrencyId INNER JOIN
     SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o_off.Type INNER JOIN
     SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o_off.Status INNER JOIN
     SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i_off.BillingType INNER JOIN
     SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
     WHERE     i_ord.State = 0 AND o_ord.Type = 6 AND o_off.Type = 5 AND st_oStatus.SysTermGroupId = 30 AND st_oType.SysTermGroupId = 31 AND
     st_iBillingType.SysTermGroupId = 27 AND st_oStatus.LangId = st_oType.LangId AND st_iBillingType.LangId = st_oType.LangId
     UNION
     /*Invoice*/ SELECT TOP 100 PERCENT /*Order*/ i_ord.InvoiceId AS OrderId, /*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, /*Offer*/ ISNULL(CAST(0 AS bit),
     0) AS IsOffer, 0 AS OfferId, /*Invoice*/ ISNULL(CAST(1 AS bit), 1) AS IsInvoice, orm.InvoiceId AS InvoiceId, /*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId,
     ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, /*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId,  /*SupplierInvoice*/ ISNULL(CAST(0 AS bit), 0) AS IsSupplierInvoice,
     0 AS SupplierInvoiceId, /*Origin*/ o_inv.Description, o_inv.Type AS OriginType,
     st_oType.Name AS OriginTypeName, o_inv.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, /*Common*/ i_inv.BillingType,
     st_iBillingType.Name AS BillingTypeName, i_inv.InvoiceNr AS Number, ISNULL(CAST(i_inv.TotalAmount AS decimal(10, 2)), 0) AS Amount,
     ISNULL(CAST(i_inv.TotalAmountCurrency AS decimal(10, 2)), 0) AS AmountCurrency, i_inv.VATAmount AS VatAmount, i_inv.VATAmountCurrency AS VatAmountCurrency,
     i_inv.InvoiceDate AS Date, i_inv.State, /*Currency*/ s_cu.Code AS CurrencyCode, i_ord.CurrencyRate, cu.SysCurrencyId, /*Lang*/ st_oType.LangId AS LangId
     FROM         dbo.Invoice AS i_ord WITH (NOLOCK) INNER JOIN
     dbo.Origin AS o_ord WITH (NOLOCK) ON o_ord.OriginId = i_ord.InvoiceId INNER JOIN
     dbo.OriginInvoiceMapping AS orm WITH (NOLOCK) ON orm.OriginId = o_ord.OriginId INNER JOIN
     dbo.Origin AS o_inv WITH (NOLOCK) ON o_inv.OriginId = orm.InvoiceId INNER JOIN
     dbo.Invoice AS i_inv WITH (NOLOCK) ON i_inv.InvoiceId = o_inv.OriginId INNER JOIN
     dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_ord.CurrencyId INNER JOIN
     SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o_inv.Type INNER JOIN
     SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o_inv.Status INNER JOIN
     SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i_inv.BillingType INNER JOIN
     SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
     WHERE     i_ord.State = 0 AND o_ord.Type = 6 AND o_inv.Type = 2 AND st_oStatus.SysTermGroupId = 30 AND st_oType.SysTermGroupId = 31 AND
     st_iBillingType.SysTermGroupId = 27 AND st_oStatus.LangId = st_oType.LangId AND st_iBillingType.LangId = st_oType.LangId
     UNION
     /*EDI*/ SELECT TOP 100 PERCENT /*Order*/ i_ord.InvoiceId AS OrderId, /*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, /*Offer*/ ISNULL(CAST(0 AS bit), 0)
     AS IsOffer, 0 AS OfferId, /*Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS InvoiceId, /*Edi*/ ISNULL(CAST(1 AS bit), 1) AS IsEdi, ISNULL(edi_ord.EdiEntryId, 0)
     AS EdiEntryId, edi_ord.HasPdf AS EdiHasPdf, /*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId,  /*SupplierInvoice*/ ISNULL(CAST(0 AS bit), 0) AS IsSupplierInvoice,
     0 AS SupplierInvoiceId, (ISNULL(edi_ord.WholesellerName, '')) AS Description, 0 AS OriginType,
     'EDI' AS OriginTypeName, edi_ord.OrderStatus AS OriginStatus, edi_ord.OrderStatusName AS OriginStatusName, /*Common*/ 0 AS BillingType, '' AS BillingTypeName,
     ISNULL(edi_ord.SellerOrderNr, '') AS Number, edi_ord.Sum AS Amount, edi_ord.SumCurrency AS AmountCurrency, edi_ord.SumVat AS VatAmount,
     edi_ord.SumVatCurrency AS VatAmountCurrency, ISNULL(edi_ord.Date, i_ord.InvoiceDate) AS Date, edi_ord.State, /*Currency*/ edi_ord.CurrencyCode,
     edi_ord.CurrencyRate, edi_ord.SysCurrencyId, /*Lang*/ edi_ord.LangId
     FROM         dbo.Invoice AS i_ord WITH (NOLOCK) INNER JOIN
     dbo.Origin AS o_ord WITH (NOLOCK) ON o_ord.OriginId = i_ord.InvoiceId INNER JOIN
     dbo.EdiEntryView AS edi_ord WITH (NOLOCK) ON edi_ord.OrderId = i_ord.InvoiceId
     WHERE     i_ord.State = 0 AND o_ord.Type = 6 AND edi_ord.Type = 1
     UNION
     /*Project*/ SELECT TOP 100 PERCENT /*Order*/ i_ord.InvoiceId AS OrderId, /*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, /*Offer*/ ISNULL(CAST(0 AS bit),
     0) AS IsOffer, 0 AS OfferId, /*Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS InvoiceId, /*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId,
     ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, /*Project*/ ISNULL(CAST(1 AS bit), 1) AS IsProject, proj.ProjectId,  /*SupplierInvoice*/ ISNULL(CAST(0 AS bit), 0) AS IsSupplierInvoice,
     0 AS SupplierInvoiceId, /*Origin*/ proj.Description AS Description,
     proj.Type AS OriginType, st_pType.Name AS OriginTypeName, proj.Status AS OriginStatus, st_pStatus.Name AS OriginStatusName, /*Common*/ 0 AS BillingType,
     '' AS BillingTypeName, cast(proj.Number AS nvarchar(100)) AS Number, 0 AS Amount, 0 AS AmountCurrency, 0 AS VatAmount, 0 AS VatAmountCurrency,
     proj.StartDate AS Date, proj.State, /*Currency*/ s_cu.Code AS CurrencyCode, 0 AS CurrencyRate, cu.SysCurrencyId, /*Lang*/ st_pType.LangId AS LangId
     FROM         dbo.Invoice AS i_ord WITH (NOLOCK) INNER JOIN
     dbo.Origin AS o_ord WITH (NOLOCK) ON o_ord.OriginId = i_ord.InvoiceId INNER JOIN
     dbo.Project AS proj WITH (NOLOCK) ON proj.ProjectId = i_ord.ProjectId INNER JOIN
     dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_ord.CurrencyId INNER JOIN
     SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId INNER JOIN
     SoesysV2.dbo.SysTerm AS st_pType WITH (NOLOCK) ON st_pType.SysTermId = proj.Type INNER JOIN
     SoesysV2.dbo.SysTerm AS st_pStatus WITH (NOLOCK) ON st_pStatus.SysTermId = proj.Status
     WHERE     i_ord.State = 0 AND o_ord.Type = 6 AND st_pType.SysTermGroupId = 297 AND st_pStatus.SysTermGroupId = 287 AND st_pType.LangId = st_pStatus.LangId
     UNION
     /*Supplier invoice*/ SELECT TOP 100 PERCENT /*Order*/ i_ord.InvoiceId AS OrderId, /*Contract*/ ISNULL(CAST(0 AS bit), 0) AS IsContract, 0 AS ContractId, /*Offer*/ ISNULL(CAST(0 AS bit),
     0) AS IsOffer, 0 AS OfferId, /*Invoice*/ ISNULL(CAST(0 AS bit), 0) AS IsInvoice, 0 AS InvoiceId, /*Edi*/ ISNULL(CAST(0 AS bit), 0) AS IsEdi, 0 AS EdiEntryId,
     ISNULL(CAST(0 AS bit), 0) AS EdiHasPdf, /*Project*/ ISNULL(CAST(0 AS bit), 0) AS IsProject, 0 AS ProjectId, /*SupplierInvoice*/ ISNULL(CAST(1 AS bit), 1) AS IsSupplierInvoice,
     i_inv.InvoiceId AS SupplierInvoiceId, /*Origin*/ o_inv.Description, o_inv.Type AS OriginType,
     st_oType.Name AS OriginTypeName, o_inv.Status AS OriginStatus, st_oStatus.Name AS OriginStatusName, /*Common*/ i_inv.BillingType,
     st_iBillingType.Name AS BillingTypeName, i_inv.InvoiceNr AS Number, ISNULL(CAST(i_inv.TotalAmount AS decimal(10, 2)), 0) AS Amount,
     ISNULL(CAST(i_inv.TotalAmountCurrency AS decimal(10, 2)), 0) AS AmountCurrency, i_inv.VATAmount AS VatAmount, i_inv.VATAmountCurrency AS VatAmountCurrency,
     i_inv.InvoiceDate AS Date, i_inv.State, /*Currency*/ s_cu.Code AS CurrencyCode, i_ord.CurrencyRate, cu.SysCurrencyId, /*Lang*/ st_oType.LangId AS LangId
     FROM         dbo.Invoice AS i_ord WITH (NOLOCK) INNER JOIN
     dbo.Origin AS o_ord WITH (NOLOCK) ON o_ord.OriginId = i_ord.InvoiceId INNER JOIN
     dbo.OriginInvoiceMapping AS orm WITH (NOLOCK) ON orm.OriginId = o_ord.OriginId INNER JOIN
     dbo.Origin AS o_inv WITH (NOLOCK) ON o_inv.OriginId = orm.InvoiceId INNER JOIN
     dbo.Invoice AS i_inv WITH (NOLOCK) ON i_inv.InvoiceId = o_inv.OriginId INNER JOIN
     dbo.Currency AS cu WITH (NOLOCK) ON cu.CurrencyId = i_ord.CurrencyId INNER JOIN
     SoesysV2.dbo.SysTerm AS st_oType WITH (NOLOCK) ON st_oType.SysTermId = o_inv.Type INNER JOIN
     SoesysV2.dbo.SysTerm AS st_oStatus WITH (NOLOCK) ON st_oStatus.SysTermId = o_inv.Status INNER JOIN
     SoesysV2.dbo.SysTerm AS st_iBillingType WITH (NOLOCK) ON st_iBillingType.SysTermId = i_inv.BillingType INNER JOIN
     SoesysV2.dbo.SysCurrency AS s_cu WITH (NOLOCK) ON s_cu.SysCurrencyId = cu.SysCurrencyId
     WHERE     i_ord.State = 0 AND o_ord.Type = 6 AND o_inv.Type = 1 AND st_oStatus.SysTermGroupId = 30 AND st_oType.SysTermGroupId = 31 AND
     st_iBillingType.SysTermGroupId = 27 AND st_oStatus.LangId = st_oType.LangId AND st_iBillingType.LangId = st_oType.LangId




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
ALTER TABLE dbo.CustomerInvoiceHeadIO
	DROP CONSTRAINT DF_CustomerInvoiceHeadIO_Import
GO
ALTER TABLE dbo.CustomerInvoiceHeadIO
	DROP CONSTRAINT DF_CustomerInvoiceHeadIO_TotalAmount
GO
ALTER TABLE dbo.CustomerInvoiceHeadIO
	DROP CONSTRAINT DF_CustomerInvoiceHeadIO_TotalAmountCurrency
GO
ALTER TABLE dbo.CustomerInvoiceHeadIO
	DROP CONSTRAINT DF_CustomerInvoiceHeadIO_VATAmount
GO
ALTER TABLE dbo.CustomerInvoiceHeadIO
	DROP CONSTRAINT DF_CustomerInvoiceHeadIO_VATAmountCurrency
GO
ALTER TABLE dbo.CustomerInvoiceHeadIO
	DROP CONSTRAINT DF_CustomerInvoiceHeadIO_PaidAmount
GO
ALTER TABLE dbo.CustomerInvoiceHeadIO
	DROP CONSTRAINT DF_CustomerInvoiceHeadIO_PaidAmountCurrency
GO
ALTER TABLE dbo.CustomerInvoiceHeadIO
	DROP CONSTRAINT DF_CustomerInvoiceHeadIO_FreightAmount
GO
ALTER TABLE dbo.CustomerInvoiceHeadIO
	DROP CONSTRAINT DF_CustomerInvoiceHeadIO_InvoiceFee
GO
ALTER TABLE dbo.CustomerInvoiceHeadIO
	DROP CONSTRAINT DF_CustomerInvoiceHeadIO_CentRounding
GO
ALTER TABLE dbo.CustomerInvoiceHeadIO
	DROP CONSTRAINT DF_CustomerInvoiceHeadIO_FullyPayed
GO
ALTER TABLE dbo.CustomerInvoiceHeadIO
	DROP CONSTRAINT DF_CustomerInvoiceHeadIO_ImportHeadType
GO
ALTER TABLE dbo.CustomerInvoiceHeadIO
	DROP CONSTRAINT DF_CustomerInvoiceHeadIO_State
GO
CREATE TABLE dbo.Tmp_CustomerInvoiceHeadIO
	(
	CustomerInvoiceHeadIOId int NOT NULL IDENTITY (1, 1),
	InvoiceId int NULL,
	CustomerInvoiceNr nvarchar(50) NULL,
	SeqNr int NULL,
	OCR nvarchar(50) NULL,
	ActorCompanyId int NOT NULL,
	Import bit NOT NULL,
	Type int NOT NULL,
	Status int NOT NULL,
	Source int NOT NULL,
	BatchId nvarchar(50) NOT NULL,
	RegistrationType int NOT NULL,
	OriginType int NOT NULL,
	OriginStatus int NOT NULL,
	InvoiceState int NULL,
	CustomerId int NULL,
	CustomerNr nvarchar(50) NULL,
	CustomerName nvarchar(250) NULL,
	PaymentCondition nvarchar(50) NULL,
	InvoiceDate datetime NULL,
	DueDate datetime NULL,
	VoucherDate datetime NULL,
	ReferenceOur nvarchar(200) NULL,
	ReferenceYour nvarchar(200) NULL,
	CurrencyRate decimal(15, 4) NULL,
	CurrencyDate datetime NULL,
	TotalAmount decimal(10, 2) NULL,
	TotalAmountCurrency decimal(10, 2) NULL,
	VatType int NULL,
	VATAmount decimal(10, 2) NULL,
	VATAmountCurrency decimal(10, 2) NULL,
	PaidAmount decimal(10, 2) NULL,
	PaidAmountCurrency decimal(10, 2) NULL,
	RemainingAmount decimal(10, 2) NULL,
	FreightAmount decimal(10, 2) NULL,
	FreightAmountCurrency decimal(10, 2) NULL,
	InvoiceFee decimal(10, 2) NULL,
	InvoiceFeeCurrency decimal(10, 2) NULL,
	CentRounding decimal(10, 2) NULL,
	FullyPayed bit NULL,
	PaymentNr nvarchar(100) NULL,
	VoucherNr nvarchar(50) NULL,
	CreateAccountingInXE bit NULL,
	Note nvarchar(MAX) NULL,
	BillingType int NULL,
	Currency nvarchar(50) NULL,
	TransferType nvarchar(50) NULL,
	ErrorMessage nvarchar(MAX) NULL,
	ImportHeadType int NOT NULL,
	ImportId int NULL,
	Created datetime NULL,
	CreatedBy nvarchar(50) NULL,
	Modified datetime NULL,
	ModifiedBy nvarchar(50) NULL,
	BillingAddressName nvarchar(100) NULL,
	BillingAddressAddress nvarchar(100) NULL,
	BillingAddressCO nvarchar(100) NULL,
	BillingAddressPostNr nvarchar(50) NULL,
	BillingAddressCity nvarchar(100) NULL,
	BillingAddressCountry nvarchar(100) NULL,
	DeliveryAddressName nvarchar(100) NULL,
	DeliveryAddressAddress nvarchar(100) NULL,
	DeliveryAddressCO nvarchar(100) NULL,
	DeliveryAddressPostNr nvarchar(50) NULL,
	DeliveryAddressCity nvarchar(100) NULL,
	DeliveryAddressCountry nvarchar(100) NULL,
	UseFixedPriceArticle bit NULL,
	VatRate1 decimal(10, 2) NULL,
	VatAmount1 decimal(10, 2) NULL,
	VatRate2 decimal(10, 2) NULL,
	VatAmount2 decimal(10, 2) NULL,
	VatRate3 decimal(10, 2) NULL,
	VatAmount3 decimal(10, 2) NULL,
	PaymentConditionCode nvarchar(50) NULL,
	DeliveryTypeCode nvarchar(50) NULL,
	DeliveryConditionCode nvarchar(50) NULL,
	SalesAccountNr nvarchar(50) NULL,
	SalesAccountNrDim2 nvarchar(50) NULL,
	SalesAccountNrDim3 nvarchar(50) NULL,
	SalesAccountNrDim4 nvarchar(50) NULL,
	SalesAccountNrDim5 nvarchar(50) NULL,
	SalesAccountNrDim6 nvarchar(50) NULL,
	ClaimAccountNr nvarchar(50) NULL,
	ClaimAccountNrDim2 nvarchar(50) NULL,
	ClaimAccountNrDim3 nvarchar(50) NULL,
	ClaimAccountNrDim4 nvarchar(50) NULL,
	ClaimAccountNrDim5 nvarchar(50) NULL,
	ClaimAccountNrDim6 nvarchar(50) NULL,
	VatAccountnr nvarchar(50) NULL,
	OrderNr nvarchar(50) NULL,
	OfferNr nvarchar(50) NULL,
	ContractNr nvarchar(50) NULL,
	Language nvarchar(50) NULL,
	isCaschSale bit NULL,
	InternalDescription nvarchar(MAX) NULL,
	ExternalDescription nvarchar(MAX) NULL,
	WorkingDescription nvarchar(MAX) NULL,
	ProjectNr nvarchar(50) NULL,
	State int NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceHeadIO SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceHeadIO ADD CONSTRAINT
	DF_CustomerInvoiceHeadIO_Import DEFAULT ((1)) FOR Import
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceHeadIO ADD CONSTRAINT
	DF_CustomerInvoiceHeadIO_TotalAmount DEFAULT ((0)) FOR TotalAmount
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceHeadIO ADD CONSTRAINT
	DF_CustomerInvoiceHeadIO_TotalAmountCurrency DEFAULT ((0)) FOR TotalAmountCurrency
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceHeadIO ADD CONSTRAINT
	DF_CustomerInvoiceHeadIO_VATAmount DEFAULT ((0)) FOR VATAmount
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceHeadIO ADD CONSTRAINT
	DF_CustomerInvoiceHeadIO_VATAmountCurrency DEFAULT ((0)) FOR VATAmountCurrency
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceHeadIO ADD CONSTRAINT
	DF_CustomerInvoiceHeadIO_PaidAmount DEFAULT ((0)) FOR PaidAmount
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceHeadIO ADD CONSTRAINT
	DF_CustomerInvoiceHeadIO_PaidAmountCurrency DEFAULT ((0)) FOR PaidAmountCurrency
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceHeadIO ADD CONSTRAINT
	DF_CustomerInvoiceHeadIO_FreightAmount DEFAULT ((0)) FOR FreightAmount
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceHeadIO ADD CONSTRAINT
	DF_CustomerInvoiceHeadIO_InvoiceFee DEFAULT ((0)) FOR InvoiceFee
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceHeadIO ADD CONSTRAINT
	DF_CustomerInvoiceHeadIO_CentRounding DEFAULT ((0)) FOR CentRounding
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceHeadIO ADD CONSTRAINT
	DF_CustomerInvoiceHeadIO_FullyPayed DEFAULT ((0)) FOR FullyPayed
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceHeadIO ADD CONSTRAINT
	DF_CustomerInvoiceHeadIO_ImportHeadType DEFAULT ((0)) FOR ImportHeadType
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceHeadIO ADD CONSTRAINT
	DF_CustomerInvoiceHeadIO_State DEFAULT ((0)) FOR State
GO
SET IDENTITY_INSERT dbo.Tmp_CustomerInvoiceHeadIO ON
GO
IF EXISTS(SELECT * FROM dbo.CustomerInvoiceHeadIO)
	 EXEC('INSERT INTO dbo.Tmp_CustomerInvoiceHeadIO (CustomerInvoiceHeadIOId, InvoiceId, CustomerInvoiceNr, SeqNr, OCR, ActorCompanyId, Import, Type, Status, Source, BatchId, RegistrationType, OriginType, OriginStatus, CustomerId, CustomerNr, CustomerName, PaymentCondition, InvoiceDate, DueDate, VoucherDate, ReferenceOur, ReferenceYour, CurrencyRate, CurrencyDate, TotalAmount, TotalAmountCurrency, VATAmount, VATAmountCurrency, PaidAmount, PaidAmountCurrency, RemainingAmount, FreightAmount, FreightAmountCurrency, InvoiceFee, InvoiceFeeCurrency, CentRounding, FullyPayed, PaymentNr, VoucherNr, CreateAccountingInXE, Note, BillingType, Currency, TransferType, ErrorMessage, ImportHeadType, ImportId, Created, CreatedBy, Modified, ModifiedBy, BillingAddressAddress, BillingAddressCO, BillingAddressPostNr, BillingAddressCity, DeliveryAddressAddress, DeliveryAddressPostNr, DeliveryAddressCity, UseFixedPriceArticle, VatRate1, VatAmount1, VatRate2, VatAmount2, VatRate3, VatAmount3, State)
		SELECT CustomerInvoiceHeadIOId, InvoiceId, CustomerInvoiceNr, SeqNr, OCR, ActorCompanyId, Import, Type, Status, Source, BatchId, RegistrationType, OriginType, OriginStatus, CustomerId, CustomerNr, CustomerName, PaymentCondition, InvoiceDate, DueDate, VoucherDate, ReferenceOur, ReferenceYour, CurrencyRate, CurrencyDate, TotalAmount, TotalAmountCurrency, VATAmount, VATAmountCurrency, PaidAmount, PaidAmountCurrency, RemainingAmount, FreightAmount, FreightAmountCurrency, InvoiceFee, InvoiceFeeCurrency, CentRounding, FullyPayed, PaymentNr, VoucherNr, CreateAccountingInXE, Note, BillingType, Currency, TransferType, ErrorMessage, ImportHeadType, ImportId, Created, CreatedBy, Modified, ModifiedBy, BillingAddressAddress, BillingAddressCO, BillingAddressPostNr, BillingAddressCity, DeliveryAddressAddress, DeliveryAddressPostNr, CONVERT(nvarchar(100), DeliveryAddressCity), UseFixedPriceArticle, VatRate1, VatAmount1, VatRate2, VatAmount2, VatRate3, VatAmount3, State FROM dbo.CustomerInvoiceHeadIO WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_CustomerInvoiceHeadIO OFF
GO
ALTER TABLE dbo.CustomerInvoiceRowIO
	DROP CONSTRAINT FK_CustomerInvoiceRowIO_CustomerInvoiceHeadIO
GO
DROP TABLE dbo.CustomerInvoiceHeadIO
GO
EXECUTE sp_rename N'dbo.Tmp_CustomerInvoiceHeadIO', N'CustomerInvoiceHeadIO', 'OBJECT' 
GO
ALTER TABLE dbo.CustomerInvoiceHeadIO ADD CONSTRAINT
	PK_CustomerInvoiceHeadIO PRIMARY KEY CLUSTERED 
	(
	CustomerInvoiceHeadIOId
	) WITH( PAD_INDEX = OFF, FILLFACTOR = 90, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX XE_Stats_ActorCompanyId_BatchId ON dbo.CustomerInvoiceHeadIO
	(
	ActorCompanyId,
	BatchId
	) INCLUDE (CustomerInvoiceHeadIOId, InvoiceId, CustomerInvoiceNr, SeqNr, VatAmount3, UseFixedPriceArticle, VatRate1, VatAmount1, VatRate2, VatAmount2, VatRate3, BillingAddressCO, BillingAddressPostNr, BillingAddressCity, DeliveryAddressAddress, DeliveryAddressPostNr, DeliveryAddressCity, Created, CreatedBy, Modified, ModifiedBy, State, BillingAddressAddress, BillingType, Currency, TransferType, ErrorMessage, ImportHeadType, ImportId, CentRounding, FullyPayed, PaymentNr, VoucherNr, CreateAccountingInXE, Note, PaidAmountCurrency, RemainingAmount, FreightAmount, FreightAmountCurrency, InvoiceFee, InvoiceFeeCurrency, CurrencyDate, TotalAmount, TotalAmountCurrency, VATAmount, VATAmountCurrency, PaidAmount, InvoiceDate, DueDate, VoucherDate, ReferenceOur, ReferenceYour, CurrencyRate, OriginType, OriginStatus, CustomerId, CustomerNr, CustomerName, PaymentCondition, OCR, Import, Type, Status, Source, RegistrationType) 
 WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

BEGIN TRANSACTION
GO
ALTER TABLE dbo.CustomerInvoiceRowIO ADD CONSTRAINT
	FK_CustomerInvoiceRowIO_CustomerInvoiceHeadIO FOREIGN KEY
	(
	CustomerInvoiceHeadIOId
	) REFERENCES dbo.CustomerInvoiceHeadIO
	(
	CustomerInvoiceHeadIOId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.CustomerInvoiceRowIO SET (LOCK_ESCALATION = TABLE)
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
ALTER TABLE dbo.CustomerInvoiceRowIO ADD
	VatCode nvarchar(50) NULL,
	RowStatus nvarchar(50) NULL,
	InvoiceQuantity decimal(20, 6) NULL,
	PreviouslyInvoicedQuantity decimal(20, 6) NULL,
	RowDate datetime NULL,
	Stock nvarchar(100) NULL,
	ClaimAccountNr nvarchar(50) NULL,
	ClaimAccountNrDim2 nvarchar(50) NULL,
	ClaimAccountNrDim3 nvarchar(50) NULL,
	ClaimAccountNrDim4 nvarchar(50) NULL,
	ClaimAccountNrDim5 nvarchar(50) NULL,
	ClaimAccountNrDim6 nvarchar(50) NULL,
	VatAccountnr nvarchar(50) NULL
GO
ALTER TABLE dbo.CustomerInvoiceRowIO SET (LOCK_ESCALATION = TABLE)
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
ALTER TABLE dbo.CustomerInvoiceHeadIO
	DROP CONSTRAINT DF_CustomerInvoiceHeadIO_Import
GO
ALTER TABLE dbo.CustomerInvoiceHeadIO
	DROP CONSTRAINT DF_CustomerInvoiceHeadIO_TotalAmount
GO
ALTER TABLE dbo.CustomerInvoiceHeadIO
	DROP CONSTRAINT DF_CustomerInvoiceHeadIO_TotalAmountCurrency
GO
ALTER TABLE dbo.CustomerInvoiceHeadIO
	DROP CONSTRAINT DF_CustomerInvoiceHeadIO_VATAmount
GO
ALTER TABLE dbo.CustomerInvoiceHeadIO
	DROP CONSTRAINT DF_CustomerInvoiceHeadIO_VATAmountCurrency
GO
ALTER TABLE dbo.CustomerInvoiceHeadIO
	DROP CONSTRAINT DF_CustomerInvoiceHeadIO_PaidAmount
GO
ALTER TABLE dbo.CustomerInvoiceHeadIO
	DROP CONSTRAINT DF_CustomerInvoiceHeadIO_PaidAmountCurrency
GO
ALTER TABLE dbo.CustomerInvoiceHeadIO
	DROP CONSTRAINT DF_CustomerInvoiceHeadIO_FreightAmount
GO
ALTER TABLE dbo.CustomerInvoiceHeadIO
	DROP CONSTRAINT DF_CustomerInvoiceHeadIO_InvoiceFee
GO
ALTER TABLE dbo.CustomerInvoiceHeadIO
	DROP CONSTRAINT DF_CustomerInvoiceHeadIO_CentRounding
GO
ALTER TABLE dbo.CustomerInvoiceHeadIO
	DROP CONSTRAINT DF_CustomerInvoiceHeadIO_FullyPayed
GO
ALTER TABLE dbo.CustomerInvoiceHeadIO
	DROP CONSTRAINT DF_CustomerInvoiceHeadIO_ImportHeadType
GO
ALTER TABLE dbo.CustomerInvoiceHeadIO
	DROP CONSTRAINT DF_CustomerInvoiceHeadIO_State
GO
CREATE TABLE dbo.Tmp_CustomerInvoiceHeadIO
	(
	CustomerInvoiceHeadIOId int NOT NULL IDENTITY (1, 1),
	InvoiceId int NULL,
	CustomerInvoiceNr nvarchar(50) NULL,
	SeqNr int NULL,
	OCR nvarchar(50) NULL,
	ActorCompanyId int NOT NULL,
	Import bit NOT NULL,
	Type int NOT NULL,
	Status int NOT NULL,
	Source int NOT NULL,
	BatchId nvarchar(50) NOT NULL,
	RegistrationType int NOT NULL,
	OriginType int NOT NULL,
	OriginStatus int NOT NULL,
	InvoiceState int NULL,
	CustomerId int NULL,
	CustomerNr nvarchar(50) NULL,
	CustomerName nvarchar(250) NULL,
	PaymentCondition nvarchar(50) NULL,
	InvoiceDate datetime NULL,
	DueDate datetime NULL,
	VoucherDate datetime NULL,
	ReferenceOur nvarchar(200) NULL,
	ReferenceYour nvarchar(200) NULL,
	CurrencyRate decimal(15, 4) NULL,
	SumAmount decimal(10, 2) NULL,
	SumAmountCurrency decimal(10, 2) NULL,
	CurrencyDate datetime NULL,
	TotalAmount decimal(10, 2) NULL,
	TotalAmountCurrency decimal(10, 2) NULL,
	VatType int NULL,
	VATAmount decimal(10, 2) NULL,
	VATAmountCurrency decimal(10, 2) NULL,
	PaidAmount decimal(10, 2) NULL,
	PaidAmountCurrency decimal(10, 2) NULL,
	RemainingAmount decimal(10, 2) NULL,
	FreightAmount decimal(10, 2) NULL,
	FreightAmountCurrency decimal(10, 2) NULL,
	InvoiceFee decimal(10, 2) NULL,
	InvoiceFeeCurrency decimal(10, 2) NULL,
	CentRounding decimal(10, 2) NULL,
	FullyPayed bit NULL,
	PaymentNr nvarchar(100) NULL,
	VoucherNr nvarchar(50) NULL,
	CreateAccountingInXE bit NULL,
	Note nvarchar(MAX) NULL,
	BillingType int NULL,
	Currency nvarchar(50) NULL,
	TransferType nvarchar(50) NULL,
	ErrorMessage nvarchar(MAX) NULL,
	ImportHeadType int NOT NULL,
	ImportId int NULL,
	Created datetime NULL,
	CreatedBy nvarchar(50) NULL,
	Modified datetime NULL,
	ModifiedBy nvarchar(50) NULL,
	BillingAddressName nvarchar(100) NULL,
	BillingAddressAddress nvarchar(100) NULL,
	BillingAddressCO nvarchar(100) NULL,
	BillingAddressPostNr nvarchar(50) NULL,
	BillingAddressCity nvarchar(100) NULL,
	BillingAddressCountry nvarchar(100) NULL,
	DeliveryAddressName nvarchar(100) NULL,
	DeliveryAddressAddress nvarchar(100) NULL,
	DeliveryAddressCO nvarchar(100) NULL,
	DeliveryAddressPostNr nvarchar(50) NULL,
	DeliveryAddressCity nvarchar(100) NULL,
	DeliveryAddressCountry nvarchar(100) NULL,
	UseFixedPriceArticle bit NULL,
	VatRate1 decimal(10, 2) NULL,
	VatAmount1 decimal(10, 2) NULL,
	VatRate2 decimal(10, 2) NULL,
	VatAmount2 decimal(10, 2) NULL,
	VatRate3 decimal(10, 2) NULL,
	VatAmount3 decimal(10, 2) NULL,
	PaymentConditionCode nvarchar(50) NULL,
	DeliveryTypeCode nvarchar(50) NULL,
	DeliveryConditionCode nvarchar(50) NULL,
	SalesAccountNr nvarchar(50) NULL,
	SalesAccountNrDim2 nvarchar(50) NULL,
	SalesAccountNrDim3 nvarchar(50) NULL,
	SalesAccountNrDim4 nvarchar(50) NULL,
	SalesAccountNrDim5 nvarchar(50) NULL,
	SalesAccountNrDim6 nvarchar(50) NULL,
	ClaimAccountNr nvarchar(50) NULL,
	ClaimAccountNrDim2 nvarchar(50) NULL,
	ClaimAccountNrDim3 nvarchar(50) NULL,
	ClaimAccountNrDim4 nvarchar(50) NULL,
	ClaimAccountNrDim5 nvarchar(50) NULL,
	ClaimAccountNrDim6 nvarchar(50) NULL,
	VatAccountnr nvarchar(50) NULL,
	OrderNr nvarchar(50) NULL,
	OfferNr nvarchar(50) NULL,
	ContractNr nvarchar(50) NULL,
	Language nvarchar(50) NULL,
	isCaschSale bit NULL,
	InternalDescription nvarchar(MAX) NULL,
	ExternalDescription nvarchar(MAX) NULL,
	WorkingDescription nvarchar(MAX) NULL,
	ProjectNr nvarchar(50) NULL,
	State int NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceHeadIO SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceHeadIO ADD CONSTRAINT
	DF_CustomerInvoiceHeadIO_Import DEFAULT ((1)) FOR Import
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceHeadIO ADD CONSTRAINT
	DF_CustomerInvoiceHeadIO_TotalAmount DEFAULT ((0)) FOR TotalAmount
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceHeadIO ADD CONSTRAINT
	DF_CustomerInvoiceHeadIO_TotalAmountCurrency DEFAULT ((0)) FOR TotalAmountCurrency
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceHeadIO ADD CONSTRAINT
	DF_CustomerInvoiceHeadIO_VATAmount DEFAULT ((0)) FOR VATAmount
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceHeadIO ADD CONSTRAINT
	DF_CustomerInvoiceHeadIO_VATAmountCurrency DEFAULT ((0)) FOR VATAmountCurrency
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceHeadIO ADD CONSTRAINT
	DF_CustomerInvoiceHeadIO_PaidAmount DEFAULT ((0)) FOR PaidAmount
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceHeadIO ADD CONSTRAINT
	DF_CustomerInvoiceHeadIO_PaidAmountCurrency DEFAULT ((0)) FOR PaidAmountCurrency
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceHeadIO ADD CONSTRAINT
	DF_CustomerInvoiceHeadIO_FreightAmount DEFAULT ((0)) FOR FreightAmount
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceHeadIO ADD CONSTRAINT
	DF_CustomerInvoiceHeadIO_InvoiceFee DEFAULT ((0)) FOR InvoiceFee
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceHeadIO ADD CONSTRAINT
	DF_CustomerInvoiceHeadIO_CentRounding DEFAULT ((0)) FOR CentRounding
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceHeadIO ADD CONSTRAINT
	DF_CustomerInvoiceHeadIO_FullyPayed DEFAULT ((0)) FOR FullyPayed
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceHeadIO ADD CONSTRAINT
	DF_CustomerInvoiceHeadIO_ImportHeadType DEFAULT ((0)) FOR ImportHeadType
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceHeadIO ADD CONSTRAINT
	DF_CustomerInvoiceHeadIO_State DEFAULT ((0)) FOR State
GO
SET IDENTITY_INSERT dbo.Tmp_CustomerInvoiceHeadIO ON
GO
IF EXISTS(SELECT * FROM dbo.CustomerInvoiceHeadIO)
	 EXEC('INSERT INTO dbo.Tmp_CustomerInvoiceHeadIO (CustomerInvoiceHeadIOId, InvoiceId, CustomerInvoiceNr, SeqNr, OCR, ActorCompanyId, Import, Type, Status, Source, BatchId, RegistrationType, OriginType, OriginStatus, InvoiceState, CustomerId, CustomerNr, CustomerName, PaymentCondition, InvoiceDate, DueDate, VoucherDate, ReferenceOur, ReferenceYour, CurrencyRate, CurrencyDate, TotalAmount, TotalAmountCurrency, VatType, VATAmount, VATAmountCurrency, PaidAmount, PaidAmountCurrency, RemainingAmount, FreightAmount, FreightAmountCurrency, InvoiceFee, InvoiceFeeCurrency, CentRounding, FullyPayed, PaymentNr, VoucherNr, CreateAccountingInXE, Note, BillingType, Currency, TransferType, ErrorMessage, ImportHeadType, ImportId, Created, CreatedBy, Modified, ModifiedBy, BillingAddressName, BillingAddressAddress, BillingAddressCO, BillingAddressPostNr, BillingAddressCity, BillingAddressCountry, DeliveryAddressName, DeliveryAddressAddress, DeliveryAddressCO, DeliveryAddressPostNr, DeliveryAddressCity, DeliveryAddressCountry, UseFixedPriceArticle, VatRate1, VatAmount1, VatRate2, VatAmount2, VatRate3, VatAmount3, PaymentConditionCode, DeliveryTypeCode, DeliveryConditionCode, SalesAccountNr, SalesAccountNrDim2, SalesAccountNrDim3, SalesAccountNrDim4, SalesAccountNrDim5, SalesAccountNrDim6, ClaimAccountNr, ClaimAccountNrDim2, ClaimAccountNrDim3, ClaimAccountNrDim4, ClaimAccountNrDim5, ClaimAccountNrDim6, VatAccountnr, OrderNr, OfferNr, ContractNr, Language, isCaschSale, InternalDescription, ExternalDescription, WorkingDescription, ProjectNr, State)
		SELECT CustomerInvoiceHeadIOId, InvoiceId, CustomerInvoiceNr, SeqNr, OCR, ActorCompanyId, Import, Type, Status, Source, BatchId, RegistrationType, OriginType, OriginStatus, InvoiceState, CustomerId, CustomerNr, CustomerName, PaymentCondition, InvoiceDate, DueDate, VoucherDate, ReferenceOur, ReferenceYour, CurrencyRate, CurrencyDate, TotalAmount, TotalAmountCurrency, VatType, VATAmount, VATAmountCurrency, PaidAmount, PaidAmountCurrency, RemainingAmount, FreightAmount, FreightAmountCurrency, InvoiceFee, InvoiceFeeCurrency, CentRounding, FullyPayed, PaymentNr, VoucherNr, CreateAccountingInXE, Note, BillingType, Currency, TransferType, ErrorMessage, ImportHeadType, ImportId, Created, CreatedBy, Modified, ModifiedBy, BillingAddressName, BillingAddressAddress, BillingAddressCO, BillingAddressPostNr, BillingAddressCity, BillingAddressCountry, DeliveryAddressName, DeliveryAddressAddress, DeliveryAddressCO, DeliveryAddressPostNr, DeliveryAddressCity, DeliveryAddressCountry, UseFixedPriceArticle, VatRate1, VatAmount1, VatRate2, VatAmount2, VatRate3, VatAmount3, PaymentConditionCode, DeliveryTypeCode, DeliveryConditionCode, SalesAccountNr, SalesAccountNrDim2, SalesAccountNrDim3, SalesAccountNrDim4, SalesAccountNrDim5, SalesAccountNrDim6, ClaimAccountNr, ClaimAccountNrDim2, ClaimAccountNrDim3, ClaimAccountNrDim4, ClaimAccountNrDim5, ClaimAccountNrDim6, VatAccountnr, OrderNr, OfferNr, ContractNr, Language, isCaschSale, InternalDescription, ExternalDescription, WorkingDescription, ProjectNr, State FROM dbo.CustomerInvoiceHeadIO WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_CustomerInvoiceHeadIO OFF
GO
ALTER TABLE dbo.CustomerInvoiceRowIO
	DROP CONSTRAINT FK_CustomerInvoiceRowIO_CustomerInvoiceHeadIO
GO
DROP TABLE dbo.CustomerInvoiceHeadIO
GO
EXECUTE sp_rename N'dbo.Tmp_CustomerInvoiceHeadIO', N'CustomerInvoiceHeadIO', 'OBJECT' 
GO
ALTER TABLE dbo.CustomerInvoiceHeadIO ADD CONSTRAINT
	PK_CustomerInvoiceHeadIO PRIMARY KEY CLUSTERED 
	(
	CustomerInvoiceHeadIOId
	) WITH( PAD_INDEX = OFF, FILLFACTOR = 90, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX XE_Stats_ActorCompanyId_BatchId ON dbo.CustomerInvoiceHeadIO
	(
	ActorCompanyId,
	BatchId
	) INCLUDE (CustomerInvoiceHeadIOId, InvoiceId, CustomerInvoiceNr, SeqNr, RegistrationType, PaymentCondition, OCR, Import, Type, Status, Source, CurrencyRate, OriginType, OriginStatus, CustomerId, CustomerNr, CustomerName, PaidAmount, InvoiceDate, DueDate, VoucherDate, ReferenceOur, ReferenceYour, InvoiceFeeCurrency, CurrencyDate, TotalAmount, TotalAmountCurrency, VATAmount, VATAmountCurrency, Note, PaidAmountCurrency, RemainingAmount, FreightAmount, FreightAmountCurrency, InvoiceFee, ImportId, CentRounding, FullyPayed, PaymentNr, VoucherNr, CreateAccountingInXE, BillingAddressAddress, BillingType, Currency, TransferType, ErrorMessage, ImportHeadType, DeliveryAddressCity, Created, CreatedBy, Modified, ModifiedBy, State, VatRate3, BillingAddressCO, BillingAddressPostNr, BillingAddressCity, DeliveryAddressAddress, DeliveryAddressPostNr, VatAmount3, UseFixedPriceArticle, VatRate1, VatAmount1, VatRate2, VatAmount2) 
 WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.CustomerInvoiceRowIO
	DROP CONSTRAINT DF_CustomerInvoiceRowIO_Import
GO
ALTER TABLE dbo.CustomerInvoiceRowIO
	DROP CONSTRAINT DF_CustomerInvoiceRowIO_PurchasePrice
GO
ALTER TABLE dbo.CustomerInvoiceRowIO
	DROP CONSTRAINT DF_CustomerInvoiceRowIO_PurchasePrice1
GO
ALTER TABLE dbo.CustomerInvoiceRowIO
	DROP CONSTRAINT DF_CustomerInvoiceRowIO_Amount
GO
ALTER TABLE dbo.CustomerInvoiceRowIO
	DROP CONSTRAINT DF_CustomerInvoiceRowIO_AmountCurrency
GO
ALTER TABLE dbo.CustomerInvoiceRowIO
	DROP CONSTRAINT DF_CustomerInvoiceRowIO_VatAmountCurrency
GO
ALTER TABLE dbo.CustomerInvoiceRowIO
	DROP CONSTRAINT DF_CustomerInvoiceRowIO_DiscountAmountCurrency1
GO
ALTER TABLE dbo.CustomerInvoiceRowIO
	DROP CONSTRAINT DF_CustomerInvoiceRowIO_DiscountAmountCurrency
GO
ALTER TABLE dbo.CustomerInvoiceRowIO
	DROP CONSTRAINT DF_CustomerInvoiceRowIO_MarginalIncomeCurrency1
GO
ALTER TABLE dbo.CustomerInvoiceRowIO
	DROP CONSTRAINT DF_CustomerInvoiceRowIO_MarginalIncomeCurrency
GO
ALTER TABLE dbo.CustomerInvoiceRowIO
	DROP CONSTRAINT DF_CustomerInvoiceRowIO_SumAmountEntCurrency
GO
ALTER TABLE dbo.CustomerInvoiceRowIO
	DROP CONSTRAINT DF_CustomerInvoiceRowIO_SumAmountLedgerCurrency
GO
ALTER TABLE dbo.CustomerInvoiceRowIO
	DROP CONSTRAINT DF_CustomerInvoiceRowIO_ImportHeadType
GO
ALTER TABLE dbo.CustomerInvoiceRowIO
	DROP CONSTRAINT DF_CustomerInvoiceRowIO_State
GO
ALTER TABLE dbo.CustomerInvoiceRowIO
	DROP CONSTRAINT DF_CustomerInvoiceRowIO_VatRate
GO
CREATE TABLE dbo.Tmp_CustomerInvoiceRowIO
	(
	CustomerInvoiceRowIOId int NOT NULL IDENTITY (1, 1),
	CustomerInvoiceHeadIOId int NULL,
	InvoiceId int NULL,
	InvoiceRowId int NULL,
	InvoiceNr nvarchar(50) NULL,
	ActorCompanyId int NOT NULL,
	Import bit NOT NULL,
	Type int NOT NULL,
	Status int NOT NULL,
	Source int NOT NULL,
	BatchId nvarchar(50) NOT NULL,
	CustomerRowType int NOT NULL,
	ProductNr nvarchar(50) NULL,
	ProductName nvarchar(100) NULL,
	Quantity decimal(20, 6) NULL,
	UnitPrice decimal(10, 2) NULL,
	Discount decimal(10, 2) NULL,
	AccountNr nvarchar(50) NULL,
	AccountDim2Nr nvarchar(50) NULL,
	AccountDim3Nr nvarchar(50) NULL,
	AccountDim4Nr nvarchar(50) NULL,
	AccountDim5Nr nvarchar(50) NULL,
	AccountDim6Nr nvarchar(50) NULL,
	PurchasePrice decimal(10, 2) NULL,
	PurchasePriceCurrency decimal(10, 2) NULL,
	Amount decimal(10, 2) NULL,
	AmountCurrency decimal(10, 2) NULL,
	VatAmount decimal(10, 2) NULL,
	VatAmountCurrency decimal(10, 2) NULL,
	DiscountAmount decimal(10, 2) NULL,
	DiscountAmountCurrency decimal(10, 2) NULL,
	MarginalIncome decimal(10, 2) NULL,
	MarginalIncomeCurrency decimal(10, 2) NULL,
	SumAmount decimal(10, 2) NULL,
	SumAmountCurrency decimal(10, 2) NULL,
	Text nvarchar(512) NULL,
	ErrorMessage nvarchar(MAX) NULL,
	ImportHeadType int NOT NULL,
	ImportId int NULL,
	RowNr int NULL,
	Created datetime NULL,
	CreatedBy nvarchar(50) NULL,
	Modified datetime NULL,
	ModifiedBy nvarchar(50) NULL,
	State int NOT NULL,
	VatRate decimal(18, 2) NULL,
	ProductUnitId int NULL,
	Unit nvarchar(100) NULL,
	VatCode nvarchar(50) NULL,
	RowStatus nvarchar(50) NULL,
	InvoiceQuantity decimal(20, 6) NULL,
	PreviouslyInvoicedQuantity decimal(20, 6) NULL,
	RowDate datetime NULL,
	Stock nvarchar(100) NULL,
	ClaimAccountNr nvarchar(50) NULL,
	ClaimAccountNrDim2 nvarchar(50) NULL,
	ClaimAccountNrDim3 nvarchar(50) NULL,
	ClaimAccountNrDim4 nvarchar(50) NULL,
	ClaimAccountNrDim5 nvarchar(50) NULL,
	ClaimAccountNrDim6 nvarchar(50) NULL,
	VatAccountnr nvarchar(50) NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceRowIO SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceRowIO ADD CONSTRAINT
	DF_CustomerInvoiceRowIO_Import DEFAULT ((1)) FOR Import
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceRowIO ADD CONSTRAINT
	DF_CustomerInvoiceRowIO_PurchasePrice DEFAULT ((0)) FOR PurchasePrice
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceRowIO ADD CONSTRAINT
	DF_CustomerInvoiceRowIO_PurchasePrice1 DEFAULT ((0)) FOR PurchasePriceCurrency
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceRowIO ADD CONSTRAINT
	DF_CustomerInvoiceRowIO_Amount DEFAULT ((0)) FOR Amount
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceRowIO ADD CONSTRAINT
	DF_CustomerInvoiceRowIO_AmountCurrency DEFAULT ((0)) FOR AmountCurrency
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceRowIO ADD CONSTRAINT
	DF_CustomerInvoiceRowIO_VatAmountCurrency DEFAULT ((0)) FOR VatAmountCurrency
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceRowIO ADD CONSTRAINT
	DF_CustomerInvoiceRowIO_DiscountAmountCurrency1 DEFAULT ((0)) FOR DiscountAmount
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceRowIO ADD CONSTRAINT
	DF_CustomerInvoiceRowIO_DiscountAmountCurrency DEFAULT ((0)) FOR DiscountAmountCurrency
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceRowIO ADD CONSTRAINT
	DF_CustomerInvoiceRowIO_MarginalIncomeCurrency1 DEFAULT ((0)) FOR MarginalIncome
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceRowIO ADD CONSTRAINT
	DF_CustomerInvoiceRowIO_MarginalIncomeCurrency DEFAULT ((0)) FOR MarginalIncomeCurrency
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceRowIO ADD CONSTRAINT
	DF_CustomerInvoiceRowIO_SumAmountEntCurrency DEFAULT ((0)) FOR SumAmount
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceRowIO ADD CONSTRAINT
	DF_CustomerInvoiceRowIO_SumAmountLedgerCurrency DEFAULT ((0)) FOR SumAmountCurrency
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceRowIO ADD CONSTRAINT
	DF_CustomerInvoiceRowIO_ImportHeadType DEFAULT ((0)) FOR ImportHeadType
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceRowIO ADD CONSTRAINT
	DF_CustomerInvoiceRowIO_State DEFAULT ((0)) FOR State
GO
ALTER TABLE dbo.Tmp_CustomerInvoiceRowIO ADD CONSTRAINT
	DF_CustomerInvoiceRowIO_VatRate DEFAULT ((0)) FOR VatRate
GO
SET IDENTITY_INSERT dbo.Tmp_CustomerInvoiceRowIO ON
GO
IF EXISTS(SELECT * FROM dbo.CustomerInvoiceRowIO)
	 EXEC('INSERT INTO dbo.Tmp_CustomerInvoiceRowIO (CustomerInvoiceRowIOId, CustomerInvoiceHeadIOId, InvoiceId, InvoiceRowId, InvoiceNr, ActorCompanyId, Import, Type, Status, Source, BatchId, CustomerRowType, ProductNr, ProductName, Quantity, UnitPrice, Discount, AccountNr, AccountDim2Nr, AccountDim3Nr, AccountDim4Nr, AccountDim5Nr, AccountDim6Nr, PurchasePrice, PurchasePriceCurrency, Amount, AmountCurrency, VatAmount, VatAmountCurrency, DiscountAmount, DiscountAmountCurrency, MarginalIncome, MarginalIncomeCurrency, SumAmount, SumAmountCurrency, Text, ErrorMessage, ImportHeadType, ImportId, RowNr, Created, CreatedBy, Modified, ModifiedBy, State, VatRate, ProductUnitId, Unit, VatCode, RowStatus, InvoiceQuantity, PreviouslyInvoicedQuantity, Stock, ClaimAccountNr, ClaimAccountNrDim2, ClaimAccountNrDim3, ClaimAccountNrDim4, ClaimAccountNrDim5, ClaimAccountNrDim6, VatAccountnr)
		SELECT CustomerInvoiceRowIOId, CustomerInvoiceHeadIOId, InvoiceId, InvoiceRowId, InvoiceNr, ActorCompanyId, Import, Type, Status, Source, BatchId, CustomerRowType, ProductNr, ProductName, Quantity, UnitPrice, Discount, AccountNr, AccountDim2Nr, AccountDim3Nr, AccountDim4Nr, AccountDim5Nr, AccountDim6Nr, PurchasePrice, PurchasePriceCurrency, Amount, AmountCurrency, VatAmount, VatAmountCurrency, DiscountAmount, DiscountAmountCurrency, MarginalIncome, MarginalIncomeCurrency, SumAmount, SumAmountCurrency, Text, ErrorMessage, ImportHeadType, ImportId, RowNr, Created, CreatedBy, Modified, ModifiedBy, State, VatRate, ProductUnitId, Unit, VatCode, RowStatus, InvoiceQuantity, PreviouslyInvoicedQuantity, Stock, ClaimAccountNr, ClaimAccountNrDim2, ClaimAccountNrDim3, ClaimAccountNrDim4, ClaimAccountNrDim5, ClaimAccountNrDim6, VatAccountnr FROM dbo.CustomerInvoiceRowIO WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_CustomerInvoiceRowIO OFF
GO
DROP TABLE dbo.CustomerInvoiceRowIO
GO
EXECUTE sp_rename N'dbo.Tmp_CustomerInvoiceRowIO', N'CustomerInvoiceRowIO', 'OBJECT' 
GO
ALTER TABLE dbo.CustomerInvoiceRowIO ADD CONSTRAINT
	PK_CustomerInvoiceRowIO PRIMARY KEY CLUSTERED 
	(
	CustomerInvoiceRowIOId
	) WITH( PAD_INDEX = OFF, FILLFACTOR = 90, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX XE_Stats_CustomerInvoiceHeadIOId ON dbo.CustomerInvoiceRowIO
	(
	CustomerInvoiceHeadIOId
	) INCLUDE (CustomerInvoiceRowIOId, InvoiceId, InvoiceRowId, InvoiceNr, ActorCompanyId, Modified, ModifiedBy, State, VatRate, ProductUnitId, Unit, ErrorMessage, ImportHeadType, ImportId, RowNr, Created, CreatedBy, DiscountAmountCurrency, MarginalIncome, MarginalIncomeCurrency, SumAmount, SumAmountCurrency, Text, PurchasePriceCurrency, Amount, AmountCurrency, VatAmount, VatAmountCurrency, DiscountAmount, AccountDim2Nr, AccountDim3Nr, AccountDim4Nr, AccountDim5Nr, AccountDim6Nr, PurchasePrice, ProductNr, ProductName, Quantity, UnitPrice, Discount, AccountNr, Import, Type, Status, Source, BatchId, CustomerRowType) 
 WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE dbo.CustomerInvoiceRowIO ADD CONSTRAINT
	FK_CustomerInvoiceRowIO_CustomerInvoiceHeadIO FOREIGN KEY
	(
	CustomerInvoiceHeadIOId
	) REFERENCES dbo.CustomerInvoiceHeadIO
	(
	CustomerInvoiceHeadIOId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
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
ALTER TABLE dbo.CustomerInvoiceHeadIO ADD
	NextContractPeriodYear int NULL,
	NextContractPeriodValue int NULL,
	NextContractPeriodDate datetime NULL,
	ContractGroupId int NULL,
	ContractGroupName nvarchar(50) NULL,
	ContractGroupDecription nvarchar(512) NULL,
	ContractGroupPeriod nvarchar(50) NULL,
	ContractGroupInterval int NULL,
	ContractGroupDayInMonth int NULL,
	ContractGroupPriceManagementName nvarchar(50) NULL,
	ContractGroupInvoiceText nvarchar(MAX) NULL,
	ContractGroupInvoiceRowText nvarchar(MAX) NULL,
	ContractGroupOrderTemplate int NULL,
	ContractGroupInvoiceTemplate int NULL
GO
ALTER TABLE dbo.CustomerInvoiceHeadIO SET (LOCK_ESCALATION = TABLE)
GO
COMMIT

------------------------------------------------------------
-- This script is generated by SQLDBDiff V4.0
-- http://www.sqldbtools.com 
-- 2015-01-20 15:55:48
-- Note : 
--   Script generated by SQLDBDiff might need some adjustment, please review and test the code against test environements before deployement in production. 
------------------------------------------------------------

USE [Soecompv_ref]
GO

---$ Alter table dbo.AccountStd
IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE OBJECT_ID = OBJECT_ID(N'dbo.AccountStd') AND NAME = 'ExcludeVatVerification')
BEGIN
    PRINT 'Add column : dbo.AccountStd.ExcludeVatVerification'
    ALTER TABLE dbo.AccountStd
        ADD ExcludeVatVerification bit NULL
END
GO


---$ Alter table dbo.CustomerInvoicePrintedReminder
-- Diff on PK, related script will be generated if Indexes/PK is selected in Generate sync scripts form 
 GO

---$ Alter table dbo.StockTransaction
-- Diff on PK, related script will be generated if Indexes/PK is selected in Generate sync scripts form 
 GO

---$ Alter Index/PK: XE_AccountDimId_State, Table : dbo.Account
IF EXISTS(SELECT * FROM SYS.INDEXES WHERE OBJECT_ID = OBJECT_ID(N'dbo.Account') AND NAME = 'XE_AccountDimId_State')
BEGIN
    PRINT 'Drop index dbo.Account.XE_AccountDimId_State'
    DROP INDEX XE_AccountDimId_State ON dbo.Account
END
GO
IF NOT EXISTS(SELECT * FROM SYS.INDEXES WHERE OBJECT_ID = OBJECT_ID(N'dbo.Account') AND NAME = 'XE_AccountDimId_State')
BEGIN
    PRINT 'Create index : dbo.Account.XE_AccountDimId_State'
    CREATE  NONCLUSTERED INDEX XE_AccountDimId_State
        ON dbo.Account(AccountDimId, State) INCLUDE(AccountId,ModifiedBy,Name,CreatedBy,Modified,AccountNr,Created)
END
GO

---$ Alter Index/PK: XE_Q_AccountDimId_State_ActorCompanyId, Table : dbo.Account
IF EXISTS(SELECT * FROM SYS.INDEXES WHERE OBJECT_ID = OBJECT_ID(N'dbo.Account') AND NAME = 'XE_Q_AccountDimId_State_ActorCompanyId')
BEGIN
    PRINT 'Drop index dbo.Account.XE_Q_AccountDimId_State_ActorCompanyId'
    DROP INDEX XE_Q_AccountDimId_State_ActorCompanyId ON dbo.Account
END
GO
IF NOT EXISTS(SELECT * FROM SYS.INDEXES WHERE OBJECT_ID = OBJECT_ID(N'dbo.Account') AND NAME = 'XE_Q_AccountDimId_State_ActorCompanyId')
BEGIN
    PRINT 'Create index : dbo.Account.XE_Q_AccountDimId_State_ActorCompanyId'
    CREATE  NONCLUSTERED INDEX XE_Q_AccountDimId_State_ActorCompanyId
        ON dbo.Account(AccountDimId, State, ActorCompanyId) INCLUDE(AccountId,ModifiedBy,Name,CreatedBy,Modified,Created)
END
GO

---$ Alter Index/PK: XE_Q_State_ActorCompanyID, Table : dbo.Account
IF EXISTS(SELECT * FROM SYS.INDEXES WHERE OBJECT_ID = OBJECT_ID(N'dbo.Account') AND NAME = 'XE_Q_State_ActorCompanyID')
BEGIN
    PRINT 'Drop index dbo.Account.XE_Q_State_ActorCompanyID'
    DROP INDEX XE_Q_State_ActorCompanyID ON dbo.Account
END
GO
IF NOT EXISTS(SELECT * FROM SYS.INDEXES WHERE OBJECT_ID = OBJECT_ID(N'dbo.Account') AND NAME = 'XE_Q_State_ActorCompanyID')
BEGIN
    PRINT 'Create index : dbo.Account.XE_Q_State_ActorCompanyID'
    CREATE  NONCLUSTERED INDEX XE_Q_State_ActorCompanyID
        ON dbo.Account(State, ActorCompanyId) INCLUDE(AccountDimId,AccountId,Name,Modified,Created)
END
GO

---$ Create Index/PK: XE_Stats_ActorCompanyId_State, Table : dbo.Account
IF NOT EXISTS(SELECT * FROM SYS.INDEXES WHERE OBJECT_ID = OBJECT_ID(N'dbo.Account') AND NAME = 'XE_Stats_ActorCompanyId_State')
BEGIN
    PRINT 'Create index : dbo.Account.XE_Stats_ActorCompanyId_State'
    CREATE  NONCLUSTERED INDEX XE_Stats_ActorCompanyId_State
        ON dbo.Account(ActorCompanyId, State) INCLUDE(AccountDimId,AccountId,Name,Modified,ModifiedBy,CreatedBy,AccountNr,Created)
END
GO

---$ Alter Index/PK: XE_Stats_ParentId, Table : dbo.Category
IF EXISTS(SELECT * FROM SYS.INDEXES WHERE OBJECT_ID = OBJECT_ID(N'dbo.Category') AND NAME = 'XE_Stats_ParentId')
BEGIN
    PRINT 'Drop index dbo.Category.XE_Stats_ParentId'
    DROP INDEX XE_Stats_ParentId ON dbo.Category
END
GO
IF NOT EXISTS(SELECT * FROM SYS.INDEXES WHERE OBJECT_ID = OBJECT_ID(N'dbo.Category') AND NAME = 'XE_Stats_ParentId')
BEGIN
    PRINT 'Create index : dbo.Category.XE_Stats_ParentId'
    CREATE  NONCLUSTERED INDEX XE_Stats_ParentId
        ON dbo.Category(ParentId) INCLUDE(ActorCompanyId,CategoryId,Code,Name,State,Type)
END
GO

---$ Create Index/PK: PK_CustomerInvoicePrintedReminder, Table : dbo.CustomerInvoicePrintedReminder
IF OBJECT_ID(N'dbo.PK_CustomerInvoicePrintedReminder') IS NULL
BEGIN
    PRINT 'Add PK constraint : PK_CustomerInvoicePrintedReminder'
    ALTER TABLE dbo.CustomerInvoicePrintedReminder
        ADD CONSTRAINT PK_CustomerInvoicePrintedReminder PRIMARY KEY CLUSTERED(CustomerInvoicePrintedReminderId)
END
GO

---$ Create Index/PK: PK_StockTransaction, Table : dbo.StockTransaction
IF OBJECT_ID(N'dbo.PK_StockTransaction') IS NULL
BEGIN
    PRINT 'Add PK constraint : PK_StockTransaction'
    ALTER TABLE dbo.StockTransaction
        ADD CONSTRAINT PK_StockTransaction PRIMARY KEY CLUSTERED(StockTransactionId)
END
GO

---$ Create Index/PK: _dta_index_TimeScheduleTemplateBlock_6_2082926592__K23_K12_K6_K2_K1_K8_K5_3_4_7_9_10_11_13_14_15_16_17_18_19_20_21_22_24_25_26_, Table : dbo.TimeScheduleTemplateBlock
IF NOT EXISTS(SELECT * FROM SYS.INDEXES WHERE OBJECT_ID = OBJECT_ID(N'dbo.TimeScheduleTemplateBlock') AND NAME = '_dta_index_TimeScheduleTemplateBlock_6_2082926592__K23_K12_K6_K2_K1_K8_K5_3_4_7_9_10_11_13_14_15_16_17_18_19_20_21_22_24_25_26_')
BEGIN
    PRINT 'Create index : dbo.TimeScheduleTemplateBlock._dta_index_TimeScheduleTemplateBlock_6_2082926592__K23_K12_K6_K2_K1_K8_K5_3_4_7_9_10_11_13_14_15_16_17_18_19_20_21_22_24_25_26_'
    CREATE  NONCLUSTERED INDEX _dta_index_TimeScheduleTemplateBlock_6_2082926592__K23_K12_K6_K2_K1_K8_K5_3_4_7_9_10_11_13_14_15_16_17_18_19_20_21_22_24_25_26_
        ON dbo.TimeScheduleTemplateBlock(State, Date, EmployeeId, TimeScheduleTemplatePeriodId, TimeScheduleTemplateBlockId, ShiftTypeId, TimeCodeId)
END
GO

---$ Create Index/PK: _dta_index_TimeScheduleTemplateBlock_6_2082926592__K23_K24_K8_K6_K12_K26_K15_K10_K11_K3_1_2_4_5_7_9_13_14_16_17_18_19_20_21_22_, Table : dbo.TimeScheduleTemplateBlock
IF NOT EXISTS(SELECT * FROM SYS.INDEXES WHERE OBJECT_ID = OBJECT_ID(N'dbo.TimeScheduleTemplateBlock') AND NAME = '_dta_index_TimeScheduleTemplateBlock_6_2082926592__K23_K24_K8_K6_K12_K26_K15_K10_K11_K3_1_2_4_5_7_9_13_14_16_17_18_19_20_21_22_')
BEGIN
    PRINT 'Create index : dbo.TimeScheduleTemplateBlock._dta_index_TimeScheduleTemplateBlock_6_2082926592__K23_K24_K8_K6_K12_K26_K15_K10_K11_K3_1_2_4_5_7_9_13_14_16_17_18_19_20_21_22_'
    CREATE  NONCLUSTERED INDEX _dta_index_TimeScheduleTemplateBlock_6_2082926592__K23_K24_K8_K6_K12_K26_K15_K10_K11_K3_1_2_4_5_7_9_13_14_16_17_18_19_20_21_22_
        ON dbo.TimeScheduleTemplateBlock(State, TimeDeviationCauseId, ShiftTypeId, EmployeeId, Date, BreakType, ShiftStatus, StartTime, StopTime, TimeScheduleEmployeePeriodId)
END
GO

---$ Create Index/PK: _dta_index_TimeScheduleTemplateBlock_6_2082926592__K6_K12_K23_K24_K11_K10_K3, Table : dbo.TimeScheduleTemplateBlock
IF NOT EXISTS(SELECT * FROM SYS.INDEXES WHERE OBJECT_ID = OBJECT_ID(N'dbo.TimeScheduleTemplateBlock') AND NAME = '_dta_index_TimeScheduleTemplateBlock_6_2082926592__K6_K12_K23_K24_K11_K10_K3')
BEGIN
    PRINT 'Create index : dbo.TimeScheduleTemplateBlock._dta_index_TimeScheduleTemplateBlock_6_2082926592__K6_K12_K23_K24_K11_K10_K3'
    CREATE  NONCLUSTERED INDEX _dta_index_TimeScheduleTemplateBlock_6_2082926592__K6_K12_K23_K24_K11_K10_K3
        ON dbo.TimeScheduleTemplateBlock(EmployeeId, Date, State, TimeDeviationCauseId, StopTime, StartTime, TimeScheduleEmployeePeriodId)
END
GO

---$ Create Index/PK: _dta_index_TimeScheduleTemplateBlock_6_2082926592__K6_K23_K1_K12_K8_K5_K2_3_4_7_9_10_11_13_14_15_16_17_18_19_20_21_22_24_25_26_, Table : dbo.TimeScheduleTemplateBlock
IF NOT EXISTS(SELECT * FROM SYS.INDEXES WHERE OBJECT_ID = OBJECT_ID(N'dbo.TimeScheduleTemplateBlock') AND NAME = '_dta_index_TimeScheduleTemplateBlock_6_2082926592__K6_K23_K1_K12_K8_K5_K2_3_4_7_9_10_11_13_14_15_16_17_18_19_20_21_22_24_25_26_')
BEGIN
    PRINT 'Create index : dbo.TimeScheduleTemplateBlock._dta_index_TimeScheduleTemplateBlock_6_2082926592__K6_K23_K1_K12_K8_K5_K2_3_4_7_9_10_11_13_14_15_16_17_18_19_20_21_22_24_25_26_'
    CREATE  NONCLUSTERED INDEX _dta_index_TimeScheduleTemplateBlock_6_2082926592__K6_K23_K1_K12_K8_K5_K2_3_4_7_9_10_11_13_14_15_16_17_18_19_20_21_22_24_25_26_
        ON dbo.TimeScheduleTemplateBlock(EmployeeId, State, TimeScheduleTemplateBlockId, Date, ShiftTypeId, TimeCodeId, TimeScheduleTemplatePeriodId)
END
GO

---$ Create Index/PK: XE_Stats_Type_RecordId_IsBreak, Table : dbo.TimeScheduleTemplateBlockHistory
IF NOT EXISTS(SELECT * FROM SYS.INDEXES WHERE OBJECT_ID = OBJECT_ID(N'dbo.TimeScheduleTemplateBlockHistory') AND NAME = 'XE_Stats_Type_RecordId_IsBreak')
BEGIN
    PRINT 'Create index : dbo.TimeScheduleTemplateBlockHistory.XE_Stats_Type_RecordId_IsBreak'
    CREATE  NONCLUSTERED INDEX XE_Stats_Type_RecordId_IsBreak
        ON dbo.TimeScheduleTemplateBlockHistory(Type, RecordId, IsBreak)
END
GO

---$ Alter Index/PK: [<XE_V_Employeeid_Type_State, sysname,>], Table : dbo.TimeStampEntry
IF EXISTS(SELECT * FROM SYS.INDEXES WHERE OBJECT_ID = OBJECT_ID(N'dbo.TimeStampEntry') AND NAME = '[<XE_V_Employeeid_Type_State, sysname,>]')
BEGIN
    PRINT 'Drop index dbo.TimeStampEntry.[<XE_V_Employeeid_Type_State, sysname,>]'
    DROP INDEX [<XE_V_Employeeid_Type_State, sysname,>] ON dbo.TimeStampEntry
END
GO

---$ Create Index/PK: XE_FK_VoucherHeadIOId, Table : dbo.VoucherRowIO
IF NOT EXISTS(SELECT * FROM SYS.INDEXES WHERE OBJECT_ID = OBJECT_ID(N'dbo.VoucherRowIO') AND NAME = 'XE_FK_VoucherHeadIOId')
BEGIN
    PRINT 'Create index : dbo.VoucherRowIO.XE_FK_VoucherHeadIOId'
    CREATE  NONCLUSTERED INDEX XE_FK_VoucherHeadIOId
        ON dbo.VoucherRowIO(VoucherHeadIOId)
END
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
		 tbd.Date between @dateFrom and @dateTo AND
		 tpt.EmployeeId IN (SELECT * FROM SplitDelimiterString(@employeeIds, ',')) AND
		 tpt.state = 0
		 
		
		 ORDER BY 
		 tpt.EmployeeId

END


SET ANSI_NULLS ON

GO

---$ Alter Procedure dbo.SysProductSearch 
IF OBJECT_ID(N'dbo.SysProductSearch') IS NULL
BEGIN
    PRINT 'Create procedure : dbo.SysProductSearch'
    EXECUTE('CREATE PROCEDURE dbo.SysProductSearch AS RETURN 0') 
END
GO

PRINT 'Alter procedure : dbo.SysProductSearch'
GO

SET QUOTED_IDENTIFIER ON 
go

SET ANSI_NULLS ON 
go



ALTER PROCEDURE [dbo].[SysProductSearch]
	@actorCompanyId		int,
	@number				nvarchar(50) = NULL,
	@name				nvarchar(256) = NULL
AS -- WITH NO RECOMPILE TODO could improve performance
BEGIN
	SET NOCOUNT ON;

	-- Help tables
	DECLARE @t1 TABLE(splh int PRIMARY KEY, swid int)
	CREATE TABLE #t2 (spid int, swType int, PRIMARY KEY(spid, swType))

	-- Get used pricelists into a temp table variable
	INSERT INTO @t1 (splh, swid)
	SELECT DISTINCT
		SysPriceListHeadId, SysWholesellerId 
	FROM
		CompanyWholesellerPricelist WITH (NOLOCK)
	WHERE
		ActorCompanyId = @actorCompanyId
	--UNION SELECT DISTINCT
	--	PriceListImportedHead.PriceListImportedHeadId AS SysPriceListHeadId,
	--	pli.SysWholesellerId
	--FROM
	--	PriceListImportedHead
	--	INNER JOIN PriceListImported pli ON pli.PriceListImportedHeadId = PriceListImportedHead.PriceListImportedHeadId
	--WHERE
	--	ActorCompanyId = @actorCompanyId

	-- Get all products within used pricelists
	-- Insert their ID's into a temp table for faster search below
	INSERT INTO #t2 (spid, swType)
	SELECT DISTINCT
		pl.SysProductId,
		sw.[Type]
	FROM
		SoesysV2.dbo.SysPriceList pl WITH (NOLOCK)
		INNER JOIN soesysv2.dbo.SysWholeseller sw ON sw.SysWholesellerId = pl.SysWholesellerId
		--GROUP BY SysProductId, sw.[Type]
		--ORDER BY COUNT(*) desc
	WHERE
		SysPriceListHeadId IN (SELECT DISTINCT splh FROM @t1 where swid = pl.SysWholesellerId)
	--UNION SELECT DISTINCT
	--	ProductImportedId AS SysProductId,
	--	sw.[Type]
	--FROM
	--	dbo.PriceListImported pli WITH (NOLOCK)
	--	INNER JOIN soesysv2.dbo.SysWholeseller sw ON sw.SysWholesellerId = pli.SysWholesellerId
	--WHERE
	--	PriceListImportedHeadId IN (SELECT splh FROM @t1)

	-- Get external products
	SELECT TOP 500
		SoesysV2.dbo.SysProduct.SysProductId	AS 'ProductId',
		ProductId		AS 'Number',
		soesysv2.dbo.SysProduct.Name,
		1				AS 'PriceListOrigin',
	    w.[Type]
	FROM
		SoesysV2.dbo.SysProduct WITH (NOLOCK) 
		inner join Company on Company.ActorCompanyId = @actorCompanyId
		inner join soesysv2.dbo.SysPriceList pl on pl.SysProductId = SoesysV2.dbo.SysProduct.SysProductId
		inner join soesysv2.dbo.SysWholeseller w on w.SysWholesellerId = pl.SysWholesellerId
	WHERE
		SoesysV2.dbo.SysProduct.SysProductId IN (SELECT spid FROM #t2 where swType = w.[Type])
		AND [State] = 0
		AND w.[Type] = SoesysV2.dbo.SysProduct.[Type]
		AND ((Company.SysCountryId IS NULL) OR (soesysv2.dbo.SysProduct.SysCountryId = Company.SysCountryId)) -- Adding extra check that country is correct, this should not be neccessary if not db is corrupt
		AND ((@number IS NULL) OR (ProductId LIKE @Number+'%'))
		AND ((@name IS NULL) OR (soesysv2.dbo.SysProduct.Name LIKE '%'+@name+'%'))
	UNION
	SELECT TOP 500
		dbo.ProductImported.ProductImportedId	AS 'ProductId',
		dbo.ProductImported.ProductId			AS 'Number',
		dbo.ProductImported.Name,
		2					AS 'PriceListOrigin',
		w.Type as Type
	FROM
		dbo.ProductImported WITH (NOLOCK)
		INNER JOIN dbo.PriceListImported ON dbo.PriceListImported.ProductImportedId = dbo.ProductImported.ProductImportedId
		INNER JOIN dbo.PriceListImportedHead ON dbo.PriceListImportedHead.PriceListImportedHeadId = dbo.PriceListImported.PriceListImportedHeadId
		INNER JOIN soesysv2.dbo.SysWholeseller w on w.SysWholesellerId = dbo.PriceListImported.SysWholesellerId
	WHERE
		((@number IS NULL) OR (dbo.ProductImported.ProductId LIKE @Number+'%'))
		AND ((@name IS NULL) OR (dbo.ProductImported.Name LIKE '%'+@name+'%'))
		AND dbo.PriceListImportedHead.ActorCompanyId = @actorCompanyId
		AND w.[Type] = dbo.ProductImported.[Type]
	-- Clean up
	DELETE FROM @t1
	DROP TABLE #t2
END

GO

------------------------------------------------------------
-- This script is generated by SQLDBDiff V3.5
-- http://www.sqldbtools.com 
-- 2015-01-26 19:12:08
-- Notes : 
--   Script generate by SQLDBDiff may need some adjustement, code review the code before deployement in test environements 
--   Run your script against test environements before any deployement in production 
------------------------------------------------------------

USE [soecompv2]
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
		 tbd.Date between @dateFrom and @dateTo AND
		 tpt.EmployeeId IN (SELECT * FROM SplitDelimiterString(@employeeIds, ',')) AND
		 tpt.state = 0
		 
		
		 ORDER BY 
		 tpt.EmployeeId

END


SET ANSI_NULLS ON

GO

------------------------------------------------------------
-- This script is generated by SQLDBDiff V3.5
-- http://www.sqldbtools.com 
-- 2015-01-26 19:12:32
-- Notes : 
--   Script generate by SQLDBDiff may need some adjustement, code review the code before deployement in test environements 
--   Run your script against test environements before any deployement in production 
------------------------------------------------------------

USE [soecompv2]
GO

---$ Alter Procedure dbo.SysProductSearch 
IF OBJECT_ID(N'dbo.SysProductSearch') IS NULL
BEGIN
    PRINT 'Create procedure : dbo.SysProductSearch'
    EXECUTE('CREATE PROCEDURE dbo.SysProductSearch AS RETURN 0') 
END
GO

PRINT 'Alter procedure : dbo.SysProductSearch'
GO

SET QUOTED_IDENTIFIER ON 
go

SET ANSI_NULLS ON 
go




ALTER PROCEDURE [dbo].[SysProductSearch]
	@actorCompanyId		int,
	@number				nvarchar(50) = NULL,
	@name				nvarchar(256) = NULL
AS -- WITH NO RECOMPILE TODO could improve performance
BEGIN
	SET NOCOUNT ON;

	-- Help tables
	DECLARE @t1 TABLE(splh int PRIMARY KEY, swid int)
	CREATE TABLE #t2 (spid int, swType int, PRIMARY KEY(spid, swType))

	-- Get used pricelists into a temp table variable
	INSERT INTO @t1 (splh, swid)
	SELECT DISTINCT
		SysPriceListHeadId, SysWholesellerId 
	FROM
		CompanyWholesellerPricelist WITH (NOLOCK)
	WHERE
		ActorCompanyId = @actorCompanyId
	--UNION SELECT DISTINCT
	--	PriceListImportedHead.PriceListImportedHeadId AS SysPriceListHeadId,
	--	pli.SysWholesellerId
	--FROM
	--	PriceListImportedHead
	--	INNER JOIN PriceListImported pli ON pli.PriceListImportedHeadId = PriceListImportedHead.PriceListImportedHeadId
	--WHERE
	--	ActorCompanyId = @actorCompanyId

	-- Get all products within used pricelists
	-- Insert their ID's into a temp table for faster search below
	INSERT INTO #t2 (spid, swType)
	SELECT DISTINCT
		pl.SysProductId,
		sw.[Type]
	FROM
		SoesysV2.dbo.SysPriceList pl WITH (NOLOCK)
		INNER JOIN soesysv2.dbo.SysWholeseller sw ON sw.SysWholesellerId = pl.SysWholesellerId
		--GROUP BY SysProductId, sw.[Type]
		--ORDER BY COUNT(*) desc
	WHERE
		SysPriceListHeadId IN (SELECT DISTINCT splh FROM @t1 where swid = pl.SysWholesellerId)
	--UNION SELECT DISTINCT
	--	ProductImportedId AS SysProductId,
	--	sw.[Type]
	--FROM
	--	dbo.PriceListImported pli WITH (NOLOCK)
	--	INNER JOIN soesysv2.dbo.SysWholeseller sw ON sw.SysWholesellerId = pli.SysWholesellerId
	--WHERE
	--	PriceListImportedHeadId IN (SELECT splh FROM @t1)

	-- Get external products
	SELECT TOP 500
		SoesysV2.dbo.SysProduct.SysProductId	AS 'ProductId',
		ProductId		AS 'Number',
		soesysv2.dbo.SysProduct.Name,
		1				AS 'PriceListOrigin',
	    w.[Type]
	FROM
		SoesysV2.dbo.SysProduct WITH (NOLOCK) 
		inner join Company on Company.ActorCompanyId = @actorCompanyId
		inner join soesysv2.dbo.SysPriceList pl on pl.SysProductId = SoesysV2.dbo.SysProduct.SysProductId
		inner join soesysv2.dbo.SysWholeseller w on w.SysWholesellerId = pl.SysWholesellerId
	WHERE
		SoesysV2.dbo.SysProduct.SysProductId IN (SELECT spid FROM #t2 where swType = w.[Type])
		AND [State] = 0
		AND w.[Type] = SoesysV2.dbo.SysProduct.[Type]
		AND ((Company.SysCountryId IS NULL) OR (soesysv2.dbo.SysProduct.SysCountryId = Company.SysCountryId)) -- Adding extra check that country is correct, this should not be neccessary if not db is corrupt
		AND ((@number IS NULL) OR (ProductId LIKE @Number+'%'))
		AND ((@name IS NULL) OR (soesysv2.dbo.SysProduct.Name LIKE '%'+@name+'%'))
	UNION
	SELECT TOP 500
		dbo.ProductImported.ProductImportedId	AS 'ProductId',
		dbo.ProductImported.ProductId			AS 'Number',
		dbo.ProductImported.Name,
		2					AS 'PriceListOrigin',
		w.Type as Type
	FROM
		dbo.ProductImported WITH (NOLOCK)
		INNER JOIN dbo.PriceListImported ON dbo.PriceListImported.ProductImportedId = dbo.ProductImported.ProductImportedId
		INNER JOIN dbo.PriceListImportedHead ON dbo.PriceListImportedHead.PriceListImportedHeadId = dbo.PriceListImported.PriceListImportedHeadId
		INNER JOIN soesysv2.dbo.SysWholeseller w on w.SysWholesellerId = dbo.PriceListImported.SysWholesellerId
	WHERE
		((@number IS NULL) OR (dbo.ProductImported.ProductId LIKE @Number+'%'))
		AND ((@name IS NULL) OR (dbo.ProductImported.Name LIKE '%'+@name+'%'))
		AND dbo.PriceListImportedHead.ActorCompanyId = @actorCompanyId
		AND w.[Type] = dbo.ProductImported.[Type]
	-- Clean up
	DELETE FROM @t1
	DROP TABLE #t2
END

GO

------------------------------------------------------------
-- This script is generated by SQLDBDiff V3.5
-- http://www.sqldbtools.com 
-- 2015-01-26 19:14:48
-- Notes : 
--   Script generate by SQLDBDiff may need some adjustement, code review the code before deployement in test environements 
--   Run your script against test environements before any deployement in production 
------------------------------------------------------------

USE [soecompv2]
GO

---$ Alter table dbo.AccountStd
IF NOT EXISTS(SELECT * FROM SYS.COLUMNS WHERE OBJECT_ID = OBJECT_ID(N'dbo.AccountStd') AND NAME = 'ExcludeVatVerification')
BEGIN
    PRINT 'Add column : dbo.AccountStd.ExcludeVatVerification'
    ALTER TABLE dbo.AccountStd
        ADD ExcludeVatVerification bit NULL
END
GO


---$ Alter table dbo.CustomerInvoicePrintedReminder
-- Diff on PK, related script will be generated if Indexes/PK is selected in Generate sync scripts form 
 GO

---$ Alter table dbo.CustomerInvoiceRowIO
IF OBJECT_ID(N'dbo.DF_CustomerInvoiceRowIO_Import') IS NULL
BEGIN
    PRINT 'Add constraint DF_CustomerInvoiceRowIO_Import'
    ALTER TABLE dbo.CustomerInvoiceRowIO
        ADD CONSTRAINT DF_CustomerInvoiceRowIO_Import DEFAULT (1) FOR Import
END
GO

IF OBJECT_ID(N'dbo.DF_CustomerInvoiceRowIO_PurchasePrice') IS NULL
BEGIN
    PRINT 'Add constraint DF_CustomerInvoiceRowIO_PurchasePrice'
    ALTER TABLE dbo.CustomerInvoiceRowIO
        ADD CONSTRAINT DF_CustomerInvoiceRowIO_PurchasePrice DEFAULT (0) FOR PurchasePrice
END
GO


---$ Alter table dbo.StockTransaction
-- Diff on PK, related script will be generated if Indexes/PK is selected in Generate sync scripts form 
 GO


