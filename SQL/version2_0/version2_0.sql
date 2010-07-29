SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE xtype='U' AND name='cust_hdc_checkin_family_number')
BEGIN
	CREATE TABLE [dbo].[cust_hdc_checkin_family_number]
	(
		[family_number] [int] IDENTITY(2000,1) NOT NULL,
		[date_created] [datetime] NOT NULL,
		CONSTRAINT [PK_cust_hdc_checkin_family_number] PRIMARY KEY CLUSTERED
		(
			[family_number] ASC
		) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[cust_hdc_checkin_family_number] ADD CONSTRAINT [DF_cust_hdc_checkin_family_number_date_created] DEFAULT (getdate()) FOR [date_created]
END
GO
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE xtype='U' AND name='cust_hdc_checkin_occurrence_type_template_location')
BEGIN
	CREATE TABLE [dbo].[cust_hdc_checkin_occurrence_type_template_location]
	(
		[occurrence_type_template_id] [int] NOT NULL,
		[location_id] [int] NOT NULL,
		CONSTRAINT [PK_cust_hdc_checkin_occurrence_type_template_location] PRIMARY KEY CLUSTERED
		(
			[occurrence_type_template_id] ASC,
			[location_id] ASC
		) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Links an occurrence type template to a list of locations that should be created.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'cust_hdc_checkin_occurrence_type_template_location'

	ALTER TABLE [dbo].[cust_hdc_checkin_occurrence_type_template_location] WITH CHECK ADD CONSTRAINT [FK_cust_hdc_checkin_occurrence_type_template_location_core_occurrence_type_template] FOREIGN KEY([occurrence_type_template_id])
		REFERENCES [dbo].[core_occurrence_type_template] ([occurrence_type_template_id])
		ON DELETE CASCADE

	ALTER TABLE [dbo].[cust_hdc_checkin_occurrence_type_template_location] CHECK CONSTRAINT [FK_cust_hdc_checkin_occurrence_type_template_location_core_occurrence_type_template]

	ALTER TABLE [dbo].[cust_hdc_checkin_occurrence_type_template_location] WITH CHECK ADD CONSTRAINT [FK_cust_hdc_checkin_occurrence_type_template_location_orgn_location] FOREIGN KEY([location_id])
		REFERENCES [dbo].[orgn_location] ([location_id])
		ON DELETE CASCADE

	ALTER TABLE [dbo].[cust_hdc_checkin_occurrence_type_template_location] CHECK CONSTRAINT [FK_cust_hdc_checkin_occurrence_type_template_location_orgn_location]
END
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT 1 FROM sysobjects WHERE xtype='IF' AND name='cust_hdc_checkin_func_active_occurrencesByLocation')
	DROP FUNCTION [dbo].[cust_hdc_checkin_func_active_occurrencesByLocation]
GO

CREATE FUNCTION [dbo].[cust_hdc_checkin_func_active_occurrencesByLocation]
	(@location_id int)
	RETURNS TABLE
AS
	RETURN SELECT [co].[occurrence_id]
			FROM core_occurrence AS [co]
			WHERE [co].[location_id] = @location_id
				AND [co].[occurrence_closed] = 0
				AND (([co].[occurrence_start_time] <= GETDATE() AND [co].[occurrence_end_time] >= GETDATE())
					OR ([co].[check_in_start] <= GETDATE() AND [co].[check_in_end] >= GETDATE()))
GO
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/*
FUNCTION: [cust_hdc_checkin_funct_family_numberByFamily]
AUTHOR:  Jeremy Stone
DATE CREATED:  8/22/2009
DESCRIPTION:  This function is used to get the 
			  Person's Family Number by Family ID
			  if it exists
			  If the Family Number does not exist
			  RETURN 0

*/
IF EXISTS (SELECT 1 FROM sysobjects WHERE xtype='FN' AND name='cust_hdc_checkin_funct_family_numberByFamily')
	DROP FUNCTION [dbo].[cust_hdc_checkin_funct_family_numberByFamily]
GO

CREATE FUNCTION [dbo].[cust_hdc_checkin_funct_family_numberByFamily]
(
	@family_id int
	, @attribute_id int
)
RETURNS INT
AS
BEGIN
	DECLARE @FamilyNumber int
	SET @FamilyNumber = 0

	SELECT TOP 1 @FamilyNumber = ISNULL(int_value, 0) 
		FROM core_person_attribute 
		WHERE attribute_id = @attribute_id
		  AND person_id IN (SELECT person_id FROM [core_family_member] WHERE family_id = @family_id)

	RETURN @FamilyNumber
