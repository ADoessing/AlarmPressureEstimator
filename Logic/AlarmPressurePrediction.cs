using Microsoft.ML.Data;

namespace temperaturepredictor.Logic
{
    public class AlarmPressurePrediction
    {

        [ColumnName("Score")]
        public float PredictedAlarmPressure { get; set; }
    }
}