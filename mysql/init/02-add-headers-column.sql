-- Add Headers column to tools table
-- This migration adds support for storing HTTP headers configuration for tools

ALTER TABLE `tools` 
ADD COLUMN `Headers` longtext DEFAULT NULL AFTER `Parameters`;

-- Update existing tools to have empty headers array
UPDATE `tools` 
SET `Headers` = '[]' 
WHERE `Headers` IS NULL;

