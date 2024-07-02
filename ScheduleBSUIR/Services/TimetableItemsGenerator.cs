using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.UI;
using System.Diagnostics;

namespace ScheduleBSUIR.Services
{
    public class TimetableItemsGenerator(ILoggingService loggingService)
    {
        private readonly ILoggingService _loggingService = loggingService;

        private const int LoadingStep = 10;

        private IEnumerator<DailySchedule>? _schedulesEnumerator = null;

        private Timetable? _timetable = null;
        private TimetableTabs _currentTab;
        private SubgroupType _currentSubgroupType;

        public Task<List<ITimetableItem>> GenerateMoreItems(
            Timetable timetable, 
            TimetableTabs timetableTabs = TimetableTabs.Schedule, 
            SubgroupType subgroupType = SubgroupType.All, 
            bool generateTillActive = false)
        {
            _timetable = timetable;

            if (timetableTabs != _currentTab || subgroupType != _currentSubgroupType)
            {
                _currentTab = timetableTabs;
                _currentSubgroupType = subgroupType;

                _schedulesEnumerator?.Dispose();
                _schedulesEnumerator = null;
            }

            _schedulesEnumerator ??= GetEnumeratorForParameters();

            TaskCompletionSource<List<ITimetableItem>> tcs = new();

            _ = Task.Run(() =>
            {
                // Deferred LINQ
                List<ITimetableItem> resultList = [];

                // todo: generateTillActive

                for (int i = 0; i < LoadingStep; i++)
                {
                    if (!_schedulesEnumerator.MoveNext())
                        break;

                    var currentDailySchedule = _schedulesEnumerator.Current;

                    // todo: also add ScheduleWeek?
                    resultList.Add(new ScheduleDay(currentDailySchedule.Day));

                    resultList.AddRange(currentDailySchedule);
                }

                tcs.SetResult(resultList);
            });

            return tcs.Task;
        }

        private IEnumerator<DailySchedule> GetEnumeratorForParameters()
        {
            IEnumerable<DailySchedule> result = [];

            if (_currentTab is TimetableTabs.Exams)
            {
                var allSchedules = _currentSubgroupType switch
                {
                    SubgroupType.All => _timetable?.Exams,

                    SubgroupType.FirstSubgroup => _timetable?.Exams?
                        .Where(schedule => schedule is { NumSubgroup: not SubgroupType.SecondSubgroup }),

                    SubgroupType.SecondSubgroup => _timetable?.Exams?
                        .Where(schedule => schedule is { NumSubgroup: not SubgroupType.FirstSubgroup }),

                    _ => throw new UnreachableException(),
                }
                ?? [];

                result = allSchedules
                        .GroupBy(schedule => schedule.DateLesson)
                        .Select(grouping => new DailySchedule(grouping));
            }

            // todo: for schedule tab

            return result.GetEnumerator();
        }
    }
}
