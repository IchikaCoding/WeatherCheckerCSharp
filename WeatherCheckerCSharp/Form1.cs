using System.Diagnostics;
using System.Security.Policy;
using System.Text.Json;
using System.Threading.Channels;
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
            //　Uri.EscapeDataString(cityName)はどうして使うの？Uriクラスってなに？
            // クエリパラメーターの前はスラッシュいらないらしい
            // パラメーターの中にどうしてUri.EscapeDataStringがあるの？
            // ▷文字列の中に予約語があった場合も安全に送るため
            string geoUrl = $"https://geocoding-api.open-meteo.com/v1/search" + $"?name={Uri.EscapeDataString(cityName)}&count=1&language=ja&format=json";
            // EscapeDataStringでURLを安全な文字列に修正してくれうらしんだけど、間違っているかも、、、、
            // TODO: EscapeDataString()はここにいるのか？いらないのかい？
            string geoJson = await http.GetStringAsync(geoUrl);
            txtRaw.Text = geoJson;

            // デシリアライズするとrecordの型にはめる事が可能（？）
            GeoResponse? geoString = JsonSerializer.Deserialize<GeoResponse>(geoJson);
            // 出力：geoString:GeoResponse { Results =  }
            Debug.WriteLine($"geoString:{geoString}");

            // 緯度経度を取得する
            // TODO: これ間違っているらしい。
            // GeoResult? hit = geo?.Results?.FirstOrDefault();らしいよ
            // GeoResponseに入れたデータはレコードの型になっている。
            // そのResultsにアクセスすると、List<GeoResult>が取れる。
            List<GeoResult>? geoResults = geoString?.Results;
            
            // 出力：geoResults: 
            Debug.WriteLine($"geoResults: {geoResults}");

            // FirstOrDefault()を使わないでやってみたいときはこれでいいですか？
            // ?でnull許容しすぎている気がするけどいいのかな？
            // Listの中身が0の可能性があるから、件数も条件に入れる
            if (geoResults is null || geoResults.Count == 0)
            {
                lblStatus.Text = $"「{cityName}」の検索結果がnullでした";
                return;
            }
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
            // 👉️カンマは一つの項目の値を並べるやつ。＆は項目自体をくっつけるやつ？これどこで定義されているの？
            string forecastUrl = $"https://api.open-meteo.com/v1/forecast" + $"?latitude={latitude}&longitude={longitude}" + "&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max" + "&timezone=Asia%2FTokyo&forecast_days=3";
            string forecastJson = await http.GetStringAsync(forecastUrl);
            // ForecastResponse型のデータにする
            ForecastResponse? forecastResponse = JsonSerializer.Deserialize<ForecastResponse>(forecastJson);
            // nullの可能性があるっぽい？
            DailyData dailyData = forecastResponse.Daily;
            List<double> tempMaxList =  dailyData.TempMax;
             List<double> tempMinList =  dailyData.TempMin;


            // 最高気温。1日目：15℃, 2日目：10℃
            lblStatus.Text = $"{cityName}：今日の最高 {tempMaxList[0]}℃ / 最低 {tempMinList[0]}℃";


            }

        //private string? EscapeDataString(string url)
        //{
        //    throw new NotImplementedException();
        //}

        // これどこで使うメソッド？JSONだからクラスに直すのでは？
        private void txtRaw_TextChanged(object sender, EventArgs e)
        {
            Console.WriteLine();
        }

        // EmojiとLabelは戻り値
        //  Describeメソッドでcodeが引数。これはAPIから帰ってくるコードを渡す場所
        static (string Emoji, string Label) Describe(int code) => code switch
        {
            // codeが0なら快晴
            0 => ("☀️", "快晴"),
            // codeが1か2か3なら晴れ/くもりを返す
            1 or 2 or 3 => ("⛅", "晴れ／くもり"),
            45 or 48 => ("🌫️", "霧"),
            51 or 53 or 55 => ("🌦️", "霧雨"),
            61 or 63 or 65 => ("🌧️", "雨"),
            71 or 73 or 75 => ("❄️", "雪"),
            80 or 81 or 82 => ("🌧️", "にわか雨"),
            95 or 96 or 99 => ("⛈️", "雷雨"),
            // 0～99以外のコードが入っていたら不明と出力する
            _ => ("❔", "不明"),
        };

        
    }   
   }
