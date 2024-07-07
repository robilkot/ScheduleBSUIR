using MethodTimer;
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

        private const int DaysPerLoad = 6;

        private IEnumerator<DailySchedule>? _schedulesEnumerator = null;

        private Timetable _timetable;
        private TimetableTabs _currentTab;
        private SubgroupType _currentSubgroupType;

        private int _currentItemIndex = 0;
        private int? _nearestScheduleIndex = null;
        private bool _endRecordReturned = false;
        private bool _firstRun = true;

        [Time]
        public async Task<List<ITimetableItem>> GenerateMoreItems(
            Timetable timetable,
            TimetableTabs timetableTabs = TimetableTabs.Schedule,
            SubgroupType subgroupType = SubgroupType.All)
        {
            if (_timetable != timetable || timetableTabs != _currentTab || subgroupType != _currentSubgroupType || _firstRun)
            {
                _timetable = timetable;
                _currentTab = timetableTabs;
                _currentSubgroupType = subgroupType;

                _schedulesEnumerator?.Dispose();
                _schedulesEnumerator = null;
                _nearestScheduleIndex = null;

                _endRecordReturned = false;
                _currentItemIndex = 0;

                _firstRun = false;
            }

            _schedulesEnumerator ??= GetEnumeratorForParameters(timetable, timetableTabs, subgroupType);

            // Deferred LINQ
            List<ITimetableItem> resultList = [];

            await Task.Run(() =>
            {
                DailySchedule currentDailySchedule;

                for (int currentDayIndex = 0; currentDayIndex < DaysPerLoad; currentDayIndex++)
                {
                    if (!_schedulesEnumerator.MoveNext())
                    {
                        if (!_endRecordReturned)
                        {
                            resultList.Add(new TimetableEnd());
                            _endRecordReturned = true;
                        }

                        return;
                    }

                    currentDailySchedule = _schedulesEnumerator.Current;

                    resultList.Add(new DayHeader(currentDailySchedule.Day, currentDailySchedule.Week));

                    resultList.AddRange(currentDailySchedule);

                    if (_nearestScheduleIndex is null && currentDailySchedule.Day.Date >= _dateTimeProvider.Now.Date)
                    {
                        _nearestScheduleIndex = _currentItemIndex;
                        _loggingService.LogInfo($"NearestScheduleIndex set to {_nearestScheduleIndex}");
                    }

                    _currentItemIndex += 1 + currentDailySchedule.Count;
                }
            });

            return resultList;
        }

        public int? GetNearestScheduleIndex() => _nearestScheduleIndex;

        private IEnumerator<DailySchedule> GetEnumeratorForParameters(Timetable timetable, TimetableTabs timetableTabs, SubgroupType subgroupType)
        {
            return timetableTabs switch
            {
                TimetableTabs.Exams when timetable.Exams is not null => GetExamsEnumerator(timetable, subgroupType),
                TimetableTabs.Schedule when timetable.Schedules is not null => GetScheduleEnumerator(timetable, subgroupType),
                _ => Enumerable.Empty<DailySchedule>().GetEnumerator()
            };
        }

        private IEnumerator<DailySchedule> GetExamsEnumerator(Timetable timetable, SubgroupType subgroupType)
        {
            var enumerable = timetable.Exams!
                    .OnlySubgroup(subgroupType)
                    .GroupBy(schedule => schedule.DateLesson)
                    .Select(grouping => new DailySchedule(grouping))
                    ?? [];

            return enumerable.GetEnumerator();
        }

        private IEnumerator<DailySchedule> GetScheduleEnumerator(Timetable timetable, SubgroupType subgroupType)
        {
            DateTime startDate = _dateTimeProvider.Now.AddDays(-1);
            DateTime? endDate = timetable.EndDate;

            if (endDate is null || startDate > endDate)
                yield break;

            DateTime currentDate = startDate;
            int currentWeekNumber = _dateTimeProvider.GetWeekAsync(currentDate, CancellationToken.None).GetAwaiter().GetResult(); // todo: this does not run on main thread by design, but maybe refactor? 

            while (currentDate <= endDate)
            {
                var schedulesOnThisDay = timetable.Schedules!.GetByDayOfWeek(currentDate.DayOfWeek);

                var schedules = schedulesOnThisDay
                    .Where(schedule => schedule.WeekNumber?.Contains(currentWeekNumber) ?? true) // No weeks may occur in announcement. Monitor if bug appears
                    .Where(schedule => schedule.EndLessonDate >= currentDate)
                    .OnlySubgroup(subgroupType)
                    .Select(schedule =>
                    {
                        Schedule cloned = (Schedule)schedule.Clone();
                        cloned.DateLesson = currentDate;
                        return cloned;
                    });

                DailySchedule currentSchedule = new(schedules, currentWeekNumber);

                currentDate = currentDate.AddDays(1);

                if (currentDate.DayOfWeek is DayOfWeek.Sunday)
                {
                    currentWeekNumber += 1;

                    if (currentWeekNumber == 5)
                    {
                        currentWeekNumber = 1;
                    }
                }

                if (schedules.Any())
                {
                    yield return currentSchedule;
                }
            }
        }
    }
    public static class TimetableItemsGeneratorExtensions
    {
        public static IEnumerable<Schedule> OnlySubgroup(this IEnumerable<Schedule> schedules, SubgroupType subgroupType)
        {
            return subgroupType switch
            {
                SubgroupType.All => schedules,

                SubgroupType.FirstSubgroup => schedules
                    .Where(schedule => schedule is { NumSubgroup: not SubgroupType.SecondSubgroup }),

                SubgroupType.SecondSubgroup => schedules
                    .Where(schedule => schedule is { NumSubgroup: not SubgroupType.FirstSubgroup }),

                _ => throw new UnreachableException(),
            };
        }
    }
}
