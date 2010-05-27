SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE xtype='P' AND name='cust_hdc_checkin_sp_get_security_code')
BEGIN
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
END
