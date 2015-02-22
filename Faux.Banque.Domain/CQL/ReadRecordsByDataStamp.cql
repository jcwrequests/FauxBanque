SELECT * 
FROM    "EventStore"."Events"
WHERE version_time_stamp  > {0} LIMIT {1} ALLOW FILTERING;