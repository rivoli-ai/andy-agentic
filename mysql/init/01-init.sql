-- This script will use the environment variables passed to MySQL container
-- MYSQL_DATABASE, MYSQL_USER, MYSQL_PASSWORD are automatically available

-- Create database if it doesn't exist (using env variable)
CREATE DATABASE IF NOT EXISTS `${MYSQL_DATABASE}`;

-- Create user if it doesn't exist (using env variables)
CREATE USER IF NOT EXISTS '${MYSQL_USER}'@'%' IDENTIFIED BY '${MYSQL_PASSWORD}';
CREATE USER IF NOT EXISTS '${MYSQL_USER}'@'localhost' IDENTIFIED BY '${MYSQL_PASSWORD}';

-- Grant all privileges to the user on the database
GRANT ALL PRIVILEGES ON `${MYSQL_DATABASE}`.* TO '${MYSQL_USER}'@'%';
GRANT ALL PRIVILEGES ON `${MYSQL_DATABASE}`.* TO '${MYSQL_USER}'@'localhost';

-- Grant additional privileges that might be needed
GRANT CREATE, ALTER, DROP, INSERT, UPDATE, DELETE, SELECT, REFERENCES, RELOAD on *.* TO '${MYSQL_USER}'@'%';
GRANT CREATE, ALTER, DROP, INSERT, UPDATE, DELETE, SELECT, REFERENCES, RELOAD on *.* TO '${MYSQL_USER}'@'localhost';

-- Flush privileges to apply changes
FLUSH PRIVILEGES;

-- Show created user for verification
SELECT User, Host FROM mysql.user WHERE User = '${MYSQL_USER}';