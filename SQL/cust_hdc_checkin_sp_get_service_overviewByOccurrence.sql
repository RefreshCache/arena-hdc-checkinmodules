SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE xtype='P' AND name='cust_hdc_checkin_sp_get_service_overviewByOccurrence')
BEGIN
	CREATE Proc [dbo].[cust_hdc_checkin_sp_get_service_overviewByOccurrence]
		@group_id int,
		@service_time datetime
	AS
	BEGIN
		SELECT  [co].[occurrence_id],[co].[occurrence_name],[co].[occurrence_start_time]
			,[co].[occurrence_end_time],[co].[check_in_start],[co].[check_in_end]
			,[co].[occurrence_closed]
			,[co].[location_id]
			,[ol].[building_name],[ol].[location_name]
			,[ol].[building_name] + ' - ' + [ol].[location_name] AS 'building_location_name'
			,[cotg].[group_id],[cot].[occurrence_type_id]
			,[cotg].[group_name],[cot].[type_name]
			,[cotg].[group_name] + ' - ' + [cot].[type_name] AS 'occurrence_group_type_name'
			,(SELECT COUNT([coa].[person_id]) FROM core_occurrence_attendance AS [coa] WHERE [coa].[occurrence_id] = [co].[occurrence_id]) AS 'attendance_count'
		FROM core_occurrence AS [co]
			LEFT JOIN core_occurrence_type AS [cot] ON [cot].[occurrence_type_id] = [co].[occurrence_type]
			LEFT JOIN core_occurrence_type_group AS [cotg] ON [cotg].[group_id] = [cot].[group_id]
			LEFT JOIN orgn_location AS [ol] ON [ol].[location_id] = [co].[location_id]
		WHERE [cot].[group_id] = @group_id
			AND [co].[occurrence_start_time] = @service_time
	END
END
GO
