using DomainRule.Models.EMRDB;
using DomainRule.Repositories.EMRDB;
using FluentValidation;
using Lib.Utilities;

namespace DomainRule.Validators.EMRDB
{
    public class EMROuterValidator : AbstractValidator<EMROuter>
    {
        private readonly OtherHospitalRepository _otherHospitalRepository;

        public EMROuterValidator(OtherHospitalRepository otherHospitalRepository)
        {
            _otherHospitalRepository = otherHospitalRepository;

            RuleFor(m => m.CreateDateTime)
                .NotEmpty();

            RuleFor(m => m.PatId)
                .NotEmpty()
                .MaxLen(m => m.GetPropertyMaxLength(nameof(m.PatId)));

            RuleFor(m => m.OrgId)
                .NotEmpty()
                .MaxLen(m => m.GetPropertyMaxLength(nameof(m.OrgId)))
                .MustAsync(async (orgId, cancellation) =>
                {
                    if (orgId.IsNullOrWhiteSpace()) return false;

                    var otherHospital = (await _otherHospitalRepository.Get(new OtherHospitalQuery
                    {
                        HospitalId = orgId,
                        Activate = true
                    })).Data?.FirstOrDefault();

                    return otherHospital != null;
                })
                .WithMessage("'{PropertyName}' 不存在！");

            RuleFor(m => m.RecType)
                .NotEmpty()
                .MaxLen(m => m.GetPropertyMaxLength(nameof(m.RecType)));

            RuleFor(m => m.RecId)
                .NotEmpty()
                .MaxLen(m => m.GetPropertyMaxLength(nameof(m.RecId)));

            RuleFor(m => m.RecTypeText)
                .NotEmpty()
                .MaxLenUnicode(m => m.GetPropertyMaxLength(nameof(m.RecTypeText)));

            RuleFor(m => m.PatMedicalDate)
                .NotEmpty()
                .Matches(@"^\d{4}-\d{2}-\d{2}$")
                .MaxLen(m => m.GetPropertyMaxLength(nameof(m.PatMedicalDate)));

            RuleFor(m => m.PatName)
                .NotEmpty()
                .MaxLenUnicode(m => m.GetPropertyMaxLength(nameof(m.PatName)));

            RuleFor(m => m.DocCertId)
                .MaxLenUnicode(m => m.GetPropertyMaxLength(nameof(m.DocCertId)));

            RuleFor(m => m.DocName)
                .NotEmpty()
                .MaxLenUnicode(m => m.GetPropertyMaxLength(nameof(m.DocName)));

            RuleFor(m => m.Dept)
                .NotEmpty()
                .MaxLenUnicode(m => m.GetPropertyMaxLength(nameof(m.Dept)));

            RuleFor(m => m.NhiType)
                .NotEmpty()
                .MaxLenUnicode(m => m.GetPropertyMaxLength(nameof(m.NhiType)));

            RuleFor(m => m.RecState)
                .MaxLen(m => m.GetPropertyMaxLength(nameof(m.RecState)));

            RuleFor(m => m.UploadDateTime)
                .NotEmpty();

            RuleFor(m => m.S3Path)
                .MaxLen(m => m.GetPropertyMaxLength(nameof(m.S3Path)));
        }
    }
}
