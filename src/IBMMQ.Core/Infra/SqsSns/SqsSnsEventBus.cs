using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using IBMMQ.Core.Infra.Abstractions;
using System.Text.Json;

namespace IBMMQ.Core.Infra.SqsSns
{
    public class SnsSqsEventBus : EventBus
    {

        private readonly AwsOptions _options;
        private readonly IAmazonSQS _sqsClient;
        private readonly IAmazonSimpleNotificationService _snsClient;
      

        public SnsSqsEventBus(AwsOptions options, IServiceProvider? provider = null) 
            : base(provider)
        {
            _options = options;
            _sqsClient = new AmazonSQSClient(_options.AwsAccessKeyId, _options.AwsSecretAccessKey, _options.Region);
            _snsClient = new AmazonSimpleNotificationServiceClient(_options.AwsAccessKeyId, _options.AwsSecretAccessKey, _options.Region);
        }

        public override async Task Listen<TEvent>()
        {
            var queueUrl = await GetQueueUrlAsync(_options.QueueName);
            var receiveMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                WaitTimeSeconds = 20,
                MaxNumberOfMessages = 10
            };

            while (true)
            {
                var response = await _sqsClient.ReceiveMessageAsync(receiveMessageRequest);
                foreach (var message in response.Messages)
                {
                    var @event = JsonSerializer.Deserialize<TEvent>(message.Body);

                    var payload = JsonSerializer.Serialize(@event);
                    if (await ProcessEvent(typeof(TEvent).Name, payload, _provider))
                        await _sqsClient.DeleteMessageAsync(queueUrl, message.ReceiptHandle);
                }
            }
        }

        public override async void Publish(Event @event)
        {
            var message = new PublishRequest
            {
                TopicArn = _options.TopicArn,
                Message = @event.Payload
            };
            await _snsClient.PublishAsync(message);
        }

        public override async Task<string> PublishAsync(Event @event)
        {
            var message = new PublishRequest
            {
                TopicArn = _options.TopicArn,
                Message = @event.Payload
            };
            var response = await _snsClient.PublishAsync(message);
            return response.MessageId;
        }

        protected override void Dispose(bool disposing)
        {
            _evSubscriptionManager.Clear();
            _sqsClient?.Dispose();
            _snsClient?.Dispose();
            base.Dispose(disposing);
        }

        private async Task<string> GetQueueUrlAsync(string queueName)
        {
            var request = new GetQueueUrlRequest
            {
                QueueName = queueName
            };

            var response = await _sqsClient.GetQueueUrlAsync(request);
            return response.QueueUrl;
        }
    }
}



