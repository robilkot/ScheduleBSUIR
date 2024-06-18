﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ScheduleBSUIR.Models;
using ScheduleBSUIR.Models.UI;

namespace ScheduleBSUIR.Viewmodels
{
    public partial class ExamsPageViewModel : BaseViewModel
    {
        [ObservableProperty]
        private Timetable _timetable = null!;

        [ObservableProperty]
        private List<ScheduleGroup> _exams = [];

        [ObservableProperty]
        private string _timetableHeader = string.Empty;

        [RelayCommand]
        public void SetExams(Timetable? timetable)
        {
            if (timetable is null)
                return;

            Timetable = timetable;

            try
            {
                var daysExams = Timetable.Exams?
                    .GroupBy(schedule => schedule.DateLesson ?? DateTime.MinValue)
                    .ToDictionary(g => g.Key, g => g.ToList());

                Exams = daysExams?
                    .Select(kvp => new ScheduleGroup(kvp.Key, kvp.Value))
                    .ToList()
                    ?? [];
            }
            catch
            {
                // todo: log
            }
        }
    }
}
