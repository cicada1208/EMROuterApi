using DomainRule.Consts;
using Lib.Utilities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainRule.Models.EMRDB
{
    [Lib.Attributes.Table(DBType.SQLSERVER, DBName.EMRDB, "PatientNoMapping")]
    public class PatientNoMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [MaxLength(50)]
        public string PatientId { get; set; } = string.Empty;

        [MaxLength(10)]
        public string HospitalId { get; set; } = string.Empty;

        public int PatientNo { get; set; }

        public bool IsDefault { get; set; }

    }
}
