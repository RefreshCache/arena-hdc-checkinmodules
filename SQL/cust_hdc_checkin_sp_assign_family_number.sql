/****** Object:  StoredProcedure [dbo].[cust_hdc_checkin_sp_assign_family_number]    Script Date: 12/03/2009 15:30:50 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/* 
###############################
 AUTHOR:  Jeremy Stone
 CREATE DATE: 8/22/2009
 DESCRIPTION:  INSERTS Family Number INTO 
				ALL childeren of the Parent Given
###############################
*/
CREATE PROCEDURE [dbo].[cust_hdc_checkin_sp_assign_family_number]
(
	@family_id int
	, @family_number int
	, @attribute_id int
)
AS
BEGIN

	INSERT INTO [ArenaDB].[dbo].[core_person_attribute]
           ([person_id]
           ,[attribute_id]
           ,[int_value]
           ,[varchar_value]
           ,[datetime_value]
           ,[decimal_value]
           ,[date_created]
           ,[date_modified]
           ,[created_by]
           ,[modified_by])
    SELECT 
           person_id
		   ,@attribute_id
           ,@family_number
		   ,NULL
           ,NULL
           ,NULL
           ,getdate()
           ,getdate()
           ,'AutomatedProcess'
           ,'AutomatedProcess'
	FROM [ArenaDB].[dbo].[core_family_member] a
	WHERE a.family_id = @family_id
	  AND a.person_id NOT IN (SELECT person_id FROM [ArenaDB].[dbo].[core_person_attribute] WHERE attribute_id = @attribute_id)

	UPDATE [ArenaDB].[dbo].[core_person_attribute]
		SET int_value = @family_number
			,date_modified = getdate()
	WHERE attribute_id = @attribute_id
	  AND person_id in (Select person_id FROM [ArenaDB].[dbo].[core_family_member] WHERE family_id = @family_id)
END


GO


