CREATE TABLE IF NOT EXISTS
"EventStore"."EventsToBeProcessed" (
id uuid,
name varchar,
version int,
version_time_stamp timestamp,
data blob,
processed boolean,
PRIMARY KEY(processed,version_time_stamp,name,version ))
WITH CLUSTERING ORDER BY (version_time_stamp ASC, name DESC, version ASC);