using Microsoft.ML;
using System;

namespace temperaturepredictor
{
    class Program
    {
        static void Main(string[] args)
        {

            var context = new MLContext();

            //Load data
            var trainData = context.Data.LoadFromTextFile<TemperatureData>(@"C:\Users\Asmus\source\repos\temperaturepredictor\AlarmDataTestAllStations.csv",
                hasHeader: true, separatorChar: ',');

            //splits data into test and train sets.
            var testTrainSplit = context.Data.TrainTestSplit(trainData, testFraction: 0.30);
            // build the model
            var pipeline = context.Transforms.Concatenate("Features", new[] { "Stores", "TempMean", "Humidity", "Pressure", "TempMin", "TempMax" })
                .Append(context.Regression.Trainers.FastTreeTweedie());

            var model = pipeline.Fit(testTrainSplit.TrainSet);

            //evalutae 
            var predictions = model.Transform(testTrainSplit.TestSet);

            var metrics = context.Regression.Evaluate(predictions);

            Console.WriteLine($"R^2 - {metrics.RSquared}");
            //predict
            var newData = new TemperatureData
            {
                Stores = 55F,
                TempMean = 13.10F,
                Humidity = 83.541666F,
                Pressure = 1025.062500F,
                TempMin = 9.6F,
                TempMax = 17.2F
            };
            var predictionFunc = context.Model.CreatePredictionEngine<TemperatureData, temperaturePrediction>(model);

            var prediction = predictionFunc.Predict(newData);

            //DdHelper ddHelper = new DdHelper();
            //ddHelper.SaveAlarmDataset();

            Console.WriteLine($"Prediction - {prediction.PredictedTemperature}");
            Console.ReadLine();

        }

    }
}
