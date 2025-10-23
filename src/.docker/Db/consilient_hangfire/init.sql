-- Create the database if it does not already exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'consilient_hangfire')
BEGIN
    CREATE DATABASE [consilient_hangfire];
END;
GO