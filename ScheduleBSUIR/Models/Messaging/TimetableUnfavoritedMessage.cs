using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ScheduleBSUIR.Models.Messaging
{
    public sealed class TimetableUnfavoritedMessage(TypedId id) : ValueChangedMessage<TypedId>(id);
}
