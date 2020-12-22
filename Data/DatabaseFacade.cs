using System;
using System.Collections.Generic;
using System.Text;
using alarmpressureestimator.Aquaintance;
using alarmpressureestimator.Logic;

namespace alarmpressureestimator.Data
{
    public class DatabaseFacade : IDatabaseFacade
    {
        private static DatabaseFacade instance;
        private DbHelper dbHelper;

        private DatabaseFacade()
        {
            dbHelper = new DbHelper();
        }

        public static DatabaseFacade GetInstance()
        {
            if(instance == null)
            {
                instance = new DatabaseFacade();
            }
            return instance;
        }

        public List<TemperatureData> GetDataSet()
        {
            return dbHelper.GetDataSet();
        }

        public (TemperatureData, string, int) GetNewestWeatherforecastByStationId(string StationId, int day)
        {
            return dbHelper.GetNewestWeatherforecastByStationId(StationId, day);
        }

        public List<string> GetStations()
        {
            return dbHelper.GetStations();
        }

        public void UpdateDataset(string date)
        {
            dbHelper.UpdateDataset(date);
        }

        public void UploadNewestAlarmPrediction(string Date, string StationId, int Alarms)
        {
            dbHelper.UploadNewestAlarmPrediction(Date, StationId, Alarms);
        }
    }
}
