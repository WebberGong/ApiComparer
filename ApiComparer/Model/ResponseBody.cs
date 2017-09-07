using System.Collections.Generic;
using System.Threading.Tasks;
using ApiComparer.Model.Common;
using ApiComparer.Model.Enum;

namespace ApiComparer.Model
{
    public class ResponseBody : States
    {
        private KeyValueState<string, object> _content;

        public ResponseBody(Response parent) : base(parent)
        {
        }

        public KeyValueState<string, object> Content
        {
            get { return _content; }
            set
            {
                _content = value;
                RaisePropertyChanged();
            }
        }

        public static ResponseBody ParseFromTextBlock(Response parent, string textBlock)
        {
            var body = new ResponseBody(parent);
            body.Content = new KeyValueState<string, object>(body, "Content", Utility.TryParseToObject(textBlock, body));
            return body;
        }

        public static async Task<ResponseBody> ParseFromTextBlockAsync(Response parent, string textBlock)
        {
            return await Task.Run(() => ParseFromTextBlock(parent, textBlock));
        }

        public static async void Compare(ResponseBody body1, ResponseBody body2, bool isCompareValue)
        {
            var itemStr1 = body1.Content?.Value as string;
            var itemStr2 = body2.Content?.Value as string;
            if ((itemStr1 != null) && (itemStr2 != null))
            {
                if (isCompareValue)
                    body1.Content.State = itemStr1 != itemStr2 ? StateEnum.ValueUnequal : StateEnum.ValueEqual;
            }
            else if (body1.Content?.Value is StateNode && body2.Content?.Value is StateNode)
            {
                if (isCompareValue)
                {
                    var node1 = (StateNode)body1.Content.Value;
                    var node2 = (StateNode)body2.Content.Value;
                    await StateNode.CompareAsync(node1, node2, true);
                }
            }
            else
            {
                if (body1.Content != null) body1.Content.State = StateEnum.ValueUnequal;
            }
        }

        public static async Task CompareAsync(ResponseBody body1, ResponseBody body2, bool isCompareValue)
        {
            IList<Task> tasks = new List<Task>();
            tasks.Add(Task.Run(() => Compare(body1, body2, isCompareValue)));
            tasks.Add(Task.Run(() => Compare(body2, body1, isCompareValue)));
            await Task.WhenAll(tasks);
        }
    }
}