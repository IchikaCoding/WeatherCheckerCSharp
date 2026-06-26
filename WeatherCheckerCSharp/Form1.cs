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
            string cityName = txtCity.Text;
            cityName.Trim();
            // GeoResponseを受ける
            // URLで都市名で検索する
            // URLの作り方がわからない。非同期処理がGood
            string url = "https://geocoding-api.open-meteo.com/v1/search/&?name={cityName}&?count=1&language=ja&format=json";
            // EscapeDataStringでURLを安全な文字列に修正してくれうらしんだけど、間違っているかも、、、、
            string geoJson = await http.GetStringAsync(EscapeDataString(url));

            // 都市名が見つからなかった場合はここで早期リターン
            if(geoJson is null)
            {
                lblStatus.Text =$"「{cityName}」が見つかりません";
                return;
            }
            txtRaw.Text = geoJson;

           // デシリアライズするとrecordの型にはめる事が可能（？）
            GeoResponse? geoString = JsonSerializer.Deserialize<GeoResponse>(geoJson);

            // 緯度経度を取得する
            List<GeoResult> geoResults = geoString.Results;
            GeoResult geoFirstItem = geoResults[1];
            double latitude = geoFirstItem.Latitude;
            double longitude = geoFirstItem.Longitude;
            
            // 3日分取得
            string forecastUrl = $"https://api.open-meteo.com/v1/forecast/?latitude={latitude}/?longitude={longitude}/?daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max,timezone=Asia%2FTokyo,forecast_days=3";
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