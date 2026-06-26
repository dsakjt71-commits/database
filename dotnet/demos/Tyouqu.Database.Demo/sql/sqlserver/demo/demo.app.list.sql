-- Query latest apps.
select top (@Take)
    app_id as AppId,
    app_name as AppName,
    client_id as ClientId,
    status as Status
from Apps
order by app_id desc;

