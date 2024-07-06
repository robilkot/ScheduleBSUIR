using LiteDB;
using ScheduleBSUIR.Interfaces;

namespace ScheduleBSUIR.Models
{
    public abstract class TimetableHeader : ICacheable
    {
        public abstract string PrimaryKey { get; }
        public abstract string HeaderText { get; }
        public abstract string? SubheaderText { get; }
        public abstract string? GroupingText { get; }
        public required ITimetableOwner TimetableOwner { get; init; }
        public static TimetableHeader FromOwner(ITimetableOwner owner) => owner switch
        {
            IStudentGroup group => new StudentGroupTimetableHeader() { TimetableOwner = group },
            IEmployee employee => new EmployeeTimetableHeader() { TimetableOwner = employee },
            _ => throw new NotImplementedException()
        };
        public override bool Equals(object? obj) => obj switch
        {
            TimetableHeader header => PrimaryKey.Equals(header.PrimaryKey),
            _ => false,
        };
        public override int GetHashCode() => PrimaryKey.GetHashCode();
        public override string ToString() => PrimaryKey;
    }

    public sealed class StudentGroupTimetableHeader : TimetableHeader
    {
        [BsonId]
        public override string PrimaryKey => TimetableOwner.TimetableId;
        [BsonIgnore]
        public override string HeaderText => ((IStudentGroup)TimetableOwner).Name;
        [BsonIgnore]
        public override string? SubheaderText => ((IStudentGroup)TimetableOwner).SpecialityAbbreviation;
        [BsonIgnore]
        public override string? GroupingText => ((IStudentGroup)TimetableOwner).SpecialityAbbreviation;
    }
    public sealed class EmployeeTimetableHeader : TimetableHeader
    {
        [BsonId]
        public override string PrimaryKey => TimetableOwner.TimetableId;
        [BsonIgnore]
        public override string HeaderText => ((IEmployee)TimetableOwner).AbbreviatedName;
        [BsonIgnore]
        public override string? SubheaderText => null;
        [BsonIgnore]
        public override string? GroupingText => ((IEmployee)TimetableOwner).LastName;
        [BsonIgnore]
        public string LongHeaderText => ((IEmployee)TimetableOwner).FullName;
    }

    public static class TimetableHeadersExtensions
    {
        public static IEnumerable<TimetableHeader> ToTimetableHeaders(this IEnumerable<ITimetableOwner> headers) =>
            headers.Select(TimetableHeader.FromOwner);
    }
}
