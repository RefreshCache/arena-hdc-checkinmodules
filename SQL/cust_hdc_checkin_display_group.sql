/****** Object:  Table [dbo].[cust_hdc_checkin_display_group]    Script Date: 12/04/2009 15:10:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[cust_hdc_checkin_display_group](
	[display_group_id] [int] IDENTITY(1,1) NOT NULL,
	[display_group_name] [varchar](150) NOT NULL,
	[display_group_description] [varchar](max) NOT NULL DEFAULT (''),
	[date_created] [datetime] NOT NULL CONSTRAINT [DF_cust_hdc_checkin_display_group_date_created]  DEFAULT (getdate()),
	[date_modified] [datetime] NOT NULL CONSTRAINT [DF_cust_hdc_checkin_display_group_date_modified]  DEFAULT (getdate()),
	[created_by] [varchar](50) NULL,
	[modified_by] [varchar](50) NULL,
 CONSTRAINT [PK_cust_hdc_checkin_display_group] PRIMARY KEY CLUSTERED 
(
	[display_group_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF