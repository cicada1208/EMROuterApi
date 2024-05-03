namespace DomainRule.Models.EMRDB
{
    public class EMROuterConsentQuery
    {
        public string PatId { get; set; } = string.Empty;

        public string OrgId { get; set; } = string.Empty;

        public bool? Activate { get; set; }

        public string ModifyUser { get; set; } = string.Empty;

        public DateTime? ModifyDateTime { get; set; }
    }
}
