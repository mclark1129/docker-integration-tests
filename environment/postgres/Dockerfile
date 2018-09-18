# Creates a docker image that initalizes the database with the current schema
FROM library/postgres:9.5

COPY Scripts/ ./docker-entrypoint-initdb.d

ENV POSTGRES_DB=location_numbers

EXPOSE 5432