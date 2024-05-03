using DomainRule.Models.EMRDB;
using DomainRule.Repositories.EMRDB;
using FluentValidation;
using Lib.Utilities;

namespace DomainRule.Validators.EMRDB
{
    public class Mch_emr_UploadValidator : AbstractValidator<Mch_emr_Upload>
    {
        private readonly IValidator<Mch_emr> _mch_emrValidator;
        private readonly OtherHospitalRepository _otherHospitalRepository;

        public Mch_emr_UploadValidator(
            IValidator<Mch_emr> mch_emrValidator,
            OtherHospitalRepository otherHospitalRepository)
        {
            _mch_emrValidator = mch_emrValidator;
            _otherHospitalRepository = otherHospitalRepository;

            Include(_mch_emrValidator);

            RuleFor(m => m.HospitalId)
                .NotEmpty()
                .MaxLen(m => m.GetPropertyMaxLength(nameof(m.HospitalId)))
                .MustAsync(async (hospitalId, cancellation) =>
                {
                    if (hospitalId.IsNullOrWhiteSpace()) return false;

                    var otherHospital = (await _otherHospitalRepository.Get(new OtherHospitalQuery
                    {
                        HospitalId = hospitalId,
                        Activate = true
                    })).Data?.FirstOrDefault();

                    return otherHospital != null;
                })
                .WithMessage("'{PropertyName}' 不存在！");

            RuleFor(m => m.XML)
                .NotEmpty();

            RuleFor(m => m.File)
                .Must((file) => !file!.Any(f => f.ContentType != "image/jpeg" && f.ContentType != "image/jpg"))
                .WithMessage("'{PropertyName}' ContentType 只能為 \"image/jpeg\" 或 \"image/jpg\"！")
                .Must((file) => !file!.Any(f => !f.FileName.EndsWith(".jpg")))
                .WithMessage("'{PropertyName}' FileName 只能為 \".jpg\") 結尾！")
                .Must((m, file) => m.emr_pic?.TrimEnd() == "Y")
                .WithMessage("'{PropertyName}' 上傳圖檔，欄位 emr_pic 需為 Y！")
                .When(m => m.File != null && m.File.Count > 0,
                ApplyConditionTo.AllValidators)
                .Must((m, file) => m.emr_pic?.TrimEnd() != "Y")
                .WithMessage("'{PropertyName}' 無上傳圖檔，欄位 emr_pic 不能為 Y！")
                .When(m => m.File == null || m.File.Count == 0,
                ApplyConditionTo.CurrentValidator);

        }
    }
}
