-- First result set: app detail.
select
    app_id as AppId,
    app_name as AppName,
    client_id as ClientId,
    status as Status
from Apps
where app_id = @AppId;

-- Second result set: app stats.
select
    count(1) as TotalApps,
    sum(case when status = 1 then 1 else 0 end) as EnabledApps
from Apps;

