using DomainRule.Models.EMRDB;
using FluentValidation;
using Lib.Utilities;

namespace DomainRule.Validators.EMRDB
{
    public class Mch_emrValidator : AbstractValidator<Mch_emr>
    {
        public Mch_emrValidator()
        {
            RuleFor(m => m.emr_cre_dt)
                .NotEmpty();

            RuleFor(m => m.emr_cre_time)
                .NotNull();

            RuleFor(m => m.emr_ro_date_v)
                .NotNull();

            RuleFor(m => m.emr_ro_seq_v)
                .NotNull();

            RuleFor(m => m.emr_type_h)
                .NotNull()
                .MaxLen(m => m.GetPropertyMaxLength(nameof(m.emr_type_h)));

            RuleFor(m => m.emr_type)
                .NotEmpty()
                .MaxLen(m => m.GetPropertyMaxLength(nameof(m.emr_type)));

            RuleFor(m => m.emr_chk_no)
                .NotEmpty()
                .MaxLen(m => m.GetPropertyMaxLength(nameof(m.emr_chk_no)));

            RuleFor(m => m.emr_cre_dt_v)
                .NotNull();

            RuleFor(m => m.emr_cre_time_v)
                .NotNull();

            RuleFor(m => m.emr_type_c)
                .NotEmpty()
                .MaxLen(m => m.GetPropertyMaxLength(nameof(m.emr_type_c)));

            RuleFor(m => m.emr_sts)
                .NotNull()
                .MaxLen(m => m.GetPropertyMaxLength(nameof(m.emr_sts)));

            RuleFor(m => m.emr_event)
                .NotNull()
                .MaxLen(m => m.GetPropertyMaxLength(nameof(m.emr_event)));

            RuleFor(m => m.emr_pat_no)
                .NotNull();

            RuleFor(m => m.emr_oei)
                .MaxLen(m => m.GetPropertyMaxLength(nameof(m.emr_oei)));

            RuleFor(m => m.emr_rp_mark)
                .MaxLen(m => m.GetPropertyMaxLength(nameof(m.emr_rp_mark)));

            RuleFor(m => m.emr_itm)
                .MaxLen(m => m.GetPropertyMaxLength(nameof(m.emr_itm)));

            RuleFor(m => m.emr_dept_no)
                .MaxLen(m => m.GetPropertyMaxLength(nameof(m.emr_dept_no)));

            RuleFor(m => m.emr_dept)
                .MaxLen(m => m.GetPropertyMaxLength(nameof(m.emr_dept)));

            RuleFor(m => m.emr_dr_no)
                .MaxLen(m => m.GetPropertyMaxLength(nameof(m.emr_dr_no)));

            RuleFor(m => m.emr_dr)
                .MaxLen(m => m.GetPropertyMaxLength(nameof(m.emr_dr)));

            RuleFor(m => m.emr_pat_idno)
                .MaxLen(m => m.GetPropertyMaxLength(nameof(m.emr_pat_idno)));

            RuleFor(m => m.emr_pat_name)
                .MaxLen(m => m.GetPropertyMaxLength(nameof(m.emr_pat_name)));

            RuleFor(m => m.emr_pat_sex)
                .MaxLen(m => m.GetPropertyMaxLength(nameof(m.emr_pat_sex)));

            RuleFor(m => m.emr_birth_dt)
                .Matches(@"^\d{3}/\d{2}/\d{2}$")
                .When(m => !m.emr_birth_dt.IsNullOrWhiteSpace(),
                ApplyConditionTo.CurrentValidator)
                .MaxLen(m => m.GetPropertyMaxLength(nameof(m.emr_birth_dt)));

            RuleFor(m => m.emr_cre_id)
                .MaxLen(m => m.GetPropertyMaxLength(nameof(m.emr_cre_id)));

            RuleFor(m => m.emr_fil)
                .MaxLen(m => m.GetPropertyMaxLength(nameof(m.emr_fil)));

            RuleFor(m => m.emr_pic)
                .NotNull()
                .MaxLen(m => m.GetPropertyMaxLength(nameof(m.emr_pic)));

        }
    }
}
