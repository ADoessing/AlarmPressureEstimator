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
using temperaturepredictor.Data;
using temperaturepredictor.Aquaintance;
using temperaturepredictor.Logic;

namespace temperaturepredictor.Starter
{
    class Program
    {
        static void Main(string[] args)
        {
            IDatabaseFacade databaseFacade = DatabaseFacade.GetInstance();
            ILogicFacade logicFacade = LogicFacade.GetInstance();

            while (true)
            {

                logicFacade.CreatePredictionsForAllStations();


                if (DateTime.Now.AddDays(1).Day == 1)
                {
                    databaseFacade.UpdateDataset(DateTime.Now.AddMonths(-1).Date.ToString().Substring(6, 4) + "-" + DateTime.Now.AddMonths(-1).Date.ToString().Substring(3, 2) + "-" + DateTime.Now.AddMonths(-1).Date.ToString().Substring(0, 2));
                    logicFacade = LogicFacade.GetInstance();
                }

                Thread.Sleep(86400000);
            }
        }

    }
}
