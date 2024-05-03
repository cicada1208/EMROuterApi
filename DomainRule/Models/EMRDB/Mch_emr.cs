using DomainRule.Consts;
using Lib.Utilities;
using System.ComponentModel.DataAnnotations;

namespace DomainRule.Models.EMRDB
{
    /// <summary>
    /// Á`°|
    /// </summary>
    [Lib.Attributes.Table(DBType.SQLSERVER, DBName.EMRDB, "mch_emr")]
    public class Mch_emr
    {
        [Key]
        public int emr_cre_dt { get; set; }

        [Key]
        public int emr_cre_time { get; set; }

        [Key]
        public int emr_ro_date_v { get; set; }

        [Key]
        public int emr_ro_seq_v { get; set; }

        [Key]
        [MaxLength(1)]
        public string emr_type_h { get; set; } = string.Empty;

        [Key]
        [MaxLength(3)]
        public string emr_type { get; set; } = string.Empty;

        [Key]
        [MaxLength(20)]
        public string emr_chk_no { get; set; } = string.Empty;

        public int emr_cre_dt_v { get; set; }

        public int emr_cre_time_v { get; set; }

        [MaxLength(16)]
        public string? emr_type_c { get; set; }

        [MaxLength(1)]
        public string? emr_sts { get; set; }

        [MaxLength(1)]
        public string? emr_event { get; set; }

        public int emr_pat_no { get; set; }

        [MaxLength(6)]
        public string? emr_oei { get; set; }

        public int? emr_odr_date { get; set; }

        public int? emr_rp_time { get; set; }

        [MaxLength(6)]
        public string? emr_rp_mark { get; set; }

        [MaxLength(40)]
        public string? emr_itm { get; set; }

        public int? emr_ipd_dt { get; set; }

        public int? emr_ipd_out_dt { get; set; }

        [MaxLength(4)]
        public string? emr_dept_no { get; set; }

        [MaxLength(16)]
        public string? emr_dept { get; set; }

        [MaxLength(5)]
        public string? emr_dr_no { get; set; }

        [MaxLength(16)]
        public string? emr_dr { get; set; }

        [MaxLength(10)]
        public string? emr_pat_idno { get; set; }

        [MaxLength(40)]
        public string? emr_pat_name { get; set; }

        [MaxLength(2)]
        public string? emr_pat_sex { get; set; }

        [MaxLength(10)]
        public string? emr_birth_dt { get; set; }

        [MaxLength(5)]
        public string? emr_cre_id { get; set; }

        [MaxLength(80)]
        public string? emr_fil { get; set; }

        [MaxLength(80)]
        public string? emr_pic { get; set; }

    }
}
