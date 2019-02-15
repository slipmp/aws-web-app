namespace Forro.Domain
{
    public interface IForroAppConfig
    {
        string AWSForroBucketName { get; set; }
        string AWSRegionEndpoint { get; set; }
        string ForroLevelSNSTopicArn { get; set; }
    }
    public class ForroAppConfig : IForroAppConfig
    {
        public string AWSForroBucketName { get; set; }
        public string AWSRegionEndpoint { get; set; }
        public string ForroLevelSNSTopicArn { get; set; }
    }
}
