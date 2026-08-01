using System.Text.Json;
// CultureInfoはこれで使える
using System.Globalization;
// Debugを使うにはこれを入れる
using System.Diagnostics;
// using System.Net.Http; 

namespace WeatherCheckerCSharp;

public class WeatherApiClient{

            // HTTPクライアントを使用するとGetStringAsyncが使えて，渡されたデータ（ＪＳＯＮ）を文字列として受け取ることができる
            // Webにアクセスするためのインスタンス
            private static readonly HttpClient http = new HttpClient();

            // この処理は内部でしか使わない。だからprivateにしておこう
            private async Task<List<GeoResult>?> GetGeoResultsListAsync(string cityName)
             {
                // GeoResponseを受ける
                // URLで都市名で検索する
                // URLの作り方がわからない。非同期処理がGood
                // 変数名が変数名っぽく光っているかどうかを確認しよう！
                // TODO: どうしてURLを一括で書かないの？
                //　Uri.EscapeDataString(cityName)はどうして使うの？Uriクラスってなに？
                // クエリパラメーターの前はスラッシュいらないらしい
                // パラメーターの中にどうしてUri.EscapeDataStringがあるの？
                // ▷文字列の中に予約語があった場合も安全に送るため
                string geoUrl = $"https://geocoding-api.open-meteo.com/v1/search" + $"?name={Uri.EscapeDataString(cityName)}&count=1&language=ja&format=json";
                // EscapeDataStringでURLを安全な文字列に修正してくれうらしんだけど、間違っているかも、、、、
                // TODO: EscapeDataString()はここにいるのか？いらないのかい？
                // TODO: ここでnullが返ってくる可能性はないの？GeoResponse? geoString はnullの可能性がないと言うコードにした
                string geoJson = await http.GetStringAsync(geoUrl);

                // optionでJSONとプロパティ名をクラスに合わせて修正してくれる
                var option = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                // デシリアライズするとrecordの型にはめる事が可能（？）
                // TODO: GeoResponseはnullが入る可能性ある？👉️例外に変えたからnullにならない
                // どうしてInvalidOperationExceptionの例外を使用したの？
                // 👉️JSONからクラスに変換する時の例外は、JSONExceptionにした
                GeoResponse geoResponse = JsonSerializer.Deserialize<GeoResponse>(geoJson, option) ?? throw new JsonException("位置情報APIのレスポンスを読み取れませんでした。");
                
                // Debug.WriteLine($"geoString:{geoResponse}");

                // 緯度経度を取得する
                // GeoResult? hit = geo?.Results?.FirstOrDefault();らしいよ
                // GeoResponseに入れたデータはレコードの型になっている。
                // そのResultsにアクセスすると、List<GeoResult>が取れる。

                // TODO: Resultsプロパティってnullになる可能性はある？
                // GeoResponsの型としてgeoString変数がある
                // →Resultsプロパティが存在するってこと→nullになる可能性ないのでは？
                // 👉️GeoResponseが合っても、Resultsに値が入っているとは限らない。nullの可能性がある
                // 外からくるデータはnull許容型で受け止めてあげるほうが安全！
                List<GeoResult>? geoResults = geoResponse.Results;
                return geoResults;
                //// 出力：geoResults: 
                //Debug.WriteLine($"geoResults: {geoResults}");
             }

             private GeoInfo CreateGeoInfoFromGeoResults(List<GeoResult>? geoResults, string cityName)
            {
                // FirstOrDefault()を使わないでやってみたいときはこれでいいですか？
                // Listの中身が0の可能性があるから、件数も条件に入れる
                if (geoResults is null || geoResults.Count == 0)
                {
                    // ここは例外に直す👉️CityNotFoundException
                    throw new CityNotFoundException($"「{cityName}」の検索結果がありませんでした");
                }
                // Listの最初の要素は0だよ
                GeoResult geoFirstItem = geoResults[0];

                // 都市名が見つからなかった場合はここで早期リターン
                // TODO: 取得出来なかった判定はどうやってやるの？
                // GeoResultのNameプロパティがnullだったら、という条件じゃだめ？
                // TODO: この処理によってgeoResultsが0件以上っていうことになっている
                // if (geoFirstItem is null)
                // {

                //     lblStatus.Text = $"「{cityName}」が見つかりません";
                //     throw new CityNotFoundException(cityName);
                // }
                // クラスで型を作成して戻り値にしてみる？
                // 戻り値が複数ある時って、タプルとクラスの2種類ある？
                double latitude = geoFirstItem.Latitude;
                // ロンジェチュードって読むらしい
                double longitude = geoFirstItem.Longitude;
                // recordだから、括弧の中に直接値を渡したらプロパティに入れれるかも？！
                return new GeoInfo(latitude, longitude);
                }

