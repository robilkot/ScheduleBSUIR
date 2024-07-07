using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.UI;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ScheduleBSUIR.Helpers.TemplateSelectors
{
    class TimetableItemTemplateSelector : DataTemplateSelector
    {
        // Converter is being created before app initializtion, thus DI is not possible in ctor
        private readonly Lazy<IDateTimeProvider> _lazyDateTimeProvider = new(() => App.Current.Handler?.MauiContext.Services.GetRequiredService<IDateTimeProvider>());

        // todo: check this on DXCollectionView
        // OnSelectTemplate gets called on each collectionview actually (even scrolling).
        // Benchmarks:
        // ELAPSED FOR 1000 CALLS: 00:00:00.0506691 without cache
        // ELAPSED FOR 1000 CALLS: 00:00:00.0168000 with cache
        private readonly Dictionary<object, DataTemplate> _cachedResults = [];

        public required DataTemplate ActiveScheduleTemplate { get; init; }
        public required DataTemplate InactiveScheduleTemplate { get; init; }
        public required DataTemplate ActiveScheduleDayTemplate { get; init; }
        public required DataTemplate InactiveScheduleDayTemplate { get; init; }
        public required DataTemplate ActiveAnnouncementTemplate { get; init; }
        public required DataTemplate InactiveAnnouncementTemplate { get; init; }
        public required DataTemplate TimetableEndTemplate { get; init; }

        protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        {
            // Avoid double hashing
            ref var cachedTemplate = ref CollectionsMarshal.GetValueRefOrNullRef(_cachedResults, item);

            if (!Unsafe.IsNullRef(in cachedTemplate))
                return cachedTemplate;

            DataTemplate template = item switch
            {
                Schedule schedule =>
                    schedule.DateLesson >= _lazyDateTimeProvider.Value.Now.Date
                    ? (schedule.Announcement ? ActiveAnnouncementTemplate : ActiveScheduleTemplate)
                    : (schedule.Announcement ? InactiveAnnouncementTemplate : InactiveScheduleTemplate),

                DayHeader header => header.Day >= _lazyDateTimeProvider.Value.Now.Date ? ActiveScheduleDayTemplate : InactiveScheduleDayTemplate,

                TimetableEnd => TimetableEndTemplate,

                _ => throw new UnreachableException(),
            };

            _cachedResults.Add(item, template);

            return template;
        }
    }
}
