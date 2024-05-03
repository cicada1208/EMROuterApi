using DomainRule.Consts;
using Lib.Utilities;
using System.ComponentModel.DataAnnotations;

namespace DomainRule.Models.EMRDB
{
    [Lib.Attributes.Table(DBType.SQLSERVER, DBName.EMRDB, "OtherHospital")]
    public class OtherHospital
    {
        [Key]
        [Required]
        [MaxLength(10)]
        public string HospitalId { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string HospitalName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string HospitalNameAbbr { get; set; } = string.Empty;

        [Required]
        public bool Activate { get; set; } = true;

        [MaxLength(20)]
        public string ModifyUser { get; set; } = string.Empty;

        public DateTime ModifyDateTime { get; set; } = DateTime.Now;

        [Required]
        public bool Branch { get; set; } = true;
    }
}
