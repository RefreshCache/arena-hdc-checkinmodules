/****** Object:  Table [dbo].[cust_hdc_checkin_occurrence_type_template_location]    Script Date: 10/22/2009 13:19:46 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[cust_hdc_checkin_occurrence_type_template_location](
	[occurrence_type_template_id] [int] NOT NULL,
	[location_id] [int] NOT NULL,
 CONSTRAINT [PK_cust_hdc_checkin_occurrence_type_template_location] PRIMARY KEY CLUSTERED 
(
	[occurrence_type_template_id] ASC,
	[location_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Links an occurrence type template to a list of locations that should be created.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'cust_hdc_checkin_occurrence_type_template_location'
GO

ALTER TABLE [dbo].[cust_hdc_checkin_occurrence_type_template_location]  WITH CHECK ADD  CONSTRAINT [FK_cust_hdc_checkin_occurrence_type_template_location_core_occurrence_type_template] FOREIGN KEY([occurrence_type_template_id])
REFERENCES [dbo].[core_occurrence_type_template] ([occurrence_type_template_id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[cust_hdc_checkin_occurrence_type_template_location] CHECK CONSTRAINT [FK_cust_hdc_checkin_occurrence_type_template_location_core_occurrence_type_template]
GO

ALTER TABLE [dbo].[cust_hdc_checkin_occurrence_type_template_location]  WITH CHECK ADD  CONSTRAINT [FK_cust_hdc_checkin_occurrence_type_template_location_orgn_location] FOREIGN KEY([location_id])
REFERENCES [dbo].[orgn_location] ([location_id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[cust_hdc_checkin_occurrence_type_template_location] CHECK CONSTRAINT [FK_cust_hdc_checkin_occurrence_type_template_location_orgn_location]
GO


