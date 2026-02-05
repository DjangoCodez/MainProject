USE [soecompv2]
GO


/****** Object:  Index [XE_Q_IsPreliminary]    Script Date: 2014-11-17 16:23:41 ******/
CREATE NONCLUSTERED INDEX [XE_Q_IsPreliminary] ON [dbo].[TimeScheduleEmployeePeriod]
(
	[TimeScheduleEmployeePeriodId] ASC
)
INCLUDE ( 	[IsPreliminary]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO


