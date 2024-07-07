using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Models.Messaging;
using ScheduleBSUIR.Services;

namespace ScheduleBSUIR.View
{
    public class PinnedTimetablePage : TimetablePage
    {
        public PinnedTimetablePage()
        {
            _viewmodel.IsPinnedTimetable = true;
        }

        protected async override void OnAppearing()
        {
            if(_viewmodel.TimetableId is null)
            {
                _loggingService.LogInfo($"PinnedTimetablePage getting id", displayCaller: false);
                var _timetableService = App.Current.Handler.MauiContext.Services.GetRequiredService<TimetableService>();
                _viewmodel.TimetableId = await _timetableService.GetPinnedTimetableAsync();
            }
        }

        // If we pin some timetable, pages down the stack should update their state. As well as pinned timetable on separate tab
        public override void Receive(TimetableStateChangedMessage message)
        {
            _loggingService.LogInfo($"Pinned page rec StateChanged");

            var msgId = message.Value.Item1;
            var msgState = message.Value.Item2;

            if (msgId.Equals(_viewmodel.TimetableId))
            {
                if (msgState != TimetableState.Pinned)
                {
                    _viewmodel.TimetableId = null;
                }
            }
            else
            {
                if (msgState == TimetableState.Pinned)
                {
                    _viewmodel.TimetableId = null;
                }
            }
        }
    }
}
