/****** Object:  StoredProcedure [dbo].[cust_hdc_checkin_sp_del_number_board_note]    Script Date: 12/04/2009 15:11:06 ******/
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
CREATE PROCEDURE [dbo].[cust_hdc_checkin_sp_del_number_board_note]
(@NoteId Int)
AS
BEGIN

	DELETE
  FROM [ArenaDB].[dbo].[cust_hdc_checkin_number_board_note]
	Where [note_id] = @NoteId
	

END
