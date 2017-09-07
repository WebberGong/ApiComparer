using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ApiComparer.Model.Common;
using ApiComparer.Model.Enum;

namespace ApiComparer.Model
{
    public class RequestBody : States
    {
        public const string SpecialParameter = "signed_body";

        public static readonly Regex RegexJson = new Regex("^signed_body=([0-9a-zA-Z]+\\.)[\\s\\S]*$");
        public static readonly Regex RegexString = new Regex("^([\\S]+=[\\s\\S]*&)*([\\S]+=[\\s\\S]*&?)$");

        public static readonly Regex RegexImageDataKey =
            new Regex("Content-Disposition: form-data; (name=\"\\w+\"(; filename=\"\\S+\")?)");

        public static readonly Regex RegexImageDataValue = new Regex("([0-9a-zA-Z]+\\.)[\\s\\S]*$");

        private IDictionary<string, KeyValueState<string, object>> _content =
            new Dictionary<string, KeyValueState<string, object>>();

        public RequestBody(Request parent) : base(parent)
        {
        }

        public IDictionary<string, KeyValueState<string, object>> Content
        {
            get { return _content; }
            set
            {
                _content = value;
                RaisePropertyChanged();
            }
        }

        public static RequestBody ParseFromTextBlock(Request parent, string textBlock, bool hasImageData,
            string boundary)
        {
            var body = new RequestBody(parent);
            var matches = RegexJson.Matches(textBlock);
            if ((matches.Count == 1) && (matches[0].Groups.Count == 2))
                textBlock = textBlock.Replace(matches[0].Groups[1].Value, string.Empty);
            if (RegexString.IsMatch(textBlock))
            {
                var parameters = Regex.Split(textBlock, "&");
                foreach (var parameter in parameters)
                {
                    if (parameter.StartsWith(SpecialParameter + "="))
                    {
                        body.Content.Add(SpecialParameter,
                            new KeyValueState<string, object>(body, SpecialParameter,
                                Utility.TryParseToObject(parameter.Substring((SpecialParameter + "=").Length), body)));
                    }
                    else
                    {
                        var keyValues = Regex.Split(parameter, "=");
                        if (keyValues.Length == 2)
                        {
                            if (body.Content.ContainsKey(keyValues[0]))
                            {
                                Console.WriteLine($@"Parameter {keyValues[0]} is duplicated in get request");
                            }
                            else
                            {
                                body.Content.Add(keyValues[0],
                                    new KeyValueState<string, object>(body, keyValues[0],
                                        Utility.TryParseToObject(keyValues[1], body)));
                            }
                        }
                    }
                }
                return body;
            }
            Utility.TryParseToObject(textBlock, body,
                jObj => { body.Content.Add("Content", new KeyValueState<string, object>(body, "Content", jObj)); },
                text =>
                {
                    if (hasImageData && !string.IsNullOrEmpty(boundary))
                    {
                        var parameters = text.Split(new[] { $"--{boundary}" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var item in parameters)
                        {
                            if (item.StartsWith("--"))
                                continue;
                            var match = RegexImageDataKey.Match(item);
                            if (match.Success && (match.Groups.Count == 3))
                            {
                                var key = match.Groups[1].Value;
                                var value =
                                    Utility.Trim(
                                        item.Substring(item.IndexOf("\r\n", match.Groups[0].Index,
                                            StringComparison.Ordinal)));
                                var valueMatche = RegexImageDataValue.Match(value);
                                if (valueMatche.Success && (valueMatche.Groups.Count == 2))
                                    value = value.Replace(valueMatche.Groups[1].Value, string.Empty);
                                body.Content.Add(key,
                                    new KeyValueState<string, object>(body, key, Utility.TryParseToObject(value, body)));
                            }
                        }
                    }
                    else
                    {
                        body.Content.Add("Content", new KeyValueState<string, object>(body, "Content", text.Trim()));
                    }
                });
            return body;
        }

        public static async Task<RequestBody> ParseFromTextBlockAsync(Request parent, string textBlock,
            bool hasImageData, string boundary)
        {
            return await Task.Run(() => ParseFromTextBlock(parent, textBlock, hasImageData, boundary));
        }

        public static async void Compare(RequestBody body1, RequestBody body2, bool isCompareValue)
        {
            foreach (var item1 in body1.Content)
                if (body2.Content.ContainsKey(item1.Key))
                {
                    item1.Value.State = StateEnum.PropertyExisted;
                    if (isCompareValue)
                    {
                        var itemStr1 = item1.Value.Value as string;
                        var itemStr2 = body2.Content[item1.Key].Value as string;
                        if ((itemStr1 != null) && (itemStr2 != null))
                        {
                            item1.Value.State = itemStr1 != (string)body2.Content[item1.Key].Value ? StateEnum.ValueUnequal : StateEnum.ValueEqual;
                        }
                        else if (item1.Value.Value is StateNode && body2.Content[item1.Key].Value is StateNode)
                        {
                            item1.Value.State = StateEnum.ValueEqual;
                            var node1 = (StateNode)item1.Value.Value;
                            var node2 = (StateNode)body2.Content[item1.Key].Value;
                            await StateNode.CompareAsync(node1, node2, true);
                        }
                        else
                        {
                            item1.Value.State = StateEnum.ValueUnequal;
                        }
                    }
                }
                else
                    item1.Value.State = StateEnum.PropertyMissing;
        }

        public static async Task CompareAsync(RequestBody body1, RequestBody body2, bool isCompareValue)
        {
            IList<Task> tasks = new List<Task>();
            tasks.Add(Task.Run(() => Compare(body1, body2, isCompareValue)));
            tasks.Add(Task.Run(() => Compare(body2, body1, isCompareValue)));
            await Task.WhenAll(tasks);
        }
    }
}