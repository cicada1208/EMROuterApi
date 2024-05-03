using Lib.Models;
using Lib.Utilities;

namespace DomainRule.Repositories
{
    public abstract class BaseRepository<TModel> where TModel : class
    {
        public BaseRepository()
        {
            DBType = DBUtil.GetDBType<TModel>();
            DBName = DBUtil.GetDBName<TModel>();
            TableName = DBUtil.GetTableName<TModel>();
        }

        /// <summary>
        /// DB類型
        /// </summary>
        public DBType DBType { get; set; } = DBType.SQLSERVER;
        /// <summary>
        /// DB名稱
        /// </summary>
        public string DBName { get; set; } = string.Empty;
        /// <summary>
        /// 表格名稱
        /// </summary>
        public string TableName { get; set; } = string.Empty;

        private DBUtil? _DBUtil;
        protected DBUtil DBUtil =>
            _DBUtil ??= new DBUtil(DBName, DBType);

        /// <summary>
        /// 查詢 TModel 資料表
        /// </summary>
        public virtual async Task<ApiResult<List<TModel>>> Get(object? param)
        {
            var query = (await DBUtil.QueryAsync<TModel>(param)).ToList();
            return new ApiResult<List<TModel>>(query);
        }

        /// <summary>
        /// 新增 TModel 資料表
        /// </summary>
        public virtual async Task<ApiResult<TModel>> Insert(TModel param)
        {
            int rowsAffected = await DBUtil.InsertAsync<TModel>(param);
            return new ApiResult<TModel>(rowsAffected, param, msgType: ApiMsgType.INSERT);
        }

        /// <summary>
        /// 整筆更新 TModel 資料表
        /// </summary>
        public virtual async Task<ApiResult<TModel>> Update(TModel param)
        {
            int rowsAffected = await DBUtil.UpdateAsync<TModel>(param);
            return new ApiResult<TModel>(rowsAffected, param, msgType: ApiMsgType.UPDATE);
        }

        /// <summary>
        /// 部份欄位更新 TModel 資料表
        /// </summary>
        public virtual async Task<ApiResult<TModel>> Patch(TModel param, HashSet<string> updateCol)
        {
            int rowsAffected = await DBUtil.PatchAsync<TModel>(param, updateCol);
            return new ApiResult<TModel>(rowsAffected, param, msgType: ApiMsgType.UPDATE);
        }

        /// <summary>
        /// 刪除 TModel 資料表
        /// </summary>
        public virtual async Task<ApiResult<TModel>> Delete(TModel param)
        {
            int rowsAffected = await DBUtil.DeleteAsync<TModel>(param);
            return new ApiResult<TModel>(rowsAffected, msgType: ApiMsgType.DELETE);
        }

    }
}
