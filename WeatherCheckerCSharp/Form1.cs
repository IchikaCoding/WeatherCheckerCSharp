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

            // 入力されたテキストから前後から空白文字をすべて削除
            string city = txtCity.Text.Trim();
            // 都市名 → 緯度経度（日本語で検索、上位1件）
            // EscapeDataString()でなにをしているの？
            string geoUrl = $"https://geocoding-api.open-meteo.com/v1/search" +
        $"?name={Uri.EscapeDataString(city)}&count=1&language=ja&format=json";
            // ラベルにURLをいれる
            lblStatus.Text = geoUrl;

            // Webに対して，URLのデータを文字列でほしいです。
            // 時間がかかったら終わるまで待って、結果をgeoJsonに入れる
            // 👉ここで結果を待つ。でも待っている間、画面全体は止めない。
            // これってどうして画面が止まらないのですか？
            // 👉️await がスレッドを解放しているかららしい（？）
            string geoJson = await http.GetStringAsync(geoUrl);

            // WebからもらってきたデータをtxtRawのテキスト欄に入れる
            txtRaw.Text = geoJson;

            // 1段目：座標を取り出す
            // APIから取得したJSON文字列をデシリアライズしてrecordの型に入ったgeoを作る
            var geo = JsonSerializer.Deserialize<GeoResponse>(geoJson);
            // geoがあるなら、Resultsプロパティを見て、それがあるなら、LINQで一致した最初の値だけ取得
            GeoResult? hit = geo?.Results?.FirstOrDefault();
            // hitがnullなら、受け取ったデータには存在しなかった。見つからなかったよを表示する
            if (hit is null) { lblStatus.Text = $"「{city}」が見つかりません"; return; }

            // 2段目：予報を取る（3日分）forecast_days=3で指定
            // JSON文字列をrecord型に入れた。それのPropertyを使用してアクセスするためのリンクを作成
            // Tokyoとか取得したいデータは固定になってしまう気がする。。。
            string fcUrl =
                $"https://api.open-meteo.com/v1/forecast" +
                $"?latitude={hit.Latitude}&longitude={hit.Longitude}" +
                "&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max" +
                "&timezone=Asia%2FTokyo&forecast_days=3";

            // また作成したURLでJSONを取得
            string fcJson = await http.GetStringAsync(fcUrl);
            // JSON文字列をrecordのForecastResponse型に変換。
            // recordってインスタンス的なもの？変換したらそこにデータが代入されるって感じ
            // ForecastResponse型がforecastだから、これのプロパティにDailyあり
            var forecast = JsonSerializer.Deserialize<ForecastResponse>(fcJson);

            // !はなに？
            // DailyからTempMaxとかにアクセスする（？）ナニコレ？
            DailyData d = forecast!.Daily;
            lblStatus.Text = $"{hit.Name}：今日の最高 {d.TempMax[0]}℃ / 最低 {d.TempMin[0]}℃";
        }

        // これどこで使うメソッド？JSONだからクラスに直すのでは？
        private void txtRaw_TextChanged(object sender, EventArgs e)
        {
            Console.WriteLine();
        }

       
        
       }   
   }