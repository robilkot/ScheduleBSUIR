using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ScheduleBSUIR.Models.Messaging
{
    public sealed class ScrollToIndex(int index) : ValueChangedMessage<int>(index);
}
