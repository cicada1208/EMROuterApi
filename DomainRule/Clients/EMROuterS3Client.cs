using Amazon.S3;

namespace DomainRule.Clients
{
    public class EMROuterS3Client : AmazonS3Client
    {
        public EMROuterS3Client() : base(
            "",
            "",
            new AmazonS3Config()
            {
                ServiceURL = ""
            })
        {
        }

    }
}
