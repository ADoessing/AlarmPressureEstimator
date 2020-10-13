using Microsoft.ML.Data;

namespace temperaturepredictor
{
    public class TemperatureData
    {
        [LoadColumn(0)]
        public float Stores;

        [LoadColumn(1), ColumnName("Label")]
        public float Alarms;

        [LoadColumn(2)]
        public float TempMean;

        [LoadColumn(3)]
        public float Humidity;

        [LoadColumn(4)]
        public float Pressure;

        [LoadColumn(5)]
        public float TempMin;

        [LoadColumn(6)]
        public float TempMax;
    }
}