using ChatApp.Model.Utils;
using System.Windows;
using System.Windows.Controls;

namespace ChatApp.View
{
    public class MessageTemplateSelector : DataTemplateSelector
    {
        public required DataTemplate UserMessageTemplate { get; set; }
        public required DataTemplate ReceivedMessageTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is Message message)
            {
                return message.Direction == Message.ConnectionDirection.Outgoing ? UserMessageTemplate : ReceivedMessageTemplate;
            }
            return base.SelectTemplate(item, container);
        }
    }
}
