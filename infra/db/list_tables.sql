SELECT SCHEMA_NAME(schema_id) as SchemaName, name as TableName, create_date as Created
FROM sys.tables
ORDER BY SchemaName, TableName;
