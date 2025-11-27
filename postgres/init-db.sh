#!/bin/bash
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" <<-EOSQL
    CREATE DATABASE keycloakdb;
    GRANT ALL PRIVILEGES ON DATABASE keycloakdb TO admin;
EOSQL