END
GO


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
IF EXISTS (SELECT 1 FROM sysobjects WHERE xtype='FN' AND name='cust_hdc_checkin_funct_family_numberByPerson')
	DROP FUNCTION [dbo].[cust_hdc_checkin_funct_family_numberByPerson]
GO

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
GO
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:	Jeremy Stone
-- Create date: 8/22/2009
-- Description:	Returns number of Occurrences 
--              in the past 12 months
-- =============================================
IF EXISTS (SELECT 1 FROM sysobjects WHERE xtype='FN' AND name='cust_hdc_checkin_funct_number_of_occurrences')
	DROP FUNCTION [dbo].[cust_hdc_checkin_funct_number_of_occurrences]
GO

CREATE FUNCTION [dbo].[cust_hdc_checkin_funct_number_of_occurrences]
(
	@PersonID int,
	@attendanceMonths int = 12
)
RETURNS int
AS
BEGIN
	DECLARE @NumOfOccurrences int

	SET @NumOfOccurrences = 0	

	SELECT @NumOfOccurrences = COUNT(coa.occurrence_attendance_id) 
	  FROM [core_occurrence_attendance] coa 
	INNER JOIN [core_occurrence] co ON coa.occurrence_id = co.occurrence_id
	 WHERE co.occurrence_start_time > DateAdd(month, -@attendanceMonths, getDate()) 
	   AND coa.person_id = @PersonID

	RETURN @NumOfOccurrences
END

GO


SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT 1 FROM sysobjects WHERE xtype='P' AND name='cust_hdc_checkin_sp_delete_locationsByTemplateID')
	DROP PROCEDURE [dbo].[cust_hdc_checkin_sp_delete_locationsByTemplateID]
GO

CREATE PROC [dbo].[cust_hdc_checkin_sp_delete_locationsByTemplateID]
    @TemplateID int
AS
BEGIN
	DELETE FROM cust_hdc_checkin_occurrence_type_template_location
		WHERE occurrence_type_template_id = @TemplateID
END
GO
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
IF EXISTS (SELECT 1 FROM sysobjects WHERE xtype='P' AND name='cust_hdc_checkin_sp_family_number_insert')
	DROP PROCEDURE [dbo].[cust_hdc_checkin_sp_family_number_insert]
GO

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


SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT 1 FROM sysobjects WHERE xtype='P' AND name='cust_hdc_checkin_sp_generate_active_occurrences')
	DROP PROCEDURE [dbo].[cust_hdc_checkin_sp_generate_active_occurrences]
GO

