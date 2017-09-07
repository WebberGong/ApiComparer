using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ApiComparer
{
    public class Settings
    {
        public static IList<string> UrlContainsKeys { get; set; }

        public static IList<Regex> SpecifiedUrlRegexes { get; set; }

        static Settings()
        {
            var keys = ConfigurationManager.AppSettings["UrlContainsKeys"];
            var keysArr = keys.Split(new char[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
            UrlContainsKeys = new List<string>();
            foreach (var key in keysArr)
            {
                if (!string.IsNullOrEmpty(key.Trim()))
                {
                    UrlContainsKeys.Add(key.Trim());
                }
            }

            var regexes = ConfigurationManager.AppSettings["SpecifiedUrlRegexes"];
            var regexArr = regexes.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            SpecifiedUrlRegexes = new List<Regex>();
            foreach (var regex in regexArr)
            {
                if (!string.IsNullOrEmpty(regex.Trim()))
                {
                    SpecifiedUrlRegexes.Add(new Regex(regex.Trim()));
                }
            }
        }

        public static bool IsSpecifiedUrl(string url, out Regex matchedRegex)
        {
            var isMatched = false;
            matchedRegex = null;
            foreach (var regex in Settings.SpecifiedUrlRegexes)
            {
                if (regex.IsMatch(url))
                {
                    isMatched = true;
                    matchedRegex = regex;
                    break;
                }
            }
            return isMatched;
        }

        public static bool IsUrlMatched(string url)
        {
            var isMatched = false;
            foreach (var key in Settings.UrlContainsKeys)
            {
                if (url.Contains(key))
                {
                    isMatched = true;
                    break;
                }
            }
            return isMatched;
        }
    }
}
