using Microsoft.ML.Trainers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using temperaturepredictor.Logic;

namespace temperaturepredictor.Data
{
    public class DbHelper
    {
        private string ConnectionString = "Server=akctest01.database.windows.net;Database=akctestdb01;uid=DMIuserLogin;password=DmiLogin34!DK;Trusted_Connection=false";
        List<string> KnownGoodStations = new List<string>() {/* "06056", "06068", "06116", "06126", "06123", "06032", "06074", "06119", "06149", "06065",*/ "06124", "06058", "06041", "06049", "06147" };
        List<string> MoreStations = new List<string>() { /*"06159", "06135", "06132", "06186", "06082", "06073", "06072", "06102", "06183", "06096", "06188", "06138", "06141", "06031", "06081", "06093", "06168", "06088", "06169", "06136", "06174", "06154", "06193", "06156", "06019",*/ "06197", "06181", "06184", "06051" };

        internal List<string> GetStations()
        {
            List<string> stations = new List<string>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                SqlCommand cmd1 = new SqlCommand("SELECT DISTINCT StationId FROM Subscriptions", conn);
                {
                    conn.Open();
                    using SqlDataReader reader = cmd1.ExecuteReader();

                    while (reader.Read())
                    {
                        stations.Add((string)reader["StationId"]);
                    }
                }
            }
            return stations;
        }

