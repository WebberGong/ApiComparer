using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web;
using ApiComparer.Model.Common;

namespace ApiComparer.Model
{
    public class Api : States
    {
        private const string ImageDataStart = "Content-Transfer-Encoding: binary";

        private string _originalText;
        private Request _request;
        private Response _response;

        public Api(ApiList parent) : base(parent)
        {
        }

        public string OriginalText
        {
            get { return _originalText; }
            set
            {
                _originalText = value;
                RaisePropertyChanged();
            }
        }

        public Request Request
        {
            get { return _request; }
            set
            {
                _request = value;
                RaisePropertyChanged();
            }
        }

        public Response Response
        {
            get { return _response; }
            set
            {
                _response = value;
                RaisePropertyChanged();
            }
        }

        public static async Task<Api> ParseFromTextBlockAsync(ApiList parent, string textBlock)
        {
            if (string.IsNullOrEmpty(textBlock))
                return null;

            string boundary;
            var hasImageData = RemoveImageData(ref textBlock, out boundary);

            var api = new Api(parent);
            if (textBlock != null)
            {
                api.OriginalText = textBlock;
                textBlock = HttpUtility.UrlDecode(textBlock);
                var match = ResponseStatus.Regex.Match(textBlock);
                if (match.Success)
                {
                    api.Request =
                        await
                            Request.ParseFromTextBlockAsync(api, textBlock.Substring(0, match.Index), hasImageData,
                                boundary);
                    api.Response = await Response.ParseFromTextBlockAsync(api, textBlock.Substring(match.Index));
                }
            }
            return api;
        }

        public static async Task CompareAsync(Api api1, Api api2)
        {
            IList<Task> tasks = new List<Task>();
            tasks.Add(Request.CompareAsync(api1.Request, api2.Request));
            tasks.Add(Response.CompareAsync(api1.Response, api2.Response));
            await Task.WhenAll(tasks);
        }

        public static bool RemoveImageData(ref string textBlock, out string boundary)
        {
            boundary = string.Empty;
            var startIndex = textBlock.IndexOf(ImageDataStart, StringComparison.Ordinal);
            if (startIndex < 0)
                return false;
            var boundaryIndex = textBlock.IndexOf("boundary=\"", StringComparison.Ordinal);
            if (boundaryIndex < 0)
            {
                boundaryIndex = textBlock.IndexOf("boundary=", StringComparison.Ordinal);
                if (boundaryIndex < 0)
                    return false;
                boundary = textBlock.Substring(boundaryIndex + 9, 30);
            }
            else
            {
                boundary = textBlock.Substring(boundaryIndex + 10, 30);
            }
            var endIndex = textBlock.IndexOf($"--{boundary}", startIndex, StringComparison.Ordinal);
            if (endIndex < 0)
                return false;
            var temp = Utility.Trim(textBlock.Substring(startIndex + ImageDataStart.Length, endIndex - startIndex - ImageDataStart.Length));
            var jArr = Utility.TryParseToJArray(temp);
            var jObj = Utility.TryParseToJObject(temp);
            if (jArr == null && jObj == null)
            {
                textBlock = textBlock.Remove(startIndex +ImageDataStart.Length, endIndex - startIndex - ImageDataStart.Length).Insert(startIndex + ImageDataStart.Length, "\r\n");
            }
            return true;
        }
    }
}