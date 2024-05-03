using DomainRule.Models.EMRDB;
using Lib.Models;

namespace DomainRule.Repositories.EMRDB
{
    public class EMROuterConsentRepository : BaseRepository<EMROuterConsent>
    {
        /// <summary>
        /// 慶昇一開始上傳的病歷還未簽署同意書，EMROuter 有簽同意、不同意、未簽的病人病歷，
        /// 故需此表格，但之後上傳的病歷其病人即為已簽署同意，
        /// 故將來若將 EMROuter 中簽不同意的病歷註記刪除，可不再需要此表
        /// </summary>
        public async Task<ApiResult<EMROuterConsent>> InsertWhenUpload(EMROuterConsent param)
        {
            try
            {
                var consent = (await DBUtil.QueryAsync<EMROuterConsent>(new EMROuterConsentQuery
                {
                    PatId = param.PatId,
                    OrgId = param.OrgId,
                })).FirstOrDefault();

                int rowsAffected = 0;
                if (consent == null)
                    rowsAffected = await DBUtil.InsertAsync<EMROuterConsent>(param);
            }
            catch (Exception) { }

            return new ApiResult<EMROuterConsent>(true, msgType: ApiMsgType.INSERT);
        }
    }
}
