using System;
using System.Collections.Generic;

namespace AppEntradaSalidaDESO.Models
{
    /// <summary>
    /// Estadísticas del usuario
    /// </summary>
    public class Statistics
    {
        public int TotalExercisesCompleted { get; set; }
        public double AverageAccuracy { get; set; }
        public int TotalTimeMinutes { get; set; }
        public int CurrentStreak { get; set; }
        public Dictionary<string, AlgorithmStats> AlgorithmStatistics { get; set; } = new();
        public DateTime LastUpdated { get; set; }

        public Statistics()
        {
            LastUpdated = DateTime.Now;
        }

        public void UpdateStats(string algorithm, bool correct, int timeSeconds)
        {
            TotalExercisesCompleted++;
            TotalTimeMinutes += timeSeconds / 60;

            if (!AlgorithmStatistics.ContainsKey(algorithm))
            {
                AlgorithmStatistics[algorithm] = new AlgorithmStats { AlgorithmName = algorithm };
            }

            AlgorithmStatistics[algorithm].TotalAttempts++;
            if (correct)
            {
                AlgorithmStatistics[algorithm].CorrectAttempts++;
                CurrentStreak++;
            }
            else
            {
                CurrentStreak = 0;
            }

            RecalculateAverageAccuracy();
            LastUpdated = DateTime.Now;
        }

        private void RecalculateAverageAccuracy()
        {
            if (TotalExercisesCompleted == 0)
            {
                AverageAccuracy = 0;
                return;
            }

            int totalCorrect = 0;
            foreach (var stat in AlgorithmStatistics.Values)
            {
                totalCorrect += stat.CorrectAttempts;
            }

            AverageAccuracy = (double)totalCorrect / TotalExercisesCompleted * 100;
        }

        public void Reset()
        {
            TotalExercisesCompleted = 0;
            AverageAccuracy = 0;
            TotalTimeMinutes = 0;
            CurrentStreak = 0;
            AlgorithmStatistics.Clear();
            LastUpdated = DateTime.Now;
        }
    }

    public class AlgorithmStats
    {
        public string AlgorithmName { get; set; } = string.Empty;
        public int TotalAttempts { get; set; }
        public int CorrectAttempts { get; set; }

        public double SuccessRate => TotalAttempts > 0 ? (double)CorrectAttempts / TotalAttempts * 100 : 0;
    }
}
