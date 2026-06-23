-- MySQL dump 10.13  Distrib 8.0.35, for Win64 (x86_64)
--
-- Host: localhost    Database: andy_db
-- ------------------------------------------------------
-- Server version	5.5.5-10.4.28-MariaDB

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `agentmcpservers`
--

DROP TABLE IF EXISTS `agentmcpservers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `agentmcpservers` (
  `Id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `Name` varchar(100) NOT NULL,
  `IsActive` tinyint(1) NOT NULL,
  `Capabilities` longtext DEFAULT NULL,
  `AgentId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_AgentMcpServers_AgentId` (`AgentId`),
  CONSTRAINT `FK_AgentMcpServers_Agents_AgentId` FOREIGN KEY (`AgentId`) REFERENCES `agents` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `agents`
--

DROP TABLE IF EXISTS `agents`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `agents` (
  `Id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `Name` varchar(100) NOT NULL,
  `Description` varchar(500) NOT NULL,
  `Type` varchar(50) NOT NULL,
  `IsActive` tinyint(1) NOT NULL,
  `ExecutionCount` int(11) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) NOT NULL,
  `LlmConfigId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Agents_Name` (`Name`),
  KEY `IX_Agents_LlmConfigId` (`LlmConfigId`),
  CONSTRAINT `FK_Agents_LlmConfigs_LlmConfigId` FOREIGN KEY (`LlmConfigId`) REFERENCES `llmconfigs` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `agenttags`
--

DROP TABLE IF EXISTS `agenttags`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `agenttags` (
  `Id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `AgentId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `TagId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_AgentTags_AgentId` (`AgentId`),
  KEY `IX_AgentTags_TagId` (`TagId`),
  CONSTRAINT `FK_AgentTags_Agents_AgentId` FOREIGN KEY (`AgentId`) REFERENCES `agents` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_AgentTags_Tags_TagId` FOREIGN KEY (`TagId`) REFERENCES `tags` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `agenttools`
--

DROP TABLE IF EXISTS `agenttools`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `agenttools` (
  `Id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `Name` varchar(100) NOT NULL,
  `Type` varchar(50) NOT NULL,
  `IsActive` tinyint(1) NOT NULL,
  `Parameters` longtext DEFAULT NULL,
  `Category` varchar(50) DEFAULT NULL,
  `Configuration` longtext DEFAULT NULL,
  `Authentication` longtext DEFAULT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `AgentId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `ToolId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_AgentTools_AgentId` (`AgentId`),
  KEY `IX_AgentTools_ToolId` (`ToolId`),
  CONSTRAINT `FK_AgentTools_Agents_AgentId` FOREIGN KEY (`AgentId`) REFERENCES `agents` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_AgentTools_Tools_ToolId` FOREIGN KEY (`ToolId`) REFERENCES `tools` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `chatmessages`
--

DROP TABLE IF EXISTS `chatmessages`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `chatmessages` (
  `Id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `Content` longtext NOT NULL,
  `Role` varchar(50) NOT NULL,
  `AgentName` varchar(100) DEFAULT NULL,
  `Timestamp` datetime(6) NOT NULL,
  `SessionId` longtext DEFAULT NULL,
  `AgentId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `TokenCount` int(11) DEFAULT NULL,
  `IsToolExecution` tinyint(1) NOT NULL,
  `ToolName` varchar(100) DEFAULT NULL,
  `ToolResult` longtext DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ChatMessages_AgentId` (`AgentId`),
  CONSTRAINT `FK_ChatMessages_Agents_AgentId` FOREIGN KEY (`AgentId`) REFERENCES `agents` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `llmconfigs`
--

DROP TABLE IF EXISTS `llmconfigs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `llmconfigs` (
  `Id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `Name` varchar(100) NOT NULL,
  `BaseUrl` varchar(500) NOT NULL,
  `ApiKey` varchar(500) NOT NULL,
  `Model` varchar(100) NOT NULL,
  `Provider` varchar(50) NOT NULL,
  `IsActive` tinyint(1) NOT NULL,
  `MaxTokens` int(11) DEFAULT NULL,
  `Temperature` double DEFAULT NULL,
  `TopP` double DEFAULT NULL,
  `FrequencyPenalty` double DEFAULT NULL,
  `PresencePenalty` double DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `mcpservers`
--

DROP TABLE IF EXISTS `mcpservers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mcpservers` (
  `Id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `Name` varchar(100) NOT NULL,
  `Description` varchar(500) NOT NULL,
  `Endpoint` varchar(500) NOT NULL,
  `Protocol` varchar(50) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL,
  `Capabilities` longtext DEFAULT NULL,
  `Configuration` longtext DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_McpServers_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `prompts`
--

DROP TABLE IF EXISTS `prompts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `prompts` (
  `Id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `Content` longtext NOT NULL,
  `IsActive` tinyint(1) NOT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) NOT NULL,
  `AgentId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Prompts_AgentId` (`AgentId`),
  CONSTRAINT `FK_Prompts_Agents_AgentId` FOREIGN KEY (`AgentId`) REFERENCES `agents` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `promptvariables`
--

DROP TABLE IF EXISTS `promptvariables`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `promptvariables` (
  `Id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `Name` varchar(100) NOT NULL,
  `Type` varchar(50) NOT NULL,
  `Required` tinyint(1) NOT NULL,
  `DefaultValue` varchar(500) DEFAULT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `PromptId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_PromptVariables_PromptId` (`PromptId`),
  CONSTRAINT `FK_PromptVariables_Prompts_PromptId` FOREIGN KEY (`PromptId`) REFERENCES `prompts` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `tags`
--

DROP TABLE IF EXISTS `tags`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tags` (
  `Id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `Name` varchar(50) NOT NULL,
  `Description` varchar(200) DEFAULT NULL,
  `Color` longtext DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Tags_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `toolexecutionlogs`
--

DROP TABLE IF EXISTS `toolexecutionlogs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `toolexecutionlogs` (
  `Id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `ToolId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `ToolName` varchar(100) NOT NULL,
  `AgentId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci DEFAULT NULL,
  `SessionId` varchar(100) DEFAULT NULL,
  `Parameters` longtext DEFAULT NULL,
  `Result` longtext DEFAULT NULL,
  `Success` tinyint(1) NOT NULL,
  `ErrorMessage` varchar(1000) DEFAULT NULL,
  `ExecutedAt` datetime(6) NOT NULL,
  `ExecutionTime` double NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ToolExecutionLogs_AgentId` (`AgentId`),
  CONSTRAINT `FK_ToolExecutionLogs_Agents_AgentId` FOREIGN KEY (`AgentId`) REFERENCES `agents` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `tools`
--

DROP TABLE IF EXISTS `tools`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tools` (
  `Id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `Name` varchar(100) NOT NULL,
  `Description` varchar(500) NOT NULL,
  `Type` varchar(50) NOT NULL,
  `Category` varchar(50) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL,
  `Configuration` longtext DEFAULT NULL,
  `Authentication` longtext DEFAULT NULL,
  `Parameters` longtext DEFAULT NULL,
  `Headers` longtext DEFAULT NULL,
  `CreatedAt` datetime(6) NOT NULL,
  `UpdatedAt` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Tools_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping events for database 'andy_db'
--

--
-- Dumping routines for database 'andy_db'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-08-29 21:53:43
