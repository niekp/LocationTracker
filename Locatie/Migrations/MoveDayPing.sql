ALTER TABLE ping ADD COLUMN dag_id int AFTER rit_id, ADD KEY(dag_id);
UPDATE ping SET dag_id = (SELECT dag_id FROM dag_ping WHERE ping_id = ping.id LIMIT 0, 1);

ALTER TABLE locatie DROP COLUMN rustmode;

/*
reset:

delete from dag where tijd_van >= '2020/1/27';
delete from rit where tijd_van >= '2020/1/27';
update ping set rit_id = null, locatie_id = null, verwerkt = 0 where tijd >= '2020/1/27';