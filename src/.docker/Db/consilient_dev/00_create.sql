-- Create the database if it does not already exist
USE [master]
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'consilient_dev')
BEGIN
    CREATE DATABASE [consilient_dev];
END;
GO
