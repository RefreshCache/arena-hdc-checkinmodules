SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE xtype='P' AND name='cust_hdc_checkin_sp_delete_locationsByTemplateID')
BEGIN
	CREATE PROC cust_hdc_checkin_sp_delete_locationsByTemplateID
	    @TemplateID int
	AS
	BEGIN
		DELETE FROM cust_hdc_checkin_occurrence_type_template_location
			WHERE occurrence_type_template_id = @TemplateID
	END
END
GO
