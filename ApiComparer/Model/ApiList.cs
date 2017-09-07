using System.Collections.Generic;
using System.Threading.Tasks;
using ApiComparer.Model.Common;

namespace ApiComparer.Model
{
    public class ApiList : States
    {
        private IList<Api> _content = new List<Api>();

        public ApiList(ApiDictionary parent) : base(parent)
        {
        }

        public IList<Api> Content
        {
            get { return _content; }
            set
            {
                _content = value;
                RaisePropertyChanged();
            }
        }

        public static async Task CompareAsync(ApiList apiList1, ApiList apiList2)
        {
            if ((apiList1 == null) || (apiList2 == null))
                return;
            IList<Task> tasks = new List<Task>();
            foreach (var api1 in apiList1.Content)
                foreach (var api2 in apiList2.Content)
                    tasks.Add(Api.CompareAsync(api1, api2));
            await Task.WhenAll(tasks);
        }
    }
}