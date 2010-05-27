SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE Proc [dbo].[cust_hdc_checkin_sp_save_template_location]
@OccurrenceTypeTemplateId int,
@LocationId int

AS

	DECLARE @UpdateDateTime DateTime SET @UpdateDateTime = GETDATE()
		
	IF NOT EXISTS(
		SELECT * FROM cust_hdc_checkin_occurrence_type_template_location
		WHERE [occurrence_type_template_id] = @OccurrenceTypeTemplateId
		  AND [location_id] = @LocationId
		)
		
	BEGIN
	
		INSERT INTO cust_hdc_checkin_occurrence_type_template_location
		(	
			 [occurrence_type_template_id]
			,[location_id]
		)
		values
		(	
			 @OccurrenceTypeTemplateId
			,@LocationId
		)

	END

