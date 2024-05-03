using Lib.Utilities;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace Lib.Utilities
{
    public class ModelUtil
    {
        /// <summary>
        /// 取得 Attribute Display(Name) 值
        /// </summary>
        /// <typeparam name="T">Model</typeparam>
        /// <param name="propertyName">Model PropertyName</param>
        ///<remarks>[Display(Name = "標題")]</remarks>
        public static string GetPropertyDisplayName<T>(string propertyName)
        {
            // usage: ModelUtil.GetPropertyDisplayName<MvvmViewModel>(nameof(MvvmViewModel.Title))
            var type = typeof(T);
            var pi = type.GetProperty(propertyName);
            string result = pi.GetPropertyDisplayName();
            return result;
        }

        /// <summary>
        /// 取得 Attribute Display(Name) 值
        /// </summary>
        /// <typeparam name="T">Model</typeparam>
        /// <param name="propertyExpression">Model PropertyExpression</param>
        ///<remarks>[Display(Name = "標題")]</remarks>
        public static string GetPropertyDisplayName<T>(Expression<Func<T, object>> propertyExpression)
        {
            // usage: ModelUtil.GetPropertyDisplayName<MvvmViewModel>(i => i.Title)
            var memberInfo = GetPropertyInfo(propertyExpression.Body);
            if (memberInfo == null)
                return string.Empty;

            var attr = memberInfo.GetAttribute<DisplayAttribute>();
            return attr?.GetName() ?? memberInfo.Name;
        }

        /// <summary>
        /// 取得 Attribute MaxLength 值
        /// </summary>
        /// <typeparam name="T">Model</typeparam>
        /// <param name="propertyName">Model PropertyName</param>
        ///<remarks>[MaxLength(1)]</remarks>
        public static int GetPropertyMaxLength<T>(string propertyName)
        {
            var type = typeof(T);
            var pi = type.GetProperty(propertyName);
            int result = pi.GetPropertyMaxLength();
            return result;
        }

        /// <summary>
        /// 取得 Attribute MaxLength 值
        /// </summary>
        /// <typeparam name="T">Model</typeparam>
        /// <param name="propertyExpression">Model PropertyExpression</param>
        ///<remarks>[MaxLength(1)]</remarks>
        public static int GetPropertyMaxLength<T>(Expression<Func<T, object>> propertyExpression)
        {
            var memberInfo = GetPropertyInfo(propertyExpression.Body);
            if (memberInfo == null)
                return 8000;

            var attr = memberInfo.GetAttribute<MaxLengthAttribute>();
            return attr?.Length ?? 8000;
        }

        public static MemberInfo? GetPropertyInfo(Expression propertyExpression)
        {
            Debug.Assert(propertyExpression != null, "propertyExpression != null");
            MemberExpression? memberExpr = propertyExpression as MemberExpression;

            if (memberExpr == null)
            {
                UnaryExpression? unaryExpr = propertyExpression as UnaryExpression;
                if (unaryExpr != null && unaryExpr.NodeType == ExpressionType.Convert)
                    memberExpr = unaryExpr.Operand as MemberExpression;
            }

            if (memberExpr != null && memberExpr.Member.MemberType == MemberTypes.Property)
                return memberExpr.Member;

            return null;
        }

    }

    public static class ModelExUtil
    {
        /// <summary>
        /// 取得 Attribute Display(Name) 值
        /// </summary>
        ///<remarks>[Display(Name = "標題")]</remarks>
        public static string GetPropertyDisplayName(this PropertyInfo? pi)
        {
            string result = string.Empty;
            if (pi != null)
                result = pi.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? pi.Name;
            return result;
        }

        /// <summary>
        /// 取得 Attribute Display(Name) 值
        /// </summary>
        /// <param name="entity">Model Entity</param>
        ///<remarks>[Display(Name = "標題")]</remarks>
        public static string GetPropertyDisplayName(this object entity, string propertyName) =>
             entity?.GetType().GetProperty(propertyName).GetPropertyDisplayName() ?? string.Empty;

        /// <summary>
        /// 取得 Attribute MaxLength 值
        /// </summary>
        ///<remarks>[MaxLength(1)]</remarks>
        public static int GetPropertyMaxLength(this PropertyInfo? pi)
        {
            int result = 8000;
            if (pi != null)
                result = pi.IsDefined(typeof(MaxLengthAttribute)) ? pi.GetCustomAttribute<MaxLengthAttribute>()!.Length : result;
            return result;
        }

        /// <summary>
        /// 取得 Attribute MaxLength 值
        /// </summary>
        /// <param name="entity">Model Entity</param>
        ///<remarks>[MaxLength(1)]</remarks>
        public static int GetPropertyMaxLength(this object entity, string propertyName) =>
             entity?.GetType().GetProperty(propertyName).GetPropertyMaxLength() ?? 8000;

        /// <summary>
        /// 取得 Attribute
        /// </summary>
        /// <typeparam name="T">Attribute</typeparam>
        public static T? GetAttribute<T>(this MemberInfo member, bool isRequired = false)
            where T : Attribute
        {
            var attribute = member.GetCustomAttributes(typeof(T), true).SingleOrDefault();

            if (attribute == null && isRequired)
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "The {0} attribute must be defined on member {1}",
                        typeof(T).Name,
                        member.Name));
            }

            return (T?)attribute;
        }

        /// <summary>
        /// 設定 Property Value
        /// </summary>
        public static void SetPropertyValue(this object? entity, string propertyName, object? newValue)
        {
            if (entity == null) return;
            PropertyInfo? prop = entity.GetType().GetProperty(propertyName);
            if (prop == null) return;
            prop.SetValue(entity, newValue);
        }

        /// <summary>
        /// 取得 Property Value
        /// </summary>
        public static object? GetPropertyValue(this object? entity, string propertyName)
        {
            if (entity == null) return null;
            PropertyInfo? prop = entity.GetType().GetProperty(propertyName);
            if (prop == null) return null;
            return prop.GetValue(entity);
        }

        /// <summary>
        /// 字串處理 (List<TModel>專用)
        /// </summary>
        public static List<TModel> StrProcess<TModel>(this List<TModel> list,
            TrimType trimType = TrimType.TrimEnd,
            bool nullToEmpty = true) //where TModel : class
        {
            try
            {
                list.ForEach(entity =>
                {
                    // BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance // 排除繼承屬性
                    var props = entity?.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite == true);

                    if (props != null)
                    {
                        foreach (var p in props)
                        {
                            if (p.PropertyType != typeof(string)) continue;
                            object? value = p.GetValue(entity);

                            if (trimType == TrimType.Trim)
                                value = value?.ToString()?.Trim();
                            else if (trimType == TrimType.TrimEnd)
                                value = value?.ToString()?.TrimEnd();
                            else if (trimType == TrimType.TrimStart)
                                value = value?.ToString()?.TrimStart();

                            if (nullToEmpty) value = value ?? string.Empty;

                            if (value != null) p.SetValue(entity, value);
                        }
                    }
                });
            }
            catch (Exception) { }
            return list;
        }

        /// <summary>
        /// 字串處理 (Model專用)
        /// </summary>
        public static TModel StrProcess<TModel>(this TModel entity,
            TrimType trimType = TrimType.TrimEnd,
            bool nullToEmpty = true) //where TModel : class
        {
            try
            {
                // BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance // 排除繼承屬性
                var props = entity?.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite == true);

                if (props != null)
                {
                    foreach (var p in props)
                    {
                        if (p.PropertyType != typeof(string)) continue;
                        object? value = p.GetValue(entity);

                        if (trimType == TrimType.Trim)
                            value = value?.ToString()?.Trim();
                        else if (trimType == TrimType.TrimEnd)
                            value = value?.ToString()?.TrimEnd();
                        else if (trimType == TrimType.TrimStart)
                            value = value?.ToString()?.TrimStart();

                        if (nullToEmpty) value = value ?? string.Empty;

                        if (value != null) p.SetValue(entity, value);
                    }
                }
            }
            catch (Exception) { }
            return entity;
        }

        /// <summary>
        /// JsonElement、JObject、Anonymous Type 轉換成 TModel
        /// </summary>
        /// <param name="param">JsonElement、JObject、Anonymous Type</param>
        /// <param name="model">Model</param>
        /// <param name="props">Model Selected Properties</param>
        /// <remarks>前端傳入匿名型別 param，先取得更新欄位再轉型</remarks>
        public static void GetModelAndProps<TModel>(this object param, out TModel model, out HashSet<string> props) where TModel : class, new()
        {
            if (param is JsonElement)
            {
                JsonElement jsonElement = (JsonElement)param;
                string json = jsonElement.ToString();
                model = JsonSerializer.Deserialize<TModel>(json)!;
                props = JsonDocument.Parse(json).RootElement.EnumerateObject().Select(p => p.Name).ToHashSet();
            }
            else if (param is JObject)
            {
                JObject jObject = (JObject)param;
                model = jObject.ToObject<TModel>()!;
                props = jObject.Properties().Select(p => p.Name).ToHashSet();
            }
            else
            {
                model = param is TModel ? (param as TModel)! : param.ToStatic<TModel>()!;
                props = param.GetType().GetProperties().Select(p => p.Name).ToHashSet();
            }

            if (props != null)
            {
                props = typeof(TModel).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(p => p.Name).ToHashSet()
                    .Join(props, modelPName => modelPName, paramPName => paramPName,
                    (modelPName, paramPName) => modelPName).ToHashSet();
            }
        }

        /// <summary>
        /// Anonymous Type or Model 轉換成 TModel
        /// </summary>
        public static TModel? ToStatic<TModel>(this object? anonymous) where TModel : class, new()
        {
            //var entity = Activator.CreateInstance<T>();
            TModel? entity = anonymous == null ? null : new();

            if (anonymous != null)
            {
                foreach ((PropertyInfo pEntity, PropertyInfo pAnonymous) in
                    entity!.GetType().GetProperties().Join(anonymous.GetType().GetProperties(),
                    pEntity => pEntity.Name,
                    pAnonymous => pAnonymous.Name,
                    (pEntity, pAnonymous) => (pEntity, pAnonymous)))
                {
                    if (pEntity.CanWrite)
                        pEntity.SetValue(entity, pAnonymous.GetValue(anonymous));
                }
            }

            return entity;
        }

        /// <summary>
        /// 加入選取的欄位
        /// </summary>
        public static void AddProps<TModel>(this HashSet<string> props, Expression<Func<TModel, object>> propSelector) where TModel : class
        {
            var selectProps = propSelector.Body.Type.GetProperties();

            var selectPropsName = typeof(TModel).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(pModel => selectProps.Any(pSelect => pModel.Name == pSelect.Name))
                .Select(pModel => pModel.Name).ToList();

            selectPropsName.ForEach(p =>
            {
                if (!props.Contains(p))
                    props.Add(p);
            });
        }

    }

    //public static class ListToDataTable
    //{
    //    public static DataTable ToDataTable<T>(this IEnumerable<T> source)
    //    {
    //        return new ObjectShredder<T>().Shred(source, null, null);
    //    }

    //    public static DataTable ToDataTable<T>(this IEnumerable<T> source,
    //    DataTable table, LoadOption? options)
    //    {
    //        return new ObjectShredder<T>().Shred(source, table, options);
    //    }
    //}

    //public class ObjectShredder<T>
    //{
    //    private System.Reflection.FieldInfo[] _fi;
    //    private System.Reflection.PropertyInfo[] _pi;
    //    private System.Collections.Generic.Dictionary<string, int> _ordinalMap;
    //    private System.Type _type;

    //    // ObjectShredder constructor.
    //    public ObjectShredder()
    //    {
    //        _type = typeof(T);
    //        _fi = _type.GetFields();
    //        _pi = _type.GetProperties();
    //        _ordinalMap = new Dictionary<string, int>();
    //    }

    //    /// <summary>
    //    /// Loads a DataTable from a sequence of objects.
    //    /// </summary>
    //    /// <param name="source">The sequence of objects to load into the DataTable.</param>
    //    /// <param name="table">The input table. The schema of the table must match that 
    //    /// the type T.  If the table is null, a new table is created with a schema 
    //    /// created from the public properties and fields of the type T.</param>
    //    /// <param name="options">Specifies how values from the source sequence will be applied to 
    //    /// existing rows in the table.</param>
    //    /// <returns>A DataTable created from the source sequence.</returns>
    //    public DataTable Shred(IEnumerable<T> source, DataTable table, LoadOption? options)
    //    {
    //        // Load the table from the scalar sequence if T is a primitive type.
    //        if (typeof(T).IsPrimitive)
    //        {
    //            return ShredPrimitive(source, table, options);
    //        }

    //        // Create a new table if the input table is null.
    //        if (table == null)
    //        {
    //            table = new DataTable(typeof(T).Name);
    //        }

    //        // Initialize the ordinal map and extend the table schema based on type T.
    //        table = ExtendTable(table, typeof(T));

    //        // Enumerate the source sequence and load the object values into rows.
    //        table.BeginLoadData();
    //        using (IEnumerator<T> e = source.GetEnumerator())
    //        {
    //            while (e.MoveNext())
    //            {
    //                if (options != null)
    //                {
    //                    table.LoadDataRow(ShredObject(table, e.Current), (LoadOption)options);
    //                }
    //                else
    //                {
    //                    table.LoadDataRow(ShredObject(table, e.Current), true);
    //                }
    //            }
    //        }
    //        table.EndLoadData();

    //        // Return the table.
    //        return table;
    //    }

    //    public DataTable ShredPrimitive(IEnumerable<T> source, DataTable table, LoadOption? options)
    //    {
    //        // Create a new table if the input table is null.
    //        if (table == null)
    //        {
    //            table = new DataTable(typeof(T).Name);
    //        }

    //        if (!table.Columns.Contains("Value"))
    //        {
    //            table.Columns.Add("Value", typeof(T));
    //        }

    //        // Enumerate the source sequence and load the scalar values into rows.
    //        table.BeginLoadData();
    //        using (IEnumerator<T> e = source.GetEnumerator())
    //        {
    //            Object[] values = new object[table.Columns.Count];
    //            while (e.MoveNext())
    //            {
    //                values[table.Columns["Value"].Ordinal] = e.Current;

    //                if (options != null)
    //                {
    //                    table.LoadDataRow(values, (LoadOption)options);
    //                }
    //                else
    //                {
    //                    table.LoadDataRow(values, true);
    //                }
    //            }
    //        }
    //        table.EndLoadData();

    //        // Return the table.
    //        return table;
    //    }

    //    public object[] ShredObject(DataTable table, T instance)
    //    {

    //        FieldInfo[] fi = _fi;
    //        PropertyInfo[] pi = _pi;

    //        if (instance.GetType() != typeof(T))
    //        {
    //            // If the instance is derived from T, extend the table schema
    //            // and get the properties and fields.
    //            ExtendTable(table, instance.GetType());
    //            fi = instance.GetType().GetFields();
    //            pi = instance.GetType().GetProperties();
    //        }

    //        // Add the property and field values of the instance to an array.
    //        Object[] values = new object[table.Columns.Count];
    //        foreach (FieldInfo f in fi)
    //        {
    //            values[_ordinalMap[f.Name]] = f.GetValue(instance);
    //        }

    //        foreach (PropertyInfo p in pi)
    //        {
    //            values[_ordinalMap[p.Name]] = p.GetValue(instance, null);
    //        }

    //        // Return the property and field values of the instance.
    //        return values;
    //    }

    //    public DataTable ExtendTable(DataTable table, Type type)
    //    {
    //        // Extend the table schema if the input table was null or if the value 
    //        // in the sequence is derived from type T.            
    //        foreach (FieldInfo f in type.GetFields())
    //        {
    //            if (!_ordinalMap.ContainsKey(f.Name))
    //            {
    //                // Add the field as a column in the table if it doesn't exist
    //                // already.
    //                DataColumn dc = table.Columns.Contains(f.Name) ? table.Columns[f.Name]
    //                    : table.Columns.Add(f.Name, f.FieldType);

    //                // Add the field to the ordinal map.
    //                _ordinalMap.Add(f.Name, dc.Ordinal);
    //            }
    //        }
    //        foreach (PropertyInfo p in type.GetProperties())
    //        {
    //            if (!_ordinalMap.ContainsKey(p.Name))
    //            {
    //                // Add the property as a column in the table if it doesn't exist
    //                // already.
    //                DataColumn dc = table.Columns.Contains(p.Name) ? table.Columns[p.Name]
    //                    : table.Columns.Add(p.Name, p.PropertyType);

    //                // Add the property to the ordinal map.
    //                _ordinalMap.Add(p.Name, dc.Ordinal);
    //            }
    //        }

    //        // Return the table.
    //        return table;
    //    }
    //}
}
