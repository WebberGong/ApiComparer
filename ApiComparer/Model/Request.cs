using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ApiComparer.Model.Common;

namespace ApiComparer.Model
{
    public class Request : States
    {
        private RequestAddress _address;
        private RequestBody _body;
        private Cookie _cookie;
        private Header _header;

        public Request(Api parent) : base(parent)
        {
        }

        public RequestAddress Address
        {
            get { return _address; }
            set
            {
                _address = value;
                RaisePropertyChanged();
            }
        }

        public Header Header
        {
            get { return _header; }
            set
            {
                _header = value;
                RaisePropertyChanged();
            }
        }

        public Cookie Cookie
        {
            get { return _cookie; }
            set
            {
                _cookie = value;
                RaisePropertyChanged();
            }
        }

        public RequestBody Body
        {
            get { return _body; }
            set
            {
                _body = value;
                RaisePropertyChanged();
            }
        }

        public static async Task<Request> ParseFromTextBlockAsync(Api parent, string textBlock, bool hasImageData,
            string boundary)
        {
            var request = new Request(parent);
            var lines = textBlock.Split(new[] {"\r\n"}, StringSplitOptions.None);
            var prevIsNull = false;
            var prevType = string.Empty;

            foreach (var text in lines)
            {
                if (string.IsNullOrEmpty(text))
                {
                    prevIsNull = true;
                    continue;
                }

                if (prevIsNull && ((prevType == nameof(request.Header)) || (prevType == nameof(request.Cookie))))
                {
                    var bodyBlock = textBlock.Substring(textBlock.IndexOf(text, StringComparison.Ordinal));
                    var body = await RequestBody.ParseFromTextBlockAsync(request, bodyBlock, hasImageData, boundary);
                    if ((request.Body == null) && (body != null))
                    {
                        request.Body = body;
                        prevType = nameof(request.Body);
                        continue;
                    }
                }

                if (prevType == string.Empty)
                {
                    var address = await RequestAddress.ParseFromTextLineAsync(text);
                    if ((request.Address == null) && (address != null))
                    {
                        request.Address = address;
                        if (address.Method == "GET")
                        {
                            var index = address.Url.IndexOf("?", StringComparison.Ordinal);
                            if (index > -1)
                            {
                                var getParameters = address.Url.Substring(index + 1);
                                if (RequestBody.RegexString.IsMatch(getParameters))
                                {
                                    var body = new RequestBody(request);
                                    var parameters = Regex.Split(getParameters, "&");
                                    foreach (var parameter in parameters)
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
                                    request.Body = body;
                                    prevType = nameof(request.Body);
                                }
                            }
                        }
                        prevType = nameof(request.Address);
                        continue;
                    }
                }

                if ((prevType == nameof(request.Address)) || (prevType == nameof(request.Header)))
                {
                    var cookie = await Cookie.ParseFromTextLineAsync(request, text);
                    if ((request.Cookie == null) && (cookie != null))
                    {
                        request.Cookie = cookie;
                        prevType = nameof(request.Cookie);
                        continue;
                    }
                }

                if ((prevType == nameof(request.Address)) || (prevType == nameof(request.Header)) ||
                    (prevType == nameof(request.Cookie)))
                {
                    if (request.Header == null)
                        request.Header = new Header(request);
                    if (await request.Header.AppendAsync(text))
                        prevType = nameof(request.Header);
                }
            }

            SetDefaultValue(request);

            return request;
        }

        private static void SetDefaultValue(Request request)
        {
            if (request?.Address == null)
                return;
            if (request.Header == null)
                request.Header = new Header(request);
            if (request.Cookie == null)
                request.Cookie = new Cookie(request);
            if (request.Body == null)
                request.Body = new RequestBody(request);
        }

        public static async Task CompareAsync(Request request1, Request request2)
        {
            IList<Task> tasks = new List<Task>();
            tasks.Add(Header.CompareAsync(request1.Header, request2.Header, true));
            tasks.Add(Cookie.CompareAsync(request1.Cookie, request2.Cookie, true));
            tasks.Add(RequestBody.CompareAsync(request1.Body, request2.Body, true));
            await Task.WhenAll(tasks);
        }
    }
}