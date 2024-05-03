namespace DomainRule.Models.EMRDB
{
    public class OtherHospitalQuery
    {
        public string HospitalId { get; set; } = string.Empty;

        public string HospitalName { get; set; } = string.Empty;

        public string HospitalNameAbbr { get; set; } = string.Empty;

        public bool? Activate { get; set; }

        public string ModifyUser { get; set; } = string.Empty;

        public DateTime? ModifyDateTime { get; set; }
    }
}
