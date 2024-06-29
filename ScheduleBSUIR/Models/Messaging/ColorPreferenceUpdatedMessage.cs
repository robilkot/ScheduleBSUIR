using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ScheduleBSUIR.Models.Messaging
{
    public sealed class ColorPreferenceUpdatedMessage((string, Color) keyValuePair) : ValueChangedMessage<(string, Color)>(keyValuePair);
}
