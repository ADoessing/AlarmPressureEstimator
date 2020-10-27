using Microsoft.ML.Data;

namespace temperaturepredictor
{
    public class AlarmPressurePrediction
    {

        [ColumnName("Score")]
        public float PredictedAlarmPressure { get; set; }
    }
}