using System;
using Lib.Utilities;

namespace Lib.Attributes
{
    /// <summary>
    /// mapped model to table
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false), Serializable]
    public class TableAttribute : Attribute
    {
        /// <summary>
        /// DB類型
        /// </summary>
        public DBType DBType;
        /// <summary>
        /// DB名稱
        /// </summary>
        public string DBName;
        /// <summary>
        /// 表格名稱
        /// </summary>
        public string TableName;

        public TableAttribute(DBType dbType, string dbName, string tbName = "")
        {
            DBType = dbType;
            DBName = dbName;
            TableName = tbName;
        }
    }
}
