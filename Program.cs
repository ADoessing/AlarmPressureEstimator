using Microsoft.Extensions.Configuration;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace temperaturepredictor
{
    class Program
    {
        static void Main(string[] args)
        {
            DbHelper ddHelper = new DbHelper();
            var context = new MLContext();

            //Load data

            //var trainData = context.Data.LoadFromTextFile<TemperatureData>(@"C:\Users\Asmus\Source\Repos\ADoessing\AlarmPressureEstimator\Csvs\LastSet4.csv",
            //    hasHeader: true, separatorChar: ',');

            var trainData = context.Data.LoadFromEnumerable(ddHelper.GetDataSet());

            //splits data into test and train sets.
            var testTrainSplit = context.Data.TrainTestSplit(trainData, testFraction: 0.30);
            // build the model
            var pipeline = context.Transforms.Concatenate("Features", new[] { "Stores", "AlarmItems", "TempMean", "Humidity", "Pressure", "TempMin", "TempMax" })
                .Append(context.Regression.Trainers.FastTreeTweedie());

            var model = pipeline.Fit(testTrainSplit.TrainSet);

            //evaluate
            //StreamWriter writer = new StreamWriter(@"C:\Users\Asmus\Source\Repos\ADoessing\AlarmPressureEstimator\Csvs\results.txt", true);
            var predictions = model.Transform(testTrainSplit.TestSet);

            var metrics = context.Regression.Evaluate(predictions);

            Console.WriteLine($"R^2 - {metrics.RSquared}");
            Console.WriteLine($"RMSE - {metrics.RootMeanSquaredError}");



            //predict
            var newData = new TemperatureData
            {
                Stores = Convert.ToSingle(84),
                AlarmItems = Convert.ToSingle(2301),
                TempMean = Convert.ToSingle(10.262500),
                Humidity = Convert.ToSingle(94.791666),
                Pressure = Convert.ToSingle(1015.554166),
                TempMin = Convert.ToSingle(8.4000),
                TempMax = Convert.ToSingle(12.1000)
            };
            var predictionFunc = context.Model.CreatePredictionEngine<TemperatureData, AlarmPressurePrediction>(model);

            var prediction = predictionFunc.Predict(newData);


            //ddHelper.SavePerfectAlarmDataset();

            //List<string> Stations = ddHelper.GetStations();
            //for (int i = 0; i < Stations.Count; i++)
            //{
            //    for (int j = 1; j <= 7; j++)
            //    {
            //        var values = ddHelper.GetNewestWeatherforecastByStationId(Stations[i], j);
            //        prediction = predictionFunc.Predict(values.Item1);
            //        var date = values.Item2.Substring(6, 4) + "-" + values.Item2.ToString().Substring(3, 2) + "-" + (Convert.ToInt32(values.Item2.ToString().Substring(0, 2)) + j).ToString();
            //        //Console.WriteLine(values.Item2.Substring(6, 4) + "-" + (Convert.ToInt32(values.Item2.ToString().Substring(0, 2)) + j).ToString() + "-" + values.Item2.ToString().Substring(3, 2));
            //        //Console.WriteLine(prediction.PredictedAlarmPressure);
            //        //Console.WriteLine(Convert.ToInt32(prediction.PredictedAlarmPressure));
            //        ddHelper.UploadNewestAlarmPrediction(date, Stations[i], Convert.ToInt32(prediction.PredictedAlarmPressure));
            //    }
            //}

            //Console.WriteLine("Færdig");
            //writer.WriteLine($"R^2 - {metrics.RSquared}" + ", " + $"RMSE - {metrics.RootMeanSquaredError}" + ", " + $"Prediction - {prediction.PredictedAlarmPressure}");

            //writer.Close();
            Console.WriteLine($"Prediction - {prediction.PredictedAlarmPressure}");
            //Console.ReadLine();

            //var items = File.ReadAllLines(@"C:\Users\Asmus\Source\Repos\ADoessing\AlarmPressureEstimator\Csvs\LastSetAll.csv").Skip(1).Select(line => line.Split(",")).Select(i => i);
            //foreach (string[] i in items)
            //{
            //    ddHelper.fillDataSetTable(Convert.ToInt32(i[0]), Convert.ToInt32(i[1]), Convert.ToInt32(i[2]), Convert.ToDouble(i[3].Replace('.', ',')), Convert.ToDouble(i[4].Replace('.', ',')), Convert.ToDouble(i[5].Replace('.', ',')), Convert.ToDouble(i[6].Replace('.', ',')), Convert.ToDouble(i[7].Replace('.', ',')));
            //}

            //if (DateTime.Now.AddDays(1).Day == 1)
            //{
            //    ddHelper.UpdateDataset(DateTime.Now.AddMonths(-1).Date.ToString().Substring(6, 4) + "-" + DateTime.Now.AddMonths(-1).Date.ToString().Substring(3, 2) + "-" + DateTime.Now.AddMonths(-1).Date.ToString().Substring(0, 2));
            //}

        }

    }
}
