using DevExpress.Maui.CollectionView;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Services;
using System.Diagnostics;

namespace ScheduleBSUIR.View.TemplateSelectors
{
    class ActiveTemplateSelector : DataTemplateSelector
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        // OnSelectTemplate gets called on each collectionview actually (even scrolling).
        // Benchmarks:
        // ELAPSED FOR 1000 CALLS: 00:00:00.0506691 without cache
        // ELAPSED FOR 1000 CALLS: 00:00:00.0168000 with cache
        private readonly Dictionary<object, bool> _cachedResults = [];

        public DataTemplate ActiveTemplate { get; set; } = null!;
        public DataTemplate InactiveTemplate { get; set; } = null!;

        public ActiveTemplateSelector()
        {
            // todo: DI
            _dateTimeProvider = new DateTimeProviderService();
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
                        Schedule schedule => schedule.DateLesson,
                        GroupInfo scheduleGroup => (DateTime)scheduleGroup.GroupValue,
                        _ => throw new NotImplementedException(),
                    } ?? DateTime.MinValue;

                    isActive = dateTime >= _dateTimeProvider.Now.Date;

                    _cachedResults.Add(item, isActive);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    // todo: log
                }
            }

            return isActive ?
                ActiveTemplate :
                InactiveTemplate;
        }
    }
}
