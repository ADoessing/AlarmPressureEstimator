using Microsoft.ML.Data;

namespace alarmpressureestimator.Logic
{
    public class AlarmPressurePrediction
    {

        [ColumnName("Score")]
        public float PredictedAlarmPressure { get; set; }
    }
}