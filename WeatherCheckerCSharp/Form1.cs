using System.Diagnostics;
using System.Reflection.Emit;
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
            // 参照先を表示
            // リンクラベルの表示を書き換える
            // TODO: イベントについて学んだあとにもう一度このコードを見直す
            linkLabel1.Text = "Weather data by Open-Meteo.com";
            // リンクがクリックされたら、、
            // System.Diagnostics.Process.Start("起動したいアプリ")
            // 引数はsenderとeventカナ？
            linkLabel1.LinkClicked += (s, e) => System.Diagnostics.Process.Start(
               // ここで外部アプリを開く処理を設定している
               // シェルを使用する必要がある場合はtrueにするらしい
               // UseShellExecuteがtrueだと、シェルを使って処理を実行したいっていう設定？
               new System.Diagnostics.ProcessStartInfo("https://open-meteo.com/") { UseShellExecute = true });
            linkLabel2.Text = "🌻いちかどんのGitHubのページ🌻";
            //linkLabel2.LinkClicked += (s, e) => System.Diagnostics.Process.Start(
            //    new System.Diagnostics.ProcessStartInfo("https://github.com/IchikaCoding?tab=repositories") { UseShellExecute = true }
            //    );
            linkLabel2.LinkClicked += (s, e) => System.Diagnostics.Process.Start("notepad");
            Debug.WriteLine(new System.Diagnostics.ProcessStartInfo("https://github.com/IchikaCoding?tab=repositories"));
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
            // 非同期処理＋recordでちょっと壁高めかも
            // cityNameはstring?のほうがいいのかな？
            async Task<GeoInfo> GeoCodeAsync(string cityName)
            {
                // TODO: 2026-07-10ここ
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
                txtRaw.Text = geoJson;

                // optionでJSONとプロパティ名をクラスに合わせて修正してくれる
                var option = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                // デシリアライズするとrecordの型にはめる事が可能（？）
                // TODO: GeoResponseはnullが入る可能性ある？👉️例外に変えたからnullにならない
                // TODO: どうしてInvalidOperationExceptionの例外を使用したの？
                GeoResponse geoString = JsonSerializer.Deserialize<GeoResponse>(geoJson, option) ?? throw new InvalidOperationException("位置情報APIのレスポンスを読み取れませんでした。");

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

            // ＝＝＝＝＝実行させる場所＝＝＝＝＝
            if (string.IsNullOrEmpty(cityName))
            {
                MessageBox.Show("都市名を入力してください🙇‍");
                return;
            }
            btnSearch.Enabled = false;
            lblStatus.Text = "取得中・・・";
            try
            {
                // GeoCodeAsyncで取得した戻り値をawait して実行
                // Nullじゃなかった場合、例外を返す
                GeoInfo? geoInfo = await GeoCodeAsync(cityName);
                if (geoInfo is null)
                {
                    return;
                }
                List<DayForecast> dayForecasts = await DayForecastAsync(geoInfo);
                Debug.WriteLine($"dayForecasts: {dayForecasts}");
                ShowDayForecast(dayForecasts);
            }
            catch (CityNotFoundException error)
            {
                // GeoCodeAsyncから切り離してこっちで表示されるように修正
                lblStatus.Text = $"「{cityName}」の検索結果がnullでした";
                MessageBox.Show(error.Message);
            }catch(HttpRequestException error)
            {
                MessageBox.Show($"通信エラーです！！！{error.Message}");
            }
            finally
            {
                btnSearch.Enabled = true;
                
            }
            

            async Task<List<DayForecast>> DayForecastAsync(GeoInfo geoInfoPram)
            {
                // 3日分取得
                // URLのパラメーター部分は最初?でその後は＆で続ける
                // TODO: ＆とカンマの違いと、カンマの位置と足し算にする場所が不明
                // 👉️カンマは一つの項目の値を並べるやつ。＆は項目自体をくっつけるやつ？これどこで定義されているの？
                string forecastUrl = $"https://api.open-meteo.com/v1/forecast" + $"?latitude={geoInfoPram.Latitude}&longitude={geoInfoPram.Longitude}" + "&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max" + "&timezone=Asia%2FTokyo&forecast_days=3";
                string forecastJson = await http.GetStringAsync(forecastUrl);
                // ForecastResponse型のデータにする
                ForecastResponse? forecastResponse = JsonSerializer.Deserialize<ForecastResponse>(forecastJson);
                // nullの可能性があるっぽい？
                DailyData dailyData = forecastResponse.Daily;
                //Debug.WriteLine($"dailyData: {dailyData}");
                List<double> tempMaxList = dailyData.TempMax;
                List<double> tempMinList = dailyData.TempMin;
                // 最高気温。1日目：15℃, 2日目：10℃
                lblStatus.Text = $"{cityName}：今日の最高 {tempMaxList[0]}℃ / 最低 {tempMinList[0]}℃";

                // ======================================================
                // 3日分のデータを1日分ごとにまとめてリストにする
                var days = new List<DayForecast>();
                for (int i = 0; i < dailyData.Time.Count; i++)
                {
                    days.Add(new DayForecast(dailyData.Time[i], dailyData.WeatherCode[i], dailyData.TempMax[i], dailyData.TempMin[i], dailyData.PrecipProb[i]));
                }
                Debug.WriteLine($"days:{days}");
                return days;
            }


            void ShowDayForecast(List<DayForecast> dayForecasts)
            {
                // AppendLineが使えるようになるっぽい
                var sb = new System.Text.StringBuilder();

                foreach (var day in dayForecasts)
                {
                    (string emoji, string label) = Describe(day.Code);
                    sb.AppendLine($"{day.Time} {emoji} {label}");
                    sb.AppendLine($"最高気温：{day.Max} 最低気温：{day.Min} 降水確率：{day.Prob}");
                }
                // sbはToString()で文字列として表示できるらしい
                lblStatus.Text = sb.ToString();
                //this.BackColor = days[0].Code == 0 ? Color.FromArgb(255, 247, 224): Color.FromArgb(232, 238, 245);
                // TODO:　thisってだれのこと？　ArgbのAって何が由来なの？　この色探しをするツールを探す
                // ここはそもそもFrom1のクラス内。つまり、thisはForm1のインスタンスのこと
                this.BackColor = Color.FromArgb(255, 247, 224);

            }
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

        private void label1_Click(object sender, EventArgs e)
        {

        }
        // 例外処理を一旦書いてみようね
        private void button1_Click(object sender, EventArgs e)
        {
            static int CharToInt(char c)
            {
                if('0' <= c && c <= '9')
                {
                    // しんぐるじゃないとだめっぽいのなぜ？
                    return c - '0';
                }
                else
                {
                    Debug.WriteLine("入力値が0以上9以下を満たしません");
                    return -1;
                }
                
            }
            static int StringToInt(string str)
            {
                int val = 0;
                foreach (char c in str)
                {
                    int i = CharToInt(c);
                    if(i == -1)
                    {
                        return -1;
                    }
                    val = val * 10 + i;
                }
                return val;
            }
            // 文字列もASCIIコードで数値として表すことも一応可能
            int result = StringToInt("ichika");
            int result2 = StringToInt("1234");
            //char numString = (char)48;
            //Debug.WriteLine($"numString:{numString}");

            Debug.WriteLine($"result: {result}"); // result: 6272339
            Debug.WriteLine($"result2: {result2}");
        }

        //        private void AddList()
        //        {
        //           public record DayForecast(string Date, int Code, double Max, double Min, int Pop);

        //            List<DayForecast> days = new List<DayForecast>();
        //                for (int i = 0; i<d.Time.Coount; i++){
        //                days.Add()
        //    }

        //};

    }
}
