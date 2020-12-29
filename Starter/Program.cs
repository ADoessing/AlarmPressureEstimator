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
            int sleeptimer = 86400000;


            while (true)
            {
                var values2 = databaseFacade.GetNewestWeatherforecastByStationId("06123", 1);
                var today = values.Item2.Substring(6, 4) + "-" + values.Item2.ToString().Substring(3, 2) + "-" + values.Item2.ToString().Substring(0, 2);
                if (String.Equals(today, yesterday))
                {
                    Thread.Sleep(1800000);
                    if (!String.Equals(today, yesterday))
                    {
                        logicFacade.CreatePredictionsForAllStations();
                        yesterday = values.Item2.Substring(6, 4) + "-" + values.Item2.ToString().Substring(3, 2) + "-" + values.Item2.ToString().Substring(0, 2);
                    }
                }

                else
                {
                    logicFacade.CreatePredictionsForAllStations();
                    yesterday = values.Item2.Substring(6, 4) + "-" + values.Item2.ToString().Substring(3, 2) + "-" + values.Item2.ToString().Substring(0, 2);
                }



                if (DateTime.Now.AddDays(1).Day == 1)
                {
                    databaseFacade.UpdateDataset(DateTime.Now.AddMonths(-1).Date.ToString().Substring(6, 4) + "-" + DateTime.Now.AddMonths(-1).Date.ToString().Substring(3, 2) + "-" + DateTime.Now.AddMonths(-1).Date.ToString().Substring(0, 2));
                    databaseFacade = DatabaseFacade.GetInstance();
                    logicFacade = LogicFacade.GetInstance();
                }

                if (String.Equals(today, yesterday))
                {
                    Thread.Sleep(sleeptimer - 1800000);
                }
                else
                {
                    Thread.Sleep(sleeptimer);
                }

            }
        }

    }
}
