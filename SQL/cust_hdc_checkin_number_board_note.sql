/****** Object:  Table [dbo].[cust_hdc_checkin_number_board_note]    Script Date: 12/04/2009 15:10:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[cust_hdc_checkin_number_board_note](
	[note_id] [int] IDENTITY(1,1) NOT NULL,
	[note_html] [varchar](max) NULL,
	[note_image] [uniqueidentifier] NULL,
	[note_info] [varchar](max) NULL,
	[system_id] [int] NULL,
	[display_group_id] [int] NULL,
	[user_info] [int] NULL,
	[date_created] [datetime] NOT NULL CONSTRAINT [DF_cust_hdc_checkin_number_board_note_date_created]  DEFAULT (getdate()),
	[date_modified] [datetime] NOT NULL CONSTRAINT [DF_cust_hdc_checkin_number_board_note_date_modified]  DEFAULT (getdate()),
	[created_by] [varchar](50) NULL,
	[modified_by] [varchar](50) NULL,
 CONSTRAINT [PK_cust_hdc_checkin_number_board_note] PRIMARY KEY CLUSTERED 
(
	[note_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[cust_hdc_checkin_number_board_note]  WITH CHECK ADD  CONSTRAINT [FK_cust_hdc_checkin_number_board_note_comp_system] FOREIGN KEY([system_id])
REFERENCES [dbo].[comp_system] ([system_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[cust_hdc_checkin_number_board_note] CHECK CONSTRAINT [FK_cust_hdc_checkin_number_board_note_comp_system]
GO
ALTER TABLE [dbo].[cust_hdc_checkin_number_board_note]  WITH CHECK ADD  CONSTRAINT [FK_cust_hdc_checkin_number_board_note_cust_hdc_checkin_display_group] FOREIGN KEY([display_group_id])
REFERENCES [dbo].[cust_hdc_checkin_display_group] ([display_group_id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[cust_hdc_checkin_number_board_note] CHECK CONSTRAINT [FK_cust_hdc_checkin_number_board_note_cust_hdc_checkin_display_group]