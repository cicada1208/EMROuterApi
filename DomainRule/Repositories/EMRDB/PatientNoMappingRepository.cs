using DomainRule.Models.EMRDB;
using FluentValidation;
using Lib.Consts;
using Lib.Models;
using Lib.Utilities;

namespace DomainRule.Repositories.EMRDB
{
    public class PatientNoMappingRepository(
        UtilLocator utils,
        IValidator<PatientNoMapping> patientNoMappingValidator) : BaseRepository<PatientNoMapping>
    {
        public async Task<ApiResult<PatientNoMapping>> Write(PatientNoMapping param)
        {
            var validationApiResult = (await patientNoMappingValidator.ValidateAsync(param)).ToApiResult<PatientNoMapping>();
            if (validationApiResult != null) return validationApiResult;

            var patientNoMapping = (await DBUtil.QueryAsync<PatientNoMapping>(new PatientNoMappingQuery
            {
                PatientId = param.PatientId,
                HospitalId = param.HospitalId,
                PatientNo = param.PatientNo
            })).FirstOrDefault();

            if (patientNoMapping == null)
            {
                if (!param.IsDefault)
                    return await Insert(param);
                else
                {
                    List<SqlCmd> sqlCmds = [];
                    sqlCmds.Add(utils.SqlBuild.Insert(typeof(PatientNoMapping), param));

                    SqlCmd sqlCmd = new() { Param = param };
                    sqlCmd.Builder.Append(@"
                    update PatientNoMapping set IsDefault = 0
                    where PatientId = @PatientId and HospitalId = @HospitalId and PatientNo <> @PatientNo");
                    sqlCmds.Add(sqlCmd);

                    int rowsAffected = await DBUtil.ExecuteAsync(sqlCmds);
                    return new ApiResult<PatientNoMapping>(rowsAffected, msgType: ApiMsgType.INSERT);
                }
            }
            else if (param.IsDefault)
            {
                param.Id = patientNoMapping.Id;

                List<SqlCmd> sqlCmds = [];
                sqlCmds.Add(utils.SqlBuild.Patch(typeof(PatientNoMapping), param, [nameof(PatientNoMapping.IsDefault)]));

                SqlCmd sqlCmd = new() { Param = param };
                sqlCmd.Builder.Append(@"
                update PatientNoMapping set IsDefault = 0
                where PatientId = @PatientId and HospitalId = @HospitalId and PatientNo <> @PatientNo");
                sqlCmds.Add(sqlCmd);

                int rowsAffected = await DBUtil.ExecuteAsync(sqlCmds);
                return new ApiResult<PatientNoMapping>(rowsAffected, msgType: ApiMsgType.UPDATE);
            }
            else
                return new ApiResult<PatientNoMapping>(false, msg: MsgConst.DataDuplication, msgType: ApiMsgType.INSERT);
        }

    }
}
