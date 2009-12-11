/****** Object:  StoredProcedure [dbo].[cust_hdc_checkin_sp_get_dispayGroupsForSystemID]    Script Date: 12/04/2009 15:11:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE Proc [dbo].[cust_hdc_checkin_sp_get_dispayGroupsForSystemID]
	@SystemId int
AS

	SELECT [dgs].display_group_id
		FROM cust_hdc_checkin_display_group_system AS dgs
		WHERE [dgs].system_id = @SystemId
