using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ApiComparer.Model.Enum;
using Newtonsoft.Json.Linq;

namespace ApiComparer.Model.Common
{
    public class StateNode : States
    {
        private IList<StateNode> _childNodes;

        private Guid _id;

        private bool _isLeafNode;

        private StateNode _parentNode;

        private JTokenType _type;

        private KeyValueState<string, string> _value;

        public StateNode(States parent, StateNode parentNode, KeyValueState<string, string> value, JTokenType type,
            bool isLeafNode)
            : base(parent)
        {
            _id = Guid.NewGuid();
            _parentNode = parentNode;
            _value = value;
            _type = type;
            _isLeafNode = isLeafNode;
            _childNodes = new List<StateNode>();

            _value.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "State" && InvalidStates.Contains(_value.State))
                    foreach (var child in _childNodes)
                        child.Value.State = _value.State;
            };
        }

        public Guid Id
        {
            get { return _id; }
            set
            {
                _id = value;
                RaisePropertyChanged();
            }
        }

        public StateNode ParentNode
        {
            get { return _parentNode; }
            set
            {
                _parentNode = value;
                RaisePropertyChanged();
            }
        }

        public KeyValueState<string, string> Value
        {
            get { return _value; }
            set
            {
                _value = value;
                RaisePropertyChanged();
            }
        }

        public JTokenType Type
        {
            get { return _type; }
            set
            {
                _type = value;
                RaisePropertyChanged();
            }
        }

        public bool IsLeafNode
        {
            get { return _isLeafNode; }
            set
            {
                _isLeafNode = value;
                RaisePropertyChanged();
            }
        }

        public IList<StateNode> ChildNodes
        {
            get { return _childNodes; }
            set
            {
                _childNodes = value;
                RaisePropertyChanged();
            }
        }

        public static void Compare(StateNode node1, StateNode node2, bool isCompareValue)
        {
            if (node1.Type == JTokenType.Null && node2.Type == JTokenType.Null)
            {
                node1.Value.State = StateEnum.ValueEqual;
                return;
            }
            if (node1.Type != JTokenType.Null && node2.Type != JTokenType.Null)
            {
                if (node1.Type != node2.Type)
                {
                    node1.Value.State = StateEnum.ValueTypeNotMatched;
                    return;
                }
                node1.Value.State = StateEnum.ValueTypeMatched;
            }
            if (node1.IsLeafNode && node2.IsLeafNode)
            {
                if (isCompareValue)
                {
                    node1.Value.State = node1.Value.Value != node2.Value.Value ? StateEnum.ValueUnequal : StateEnum.ValueEqual;
                }
            }
            else
            {
                foreach (var childNode1 in node1.ChildNodes)
                {
                    var childNode2 = node2.ChildNodes.FirstOrDefault(x => x.Value.Key == childNode1.Value.Key);
                    if (childNode2 != null)
                    {
                        childNode1.Value.State = StateEnum.PropertyExisted;
                        Compare(childNode1, childNode2, isCompareValue);
                    }
                    else
                        childNode1.Value.State = StateEnum.PropertyMissing;
                }
            }
        }

        public static async Task CompareAsync(StateNode node1, StateNode node2, bool isCompareValue)
        {
            IList<Task> tasks = new List<Task>();
            tasks.Add(Task.Run(() => Compare(node1, node2, isCompareValue)));
            tasks.Add(Task.Run(() => Compare(node2, node1, isCompareValue)));
            await Task.WhenAll(tasks);
        }
    }
}