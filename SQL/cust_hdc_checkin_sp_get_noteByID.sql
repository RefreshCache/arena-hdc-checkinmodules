/****** Object:  StoredProcedure [dbo].[cust_hdc_checkin_sp_get_noteByID]    Script Date: 12/04/2009 15:11:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE Proc [dbo].[cust_hdc_checkin_sp_get_noteByID]
	@NoteId int
AS

	SELECT [nbn].*
		FROM cust_hdc_checkin_number_board_note AS nbn
		WHERE [nbn].note_id = @NoteId
