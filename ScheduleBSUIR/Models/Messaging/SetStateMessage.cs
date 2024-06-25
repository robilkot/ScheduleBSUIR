using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ScheduleBSUIR.Models.Messaging
{
    sealed class SetStateMessage(string? state) : ValueChangedMessage<string?>(state);
}
