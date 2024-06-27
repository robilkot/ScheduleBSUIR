using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ScheduleBSUIR.Models.Messaging
{
    public sealed class TimetableFavoritedMessage(TypedId id) : ValueChangedMessage<TypedId>(id);
}
