using NCalc;

namespace Lib.Utilities
{
    public class StrUtil
    {
        /// <summary>
        /// 比較兩字串數值是否相等
        /// </summary>
        /// <returns>true: 相等，false: 不等</returns>
        public static bool CompareEqualStrNum(object? obj1, object? obj2)
        {
            bool result = false;
            string str1, str2;
            double num1, num2;
            bool compareStr = false;

            try
            {
                str1 = obj1.NullableToStr();
                str2 = obj2.NullableToStr();

                if (!double.TryParse(str1, out num1))
                    compareStr = true;

                if (!double.TryParse(str2, out num2))
                    compareStr = true;

                if (!compareStr)
                    result = num1 == num2;
                else
                    result = str1 == str2;
            }
            catch (Exception) { }

            return result;
        }

        /// <summary>
        /// 比較兩字串數值
        /// </summary>
        /// <returns>
        /// -1: obj1 小於 obj2,
        /// 0: obj1 等於 obj2,
        /// 1: obj1 大於 obj2,
        /// 99: format error
        /// </returns>
        public static CompareResult CompareStrNum(object? obj1, object? obj2)
        {
            CompareResult result = CompareResult.FormatErr;
            string str1, str2;

            try
            {
                str1 = obj1.NullableToStr();
                str2 = obj2.NullableToStr();

                if (double.Parse(str1) < double.Parse(str2))
                    result = CompareResult.OneLessTwo;
                else if (double.Parse(str1) == double.Parse(str2))
                    result = CompareResult.OneEqualTwo;
                else
                    result = CompareResult.OneMoreTwo;
            }
            catch (Exception) { }

            return result;
        }

    }

    public static class StrExUtil
    {
        /// <summary>
        ///  回傳物件值的字串，並執行Trim
        ///  (DBNull、null 回傳 string.Empty)
        /// </summary>
        public static string NullableToStr(this object? value,
            TrimType trimType = TrimType.Trim)
        {
            // DBNull.Value.ToString() == string.Empty
            //return (value ?? string.Empty).ToString().Trim();
            string result = (value ?? string.Empty).ToString()!;
            if (trimType == TrimType.Trim) result = result.Trim();
            else if (trimType == TrimType.TrimEnd) result = result.TrimEnd();
            else if (trimType == TrimType.TrimStart) result = result.TrimStart();
            return result;
        }

        /// <summary>
        ///  回傳 DateTime? 物件值的字串
        /// </summary>
        public static string NullableToStr(this DateTime? value, string? format = null)
        {
            if (value != null)
                return format == null ? ((DateTime)value).ToString() : ((DateTime)value).ToString(format);
            else
                return string.Empty;
        }

        /// <summary>
        ///  取得字串片斷
        /// </summary>
        /// <param name="startIndex">The zero-based starting character position of a substring in this instance.</param>
        /// <param name="length">The number of characters in the substring.</param>
        /// <returns></returns>
        public static string SubStr(this string? value, int startIndex, int? length = null)
        {
            string data = value ?? string.Empty;
            int dataLength = data.Length;
            int subLength = dataLength - startIndex;
            string result;

            if (length == 0)
                result = string.Empty;
            else
            {
                if (dataLength <= startIndex)
                    result = string.Empty;
                else
                {
                    if (length > subLength)
                        length = subLength;
                    if (length == null)
                        result = data.Substring(startIndex);
                    else
                        result = data.Substring(startIndex, (int)length);
                }
            }

            return result;
        }

        /// <summary>
        ///  取得字串片斷 (編碼 big5, codepage=950)
        /// </summary>
        /// <param name="startIndex">begin with 1</param>
        public static string SybSubStr(this string? value, int startIndex, int? length = null)
        {
            if (startIndex <= 0) return string.Empty;
            startIndex = startIndex - 1;
            string data = value ?? string.Empty;
            byte[] dataByte = System.Text.Encoding.GetEncoding(950).GetBytes(data);
            int dataLength = dataByte.Length;
            int subLength = dataLength - startIndex;
            string result;

            if (length == 0)
                result = string.Empty;
            else
            {
                if (dataLength <= startIndex)
                    result = string.Empty;
                else
                {
                    if (length > subLength)
                        length = subLength;
                    if (length == null)
                        result = System.Text.Encoding.GetEncoding(950).GetString(dataByte, startIndex, subLength);
                    else
                        result = System.Text.Encoding.GetEncoding(950).GetString(dataByte, startIndex, (int)length);
                }
            }

            return result;
        }

        /// <summary>
        /// 取得字串長度 (預設編碼 big5, codepage=950)
        /// </summary>
        /// <remarks>
        /// codepage = 950 適用於 sybase、sqlserver 的 char、varchar。
        /// sqlserver 的 nchar、nvarchar 直接使用 Length 即可。
        /// </remarks>
        public static int StrLen(this string? value, int codepage = 950) =>
           value != null ? System.Text.Encoding.GetEncoding(codepage).GetBytes(value).Length : 0;

