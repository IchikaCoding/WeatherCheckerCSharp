using System;
using System.Collections.Generic;
using System.Text;
// これは何に使っていますか？↓
using System.Text.Json.Serialization;
namespace WeatherCheckerCSharp;

// これはどうして必要なの？
// 理由は、JSONで受け取ったデータを加工するため（？）




// ここにJSON
// 緯度経度もらってくるやつ
// 一つの要素
// 型名とプロパティ名
public record GeoResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("latitude")] double Latitude,
    [property: JsonPropertyName("longitude")] double Longitude
    );

// 結果一覧（リスト）
public record GeoResponse(
    [property: JsonPropertyName("results")]  List<GeoResult> Results
    );

// 天気予報はここから　
public record DailyData(
    // もしかして、日付じゃなくて文字列？
    [property: JsonPropertyName("time")] List<string> Time,
    [property: JsonPropertyName("weather_code")] List<int> WeatherCode,
    [property: JsonPropertyName("temperature_2m_max")] List<double> TempMax,
    [property: JsonPropertyName("temperature_2m_min")] List<double> TempMin,
    // 変数名長すぎた
    [property: JsonPropertyName("precipitation_probability_max")] List<int> PrecipitationProbabilityMax
    );

// 天気予報一覧はこれ。List
// 天気予報の一覧ってなんだろう、
public record ForecastResponse(
    [property: JsonPropertyName("daily")] DailyData Daily
    );