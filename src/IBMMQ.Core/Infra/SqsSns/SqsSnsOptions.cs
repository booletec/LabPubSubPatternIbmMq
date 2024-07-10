namespace IBMMQ.Core.Infra.SqsSns
{
    public class AwsOptions
    {
        public string AwsAccessKeyId { get; set; }
        public string AwsSecretAccessKey { get; set; }
        public string Region { get; set; }
        public string QueueName { get; set; }
        public string TopicArn { get; set; }
    }
}
