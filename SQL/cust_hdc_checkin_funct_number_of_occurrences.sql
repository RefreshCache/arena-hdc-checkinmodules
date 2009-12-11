/****** Object:  UserDefinedFunction [dbo].[cust_hdc_checkin_funct_number_of_occurrences]    Script Date: 12/03/2009 15:33:18 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Jeremy Stone
-- Create date: 8/22/2009
-- Description:	Returns number of Occurrences 
--              in the past 12 months
-- =============================================
CREATE FUNCTION [dbo].[cust_hdc_checkin_funct_number_of_occurrences]
(
	-- Add the parameters for the function here
	@PersonID int,
	@attendanceMonths int = 12
)
RETURNS int
AS
BEGIN
	-- Declare the return variable here
	DECLARE @NumOfOccurrences int

	SET @NumOfOccurrences = 0	

	SELECT @NumOfOccurrences = COUNT(coa.occurrence_attendance_id) 
	  FROM [core_occurrence_attendance] coa 
		INNER JOIN [core_occurrence] co ON coa.occurrence_id = co.occurrence_id
	 WHERE co.occurrence_start_time > DateAdd(month, -@attendanceMonths, getDate()) 
	   AND coa.person_id = @PersonID

	-- Return the result of the function
	RETURN @NumOfOccurrences

END

GO


