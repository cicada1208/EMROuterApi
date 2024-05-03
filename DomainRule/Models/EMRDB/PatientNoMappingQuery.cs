namespace DomainRule.Models.EMRDB
{
    public class PatientNoMappingQuery
    {
        public long? Id { get; set; }

        public string PatientId { get; set; } = string.Empty;

        public string HospitalId { get; set; } = string.Empty;

        public int? PatientNo { get; set; }

        public bool? IsDefault { get; set; }
    }
}
