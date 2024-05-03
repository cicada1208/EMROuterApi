using System.Globalization;

namespace Lib.Utilities
{
    public class DateTimeUtil
    {
        public static void SetCurrentCulture()
        {
            CultureInfo ci = new CultureInfo("zh-TW"); // zh-CN
            ci.DateTimeFormat.ShortDatePattern = "yyyy/MM/dd";
            ci.DateTimeFormat.ShortTimePattern = "HH:mm:ss";
            Thread.CurrentThread.CurrentCulture = ci;
            //Thread.CurrentThread.CurrentUICulture = ci;
        }

        public static void SetDefaultCulture()
        {
            CultureInfo ci = new CultureInfo("zh-TW"); // zh-CN
            ci.DateTimeFormat.ShortDatePattern = "yyyy/MM/dd";
            ci.DateTimeFormat.ShortTimePattern = "HH:mm:ss";
            CultureInfo.DefaultThreadCurrentCulture = ci;
            //CultureInfo.DefaultThreadCurrentUICulture = ci;
        }

        /// <summary>
        /// 西元年轉換為民國年或西元年
        /// </summary>
        /// <param name="inDate">西元年</param>
        /// <param name="toROC">是否轉為民國年</param>
        /// <param name="inFormat">輸入西元年格式</param>
        /// <param name="outFormat">輸出格式</param>
        /// <returns>民國年或西元年</returns>
        /// <remarks>測試閏年 2012/02/29 to 101/02/29</remarks>
        public static string ConvertAD(string inDate, bool toROC = true,
            string inFormat = "yyyyMMdd", string outFormat = "yyy/MM/dd")
        {
            string result = string.Empty;

            try
            {
                inDate = inDate.NullableToStr();
                result = inDate == "0" ? string.Empty : inDate;
                if (result.IsNullOrWhiteSpace()) return result;

                DateTime parseDate = DateTime.ParseExact(inDate, inFormat, null);

                if (toROC)
                {
                    CultureInfo culture = new CultureInfo("zh-TW");
                    culture.DateTimeFormat.Calendar = new TaiwanCalendar();
                    result = parseDate.ToString(outFormat, culture);
                }
                else
                    result = parseDate.ToString(outFormat);
            }
            catch (Exception) { }

            return result;
        }

        /// <summary>
        /// 民國年轉換為西元年或民國年
        /// </summary>
        /// <param name="inDate"> 民國年</param>
        /// <param name="toAD">是否轉為西元年</param>
        /// <param name="inFormat">輸入民國年格式</param>
        /// <param name="outFormat">輸出格式</param>
        /// <returns>西元年或民國年</returns>
        public static string ConvertROC(string inDate, bool toAD = true,
            string inFormat = "yyyMMdd", string outFormat = "yyyy/MM/dd")
        {
            string result = string.Empty;

            try
            {
                inDate = inDate.NullableToStr();
                result = inDate == "0" ? string.Empty : inDate;
                if (result.IsNullOrWhiteSpace()) return result;

                if (inFormat.StartsWith("yyy"))
                    inDate = inDate.PadLeft(inFormat.Length + 1, '0');

                CultureInfo culture = new CultureInfo("zh-TW");
                culture.DateTimeFormat.Calendar = new TaiwanCalendar();
                DateTime parseDate = DateTime.ParseExact(inDate, inFormat, culture);

                if (toAD)
                    result = parseDate.ToString(outFormat);
                else
                    result = parseDate.ToString(outFormat, culture);
            }
            catch (Exception) { }

            return result;
        }

        /// <summary>
        /// 計算年齡
        /// </summary>
        /// <param name="birth">生日</param>
        /// <param name="adm">入院日或 Now</param>
        /// <param name="birthFormat">生日格式</param>
        /// <param name="admFormat">入院日或 Now 格式</param>
        /// <returns></returns>
        public static string GetAge(string birth, string adm = "",
            string birthFormat = "yyyy/MM/dd", string admFormat = "yyyy/MM/dd")
        {
            DateTime birthDate, admDate;
            int age = 0;
            string outFormat = "yyyy/MM/dd";

            try
            {
                if (!birthFormat.StartsWith("yyyy"))
                {
                    birth = ConvertROC(birth, inFormat: birthFormat, outFormat: outFormat);
                    birthFormat = outFormat;
                }
                if (!admFormat.StartsWith("yyyy"))
                {
                    adm = ConvertROC(adm, inFormat: admFormat, outFormat: outFormat);
                    admFormat = outFormat;
                }

                if (!DateTime.TryParseExact(birth, birthFormat, null, DateTimeStyles.None, out birthDate))
                    return string.Empty; //age.ToString();
                if (!DateTime.TryParseExact(adm, admFormat, null, DateTimeStyles.None, out admDate))
                    return string.Empty; //age.ToString();

                age = admDate.Year - birthDate.Year;
                if (admDate.Month < birthDate.Month || admDate.Month == birthDate.Month && admDate.Day < birthDate.Day)
                    age--;

                age = age < 0 ? 0 : age;
            }
            catch (Exception) { }

            return age.ToString();
        }

