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
        List<int> KnownGoodStations = new List<int>() {06056, 06068, 06116, 06126, 06123, 06032, 06074, 06119, 06149, 06065, 06124, 06058, 06041, 06049, 06147 };
        
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

        public void SaveAlarmDataset()
        {
            List<string> stations = GetStations();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {

                {
                    conn.Open();
                    for(int i=0; i < stations.Count; i++)
                    {
                        SqlCommand cmd = new SqlCommand($"SELECT MAX(Stores.Stores) as Stores, MAX(Alarm.Alarms) as Alarms, AVG(ALL case when Observations.ParameterId='temp_mean_past1h' then Observations.Value end) as TempMean, AVG(ALL case when Observations.ParameterId= 'humidity_past1h' then Observations.Value end) as Humidty, AVG(ALL case when Observations.ParameterId= 'pressure_at_sea' then Observations.Value end) as Pressure, min(ALL case when Observations.ParameterId= 'temp_min_past1h' then Observations.Value end) as TempMin, max(ALL case when Observations.ParameterId= 'temp_max_past1h' then Observations.Value end) as TempMax FROM Observations JOIN (SELECT Alarms.DateKey, SUM(ALL Alarms.AlarmCount) as Alarms From Alarms Where Alarms.StationId = {stations[i]} AND Alarms.IsValid = 1 Group by Alarms.DateKey) as Alarm ON Alarm.DateKey = Observations.DateKey JOIN (SELECT DISTINCT Subscriptions.StationId, COUNT(Subscriptions.ID) as Stores FROM Subscriptions Group by Subscriptions.StationId) as Stores ON Stores.StationId = Observations.StationId Where Observations.StationId = {stations[i]} AND Observations.IsValid = 1 Group by Alarm.DateKey order by Alarm.Datekey", conn);
                        using SqlDataReader reader = cmd.ExecuteReader();

                        using (StreamWriter writer = new StreamWriter(@"C:\Users\Asmus\source\repos\temperaturepredictor\AlarmDataTestAllStations.csv",true))
                        {
                            writer.WriteLine("Alarms,TempMean,Humidity,Pressure,TempMin,TempMax");
                            while (reader.Read())


                                writer.WriteLine("{0},{1},{2},{3},{4},{5},{5}",
                                         reader["Stores"].ToString().Replace(',', '.'), reader["Alarms"].ToString().Replace(',', '.'), reader["TempMean"].ToString().Replace(',', '.'), reader["Humidty"].ToString().Replace(',', '.'), reader["Pressure"].ToString().Replace(',', '.'), reader["TempMin"].ToString().Replace(',', '.'), reader["TempMax"].ToString().Replace(',', '.'));
                        }
                    }

                }
            }
        }
    }
}