CREATE procedure [dbo].[cust_hdc_checkin_sp_generate_active_occurrences]
(
	@basedate datetime
	, @createMinutesBefore int = 30
	, @userId varchar(50) = 'OccurrenceGenerator'
)
AS
BEGIN
	-- Meaning of core_occurrence_type_template.occurrence_freq_type and freq_qualifier
	------------------------------------------------------------------------------------
	-- Undefined = -1
	-- Daily = 0		freq_qualifier: ""
	-- Weekly = 1; 		freq_qualifier: 0=Sun, 1=M, 2=Tu, 3=Wed, ... , 6=Sat (string)
	-- Monthly = 2		freq_qualifier: 1=first day of month; 31=last day of month (string)
	-- OneTime = 3		freq_qualifier: date (string)

	declare @basetime datetime
	declare @basedatenotime datetime

	declare @occurrenceIds table (occurrence_id int primary key not null)

	set @basetime = convert(char(8), @basedate, 8)
	set @basedatenotime = convert(varchar, @basedate, 112)	-- strip out time

	-- Start with all possible values, then exclude ones that do not match the current date/time
	select *
	into #possible
	from core_occurrence_type_template ott

	-- These are the only frequency_type_ids that this procedure handles.  This needs to be updated if any more types are added
	delete from #possible
	where occurrence_freq_type not in(0, 1, 2, 3)

	-- Delete any items that do not fall on today
	delete from #possible
	where occurrence_type_template_id in
	(
		-- Weekly but does not match today
		select occurrence_type_template_id
		from core_occurrence_type_template ott
		where ott.occurrence_freq_type = 1
		and convert(int, ott.freq_qualifier) <> (datepart(dw, @basedate) - 1)	-- 0=sunday, ..., 6=sat

		union

		-- Monthly but does not match today
		select occurrence_type_template_id
		from core_occurrence_type_template ott
		where ott.occurrence_freq_type = 2
		and convert(int, ott.freq_qualifier) <> day(@basedate)

		union

		-- One time but does not match today
		select occurrence_type_template_id
		from core_occurrence_type_template ott
		where ott.occurrence_freq_type = 3
		and datediff(day, @basedate, convert(datetime, ott.freq_qualifier)) <> 0
	)

	-- Get occurrences that will be active in the next @createMinutesBefore minutes but do not already have an occurrence active
	-- for the given location.
	select ott.*, l.location_id
	into #create
	from #possible ott
		inner join core_occurrence_type ot on ot.occurrence_type_id = ott.occurrence_type_id
		inner join orgn_location_occurrence_type lot on lot.occurrence_type_id = ot.occurrence_type_id
		inner join orgn_location l on l.location_id = lot.location_id
	where dateadd(mi, @createMinutesBefore, @basetime) between ott.check_in_start and ott.check_in_end
		and not exists
		(
			-- occurrences generated today from the given template.  There should only
			-- be one occurrence generated per day per location by this template.
			select * 
			from core_occurrence o 
			where 
			o.date_created >= @basedatenotime
			and o.occurrence_type_template_id = ott.occurrence_type_template_id
			and o.location_id = l.location_id
		)
		and
		(
			not exists
			(
				-- See if there are any defined locations for this template
				select *
				from cust_hdc_checkin_occurrence_type_template_location tl
				where
					tl.occurrence_type_template_id = ott.occurrence_type_template_id
			)
			or l.location_id IN
			(
				select tl.location_id
				from cust_hdc_checkin_occurrence_type_template_location tl
				where
					tl.occurrence_type_template_id = ott.occurrence_type_template_id
			)
		)

	--------------------------
	-- CREATE OCCURRENCES
	--------------------------

	begin tran

	-- Create one occurrence for each location that is associated with this occurrence_type_id.
	insert into core_occurrence
	(
		date_created
		, date_modified
		, created_by
		, modified_by
		, occurrence_name
		, occurrence_description
		, occurrence_start_time
		, occurrence_end_time
		, check_in_start
		, check_in_end
		, location
		, location_id
		, occurrence_type
		, membership_required
		, occurrence_type_template_id
		, occurrence_closed
	)
	select 
		date_created = getdate()
		, date_modified = getdate()
		, created_by = @userId
		, modified_by = @userId
		, occurrence_name = ott.schedule_name
		, occurrence_description = ot.type_name
		, occurrence_start_time = convert(varchar, @baseDateNoTime, 112) + ' ' + convert(char(8), ott.start_time, 8)	-- append date to time
		, occurrence_end_time = convert(varchar, @baseDateNoTime, 112) + ' ' + convert(char(8), ott.end_time, 8)
		, check_in_start = convert(varchar, @baseDateNoTime, 112) + ' ' + convert(char(8), ott.check_in_start, 8)
		, check_in_end = convert(varchar, @baseDateNoTime, 112) + ' ' + convert(char(8), ott.check_in_end, 8)
		, location = l.location_name
		, location_id = l.location_id
		, occurrence_type = ot.occurrence_type_id
		, membership_required = ot.membership_required
		, occurrence_type_template_id = ott.occurrence_type_template_id
		, occurrence_closed = ot.use_room_ratios	-- Classes using room ratios should initially be closed
	from #create ott
		inner join core_occurrence_type ot on ot.occurrence_type_id = ott.occurrence_type_id
		inner join orgn_location l on ott.location_id = l.location_id

	-- get ids of occurrences that were just inserted
	insert into @occurrenceIds
	select top(@@ROWCOUNT) occurrence_id 
	from core_occurrence order by occurrence_id desc

	-- The first class should automatically be opened when using room ratios but not
	-- requiring the leader to check-in first.
	update o
		set o.occurrence_closed = 0
	from core_occurrence o
		inner join @occurrenceIds tmp on tmp.occurrence_id = o.occurrence_id
		inner join core_occurrence_type ot on ot.occurrence_type_id = o.occurrence_type
		inner join orgn_location_occurrence_type lot on lot.occurrence_type_id = ot.occurrence_type_id
			and lot.location_id = o.location_id
		inner join
		(
			-- Get first room for each period
			select o2.occurrence_type_template_id, location_order = min(lot2.location_order) from core_occurrence o2
				inner join @occurrenceIds tmp2 on tmp2.occurrence_id = o2.occurrence_id
				inner join core_occurrence_type ot2 on ot2.occurrence_type_id = o2.occurrence_type
				inner join orgn_location_occurrence_type lot2 on lot2.occurrence_type_id = ot2.occurrence_type_id
					and lot2.location_id = o2.location_id
			where ot2.use_room_ratios = 1
				and ot2.min_leaders = 0
				and o2.occurrence_closed = 1
			group by o2.occurrence_type_template_id
		) t on t.occurrence_type_template_id = o.occurrence_type_template_id
			and t.location_order = lot.location_order
	where ot.use_room_ratios = 1
		and ot.min_leaders = 0
		and o.occurrence_closed = 1

	-- create link to profile for occurrences created today but do not have a link yet
	insert into core_profile_occurrence
	(
		profile_id
		, occurrence_id
	)
	select
		p.profile_id
		, o.occurrence_id	
	from core_occurrence o
		inner join core_occurrence_type ot on o.occurrence_type = ot.occurrence_type_id
		inner join core_profile p on ot.sync_with_profile = p.profile_id
	where
		o.date_created >= @basedatenotime
		and not exists(select * from core_profile_occurrence where occurrence_id = o.occurrence_id)

	-- create link to occurrence for occurrences created today but do not have a link yet
	insert into smgp_group_occurrence
	(
		group_id
		, occurrence_id
	)
	select
		g.group_id
		, o.occurrence_id 
	from core_occurrence o 
		inner join core_occurrence_type ot on o.occurrence_type = ot.occurrence_type_id
		inner join smgp_group g on ot.sync_with_group = g.group_id
	where 
		o.date_created >= @basedatenotime
		and not exists(select * from smgp_group_occurrence where occurrence_id = o.occurrence_id)

	-- created occurrences
	select * from core_occurrence o
		inner join core_occurrence_type ot on ot.occurrence_type_id = o.occurrence_type
		inner join @occurrenceIds tmp on tmp.occurrence_id = o.occurrence_id

	commit tran
	--rollback tran

	select * from #create

	drop table #possible
	drop table #create
