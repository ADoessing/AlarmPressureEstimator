using System;
using System.Collections.Generic;
using System.Text;
using temperaturepredictor.Logic;

namespace temperaturepredictor.Aquaintance
{
    interface IDatabaseFacade
    {
        public List<string> GetStations();
        public (TemperatureData, string, int) GetNewestWeatherforecastByStationId(string StationId, int day);
        public void UploadNewestAlarmPrediction(string Date, string StationId, int Alarms);
        public List<TemperatureData> GetDataSet();
        public void UpdateDataset(string date)

    }
}
