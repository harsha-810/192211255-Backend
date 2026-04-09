-- MySQL dump 10.13  Distrib 8.0.45, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: petclinic
-- ------------------------------------------------------
-- Server version	8.0.45

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `__efmigrationshistory`
--

DROP TABLE IF EXISTS `__efmigrationshistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductVersion` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `__efmigrationshistory`
--

LOCK TABLES `__efmigrationshistory` WRITE;
/*!40000 ALTER TABLE `__efmigrationshistory` DISABLE KEYS */;
INSERT INTO `__efmigrationshistory` VALUES ('20260223072100_InitialCreate','9.0.0'),('20260225035650_AddHospitalTimings','9.0.0'),('20260317094420_AddPasswordResetFields','9.0.0');
/*!40000 ALTER TABLE `__efmigrationshistory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `appointments`
--

DROP TABLE IF EXISTS `appointments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `appointments` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `PetId` int NOT NULL,
  `DoctorId` int NOT NULL,
  `Date` datetime(6) NOT NULL,
  `Status` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `RejectionReason` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Symptoms` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `Duration` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `AiCondition` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `AiSeverity` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `PriorityLevel` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Appointments_DoctorId` (`DoctorId`),
  KEY `IX_Appointments_PetId` (`PetId`),
  CONSTRAINT `FK_Appointments_Doctors_DoctorId` FOREIGN KEY (`DoctorId`) REFERENCES `doctors` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_Appointments_Pets_PetId` FOREIGN KEY (`PetId`) REFERENCES `pets` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `appointments`
--

LOCK TABLES `appointments` WRITE;
/*!40000 ALTER TABLE `appointments` DISABLE KEYS */;
INSERT INTO `appointments` VALUES (1,3,11,'2026-02-26 16:30:00.000000','Completed',NULL,'bleeding ','2','Critical Emergency','High',1),(2,3,12,'2026-02-27 10:45:00.000000','Accepted',NULL,'Fever','3','Possible Infection','Medium',2),(3,5,11,'2026-02-27 10:30:00.000000','Completed',NULL,'fever\n','4','Possible Infection','Medium',2),(4,5,12,'2026-02-28 18:12:00.000000','Cancelled',NULL,'weight loss','5','General Checkup Required','Low',3),(5,5,11,'2026-02-28 09:32:00.000000','Rejected','busy','vomiting ','3','Possible Infection','Medium',2),(6,4,12,'2026-02-28 10:42:00.000000','Completed',NULL,'bleeding ','2','Critical Emergency','High',1),(7,4,13,'2026-02-28 10:30:00.000000','Completed',NULL,'Sneezing, Coughing, Vomiting','1-2 days','Gastroenteritis','Medium',2),(8,5,12,'2026-02-28 11:00:00.000000','Completed',NULL,'Loss of appetite, Itching, Sneezing','3-5 days','General Illness','Low',3),(9,6,12,'2026-03-03 08:46:00.000000','Completed',NULL,'Vomiting, Itching','About a week','Gastroenteritis','Medium',2),(10,7,12,'2026-03-05 09:09:00.000000','Completed',NULL,'aiaisks sx8xnsnaoanqbws sbzjizixkznaayaoq0q917292jwbzbzj','1-2 days','General Checkup Required','Low',3),(12,9,12,'2026-03-16 11:30:00.000000','Accepted',NULL,'Vomiting, Coughing, Sneezing','1-2 days','Gastroenteritis','Medium',2),(13,4,11,'2026-03-22 18:00:00.000000','Pending',NULL,'Diarrhea, Sneezing, Vomiting, Coughing','3-5 days','Gastroenteritis','Medium',2),(15,10,21,'2026-03-31 10:06:00.000000','Completed',NULL,'Sneezing, Loss of appetite, Coughing','3-5 days','Upper Respiratory Infection (URI)','Low',3);
/*!40000 ALTER TABLE `appointments` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `doctors`
--

DROP TABLE IF EXISTS `doctors`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `doctors` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Specialization` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `IsActive` tinyint(1) NOT NULL,
  `UserId` int NOT NULL,
  `HospitalId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Doctors_HospitalId` (`HospitalId`),
  KEY `IX_Doctors_UserId` (`UserId`),
  CONSTRAINT `FK_Doctors_Hospitals_HospitalId` FOREIGN KEY (`HospitalId`) REFERENCES `hospitals` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_Doctors_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=22 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `doctors`
--

LOCK TABLES `doctors` WRITE;
/*!40000 ALTER TABLE `doctors` DISABLE KEYS */;
INSERT INTO `doctors` VALUES (11,'Mani','surgeon',1,15,8),(12,'Wajid','general',1,16,9),(13,'Mahesh','surgeon',1,17,8),(14,'Ashok','surgeon',1,19,9),(20,'harsha','surgeon',1,31,14),(21,'akhil','surgeon',1,32,14);
/*!40000 ALTER TABLE `doctors` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `hospitals`
--

DROP TABLE IF EXISTS `hospitals`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `hospitals` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Address` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `AdminId` int NOT NULL,
  `Timings` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Hospitals_AdminId` (`AdminId`),
  CONSTRAINT `FK_Hospitals_Users_AdminId` FOREIGN KEY (`AdminId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `hospitals`
--

LOCK TABLES `hospitals` WRITE;
/*!40000 ALTER TABLE `hospitals` DISABLE KEYS */;
INSERT INTO `hospitals` VALUES (8,'Harsha clinic','NRT',1,'09:00 AM to 10:00 PM'),(9,'Bhavesh clinic','Guntur',1,'09:00 AM to 05:00 PM'),(14,'Karthik ','chennai',1,'09:00 AM - 10:00 PM');
/*!40000 ALTER TABLE `hospitals` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `notifications`
--

DROP TABLE IF EXISTS `notifications`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `notifications` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` int NOT NULL,
  `Message` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Date` datetime(6) NOT NULL,
  `IsRead` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Notifications_UserId` (`UserId`),
  CONSTRAINT `FK_Notifications_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=27 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `notifications`
--

LOCK TABLES `notifications` WRITE;
/*!40000 ALTER TABLE `notifications` DISABLE KEYS */;
INSERT INTO `notifications` VALUES (1,13,'Your appointment for sissu has been accepted.','2026-02-26 10:21:35.048942',1),(3,13,'Your appointment for sissu has been accepted.','2026-02-26 16:16:58.954155',1),(6,13,'A digital prescription has been added for sissu.','2026-02-26 17:40:45.387018',1),(14,20,'Your appointment for pinchus has been accepted.','2026-03-02 03:20:34.034638',0),(15,20,'A digital prescription has been added for pinchus.','2026-03-02 03:29:39.138691',0),(16,23,'Your appointment for Harsha has been accepted.','2026-03-05 03:41:10.708631',1),(17,23,'A digital prescription has been added for Harsha.','2026-03-05 03:41:43.640594',1),(19,26,'Your appointment for Jimmy has been accepted.','2026-03-15 16:55:35.087454',1),(24,18,'Your appointment for Jack has been accepted.','2026-03-30 07:36:43.123259',1),(25,18,'A doctor has recorded a new vaccination for Jack: Abcd. Check medical records for details.','2026-03-30 07:37:43.274732',1),(26,18,'A digital prescription has been added for Jack.','2026-03-30 07:41:15.278136',1);
/*!40000 ALTER TABLE `notifications` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `patients`
--

DROP TABLE IF EXISTS `patients`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `patients` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Phone` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `UserId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Patients_UserId` (`UserId`),
  CONSTRAINT `FK_Patients_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `patients`
--

LOCK TABLES `patients` WRITE;
/*!40000 ALTER TABLE `patients` DISABLE KEYS */;
INSERT INTO `patients` VALUES (1,'Test Patient','1234567890',3),(2,'Karthik Keshava Reddy','9692561234',13),(3,'Harika','9063302884',18),(4,'karthi','8978706037',20),(5,'Test Patient','1234567890',22),(6,'Akhil','6348001515',23),(7,'Patient Test','1234567890',24),(8,'Patient Two','9876543210',25),(9,'Chevuri Venkata Harsha Vardhan ','9063302884',26);
/*!40000 ALTER TABLE `patients` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `pets`
--

DROP TABLE IF EXISTS `pets`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pets` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Species` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Breed` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Age` int NOT NULL,
  `PatientId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Pets_PatientId` (`PatientId`),
  CONSTRAINT `FK_Pets_Patients_PatientId` FOREIGN KEY (`PatientId`) REFERENCES `patients` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pets`
--

LOCK TABLES `pets` WRITE;
/*!40000 ALTER TABLE `pets` DISABLE KEYS */;
INSERT INTO `pets` VALUES (1,'Buddy','Dog','Golden Retriever',3,1),(2,'Tom','cat','pussy',2,1),(3,'sissu','Cat','pussy cat',2,2),(4,'Tom','Cat','pussy cat',2,3),(5,'Bob','Dog','police',3,3),(6,'pinchus','rat','fat',100,4),(7,'Harsha','Dog','Bull dog',21,6),(8,'Buddy','Dog','Golden Retriever',3,7),(9,'Jimmy','Cat','Fluffy',2,9),(10,'Jack','Cat','do',2,3);
/*!40000 ALTER TABLE `pets` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `prescriptions`
--

DROP TABLE IF EXISTS `prescriptions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `prescriptions` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `AppointmentId` int NOT NULL,
  `Diagnosis` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Medicines` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Advice` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Prescriptions_AppointmentId` (`AppointmentId`),
  CONSTRAINT `FK_Prescriptions_Appointments_AppointmentId` FOREIGN KEY (`AppointmentId`) REFERENCES `appointments` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `prescriptions`
--

LOCK TABLES `prescriptions` WRITE;
/*!40000 ALTER TABLE `prescriptions` DISABLE KEYS */;
INSERT INTO `prescriptions` VALUES (1,3,'everything is normal ','dolo','eat healthy food '),(2,1,'serious injury ','pain killer\nseptic','take rest'),(3,7,'focuses on rehydration ','fers2\nguyt3','drink more water'),(4,8,'normal','vitamins and calcium tablets ','eat healthy food '),(5,9,'small infection, medicine should be taken on time','fcu1\ndol321','give healthy food on time'),(6,10,'fteetjstkcjlh','gzjsgm????','ngjqr yqrrwjwrb ????'),(7,6,'wo dfhdjdh','gdydnekduwmei','yshejehksiwkwo'),(9,15,'wsdfghjk','asdfgh','qwerth');
/*!40000 ALTER TABLE `prescriptions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Email` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `PasswordHash` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Role` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ResetOtp` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ResetOtpExpiry` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=33 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES (1,'admin@petclinic.com','admin123','Admin',NULL,NULL),(3,'patient@petclinic.com','patient123','Patient',NULL,NULL),(11,'admin@test.com','password123','Admin',NULL,NULL),(13,'karthik123@gmail.com','Karthik @123','Patient',NULL,NULL),(15,'mani@gmail.com','mani123','Doctor',NULL,NULL),(16,'wajid@gmail.com','wajid123','Doctor',NULL,NULL),(17,'mahesh@gmail.com','mahesh123','Doctor',NULL,NULL),(18,'harika@gmail.com','harika12','Patient',NULL,NULL),(19,'ashok@gmail.com','ashok123','Doctor',NULL,NULL),(20,'keshavakarthik1@gmail.com','Karthik23.','Patient',NULL,NULL),(21,'doctor@test.com','password123','Doctor',NULL,NULL),(22,'testpatient@test.com','password123','Patient',NULL,NULL),(23,'akhil@gmail.com','Akhil','Patient',NULL,NULL),(24,'patient_test@petclinic.com','password123','Patient',NULL,NULL),(25,'patient2@test.com','password123','Patient',NULL,NULL),(26,'harshavardhan.chevuri810@gmail.com','har','Patient',NULL,NULL),(31,'hemanth@gmail.com','hemanth123','Doctor',NULL,NULL),(32,'akhil1@gmail.com','akhil123','Doctor',NULL,NULL);
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `vaccinations`
--

DROP TABLE IF EXISTS `vaccinations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `vaccinations` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `PetId` int NOT NULL,
  `VaccineName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `LastDate` datetime(6) NOT NULL,
  `NextDueDate` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Vaccinations_PetId` (`PetId`),
  CONSTRAINT `FK_Vaccinations_Pets_PetId` FOREIGN KEY (`PetId`) REFERENCES `pets` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `vaccinations`
--

LOCK TABLES `vaccinations` WRITE;
/*!40000 ALTER TABLE `vaccinations` DISABLE KEYS */;
INSERT INTO `vaccinations` VALUES (1,5,'vacci123','2026-03-25 00:00:00.000000','2026-07-25 00:00:00.000000'),(2,5,'vacc12345','2026-03-26 00:00:00.000000','2026-07-26 00:00:00.000000'),(3,10,'Abcd','2026-03-30 00:00:00.000000','2026-07-30 00:00:00.000000');
/*!40000 ALTER TABLE `vaccinations` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-04-09 22:23:34