END
GO
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT 1 FROM sysobjects WHERE xtype='P' AND name='cust_hdc_checkin_sp_get_security_code')
	DROP PROCEDURE [dbo].[cust_hdc_checkin_sp_get_security_code]
GO

CREATE PROCEDURE [dbo].[cust_hdc_checkin_sp_get_security_code]
	@PersonID INT,
	@FullCode CHAR(8) OUTPUT
AS
BEGIN
	DECLARE @AttributeName varchar(40)
	DECLARE @AttributeGroup uniqueidentifier
	DECLARE @FamilyNumber int

	SET @AttributeName = 'Family Number'
	SET @AttributeGroup = 'FE30AC51-5B67-44A5-9D73-A7D3C63A7E9E'
	SET @FamilyNumber = 0

	SELECT @FamilyNumber = cpa.int_value
		FROM core_person_attribute AS cpa
		JOIN core_attribute AS ca ON ca.attribute_id = cpa.attribute_id
		WHERE cpa.person_id = @PersonID
		  AND ca.attribute_group_id = (SELECT attribute_group_id FROM core_attribute_group WHERE [guid] = @AttributeGroup)
		  AND ca.attribute_name = @AttributeName

	IF @FamilyNumber = 0
		BEGIN
			EXEC cust_cccev_ckin_sp_get_security_code @FullCode OUTPUT
		END
	ELSE
		BEGIN
			DECLARE @Seed INT
			SET @Seed = DATEPART(ms, GETDATE())

			SELECT @FullCode = CHAR(65 + CAST((RAND() * @Seed) AS INT) % 26) + CHAR(65 + CAST((RAND() * @Seed) AS INT) % 26) + RIGHT('0000' + CONVERT(VARCHAR(4), @FamilyNumber), 4)
		END

	PRINT 'Code: ' + @FullCode
END
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT 1 FROM sysobjects WHERE xtype='P' AND name='cust_hdc_checkin_sp_get_service_overviewByLocation')
	DROP PROCEDURE [dbo].[cust_hdc_checkin_sp_get_service_overviewByLocation]
GO

CREATE PROCEDURE [dbo].[cust_hdc_checkin_sp_get_service_overviewByLocation]
	@group_id int,
	@service_time datetime