            // 非同期処理＋recordでちょっと壁高めかも
            // cityNameはstring?のほうがいいのかな？
            public async Task<GeoInfo> GeoCodeAsync(string cityName)
            
            {
                if (string.IsNullOrWhiteSpace(cityName))
                {
                    throw new ArgumentException("都市名を入力してください");
                }

                List<GeoResult>? geoResults = await  GetGeoResultsListAsync(cityName);
                GeoInfo geoInfo = CreateGeoInfoFromGeoResults(geoResults, cityName);
                return geoInfo;
            }

            public async Task<List<DayForecast>> DayForecastAsync(GeoInfo geoInfoPram)
            {
                // 文化によって小数点の表し方が異なるらしい。
                // それによってパラメーターを変えないために文字列にして`CultureInfo.InvariantCulture`を使用してみた
                string latitudeText = geoInfoPram.Latitude.ToString(CultureInfo.InvariantCulture);
                string longitudeText = geoInfoPram.Longitude.ToString(CultureInfo.InvariantCulture);
                Console.WriteLine("latitudeText: " + latitudeText);
                // 3日分取得
                // URLのパラメーター部分は最初?でその後は＆で続ける
                // TODO: ＆とカンマの違いと、カンマの位置と足し算にする場所が不明
                // 👉️カンマは一つの項目の値を並べるやつ。＆は項目自体をくっつけるやつ？これどこで定義されているの？
                string forecastUrl = $"https://api.open-meteo.com/v1/forecast" + $"?latitude={latitudeText}&longitude={longitudeText}" + "&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max" + "&timezone=Asia%2FTokyo&forecast_days=3";
                string? forecastJson = await http.GetStringAsync(forecastUrl);

                // ForecastResponse型のデータにする
                // InvalidOperationExceptionクラスの例外を投げる
                // 引数以外の失敗で発生したときの例外らしい
                ForecastResponse forecastResponse = JsonSerializer.Deserialize<ForecastResponse>(forecastJson) ?? throw new JsonException("天気予報APIのレスポンスを読み取れませんでした。");
                // Dailyプロパティがあっても値がnullの可能性がある
                DailyData? dailyData = forecastResponse.Daily;
                if (dailyData is null)
                {
                    // TODO: この例外クラスでいいのだろうか？
                    throw new JsonException("天気予報APIのレスポンスに daily がありませんでした。");
                }
                // ==================以下の部分はまだ例外の処理の実装メモがないよ====================
                //Debug.WriteLine($"dailyData: {dailyData}");
                List<string>? timeList = dailyData.Time;
                List<int>? weatherCodeList = dailyData.WeatherCode;
                List<double>? tempMaxList = dailyData.TempMax;
                List<double>? tempMinList = dailyData.TempMin;
                List<int>? precipProbList = dailyData.PrecipProb;

                if (timeList is null || weatherCodeList is null || tempMaxList is null || tempMinList is null || precipProbList is null)
                {
                    throw new JsonException("天気予報APIのレスポンスに最高気温、もしくは最低気温のデータがありませんでした。");
                }

                // 最大の日付を変数にしておくとこれだけ修正したら変更しやすい
                const int MaxForecastDays = 3;
                int count = timeList.Count;

                if (count == 0)
                {
                    throw new JsonException("天気予報APIのレスポンスの1日ごとのデータが取得出来ませんでした");
                }
                if (weatherCodeList.Count != count || tempMaxList.Count != count || tempMinList.Count != count || precipProbList.Count != count)
                {
                    throw new JsonException("天気予報APIのレスポンスのデータがうまく取得出来ませんでした");
                }
                
                if(count < MaxForecastDays)
                {
                    throw new JsonException($"天気予報は{MaxForecastDays}日分を想定していますが、{count}日分で{MaxForecastDays}日分に足りませんでした");
                }
                


                // ======================================================
                // 3日分のデータを1日分ごとにまとめてリストにする
                var days = new List<DayForecast>();
                // TODO: もしかしたらfor文全体をtry-catchで囲んでNullReferenceExceptionをしたほうがいいかも？
                for (int i = 0; i < MaxForecastDays; i++)
                {
                    days.Add(new DayForecast(timeList[i], weatherCodeList[i], tempMaxList[i], tempMinList[i], precipProbList[i]));
                }
                Debug.WriteLine($"days:{days}");
                return days;
            }
}
