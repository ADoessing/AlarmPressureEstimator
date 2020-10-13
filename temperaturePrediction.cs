using Microsoft.ML.Data;

namespace temperaturepredictor
{
    public class temperaturePrediction
    {

        [ColumnName("Score")]
        public float PredictedTemperature { get; set; }
    }
}