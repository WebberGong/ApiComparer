using System;
using ApiComparer.Model.Common;
using ApiComparer.Model.Enum;
using GalaSoft.MvvmLight;

namespace ApiComparer.Model.Base
{
    public class StateBase : ViewModelBase
    {
        private StateEnum _state;

        public StateBase(StateEnum state)
        {
            _state = state;
        }

        public StateEnum State
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = _state > value ? _state : value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}