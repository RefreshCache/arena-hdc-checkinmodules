/****** Object:  Table [dbo].[cust_hdc_checkin_display_group_system]    Script Date: 12/04/2009 15:10:13 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[cust_hdc_checkin_display_group_system](
	[system_id] [int] NOT NULL,
	[display_group_id] [int] NOT NULL,
 CONSTRAINT [PK_cust_hdc_checkin_display_group_system] PRIMARY KEY CLUSTERED 
(
	[system_id] ASC,
	[display_group_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[cust_hdc_checkin_display_group_system]  WITH CHECK ADD  CONSTRAINT [FK_cust_hdc_checkin_display_group_system_comp_system] FOREIGN KEY([system_id])
REFERENCES [dbo].[comp_system] ([system_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[cust_hdc_checkin_display_group_system] CHECK CONSTRAINT [FK_cust_hdc_checkin_display_group_system_comp_system]
GO
ALTER TABLE [dbo].[cust_hdc_checkin_display_group_system]  WITH CHECK ADD  CONSTRAINT [FK_cust_hdc_checkin_display_group_system_cust_hdc_checkin_display_group] FOREIGN KEY([display_group_id])
REFERENCES [dbo].[cust_hdc_checkin_display_group] ([display_group_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[cust_hdc_checkin_display_group_system] CHECK CONSTRAINT [FK_cust_hdc_checkin_display_group_system_cust_hdc_checkin_display_group]