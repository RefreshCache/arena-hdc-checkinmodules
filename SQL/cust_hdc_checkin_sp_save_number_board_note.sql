/****** Object:  StoredProcedure [dbo].[cust_hdc_checkin_sp_save_number_board_note]    Script Date: 12/04/2009 15:11:31 ******/
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
CREATE PROCEDURE [dbo].[cust_hdc_checkin_sp_save_number_board_note]
(@NoteId Int
, @NoteHtml varchar(Max)
, @NoteImage uniqueidentifier
, @NoteInfo varchar(Max)
, @SystemId Int
, @DisplayGroupId Int
, @UserInfo Int
, @UserId varchar(50)
, @ID int OUTPUT)
AS
BEGIN
	DECLARE @UpdateDateTime DateTime SET @UpdateDateTime = GETDATE()

	IF NOT EXISTS(
		SELECT * FROM cust_hdc_checkin_number_board_note
		WHERE [note_id] = @NoteId
		)
		
	BEGIN
		DECLARE @NewKey uniqueidentifier
		SET @NewKey = NEWID()

		INSERT INTO cust_hdc_checkin_number_board_note
		(
			 [date_created]
			,[date_modified]
			,[created_by]
			,[modified_by]
			,[note_html]
			,[note_image]
			,[note_info]
			,[system_id]
			,[display_group_id]
			,[user_info]
		)
		values
		(
			 @UpdateDateTime
			,@UpdateDateTime
			,@UserId
			,@UserId
			,@NoteHtml
			,@NoteImage
			,@NoteInfo
			,@SystemId
			,@DisplayGroupId
			,@UserInfo
		)

		SET @ID = @@IDENTITY

	END
	ELSE
	BEGIN

		UPDATE cust_hdc_checkin_number_board_note Set
			 [date_modified] = @UpdateDateTime 
			,[modified_by] = @UserID
			,[note_html] = @NoteHtml
			,[note_image] = @NoteImage
			,[note_info] = @NoteInfo
			,[system_id] = @SystemId
			,[display_group_id] = @DisplayGroupId
			,[user_info] = @UserInfo
		WHERE [note_id] = @NoteId

		SET @ID = @NoteId

	END
END