        /// <summary>
        /// 計算兩時間的差值
        /// </summary>
        /// <remarks>str2 >= str1</remarks>
        public static string DateTimeDiff(string str1, string str2,
             string str1Format = "yyyy/MM/dd HH:mm:ss", string str2Format = "yyyy/MM/dd HH:mm:ss")
        {
            string result = string.Empty;
            string outFormat = "yyyy/MM/dd HH:mm:ss";
            try
            {
                if (!str1Format.StartsWith("yyyy"))
                {
                    str1 = ConvertROC(str1, inFormat: str1Format, outFormat: outFormat);
                    str1Format = outFormat;
                }

                if (!str2Format.StartsWith("yyyy"))
                {
                    str2 = ConvertROC(str2, inFormat: str2Format, outFormat: outFormat);
                    str2Format = outFormat;
                }

                if (!DateTime.TryParseExact(str1, str1Format, null, DateTimeStyles.None, out DateTime dt1))
                    return result;
                if (!DateTime.TryParseExact(str2, str2Format, null, DateTimeStyles.None, out DateTime dt2))
                    return result;
                result = DateTimeDiff(dt1, dt2);
            }
            catch (Exception) { }

            return result;
        }

        /// <summary>
        /// 計算兩時間的差值
        /// </summary>
        /// <remarks>dt2 >= dt1</remarks>
        public static string DateTimeDiff(DateTime dt1, DateTime dt2)
        {
            string result = string.Empty;
            try
            {
                TimeSpan ts = dt2.Subtract(dt1);
                // ts.ToString(@"dd\:hh\:mm\:ss")
                // ts.ToString("dd'天 'hh'小時 'mm'分 'ss'秒'")
                string fmt = ts.Days == 0 ? @"hh\:mm\:ss" : @"dd\:hh\:mm\:ss";
                result = ts.ToString(fmt);
            }
            catch (Exception) { }

            return result;
        }

        /// <summary>
        /// 計算兩時間的差值
        /// </summary>
        /// <remarks>str2 >= str1</remarks>
        public static TimeSpan DateTimeDiffTS(string str1, string str2,
            string str1Format = "yyyy/MM/dd HH:mm:ss", string str2Format = "yyyy/MM/dd HH:mm:ss")
        {
            TimeSpan result = default;
            string outFormat = "yyyy/MM/dd HH:mm:ss";
            try
            {
                if (!str1Format.StartsWith("yyyy"))
                {
                    str1 = ConvertROC(str1, inFormat: str1Format, outFormat: outFormat);
                    str1Format = outFormat;
                }

                if (!str2Format.StartsWith("yyyy"))
                {
                    str2 = ConvertROC(str2, inFormat: str2Format, outFormat: outFormat);
                    str2Format = outFormat;
                }

                if (!DateTime.TryParseExact(str1, str1Format, null, DateTimeStyles.None, out DateTime dt1))
                    return result;
                if (!DateTime.TryParseExact(str2, str2Format, null, DateTimeStyles.None, out DateTime dt2))
                    return result;
                result = DateTimeDiffTS(dt1, dt2);
            }
            catch (Exception) { }

            return result;
        }

        /// <summary>
        /// 計算兩時間的差值
        /// </summary>
        /// <remarks>dt2 >= dt1</remarks>
        public static TimeSpan DateTimeDiffTS(DateTime dt1, DateTime dt2)
        {
            TimeSpan ts = default;
            try
            {
                ts = dt2.Subtract(dt1);
            }
            catch (Exception) { }

            return ts;
        }

        /// <summary>
        /// 民國年 n 日後
        /// </summary>
        /// <param name="inDate"> 民國年</param>
        /// <param name="toAD">是否轉為西元年</param>
        /// <param name="inFormat">輸入民國年格式</param>
        /// <param name="outFormat">輸出格式</param>
        /// <returns>西元年或民國年</returns>
        public static string ROCNextDay(string inDate, int nDays = 1, bool toAD = false,
            string inFormat = "yyyMMdd", string outFormat = "yyyMMdd")
        {
            string result = string.Empty;
            try
            {
                var adStr = ConvertROC(inDate, true, inFormat, "yyyy/MM/dd HH:mm:ss");
                DateTime adDate = DateTime.Parse(adStr);
                result = ConvertAD(adDate.AddDays(nDays).ToString("yyyy/MM/dd HH:mm:ss"),
                   !toAD, "yyyy/MM/dd HH:mm:ss", outFormat);
            }
            catch (Exception) { }
            return result;
        }

        /// <summary>
        /// 比較兩字串日期時間值
        /// </summary>
        /// <returns>
        /// -1: str1 小於 str2,
        /// 0: str1 等於 str2,
        /// 1: str1 大於 str2,
        /// 99: datetime format error
        /// </returns>
        public static CompareResult CompareStrDateTime(string str1, string str2)
        {
            CompareResult result = CompareResult.FormatErr;
            DateTime dt1, dt2;

            try
            {
                str1 = str1.NullableToStr();
                str2 = str2.NullableToStr();
                dt1 = DateTime.Parse(str1);
                dt2 = DateTime.Parse(str2);
                result = (CompareResult)dt1.CompareTo(dt2);
            }
            catch (Exception) { }

            return result;
        }

        /// <summary>
        /// 取得星期幾
        /// </summary>
        public static string DayOfWeek(string dt, bool shortest = true)
        {
            string result = string.Empty;

            try
            {
                dt = dt.NullableToStr();

                if (shortest)
                    result = CultureInfo.GetCultureInfo("zh-TW").DateTimeFormat.GetShortestDayName(DateTime.Parse(dt).DayOfWeek);
                else
                    result = CultureInfo.GetCultureInfo("zh-TW").DateTimeFormat.GetDayName(DateTime.Parse(dt).DayOfWeek);
            }
            catch (Exception) { }

            return result;
        }

    }
}
