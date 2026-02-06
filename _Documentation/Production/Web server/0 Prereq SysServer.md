# Setup new SysServers in sys database 

Before running this script, please review and adjust the variables as needed to match your environment.

## Script

```sql
DECLARE @DatabaseId INT = 1;
DECLARE @ServerStart INT = 5;
DECLARE @ServerEnd INT = 8;
DECLARE @Domain NVARCHAR(50) = '.softone.se';

DECLARE @IdList TABLE (Id INT);
INSERT INTO @IdList (Id) VALUES (1), (2), (7), (8), (18), (19), (39), (40), (50), (51);

DECLARE @Counter INT = @ServerStart;
DECLARE @Id INT;

WHILE @Counter <= @ServerEnd
BEGIN
    DECLARE IdCursor CURSOR FOR SELECT Id FROM @IdList;
    OPEN IdCursor;
    FETCH NEXT FROM IdCursor INTO @Id;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        INSERT INTO [dbo].[SysServer] (Url, UseLoadBalancer) 
        VALUES ('https://s' + CAST(@Counter AS NVARCHAR(10)) + 's1d' + CAST(@Id AS NVARCHAR(10)) + @Domain, 0);
        
        FETCH NEXT FROM IdCursor INTO @Id;
    END
    
    CLOSE IdCursor;
    DEALLOCATE IdCursor;
    
    SET @Counter = @Counter + 1;
END
