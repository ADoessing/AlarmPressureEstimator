using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Text;

namespace alarmpressureestimator.Logic
{
    public class Model
    {

        private MLContext context = new MLContext();
        private IDataView trainData;
        private DataOperationsCatalog.TrainTestData testTrainSplit;
        private IEstimator<ITransformer> pipeline;
        private ITransformer model;
        private PredictionEngine<TemperatureData, AlarmPressurePrediction> predictionFunc;

        //Load data

        public Model(List<TemperatureData> data)
        {
            trainData = context.Data.LoadFromEnumerable(data);
            testTrainSplit = context.Data.TrainTestSplit(trainData, testFraction: 0.30);
            pipeline = context.Transforms.Concatenate("Features", new[] { "Stores", "AlarmItems", "TempMean", "Humidity", "Pressure", "TempMin", "TempMax" })
            .Append(context.Regression.Trainers.FastTreeTweedie());
            model = pipeline.Fit(testTrainSplit.TrainSet);
            predictionFunc = context.Model.CreatePredictionEngine<TemperatureData, AlarmPressurePrediction>(model);
        }


        internal int Predict(TemperatureData data)
        {
            int returnValue = Convert.ToInt32(predictionFunc.Predict(data).PredictedAlarmPressure);
            return returnValue;
        } 
    }
}
