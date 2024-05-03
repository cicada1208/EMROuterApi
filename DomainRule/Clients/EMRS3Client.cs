using Amazon.S3;

namespace DomainRule.Clients
{
    public class EMRS3Client : AmazonS3Client
    {
        public EMRS3Client() : base(
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
