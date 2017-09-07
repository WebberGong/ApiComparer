using System.Collections.Generic;
using ApiComparer.Model.Enum;
using GalaSoft.MvvmLight;

namespace ApiComparer.Model.Common
{
    public class States : ViewModelBase
    {
        private readonly IDictionary<string, StateEnum> _states;
        private bool _hasApiNotFoundState;
        private bool _hasPropertyMissingState;
        private bool _hasValueTypeNotMatched;
        private bool _hasValueUnequalState;
        private bool _isNormal;
        private States _parent;
        public static readonly IList<StateEnum> InvalidStates = new List<StateEnum>()
        {
            StateEnum.ApiNotFound,
            StateEnum.PropertyMissing,
            StateEnum.ValueTypeNotMatched,
            StateEnum.ValueUnequal
        };

        public States(States parent)
        {
            _states = new Dictionary<string, StateEnum>();
            _parent = parent;
            _isNormal = true;
        }

        public States Parent
        {
            get { return _parent; }
            set
            {
                lock (_states)
                {
                    if (_parent != value)
                    {
                        _parent = value;
                        RaisePropertyChanged();
                    }
                }
            }
        }

        public bool IsNormal
        {
            get { return _isNormal; }
            set
            {
                if (_isNormal != value)
                {
                    _isNormal = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool HasApiNotFoundState
        {
            get { return _hasApiNotFoundState; }
            set
            {
                if (_hasApiNotFoundState != value)
                {
                    _hasApiNotFoundState = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool HasValueUnequalState
        {
            get { return _hasValueUnequalState; }
            set
            {
                if (_hasValueUnequalState != value)
                {
                    _hasValueUnequalState = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool HasValueTypeNotMatched
        {
            get { return _hasValueTypeNotMatched; }
            set
            {
                if (_hasValueTypeNotMatched != value)
                {
                    _hasValueTypeNotMatched = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool HasPropertyMissingState
        {
            get { return _hasPropertyMissingState; }
            set
            {
                if (_hasPropertyMissingState != value)
                {
                    _hasPropertyMissingState = value;
                    RaisePropertyChanged();
                }
            }
        }

        public void AddOrUpdateState(StateEnum state)
        {
            lock (_states)
            {
                var key = state.ToString();
                if (!_states.ContainsKey(key))
                {
                    _states.Add(key, state);
                }
                else
                {
                    _states[key] = state;
                }
                _parent?.AddOrUpdateState(state);
            }
        }

        public void AddOrUpdateState<TKey, TValue>(KeyValueState<TKey, TValue> keyValueState)
        {
            lock (_states)
            {
                var key = keyValueState.Id.ToString();
                if (!_states.ContainsKey(key))
                {
                    _states.Add(key, keyValueState.State);
                }
                else
                {
                    _states[key] = keyValueState.State;
                }
                _parent?.AddOrUpdateState(keyValueState);
            }
        }

        public void UpdateStatus()
        {
            foreach (StateEnum item in InvalidStates)
            {
                var hasState = _states.Values.Contains(item);
                switch (item)
                {
                    case StateEnum.ApiNotFound:
                        HasApiNotFoundState = hasState;
                        break;
                    case StateEnum.PropertyMissing:
                        HasPropertyMissingState = hasState;
                        break;
                    case StateEnum.ValueTypeNotMatched:
                        HasValueTypeNotMatched = hasState;
                        break;
                    case StateEnum.ValueUnequal:
                        HasValueUnequalState = hasState;
                        break;
                    default:
                        break;
                }
            }
            IsNormal = !HasApiNotFoundState && !HasValueUnequalState && 
                !HasValueTypeNotMatched && !HasPropertyMissingState;
            lock (_states)
            {
                _parent?.UpdateStatus();
            }
        }
    }
}