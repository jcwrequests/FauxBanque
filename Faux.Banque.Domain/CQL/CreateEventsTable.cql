CREATE TABLE IF NOT EXISTS
 "EventStore"."Events" (
id uuid,
name varchar,
version bigint,
version_time_stamp timestamp,
data blob,
PRIMARY KEY(name,version_time_stamp, version ))
WITH CLUSTERING ORDER BY (version_time_stamp ASC);