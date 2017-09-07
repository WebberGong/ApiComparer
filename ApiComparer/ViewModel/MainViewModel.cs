using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using ApiComparer.Model;
using ApiComparer.Model.Help;
using FontAwesome.Sharp;
using FontAwesomeWPF;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace ApiComparer.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private ApiDictionary _apiDictionary1;
        private ApiDictionary _apiDictionary2;
        private string _filePath1 = "文本1";
        private string _filePath2 = "文本2";
        private bool _isBusy;
        private ParseStatus _parseStatus1 = new ParseStatus();
        private ParseStatus _parseStatus2 = new ParseStatus();
        private string _step;

        public MainViewModel()
        {
            RunCommand = new RelayCommand(ExecuteRunCommand);
        }

        public ImageSource Icon { get; } = IconHelper.ToImageSource(Fa.Bolt, new SolidColorBrush(Colors.Gold), 50);

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                RaisePropertyChanged();
            }
        }

        public string Step
        {
            get { return _step; }
            set
            {
                _step = value;
                RaisePropertyChanged();
            }
        }

        public ApiDictionary ApiDictionary1
        {
            get { return _apiDictionary1; }
            set
            {
                _apiDictionary1 = value;
                RaisePropertyChanged();
            }
        }

        public ApiDictionary ApiDictionary2
        {
            get { return _apiDictionary2; }
            set
            {
                _apiDictionary2 = value;
                RaisePropertyChanged();
            }
        }

        public string FilePath1
        {
            get { return _filePath1; }
            set
            {
                _filePath1 = value;
                RaisePropertyChanged();
            }
        }

        public string FilePath2
        {
            get { return _filePath2; }
            set
            {
                _filePath2 = value;
                RaisePropertyChanged();
            }
        }

        public ParseStatus ParseStatus1
        {
            get { return _parseStatus1; }
            set
            {
                _parseStatus1 = value;
                RaisePropertyChanged();
            }
        }

        public ParseStatus ParseStatus2
        {
            get { return _parseStatus2; }
            set
            {
                _parseStatus2 = value;
                RaisePropertyChanged();
            }
        }

        public ICommand RunCommand { get; private set; }

        private async void ExecuteRunCommand()
        {
            IsBusy = true;
            Step = "解析";
            await Parse();
            Step = "对比";
            await Compare();
            IsBusy = false;
        }

        private async Task Parse()
        {
            ClearParseData();

            IList<Task> tasks = new List<Task>();
            if (File.Exists(FilePath1))
                tasks.Add(Parse(
                        FilePath1,
                        apiDictionary => { ApiDictionary1 = apiDictionary; },
                        (totalCount, parsedCount, parsedSuccessCount) =>
                        {
                            ParseStatus1.TotalApiCount = totalCount;
                            ParseStatus1.ParsedApiCount = parsedCount;
                            ParseStatus1.ParsedSuccessApiCount = parsedSuccessCount;
                        })
                );

            if (File.Exists(FilePath2))
                tasks.Add(Parse(
                        FilePath2,
                        apiDictionary => { ApiDictionary2 = apiDictionary; },
                        (totalCount, parsedCount, parsedSuccessCount) =>
                        {
                            ParseStatus2.TotalApiCount = totalCount;
                            ParseStatus2.ParsedApiCount = parsedCount;
                            ParseStatus2.ParsedSuccessApiCount = parsedSuccessCount;
                        })
                );

            await Task.WhenAll(tasks);
        }

        private async Task Parse(string filePath, Action<ApiDictionary> parseCompleted,
            Action<int, int, int> parseProgressUpdate)
        {
            var reader = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath));
            var allText = Utility.Trim(reader.ReadToEnd()).Trim('-');
            var apis = Regex.Split(allText, "^------------------------------------------------------------------\r\n",
                RegexOptions.Multiline);
            var apiDictionary = new ApiDictionary();
            var parsedCount = 0;
            var parsedSuccessCount = 0;
            foreach (var item in apis)
            {
                var apiList = new ApiList(apiDictionary);
                var api = await Api.ParseFromTextBlockAsync(apiList, Utility.Trim(item));
                if (api?.Request?.Address != null && Settings.IsUrlMatched(api.Request.Address.Url))
                {
                    var key = new ApiKey
                    {
                        Group = api.Request.Address.Group,
                        Method = api.Request.Address.Method
                    };
                    if (!apiDictionary.Content.ContainsKey(key))
                        apiDictionary.Content.Add(key, apiList);
                    apiDictionary.Content[key].Content.Add(api);
                    parsedSuccessCount++;
                }
                parsedCount++;
                parseProgressUpdate?.Invoke(apis.Length, parsedCount, parsedSuccessCount);
            }
            parseCompleted?.Invoke(apiDictionary);
        }

        private async Task Compare()
        {
            IList<Task> tasks = new List<Task>();
            tasks.Add(ApiDictionary.CompareAsync(ApiDictionary1, ApiDictionary2));
            tasks.Add(ApiDictionary.CompareAsync(ApiDictionary2, ApiDictionary1));
            await Task.WhenAll(tasks);
        }

        private void ClearParseData()
        {
            ApiDictionary1 = null;
            ParseStatus1 = new ParseStatus();
            ApiDictionary2 = null;
            ParseStatus2 = new ParseStatus();
        }
    }
}