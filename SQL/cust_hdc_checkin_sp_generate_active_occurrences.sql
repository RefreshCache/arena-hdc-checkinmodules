SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE xtype='P' AND name='cust_hdc_checkin_sp_generate_active_occurrences')
BEGIN
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
END
GO
