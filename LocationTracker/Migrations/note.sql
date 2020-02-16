CREATE TABLE `note` (
	`id` INT NOT NULL AUTO_INCREMENT,
	`date` DATE NOT NULL,
	`text` TEXT,
	UNIQUE KEY `date` (`date`) USING BTREE,
	PRIMARY KEY (`id`)
);