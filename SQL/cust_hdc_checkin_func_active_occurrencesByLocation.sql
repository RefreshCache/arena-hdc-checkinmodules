SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE xtype='IF' AND name='cust_hdc_checkin_func_active_occurrencesByLocation')
BEGIN
	CREATE FUNCTION [dbo].[cust_hdc_checkin_func_active_occurrencesByLocation]
		(@location_id int)
		RETURNS TABLE
	AS
		RETURN SELECT [co].[occurrence_id]
				FROM core_occurrence AS [co]
				WHERE [co].[location_id] = @location_id
					AND [co].[occurrence_closed] = 0
					AND (([co].[occurrence_start_time] <= GETDATE() AND [co].[occurrence_end_time] >= GETDATE())
						OR ([co].[check_in_start] <= GETDATE() AND [co].[check_in_end] >= GETDATE()))
END
GO
