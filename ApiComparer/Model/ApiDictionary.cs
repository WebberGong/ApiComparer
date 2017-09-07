using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ApiComparer.Model.Common;
using ApiComparer.Model.Enum;
using ApiComparer.Model.Help;

namespace ApiComparer.Model
{
    public class ApiDictionary : States
    {
        private IDictionary<ApiKey, ApiList> _content = new Dictionary<ApiKey, ApiList>();

        public ApiDictionary() : base(null)
        {
        }

        public IDictionary<ApiKey, ApiList> Content
        {
            get { return _content; }
            set
            {
                _content = value;
                RaisePropertyChanged();
            }
        }

        public static async Task CompareAsync(ApiDictionary apiDictionary1, ApiDictionary apiDictionary2)
        {
            if ((apiDictionary1 == null) || (apiDictionary2 == null))
                return;
            IList<Task> tasks = new List<Task>();
            foreach (var apis1 in apiDictionary1.Content)
            {
                if (apiDictionary2.Content.ContainsKey(apis1.Key))
                    tasks.Add(ApiList.CompareAsync(apis1.Value, apiDictionary2.Content[apis1.Key]));
                else
                {
                    apis1.Value.AddOrUpdateState(StateEnum.ApiNotFound);
                    apis1.Value.UpdateStatus();
                }
            }
            await Task.WhenAll(tasks);
        }
    }
}