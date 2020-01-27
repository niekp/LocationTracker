ALTER TABLE ping ADD COLUMN dag_id int AFTER rit_id, ADD KEY(dag_id);
UPDATE ping SET dag_id = (SELECT dag_id FROM dag_ping WHERE ping_id = ping.id LIMIT 0, 1);

ALTER TABLE locatie DROP COLUMN rustmode;