AS
BEGIN
	SELECT   [ol].[location_id]
		,[ol].[building_name],[ol].[location_name]
		,[ol].[building_name] + ' - ' + [ol].[location_name] AS 'building_location_name'
		,(SELECT COUNT([coa].[person_id])
			FROM core_occurrence_attendance AS [coa]
			LEFT JOIN core_occurrence AS [co] ON [co].[occurrence_id] = [coa].[occurrence_id]
			LEFT JOIN core_occurrence_type AS [cot] ON [cot].[occurrence_type_id] = [co].[occurrence_type]
			WHERE [cot].[group_id] = @group_id
				AND [co].[occurrence_start_time] = @service_time
				AND [co].[location_id] = [ol].[location_id]) AS 'attendance_count'
		,(SELECT COUNT([co].[location_id])
			FROM core_occurrence AS [co]
			LEFT JOIN core_occurrence_type AS [cot] ON [cot].[occurrence_type_id] = [co].[occurrence_type]
			WHERE ([cot].[group_id] = @group_id AND [co].[occurrence_start_time] = @service_time)) AS 'occurrence_count'
		,(SELECT COUNT(*) FROM cust_hdc_checkin_func_active_occurrencesByLocation([ol].[location_id])) AS 'active_occurrence_count'
	FROM orgn_location AS [ol]
	WHERE (SELECT COUNT([co].[location_id])
		FROM core_occurrence AS [co]
		LEFT JOIN core_occurrence_type AS [cot] ON [cot].[occurrence_type_id] = [co].[occurrence_type]
		WHERE [cot].[group_id] = @group_id
			AND [co].[occurrence_start_time] = @service_time
			AND [co].[location_id] = [ol].[location_id]) > 0
END
GO
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT 1 FROM sysobjects WHERE xtype='P' AND name='cust_hdc_checkin_sp_get_service_overviewByOccurrence')
	DROP PROCEDURE [dbo].[cust_hdc_checkin_sp_get_service_overviewByOccurrence]
GO

CREATE Proc [dbo].[cust_hdc_checkin_sp_get_service_overviewByOccurrence]
	@group_id int,
	@service_time datetime
AS
BEGIN
	SELECT  [co].[occurrence_id],[co].[occurrence_name],[co].[occurrence_start_time]
		,[co].[occurrence_end_time],[co].[check_in_start],[co].[check_in_end]
		,[co].[occurrence_closed]
		,[co].[location_id]
		,[ol].[building_name],[ol].[location_name]
		,[ol].[building_name] + ' - ' + [ol].[location_name] AS 'building_location_name'
		,[cotg].[group_id],[cot].[occurrence_type_id]
		,[cotg].[group_name],[cot].[type_name]
		,[cotg].[group_name] + ' - ' + [cot].[type_name] AS 'occurrence_group_type_name'
		,(SELECT COUNT([coa].[person_id]) FROM core_occurrence_attendance AS [coa] WHERE [coa].[occurrence_id] = [co].[occurrence_id]) AS 'attendance_count'
	FROM core_occurrence AS [co]
		LEFT JOIN core_occurrence_type AS [cot] ON [cot].[occurrence_type_id] = [co].[occurrence_type]
		LEFT JOIN core_occurrence_type_group AS [cotg] ON [cotg].[group_id] = [cot].[group_id]
		LEFT JOIN orgn_location AS [ol] ON [ol].[location_id] = [co].[location_id]
	WHERE [cot].[group_id] = @group_id
		AND [co].[occurrence_start_time] = @service_time
END
GO
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT 1 FROM sysobjects WHERE xtype='P' AND name='cust_hdc_checkin_sp_save_template_location')
	DROP PROCEDURE [dbo].[cust_hdc_checkin_sp_save_template_location]
GO

CREATE PROCEDURE [dbo].[cust_hdc_checkin_sp_save_template_location]
	@OccurrenceTypeTemplateId int,
	@LocationId int
AS
BEGIN
	DECLARE @UpdateDateTime DateTime SET @UpdateDateTime = GETDATE()
		
	IF NOT EXISTS (
		SELECT * FROM cust_hdc_checkin_occurrence_type_template_location
		WHERE [occurrence_type_template_id] = @OccurrenceTypeTemplateId
		  AND [location_id] = @LocationId
		)
	BEGIN
		INSERT INTO cust_hdc_checkin_occurrence_type_template_location
		(	
			 [occurrence_type_template_id]
			,[location_id]
		)
		values
		(	
			 @OccurrenceTypeTemplateId
			,@LocationId
		)
	END
