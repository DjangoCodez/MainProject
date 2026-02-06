# Setup new servers in SoftOne Status

Database is: 
appservicesdatabases.database.windows.net

Username and password is found in Azure Keyvault


Before running this script, please review and adjust the variables as needed to match your environment.

## Table StatusServer
Begin with adding server(s) to SysServer table. There are reserved numbers (up to 20) for production servers 

## Table StatusService

Control the script, make sure the SysCompDbIds are up to date. 
Also check if the domain search (s8s1*) is still a good option

## Script

```sql
DECLARE @DatabaseId INT = 1;
DECLARE @ServerStart INT = 5;
DECLARE @ServerEnd INT = 8;
DECLARE @Domain NVARCHAR(50) = '.softone.se';

-- Set the list of all current production SysCompDbIds
DECLARE @SysCompDbIdsList TABLE (Id INT);
INSERT INTO @SysCompDbIdsList (Id) VALUES (1), (2), (7), (8), (18), (19), (39), (40), (50), (51);

DECLARE @Counter INT = @ServerStart;
DECLARE @Id INT;

WHILE @Counter <= @ServerEnd
BEGIN
    -- Insert Apps(Counter).softone.se into service
    INSERT INTO [dbo].[StatusService] (Created, Domain, Modified, Name, ServerId, State, StatusServerId, StatusServiceGroupId, Url, ProductionType, SysCompDbId) 
    VALUES (GETDATE(), 
            'Apps' + CAST(@Counter AS NVARCHAR(10)), 
            GETDATE(), 
            'Apps' + CAST(@Counter AS NVARCHAR(10)), 
            0, 
            0, 
            @Counter, 
            41, 
            'https://apps' + CAST(@Counter AS NVARCHAR(10)) + @Domain, 
            0, 
            0);
    
    DECLARE IdCursor CURSOR FOR SELECT Id FROM @SysCompDbIdsList;
    OPEN IdCursor;
    FETCH NEXT FROM IdCursor INTO @Id;
    
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        DECLARE @GroupId INT;
        DECLARE @SysCompDbId INT;
		DECLARE @TemplateStatusServiceId INT;
        
        -- Get corresponding StatusServiceGroupId
        SELECT TOP 1 @GroupId = StatusServiceGroupId FROM [dbo].[StatusService] WHERE Domain = 's8s1d' + CAST(@Id AS NVARCHAR(10)) ORDER BY Created DESC;
        
        -- Get corresponding SysCompDbId
        SELECT TOP 1 @SysCompDbId = SysCompDbId FROM [dbo].[StatusService] WHERE Domain = 's8s1d' + CAST(@Id AS NVARCHAR(10)) ORDER BY Created DESC;

		-- Get template StatusServiceId for reference
        SELECT TOP 1 @TemplateStatusServiceId = StatusServiceId FROM [dbo].[StatusService] WHERE Domain = 's8s1d' + CAST(@Id AS NVARCHAR(10)) ORDER BY Created DESC;

        INSERT INTO [dbo].[StatusService] (Created, Domain, Modified, Name, ServerId, State, StatusServerId, StatusServiceGroupId, Url, ProductionType, SysCompDbId) 
        VALUES (GETDATE(), 
                's' + CAST(@Counter AS NVARCHAR(10)) + 's8d' + CAST(@Id AS NVARCHAR(10)), 
                GETDATE(), 
                's' + CAST(@Counter AS NVARCHAR(10)) + 's8d' + CAST(@Id AS NVARCHAR(10)), 
                9999, -- SoftOne status will fix this if 9999
                0, 
                @Counter, 
                @GroupId, 
                'https://s' + CAST(@Counter AS NVARCHAR(10)) + 's8d' + CAST(@Id AS NVARCHAR(10)) + @Domain, 
                0, 
                @SysCompDbId);
        
        -- Insert into StatusServiceType based on template (corresponding)
        INSERT INTO [dbo].[StatusServiceType] (Prio, ServiceType, StatusServiceId, Settings) 
        SELECT Prio, ServiceType, (SELECT MAX(StatusServiceId) FROM [dbo].[StatusService]), Settings 
        FROM [dbo].[StatusServiceType] WHERE StatusServiceId = @TemplateStatusServiceId and prio = 1;
        
        FETCH NEXT FROM IdCursor INTO @Id;
    END
    
    CLOSE IdCursor;
    DEALLOCATE IdCursor;
    
    SET @Counter = @Counter + 1;
END