using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace ApiComparer.Model
{
    public class RequestAddress : ViewModelBase
    {
        public static readonly Regex Regex =
            new Regex("^(POST|GET|PUT|DELETE)\\s+((http|ftp|https)://\\S+)\\s+(HTTP/[0-9\\.]+)$");

        private string _method;
        private string _protocol;
        private string _type;
        private string _url;
        private string _group;

        public string Method
        {
            get { return _method; }
            set
            {
                _method = value;
                RaisePropertyChanged();
            }
        }

        public string Group
        {
            get { return _group; }
            set
            {
                _group = value;
                RaisePropertyChanged();
            }
        }

        public string Url
        {
            get { return _url; }
            set
            {
                _url = value;
                var temp = _url;
                if (Method == "GET")
                {
                    var index = Url.IndexOf("?", StringComparison.Ordinal);
                    if (index > -1)
                    {
                        Group = Url.Substring(0, index);
                        temp = Group;
                    }
                }
                Regex matchedRegex;
                Group = Settings.IsSpecifiedUrl(temp, out matchedRegex) ? matchedRegex.ToString() : temp;
                RaisePropertyChanged();
            }
        }

        public string Type
        {
            get { return _type; }
            set
            {
                _type = value;
                RaisePropertyChanged();
            }
        }

        public string Protocol
        {
            get { return _protocol; }
            set
            {
                _protocol = value;
                RaisePropertyChanged();
            }
        }

        public static RequestAddress ParseFromTextLine(string textLine)
        {
            var matches = Regex.Matches(textLine);
            if ((matches.Count == 1) && (matches[0].Groups.Count == 5))
                return new RequestAddress
                {
                    Method = matches[0].Groups[1].Value,
                    Url = matches[0].Groups[2].Value,
                    Type = matches[0].Groups[3].Value,
                    Protocol = matches[0].Groups[4].Value
                };
            return null;
        }

        public static async Task<RequestAddress> ParseFromTextLineAsync(string textLine)
        {
            return await Task.Run(() => ParseFromTextLine(textLine));
        }
    }
}