SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:	Jeremy Stone
-- Create date: 8/22/2009
-- Description:	Returns number of Occurrences 
--              in the past 12 months
-- =============================================
IF EXISTS (SELECT 1 FROM sysobjects WHERE xtype='FN' AND name='cust_hdc_checkin_funct_number_of_occurrences')
	DROP FUNCTION [dbo].[cust_hdc_checkin_funct_number_of_occurrences]
GO

CREATE FUNCTION [dbo].[cust_hdc_checkin_funct_number_of_occurrences]
(
	@PersonID int,
	@attendanceMonths int = 12
)
RETURNS int
AS
BEGIN
	DECLARE @NumOfOccurrences int

	SET @NumOfOccurrences = 0	

	SELECT @NumOfOccurrences = COUNT(coa.occurrence_attendance_id) 
	  FROM [core_occurrence_attendance] coa 
	INNER JOIN [core_occurrence] co ON coa.occurrence_id = co.occurrence_id
	 WHERE co.occurrence_start_time > DateAdd(month, -@attendanceMonths, getDate()) 
	   AND coa.person_id = @PersonID

	RETURN @NumOfOccurrences
END

GO


