using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ApiComparer.Model.Common;
using ApiComparer.Model.Enum;

namespace ApiComparer.Model
{
    public class ResponseStatus : States
    {
        public static readonly Regex Regex = new Regex("(HTTP/[0-9\\.]+)\\s+([0-9]+)\\s+(\\S+)");
        private KeyValueState<string, string> _code;
        private KeyValueState<string, string> _protocol;
        private KeyValueState<string, string> _text;

        public ResponseStatus(Response parent) : base(parent)
        {
        }

        public KeyValueState<string, string> Protocol
        {
            get { return _protocol; }
            set
            {
                _protocol = value;
                RaisePropertyChanged();
            }
        }

        public KeyValueState<string, string> Code
        {
            get { return _code; }
            set
            {
                _code = value;
                RaisePropertyChanged();
            }
        }

        public KeyValueState<string, string> Text
        {
            get { return _text; }
            set
            {
                _text = value;
                RaisePropertyChanged();
            }
        }

        public static ResponseStatus ParseFromTextLine(Response parent, string textLine)
        {
            var matches = Regex.Matches(textLine);
            if ((matches.Count == 1) && (matches[0].Groups.Count == 4))
            {
                var status = new ResponseStatus(parent);
                status.Protocol = new KeyValueState<string, string>(status, nameof(Protocol),
                    matches[0].Groups[1].Value.Trim());
                status.Code = new KeyValueState<string, string>(status, nameof(Code), matches[0].Groups[2].Value.Trim());
                status.Text = new KeyValueState<string, string>(status, nameof(Text), matches[0].Groups[3].Value.Trim());
                return status;
            }
            return null;
        }

        public static async Task<ResponseStatus> ParseFromTextLineAsync(Response parent, string textLine)
        {
            return await Task.Run(() => ParseFromTextLine(parent, textLine));
        }

        public static void Compare(ResponseStatus status1, ResponseStatus status2, bool isCompareValue)
        {
            if (isCompareValue && Validate(status1) && Validate(status2))
            {
                status1.Protocol.State = status1.Protocol.Value != status2.Protocol.Value ? StateEnum.ValueUnequal : StateEnum.ValueEqual;
                status2.Protocol.State = status1.Protocol.Value != status2.Protocol.Value ? StateEnum.ValueUnequal : StateEnum.ValueEqual;

                status1.Code.State = status1.Code.Value != status2.Code.Value ? StateEnum.ValueUnequal : StateEnum.ValueEqual;
                status2.Code.State = status1.Code.Value != status2.Code.Value ? StateEnum.ValueUnequal : StateEnum.ValueEqual;

                status1.Text.State = status1.Text.Value != status2.Text.Value ? StateEnum.ValueUnequal : StateEnum.ValueEqual;
                status2.Text.State = status1.Text.Value != status2.Text.Value ? StateEnum.ValueUnequal : StateEnum.ValueEqual;
            }
        }

        public static async Task CompareAsync(ResponseStatus status1, ResponseStatus status2, bool isCompareValue)
        {
            await Task.Run(() => Compare(status1, status2, isCompareValue));
        }

        private static bool Validate(ResponseStatus status)
        {
            var isValid = true;
            if (status.Protocol != null && string.IsNullOrEmpty(status.Protocol.Value))
            {
                isValid = false;
                status.Protocol.State = string.IsNullOrEmpty(status.Protocol.Value) ? StateEnum.PropertyMissing : StateEnum.PropertyExisted;
            }
            if (status.Code != null && string.IsNullOrEmpty(status.Code.Value))
            {
                isValid = false;
                status.Code.State = string.IsNullOrEmpty(status.Code.Value) ? StateEnum.PropertyMissing : StateEnum.PropertyExisted;
            }
            if (status.Text != null && string.IsNullOrEmpty(status.Text.Value))
            {
                isValid = false;
                status.Text.State = string.IsNullOrEmpty(status.Text.Value) ? StateEnum.PropertyMissing : StateEnum.PropertyExisted;
            }
            return isValid;
        }
    }
}