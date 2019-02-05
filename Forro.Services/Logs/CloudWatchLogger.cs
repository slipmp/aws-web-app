//using Amazon.CloudWatch;
using System;
using System.Collections.Generic;
using System.Text;

namespace Forro.Services
{
    public class CloudWatchLogger : ILoggerManager
    {
        private readonly Amazon.RegionEndpoint _endpoint;

        public CloudWatchLogger(Amazon.RegionEndpoint endpoint)
        {
            _endpoint = endpoint;
        }

        public void LogError(string message)
        {
            System.Diagnostics.Debug.WriteLine("Marcos: " + message);
        }

        public void LogInfo(string message)
        {
            //using (var cw = new AmazonCloudWatchClient(_endpoint))
            //{
            //    cw.PutMetricData(new PutMetricDataRequest
            //    {
            //        MetricData = new List<MetricDatum>{new MetricDatum
            //    {
            //        MetricName = "PagesVisited",
            //        Dimensions = new List<Dimension>{dimension},
            //        Unit = "None",
            //        Value = 1.0
            //    }},
            //                Namespace = "SITE/TRAFFIC"
            //    });
            //}
        }
    }
}
