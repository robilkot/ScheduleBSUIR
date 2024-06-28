using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ScheduleBSUIR.Models.Messaging
{
    public sealed class TimetablePinnedMessage(TypedId? id) : ValueChangedMessage<TypedId?>(id);
}
