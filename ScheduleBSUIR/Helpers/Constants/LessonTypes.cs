using System.Collections.Immutable;
using static ScheduleBSUIR.Helpers.Constants.PreferencesKeys;

namespace ScheduleBSUIR.Helpers.Constants
{
    public record LessonType(string Abbreviation, string Fullname, string ColorPreferenceKey);

    public static class LessonTypesHelper
    {
        public const string AnnouncementAbbreviation = "Объявление";
        public const string UnknownTypeAbbreviation = "Неизвестно";

        public const string LectureAbbreviation = "ЛК";
        public const string PracticeAbbreviation = "ПЗ";
        public const string LabAbbreviation = "ЛР";
        public const string ConsultAbbreviation = "Консультация";
        public const string ExamAbbreviation = "Экзамен";
        public const string CreditAbbreviation = "Зачет";
        public const string DiffCreditAbbreviation = "Дифф. зачет";
        public const string CandExamAbbreviation = "Канд. экзамен";
        public const string ULectureAbbreviation = "УЛк";
        public const string UPracticeAbbreviation = "УПз";
        public const string ULabAbbreviation = "УЛР";

        public static ImmutableList<LessonType> LessonTypes = [
                new (LectureAbbreviation, "Лекция", LectureColor),
                new (PracticeAbbreviation, "Практическое занятие", PracticeColor),
                new (LabAbbreviation, "Лабораторная работа", LabColor),
                new (ConsultAbbreviation, "Консультация", ConsultColor),
                new (ExamAbbreviation, "Экзамен", ExamColor),
                new (CreditAbbreviation, "Зачет", CreditColor),
                new (AnnouncementAbbreviation, AnnouncementAbbreviation, AnnouncementColor),
                new (DiffCreditAbbreviation, "Дифференцированный зачет", CreditColor),
                new (CandExamAbbreviation, "Кандидатский экзамен", ExamColor),
                new (ULectureAbbreviation, "Лекция", LectureColor),
                new (UPracticeAbbreviation, "Практическое занятие", PracticeColor),
                new (ULabAbbreviation, "Лабораторная работа", LabColor),
                new (UnknownTypeAbbreviation, UnknownTypeAbbreviation, UnknownColor),
            ];

        public static ImmutableList<LessonType> BasicTypes => LessonTypes.Take(7).ToImmutableList();
        public static LessonType GetByAbbreviation(string abbreviation) =>
            LessonTypes
                .FirstOrDefault(t => t.Abbreviation == abbreviation) ??
                GetByAbbreviation(UnknownTypeAbbreviation);
    }
}
