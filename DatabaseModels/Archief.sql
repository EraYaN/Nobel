SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='TRADITIONAL,ALLOW_INVALID_DATES';

CREATE SCHEMA IF NOT EXISTS `dehaantj_dsbarchief` DEFAULT CHARACTER SET utf8 COLLATE utf8_general_ci ;
USE `dehaantj_dsbarchief` ;

-- -----------------------------------------------------
-- Table `dehaantj_dsbarchief`.`documenten`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `dehaantj_dsbarchief`.`documenten` (
  `document_id` INT UNSIGNED NOT NULL AUTO_INCREMENT ,
  `titel` VARCHAR(255) NULL ,
  `omschrijving` TEXT NULL ,
  `datum` DATE NULL ,
  `datum_nauwkeurigheid` ENUM('jaar','maand','dag') NULL ,
  `datum_archivering` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ,
  `type` SET('hardcopy','digitaal') NOT NULL DEFAULT 'digitaal' ,
  `locatie_file` VARCHAR(255) NULL COMMENT 'relatief tot project master.' ,
  `locatie_hc` BIGINT UNSIGNED NULL COMMENT 'locatie als in map id' ,
  PRIMARY KEY (`document_id`) )
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `dehaantj_dsbarchief`.`mappen`
-- -----------------------------------------------------
CREATE  TABLE IF NOT EXISTS `dehaantj_dsbarchief`.`mappen` (
  `map_id` INT NOT NULL ,
  `kast` VARCHAR(45) NULL ,
  `titel` VARCHAR(45) NULL ,
  `omschrijving` TEXT NULL ,
  PRIMARY KEY (`map_id`) )
ENGINE = InnoDB;

USE `dehaantj_dsbarchief` ;


SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;
