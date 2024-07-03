using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.UI;
using System.Diagnostics;

namespace ScheduleBSUIR.Services
{
    public class TimetableItemsGenerator(ILoggingService loggingService, IDateTimeProvider dateTimeProvider)
    {
        private readonly ILoggingService _loggingService = loggingService;
        private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

        private const int LoadingStep = 10;

        private IEnumerator<DailySchedule>? _schedulesEnumerator = null;

        private Timetable? _timetable = null;
        private TimetableTabs _currentTab;
        private SubgroupType _currentSubgroupType;

        public async Task<List<ITimetableItem>> GenerateMoreItems(
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

            // Deferred LINQ
            List<ITimetableItem> resultList = [];

            await Task.Run(() =>
            {
                DateTime? nearestScheduleDate = generateTillActive ? GetNearestScheduleDate(timetable, timetableTabs, subgroupType) : null;

                DailySchedule? lastObtainedSchedule = null;

                do
                {
                    for (int i = 0; i < LoadingStep; i++)
                    {
                        if (!_schedulesEnumerator.MoveNext())
                            return;

                        lastObtainedSchedule = _schedulesEnumerator.Current;

                        resultList.Add(new ScheduleDay(lastObtainedSchedule.Day));

                        resultList.AddRange(lastObtainedSchedule);
                    }
                }
                while (generateTillActive && nearestScheduleDate is not null
                && lastObtainedSchedule?.Day.Date < nearestScheduleDate.Value.Date);
            });

            return resultList;
        }

        public DateTime? GetNearestScheduleDate(Timetable timetable,
            TimetableTabs timetableTabs = TimetableTabs.Schedule,
            SubgroupType subgroupType = SubgroupType.All)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            DateTime? result = null;

            // Assuming the list is sorted
            if (timetableTabs is TimetableTabs.Exams)
            {
                var firstSchedule = subgroupType switch
                {
                    SubgroupType.All => timetable.Exams?
                        .FirstOrDefault(schedule => schedule.DateLesson >= _dateTimeProvider.Now.Date),

                    SubgroupType.FirstSubgroup => timetable.Exams?
                        .FirstOrDefault(schedule => schedule.NumSubgroup != SubgroupType.SecondSubgroup && schedule.DateLesson >= _dateTimeProvider.Now.Date),

                    SubgroupType.SecondSubgroup => timetable.Exams?
                        .FirstOrDefault(schedule => schedule.NumSubgroup != SubgroupType.FirstSubgroup && schedule.DateLesson >= _dateTimeProvider.Now.Date),

                    _ => throw new UnreachableException(),
                };

                result = firstSchedule?.DateLesson;
            }

            // todo for schedule tab
            _loggingService.LogInfo($"GetNearestScheduleDate {result?.ToString("dd.MM") ?? "NULL"} in {stopwatch.Elapsed:ss\\.FFFFF}", displayCaller: false);

            return result;
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

            return result.GetEnumerator();
        }
    }
}
