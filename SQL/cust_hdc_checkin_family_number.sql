SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE xtype='U' AND name='cust_hdc_checkin_family_number')
BEGIN
	CREATE TABLE [dbo].[cust_hdc_checkin_family_number]
	(
		[family_number] [int] IDENTITY(2000,1) NOT NULL,
		[date_created] [datetime] NOT NULL,
		CONSTRAINT [PK_cust_hdc_checkin_family_number] PRIMARY KEY CLUSTERED
		(
			[family_number] ASC
		) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[cust_hdc_checkin_family_number] ADD CONSTRAINT [DF_cust_hdc_checkin_family_number_date_created] DEFAULT (getdate()) FOR [date_created]
END
GO
