using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ScheduleBSUIR.Helpers.Constants;
using ScheduleBSUIR.Interfaces;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.UI;

namespace ScheduleBSUIR.Viewmodels
{
    public partial class SettingsPageViewModel : BaseViewModel
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        public SettingsPageViewModel(ILoggingService loggingService, IDateTimeProvider dateTimeProvider) : base(loggingService)
        {
            _dateTimeProvider = dateTimeProvider;

            InitExampleItems();
        }

        [ObservableProperty]
        private List<ITimetableItem> _exampleItems = [];

        private void InitExampleItems()
        {
            ExampleItems = [
                new DayHeader(_dateTimeProvider.Now.AddDays(-1)),
                new Schedule()
                {
                    LessonTypeAbbrev = LessonTypesHelper.Lecture.Abbreviation,
                    Subject = "Пример прошедшей пары",
                    Auditories = ["Аудитория 0"],
                    StudentGroups = [],
                    DateLesson = _dateTimeProvider.Now.AddDays(-1).Date,
                    Employees = [ new EmployeeDto() { PhotoLink = "https://avatars.githubusercontent.com/u/82116328?v=4" }],
                    WeekNumber = [1],
                    StartLessonTime = _dateTimeProvider.Now.AddHours(-5),
                    EndLessonTime = _dateTimeProvider.Now.AddHours(-3),
                },
                new DayHeader(_dateTimeProvider.Now.Date),
                new Schedule()
                {
                    LessonTypeAbbrev = LessonTypesHelper.Lab.Abbreviation,
                    Subject = "Пример ЛР",
                    Auditories = ["Аудитория пары"],
                    Note = "Примечание",
                    StudentGroups = [],
                    NumSubgroup = SubgroupType.FirstSubgroup,
                    DateLesson = _dateTimeProvider.Now.Date,
                    Employees = [ new EmployeeDto() { PhotoLink = "https://avatars.githubusercontent.com/u/82116328?v=4" }],
                    WeekNumber = [1, 2, 3, 4],
                    StartLessonTime = _dateTimeProvider.Now.AddHours(-1),
                    EndLessonTime = _dateTimeProvider.Now.AddHours(1),
                },
                new Schedule()
                {
                    LessonTypeAbbrev = LessonTypesHelper.AnnouncementAbbreviation,
                    Subject = LessonTypesHelper.GetByAbbreviation(LessonTypesHelper.AnnouncementAbbreviation).Fullname,
                    Auditories = [],
                    Note = "Расписание может содержать важную информацию (например, о графике досдач)",
                    StudentGroups = [],
                    DateLesson = _dateTimeProvider.Now.Date,
                    Announcement = true,
                    Employees = [],
                    WeekNumber = [],
                    StartLessonTime = _dateTimeProvider.Now.AddHours(1.5),
                    EndLessonTime = _dateTimeProvider.Now.AddHours(3),
                }
                ];
        }
    }
}
