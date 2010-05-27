SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/*
PROCEDURE: exec dbo.cust_hdc_checkin_sp_assign_family_numbers
AUTHOR:  Jeremy Stone
DATE CREATED: 8/22/2009
DESCRIPTION:  Used by a job to assign family numbers
			   to individuals age 12 or younger that
				have attended 3 or more times in the 
				past 12 months
*/
IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE xtype='U' AND name='cust_hdc_checkin_sp_assign_family_numbers')
BEGIN
	CREATE PROCEDURE [dbo].[cust_hdc_checkin_sp_assign_family_numbers]
	(
		@maxAge int = 12
		, @minAttendance int = 3
		, @attendanceMonths int = 12
		, @attribute_id int
	)
	AS
	BEGIN
		--Commented Fields are for testing purposes
		DECLARE @cust_temp_family_number_assign TABLE(id int identity(1,1), person_id INT, family_id INT)
		DECLARE @LCV int
		DECLARE @NumOfRecords int
		DECLARE @FamilyID int
		DECLARE @ChildID int
		DECLARE @FamilyNumber int
		DECLARE @Error int

		SET @Error = 0

		--
		-- Create temporary table to link people with their family IDs. Include only
		-- people who do not have a family number, are 12 years old or younger and
		-- have attendended at least 3 occurrences.
		--
		INSERT INTO @cust_temp_family_number_assign 
			(person_id, family_id)
			SELECT c.[person_id],d.[family_id]
			  FROM [core_person] c
			  INNER JOIN [core_family_member] d ON c.person_id = d.person_id
			WHERE dbo.cust_hdc_checkin_funct_family_numberByPerson(c.person_id, @attribute_id) = 0
			  AND DateAdd(year, @maxAge, c.[birth_date]) > getDate()
			  AND dbo.cust_hdc_checkin_funct_number_of_occurrences(c.person_id, @attendanceMonths) >= @minAttendance

		SET @NumOfRecords = @@ROWCOUNT
		SET @LCV = 1

		WHILE @LCV <= @NumOfRecords
		BEGIN
			SET @FamilyID = 0
			SET @ChildID = 0
			SET @FamilyNumber = 0

			SELECT @FamilyID = family_id, @ChildID = person_id
			  FROM @cust_temp_family_number_assign
			 WHERE id = @lcv

			--
			-- Check if a family number already exists, if not create one.
			--
			SET @FamilyNumber = dbo.cust_hdc_checkin_funct_family_numberByFamily(@FamilyId, @attribute_id)
			IF @FamilyNumber = 0
			BEGIN
				EXEC cust_hdc_checkin_sp_family_number_insert @FamilyNumber OUTPUT
			END

			--
			-- If no family number was created, throw an error, otherwise assign
			-- the family number to every person in the family.
			--
			IF @FamilyNumber = 0
			BEGIN
				SET @Error = -35
			END
			ELSE
			BEGIN
				EXEC cust_hdc_checkin_sp_assign_family_number @FamilyId, @FamilyNumber, @attribute_id
			END

			SET @LCV = @LCV + 1
		END

		RETURN @Error
	END
END
GO
