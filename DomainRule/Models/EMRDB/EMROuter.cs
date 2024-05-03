using DomainRule.Consts;
using Lib.Utilities;
using System.ComponentModel.DataAnnotations;

namespace DomainRule.Models.EMRDB
{
    [Lib.Attributes.Table(DBType.SQLSERVER, DBName.EMRDB, "EMROuter")]
    public class EMROuter
    {
        [Key]
        [Required]
        public DateTime CreateDateTime { get; set; }

        [Key]
        [Required]
        [MaxLength(10)]
        public string PatId { get; set; } = string.Empty;

        [Key]
        [Required]
        [MaxLength(10)]
        public string OrgId { get; set; } = string.Empty;

        [Key]
        [Required]
        [MaxLength(3)]
        public string RecType { get; set; } = string.Empty;

        [Key]
        [Required]
        [MaxLength(36)]
        public string RecId { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string? RecTypeText { get; set; }

        [Required]
        [MaxLength(10)]
        [RegularExpression(@"^\d{4}-\d{2}-\d{2}$")]
        public string PatMedicalDate { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string? PatName { get; set; }

        [MaxLength(200)]
        public string? DocCertId { get; set; }

        [Required]
        [MaxLength(100)]
        public string? DocName { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Dept { get; set; }

        [Required]
        [MaxLength(10)]
        public string? NhiType { get; set; }

        [MaxLength(1)]
        public string RecState { get; set; } = string.Empty;

        [Required]
        public DateTime UploadDateTime { get; set; } = DateTime.Now;

        [MaxLength(200)]
        public string? S3Path { get; set; }

    }
}
