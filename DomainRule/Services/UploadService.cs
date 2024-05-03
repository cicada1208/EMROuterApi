using Amazon.S3.Model;
using AutoMapper;
using DomainRule.Clients;
using DomainRule.Consts;
using DomainRule.Models.EMRDB;
using DomainRule.Repositories.EMRDB;
using FluentValidation;
using FluentValidation.Results;
using Lib.Models;
using Lib.Utilities;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace DomainRule.Services
{
    public class UploadService(
        EMROuterRepository emrOuterRepository,
        EMROuterConsentRepository emrOuterConsentRepository,
        EMROuterS3Client emrOuterS3Client,
        IValidator<EMROuter> emrOuterValidator,
        Repositories.EMRDB_1522011080.Mch_emrRepository mch_emr_1522011080Repository,
        EMRS3Client emrS3Client,
        IValidator<Mch_emr_Upload> mch_emr_UploadValidator,
        IMapper mapper)
    {
        public async Task<ApiResult<EMROuter>> UploadEMROuter(EMROuter param, IFormFile File)
        {
            string otherHospitalBucketName = "other-hospital";
            string otherHospitalContentType = "application/pdf";

            var validationResult = await emrOuterValidator.ValidateAsync(param);

            if (File.ContentType.ToLowerInvariant() != otherHospitalContentType)
                validationResult.Errors.Add(new ValidationFailure(nameof(File), $"檔案須為 {otherHospitalContentType}！"));

            var validationApiResult = validationResult.ToApiResult<EMROuter>();
            if (validationApiResult != null) return validationApiResult;

            string otherHospitalKey = $"{param.OrgId}/{param.PatId}/{param.RecType}/{param.PatId}-{param.OrgId}-{param.RecType}-{param.RecId}-{param.CreateDateTime:yyyyMMddHHmmss}.pdf";

            PutObjectRequest putObjectRequest = new()
            {
                BucketName = otherHospitalBucketName,
                Key = otherHospitalKey,
                ContentType = File.ContentType
            };

            using var ms = new MemoryStream();
            await File.CopyToAsync(ms);
            putObjectRequest.InputStream = ms;

            PutObjectResponse putObjectResponse = await emrOuterS3Client.PutObjectAsync(putObjectRequest);
            if (putObjectResponse.HttpStatusCode == HttpStatusCode.OK)
            {
                param.S3Path = $"{putObjectRequest.BucketName}/{putObjectRequest.Key}";
                ApiResult<EMROuter> result = await emrOuterRepository.Insert(param);
                result.Data = null;

                //更新外院病歷同意書
                await emrOuterConsentRepository.InsertWhenUpload(new EMROuterConsent
                {
                    PatId = param.PatId,
                    OrgId = param.OrgId,
                    Activate = true,
                    ModifyUser = param.OrgId
                });

                return result;
            }
            else
                return new ApiResult<EMROuter>(false, msg: "檔案寫入 S3 失敗！");
        }

        public async Task<ApiResult<object>> UploadMch_emr(Mch_emr_Upload param)
        {
            var validationResult = await mch_emr_UploadValidator.ValidateAsync(param);
            var validationApiResult = validationResult.ToApiResult<object>();
            if (validationApiResult != null) return validationApiResult;

            string emrdBucketName = $"emrd-{param.HospitalId}";
            PutObjectRequest putObjectRequest;
            PutObjectResponse putObjectResponse;

            string prefixKey = (param.emr_cre_dt + 19110000).ToString().Substring(0, 4) + "/" +
                (param.emr_cre_dt + 19110000).ToString().Substring(4, 4) + "/" +
                param.emr_cre_time.ToString().PadLeft(6, '0') + "/" +
                param.emr_ro_date_v.ToString() + "_" +
                param.emr_ro_seq_v.ToString() + "_" +
                param.emr_type + "_" +
                param.emr_chk_no.Trim();

            if (!param.XML.IsNullOrWhiteSpace())
            {
                string xmlKey = $"{prefixKey}.xml";
                putObjectRequest = new()
                {
                    BucketName = emrdBucketName,
                    Key = xmlKey,
                    ContentType = "application/xml",
                    ContentBody = param.XML
                };

                putObjectResponse = await emrS3Client.PutObjectAsync(putObjectRequest);
                if (putObjectResponse.HttpStatusCode != HttpStatusCode.OK)
                    return new ApiResult<object>(false, msg: "XML 寫入 S3 失敗！");
            }

            if (param.File != null)
            {
                foreach (var file in param.File)
                {
                    string fileKey = $"{prefixKey}_{file.FileName}";
                    putObjectRequest = new()
                    {
                        BucketName = emrdBucketName,
                        Key = fileKey,
                        ContentType = file.ContentType
                    };

                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    putObjectRequest.InputStream = ms;

                    putObjectResponse = await emrS3Client.PutObjectAsync(putObjectRequest);
                    if (putObjectResponse.HttpStatusCode != HttpStatusCode.OK)
                        return new ApiResult<object>(false, msg: "檔案寫入 S3 失敗！");
                }
            }

            return await InsertMch_emr(param);
        }

        public async Task<ApiResult<object>> InsertMch_emr(Mch_emr_Upload param)
        {
            switch (param.HospitalId)
            {
                case OtherHospitalId.Jianxing:
                    ApiResult<Models.EMRDB_1522011080.Mch_emr> jianxingResult = await mch_emr_1522011080Repository.Insert(mapper.Map<Models.EMRDB_1522011080.Mch_emr>(param));
                    jianxingResult.Data = null;
                    return jianxingResult.ToApiResultObject();
                default:
                    ApiResult<object> result = new(false, msg: $"該分院 {param.HospitalId} 未增加寫入 mch_emr 程式段！");
                    return result;
            }
        }

    }
}
