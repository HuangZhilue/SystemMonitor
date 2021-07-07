using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;

namespace SystemMonitor.Helper
{
    public static class CommonHelper
    {
        public static string ReplaceReg(this string str, string reg, string newStr = "")
        {
            var regex = new Regex(reg);
            return regex.Replace(str, newStr);
        }

        public static bool IsNull(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// 将字符串转换为整数
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int ToInt(this object str)
        {
            var result = 0;

            if (str == null || int.TryParse(str.ToString(), out result))
            {
                return result;
            }

            return str.ToString().GetInt();
        }

        public static string IntToString(int i)
        {
            return i == 0 ? "" : i.ToString();
        }

        public static int GetInt(this string str)
        {
            var regex = new Regex(@"(\d)+");
            if (!regex.IsMatch(str))
            {
                return 0;
            }

            var result = regex.Matches(str)[0].Value;
            return ToInt(result);
        }

        public static decimal ToDecimal(this object obj)
        {
            if (obj == null)
                return 0;

            if (!decimal.TryParse(obj.ToString(), out var d))
                d = 0;

            return d;
        }

        public static string DateTimeToString(this DateTime d)
        {
            return d.ToString("ddMMMyy", DateTimeFormatInfo.InvariantInfo);
        }

        public static DateTime StringToDateTime(this string d)
        {
            if (DateTime.TryParseExact(d, "ddMMMyy", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None,
                    out var d1)
                || DateTime.TryParseExact(d, "yyyyMMddHHmmss", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out d1)
                || DateTime.TryParseExact(d, "yyyyMMddHHmm", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out d1)
                || DateTime.TryParseExact(d, "yyyyMMdd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out d1))
                return d1;
            return d1;
        }

        public static string BoolToEnglish(this bool b)
        {
            return b ? "yes" : "false";
        }

        public static string SubStringFromKey(this string str, string key, string endKey)
        {
            var startIndex = 0;
            if (!string.IsNullOrEmpty(str))
            {
                startIndex = str.IndexOf(key, StringComparison.Ordinal);
            }

            if (str != null)
            {
                var endIndex = str.Length;
                if (startIndex == -1)
                {
                    return str;
                }

                startIndex += key.Length;
                if (!string.IsNullOrEmpty(endKey))
                {
                    var index = str.IndexOf(endKey, StringComparison.Ordinal);
                    endIndex = index == -1 ? str.Length : index;
                }

                return str[startIndex..(endIndex - startIndex)];
            }

            return null;
        }

        public static void RunInBackground(this Action action)
        {
            Application.Current.Dispatcher.Invoke(action, DispatcherPriority.Background);
        }
    }
}