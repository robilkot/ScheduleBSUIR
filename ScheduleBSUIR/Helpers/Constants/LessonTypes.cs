using System.Collections.Immutable;
using static ScheduleBSUIR.Helpers.Constants.PreferencesKeys;

namespace ScheduleBSUIR.Helpers.Constants
{
    public record LessonType(string Abbreviation, string Fullname, string ColorPreferenceKey);

    public static class LessonTypesHelper
    {
        public const string AnnouncementAbbreviation = "Объявление";
        public const string UnknownTypeAbbreviation = "Неизвестно";

        public static ImmutableList<LessonType> LessonTypes = [
                new ("ЛК", "Лекция", LectureColor),
                new ("ПЗ", "Практическое занятие", PracticeColor),
                new ("ЛР", "Лабораторная работа", LabColor),
                new ("Консультация", "Консультация", ConsultColor),
                new ("Экзамен", "Экзамен", ExamColor),
                new ("Зачет", "Зачет", CreditColor),
                new (AnnouncementAbbreviation, AnnouncementAbbreviation, AnnouncementColor),
                new ("Дифф. зачет", "Дифференцированный зачет", CreditColor),
                new ("Канд. экзамен", "Кандидатский экзамен", ExamColor),
                new ("УЛк", "Лекция", LectureColor),
                new ("УПз", "Практическое занятие", PracticeColor),
                new ("УЛР", "Лабораторная работа", LabColor),
                new (UnknownTypeAbbreviation, UnknownTypeAbbreviation, UnknownColor),
            ];

        public static ImmutableList<LessonType> BasicTypes => LessonTypes.Take(7).ToImmutableList();
        public static LessonType GetByAbbreviation(string abbreviation) =>
            LessonTypes
                .FirstOrDefault(t => t.Abbreviation == abbreviation) ??
                GetByAbbreviation(UnknownTypeAbbreviation);
    }
}
