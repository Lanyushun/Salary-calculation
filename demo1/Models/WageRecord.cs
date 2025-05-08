using SQLite;

namespace demo1.Models
{
    public class WageRecord
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public int ProductCount { get; set; }

        public double Rate { get; set; }

        public double DailyWage { get; set; }
    }
}