        private List<string> GetObservationDates(string ID)
        {
            List<string> oberservationDates = new List<string>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                SqlCommand cmd = new SqlCommand($"SELECT DISTINCT Observations.DateKey From Observations Where Observations.StationId={ID} Order by DateKey", conn);
                {
                    conn.Open();
                    using SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        oberservationDates.Add(reader["DateKey"].ToString().Substring(6, 4) + "-" + reader["DateKey"].ToString().Substring(3, 2) + "-" + reader["DateKey"].ToString().Substring(0, 2));
                    }
                }
            }
            return oberservationDates;
        }

        private List<string> GetObservationDatesBetween(string ID, string Date)
        {
            List<string> oberservationDates = new List<string>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                SqlCommand cmd = new SqlCommand($"SELECT DISTINCT Observations.DateKey From Observations Where Observations.StationId={ID} AND DateKey BETWEEN '{Date}' AND (SELECT MAX(DateKey) From Observations) Order by DateKey", conn);
                {
                    conn.Open();
                    using SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        oberservationDates.Add(reader["DateKey"].ToString().Substring(6, 4) + "-" + reader["DateKey"].ToString().Substring(3, 2) + "-" + reader["DateKey"].ToString().Substring(0, 2));
                    }
                }
            }
            return oberservationDates;
        }

        internal void SaveExtendedAlarmDataset()
        {
            List<string> stations = MoreStations;
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {

                {
                    conn.Open();
                    for (int i = 0; i < stations.Count; i++)
                    {
                        Console.WriteLine("test");
                        List<string> dates = GetObservationDates(stations[i]);
                        for (int counter = 0; counter < dates.Count; counter++)
                        {
                            Console.WriteLine("test igen");

                            SqlCommand cmd2 = new SqlCommand($"SELECT TOP 1 MAX(Stores.Stores) as Stores, MAX(Alarm.Alarms) as Alarms, MAX(AlarmItem.AlarmItems) as AlarmItems, AVG(ALL case when Observations.ParameterId='temp_mean_past1h' then Observations.Value end) as TempMean, AVG(ALL case when Observations.ParameterId= 'humidity_past1h' then Observations.Value end) as Humidty, AVG(ALL case when Observations.ParameterId= 'pressure_at_sea' then Observations.Value end) as Pressure, min(ALL case when Observations.ParameterId= 'temp_min_past1h' then Observations.Value end) as TempMin, max(ALL case when Observations.ParameterId= 'temp_max_past1h' then Observations.Value end) as TempMax FROM Observations JOIN (SELECT Alarms.DateKey, SUM(ALL Alarms.AlarmCount) as Alarms From Alarms Where Alarms.StationId = {stations[i]} AND Alarms.IsValid = 1 Group by Alarms.DateKey) as Alarm ON Alarm.DateKey = Observations.DateKey JOIN (SELECT DISTINCT Subscriptions.StationId, COUNT(Subscriptions.ID) as Stores FROM Subscriptions Group by Subscriptions.StationId) as Stores ON Stores.StationId = Observations.StationId JOIN(SELECT SUM(Subscriptions.AlarmItems) as AlarmItems, MIN(Subscriptions.StartDate) as StartDate, Subscriptions.StationId FROM Subscriptions Where Subscriptions.StartDate Between StartDate AND '{dates[counter]}' Group by Subscriptions.StationId) as AlarmItem ON AlarmItem.StationId = Observations.StationId Where Observations.StationId = {stations[i]} AND Observations.IsValid = 1 AND Alarm.Datekey = '{dates[counter]}' Group by Alarm.DateKey order by Alarm.Datekey", conn);
                            using SqlDataReader reader = cmd2.ExecuteReader();
                            Console.WriteLine("test igen igen");

                            using (StreamWriter writer = new StreamWriter(@"C:\Users\Asmus\Source\Repos\ADoessing\AlarmPressureEstimator\Csvs\AlarmDataTestAllStations(2).csv", true))
                            {
                                Console.WriteLine("endnu en test");
                                if (i == 0 && counter == 0)
                                {
                                    writer.WriteLine("Stores,Alarms,AlarmItems,TempMean,Humidity,Pressure,TempMin,TempMax");
                                }
                                while (reader.Read())
                                {

                                    if (String.Equals(reader["Stores"].ToString().Replace(',', '.'), "") || String.Equals(reader["Alarms"].ToString().Replace(',', '.'), "") || String.Equals(reader["TempMean"].ToString().Replace(',', '.'), "") || String.Equals(reader["Humidty"].ToString().Replace(',', '.'), "") || String.Equals(reader["Pressure"].ToString().Replace(',', '.'), "") || String.Equals(reader["TempMin"].ToString().Replace(',', '.'), "") || String.Equals(reader["TempMax"].ToString().Replace(',', '.'), ""))
                                    {
                                        continue;
                                    }

                                    writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7}",
                                             reader["Stores"].ToString().Replace(',', '.'), reader["Alarms"].ToString().Replace(',', '.'), reader["AlarmItems"].ToString().Replace(',', '.'), reader["TempMean"].ToString().Replace(',', '.'), reader["Humidty"].ToString().Replace(',', '.'), reader["Pressure"].ToString().Replace(',', '.'), reader["TempMin"].ToString().Replace(',', '.'), reader["TempMax"].ToString().Replace(',', '.'));
                                }
                            }
                        }
                        dates.Clear();
                    }

                }
            }
        }

        internal void SavePerfectAlarmDataset()
        {
            List<string> stations = MoreStations;
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {

                {
                    conn.Open();
                    for (int i = 0; i < stations.Count; i++)
                    {
                        Console.WriteLine("test");
                        List<string> dates = GetObservationDates(stations[i]);
                        for (int counter = 0; counter < dates.Count; counter++)
                        {
                            Console.WriteLine("test igen");

                            SqlCommand cmd2 = new SqlCommand($"SELECT TOP 1 MAX(Stores.Stores) as Stores, MAX(Alarm.Alarms) as Alarms, MAX(AlarmItem.AlarmItems) as AlarmItems, IIF(NULLIF((SELECT TOP 1 AVG(ALL case when Observations.ParameterId= 'temp_mean_past1h' then Observations.Value end) as TempMean FROM Observations Where StationId = {stations[i]} Group by DateKey), NULL) IS NULL, (SELECT TOP 1 AVG(ALL case when Observations.ParameterId= 'temp_mean_past1h' then Observations.Value end) as TempMean FROM Observations Where RegionId = (SELECT TOP 1 RegionId FROM Subscriptions Where StationId = {stations[i]}) AND DateKey ='{dates[counter]}' Group by DateKey), (SELECT TOP 1 AVG(ALL case when Observations.ParameterId= 'temp_mean_past1h' then Observations.Value end) as TempMean FROM Observations where StationId = {stations[i]} AND DateKey ='{dates[counter]}' Group by DateKey)) as TempMean, IIF(NULLIF((SELECT TOP 1 AVG(ALL case when Observations.ParameterId= 'humidity_past1h' then Observations.Value end) as Humidty FROM Observations Where StationId = {stations[i]} Group by DateKey), NULL) IS NULL, (SELECT TOP 1 AVG(ALL case when Observations.ParameterId= 'humidity_past1h' then Observations.Value end) as Humidty FROM Observations Where RegionId = (SELECT TOP 1 RegionId FROM Subscriptions Where StationId = {stations[i]}) AND DateKey ='{dates[counter]}' Group by DateKey), (SELECT TOP 1 AVG(ALL case when Observations.ParameterId= 'humidity_past1h' then Observations.Value end) as Humidty FROM Observations where StationId = {stations[i]} AND DateKey ='{dates[counter]}' Group by DateKey)) as Humidty, IIF(NULLIF((SELECT TOP 1 AVG(ALL case when Observations.ParameterId= 'pressure_at_sea' then Observations.Value end) as Pressure FROM Observations Where StationId = {stations[i]} Group by DateKey), NULL) IS NULL, (SELECT TOP 1 AVG(ALL case when Observations.ParameterId= 'pressure_at_sea' then Observations.Value end) as Pressure FROM Observations Where RegionId = (SELECT TOP 1 RegionId FROM Subscriptions Where StationId = {stations[i]}) AND DateKey ='{dates[counter]}' Group by DateKey), (SELECT TOP 1 AVG(ALL case when Observations.ParameterId= 'pressure_at_sea' then Observations.Value end) as Pressure FROM Observations where StationId = {stations[i]} AND DateKey ='{dates[counter]}' Group by DateKey)) as Pressure, IIF(NULLIF((SELECT TOP 1 MIN(ALL case when Observations.ParameterId= 'temp_min_past1h' then Observations.Value end) as TempMin FROM Observations Where StationId = {stations[i]} Group by DateKey), NULL) IS NULL, (SELECT TOP 1 MIN(ALL case when Observations.ParameterId= 'temp_min_past1h' then Observations.Value end) as TempMin FROM Observations Where RegionId = (SELECT TOP 1 RegionId FROM Subscriptions Where StationId = {stations[i]}) AND DateKey ='{dates[counter]}' Group by DateKey), (SELECT TOP 1 MIN(ALL case when Observations.ParameterId= 'temp_min_past1h' then Observations.Value end) as TempMin FROM Observations where StationId = {stations[i]} AND DateKey ='{dates[counter]}' Group by DateKey)) as TempMin, IIF(NULLIF((SELECT TOP 1 MAX(ALL case when Observations.ParameterId= 'temp_max_past1h' then Observations.Value end) as TempMax FROM Observations Where StationId = {stations[i]} Group by DateKey), NULL) IS NULL, (SELECT TOP 1 MAX(ALL case when Observations.ParameterId= 'temp_max_past1h' then Observations.Value end) as TempMax FROM Observations Where RegionId = (SELECT TOP 1 RegionId FROM Subscriptions Where StationId = {stations[i]}) AND DateKey ='{dates[counter]}' Group by DateKey), (SELECT TOP 1 MAX(ALL case when Observations.ParameterId= 'temp_max_past1h' then Observations.Value end) as TempMax FROM Observations where StationId = {stations[i]} AND DateKey ='{dates[counter]}' Group by DateKey)) as TempMax FROM Observations JOIN (SELECT Alarms.DateKey, SUM(ALL Alarms.AlarmCount) as Alarms From Alarms Where Alarms.StationId = {stations[i]} AND Alarms.IsValid = 1 Group by Alarms.DateKey) as Alarm ON Alarm.DateKey = Observations.DateKey JOIN (SELECT DISTINCT Subscriptions.StationId, COUNT(Subscriptions.ID) as Stores, MIN(Subscriptions.StartDate) as StartDate FROM Subscriptions WHERE Subscriptions.StartDate BETWEEN (SELECT MIN(Subscriptions.StartDate) FROM Subscriptions where StationId = {stations[i]}) AND '{dates[counter]}' Group by Subscriptions.StationId) as Stores ON Stores.StationId = Observations.StationId JOIN(SELECT SUM(Subscriptions.AlarmItems) as AlarmItems, Subscriptions.StationId FROM Subscriptions Where Subscriptions.StartDate Between (SELECT MIN(Subscriptions.StartDate) FROM Subscriptions Where StationId = {stations[i]}) AND '{dates[counter]}' Group by Subscriptions.StationId) as AlarmItem ON AlarmItem.StationId = Observations.StationId Where Observations.StationId = {stations[i]} AND Observations.IsValid = 1 AND Alarm.Datekey = '{dates[counter]}' Group by Alarm.DateKey order by Alarm.Datekey", conn);
                            using SqlDataReader reader = cmd2.ExecuteReader();
                            Console.WriteLine("test igen igen");

                            using (StreamWriter writer = new StreamWriter(@"D:\Users\Asmus\LastSetMoreStations.csv", true))
                            {
                                Console.WriteLine("endnu en test");
                                if (i == 0 && counter == 0)
                                {
                                    writer.WriteLine("Stores,Alarms,AlarmItems,TempMean,Humidity,Pressure,TempMin,TempMax");
                                }
                                while (reader.Read())
                                {

                                    if (String.Equals(reader["Stores"].ToString().Replace(',', '.'), "") || String.Equals(reader["Alarms"].ToString().Replace(',', '.'), "") || String.Equals(reader["TempMean"].ToString().Replace(',', '.'), "") || String.Equals(reader["Humidty"].ToString().Replace(',', '.'), "") || String.Equals(reader["Pressure"].ToString().Replace(',', '.'), "") || String.Equals(reader["TempMin"].ToString().Replace(',', '.'), "") || String.Equals(reader["TempMax"].ToString().Replace(',', '.'), ""))
                                    {
                                        continue;
                                    }

                                    writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7}",
                                             reader["Stores"].ToString().Replace(',', '.'), reader["Alarms"].ToString().Replace(',', '.'), reader["AlarmItems"].ToString().Replace(',', '.'), reader["TempMean"].ToString().Replace(',', '.'), reader["Humidty"].ToString().Replace(',', '.'), reader["Pressure"].ToString().Replace(',', '.'), reader["TempMin"].ToString().Replace(',', '.'), reader["TempMax"].ToString().Replace(',', '.'));
                                }
                            }
                        }
                        dates.Clear();
                    }

                }
            }
        }

        internal void UpdateDataset(string date)
        {
            List<string> newLines = new List<string>();
            List<string> stations = GetStations();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                {
                    conn.Open();
                    for (int i = 0; i < stations.Count; i++)
                    {

                        List<string> dates = GetObservationDatesBetween(stations[i], date);
                        for (int counter = 0; counter < dates.Count; counter++)
                        {
                            SqlCommand cmd2 = new SqlCommand($"SELECT TOP 1 MAX(Stores.Stores) as Stores, MAX(Alarm.Alarms) as Alarms, MAX(AlarmItem.AlarmItems) as AlarmItems, IIF(NULLIF((SELECT TOP 1 AVG(ALL case when Observations.ParameterId= 'temp_mean_past1h' then Observations.Value end) as TempMean FROM Observations Where StationId = {stations[i]} Group by DateKey), NULL) IS NULL, (SELECT TOP 1 AVG(ALL case when Observations.ParameterId= 'temp_mean_past1h' then Observations.Value end) as TempMean FROM Observations Where RegionId = (SELECT TOP 1 RegionId FROM Subscriptions Where StationId = {stations[i]}) AND DateKey ='{dates[counter]}' Group by DateKey), (SELECT TOP 1 AVG(ALL case when Observations.ParameterId= 'temp_mean_past1h' then Observations.Value end) as TempMean FROM Observations where StationId = {stations[i]} AND DateKey ='{dates[counter]}' Group by DateKey)) as TempMean, IIF(NULLIF((SELECT TOP 1 AVG(ALL case when Observations.ParameterId= 'humidity_past1h' then Observations.Value end) as Humidty FROM Observations Where StationId = {stations[i]} Group by DateKey), NULL) IS NULL, (SELECT TOP 1 AVG(ALL case when Observations.ParameterId= 'humidity_past1h' then Observations.Value end) as Humidty FROM Observations Where RegionId = (SELECT TOP 1 RegionId FROM Subscriptions Where StationId = {stations[i]}) AND DateKey ='{dates[counter]}' Group by DateKey), (SELECT TOP 1 AVG(ALL case when Observations.ParameterId= 'humidity_past1h' then Observations.Value end) as Humidty FROM Observations where StationId = {stations[i]} AND DateKey ='{dates[counter]}' Group by DateKey)) as Humidty, IIF(NULLIF((SELECT TOP 1 AVG(ALL case when Observations.ParameterId= 'pressure_at_sea' then Observations.Value end) as Pressure FROM Observations Where StationId = {stations[i]} Group by DateKey), NULL) IS NULL, (SELECT TOP 1 AVG(ALL case when Observations.ParameterId= 'pressure_at_sea' then Observations.Value end) as Pressure FROM Observations Where RegionId = (SELECT TOP 1 RegionId FROM Subscriptions Where StationId = {stations[i]}) AND DateKey ='{dates[counter]}' Group by DateKey), (SELECT TOP 1 AVG(ALL case when Observations.ParameterId= 'pressure_at_sea' then Observations.Value end) as Pressure FROM Observations where StationId = {stations[i]} AND DateKey ='{dates[counter]}' Group by DateKey)) as Pressure, IIF(NULLIF((SELECT TOP 1 MIN(ALL case when Observations.ParameterId= 'temp_min_past1h' then Observations.Value end) as TempMin FROM Observations Where StationId = {stations[i]} Group by DateKey), NULL) IS NULL, (SELECT TOP 1 MIN(ALL case when Observations.ParameterId= 'temp_min_past1h' then Observations.Value end) as TempMin FROM Observations Where RegionId = (SELECT TOP 1 RegionId FROM Subscriptions Where StationId = {stations[i]}) AND DateKey ='{dates[counter]}' Group by DateKey), (SELECT TOP 1 MIN(ALL case when Observations.ParameterId= 'temp_min_past1h' then Observations.Value end) as TempMin FROM Observations where StationId = {stations[i]} AND DateKey ='{dates[counter]}' Group by DateKey)) as TempMin, IIF(NULLIF((SELECT TOP 1 MAX(ALL case when Observations.ParameterId= 'temp_max_past1h' then Observations.Value end) as TempMax FROM Observations Where StationId = {stations[i]} Group by DateKey), NULL) IS NULL, (SELECT TOP 1 MAX(ALL case when Observations.ParameterId= 'temp_max_past1h' then Observations.Value end) as TempMax FROM Observations Where RegionId = (SELECT TOP 1 RegionId FROM Subscriptions Where StationId = {stations[i]}) AND DateKey ='{dates[counter]}' Group by DateKey), (SELECT TOP 1 MAX(ALL case when Observations.ParameterId= 'temp_max_past1h' then Observations.Value end) as TempMax FROM Observations where StationId = {stations[i]} AND DateKey ='{dates[counter]}' Group by DateKey)) as TempMax FROM Observations JOIN (SELECT Alarms.DateKey, SUM(ALL Alarms.AlarmCount) as Alarms From Alarms Where Alarms.StationId = {stations[i]} AND Alarms.IsValid = 1 Group by Alarms.DateKey) as Alarm ON Alarm.DateKey = Observations.DateKey JOIN (SELECT DISTINCT Subscriptions.StationId, COUNT(Subscriptions.ID) as Stores, MIN(Subscriptions.StartDate) as StartDate FROM Subscriptions WHERE Subscriptions.StartDate BETWEEN (SELECT MIN(Subscriptions.StartDate) FROM Subscriptions where StationId = {stations[i]}) AND '{dates[counter]}' Group by Subscriptions.StationId) as Stores ON Stores.StationId = Observations.StationId JOIN(SELECT SUM(Subscriptions.AlarmItems) as AlarmItems, Subscriptions.StationId FROM Subscriptions Where Subscriptions.StartDate Between (SELECT MIN(Subscriptions.StartDate) FROM Subscriptions Where StationId = {stations[i]}) AND '{dates[counter]}' Group by Subscriptions.StationId) as AlarmItem ON AlarmItem.StationId = Observations.StationId Where Observations.StationId = {stations[i]} AND Observations.IsValid = 1 AND Alarm.Datekey = '{dates[counter]}' Group by Alarm.DateKey order by Alarm.Datekey", conn);
                            using SqlDataReader reader = cmd2.ExecuteReader();


                            while (reader.Read())
                            {
                                if (String.Equals(reader["Stores"].ToString().Replace(',', '.'), "") || String.Equals(reader["Alarms"].ToString().Replace(',', '.'), "") || String.Equals(reader["TempMean"].ToString().Replace(',', '.'), "") || String.Equals(reader["Humidty"].ToString().Replace(',', '.'), "") || String.Equals(reader["Pressure"].ToString().Replace(',', '.'), "") || String.Equals(reader["TempMin"].ToString().Replace(',', '.'), "") || String.Equals(reader["TempMax"].ToString().Replace(',', '.'), ""))
                                {
                                    continue;
                                }
                                newLines.Add(reader["Stores"].ToString().Replace(',', '.') + "," + reader["Alarms"].ToString().Replace(',', '.') + "," + reader["AlarmItems"].ToString().Replace(',', '.') + "," + reader["TempMean"].ToString().Replace(',', '.') + "," + reader["Humidty"].ToString().Replace(',', '.') + "," + reader["Pressure"].ToString().Replace(',', '.') + "," + reader["TempMin"].ToString().Replace(',', '.') + "," + reader["TempMax"].ToString().Replace(',', '.'));
                            }

                        }
                        dates.Clear();
                    }
                    conn.Close();
                }
                {
                    conn.Open();
                    for (int i = 0; i < newLines.Count; i++)
                    {
                        SqlCommand cmd = new SqlCommand($"INSERT INTO Dataset(Stores, Alarms, AlarmItems, TempMean, Humidity, Pressure, TempMin, TempMax) Values(@Stores, @Alarms, @AlarmItems, @TempMean, @Humidity, @Pressure, @TempMin, @TempMax)", conn);
                        cmd.Parameters.AddWithValue("@Stores", newLines[i].Split(",")[0]);
                        cmd.Parameters.AddWithValue("@Alarms", newLines[i].Split(",")[1]);
                        cmd.Parameters.AddWithValue("@AlarmItems", newLines[i].Split(",")[2]);
                        cmd.Parameters.AddWithValue("@TempMean", newLines[i].Split(",")[3]);
                        cmd.Parameters.AddWithValue("@Humidity", newLines[i].Split(",")[4]);
                        cmd.Parameters.AddWithValue("@Pressure", newLines[i].Split(",")[5]);
                        cmd.Parameters.AddWithValue("@TempMin", newLines[i].Split(",")[6]);
                        cmd.Parameters.AddWithValue("@TempMax", newLines[i].Split(",")[7]);
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
            }
        }

        internal void SaveAlarmDataset()
        {
            List<string> stations = KnownGoodStations;
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {

                {
                    conn.Open();
                    for (int i = 0; i < stations.Count; i++)
                    {
                        SqlCommand cmd = new SqlCommand($"SELECT MAX(Stores.Stores) as Stores, MAX(Alarm.Alarms) as Alarms, AVG(ALL case when Observations.ParameterId='temp_mean_past1h' then Observations.Value end) as TempMean, AVG(ALL case when Observations.ParameterId= 'humidity_past1h' then Observations.Value end) as Humidty, AVG(ALL case when Observations.ParameterId= 'pressure_at_sea' then Observations.Value end) as Pressure, min(ALL case when Observations.ParameterId= 'temp_min_past1h' then Observations.Value end) as TempMin, max(ALL case when Observations.ParameterId= 'temp_max_past1h' then Observations.Value end) as TempMax FROM Observations JOIN (SELECT Alarms.DateKey, SUM(ALL Alarms.AlarmCount) as Alarms From Alarms Where Alarms.StationId = {stations[i]} AND Alarms.IsValid = 1 Group by Alarms.DateKey) as Alarm ON Alarm.DateKey = Observations.DateKey JOIN (SELECT DISTINCT Subscriptions.StationId, COUNT(Subscriptions.ID) as Stores FROM Subscriptions Group by Subscriptions.StationId) as Stores ON Stores.StationId = Observations.StationId Where Observations.StationId = {stations[i]} AND Observations.IsValid = 1 Group by Alarm.DateKey order by Alarm.Datekey", conn);
                        using SqlDataReader reader = cmd.ExecuteReader();

                        using (StreamWriter writer = new StreamWriter(@"C:\Users\Asmus\source\repos\temperaturepredictor\AlarmDataTest5.csv", true))
                        {
                            if (i == 0)
                            {
                                writer.WriteLine("Stores,Alarms,TempMean,Humidity,Pressure,TempMin,TempMax");
                            }
                            while (reader.Read())
                            {

                                if (String.Equals(reader["Stores"].ToString().Replace(',', '.'), "") || String.Equals(reader["Alarms"].ToString().Replace(',', '.'), "") || String.Equals(reader["TempMean"].ToString().Replace(',', '.'), "") || String.Equals(reader["Humidty"].ToString().Replace(',', '.'), "") || String.Equals(reader["Pressure"].ToString().Replace(',', '.'), "") || String.Equals(reader["TempMin"].ToString().Replace(',', '.'), "") || String.Equals(reader["TempMax"].ToString().Replace(',', '.'), ""))
                                {
                                    continue;
                                }

                                writer.WriteLine("{0},{1},{2},{3},{4},{5},{6}",
                                         reader["Stores"].ToString().Replace(',', '.'), reader["Alarms"].ToString().Replace(',', '.'), reader["TempMean"].ToString().Replace(',', '.'), reader["Humidty"].ToString().Replace(',', '.'), reader["Pressure"].ToString().Replace(',', '.'), reader["TempMin"].ToString().Replace(',', '.'), reader["TempMax"].ToString().Replace(',', '.'));
                            }
                        }
                    }

                }
            }
        }


        internal (TemperatureData, string, int) GetNewestWeatherforecastByStationId(string StationId, int day)
        {
            TemperatureData temperatureData = new TemperatureData { };
            string date = "";
            int forecastDay = 0;
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                SqlCommand sqlCommand = new SqlCommand($"SELECT Max(Date) as Date, Max(Stores.Stores) as Stores, MAX(TempMean) as TempMean, AVG(Humidity) as Humidity, AVG(Pressure) as Pressure, min(TempMin) as TempMin, max(TempMax) as TempMax, Max(ForecastDay) as ForecastDay, Max(AlarmItem.AlarmItems) as AlarmItems FROM ForecastsFromArima JOIN (SELECT DISTINCT Subscriptions.StationId, COUNT(Subscriptions.ID) as Stores FROM Subscriptions Group by StationId) as Stores on Stores.StationId = ForecastsFromArima.StationId JOIN(SELECT SUM(Subscriptions.AlarmItems) as AlarmItems, MIN(Subscriptions.StartDate) as StartDate, Subscriptions.StationId FROM Subscriptions Group by Subscriptions.StationId) as AlarmItem ON AlarmItem.StationId = ForecastsFromArima.StationId WHERE Date = (SELECT MAX(DATE) FROM ForecastsFromArima) AND ForecastsFromArima.StationId = {StationId} AND ForecastsFromArima.ForecastDay = {day}", conn);
                {
                    conn.Open();
                    using SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                    while (sqlDataReader.Read())
                    {
                        temperatureData = new TemperatureData
                        {
                            Stores = Convert.ToSingle(sqlDataReader["Stores"]),
                            AlarmItems = Convert.ToSingle(sqlDataReader["AlarmItems"]),
                            TempMean = Convert.ToSingle(sqlDataReader["TempMean"]),
                            Humidity = Convert.ToSingle(sqlDataReader["Humidity"]),
                            Pressure = Convert.ToSingle(sqlDataReader["Pressure"]),
                            TempMin = Convert.ToSingle(sqlDataReader["TempMin"]),
                            TempMax = Convert.ToSingle(sqlDataReader["TempMax"])
                        };
                        date = sqlDataReader["Date"].ToString();
                        forecastDay = Convert.ToInt32(sqlDataReader["ForecastDay"].ToString());

                    }
                    conn.Close();
                }
            }
            return (temperatureData, date, forecastDay);
        }

        internal void UploadNewestAlarmPrediction(string Date, string StationId, int Alarms)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                SqlCommand cmd = new SqlCommand($"INSERT INTO AlarmPressureByStation(Date, StationId, Alarms) Values(@Date, @StationId, @Alarms)", conn);
                cmd.Parameters.AddWithValue("@Date", Date);
                cmd.Parameters.AddWithValue("@StationId", StationId);
                cmd.Parameters.AddWithValue("@Alarms", Alarms);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }



        internal int GetRegionIdFromStationId(string StationId)
        {
            int regionId = 0;
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                SqlCommand cmd = new SqlCommand($"SELECT DISTINCT RegionId FROM Stations WHERE Id={StationId.Trim('"')}", conn);
                conn.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    regionId = Convert.ToInt32(reader["RegionId"].ToString());
                }
            }
            return regionId;
        }


        internal List<TemperatureData> GetDataSet()
        {
            List<TemperatureData> Dataset = new List<TemperatureData>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT Stores, Alarms, AlarmItems, TempMean, Humidity, Pressure, TempMin, TempMax FROM Dataset", conn);
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Dataset.Add(new TemperatureData
                    {
                        Stores = Convert.ToSingle(reader["Stores"].ToString()),
                        Alarms = Convert.ToSingle(reader["Alarms"].ToString()),
                        AlarmItems = Convert.ToSingle(reader["AlarmItems"].ToString()),
                        TempMean = Convert.ToSingle(reader["TempMean"].ToString()),
                        Humidity = Convert.ToSingle(reader["Humidity"].ToString()),
                        Pressure = Convert.ToSingle(reader["Pressure"].ToString()),
                        TempMin = Convert.ToSingle(reader["TempMin"].ToString()),
                        TempMax = Convert.ToSingle(reader["TempMax"].ToString())
                    });

                }
                conn.Close();
            }
            return Dataset;
        }


        internal void fillDataSetTable(int Stores, int Alarms, int AlarmItems, double TempMean, double Humidity, double Pressure, double TempMin, double TempMax)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("INSERT INTO Dataset(Stores, Alarms, AlarmItems, TempMean, Humidity, Pressure, TempMin, TempMax) VALUES(@Stores, @Alarms, @AlarmItems, @TempMean, @Humidity, @Pressure, @TempMin, @TempMax)", conn);
                cmd.Parameters.AddWithValue("@Stores", Stores);
                cmd.Parameters.AddWithValue("@Alarms", Alarms);
                cmd.Parameters.AddWithValue("@AlarmItems", AlarmItems);
                cmd.Parameters.AddWithValue("@TempMean", TempMean);
                cmd.Parameters.AddWithValue("@Humidity", Humidity);
                cmd.Parameters.AddWithValue("@Pressure", Pressure);
                cmd.Parameters.AddWithValue("@TempMin", TempMin);
                cmd.Parameters.AddWithValue("@TempMax", TempMax);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }


        }
    }
}
