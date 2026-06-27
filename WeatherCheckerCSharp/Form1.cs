using System.Security.Policy;
using System.Text.Json;
using WeatherCheckerCSharp;
using static System.Net.WebRequestMethods;

namespace WeatherCheckerCSharp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        // HTTPクライアントを使用するとGetStringAsyncが使えて，渡されたデータ（ＪＳＯＮ）を文字列として受け取ることができる
        // Webにアクセスするためのインスタンス
        private static readonly HttpClient http = new HttpClient();

        // クリック系は戻り値voidでOK。それ以外はTaskらしい。イベントハンドラは非同期処理でもvoid
        private async void btnSearch_Click(object sender, EventArgs e)
        {
            // JSON文字列がない、、、
            // この処理結果を代入しておく必要がある
            string cityName = txtCity.Text.Trim();

            // GeoResponseを受ける
            // URLで都市名で検索する
            // URLの作り方がわからない。非同期処理がGood
            // 変数名が変数名っぽく光っているかどうかを確認しよう！
            // TODO: どうしてURLを一括で書かないの？
                //　Uri.EscapeDataString(cityName)はどうして使うの？
            string geoUrl = $"https://geocoding-api.open-meteo.com/v1/search" + $"?name={EscapeDataString(cityName)}&count=1&language=ja&format=json";
            // EscapeDataStringでURLを安全な文字列に修正してくれうらしんだけど、間違っているかも、、、、
            // TODO: EscapeDataString()はここにいるのか？いらないのかい？
            string geoJson = await http.GetStringAsync(geoUrl);
            txtRaw.Text = geoJson;

            // デシリアライズするとrecordの型にはめる事が可能（？）
            GeoResponse? geoString = JsonSerializer.Deserialize<GeoResponse>(geoJson);

            // 緯度経度を取得する
            // TODO: これ間違っているらしい。
            // GeoResult? hit = geo?.Results?.FirstOrDefault();らしいよ
            // GeoResponseに入れたデータはレコードの型になっている。
            // そのResultsにアクセスすると、List<GeoResult>が取れる。
            List<GeoResult> geoResults = geoString.Results;

            // Listの最初の要素は0だよ
            GeoResult geoFirstItem = geoResults[0];

            // 都市名が見つからなかった場合はここで早期リターン
            // TODO: 取得出来なかった判定はどうやってやるの？
            // GeoResultのNameプロパティがnullだったら、という条件じゃだめ？
            if (geoFirstItem is null)
            {
                lblStatus.Text = $"「{cityName}」が見つかりません";
                return;
            }
            
            double latitude = geoFirstItem.Latitude;
            double longitude = geoFirstItem.Longitude;
            
            // 3日分取得
            // URLのパラメーター部分は最初?でその後は＆で続ける
            // TODO: ＆とカンマの違いと、カンマの位置と足し算にする場所が不明
            string forecastUrl = $"https://api.open-meteo.com/v1/forecast" + $"?latitude={latitude}&longitude={longitude}" + "&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max" + "&timezone=Asia%2FTokyo&forecast_days=3";
            string forecastJson = await http.GetStringAsync(forecastUrl);
            // ForecastResponse型のデータにする
            ForecastResponse? forecastResponse = JsonSerializer.Deserialize<ForecastResponse>(forecastJson);
            DailyData dailyData = forecastResponse.Daily;
            List<double> tempMaxList =  dailyData.TempMax;
             List<double> tempMinList =  dailyData.TempMin;


            // 最高気温。1日目：15℃, 2日目：10℃
            lblStatus.Text = $"{cityName}：今日の最高 {tempMaxList[0]}℃ / 最低 {tempMinList[0]}℃";

            }

        private string? EscapeDataString(string url)
        {
            throw new NotImplementedException();
        }

        // これどこで使うメソッド？JSONだからクラスに直すのでは？
        private void txtRaw_TextChanged(object sender, EventArgs e)
        {
            Console.WriteLine();
        }



    }   
   }