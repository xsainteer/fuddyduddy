#!/bin/bash

# Configuration
DB_NAME="fuddyduddy"
BACKUP_DIR="/var/lib/mysql/backup"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="${BACKUP_DIR}/fuddyduddy_${TIMESTAMP}.sql"

# Create backup directory if it doesn't exist
mkdir -p ${BACKUP_DIR}

# Backup database
echo "Starting backup of ${DB_NAME} database..."
mysqldump -u root -proot \
         --single-transaction \
         --set-gtid-purged=OFF \
         --triggers \
         --routines \
         --events \
         ${DB_NAME} > ${BACKUP_FILE}

# Compress the backup
gzip ${BACKUP_FILE}

echo "Backup completed: ${BACKUP_FILE}.gz"

# Keep only last 5 backups
cd ${BACKUP_DIR}
ls -t *.gz | tail -n +6 | xargs -r rm

echo "Cleanup completed. Only keeping last 5 backups." 