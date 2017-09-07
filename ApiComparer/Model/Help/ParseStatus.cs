using GalaSoft.MvvmLight;

namespace ApiComparer.Model.Help
{
    public class ParseStatus : ViewModelBase
    {
        private int _parsedApiCount;
        private int _parsedSuccessApiCount;
        private int _rateOfProgress;
        private int _totalApiCount;

        public int TotalApiCount
        {
            get { return _totalApiCount; }
            set
            {
                if (_totalApiCount != value)
                {
                    _totalApiCount = value;
                    RaisePropertyChanged();
                    SetRateOfProgress();
                }
            }
        }

        public int ParsedApiCount
        {
            get { return _parsedApiCount; }
            set
            {
                if (_parsedApiCount != value)
                {
                    _parsedApiCount = value;
                    RaisePropertyChanged();
                    SetRateOfProgress();
                }
            }
        }

        public int ParsedSuccessApiCount
        {
            get { return _parsedSuccessApiCount; }
            set
            {
                if (_parsedSuccessApiCount != value)
                {
                    _parsedSuccessApiCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        public int RateOfProgress
        {
            get { return _rateOfProgress; }
            set
            {
                if (_rateOfProgress != value)
                {
                    _rateOfProgress = value;
                    RaisePropertyChanged();
                }
            }
        }

        private void SetRateOfProgress()
        {
            RateOfProgress = TotalApiCount == 0 ? 0 : ParsedApiCount*100/TotalApiCount;
            ;
        }
    }
}