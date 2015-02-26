CREATE TABLE IF NOT EXISTS
"EventStore"."EventsVersionsToBeProcessed"(
version_time_stamp timestamp,
processed boolean,
PRIMARY KEY(version_time_stamp , processed));