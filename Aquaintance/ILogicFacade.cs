using System;
using System.Collections.Generic;
using System.Text;
using temperaturepredictor.Logic;

namespace temperaturepredictor.Aquaintance
{
    interface ILogicFacade
    {
        public int Predict(TemperatureData data);
        public void CreatePredictionsForAllStations();
    }
}
