/****** Object:  StoredProcedure [dbo].[cust_hdc_checkin_sp_get_notesForUserInfo]    Script Date: 12/04/2009 15:11:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE Proc [dbo].[cust_hdc_checkin_sp_get_notesForUserInfo]
	@UserInfo int
AS

	SELECT [nbn].*
		FROM cust_hdc_checkin_number_board_note AS nbn
		WHERE [nbn].user_info = @UserInfo
		ORDER BY note_id
