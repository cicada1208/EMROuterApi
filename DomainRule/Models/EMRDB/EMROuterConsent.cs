using DomainRule.Consts;
using Lib.Utilities;
using System.ComponentModel.DataAnnotations;

namespace DomainRule.Models.EMRDB
{
    [Lib.Attributes.Table(DBType.SQLSERVER, DBName.EMRDB, "EMROuterConsent")]
    public class EMROuterConsent
    {
        [Key]
        [Required]
        [MaxLength(10)]
        public string PatId { get; set; } = string.Empty;

        [Key]
        [Required]
        [MaxLength(10)]
        public string OrgId { get; set; } = string.Empty;

        [Required]
        public bool Activate { get; set; } = true;

        [Required]
        [MaxLength(20)]
        public string ModifyUser { get; set; } = string.Empty;

        public DateTime ModifyDateTime { get; set; } = DateTime.Now;
    }
}
