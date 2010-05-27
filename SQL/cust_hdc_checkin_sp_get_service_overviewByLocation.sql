SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE xtype='P' AND name='cust_hdc_checkin_sp_get_service_overviewByLocation')
BEGIN
	CREATE Proc [dbo].[cust_hdc_checkin_sp_get_service_overviewByLocation]
		@group_id int,
		@service_time datetime
	AS
	BEGIN
		SELECT   [ol].[location_id]
			,[ol].[building_name],[ol].[location_name]
			,[ol].[building_name] + ' - ' + [ol].[location_name] AS 'building_location_name'
			,(SELECT COUNT([coa].[person_id])
				FROM core_occurrence_attendance AS [coa]
				LEFT JOIN core_occurrence AS [co] ON [co].[occurrence_id] = [coa].[occurrence_id]
				LEFT JOIN core_occurrence_type AS [cot] ON [cot].[occurrence_type_id] = [co].[occurrence_type]
				WHERE [cot].[group_id] = @group_id
					AND [co].[occurrence_start_time] = @service_time
					AND [co].[location_id] = [ol].[location_id]) AS 'attendance_count'
			,(SELECT COUNT([co].[location_id])
				FROM core_occurrence AS [co]
				LEFT JOIN core_occurrence_type AS [cot] ON [cot].[occurrence_type_id] = [co].[occurrence_type]
				WHERE ([cot].[group_id] = @group_id AND [co].[occurrence_start_time] = @service_time)) AS 'occurrence_count'
			,(SELECT COUNT(*) FROM cust_hdc_checkin_func_active_occurrencesByLocation([ol].[location_id])) AS 'active_occurrence_count'
		FROM orgn_location AS [ol]
		WHERE (SELECT COUNT([co].[location_id])
			FROM core_occurrence AS [co]
			LEFT JOIN core_occurrence_type AS [cot] ON [cot].[occurrence_type_id] = [co].[occurrence_type]
			WHERE [cot].[group_id] = @group_id
				AND [co].[occurrence_start_time] = @service_time
				AND [co].[location_id] = [ol].[location_id]) > 0
	END
END
GO
