CREATE KEYSPACE IF NOT EXISTS 
"EventStore" WITH replication = 
{'class':'SimpleStrategy', 'replication_factor':3};