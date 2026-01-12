-- Discover user-created schemas, excluding system schemas
-- This script is used by the docs_db.yml workflow to dynamically discover
-- which schemas exist in a database, without hardcoding schema names.
--
-- Excludes system schemas and schemas created by Microsoft tools.
-- Returns a sorted list of user schema names.

SELECT s.name AS SchemaName
FROM sys.schemas s
LEFT JOIN sys.extended_properties ep
  ON ep.major_id = s.schema_id
  AND ep.name = 'microsoft_database_tools_support'
WHERE s.schema_id BETWEEN 5 AND 16384
  AND s.name NOT IN (
    'sys', 'INFORMATION_SCHEMA', 'guest',
    'db_owner', 'db_accessadmin', 'db_securityadmin',
    'db_ddladmin', 'db_backupoperator', 'db_datareader',
    'db_datawriter', 'db_denydatareader', 'db_denydatawriter'
  )
  AND ep.major_id IS NULL
ORDER BY s.name;
