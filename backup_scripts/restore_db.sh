#!/bin/bash

# Check if backup file is provided
if [ "$#" -ne 1 ]; then
    echo "Usage: $0 backup_file.sql.gz"
    exit 1
fi

BACKUP_FILE=$1
DB_NAME="fuddyduddy"

# Check if file exists
if [ ! -f "$BACKUP_FILE" ]; then
    echo "Error: Backup file $BACKUP_FILE does not exist"
    exit 1
fi

# Create database if it doesn't exist
echo "Creating database if it doesn't exist..."
mysql -e "CREATE DATABASE IF NOT EXISTS ${DB_NAME};"

# Restore from backup
echo "Starting restore of ${DB_NAME} database from ${BACKUP_FILE}..."
gunzip < ${BACKUP_FILE} | mysql ${DB_NAME}

echo "Restore completed successfully!" 