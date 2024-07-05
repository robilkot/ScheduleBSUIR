using CommunityToolkit.Mvvm.Messaging.Messages;
using ScheduleBSUIR.Helpers.Constants;

namespace ScheduleBSUIR.Models.Messaging
{
    public sealed class TimetableStateChangedMessage((TypedId, TimetableState) value) : ValueChangedMessage<(TypedId, TimetableState)>(value);
}
