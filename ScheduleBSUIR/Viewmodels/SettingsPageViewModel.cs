﻿using CommunityToolkit.Mvvm.ComponentModel;
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
                new ScheduleDay(_dateTimeProvider.Now.AddDays(-1)),
                new Schedule()
                {
                    LessonTypeAbbrev = LessonTypesHelper.Practice.Abbreviation,
                    Subject = "Прошедшая пара",
                    Auditories = ["Аудитория 0"],
                    StudentGroups = [],
                    DateLesson = _dateTimeProvider.Now.AddDays(-1).Date,
                    Employees = [ new EmployeeDto() { PhotoLink = "https://avatars.githubusercontent.com/u/82116328?v=4" }],
                    WeekNumber = [1],
                    StartLessonTime = _dateTimeProvider.Now.AddHours(-5),
                    EndLessonTime = _dateTimeProvider.Now.AddHours(-3),
                },
                new ScheduleDay(_dateTimeProvider.Now.Date),
                new Schedule()
                {
                    LessonTypeAbbrev = LessonTypesHelper.Lecture.Abbreviation,
                    Subject = "Пример лекции",
                    Auditories = ["Аудитория 1", "Аудитория 2"],
                    Note = "Примечание",
                    StudentGroups = [],
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
                    Auditories = ["Аудитория 3"],
                    Note = "Расписание может содержать важные объявления (например, о графике досдач)",
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
