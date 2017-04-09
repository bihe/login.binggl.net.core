-- --------------------------------------------------------
-- Host:                         raspi3.local.binggl.net
-- Server Version:               10.0.29-MariaDB-0+deb8u1 - (Raspbian)
-- Server Betriebssystem:        debian-linux-gnueabihf
-- HeidiSQL Version:             9.4.0.5168
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;

-- Exportiere Struktur von Tabelle login.login
DROP TABLE IF EXISTS `login`;
CREATE TABLE IF NOT EXISTS `login` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Created` datetime(6) NOT NULL,
  `Modified` datetime(6) NOT NULL,
  `Type` int(11) NOT NULL,
  `UserDisplayName` varchar(128) COLLATE utf8_unicode_ci NOT NULL,
  `UserName` varchar(128) COLLATE utf8_unicode_ci NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=19 DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- Daten Export vom Benutzer nicht ausgewählt
-- Exportiere Struktur von Tabelle login.user
DROP TABLE IF EXISTS `user`;
CREATE TABLE IF NOT EXISTS `user` (
  `Email` varchar(128) COLLATE utf8_unicode_ci NOT NULL,
  `Created` datetime(6) NOT NULL,
  `DisplayName` varchar(255) COLLATE utf8_unicode_ci NOT NULL,
  `Modified` datetime(6) NOT NULL,
  `Name` varchar(128) COLLATE utf8_unicode_ci NOT NULL,
  `Timestamp` longblob,
  PRIMARY KEY (`Email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- Daten Export vom Benutzer nicht ausgewählt
-- Exportiere Struktur von Tabelle login.usersite
DROP TABLE IF EXISTS `usersite`;
CREATE TABLE IF NOT EXISTS `usersite` (
  `Name` varchar(128) COLLATE utf8_unicode_ci NOT NULL,
  `Created` datetime(6) NOT NULL,
  `Modified` datetime(6) NOT NULL,
  `PermissionList` longtext COLLATE utf8_unicode_ci NOT NULL,
  `Timestamp` longblob,
  `Url` varchar(255) COLLATE utf8_unicode_ci NOT NULL,
  `UserEmail` varchar(255) COLLATE utf8_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`Name`),
  KEY `IX_usersite_UserEmail` (`UserEmail`),
  CONSTRAINT `FK_usersite_user_UserEmail` FOREIGN KEY (`UserEmail`) REFERENCES `user` (`Email`) ON DELETE NO ACTION
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;

-- Daten Export vom Benutzer nicht ausgewählt
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
