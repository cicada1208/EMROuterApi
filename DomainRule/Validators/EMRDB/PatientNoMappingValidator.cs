using DomainRule.Models.EMRDB;
using DomainRule.Repositories.EMRDB;
using FluentValidation;
using Lib.Utilities;

namespace DomainRule.Validators.EMRDB
{
    public class PatientNoMappingValidator : AbstractValidator<PatientNoMapping>
    {
        private readonly OtherHospitalRepository _otherHospitalRepository;

        public PatientNoMappingValidator(OtherHospitalRepository otherHospitalRepository)
        {
            _otherHospitalRepository = otherHospitalRepository;

            RuleFor(m => m.PatientId)
                .NotEmpty()
                .MaxLenUnicode(m => m.GetPropertyMaxLength(nameof(m.PatientId)));

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

            RuleFor(m => m.PatientNo)
                .NotNull();

            RuleFor(m => m.IsDefault)
                .NotNull();
        }
    }
}
