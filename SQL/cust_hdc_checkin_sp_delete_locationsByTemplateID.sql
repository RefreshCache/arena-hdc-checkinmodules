CREATE PROC cust_hdc_checkin_sp_delete_locationsByTemplateID
    @TemplateID int
AS
	DELETE FROM cust_hdc_checkin_occurrence_type_template_location
		WHERE occurrence_type_template_id = @TemplateID