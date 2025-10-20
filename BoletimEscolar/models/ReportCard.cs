using System.Text.Json.Serialization;

namespace BoletimEscolar.Models
{
    public class ReportCard
    {
        public string StudentName { get; set; } = string.Empty;
        public List<Subject> Subjects { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public double OverallAverage => Subjects.Count == 0 ? 0.0 : Math.Round(Subjects.Average(s => s.Average), 2);

        [JsonIgnore]
        public int SubjectsApproved => Subjects.Count(s => s.Average >= 7.0);

        [JsonIgnore]
        public int SubjectsInRecovery => Subjects.Count(s => s.Average >= 5.0 && s.Average < 7.0);

        [JsonIgnore]
        public int SubjectsFailed => Subjects.Count(s => s.Average < 5.0);
    }
}

