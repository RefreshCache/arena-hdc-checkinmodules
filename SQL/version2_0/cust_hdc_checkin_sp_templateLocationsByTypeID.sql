SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT 1 FROM sysobjects WHERE xtype='P' AND name='cust_hdc_checkin_sp_templateLocationsByTypeID')
	DROP PROCEDURE [dbo].[cust_hdc_checkin_sp_templateLocationsByTypeID]
GO

CREATE PROCEDURE [cust_hdc_checkin_sp_templateLocationsByTypeID]
	@TypeID int
AS
BEGIN
	SELECT [cott].[occurrence_type_template_id],[cott].[schedule_name],[cott].[start_time],[cott].[check_in_start],[cott].[occurrence_freq_type],[cott].[freq_qualifier]
			,[ol].[location_id],[ol].[building_name],[ol].[location_name]
		FROM [dbo].[core_occurrence_type_template] AS [cott]
		INNER JOIN [dbo].[core_occurrence_type] AS [cot] ON [cott].[occurrence_type_id] = [cot].[occurrence_type_id]
		INNER JOIN [dbo].[orgn_location_occurrence_type] AS [olot] ON [olot].[occurrence_type_id] = [cot].[occurrence_type_id]
		INNER JOIN [dbo].[orgn_location] AS [ol] ON [olot].[location_id] = [ol].[location_id]
		WHERE [cot].[occurrence_type_id] = @TypeID
			AND (
				EXISTS (SELECT [ottl].[location_id] FROM [dbo].[cust_hdc_checkin_occurrence_type_template_location] AS [ottl] WHERE [ottl].[occurrence_type_template_id] = [cott].[occurrence_type_template_id] AND [ottl].[location_id] = [ol].[location_id])
				OR NOT EXISTS (SELECT [ottl].[location_id] FROM [dbo].[cust_hdc_checkin_occurrence_type_template_location] AS [ottl] WHERE [ottl].[occurrence_type_template_id] = [cott].[occurrence_type_template_id]))
END
GO
