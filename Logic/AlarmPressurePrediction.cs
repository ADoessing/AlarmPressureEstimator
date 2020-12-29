using Microsoft.ML.Data;

namespace alarmpressureestimator.Logic
{
    public class AlarmPressurePrediction
    {

        [ColumnName("Score")]
        internal float PredictedAlarmPressure { get; set; }
    }
}