﻿using Microsoft.Extensions.Configuration;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http;

namespace temperaturepredictor
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            DdHelper ddHelper = new DdHelper();
            var context = new MLContext();

            //var builder = new ConfigurationBuilder();

            //var configuration = builder.Build();
            //var connectionString = ddHelper.GetConnectionString();
            //var columnLoader = new DatabaseLoader.Column[]
            //{
            //    new DatabaseLoader.Column() {Name="Stores",Type=System.Data.DbType.Int32},
            //    new DatabaseLoader.Column() {Name="Alarms",Type=System.Data.DbType.Int32},
            //    new DatabaseLoader.Column() {Name="TempMean",Type=System.Data.DbType.Decimal},
            //    new DatabaseLoader.Column() {Name="Humidity",Type=System.Data.DbType.Int32},
            //    new DatabaseLoader.Column() {Name="Pressure",Type=System.Data.DbType.Decimal},
            //    new DatabaseLoader.Column() {Name="TempMin",Type=System.Data.DbType.Decimal},
            //    new DatabaseLoader.Column() {Name="TempMax",Type=System.Data.DbType.Decimal}

            //};

            //var connection = new SqlConnection(connectionString);
            //var factory = DbProviderFactories.GetFactory(connection);
            //var loader = context.Data.CreateDatabaseLoader(columnLoader);

            //var dbSource = new DatabaseSource(factory, connectionString, "");

            //var trainData = loader.Load(dbSource);

            //Load data
            DataViewSchema modelSchema;
            ITransformer trainedModel;

            using (HttpClient client = new HttpClient())
            {
                Stream modelFile = await client.GetStreamAsync("https://cdn-125.anonfiles.com/Ve06Z7l1pc/b63dfdb6-1604313060/model.zip");

                trainedModel = context.Model.Load(modelFile, out modelSchema);
            }

            //var trainData = context.Data.LoadFromTextFile<TemperatureData>(@"C:\Users\Asmus\Source\Repos\ADoessing\AlarmPressureEstimator\Csvs\AlarmDataTestAllStationsPerfect.csv",
            //    hasHeader: true, separatorChar: ',');

            //splits data into test and train sets.
            //var testTrainSplit = context.Data.TrainTestSplit(trainData, testFraction: 0.30);
            // build the model
            //var pipeline = context.Transforms.Concatenate("Features", new[] { "Stores", "AlarmItems", "TempMean", "Humidity", "Pressure", "TempMin", "TempMax" })
            //    .Append(context.Regression.Trainers.FastTreeTweedie());

            //var model = pipeline.Fit(testTrainSplit.TrainSet);

            //context.Model.Save(model, trainData.Schema, "model.zip");

            //evalutae 
            //var predictions = model.Transform(testTrainSplit.TestSet);

            //var metrics = context.Regression.Evaluate(predictions);

            //Console.WriteLine($"R^2 - {metrics.RSquared}");
            //Console.WriteLine($"RMSE - {metrics.RootMeanSquaredError}");
            //predict
            var newData = new TemperatureData
            {
                Stores = 55F,
                AlarmItems = 1607F,
                TempMean = 13.10F,
                Humidity = 83.541666F,
                Pressure = 1025.062500F,
                TempMin = 9.6F,
                TempMax = 17.2F
            };
            var predictionFunc = context.Model.CreatePredictionEngine<TemperatureData, AlarmPressurePrediction>(trainedModel);

            var prediction = predictionFunc.Predict(newData);
            
            //DdHelper ddHelper = new DdHelper();
            //ddHelper.SavePerfectAlarmDataset();

            Console.WriteLine($"Prediction - {prediction.PredictedAlarmPressure}");
            Console.ReadLine();
            
        }

    }
}
