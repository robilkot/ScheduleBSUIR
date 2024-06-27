using DevExpress.Maui.CollectionView;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Services;
using System.Diagnostics;

namespace ScheduleBSUIR.Helpers.TemplateSelectors
{
    class ActiveTemplateSelector : DataTemplateSelector
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILoggingService _loggingService;

        // todo: check this on DXCollectionView
        // OnSelectTemplate gets called on each collectionview actually (even scrolling).
        // Benchmarks:
        // ELAPSED FOR 1000 CALLS: 00:00:00.0506691 without cache
        // ELAPSED FOR 1000 CALLS: 00:00:00.0168000 with cache
        private readonly Dictionary<object, bool> _cachedResults = [];

        public DataTemplate ActiveTemplate { get; set; } = null!;
        public DataTemplate InactiveTemplate { get; set; } = null!;

        public ActiveTemplateSelector()
        {
            _dateTimeProvider = App.Current.Handler.MauiContext.Services.GetRequiredService<IDateTimeProvider>();
            _loggingService = App.Current.Handler.MauiContext.Services.GetRequiredService<ILoggingService>();
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (!_cachedResults.TryGetValue(item, out bool isActive))
            {
                isActive = true;

                try
                {
                    var dateTime = item switch
                    {
                        DaySchedule daySchedule => daySchedule.Day, // For grouping with default collectionview
                        Schedule schedule => schedule.DateLesson, // For no grouping
                        GroupInfo scheduleGroup => (DateTime)scheduleGroup.GroupValue, // For grouping with DXCollectionview
                        _ => throw new NotImplementedException(),
                    } ?? DateTime.MinValue;

                    isActive = dateTime >= _dateTimeProvider.UtcNow.Date;

                    _cachedResults.Add(item, isActive);
                }
                catch (Exception ex)
                {
                    _loggingService.LogError(ex.Message);
                }
            }

            return isActive ?
                ActiveTemplate :
                InactiveTemplate;
        }
    }
}
