using System.Collections.Immutable;

namespace ScheduleBSUIR.Models
{
    public record LessonType(string Abbreviation, string ColorResourceKey);

    public static class LessonTypesHelper
    {
        public static ImmutableList<LessonType> LessonTypes = [
                new LessonType("Консультация", "ConsultColor"),
                new LessonType("Экзамен", "ExamColor"),
                new LessonType("ЛК", "LectureColor"),
                new LessonType("ПЗ", "PracticeColor"),
                new LessonType("ЛР", "LabColor"),
                new LessonType("Зачёт", "CreditColor"), // todo check if valid
                new LessonType("Неизвестно", "UnknownColor"), // todo check if valid
            ];

        public static LessonType GetByAbbreviation(string abbreviation)
        {
            return LessonTypes.First(t => t.Abbreviation == abbreviation);
        }
    }
}
