-- phpMyAdmin SQL Dump
-- version 4.0.9
-- http://www.phpmyadmin.net
--
-- Host: localhost
-- Generation Time: Nov 24, 2014 at 05:11 PM
-- Server version: 5.5.34
-- PHP Version: 5.3.27

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

--
-- Database: `joostqa22_joost`
--

-- --------------------------------------------------------

--
-- Table structure for table `begroting`
--

CREATE TABLE IF NOT EXISTS `begroting` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `jaar` year(4) NOT NULL,
  `is_inkomst` tinyint(1) NOT NULL,
  `post` varchar(64) NOT NULL COMMENT 'slash voor subpost ("contributie/leden"), daarnaast post invoeren dat met gb_ ervoor tabel is',
  `bedrag` decimal(12,4) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=36 ;

-- --------------------------------------------------------

--
-- Table structure for table `crediteur`
--

CREATE TABLE IF NOT EXISTS `crediteur` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `nummer` int(11) NOT NULL,
  `type` enum('DSB','Nobel','Lid','Extern') NOT NULL,
  `naam` varchar(64) NOT NULL,
  `iban` varchar(32) NOT NULL,
  `saldo` decimal(10,0) NOT NULL,
  `lid_id` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `nummer` (`nummer`),
  KEY `lid_id` (`lid_id`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=31 ;

-- --------------------------------------------------------

--
-- Table structure for table `debiteur`
--

CREATE TABLE IF NOT EXISTS `debiteur` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `nummer` int(11) NOT NULL,
  `type` enum('DSB','Nobel','Lid','Extern') DEFAULT NULL,
  `naam` varchar(64) NOT NULL,
  `iban` varchar(32) NOT NULL,
  `saldo` decimal(10,0) NOT NULL,
  `lid_id` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `nummer` (`nummer`),
  KEY `lid_id` (`lid_id`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=32 ;

-- --------------------------------------------------------

--
-- Table structure for table `gb_activiteit`
--

CREATE TABLE IF NOT EXISTS `gb_activiteit` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `datum` date NOT NULL,
  `activiteit` varchar(64) NOT NULL COMMENT '/ ertussen voor subactiviteit (bijv. "kabila/strobo" of "ip/collegekermis")',
  `factuurnummer` varchar(64) NOT NULL,
  `bedrag` decimal(12,4) NOT NULL,
  `uitleg` varchar(256) NOT NULL,
  `crediteurnummer` int(11) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=5 ;

-- --------------------------------------------------------

--
-- Table structure for table `gb_bankboek`
--

CREATE TABLE IF NOT EXISTS `gb_bankboek` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `rekening` enum('lopend','spaar') NOT NULL,
  `valuta` varchar(8) NOT NULL,
  `datum` date NOT NULL,
  `beginsaldo` decimal(12,4) NOT NULL,
  `eindsaldo` decimal(12,4) NOT NULL,
  `transactiebedrag` decimal(12,4) NOT NULL,
  `beschrijving` varchar(512) NOT NULL,
  `type` varchar(64) NOT NULL,
  `tegenrekening` varchar(32) NOT NULL COMMENT 'iban of ABN',
  `naam` varchar(32) NOT NULL,
  `opmerking` varchar(256) NOT NULL,
  `cred/debiteurnummer` int(11) DEFAULT NULL,
  `cred/debit` varchar(64) DEFAULT NULL COMMENT 'id van gb_crediteur of gb_debiteur',
  PRIMARY KEY (`id`),
  UNIQUE KEY `rekening` (`rekening`,`valuta`,`datum`,`beginsaldo`,`eindsaldo`,`transactiebedrag`,`beschrijving`),
  KEY `cred/debit` (`cred/debit`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=107 ;

-- --------------------------------------------------------

--
-- Table structure for table `gb_barbon`
--

CREATE TABLE IF NOT EXISTS `gb_barbon` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `datum` date NOT NULL,
  `nummer` varchar(64) NOT NULL,
  `cred/debnummer` int(11) NOT NULL,
  `bedrag` decimal(12,4) NOT NULL,
  `opmerking` varchar(64) NOT NULL,
  `persoonlijk/ho` enum('persoonlijk','ho') NOT NULL,
  `weging` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `crediteurnummer` (`cred/debnummer`),
  KEY `nummer` (`nummer`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=1573 ;

-- --------------------------------------------------------

--
-- Table structure for table `gb_contributie`
--

CREATE TABLE IF NOT EXISTS `gb_contributie` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `datum` date NOT NULL,
  `type` enum('lid','buitengewoon_lid','reunist') NOT NULL DEFAULT 'lid',
  `omschrijving` varchar(32) NOT NULL COMMENT 'maand jaar',
  `bedrag` decimal(12,4) NOT NULL,
  `debiteurnummer` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `debiteurnummer` (`debiteurnummer`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=44 ;

-- --------------------------------------------------------

--
-- Table structure for table `gb_crediteur`
--

CREATE TABLE IF NOT EXISTS `gb_crediteur` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `datum` date NOT NULL,
  `factuurnummer` varchar(64) NOT NULL,
  `grootboekrekeningnummer` int(11) NOT NULL,
  `bedrag` decimal(12,4) NOT NULL,
  `uitleg` varchar(256) NOT NULL,
  `crediteurnummer` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `grootboekrekeningnummer` (`grootboekrekeningnummer`),
  KEY `crediteurnummer` (`crediteurnummer`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=100 ;

-- --------------------------------------------------------

--
-- Table structure for table `gb_debiteur`
--

CREATE TABLE IF NOT EXISTS `gb_debiteur` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `datum` date NOT NULL,
  `factuurnummer` varchar(64) NOT NULL,
  `grootboekrekeningnummer` int(11) NOT NULL,
  `bedrag` decimal(12,4) NOT NULL,
  `uitleg` varchar(256) NOT NULL,
  `debiteurnummer` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `grootboekrekeningnummer` (`grootboekrekeningnummer`),
  KEY `debiteurnummer` (`debiteurnummer`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=1075 ;

-- --------------------------------------------------------

--
-- Table structure for table `gb_lustrum_spaarplan`
--

CREATE TABLE IF NOT EXISTS `gb_lustrum_spaarplan` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `datum` date NOT NULL,
  `omschrijving` varchar(32) NOT NULL COMMENT 'maand jaar',
  `bedrag` decimal(12,4) NOT NULL,
  `debiteurnummer` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `debiteurnummer` (`debiteurnummer`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=41 ;

-- --------------------------------------------------------

--
-- Table structure for table `gb_memoriaal`
--

CREATE TABLE IF NOT EXISTS `gb_memoriaal` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `datum` date NOT NULL,
  `factuurnummer` varchar(64) NOT NULL,
  `bedrag` decimal(12,4) NOT NULL,
  `uitleg` varchar(256) NOT NULL COMMENT '(''algemene_reserve'')',
  `cred/debiteurnummer` int(11) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=6 ;

-- --------------------------------------------------------

--
-- Table structure for table `grootboekrekening`
--

CREATE TABLE IF NOT EXISTS `grootboekrekening` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `nummer` int(11) NOT NULL,
  `omschrijving` varchar(128) NOT NULL,
  `type` enum('Balans','Resultaat') NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `nummer` (`nummer`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=18 ;

-- --------------------------------------------------------

--
-- Table structure for table `lid`
--

CREATE TABLE IF NOT EXISTS `lid` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `voorletters` varchar(16) NOT NULL,
  `voornaam` varchar(64) NOT NULL,
  `tussenvoegsel` varchar(32) NOT NULL,
  `achternaam` varchar(64) NOT NULL,
  `email` varchar(64) NOT NULL,
  `telefoonnummer` varchar(16) NOT NULL,
  `jaar` year(4) NOT NULL,
  `status` enum('Schildknaap','Jonkheer','Ridder','Externe Orde','Oud-lid') NOT NULL,
  `studie` varchar(32) NOT NULL,
  `studiejaar` year(4) NOT NULL,
  `adres_kamer` varchar(64) DEFAULT NULL,
  `postcode_kamer` varchar(8) DEFAULT NULL,
  `plaats_kamer` varchar(32) DEFAULT NULL,
  `adres_ouders` varchar(64) NOT NULL,
  `postcode_ouders` varchar(8) NOT NULL,
  `plaats_ouders` varchar(32) NOT NULL,
  `geboortedatum` date NOT NULL,
  `geboorteplaats` varchar(32) NOT NULL,
  `is_bestuur` tinyint(1) NOT NULL DEFAULT '0',
  `is_commissie` tinyint(1) NOT NULL DEFAULT '0',
  `ideele_stand` decimal(7,2) NOT NULL DEFAULT '100.00',
  `spaarplan` tinyint(1) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB  DEFAULT CHARSET=latin1 AUTO_INCREMENT=26 ;

-- --------------------------------------------------------

--
-- Table structure for table `raad_der_nobelen`
--

CREATE TABLE IF NOT EXISTS `raad_der_nobelen` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `jaar` year(4) NOT NULL,
  `lid_hertog` int(11) NOT NULL,
  `lid_markies` int(11) NOT NULL,
  `lid_paltsgraaf` int(11) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `lid_hertog` (`lid_hertog`),
  KEY `lid_markies` (`lid_markies`),
  KEY `lid_paltsgraaf` (`lid_paltsgraaf`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 AUTO_INCREMENT=1 ;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `crediteur`
--
ALTER TABLE `crediteur`
  ADD CONSTRAINT `crediteur_ibfk_1` FOREIGN KEY (`lid_id`) REFERENCES `lid` (`id`) ON DELETE SET NULL ON UPDATE CASCADE;

--
-- Constraints for table `debiteur`
--
ALTER TABLE `debiteur`
  ADD CONSTRAINT `debiteur_ibfk_1` FOREIGN KEY (`lid_id`) REFERENCES `lid` (`id`) ON DELETE SET NULL ON UPDATE CASCADE;

--
-- Constraints for table `gb_contributie`
--
ALTER TABLE `gb_contributie`
  ADD CONSTRAINT `gb_contributie_ibfk_1` FOREIGN KEY (`debiteurnummer`) REFERENCES `debiteur` (`nummer`) ON UPDATE CASCADE;

--
-- Constraints for table `gb_crediteur`
--
ALTER TABLE `gb_crediteur`
  ADD CONSTRAINT `gb_crediteur_ibfk_1` FOREIGN KEY (`grootboekrekeningnummer`) REFERENCES `grootboekrekening` (`nummer`) ON UPDATE CASCADE,
  ADD CONSTRAINT `gb_crediteur_ibfk_2` FOREIGN KEY (`crediteurnummer`) REFERENCES `crediteur` (`nummer`) ON UPDATE CASCADE;

--
-- Constraints for table `gb_debiteur`
--
ALTER TABLE `gb_debiteur`
  ADD CONSTRAINT `gb_debiteur_ibfk_1` FOREIGN KEY (`grootboekrekeningnummer`) REFERENCES `grootboekrekening` (`nummer`) ON UPDATE CASCADE,
  ADD CONSTRAINT `gb_debiteur_ibfk_2` FOREIGN KEY (`debiteurnummer`) REFERENCES `debiteur` (`nummer`) ON UPDATE CASCADE;

--
-- Constraints for table `raad_der_nobelen`
--
ALTER TABLE `raad_der_nobelen`
  ADD CONSTRAINT `raad_der_nobelen_ibfk_1` FOREIGN KEY (`lid_hertog`) REFERENCES `lid` (`id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `raad_der_nobelen_ibfk_2` FOREIGN KEY (`lid_markies`) REFERENCES `lid` (`id`) ON UPDATE CASCADE,
  ADD CONSTRAINT `raad_der_nobelen_ibfk_3` FOREIGN KEY (`lid_paltsgraaf`) REFERENCES `lid` (`id`) ON UPDATE CASCADE;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
