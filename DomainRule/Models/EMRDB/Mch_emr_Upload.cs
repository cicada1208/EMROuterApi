using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace DomainRule.Models.EMRDB
{
    public class Mch_emr_Upload : Mch_emr
    {
        /// <summary>
        /// 醫療機構代碼
        /// </summary>
        [MaxLength(10)]
        public string HospitalId { get; set; } = string.Empty;

        public string XML { get; set; } = string.Empty;

        public IFormFileCollection? File { get; set; }
    }
}
