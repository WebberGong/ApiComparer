using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiComparer.Model.Common;
using ApiComparer.Model.Enum;

namespace ApiComparer.Model
{
    public class Response : States
    {
        private ResponseBody _body;
        private Header _header;
        private ResponseStatus _status;

        public Response(Api parent) : base(parent)
        {
        }

        public ResponseStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
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

        public ResponseBody Body
        {
            get { return _body; }
            set
            {
                _body = value;
                RaisePropertyChanged();
            }
        }

        public static async Task<Response> ParseFromTextBlockAsync(Api parent, string textBlock)
        {
            var response = new Response(parent);
            var lines = textBlock.Split(new[] {"\r\n"}, StringSplitOptions.None);
            var prevIsNull = false;
            var prevType = string.Empty;

            foreach (var text in lines)
            {
                if (text.StartsWith("Set-Cookie"))
                    continue;
                if (string.IsNullOrEmpty(text))
                {
                    prevIsNull = true;
                    continue;
                }

                if (prevIsNull && (prevType == nameof(response.Header)))
                {
                    var bodyText = textBlock.Substring(textBlock.IndexOf(text, StringComparison.Ordinal));
                    var body = await ResponseBody.ParseFromTextBlockAsync(response, bodyText);
                    if ((response.Body == null) && (body != null))
                    {
                        response.Body = body;
                        prevType = nameof(response.Body);
                        continue;
                    }
                }

                if (prevType == string.Empty)
                {
                    var status = await ResponseStatus.ParseFromTextLineAsync(response, text);
                    if ((response.Status == null) && (status != null))
                    {
                        response.Status = status;
                        prevType = nameof(response.Status);
                        continue;
                    }
                }

                if ((prevType == nameof(response.Status)) || (prevType == nameof(response.Header)))
                {
                    if (response.Header == null)
                        response.Header = new Header(response);
                    if (await response.Header.AppendAsync(text))
                        prevType = nameof(response.Header);
                }
            }

            SetDefaultValue(response);

            return response;
        }

        private static void SetDefaultValue(Response response)
        {
            if (response == null)
                return;
            if (response.Status == null)
            {
                response.Status = new ResponseStatus(response);
                response.Status.Protocol = new KeyValueState<string, string>(response.Status,
                    nameof(response.Status.Protocol), string.Empty, StateEnum.PropertyMissing);
                response.Status.Code = new KeyValueState<string, string>(response.Status, nameof(response.Status.Code),
                    string.Empty, StateEnum.PropertyMissing);
                response.Status.Text = new KeyValueState<string, string>(response.Status, nameof(response.Status.Text),
                    string.Empty, StateEnum.PropertyMissing);
            }
            if (response.Header == null)
                response.Header = new Header(response);
            if (response.Body == null)
                response.Body = new ResponseBody(response);
        }

        public static async Task CompareAsync(Response response1, Response response2)
        {
            IList<Task> tasks = new List<Task>();
            tasks.Add(ResponseStatus.CompareAsync(response1.Status, response2.Status, true));
            tasks.Add(Header.CompareAsync(response1.Header, response2.Header, true));
            tasks.Add(ResponseBody.CompareAsync(response1.Body, response2.Body, true));
            await Task.WhenAll(tasks);
        }
    }
}