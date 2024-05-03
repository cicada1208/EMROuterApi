namespace DomainRule.Models.EMRDB
{
    public class EMROuterQuery
    {
        public DateTime? CreateDateTime { get; set; }

        public string PatId { get; set; } = string.Empty;

        public string OrgId { get; set; } = string.Empty;

        public string RecType { get; set; } = string.Empty;

        public string RecId { get; set; } = string.Empty;

        public string? RecTypeText { get; set; }

        public string PatMedicalDate { get; set; } = string.Empty;

        public string? PatName { get; set; }

        public string? DocCertId { get; set; }

        public string? DocName { get; set; }

        public string? Dept { get; set; }

        public string? NhiType { get; set; }

        public string RecState { get; set; } = string.Empty;

        public DateTime? UploadDateTime { get; set; }

        public string? S3Path { get; set; }

    }
}
