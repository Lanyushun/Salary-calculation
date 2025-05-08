using SQLite;
using System.Collections.ObjectModel;
using demo1.Models;

namespace demo1.Services
{
    public class WageService
    {
        private SQLiteAsyncConnection _database;

        public WageService(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<WageRecord>().Wait();
        }

        // 计算工资率
        public double CalculateRate(int productCount)
        {
            if (productCount <= 1000)
                return 0.17;
            else if (productCount <= 1500)
                return 0.18;
            else
                return 0.20;
        }

        // 计算当日工资
        public double CalculateDailyWage(int productCount)
        {
            var rate = CalculateRate(productCount);
            return productCount * rate;
        }

        // 保存记录
        public async Task SaveRecordAsync(int productCount)
        {
            var rate = CalculateRate(productCount);
            var wage = CalculateDailyWage(productCount);

            var record = new WageRecord
            {
                Date = DateTime.Now,
                ProductCount = productCount,
                Rate = rate,
                DailyWage = wage
            };

            await _database.InsertAsync(record);
        }

        // 获取所有记录
        public async Task<List<WageRecord>> GetAllRecordsAsync()
        {
            return await _database.Table<WageRecord>().ToListAsync();
        }

        // 获取当日记录 - 修复了日期比较问题
        public async Task<List<WageRecord>> GetTodayRecordsAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            return await _database.QueryAsync<WageRecord>(
                "SELECT * FROM WageRecord WHERE Date >= ? AND Date < ?",
                today.Ticks, tomorrow.Ticks);
        }

        // 获取当月记录 - 修复了日期比较问题
        public async Task<List<WageRecord>> GetMonthRecordsAsync()
        {
            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

            return await _database.QueryAsync<WageRecord>(
                "SELECT * FROM WageRecord WHERE Date >= ? AND Date < ?",
                firstDayOfMonth.Ticks, firstDayOfNextMonth.Ticks);
        }

        // 计算当日总工资
        public async Task<double> GetTodayTotalWageAsync()
        {
            var records = await GetTodayRecordsAsync();
            return records.Sum(r => r.DailyWage);
        }

        // 计算当月总工资
        public async Task<double> GetMonthTotalWageAsync()
        {
            var records = await GetMonthRecordsAsync();
            return records.Sum(r => r.DailyWage);
        }

        // 计算总工资
        public async Task<double> GetTotalWageAsync()
        {
            var records = await GetAllRecordsAsync();
            return records.Sum(r => r.DailyWage);
        }
    }
}