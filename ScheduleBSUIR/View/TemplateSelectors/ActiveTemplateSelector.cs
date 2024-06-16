using ScheduleBSUIR.Helpers;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.UI;
using ScheduleBSUIR.Services;
using System.Diagnostics;
using System.Globalization;

namespace ScheduleBSUIR.View.TemplateSelectors
{
    class ActiveTemplateSelector : DataTemplateSelector
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        // OnSelectTemplate gets called each frame actually.
        // Benchmarks:
        // ELAPSED FOR 1000 CALLS: 00:00:00.0506691 without cache
        // ELAPSED FOR 1000 CALLS: 00:00:00.0168000 with cache
        private readonly Dictionary<object, bool> _cachedResults = [];

        public DataTemplate ActiveTemplate { get; set; }
        public DataTemplate InactiveTemplate { get; set; }

        public ActiveTemplateSelector()
        {
            // todo: DI
            _dateTimeProvider = new DateTimeProviderService();
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            bool isActive = true;

            if (!_cachedResults.TryGetValue(item, out isActive))
            {
                try
                {
                    string dateString = item switch
                    {
                        Schedule schedule => schedule.DateLesson,
                        ScheduleGroup scheduleGroup => scheduleGroup[0].DateLesson,
                        _ => throw new NotImplementedException(),
                    } 
                    ?? string.Empty;

                    var scheduleDate = ApiDateTimeParser.Parse(dateString);

                    isActive = scheduleDate.Date >= _dateTimeProvider.Now.Date;

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
