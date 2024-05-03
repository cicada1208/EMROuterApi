using Lib.Consts;
using System.Net;

namespace Lib.Models
{
    public class ApiResult<TData>
    {
        /// <summary>
        /// 是否執行成功
        /// </summary>
        public bool Succ { get; set; } = false;

        /// <summary>
        /// Http Status Code
        /// </summary>
        public HttpStatusCode? Code { get; set; } = null;

        /// <summary>
        /// 訊息
        /// </summary>
        public string Msg { get; set; } = MsgConst.ApiFailure;

        /// <summary>
        /// 資料
        /// </summary>
        public TData? Data { get; set; }

        /// <summary>
        /// 處理筆數
        /// </summary>
        public int RowsAffected { get; set; } = 0;

        /// <summary>
        /// Model Validation Errors
        /// </summary>
        public object? ModelValidationErrors { get; set; }

        public string? TraceId { get; set; }

        public ApiResult() { }

        /// <summary>
        /// 建立 Query 成功結果
        /// </summary>
        public ApiResult(TData data, string msg = MsgConst.ApiSuccess)
        {
            Succ = true;
            Code = HttpStatusCode.OK;
            Msg = msg;
            Data = data;
        }

        /// <summary>
        /// 建立結果：依succ
        /// </summary>
        public ApiResult(bool succ, TData? data = default,
            string msg = "", ApiMsgType msgType = ApiMsgType.NONE,
            int rowsAffected = 0, HttpStatusCode? code = null)
        {
            Succ = succ;

            Code = code;
            if (Code == null) Code = Succ ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;

            Msg = msg;
            if (Msg == string.Empty)
                switch (msgType)
                {
                    case ApiMsgType.INSERT:
                        Msg = Succ ? MsgConst.InsertSuccess : MsgConst.InsertFailure;
                        break;
                    case ApiMsgType.UPDATE:
                        Msg = Succ ? MsgConst.UpdateSuccess : MsgConst.UpdateFailure;
                        break;
                    case ApiMsgType.DELETE:
                        Msg = Succ ? MsgConst.DeleteSuccess : MsgConst.DeleteFailure;
                        break;
                    case ApiMsgType.SAVE:
                        Msg = Succ ? MsgConst.SaveSuccess : MsgConst.SaveFailure;
                        break;
                    default:
                        Msg = Succ ? MsgConst.ApiSuccess : MsgConst.ApiFailure;
                        break;
                }

            Data = data;
            RowsAffected = rowsAffected;
        }

        /// <summary>
        /// 建立結果：依rowsAffected
        /// </summary>
        public ApiResult(int rowsAffected, TData? data = default,
            string msg = "", ApiMsgType msgType = ApiMsgType.NONE,
            bool? succ = null, HttpStatusCode? code = null)
        {
            if (!succ.HasValue) Succ = rowsAffected > 0;
            else Succ = succ.Value;

            Code = code;
            if (Code == null) Code = Succ ? HttpStatusCode.OK : (rowsAffected == 0 ? HttpStatusCode.NotFound : HttpStatusCode.InternalServerError);

            Msg = msg;
            if (Msg == string.Empty)
                switch (msgType)
                {
                    case ApiMsgType.INSERT:
                        Msg = Succ ? MsgConst.InsertSuccess : MsgConst.InsertFailure;
                        break;
                    case ApiMsgType.UPDATE:
                        Msg = Succ ? MsgConst.UpdateSuccess : MsgConst.UpdateFailure;
                        break;
                    case ApiMsgType.DELETE:
                        Msg = Succ ? MsgConst.DeleteSuccess : MsgConst.DeleteFailure;
                        break;
                    case ApiMsgType.SAVE:
                        Msg = Succ ? MsgConst.SaveSuccess : MsgConst.SaveFailure;
                        break;
                    default:
                        Msg = Succ ? MsgConst.ApiSuccess : MsgConst.ApiFailure;
                        break;
                }

            Data = data;
            RowsAffected = rowsAffected;
        }

        public ApiResult<object> ToApiResultObject() =>
            new()
            {
                Succ = Succ,
                Code = Code,
                Msg = Msg,
                Data = Data,
                RowsAffected = RowsAffected,
                ModelValidationErrors = ModelValidationErrors,
                TraceId = TraceId
            };
    }

    public class ApiError : ApiResult<object>
    {
        /// <summary>
        /// 建立失敗結果
        /// </summary>
        public ApiError(HttpStatusCode? code = HttpStatusCode.InternalServerError, string msg = MsgConst.ApiFailure,
            object? modelValidationErrors = null, string? traceId = null)
        {
            Succ = false;
            Code = code;
            Msg = msg;
            Data = null;
            ModelValidationErrors = modelValidationErrors;
            TraceId = traceId;
        }
    }

    /// <summary>
    /// ApiResult 訊息類型
    /// </summary>
    public enum ApiMsgType
    {
        NONE = 0,
        INSERT = 1,
        UPDATE = 2,
        DELETE = 3,
        SAVE = 4
    }

}
