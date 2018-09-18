./fake.cmd run
docker-compose -f docker-compose.integration-tests.yml -p tests build
docker-compose -f docker-compose.integration-tests.yml -p tests run tests