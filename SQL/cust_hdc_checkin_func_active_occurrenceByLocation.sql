/****** Object:  UserDefinedFunction [dbo].[cust_hdc_checkin_func_active_occurrencesByLocation]    Script Date: 10/27/2009 10:01:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
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
