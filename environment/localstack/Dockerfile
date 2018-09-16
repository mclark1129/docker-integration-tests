# Creates an SQS image with the necessary queues already deployed
FROM localstack/localstack:0.8.7

# Override the SQS startup code to provision queues with the image
# This is admittedly a hack, but there are currently no extensibility points 
# in localstack to support this
COPY sqs_starter.py localstack/services/sqs/sqs_starter.py