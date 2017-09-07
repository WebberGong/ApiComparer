using System.Windows;
using ApiComparer.Model.Common;

namespace ApiComparer.DataTemplateSelector
{
    public class ValueDataTemplateSelector : System.Windows.Controls.DataTemplateSelector
    {
        public DataTemplate TextValueTemplate { get; set; } = null;
        public DataTemplate StateNodeValueTemplate { get; set; } = null;

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var state = item as KeyValueState<string, object>;
            if (state != null)
            {
                var value = state;
                if (value.Value is string)
                    return TextValueTemplate;
                if (value.Value is StateNode)
                    return StateNodeValueTemplate;
            }
            return null;
        }
    }
}