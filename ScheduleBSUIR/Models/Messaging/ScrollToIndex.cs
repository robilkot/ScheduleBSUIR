using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ScheduleBSUIR.Models.Messaging
{
    sealed class ScrollToIndex(int index) : ValueChangedMessage<int>(index);
}
