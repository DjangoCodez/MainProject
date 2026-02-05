USE [SOESysV2]
GO

INSERT INTO SysPayrollPrice (SysCountryId, SysTermId, [Type], Code, Amount, AmountType, FromDate, [State])
VALUES (1, 27, 1, 'UNG', 18782, 0, '2014-01-01', 0)

INSERT INTO SysPayrollPrice (SysCountryId, SysTermId, [Type], Code, Amount, AmountType, FromDate, [State])
VALUES (1, 27, 1, 'UNG', 18824, 0, '2015-01-01', 0)

USE [SOECompv2]
GO

/****** Object:  StoredProcedure [dbo].[GetEdiEntriesAllPerLicenseId]    Script Date: 13.2.2015 14:52:34 ******/
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
    select v.Type, l.LicenseId, l.Name, COUNT(v.EdiEntryId) as Items
	from dbo.EdiEntry as v, dbo.Company as c, dbo.License as l
	where 
	v.Created >= @startingday and v.Created < @endingday+1
	and c.ActorCompanyId = v.ActorCompanyId 
	and l.LicenseId = c.LicenseId
	group by l.LicenseId, l.Name, v.Type
	order by l.LicenseId, v.Type

END


GO


