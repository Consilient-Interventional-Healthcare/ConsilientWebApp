-- =============================================
-- Author:      ConsilientWebApp Team
-- Create date: 2026-01-19
-- Description: Processes provider assignments from staging table and inserts clinical data.
--              Creates Patients, PatientFacilities, Providers, Hospitalizations, Visits,
--              and VisitAttendants as needed within a transaction.
-- =============================================
CREATE OR ALTER PROCEDURE [staging].[usp_ProcessProviderAssignments]
    @BatchId UNIQUEIDENTIFIER,
    @ProcessedCount INT OUTPUT,
    @ErrorCount INT OUTPUT,
    @ErrorMessage NVARCHAR(MAX) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- Track whether we own the transaction (for caller-managed transactions)
    DECLARE @OwnTransaction BIT = 0;

    -- Initialize output parameters
    SET @ProcessedCount = 0;
    SET @ErrorCount = 0;
    SET @ErrorMessage = NULL;

    -- ===========================================
    -- SECTION 1: Create Temp Tables
    -- ===========================================

    -- Main work table to track records being processed
    CREATE TABLE #StagingRecords (
        RowNum INT IDENTITY(1,1) PRIMARY KEY,
        StagingId INT NOT NULL,
        -- Patient data
        NormalizedPatientFirstName NVARCHAR(100),
        NormalizedPatientLastName NVARCHAR(100),
        Mrn NVARCHAR(50),
        Dob DATE,
        -- Provider data
        NormalizedPhysicianLastName NVARCHAR(100),
        NormalizedNursePractitionerLastName NVARCHAR(100),
        -- Hospitalization data
        HospitalNumber NVARCHAR(50),
        Admit SMALLDATETIME,
        PsychEval NVARCHAR(255),
        -- Visit data
        ServiceDate DATE,
        Room NVARCHAR(20),
        Bed NVARCHAR(5),
        -- Resolution data (pre-populated by C# resolver)
        FacilityId INT,
        ResolvedPatientId INT,
        ResolvedPhysicianId INT,
        ResolvedNursePractitionerId INT,
        ResolvedHospitalizationId INT,
        -- Computed values
        ParsedCaseId INT,
        IsPsychEval BIT,
        -- Flags for what needs to be created
        NeedsNewPatient BIT DEFAULT 0,
        NeedsNewPhysician BIT DEFAULT 0,
        NeedsNewNP BIT DEFAULT 0,
        NeedsNewHospitalization BIT DEFAULT 0,
        -- Generated IDs (populated during processing)
        NewPatientId INT,
        NewPhysicianId INT,
        NewNPId INT,
        NewHospitalizationId INT,
        NewVisitId INT,
        -- Tracking flags
        PatientWasCreated BIT DEFAULT 0,
        PatientFacilityWasCreated BIT DEFAULT 0,
        PhysicianWasCreated BIT DEFAULT 0,
        NursePractitionerWasCreated BIT DEFAULT 0,
        HospitalizationWasCreated BIT DEFAULT 0,
        -- Validation
        ExclusionReason NVARCHAR(500),
        IsValid BIT DEFAULT 1
    );

    BEGIN TRY
        -- Only start a transaction if not already in one (allows caller-managed transactions)
        IF @@TRANCOUNT = 0
        BEGIN
            BEGIN TRANSACTION;
            SET @OwnTransaction = 1;
        END

        -- ===========================================
        -- SECTION 1.5: Validate Batch Status
        -- ===========================================

        -- Ensure batch exists and is in Resolved status before processing
        IF NOT EXISTS (
            SELECT 1 FROM [staging].[ProviderAssignmentBatches]
            WHERE [Id] = @BatchId AND [Status] = 2  -- Resolved
        )
        BEGIN
            SET @ErrorMessage = 'Batch must be in Resolved status to process. BatchId: ' + CAST(@BatchId AS NVARCHAR(36));
            IF @OwnTransaction = 1
                COMMIT TRANSACTION;
            RETURN -1;
        END

        -- ===========================================
        -- SECTION 2: Load Eligible Staging Records
        -- ===========================================

        INSERT INTO #StagingRecords (
            StagingId,
            NormalizedPatientFirstName, NormalizedPatientLastName, Mrn, Dob,
            NormalizedPhysicianLastName, NormalizedNursePractitionerLastName,
            HospitalNumber, Admit, PsychEval,
            ServiceDate, Room, Bed,
            FacilityId, ResolvedPatientId, ResolvedPhysicianId,
            ResolvedNursePractitionerId, ResolvedHospitalizationId
        )
        SELECT
            Id AS StagingId,
            NormalizedPatientFirstName, NormalizedPatientLastName, Mrn, Dob,
            NormalizedPhysicianLastName, NormalizedNursePractitionerLastName,
            HospitalNumber, Admit, PsychEval,
            ServiceDate, Room, Bed,
            FacilityId, ResolvedPatientId, ResolvedPhysicianId,
            ResolvedNursePractitionerId, ResolvedHospitalizationId
        FROM [staging].[ProviderAssignments]
        WHERE BatchId = @BatchId
          AND ShouldImport = 1
          AND Imported = 0
          AND ValidationErrors IS NULL
          AND ResolvedVisitId IS NULL
          AND FacilityId IS NOT NULL;

        -- Check if we have records to process
        IF NOT EXISTS (SELECT 1 FROM #StagingRecords)
        BEGIN
            SET @ErrorMessage = 'No eligible records found for BatchId: ' + CAST(@BatchId AS NVARCHAR(36));
            IF @OwnTransaction = 1
                COMMIT TRANSACTION;
            RETURN 0;
        END

        -- ===========================================
        -- SECTION 3: Parse and Validate Data
        -- ===========================================

        -- Parse CaseId and determine PsychEval status
        UPDATE #StagingRecords
        SET
            ParsedCaseId = TRY_CAST(HospitalNumber AS INT),
            IsPsychEval = CASE
                WHEN PsychEval IN ('Yes', 'Y', '1', 'PE', 'Psych Eval') THEN 1
                ELSE 0
            END,
            NeedsNewPatient = CASE WHEN ResolvedPatientId IS NULL THEN 1 ELSE 0 END,
            NeedsNewPhysician = CASE WHEN ResolvedPhysicianId IS NULL AND NormalizedPhysicianLastName IS NOT NULL AND LEN(LTRIM(RTRIM(NormalizedPhysicianLastName))) > 0 THEN 1 ELSE 0 END,
            NeedsNewNP = CASE WHEN ResolvedNursePractitionerId IS NULL AND NormalizedNursePractitionerLastName IS NOT NULL AND LEN(LTRIM(RTRIM(NormalizedNursePractitionerLastName))) > 0 THEN 1 ELSE 0 END,
            NeedsNewHospitalization = CASE WHEN ResolvedHospitalizationId IS NULL THEN 1 ELSE 0 END;

        -- Validate required data
        UPDATE #StagingRecords
        SET
            IsValid = 0,
            ExclusionReason = 'Patient name is required but normalized names are null'
        WHERE NeedsNewPatient = 1
          AND (NormalizedPatientLastName IS NULL OR LEN(LTRIM(RTRIM(NormalizedPatientLastName))) = 0);

        UPDATE #StagingRecords
        SET
            IsValid = 0,
            ExclusionReason = 'HospitalNumber cannot be converted to integer for CaseId'
        WHERE NeedsNewHospitalization = 1
          AND ParsedCaseId IS NULL
          AND IsValid = 1;

        -- ===========================================
        -- SECTION 4: Insert Patients
        -- ===========================================

        -- Create temp table for patients to insert (with unique combinations)
        CREATE TABLE #PatientsToInsert (
            TempId INT IDENTITY(1,1) PRIMARY KEY,
            FirstName NVARCHAR(100),
            LastName NVARCHAR(100),
            BirthDate DATE,
            NewPatientId INT
        );

        INSERT INTO #PatientsToInsert (FirstName, LastName, BirthDate)
        SELECT DISTINCT
            ISNULL(NormalizedPatientFirstName, ''),
            NormalizedPatientLastName,
            Dob
        FROM #StagingRecords
        WHERE NeedsNewPatient = 1 AND IsValid = 1;

        -- Insert patients one by one using cursor to capture IDs
        DECLARE @TempId INT, @FirstName NVARCHAR(100), @LastName NVARCHAR(100), @BirthDate DATE, @NewPatientId INT;

        DECLARE patient_cursor CURSOR LOCAL FAST_FORWARD FOR
            SELECT TempId, FirstName, LastName, BirthDate FROM #PatientsToInsert;

        OPEN patient_cursor;
        FETCH NEXT FROM patient_cursor INTO @TempId, @FirstName, @LastName, @BirthDate;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            INSERT INTO [Clinical].[Patients] (FirstName, LastName, BirthDate)
            VALUES (@FirstName, @LastName, @BirthDate);

            SET @NewPatientId = SCOPE_IDENTITY();

            UPDATE #PatientsToInsert SET NewPatientId = @NewPatientId WHERE TempId = @TempId;

            FETCH NEXT FROM patient_cursor INTO @TempId, @FirstName, @LastName, @BirthDate;
        END

        CLOSE patient_cursor;
        DEALLOCATE patient_cursor;

        -- Update staging records with new patient IDs
        UPDATE sr
        SET
            sr.NewPatientId = p.NewPatientId,
            sr.ResolvedPatientId = p.NewPatientId,
            sr.PatientWasCreated = 1
        FROM #StagingRecords sr
        INNER JOIN #PatientsToInsert p
            ON ISNULL(sr.NormalizedPatientFirstName, '') = p.FirstName
            AND sr.NormalizedPatientLastName = p.LastName
            AND ISNULL(CAST(sr.Dob AS NVARCHAR(10)), '') = ISNULL(CAST(p.BirthDate AS NVARCHAR(10)), '')
        WHERE sr.NeedsNewPatient = 1 AND sr.IsValid = 1;

        -- ===========================================
        -- SECTION 5: Insert PatientFacilities
        -- ===========================================

        -- Create temp table for patient facilities
        CREATE TABLE #PatientFacilitiesToInsert (
            TempId INT IDENTITY(1,1) PRIMARY KEY,
            PatientId INT,
            FacilityId INT,
            MRN NVARCHAR(50),
            StagingId INT
        );

        INSERT INTO #PatientFacilitiesToInsert (PatientId, FacilityId, MRN, StagingId)
        SELECT DISTINCT
            sr.NewPatientId,
            sr.FacilityId,
            sr.Mrn,
            sr.StagingId
        FROM #StagingRecords sr
        WHERE sr.PatientWasCreated = 1
          AND sr.NewPatientId IS NOT NULL
          AND NOT EXISTS (
              SELECT 1 FROM [Clinical].[PatientFacilities] pf
              WHERE pf.FacilityId = sr.FacilityId
                AND pf.MRN = sr.Mrn
          );

        -- Insert patient facilities
        INSERT INTO [Clinical].[PatientFacilities] (PatientId, FacilityId, MRN)
        SELECT PatientId, FacilityId, MRN
        FROM #PatientFacilitiesToInsert;

        -- Update PatientFacilityWasCreated flag
        UPDATE sr
        SET sr.PatientFacilityWasCreated = 1
        FROM #StagingRecords sr
        WHERE EXISTS (SELECT 1 FROM #PatientFacilitiesToInsert pf WHERE pf.StagingId = sr.StagingId);

        -- ===========================================
        -- SECTION 6: Insert Physicians
        -- ===========================================

        CREATE TABLE #PhysiciansToInsert (
            TempId INT IDENTITY(1,1) PRIMARY KEY,
            LastName NVARCHAR(100),
            NewProviderId INT
        );

        INSERT INTO #PhysiciansToInsert (LastName)
        SELECT DISTINCT NormalizedPhysicianLastName
        FROM #StagingRecords
        WHERE NeedsNewPhysician = 1 AND IsValid = 1;

        -- Insert physicians using cursor
        DECLARE @PhysicianTempId INT, @PhysicianLastName NVARCHAR(100), @NewProviderId INT;

        DECLARE physician_cursor CURSOR LOCAL FAST_FORWARD FOR
            SELECT TempId, LastName FROM #PhysiciansToInsert;

        OPEN physician_cursor;
        FETCH NEXT FROM physician_cursor INTO @PhysicianTempId, @PhysicianLastName;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            INSERT INTO [Clinical].[Providers] (FirstName, LastName, TitleExtension, [Type], Email, EmployeeId)
            VALUES ('', @PhysicianLastName, NULL, 0, '', NULL);

            SET @NewProviderId = SCOPE_IDENTITY();

            UPDATE #PhysiciansToInsert SET NewProviderId = @NewProviderId WHERE TempId = @PhysicianTempId;

            FETCH NEXT FROM physician_cursor INTO @PhysicianTempId, @PhysicianLastName;
        END

        CLOSE physician_cursor;
        DEALLOCATE physician_cursor;

        -- Update staging records with new physician IDs
        UPDATE sr
        SET
            sr.NewPhysicianId = p.NewProviderId,
            sr.ResolvedPhysicianId = p.NewProviderId,
            sr.PhysicianWasCreated = 1
        FROM #StagingRecords sr
        INNER JOIN #PhysiciansToInsert p ON sr.NormalizedPhysicianLastName = p.LastName
        WHERE sr.NeedsNewPhysician = 1 AND sr.IsValid = 1;

        -- ===========================================
        -- SECTION 7: Insert Nurse Practitioners
        -- ===========================================

        CREATE TABLE #NPsToInsert (
            TempId INT IDENTITY(1,1) PRIMARY KEY,
            LastName NVARCHAR(100),
            NewProviderId INT
        );

        INSERT INTO #NPsToInsert (LastName)
        SELECT DISTINCT NormalizedNursePractitionerLastName
        FROM #StagingRecords
        WHERE NeedsNewNP = 1 AND IsValid = 1;

        -- Insert NPs using cursor
        DECLARE @NPTempId INT, @NPLastName NVARCHAR(100), @NewNPId INT;

        DECLARE np_cursor CURSOR LOCAL FAST_FORWARD FOR
            SELECT TempId, LastName FROM #NPsToInsert;

        OPEN np_cursor;
        FETCH NEXT FROM np_cursor INTO @NPTempId, @NPLastName;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            INSERT INTO [Clinical].[Providers] (FirstName, LastName, TitleExtension, [Type], Email, EmployeeId)
            VALUES ('', @NPLastName, NULL, 1, '', NULL);

            SET @NewNPId = SCOPE_IDENTITY();

            UPDATE #NPsToInsert SET NewProviderId = @NewNPId WHERE TempId = @NPTempId;

            FETCH NEXT FROM np_cursor INTO @NPTempId, @NPLastName;
        END

        CLOSE np_cursor;
        DEALLOCATE np_cursor;

        -- Update staging records with new NP IDs
        UPDATE sr
        SET
            sr.NewNPId = p.NewProviderId,
            sr.ResolvedNursePractitionerId = p.NewProviderId,
            sr.NursePractitionerWasCreated = 1
        FROM #StagingRecords sr
        INNER JOIN #NPsToInsert p ON sr.NormalizedNursePractitionerLastName = p.LastName
        WHERE sr.NeedsNewNP = 1 AND sr.IsValid = 1;

        -- ===========================================
        -- SECTION 8: Insert Hospitalizations
        -- ===========================================

        -- Set final patient IDs (either resolved or newly created)
        UPDATE sr
        SET sr.ResolvedPatientId = COALESCE(sr.NewPatientId, sr.ResolvedPatientId)
        FROM #StagingRecords sr
        WHERE sr.ResolvedPatientId IS NULL AND sr.NewPatientId IS NOT NULL;

        CREATE TABLE #HospitalizationsToInsert (
            TempId INT IDENTITY(1,1) PRIMARY KEY,
            PatientId INT,
            CaseId INT,
            FacilityId INT,
            PsychEvaluation BIT,
            AdmissionDate SMALLDATETIME,
            HospitalizationStatusId INT,
            StagingId INT,
            NewHospitalizationId INT
        );

        INSERT INTO #HospitalizationsToInsert (PatientId, CaseId, FacilityId, PsychEvaluation, AdmissionDate, HospitalizationStatusId, StagingId)
        SELECT DISTINCT
            sr.ResolvedPatientId,
            sr.ParsedCaseId,
            sr.FacilityId,
            sr.IsPsychEval,
            sr.Admit,
            CASE WHEN sr.IsPsychEval = 1 THEN 8 ELSE 1 END,
            sr.StagingId
        FROM #StagingRecords sr
        WHERE sr.NeedsNewHospitalization = 1
          AND sr.IsValid = 1
          AND sr.ResolvedPatientId IS NOT NULL
          AND sr.ParsedCaseId IS NOT NULL
          AND NOT EXISTS (
              SELECT 1 FROM [Clinical].[Hospitalizations] h
              WHERE h.CaseId = sr.ParsedCaseId
          );

        -- Insert hospitalizations using cursor
        DECLARE @HospTempId INT, @HospPatientId INT, @HospCaseId INT, @HospFacilityId INT;
        DECLARE @HospPsychEval BIT, @HospAdmitDate SMALLDATETIME, @HospStatusId INT, @NewHospId INT;

        DECLARE hosp_cursor CURSOR LOCAL FAST_FORWARD FOR
            SELECT TempId, PatientId, CaseId, FacilityId, PsychEvaluation, AdmissionDate, HospitalizationStatusId
            FROM #HospitalizationsToInsert;

        OPEN hosp_cursor;
        FETCH NEXT FROM hosp_cursor INTO @HospTempId, @HospPatientId, @HospCaseId, @HospFacilityId, @HospPsychEval, @HospAdmitDate, @HospStatusId;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            INSERT INTO [Clinical].[Hospitalizations] (PatientId, CaseId, FacilityId, PsychEvaluation, AdmissionDate, HospitalizationStatusId)
            VALUES (@HospPatientId, @HospCaseId, @HospFacilityId, @HospPsychEval, @HospAdmitDate, @HospStatusId);

            SET @NewHospId = SCOPE_IDENTITY();

            UPDATE #HospitalizationsToInsert SET NewHospitalizationId = @NewHospId WHERE TempId = @HospTempId;

            FETCH NEXT FROM hosp_cursor INTO @HospTempId, @HospPatientId, @HospCaseId, @HospFacilityId, @HospPsychEval, @HospAdmitDate, @HospStatusId;
        END

        CLOSE hosp_cursor;
        DEALLOCATE hosp_cursor;

        -- Update staging records with new hospitalization IDs
        UPDATE sr
        SET
            sr.NewHospitalizationId = h.NewHospitalizationId,
            sr.ResolvedHospitalizationId = h.NewHospitalizationId,
            sr.HospitalizationWasCreated = 1
        FROM #StagingRecords sr
        INNER JOIN #HospitalizationsToInsert h ON sr.StagingId = h.StagingId;

        -- Set final hospitalization IDs
        UPDATE sr
        SET sr.ResolvedHospitalizationId = COALESCE(sr.NewHospitalizationId, sr.ResolvedHospitalizationId)
        FROM #StagingRecords sr
        WHERE sr.ResolvedHospitalizationId IS NULL AND sr.NewHospitalizationId IS NOT NULL;

        -- ===========================================
        -- SECTION 9: Insert Visits
        -- ===========================================

        CREATE TABLE #VisitsToInsert (
            TempId INT IDENTITY(1,1) PRIMARY KEY,
            DateServiced DATE,
            HospitalizationId INT,
            Room NVARCHAR(20),
            Bed NVARCHAR(5),
            StagingId INT,
            NewVisitId INT
        );

        INSERT INTO #VisitsToInsert (DateServiced, HospitalizationId, Room, Bed, StagingId)
        SELECT
            sr.ServiceDate,
            sr.ResolvedHospitalizationId,
            ISNULL(sr.Room, ''),
            ISNULL(sr.Bed, ''),
            sr.StagingId
        FROM #StagingRecords sr
        WHERE sr.IsValid = 1
          AND sr.ResolvedHospitalizationId IS NOT NULL;

        -- Insert visits using cursor
        DECLARE @VisitTempId INT, @VisitDate DATE, @VisitHospId INT;
        DECLARE @VisitRoom NVARCHAR(20), @VisitBed NVARCHAR(5), @NewVisitId INT;

        DECLARE visit_cursor CURSOR LOCAL FAST_FORWARD FOR
            SELECT TempId, DateServiced, HospitalizationId, Room, Bed
            FROM #VisitsToInsert;

        OPEN visit_cursor;
        FETCH NEXT FROM visit_cursor INTO @VisitTempId, @VisitDate, @VisitHospId, @VisitRoom, @VisitBed;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            INSERT INTO [Clinical].[Visits] (DateServiced, HospitalizationId, IsScribeServiceOnly, Room, Bed)
            VALUES (@VisitDate, @VisitHospId, 0, @VisitRoom, @VisitBed);

            SET @NewVisitId = SCOPE_IDENTITY();

            UPDATE #VisitsToInsert SET NewVisitId = @NewVisitId WHERE TempId = @VisitTempId;

            FETCH NEXT FROM visit_cursor INTO @VisitTempId, @VisitDate, @VisitHospId, @VisitRoom, @VisitBed;
        END

        CLOSE visit_cursor;
        DEALLOCATE visit_cursor;

        -- Update staging records with new visit IDs
        UPDATE sr
        SET sr.NewVisitId = v.NewVisitId
        FROM #StagingRecords sr
        INNER JOIN #VisitsToInsert v ON sr.StagingId = v.StagingId;

        -- ===========================================
        -- SECTION 10: Insert VisitAttendants
        -- ===========================================

        -- Set final provider IDs
        UPDATE sr
        SET
            sr.ResolvedPhysicianId = COALESCE(sr.NewPhysicianId, sr.ResolvedPhysicianId),
            sr.ResolvedNursePractitionerId = COALESCE(sr.NewNPId, sr.ResolvedNursePractitionerId)
        FROM #StagingRecords sr;

        -- Link physicians to visits
        INSERT INTO [Clinical].[VisitAttendants] (VisitId, ProviderId)
        SELECT DISTINCT
            sr.NewVisitId,
            sr.ResolvedPhysicianId
        FROM #StagingRecords sr
        WHERE sr.NewVisitId IS NOT NULL
          AND sr.ResolvedPhysicianId IS NOT NULL
          AND NOT EXISTS (
              SELECT 1 FROM [Clinical].[VisitAttendants] va
              WHERE va.VisitId = sr.NewVisitId
                AND va.ProviderId = sr.ResolvedPhysicianId
          );

        -- Link nurse practitioners to visits
        INSERT INTO [Clinical].[VisitAttendants] (VisitId, ProviderId)
        SELECT DISTINCT
            sr.NewVisitId,
            sr.ResolvedNursePractitionerId
        FROM #StagingRecords sr
        WHERE sr.NewVisitId IS NOT NULL
          AND sr.ResolvedNursePractitionerId IS NOT NULL
          AND NOT EXISTS (
              SELECT 1 FROM [Clinical].[VisitAttendants] va
              WHERE va.VisitId = sr.NewVisitId
                AND va.ProviderId = sr.ResolvedNursePractitionerId
          );

        -- ===========================================
        -- SECTION 11: Update Staging Table
        -- ===========================================

        -- Update successfully processed records
        UPDATE spa
        SET
            spa.ResolvedVisitId = sr.NewVisitId,
            spa.ResolvedPatientId = sr.ResolvedPatientId,
            spa.ResolvedPhysicianId = sr.ResolvedPhysicianId,
            spa.ResolvedNursePractitionerId = sr.ResolvedNursePractitionerId,
            spa.ResolvedHospitalizationId = sr.ResolvedHospitalizationId,
            spa.PatientWasCreated = sr.PatientWasCreated,
            spa.PatientFacilityWasCreated = sr.PatientFacilityWasCreated,
            spa.PhysicianWasCreated = sr.PhysicianWasCreated,
            spa.NursePractitionerWasCreated = sr.NursePractitionerWasCreated,
            spa.HospitalizationWasCreated = sr.HospitalizationWasCreated,
            spa.Imported = CASE WHEN sr.NewVisitId IS NOT NULL THEN 1 ELSE 0 END,
            spa.UpdatedAtUtc = SYSUTCDATETIME()
        FROM [staging].[ProviderAssignments] spa
        INNER JOIN #StagingRecords sr ON spa.Id = sr.StagingId
        WHERE sr.IsValid = 1;

        -- ===========================================
        -- SECTION 12: Calculate Results
        -- ===========================================

        SELECT @ProcessedCount = COUNT(*)
        FROM #StagingRecords
        WHERE NewVisitId IS NOT NULL;

        SELECT @ErrorCount = COUNT(*)
        FROM #StagingRecords
        WHERE IsValid = 0 OR NewVisitId IS NULL;

        -- ===========================================
        -- SECTION 13: Update Batch Status to Processed
        -- ===========================================

        UPDATE [staging].[ProviderAssignmentBatches]
        SET
            [Status] = 3,  -- Processed
            [UpdatedAtUtc] = SYSUTCDATETIME()
        WHERE [Id] = @BatchId;

        -- Only commit if we started the transaction
        IF @OwnTransaction = 1
            COMMIT TRANSACTION;
        RETURN 0;

    END TRY
    BEGIN CATCH
        SET @ErrorMessage = ERROR_MESSAGE();
        SET @ErrorCount = (SELECT COUNT(*) FROM #StagingRecords);

        -- Build detailed error message
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        DECLARE @ErrorLine INT = ERROR_LINE();
        DECLARE @ErrorProcedure NVARCHAR(200) = ERROR_PROCEDURE();

        SET @ErrorMessage = CONCAT(
            'Error in ', ISNULL(@ErrorProcedure, 'Unknown'),
            ' at line ', @ErrorLine,
            ': ', @ErrorMessage
        );

        -- Handle transaction cleanup based on ownership
        IF @OwnTransaction = 1
        BEGIN
            -- We own the transaction, roll it back
            IF @@TRANCOUNT > 0
                ROLLBACK TRANSACTION;
            RETURN -1;
        END
        ELSE
        BEGIN
            -- We're in a caller's transaction - re-raise the error
            -- Let the caller handle the rollback
            THROW;
        END
    END CATCH;
END;
GO

-- =============================================
-- Author:      ConsilientWebApp Team
-- Create date: 2026-01-19
-- Description: Undoes provider assignments import by deleting records created during import.
--              Uses *WasCreated flags to determine which records to delete.
--              Deletes in reverse order of creation to respect FK constraints.
-- =============================================
CREATE OR ALTER PROCEDURE [staging].[usp_UndoProviderAssignments]
    @BatchId UNIQUEIDENTIFIER,
    @UndoneCount INT OUTPUT,
    @ErrorMessage NVARCHAR(MAX) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- Track whether we own the transaction (for caller-managed transactions)
    DECLARE @OwnTransaction BIT = 0;

    -- Initialize output parameters
    SET @UndoneCount = 0;
    SET @ErrorMessage = NULL;

    -- ===========================================
    -- SECTION 1: Create Temp Table for Staging Records
    -- ===========================================

    CREATE TABLE #StagingRecords (
        StagingId INT NOT NULL PRIMARY KEY,
        Mrn NVARCHAR(50),
        FacilityId INT,
        ResolvedPatientId INT,
        ResolvedPhysicianId INT,
        ResolvedNursePractitionerId INT,
        ResolvedHospitalizationId INT,
        ResolvedVisitId INT,
        PatientWasCreated BIT,
        PatientFacilityWasCreated BIT,
        PhysicianWasCreated BIT,
        NursePractitionerWasCreated BIT,
        HospitalizationWasCreated BIT
    );

    BEGIN TRY
        -- Only start a transaction if not already in one (allows caller-managed transactions)
        IF @@TRANCOUNT = 0
        BEGIN
            BEGIN TRANSACTION;
            SET @OwnTransaction = 1;
        END

        -- ===========================================
        -- SECTION 2: Load Imported Staging Records
        -- ===========================================

        INSERT INTO #StagingRecords (
            StagingId, Mrn,
            FacilityId, ResolvedPatientId, ResolvedPhysicianId,
            ResolvedNursePractitionerId, ResolvedHospitalizationId, ResolvedVisitId,
            PatientWasCreated, PatientFacilityWasCreated, PhysicianWasCreated,
            NursePractitionerWasCreated, HospitalizationWasCreated
        )
        SELECT
            Id, Mrn,
            FacilityId, ResolvedPatientId, ResolvedPhysicianId,
            ResolvedNursePractitionerId, ResolvedHospitalizationId, ResolvedVisitId,
            PatientWasCreated, PatientFacilityWasCreated, PhysicianWasCreated,
            NursePractitionerWasCreated, HospitalizationWasCreated
        FROM [staging].[ProviderAssignments]
        WHERE BatchId = @BatchId
          AND Imported = 1;

        -- Check if we have records to undo
        IF NOT EXISTS (SELECT 1 FROM #StagingRecords)
        BEGIN
            SET @ErrorMessage = 'No imported records found for BatchId: ' + CAST(@BatchId AS NVARCHAR(36));
            IF @OwnTransaction = 1
                COMMIT TRANSACTION;
            RETURN 0;
        END

        -- Store the count for output
        SELECT @UndoneCount = COUNT(*) FROM #StagingRecords;

        -- ===========================================
        -- SECTION 3: Delete VisitAttendants
        -- ===========================================

        DELETE va
        FROM [Clinical].[VisitAttendants] va
        WHERE va.VisitId IN (SELECT ResolvedVisitId FROM #StagingRecords WHERE ResolvedVisitId IS NOT NULL);

        -- ===========================================
        -- SECTION 4: Delete Visits
        -- ===========================================

        DELETE v
        FROM [Clinical].[Visits] v
        WHERE v.Id IN (SELECT ResolvedVisitId FROM #StagingRecords WHERE ResolvedVisitId IS NOT NULL);

        -- ===========================================
        -- SECTION 5: Delete Hospitalizations (only if created by import)
        -- ===========================================

        DELETE h
        FROM [Clinical].[Hospitalizations] h
        WHERE h.Id IN (
            SELECT DISTINCT ResolvedHospitalizationId
            FROM #StagingRecords
            WHERE HospitalizationWasCreated = 1
              AND ResolvedHospitalizationId IS NOT NULL
        );

        -- ===========================================
        -- SECTION 6: Delete Nurse Practitioners (only if created by import)
        -- ===========================================

        DELETE p
        FROM [Clinical].[Providers] p
        WHERE p.Id IN (
            SELECT DISTINCT ResolvedNursePractitionerId
            FROM #StagingRecords
            WHERE NursePractitionerWasCreated = 1
              AND ResolvedNursePractitionerId IS NOT NULL
        );

        -- ===========================================
        -- SECTION 7: Delete Physicians (only if created by import)
        -- ===========================================

        DELETE p
        FROM [Clinical].[Providers] p
        WHERE p.Id IN (
            SELECT DISTINCT ResolvedPhysicianId
            FROM #StagingRecords
            WHERE PhysicianWasCreated = 1
              AND ResolvedPhysicianId IS NOT NULL
        );

        -- ===========================================
        -- SECTION 8: Delete PatientFacilities (only if created by import)
        -- ===========================================

        DELETE pf
        FROM [Clinical].[PatientFacilities] pf
        WHERE EXISTS (
            SELECT 1
            FROM #StagingRecords sr
            WHERE sr.PatientFacilityWasCreated = 1
              AND sr.ResolvedPatientId IS NOT NULL
              AND pf.PatientId = sr.ResolvedPatientId
              AND pf.FacilityId = sr.FacilityId
              AND pf.MRN = sr.Mrn
        );

        -- ===========================================
        -- SECTION 9: Delete Patients (only if created by import)
        -- ===========================================

        DELETE p
        FROM [Clinical].[Patients] p
        WHERE p.Id IN (
            SELECT DISTINCT ResolvedPatientId
            FROM #StagingRecords
            WHERE PatientWasCreated = 1
              AND ResolvedPatientId IS NOT NULL
        );

        -- ===========================================
        -- SECTION 10: Reset Staging Records
        -- ===========================================

        UPDATE spa
        SET
            -- Always clear visit ID and set imported to 0
            spa.ResolvedVisitId = NULL,
            spa.Imported = 0,
            -- Clear resolved IDs only if they were created by import
            spa.ResolvedHospitalizationId = CASE WHEN sr.HospitalizationWasCreated = 1 THEN NULL ELSE spa.ResolvedHospitalizationId END,
            spa.ResolvedNursePractitionerId = CASE WHEN sr.NursePractitionerWasCreated = 1 THEN NULL ELSE spa.ResolvedNursePractitionerId END,
            spa.ResolvedPhysicianId = CASE WHEN sr.PhysicianWasCreated = 1 THEN NULL ELSE spa.ResolvedPhysicianId END,
            spa.ResolvedPatientId = CASE WHEN sr.PatientWasCreated = 1 THEN NULL ELSE spa.ResolvedPatientId END,
            -- Reset all WasCreated flags
            spa.PatientWasCreated = 0,
            spa.PatientFacilityWasCreated = 0,
            spa.PhysicianWasCreated = 0,
            spa.NursePractitionerWasCreated = 0,
            spa.HospitalizationWasCreated = 0,
            spa.UpdatedAtUtc = SYSUTCDATETIME()
        FROM [staging].[ProviderAssignments] spa
        INNER JOIN #StagingRecords sr ON spa.Id = sr.StagingId;

        -- ===========================================
        -- SECTION 11: Reset Batch Status to Resolved
        -- ===========================================

        UPDATE [staging].[ProviderAssignmentBatches]
        SET
            [Status] = 2,  -- Resolved (ready for re-processing)
            [UpdatedAtUtc] = SYSUTCDATETIME()
        WHERE [Id] = @BatchId;

        -- Only commit if we started the transaction
        IF @OwnTransaction = 1
            COMMIT TRANSACTION;
        RETURN 0;

    END TRY
    BEGIN CATCH
        SET @ErrorMessage = ERROR_MESSAGE();
        SET @UndoneCount = 0;

        -- Build detailed error message
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        DECLARE @ErrorLine INT = ERROR_LINE();
        DECLARE @ErrorProcedure NVARCHAR(200) = ERROR_PROCEDURE();

        SET @ErrorMessage = CONCAT(
            'Error in ', ISNULL(@ErrorProcedure, 'Unknown'),
            ' at line ', @ErrorLine,
            ': ', @ErrorMessage
        );

        -- Handle transaction cleanup based on ownership
        IF @OwnTransaction = 1
        BEGIN
            -- We own the transaction, roll it back
            IF @@TRANCOUNT > 0
                ROLLBACK TRANSACTION;
            RETURN -1;
        END
        ELSE
        BEGIN
            -- We're in a caller's transaction - re-raise the error
            -- Let the caller handle the rollback
            THROW;
        END
    END CATCH;
END;
GO
