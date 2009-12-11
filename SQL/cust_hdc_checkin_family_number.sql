/****** Object:  Table [dbo].[cust_hdc_checkin_family_number]    Script Date: 12/03/2009 15:34:10 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[cust_hdc_checkin_family_number](
	[family_number] [int] IDENTITY(2000,1) NOT NULL,
	[date_created] [datetime] NOT NULL,
 CONSTRAINT [PK_cust_hdc_checkin_family_number] PRIMARY KEY CLUSTERED 
(
	[family_number] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[cust_hdc_checkin_family_number] ADD  CONSTRAINT [DF_cust_hdc_checkin_family_number_date_created]  DEFAULT (getdate()) FOR [date_created]
GO


