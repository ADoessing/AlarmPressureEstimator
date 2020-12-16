using System;
using System.Collections.Generic;
using System.Text;
using temperaturepredictor.Aquaintance;
using temperaturepredictor.Data;

namespace temperaturepredictor.Logic
{
    class LogicFacade : ILogicFacade
    {
        private static LogicFacade instance;
        private Model model;
        private IDatabaseFacade databaseFacade;
        private LogicFacade()
        {
            databaseFacade = DatabaseFacade.GetInstance();
            model = new Model(databaseFacade.GetDataSet());
        }

        public static LogicFacade GetInstance()
        {
            if(instance == null)
            {
                instance = new LogicFacade();
            }
            return instance;
        }

        public int Predict(TemperatureData data)
        {
            return model.Predict(data);
        }

        public void CreatePredictionsForAllStations()
        {
            List<string> Stations = databaseFacade.GetStations();
            for (int i = 0; i < Stations.Count; i++)
            {
                for (int j = 1; j <= 7; j++)
                {
                    var values = databaseFacade.GetNewestWeatherforecastByStationId(Stations[i], j);
                    var prediction = Predict(values.Item1);
                    var date = values.Item2.Substring(6, 4) + "-" + values.Item2.ToString().Substring(3, 2) + "-" + (Convert.ToInt32(values.Item2.ToString().Substring(0, 2)) + j).ToString();
                    databaseFacade.UploadNewestAlarmPrediction(date, Stations[i], Convert.ToInt32(prediction));
                }
            }
        }

    }
}
