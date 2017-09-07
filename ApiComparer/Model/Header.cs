using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ApiComparer.Model.Common;
using ApiComparer.Model.Enum;

namespace ApiComparer.Model
{
    public class Header : States
    {
        public static readonly Regex Regex = new Regex("^([\\S]+):\\s+([\\s\\S]*)$");

        private IDictionary<string, KeyValueState<string, string>> _content =
            new Dictionary<string, KeyValueState<string, string>>();

        public Header(States parent) : base(parent)
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

        public bool Append(string textLine)
        {
            var matches = Regex.Matches(textLine);
            if ((matches.Count == 1) && (matches[0].Groups.Count == 3))
            {
                var key = matches[0].Groups[1].Value;
                var value = matches[0].Groups[2].Value;
                if (Content.ContainsKey(key))
                    Console.WriteLine($@"Header ({key} : {value}) is duplicated");
                else
                    Content.Add(key, new KeyValueState<string, string>(this, key, value.Trim()));
                return true;
            }
            return false;
        }

        public async Task<bool> AppendAsync(string textLine)
        {
            return await Task.Run(() => Append(textLine));
        }

        public static void Compare(Header header1, Header header2, bool isCompareValue)
        {
            foreach (var item1 in header1.Content)
                if (header2.Content.ContainsKey(item1.Key))
                {
                    item1.Value.State = StateEnum.PropertyExisted;
                    if (isCompareValue)
                        item1.Value.State = item1.Value.Value != header2.Content[item1.Key].Value ? StateEnum.ValueUnequal : StateEnum.ValueEqual;
                }
                else
                    item1.Value.State = StateEnum.PropertyMissing;
        }

        public static async Task CompareAsync(Header header1, Header header2, bool isCompareValue)
        {
            IList<Task> tasks = new List<Task>();
            tasks.Add(Task.Run(() => Compare(header1, header2, isCompareValue)));
            tasks.Add(Task.Run(() => Compare(header2, header1, isCompareValue)));
            await Task.WhenAll(tasks);
        }
    }
}