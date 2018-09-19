docker-compose -f docker-compose.integration-tests.yml -p tests build
docker-compose --compatibility -f docker-compose.integration-tests.yml -p tests run tests
docker-compose -f docker-compose.integration-tests.yml -p tests down