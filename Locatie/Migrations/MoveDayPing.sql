ALTER TABLE ping ADD COLUMN dag_id int AFTER rit_id, ADD KEY(dag_id);
UPDATE ping SET dag_id = (SELECT dag_id FROM dag_ping WHERE ping_id = ping.id LIMIT 0, 1);

ALTER TABLE locatie DROP COLUMN rustmode;

-- fout
--ALTER TABLE user ADD COLUMN code varchar(255);

CREATE TABLE `user_session` (
	`id` INT NOT NULL AUTO_INCREMENT,
	`user_id` INT NOT NULL,
	`token` VARCHAR(255) NOT NULL,
	`valid_till` DATETIME NOT NULL,
	KEY `user_id` (`user_id`) USING HASH,
	PRIMARY KEY (`id`)
);
/*
reset:

delete from dag where tijd_van >= '2020/1/28';
update ping set rit_id = null, locatie_id = null, verwerkt = 0 where tijd >= '2020/1/28';
delete from rit where tijd_van >= '2020/1/28';

5c719e5b33c6223d7a06f3fd258de44974591a6fd9fdb78befc9cd2d4ef5a2f1
