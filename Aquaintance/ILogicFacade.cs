using System;
using System.Collections.Generic;
using System.Text;
using alarmpressureestimator.Logic;

namespace alarmpressureestimator.Aquaintance
{
    interface ILogicFacade
    {
        public int Predict(TemperatureData data);
        public void CreatePredictionsForAllStations();
    }
}
