using CommunityToolkit.Mvvm.Messaging.Messages;
using ScheduleBSUIR.Helpers.Constants;

namespace ScheduleBSUIR.Models.Messaging
{
    public sealed class TimetableStateChangedMessage((TimetableHeader, TimetableState) value) : ValueChangedMessage<(TimetableHeader, TimetableState)>(value);
}
