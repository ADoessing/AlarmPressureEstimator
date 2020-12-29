using Microsoft.ML.Data;

namespace alarmpressureestimator.Logic
{
    public class TemperatureData
    {
        [LoadColumn(0)]
        public float Stores;

        [LoadColumn(1), ColumnName("Label")]
        public float Alarms;

        [LoadColumn(2)]
        public float AlarmItems;

        [LoadColumn(3)]
        public float TempMean;

        [LoadColumn(4)]
        public float Humidity;

        [LoadColumn(5)]
        public float Pressure;

        [LoadColumn(6)]
        public float TempMin;

        [LoadColumn(7)]
        public float TempMax;
    }
}