using Lib.Models;
using Lib.Utilities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Lib.Utilities
{
    public class SqlBuildUtil
    {
        ///// <summary>
        ///// SQL 指令的 Table name
        ///// </summary>
        //public string TableName { get; set; }

        public SqlBuildUtil() { }

        //public SqlBuildUtil(string tableName)
        //{
        //    TableName = tableName;
        //}

        /// <summary>
        /// Build Select SQL
        /// </summary>
        /// <param name="type">typeof(model), 具有定義 TableAttribute</param>
        /// <param name="param">未必定義 TableAttribute</param>
        public virtual SqlCmd Select(Type type, dynamic? param = null)
        {
            SqlCmd sqlCmd = new SqlCmd();
            var TableName = DBUtil.GetTableName(type);
            // BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance // 排除繼承屬性
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // 組建 where：欄位屬性非[NotMapped]、欄位值非null/空白
            var wheres = props.Where(p => p.GetCustomAttribute(typeof(NotMappedAttribute), true) == null &&
            ((param as object)?.GetType()?.GetProperty(p.Name)?.GetValue(param as object)).NullableToStr(TrimType.None) != string.Empty) // p.GetValue(entity)
            .Select(p => p.Name).ToList();

            sqlCmd.Builder.Append($"select * from {TableName} ");
            if (wheres != null && wheres.Any())
            {
                sqlCmd.Builder.Append($"where {string.Join(" and ", wheres.Select(name => name + " = @" + name).ToArray())} ");
                sqlCmd.Param = param;
            }

            return sqlCmd;
        }

        /// <summary>
        /// Build Insert SQL
        /// </summary>
        /// <param name="type">typeof(model), 具有定義 TableAttribute</param>
        /// <param name="param">未必定義 TableAttribute</param>
        /// <param name="excludeCol">insert 排除欄位</param>
        public virtual SqlCmd Insert(Type type, dynamic param, HashSet<string>? excludeCol = null)
        {
            SqlCmd sqlCmd = new SqlCmd();
            var TableName = DBUtil.GetTableName(type);
            // BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance // 排除繼承屬性
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // 組建 insert column：欄位屬性非[NotMapped]、欄位非自增子[DatabaseGenerated(DatabaseGeneratedOption.Identity)]、欄位非excludeCol
            var filter = props.Where(p => p.GetCustomAttribute(typeof(NotMappedAttribute), true) == null &&
            (p.GetCustomAttribute(typeof(DatabaseGeneratedAttribute), true) as DatabaseGeneratedAttribute)?.DatabaseGeneratedOption != DatabaseGeneratedOption.Identity);
            if (excludeCol != null) filter = filter.Where(p => !excludeCol.Contains(p.Name));
            var columnNames = filter.Select(p => p.Name).ToList();

            sqlCmd.Builder.Append($"insert into {TableName} ( {string.Join(", ", columnNames.ToArray())} ) ");
            sqlCmd.Builder.Append($"values ( {string.Join(", ", columnNames.Select(name => "@" + name).ToArray())} ) ");
            sqlCmd.Param = param;

            return sqlCmd;
        }

        /// <summary>
        /// Build Update SQL
        /// </summary>
        /// <param name="type">typeof(model), 具有定義 TableAttribute</param>
        /// <param name="param">未必定義 TableAttribute</param>
        /// <param name="excludeCol">update 排除欄位</param>
        public virtual SqlCmd Update(Type type, dynamic param, HashSet<string>? excludeCol = null)
        {
            SqlCmd sqlCmd = new SqlCmd();
            var TableName = DBUtil.GetTableName(type);
            // BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance // 排除繼承屬性
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // 組建 where：搜尋Key欄位、皆建立條件無論Key值null/空白
            var wheres = props.Where(p => p.GetCustomAttribute(typeof(KeyAttribute), true) != null)
            .Select(p => p.Name).ToList();

            // 組建 set：欄位屬性非[NotMapped]、排除Key欄位、欄位非excludeCol
            var filter = props.Where(p => p.GetCustomAttribute(typeof(NotMappedAttribute), true) == null &&
            !wheres.Contains(p.Name));
            if (excludeCol != null) filter = filter.Where(p => !excludeCol.Contains(p.Name));
            var sets = filter.Select(p => p.Name).ToList();

            sqlCmd.Builder.Append($"update {TableName} set {string.Join(", ", sets.Select(name => name + " = @" + name).ToArray())} ");
            sqlCmd.Builder.Append($"where {string.Join(" and ", wheres.Select(name => name + " = @" + name).ToArray())} ");
            sqlCmd.Param = param;

            // 沒有PK則報錯
            if (wheres == null || !wheres.Any())
                throw new Exception($"Update TableName: {TableName}. Primary key is empty.");

            return sqlCmd;
        }

        /// <summary>
        /// Build Patch SQL
        /// </summary>
        /// <param name="type">typeof(model), 具有定義 TableAttribute</param>
        /// <param name="param">未必定義 TableAttribute</param>
        /// <param name="updateCol">update 欄位</param>
        public virtual SqlCmd Patch(Type type, dynamic param, HashSet<string> updateCol)
        {
            SqlCmd sqlCmd = new SqlCmd();
            var TableName = DBUtil.GetTableName(type);
            // BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance // 排除繼承屬性
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // 組建 where：搜尋Key欄位、皆建立條件無論Key值null/空白
            var wheres = props.Where(p => p.GetCustomAttribute(typeof(KeyAttribute), true) != null)
            .Select(p => p.Name).ToList();

            // 組建 set：欄位屬性非[NotMapped]、排除Key欄位、只更新updateCol
            var filter = props.Where(p => p.GetCustomAttribute(typeof(NotMappedAttribute), true) == null &&
             !wheres.Contains(p.Name) &&
            updateCol.Contains(p.Name));
            var sets = filter.Select(p => p.Name).ToList();

            sqlCmd.Builder.Append($"update {TableName} set {string.Join(", ", sets.Select(name => name + " = @" + name).ToArray())} ");
            sqlCmd.Builder.Append($"where {string.Join(" and ", wheres.Select(name => name + " = @" + name).ToArray())} ");
            sqlCmd.Param = param;

            // 沒有PK則報錯
            if (wheres == null || !wheres.Any())
                throw new Exception($"Patch TableName: {TableName}. Primary key is empty.");

            return sqlCmd;
        }

        /// <summary>
        /// Build Delete SQL
        /// </summary>
        /// <param name="type">typeof(model), 具有定義 TableAttribute</param>
        /// <param name="param">未必定義 TableAttribute</param>
        public virtual SqlCmd Delete(Type type, dynamic param)
        {
            SqlCmd sqlCmd = new SqlCmd();
            var TableName = DBUtil.GetTableName(type);
            // BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance // 排除繼承屬性
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // 組建 where：搜尋Key欄位、皆建立條件無論Key值null/空白
            var wheres = props.Where(p => p.GetCustomAttribute(typeof(KeyAttribute), true) != null)
            .Select(p => p.Name).ToList();

            sqlCmd.Builder.Append($"delete from {TableName} ");
            sqlCmd.Builder.Append($"where {string.Join(" and ", wheres.Select(name => name + " = @" + name).ToArray())} ");
            sqlCmd.Param = param;

            // 沒有PK則報錯
            if (wheres == null || !wheres.Any())
                throw new Exception($"Delete TableName: {TableName}. Primary key is empty.");

            return sqlCmd;
        }

    }
}