END
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT 1 FROM sysobjects WHERE xtype='P' AND name='cust_hdc_checkin_sp_templateLocationsByGroupID')
	DROP PROCEDURE [dbo].[cust_hdc_checkin_sp_templateLocationsByGroupID]
GO

CREATE PROCEDURE [dbo].[cust_hdc_checkin_sp_templateLocationsByGroupID]
	@GroupID int
AS
BEGIN
	SELECT [cott].[schedule_name],[cott].[start_time],[cott].[occurrence_freq_type],[cott].[freq_qualifier]
			,[ol].[building_name],[ol].[location_name]
		FROM [dbo].[cust_hdc_checkin_occurrence_type_template_location] AS [ottl]
		INNER JOIN [dbo].[core_occurrence_type_template] AS [cott] ON [ottl].[occurrence_type_template_id] = [cott].[occurrence_type_template_id]
		INNER JOIN [dbo].[core_occurrence_type] AS [cot] ON [cott].[occurrence_type_id] = [cot].[occurrence_type_id]
		INNER JOIN [dbo].[orgn_location] AS [ol] ON [ottl].[location_id] = [ol].[location_id]
		WHERE [cot].[group_id] = @GroupID
END
GO
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT 1 FROM sysobjects WHERE xtype='P' AND name='cust_hdc_checkin_sp_templateLocationsByTypeID')
	DROP PROCEDURE [dbo].[cust_hdc_checkin_sp_templateLocationsByTypeID]
GO

CREATE PROCEDURE [cust_hdc_checkin_sp_templateLocationsByTypeID]
	@TypeID int
AS
BEGIN
	SELECT [cott].[occurrence_type_template_id],[cott].[schedule_name],[cott].[start_time],[cott].[check_in_start],[cott].[occurrence_freq_type],[cott].[freq_qualifier]
			,[ol].[location_id],[ol].[building_name],[ol].[location_name]
		FROM [dbo].[core_occurrence_type_template] AS [cott]
		INNER JOIN [dbo].[core_occurrence_type] AS [cot] ON [cott].[occurrence_type_id] = [cot].[occurrence_type_id]
		INNER JOIN [dbo].[orgn_location_occurrence_type] AS [olot] ON [olot].[occurrence_type_id] = [cot].[occurrence_type_id]
		INNER JOIN [dbo].[orgn_location] AS [ol] ON [olot].[location_id] = [ol].[location_id]
		WHERE [cot].[occurrence_type_id] = @TypeID
			AND (
				EXISTS (SELECT [ottl].[location_id] FROM [dbo].[cust_hdc_checkin_occurrence_type_template_location] AS [ottl] WHERE [ottl].[occurrence_type_template_id] = [cott].[occurrence_type_template_id] AND [ottl].[location_id] = [ol].[location_id])
				OR NOT EXISTS (SELECT [ottl].[location_id] FROM [dbo].[cust_hdc_checkin_occurrence_type_template_location] AS [ottl] WHERE [ottl].[occurrence_type_template_id] = [cott].[occurrence_type_template_id]))
END
GO
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
IF EXISTS (SELECT 1 FROM sysobjects WHERE xtype='P' AND name='cust_hdc_checkin_sp_assign_family_number')
	DROP PROCEDURE [dbo].[cust_hdc_checkin_sp_assign_family_number]
GO

CREATE PROCEDURE [dbo].[cust_hdc_checkin_sp_assign_family_number]
(
	@family_id int
	, @family_number int
	, @attribute_id int
)
AS
BEGIN
	INSERT INTO [dbo].[core_person_attribute]
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
	FROM [dbo].[core_family_member] a
	WHERE a.family_id = @family_id
	  AND a.person_id NOT IN (SELECT person_id FROM [dbo].[core_person_attribute] WHERE attribute_id = @attribute_id)

	UPDATE [dbo].[core_person_attribute]
		SET int_value = @family_number
		    ,date_modified = getdate()
		WHERE attribute_id = @attribute_id
		  AND person_id in (Select person_id FROM [dbo].[core_family_member] WHERE family_id = @family_id)
END
GO


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
IF EXISTS (SELECT 1 FROM sysobjects WHERE xtype='P' AND name='cust_hdc_checkin_sp_assign_family_numbers')
	DROP PROCEDURE [dbo].[cust_hdc_checkin_sp_assign_family_numbers]
GO

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
GO
