SELECT AVG(ALL case when Observations.ParameterId= 'humidity_past1h' then Observations.Value end) as Humidty, AVG(ALL case when Observations.ParameterId= 'pressure_at_sea' then Observations.Value end) as Pressure, AVG(ALL case when Observations.ParameterId= 'temp_mean_past1h' then Observations.Value end) as TempMean, Max(ALL case when Observations.ParameterId= 'temp_max_past1h' then Observations.Value end) as TempMax, MIN(ALL case when Observations.ParameterId= 'temp_min_past1h' then Observations.Value end) as TempMin from Observations where DateKey = '2020-11-17' and StationId = 06126

SELECT SUM(AlarmItems) as AlarmItems from Subscriptions where StationId = 06126

SELECT Count(ID) as Stores FROM Subscriptions where StationId = 06126

SELECT SUM(AlarmCount) as Alarms FROM Alarms where DateKey = '2020-11-16' and IsValid = 1 and StationId = 06126

SELECT Sum(AlarmCount) as Alarms, Datekey from Alarms where IsValid = 1 group by DateKey order by DateKey DESC

SELECT * FROM Alarms where DateKey = '2020-11-16' AND IsValid = 1 order by AlarmCount

SELECT * From Dataset Where Stores = 84

SELECT * From ForecastsFromArima where StationId = 06123 and ForecastDay = 1

Select * FROM ForecastsFromArima

SELECT * FROM Observations Where Observed = (SELECT Max(Observed) FROM Observations) 

