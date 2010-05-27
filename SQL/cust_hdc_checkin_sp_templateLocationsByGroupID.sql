SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE xtype='P' AND name='cust_hdc_checkin_sp_templateLocationsByGroupID')
BEGIN
	CREATE PROC [cust_hdc_checkin_sp_templateLocationsByGroupID]
		@GroupID int
	AS
		SELECT [cott].[schedule_name],[cott].[start_time],[cott].[occurrence_freq_type],[cott].[freq_qualifier]
				,[ol].[building_name],[ol].[location_name]
			FROM [ArenaDB].[dbo].[cust_hdc_checkin_occurrence_type_template_location] AS [ottl]
			INNER JOIN [ArenaDB].[dbo].[core_occurrence_type_template] AS [cott] ON [ottl].[occurrence_type_template_id] = [cott].[occurrence_type_template_id]
			INNER JOIN [ArenaDB].[dbo].[core_occurrence_type] AS [cot] ON [cott].[occurrence_type_id] = [cot].[occurrence_type_id]
			INNER JOIN [ArenaDB].[dbo].[orgn_location] AS [ol] ON [ottl].[location_id] = [ol].[location_id]
			WHERE [cot].[group_id] = @GroupID
	END
END
GO
