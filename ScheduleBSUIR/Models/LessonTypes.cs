﻿using System.Collections.Immutable;

namespace ScheduleBSUIR.Models
{
    public record LessonType(string Abbreviation, string ColorResourceKey);

    public static class LessonTypesHelper
    {
        public const string UnknownTypeAbbreviation = "Неизвестно";

        public static ImmutableList<LessonType> LessonTypes = [
                new LessonType("Консультация", "ConsultColor"),
                new LessonType("Экзамен", "ExamColor"),
                new LessonType("Канд. экзамен", "ExamColor"),
                new LessonType("ЛК", "LectureColor"),
                new LessonType("ПЗ", "PracticeColor"),
                new LessonType("ЛР", "LabColor"),
                new LessonType("Зачёт", "CreditColor"), // todo check if valid
                new LessonType(UnknownTypeAbbreviation, "UnknownColor"), // todo check if valid
            ];

        public static LessonType GetByAbbreviation(string abbreviation)
        {
            return LessonTypes.FirstOrDefault(t => t.Abbreviation == abbreviation) ??
                GetByAbbreviation(UnknownTypeAbbreviation);
        }
    }
}