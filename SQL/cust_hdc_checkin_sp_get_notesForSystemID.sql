/****** Object:  StoredProcedure [dbo].[cust_hdc_checkin_sp_get_notesForSystemID]    Script Date: 12/04/2009 15:11:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE Proc [dbo].[cust_hdc_checkin_sp_get_notesForSystemID]
	@SystemId int
AS

	SELECT [nbn].*
		FROM cust_hdc_checkin_number_board_note AS nbn
		WHERE [nbn].system_id = @SystemId
		   OR [nbn].display_group_id IN (SELECT display_group_id FROM cust_hdc_checkin_display_group_system WHERE system_id = @SystemId)
		ORDER BY note_id
