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
using alarmpressureestimator.Data;
using alarmpressureestimator.Aquaintance;
using alarmpressureestimator.Logic;

namespace alarmpressureestimator.Starter
{
    class Program
    {
        static void Main(string[] args)
        {
            IDatabaseFacade databaseFacade = DatabaseFacade.GetInstance();
            ILogicFacade logicFacade = LogicFacade.GetInstance();
            var values = databaseFacade.GetNewestWeatherforecastByStationId("06123", 1);
            var yesterday = values.Item2.Substring(6, 4) + "-" + values.Item2.ToString().Substring(3, 2) + "-" + (Convert.ToInt32(values.Item2.ToString().Substring(0, 2)) - 1).ToString();
            //int sleeptimer = 86400000;

            Console.WriteLine(logicFacade.Predict(new TemperatureData()
            {
                Stores = 55F,
                AlarmItems = 1607F,
                TempMean = 13.10F,
                Humidity = 83.541666F,
                Pressure = 1025.062500F,
                TempMin = 9.6F,
                TempMax = 17.2F
            }));

            Console.WriteLine("ALL");
            //logicFacade.CreatePredictionsForAllStations();
            Console.WriteLine("Done");

            //while (true)
            //{
            //    var values2 = databaseFacade.GetNewestWeatherforecastByStationId("06123", 1);
            //    var today = values.Item2.Substring(6, 4) + "-" + values.Item2.ToString().Substring(3, 2) + "-" + values.Item2.ToString().Substring(0, 2);
            //    if (String.Equals(today, yesterday))
            //    {
            //        Thread.Sleep(1800000);
            //    }
            //    else
            //    {
            //        logicFacade.CreatePredictionsForAllStations();
            //        yesterday = values.Item2.Substring(6, 4) + "-" + values.Item2.ToString().Substring(3, 2) + "-" + values.Item2.ToString().Substring(0, 2);
            //    }



            //    if (DateTime.Now.AddDays(1).Day == 1)
            //    {
            //        databaseFacade.UpdateDataset(DateTime.Now.AddMonths(-1).Date.ToString().Substring(6, 4) + "-" + DateTime.Now.AddMonths(-1).Date.ToString().Substring(3, 2) + "-" + DateTime.Now.AddMonths(-1).Date.ToString().Substring(0, 2));
            //        databaseFacade = DatabaseFacade.GetInstance();
            //        logicFacade = LogicFacade.GetInstance();
            //    }

            //    if (String.Equals(today, yesterday))
            //    {
            //        Thread.Sleep(sleeptimer - 1800000);
            //    }
            //    else
            //    {
            //        Thread.Sleep(sleeptimer);
            //    }

            //}
        }

    }
}
