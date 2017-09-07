using System;
using ApiComparer.Model.Base;
using ApiComparer.Model.Enum;

namespace ApiComparer.Model.Common
{
    public class KeyValueState<TKey, TValue> : StateBase
    {
        private TKey _key;

        private TValue _value;

        public KeyValueState(States parent, TKey key, TValue value, StateEnum state = StateEnum.Normal) : base(state)
        {
            Id = Guid.NewGuid();
            _key = key;
            _value = value;

            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "State")
                {
                    parent.AddOrUpdateState(this);
                    parent.UpdateStatus();
                }
            };
        }

        public Guid Id { get; }

        public TKey Key
        {
            get { return _key; }
            set
            {
                _key = value;
                RaisePropertyChanged();
            }
        }

        public TValue Value
        {
            get { return _value; }
            set
            {
                _value = value;
                RaisePropertyChanged();
            }
        }
    }
}