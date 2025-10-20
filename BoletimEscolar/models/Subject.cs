using System.Text.Json.Serialization;

namespace BoletimEscolar.Models
{
    public class Subject
    {
        public string Name { get; set; } = string.Empty;
        public List<double> Grades { get; set; } = new();

        [JsonIgnore]
        public double Average => Grades.Count == 0 ? 0.0 : Math.Round(Grades.Average(), 2);

        [JsonIgnore]
        public string Situation => (Average >= 7.0) ? "Aprovado" : (Average >= 5.0) ? "Recuperação" : "Reprovado";

    }
}