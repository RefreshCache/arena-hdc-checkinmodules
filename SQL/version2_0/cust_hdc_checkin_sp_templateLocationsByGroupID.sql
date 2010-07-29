SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT 1 FROM sysobjects WHERE xtype='P' AND name='cust_hdc_checkin_sp_templateLocationsByGroupID')
	DROP PROCEDURE [dbo].[cust_hdc_checkin_sp_templateLocationsByGroupID]
GO

CREATE PROCEDURE [dbo].[cust_hdc_checkin_sp_templateLocationsByGroupID]
	@GroupID int
AS
BEGIN
	SELECT [cott].[schedule_name],[cott].[start_time],[cott].[occurrence_freq_type],[cott].[freq_qualifier]
			,[ol].[building_name],[ol].[location_name]
		FROM [dbo].[cust_hdc_checkin_occurrence_type_template_location] AS [ottl]
		INNER JOIN [dbo].[core_occurrence_type_template] AS [cott] ON [ottl].[occurrence_type_template_id] = [cott].[occurrence_type_template_id]
		INNER JOIN [dbo].[core_occurrence_type] AS [cot] ON [cott].[occurrence_type_id] = [cot].[occurrence_type_id]
		INNER JOIN [dbo].[orgn_location] AS [ol] ON [ottl].[location_id] = [ol].[location_id]
		WHERE [cot].[group_id] = @GroupID
END
GO