        /// <summary>
        /// 首字轉為大寫
        /// </summary>
        public static string ToUpperFirstChar(this string? value) =>
            value.SubStr(0, 1).ToUpper() + value.SubStr(1);

        /// <summary>
        /// 判斷是否為數值
        /// </summary>
        /// <remarks>資料來源：http://support.microsoft.com/kb/329488/zh-tw </remarks>
        public static bool IsNumeric(this object? value)
        {
            // The TryParse method converts a string in a specified style and culture-specific format to its double-precision floating point number equivalent.
            // The TryParse method does not generate an exception if the conversion fails. If the conversion passes, True is returned. If it does not, False is returned.
            return double.TryParse(Convert.ToString(value),
                System.Globalization.NumberStyles.Any,
                System.Globalization.NumberFormatInfo.InvariantInfo,
                out double dNum);
        }

        public static bool IsNullOrWhiteSpace(this object? value) =>
            string.IsNullOrWhiteSpace(value.NullableToStr());

        public static short ToShort(this object? value, bool useExpression = false)
        {
            short result = 0;
            try
            {
                if (useExpression)
                {
                    Expression e = new Expression(value.NullableToStr());
                    short.TryParse(e.Evaluate().NullableToStr(), out result);
                }
                else
                    short.TryParse(value.NullableToStr(), out result);
            }
            catch (Exception) { }
            return result;
        }

        public static short? ToNullableShort(this object? value, bool useExpression = false)
        {
            try
            {
                if (useExpression)
                {
                    Expression e = new Expression(value.NullableToStr());
                    return short.TryParse(e.Evaluate().NullableToStr(), out short result) ? result : null;
                }
                else
                    return short.TryParse(value.NullableToStr(), out short result) ? result : null;
            }
            catch (Exception) { return null; }
        }

        public static int ToInt(this object? value, bool useExpression = false)
        {
            int result = 0;
            try
            {
                if (useExpression)
                {
                    Expression e = new Expression(value.NullableToStr());
                    int.TryParse(e.Evaluate().NullableToStr(), out result);
                }
                else
                    int.TryParse(value.NullableToStr(), out result);
            }
            catch (Exception) { }
            return result;
        }

        public static int? ToNullableInt(this object? value, bool useExpression = false)
        {
            try
            {
                if (useExpression)
                {
                    Expression e = new Expression(value.NullableToStr());
                    return int.TryParse(e.Evaluate().NullableToStr(), out int result) ? result : null;
                }
                else
                    return int.TryParse(value.NullableToStr(), out int result) ? result : null;
            }
            catch (Exception) { return null; }
        }

        public static long ToLong(this object? value, bool useExpression = false)
        {
            long result = 0;
            try
            {
                if (useExpression)
                {
                    Expression e = new Expression(value.NullableToStr());
                    long.TryParse(e.Evaluate().NullableToStr(), out result);
                }
                else
                    long.TryParse(value.NullableToStr(), out result);
            }
            catch (Exception) { }
            return result;
        }

        public static long? ToNullableLong(this object? value, bool useExpression = false)
        {
            try
            {
                if (useExpression)
                {
                    Expression e = new Expression(value.NullableToStr());
                    return long.TryParse(e.Evaluate().NullableToStr(), out long result) ? result : null;
                }
                else
                    return long.TryParse(value.NullableToStr(), out long result) ? result : null;
            }
            catch (Exception) { return null; }
        }

        public static double ToDouble(this object? value, bool useExpression = false)
        {
            double result = 0;
            try
            {
                if (useExpression)
                {
                    Expression e = new Expression(value.NullableToStr());
                    double.TryParse(e.Evaluate().NullableToStr(), out result);
                }
                else
                    double.TryParse(value.NullableToStr(), out result);
            }
            catch (Exception) { }
            return result;
        }

        public static double? ToNullableDouble(this object? value, bool useExpression = false)
        {
            try
            {
                if (useExpression)
                {
                    Expression e = new Expression(value.NullableToStr());
                    return double.TryParse(e.Evaluate().NullableToStr(), out double result) ? result : null;
                }
                else
                    return double.TryParse(value.NullableToStr(), out double result) ? result : null;
            }
            catch (Exception) { return null; }
        }

        /// <summary>
        /// 串接分隔符
        /// </summary>
        /// <returns></returns>
        public static string ConcatSeparator(this string? value, string separator = " ") =>
             string.Join(separator, value.NullableToStr().ToCharArray());

    }

    public enum TrimType
    {
        None = 0,
        Trim = 1,
        TrimEnd = 2,
        TrimStart = 3
    }

    public enum CompareResult
    {
        OneLessTwo = -1,
        OneEqualTwo = 0,
        OneMoreTwo = 1,
        FormatErr = 99
    }

}
