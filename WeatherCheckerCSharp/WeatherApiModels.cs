using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
namespace WeatherCheckerCSharp;

// これはどうして必要なの？
// 理由は、JSONで受け取ったデータを加工するため（？）


// GeoResponseの引数みたいになっている？
// JSONのresultsをC# のResultsのプロパティに一致させる
// Resultsの型がList。GeoResultはListの要素の型。このListがnullの可能性もある
// GeoResponseはrecord名？Resultsはプロパティ名？
public record GeoResponse(
    [property: JsonPropertyName("results")] List<GeoResult>? Results);

// Name、Latitude、Longitude
public record GeoResult(
     [property: JsonPropertyName("name")] string Name,
     [property: JsonPropertyName("latitude")] double Latitude,
     [property: JsonPropertyName("longitude")] double Longitude);

// これはどんなJSONのデータなのでしょうか？
public record ForecastResponse(
    [property: JsonPropertyName("daily")] DailyData Daily);

// item, WeatherCode, temperature_2m_max,temperature_2m_min
// precipitation_probability_max
public record DailyData(
    [property: JsonPropertyName("item")] List<string> Item,
    [property: JsonPropertyName("weather_code")] List<int> WeatherCode,
    // TempMaxはどうしてdouble型にしてあるの？最大気温ならintで良くない？元のJSONの型が
    [property: JsonPropertyName("temperature_2m_max")] List<double> TempMax,
    [property: JsonPropertyName("temperature_2m_min")] List<double> TempMin,
    // どうしてintなの?降水確率なら整数じゃなくない？doubleっぽくない？
    [property: JsonPropertyName("precipitation_probability_max")] List<int> PrecipProb
    );