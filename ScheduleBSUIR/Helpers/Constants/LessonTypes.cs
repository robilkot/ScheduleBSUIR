using System.Collections.Immutable;
using static ScheduleBSUIR.Helpers.Constants.PreferencesKeys;

namespace ScheduleBSUIR.Helpers.Constants
{
    public record LessonType(string Abbreviation, string Fullname, string ColorPreferenceKey);

    public static class LessonTypesHelper
    {
        public const string AnnouncementAbbreviation = "Объявление";

        public static readonly LessonType Lecture = new("ЛК", "Лекция", LectureColor);
        public static readonly LessonType Practice = new("ПЗ", "Практическое занятие", PracticeColor);
        public static readonly LessonType Lab = new("ЛР", "Лабораторная работа", LabColor);
        public static readonly LessonType Consult = new("Консультация", "Консультация", ConsultColor);
        public static readonly LessonType Exam = new("Экзамен", "Экзамен", ExamColor);
        public static readonly LessonType Credit = new("Зачет", "Зачет", CreditColor);
        public static readonly LessonType Announcement = new(AnnouncementAbbreviation, AnnouncementAbbreviation, AnnouncementColor);
        public static readonly LessonType DiffCredit = new("Дифф. зачет", "Дифференцированный зачет", CreditColor);
        public static readonly LessonType CandExam = new("Канд. экзамен", "Кандидатский экзамен", ExamColor);
        public static readonly LessonType ULecture = new("УЛк", "Лекция", LectureColor);
        public static readonly LessonType UPractice = new("УПз", "Практическое занятие", PracticeColor);
        public static readonly LessonType ULab = new("УЛР", "Лабораторная работа", LabColor);

        public static ImmutableList<LessonType> LessonTypes = [
                Lecture,
                Practice,
                Lab,
                Consult,
                Exam,
                Credit,
                Announcement,
                DiffCredit,
                CandExam,
                ULecture,
                UPractice,
                ULab,
            ];

        public static ImmutableList<LessonType> BasicTypes = [
                Lecture,
                Practice,
                Lab,
                Consult,
                Exam,
                Credit,
                Announcement,
            ];

        // Return announcement type if nothing else found
        public static LessonType GetByAbbreviation(string abbreviation) =>
            LessonTypes
                .FirstOrDefault(t => t.Abbreviation == abbreviation) ??
                GetByAbbreviation(AnnouncementAbbreviation);
    }
}
