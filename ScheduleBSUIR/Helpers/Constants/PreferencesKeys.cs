using System.Collections.Immutable;

namespace ScheduleBSUIR.Helpers.Constants
{
    public static class PreferencesKeys
    {
        public const string FavoriteTimetableId = nameof(FavoriteTimetableId);
        public const string SelectedSubgroupType = nameof(SelectedSubgroupType);
        public const string CacheClearInterval = nameof(CacheClearInterval);
        public const string CacheClearLastDate = nameof(CacheClearLastDate);

        public const string LectureColor = nameof(LectureColor);
        public const string PracticeColor = nameof(PracticeColor);
        public const string LabColor = nameof(LabColor);
        public const string ConsultColor = nameof(ConsultColor);
        public const string ExamColor = nameof(ExamColor);
        public const string CreditColor = nameof(CreditColor);
        public const string AnnouncementColor = nameof(AnnouncementColor);
        public const string UnknownColor = nameof(UnknownColor);

        public static readonly ImmutableList<string> ColorPreferencesKeys = [
            LectureColor,
            PracticeColor,
            LabColor,
            ConsultColor,
            ExamColor,
            CreditColor,
            AnnouncementColor,
            UnknownColor,
            ];
    }
}
