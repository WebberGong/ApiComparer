using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ApiComparer.Model.Common;
using ApiComparer.Model.Enum;

namespace ApiComparer.Model
{
    public class Cookie : States
    {
        private IDictionary<string, KeyValueState<string, string>> _content =
            new Dictionary<string, KeyValueState<string, string>>();

        public Cookie(Request parent) : base(parent)
        {
        }

        public IDictionary<string, KeyValueState<string, string>> Content
        {
            get { return _content; }
            set
            {
                _content = value;
                RaisePropertyChanged();
            }
        }

        public static Cookie ParseFromTextLine(Request parent, string textLine)
        {
            if (textLine.StartsWith("Cookie: "))
            {
                var cookie = new Cookie(parent);
                var parameters = Regex.Split(textLine.Remove(0, "Cookie: ".Length), "; ");
                foreach (var parameter in parameters)
                {
                    var keyValues = Regex.Split(parameter, "=");
                    if (keyValues.Length == 2)
                    {
                        if (cookie.Content.ContainsKey(keyValues[0]))
                        {
                            Console.WriteLine($@"Cookie {keyValues[0]} is duplicated in get request");
                        }
                        else
                        {
                            cookie.Content.Add(keyValues[0],
                                new KeyValueState<string, string>(cookie, keyValues[0], keyValues[1].Trim()));
                        }
                    }
                }
                return cookie;
            }
            return null;
        }

        public static async Task<Cookie> ParseFromTextLineAsync(Request parent, string textLine)
        {
            return await Task.Run(() => ParseFromTextLine(parent, textLine));
        }

        public static void Compare(Cookie cookie1, Cookie cookie2, bool isCompareValue)
        {
            foreach (var item1 in cookie1.Content)
            {
                if (cookie2.Content.ContainsKey(item1.Key))
                {
                    item1.Value.State = StateEnum.PropertyExisted;
                    if (isCompareValue)
                        item1.Value.State = item1.Value.Value != cookie2.Content[item1.Key].Value
                            ? StateEnum.ValueUnequal
                            : StateEnum.ValueEqual;
                }
                else
                    item1.Value.State = StateEnum.PropertyMissing;
            }
        }

        public static async Task CompareAsync(Cookie cookie1, Cookie cookie2, bool isCompareValue)
        {
            IList<Task> tasks = new List<Task>();
            tasks.Add(Task.Run(() => Compare(cookie1, cookie2, isCompareValue)));
            tasks.Add(Task.Run(() => Compare(cookie2, cookie1, isCompareValue)));
            await Task.WhenAll(tasks);
        }
    }
}