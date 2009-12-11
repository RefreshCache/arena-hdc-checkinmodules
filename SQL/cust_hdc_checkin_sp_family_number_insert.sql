/****** Object:  StoredProcedure [dbo].[cust_hdc_checkin_sp_family_number_insert]    Script Date: 12/03/2009 15:31:49 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Jeremy Stone
-- Create date: 8/20/2009
-- Description: This procedure will be used to 
--              create a new family_number
-- =============================================
CREATE PROCEDURE [dbo].[cust_hdc_checkin_sp_family_number_insert]
(@family_number int output)
AS
BEGIN

	INSERT INTO [cust_hdc_checkin_family_number]
           ([date_created])
     VALUES
           (GETDATE())
	
	SET @family_number = @@IDENTITY

END

GO


