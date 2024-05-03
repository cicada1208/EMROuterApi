using System.Text.RegularExpressions;

namespace Lib.Utilities
{
    public static class CodecExUtil
    {
        /// <summary>
        /// Binary File to Base64String
        /// </summary>
        public static string TryFileToBase64String(this string filePath)
        {
            try
            {
                return Convert.ToBase64String(File.ReadAllBytes(filePath));
            }
            catch (Exception)
            {
                return filePath;
            }
        }

        /// <summary>
        /// Base64String to Byte Array
        /// </summary>
        /// <param name="input">Base64String</param>
        /// <param name="output">Byte Array</param>
        public static bool TryBase64StringToByteArray(this string input, out byte[]? output)
        {
            output = null;
            try
            {
                output = Convert.FromBase64String(input);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// 判斷是否為 Base64String
        /// </summary>
        public static bool IsBase64String(this object base64)
        {
            string data = base64.NullableToStr();
            return data.Length % 4 == 0 && Regex.IsMatch(data, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
        }
    }
}
