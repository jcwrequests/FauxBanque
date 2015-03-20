CREATE KEYSPACE IF NOT EXISTS 
"EventStore" WITH replication = 
{'class':'NetworkTopologyStrategy', 'replication_factor':3};