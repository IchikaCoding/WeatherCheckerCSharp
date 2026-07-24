using System.Text.Json;
// using System.Net.Http; 

namespace WeatherCheckerCSharp;

public class WeatherApiClient{

            // HTTPクライアントを使用するとGetStringAsyncが使えて，渡されたデータ（ＪＳＯＮ）を文字列として受け取ることができる
            // Webにアクセスするためのインスタンス
            private static readonly HttpClient http = new HttpClient();
            // 非同期処理＋recordでちょっと壁高めかも
            // cityNameはstring?のほうがいいのかな？
            public async Task<GeoInfo> GeoCodeAsync(string cityName)
            {
                if (string.IsNullOrWhiteSpace(cityName))
                {
                    throw new ArgumentException("都市名を入力してください");
                }
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
                GeoResponse geoString = JsonSerializer.Deserialize<GeoResponse>(geoJson, option) ?? throw new JsonException("位置情報APIのレスポンスを読み取れませんでした。");

                // Debug.WriteLine($"geoString:{geoString}");

                // 緯度経度を取得する
                // GeoResult? hit = geo?.Results?.FirstOrDefault();らしいよ
                // GeoResponseに入れたデータはレコードの型になっている。
                // そのResultsにアクセスすると、List<GeoResult>が取れる。

                // TODO: Resultsプロパティってnullになる可能性はある？
                // GeoResponsの型としてgeoString変数がある
                // →Resultsプロパティが存在するってこと→nullになる可能性ないのでは？
                // 👉️GeoResponseが合っても、Resultsに値が入っているとは限らない。nullの可能性がある
                // 外からくるデータはnull許容型で受け止めてあげるほうが安全！
                List<GeoResult>? geoResults = geoString.Results;

                //// 出力：geoResults: 
                //Debug.WriteLine($"geoResults: {geoResults}");

                // FirstOrDefault()を使わないでやってみたいときはこれでいいですか？
                // Listの中身が0の可能性があるから、件数も条件に入れる
                if (geoResults is null || geoResults.Count == 0)
                {
                    // ここは例外に直す👉️CityNotFoundException
                    throw new CityNotFoundException($"「{cityName}」の検索結果がnullでした");
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
}
