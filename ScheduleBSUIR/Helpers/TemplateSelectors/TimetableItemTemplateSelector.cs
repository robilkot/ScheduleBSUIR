using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.UI;
using System.Diagnostics;

namespace ScheduleBSUIR.Helpers.TemplateSelectors
{
    class TimetableItemTemplateSelector : DataTemplateSelector
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        // todo: check this on DXCollectionView
        // OnSelectTemplate gets called on each collectionview actually (even scrolling).
        // Benchmarks:
        // ELAPSED FOR 1000 CALLS: 00:00:00.0506691 without cache
        // ELAPSED FOR 1000 CALLS: 00:00:00.0168000 with cache
        private readonly Dictionary<ITimetableItem, DataTemplate> _cachedResults = [];

        public TimetableItemTemplateSelector()
        {
            _dateTimeProvider = App.Current!.Handler.MauiContext!.Services.GetRequiredService<IDateTimeProvider>();
        }

        public required DataTemplate ActiveScheduleTemplate { get; init; }
        public required DataTemplate InactiveScheduleTemplate { get; init; }
        public required DataTemplate ActiveScheduleDayTemplate { get; init; }
        public required DataTemplate InactiveScheduleDayTemplate { get; init; }
        public required DataTemplate ActiveAnnouncementTemplate { get; init; }
        public required DataTemplate InactiveAnnouncementTemplate { get; init; }

        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            if (item is not ITimetableItem timetableItem)
                return null;

            if (_cachedResults.TryGetValue(timetableItem, out DataTemplate? template))
                return template;

            template = timetableItem switch
            {
                Schedule schedule =>
                    schedule.DateLesson >= _dateTimeProvider.Now.Date
                    ? (schedule.Announcement ? ActiveAnnouncementTemplate : ActiveScheduleTemplate)
                    : (schedule.Announcement ? InactiveAnnouncementTemplate : InactiveScheduleTemplate),

                ScheduleDay scheduleDay => scheduleDay.Day >= _dateTimeProvider.Now.Date ? ActiveScheduleDayTemplate : InactiveScheduleDayTemplate,
                _ => throw new UnreachableException(),
            };

            _cachedResults.Add(timetableItem, template);

            return template;
        }
    }
}
