-- Create virtual test databases for all microservices
CREATE DATABASE IF NOT EXISTS customer_booking_db;
CREATE DATABASE IF NOT EXISTS job_maintenance_db;
CREATE DATABASE IF NOT EXISTS inventory_db;
CREATE DATABASE IF NOT EXISTS billing_db;
CREATE DATABASE IF NOT EXISTS notification_db;

-- Grant all permissions to vsc_app user
GRANT ALL PRIVILEGES ON customer_booking_db.* TO 'vsc_app'@'%';
GRANT ALL PRIVILEGES ON job_maintenance_db.* TO 'vsc_app'@'%';
GRANT ALL PRIVILEGES ON inventory_db.* TO 'vsc_app'@'%';
GRANT ALL PRIVILEGES ON billing_db.* TO 'vsc_app'@'%';
GRANT ALL PRIVILEGES ON notification_db.* TO 'vsc_app'@'%';

FLUSH PRIVILEGES;
