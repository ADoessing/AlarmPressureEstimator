using Microsoft.ML.Data;

namespace alarmpressureestimator.Logic
{
    public class TemperatureData
    {
        [LoadColumn(0)]
        internal float Stores;

        [LoadColumn(1), ColumnName("Label")]
        internal float Alarms;

        [LoadColumn(2)]
        internal float AlarmItems;

        [LoadColumn(3)]
        internal float TempMean;

        [LoadColumn(4)]
        internal float Humidity;

        [LoadColumn(5)]
        internal float Pressure;

        [LoadColumn(6)]
        internal float TempMin;

        [LoadColumn(7)]
        internal float TempMax;
    }
}