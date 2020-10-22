using Microsoft.ML.Trainers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace temperaturepredictor
{
    public class DdHelper
    {
        private string ConnectionString = "Server=akctest01.database.windows.net;Database=akctestdb01;uid=DMIuserLogin;password=DmiLogin34!DK;Trusted_Connection=false";
        List<int> KnownGoodStations = new List<int>() { 06056, 06068, 06116, 06126, 06123, 06032, 06074, 06119, 06149, 06065, 06124, 06058, 06041, 06049, 06147 };

        public string GetConnectionString()
        {
            return ConnectionString;
        }

        private List<String> GetStations()
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

        private List<string> GetObservationDate(string ID)
        {
            List<string> oberservationDates = new List<string>();
            using(SqlConnection conn = new SqlConnection(ConnectionString))
            {
                SqlCommand cmd = new SqlCommand($"SELECT DISTINCT Observations.DateKey From Observations Where Observations.StationId={ID} Order by DateKey", conn);
                {
                    conn.Open();
                    using SqlDataReader reader = cmd.ExecuteReader();

                    while(reader.Read())
                    {
                        oberservationDates.Add(reader["DateKey"].ToString().Substring(6,4) + "-" + reader["DateKey"].ToString().Substring(3,2) + "-" + reader["DateKey"].ToString().Substring(0,2));
                    }
                }
            }
            return oberservationDates;
        }

        public void SaveExtendedAlarmDataset()
        {
            List<string> stations = GetStations();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {

                {
                    conn.Open();
                    for (int i = 0; i < stations.Count; i++)
                    {
                        List<string> dates = GetObservationDate(stations[i]);
                        for (int counter=0; counter<dates.Count; counter++) {
                            SqlCommand cmd2 = new SqlCommand($"SELECT MAX(Stores.Stores) as Stores, MAX(Alarm.Alarms) as Alarms, MAX(AlarmItem.AlarmItems) as AlarmItems, AVG(ALL case when Observations.ParameterId='temp_mean_past1h' then Observations.Value end) as TempMean, AVG(ALL case when Observations.ParameterId= 'humidity_past1h' then Observations.Value end) as Humidty, AVG(ALL case when Observations.ParameterId= 'pressure_at_sea' then Observations.Value end) as Pressure, min(ALL case when Observations.ParameterId= 'temp_min_past1h' then Observations.Value end) as TempMin, max(ALL case when Observations.ParameterId= 'temp_max_past1h' then Observations.Value end) as TempMax FROM Observations JOIN (SELECT Alarms.DateKey, SUM(ALL Alarms.AlarmCount) as Alarms From Alarms Where Alarms.StationId = {stations[i]} AND Alarms.IsValid = 1 Group by Alarms.DateKey) as Alarm ON Alarm.DateKey = Observations.DateKey JOIN (SELECT DISTINCT Subscriptions.StationId, COUNT(Subscriptions.ID) as Stores FROM Subscriptions Group by Subscriptions.StationId) as Stores ON Stores.StationId = Observations.StationId JOIN(SELECT SUM(Subscriptions.AlarmItems) as AlarmItems, MIN(Subscriptions.StartDate) as StartDate, Subscriptions.StationId FROM Subscriptions Where Subscriptions.StartDate Between StartDate AND '{dates[counter]}' Group by Subscriptions.StationId) as AlarmItem ON AlarmItem.StationId = Observations.StationId Where Observations.StationId = {stations[i]} AND Observations.IsValid = 1 Group by Alarm.DateKey order by Alarm.Datekey", conn);
                            using SqlDataReader reader = cmd2.ExecuteReader();

                            using (StreamWriter writer = new StreamWriter(@"C:\Users\Asmus\source\repos\temperaturepredictor\AlarmDataTest6.csv", true))
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

        public void SaveAlarmDataset()
        {
            List<int> stations = KnownGoodStations;
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

        public TemperatureData GetNewestWeatherforecastByStationId(int StationId)
        {
            TemperatureData temperatureData = new TemperatureData { };
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                SqlCommand sqlCommand = new SqlCommand($"SELECT Stores, TempMean, Humidity, Pressure, TempMin, TempMax FROM ForecastsFromArima WHERE Date = (SELECT MAX(DATE) FROM ForecastsFromArima) AND StationId = {StationId}", conn);
                {
                    conn.Open();
                    using SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                    while (sqlDataReader.Read())
                    {
                        temperatureData = new TemperatureData
                        {
                            Stores = (float)sqlDataReader["Stores"],
                            TempMean = (float)sqlDataReader["TempMean"],
                            Humidity = (float)sqlDataReader["Humidity"],
                            Pressure = (float)sqlDataReader["Pressure"],
                            TempMin = (float)sqlDataReader["TempMin"],
                            TempMax = (float)sqlDataReader["TempMax"]
                        };
                    }
                }
            }
            return temperatureData;
        }
    }
}
