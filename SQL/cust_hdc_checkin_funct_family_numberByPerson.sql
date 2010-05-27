SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/*
FUNCTION: cust_hdc_checkin_funct_family_number_check
AUTHOR:  Jeremy Stone
DATE CREATED:  8/22/2009
DESCRIPTION:  This function is used to get the 
			  Person's Family Number if it exists
			  If the Family Number does not exist
			  RETURN 0

*/
IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE xtype='FN' AND name='cust_hdc_checkin_funct_family_numberByPerson')
BEGIN
	CREATE FUNCTION [dbo].[cust_hdc_checkin_funct_family_numberByPerson]
	(
		@person_id int
		, @attribute_id int
	)
	RETURNS INT
	AS
	BEGIN
		DECLARE @FamilyNumber int
		SET @FamilyNumber = 0

		SELECT @FamilyNumber = ISNULL(int_value, 0) 
		  FROM core_person_attribute 
		 WHERE attribute_id = @attribute_id
		   AND person_id = @person_id

		RETURN @FamilyNumber
	END
END
GO
