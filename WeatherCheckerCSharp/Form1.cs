using System.Diagnostics;
using System.Reflection.Emit;
using System.Security.Policy;
using System.Text.Json;
//using System.IO;
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

        // 非同期処理だけどTask型はLoadに登録できない。だからVoidにした
        // ファイルが壊れているかもしれない。読み込む権限がないかもしれない。読み込み失敗しているかもしれない
        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                List<string> favs = await LoadFavoritesAsync();
                // 配列に直したお気に入りの都市たちを
                // AddRange()はListの末尾に要素を追加できるやつ。
                // コンボボックスの中に配列にして一気にお気に入りを追加
                cmbFavorites.Items.AddRange(favs.ToArray());
            }
            catch (JsonException error)
            {
                MessageBox.Show($"お気に入りファイルが壊れているため、読み込めません\n{error.Message}");
            }
            catch (UnauthorizedAccessException error)
            {
                MessageBox.Show($"お気に入りファイルを読み込む権限がありません\n{error.Message}");
            }
            catch (IOException error)
            {
                MessageBox.Show($"お気に入りファイルの読み込み失敗しています\n{error.Message}");
            }

        }
        // TODO: こんなのあったっけ？
        // ここの処理はテキストボックスが更新されるたびに実行される処理
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
            }
            catch (HttpRequestException error)
            {
                MessageBox.Show($"通信エラーです！！！{error.Message}");
            }
            catch (JsonException error)
            {
                MessageBox.Show($"天気データの形式が想定と違います。{error.Message}");
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

                int count = timeList.Count;
                if (count == 0)
                {
                    throw new JsonException("天気予報APIのレスポンスの1日ごとのデータが取得出来ませんでした");
                }
                if (weatherCodeList.Count != count || tempMaxList.Count != count || tempMinList.Count != count || precipProbList.Count != count)
                {
                    throw new JsonException("天気予報APIのレスポンスのデータがうまく取得出来ませんでした");
                }
                // 最高気温。1日目：15℃, 2日目：10℃
                lblStatus.Text = $"{cityName}：今日の最高 {tempMaxList[0]}℃ / 最低 {tempMinList[0]}℃";


                // ======================================================
                // 3日分のデータを1日分ごとにまとめてリストにする
                var days = new List<DayForecast>();
                // TODO: もしかしたらfor文全体をtry-catchで囲んでNullReferenceExceptionをしたほうがいいかも？
                for (int i = 0; i < timeList.Count; i++)
                {
                    days.Add(new DayForecast(timeList[i], weatherCodeList[i], tempMaxList[i], tempMaxList[i], precipProbList[i]));
                }
                Debug.WriteLine($"days:{days}");
                return days;
            }


            void ShowDayForecast(List<DayForecast> dayForecasts)
            {
                // AppendLineが使えるようになるっぽい
                var sb = new System.Text.StringBuilder();

                // ここでfor分を書く
                // ラベルに一つずつ入れる処理を書く
                // dayForecastsのインデックスに合わせて、ラベルに入れる
                // 3つのラベルが入っている配列を作成する
                System.Windows.Forms.Label[] forecastLabels = { lblForecast1, lblForecast2, lblForecast3 };

                for (int i = 0; i < dayForecasts.Count; i++)
                {
                    // dayForecastsの要素を一つずつ取ってくる
                    DayForecast day = dayForecasts[i];
                    (string emoji, string label) = Describe(day.Code);
                    string stringDayForecast = $"{day.Time}\n{emoji} {label}\n";
                }

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

        // お気に入り登録処理
        // TODO: おそらく、各自の環境のApplicationDataフォルダがあるパスを取得
        //  "MyWeather", "favorites.json"とかとくっつけてFavPathに代入する
        // どうしてstaticなの？変数ってstaticにする意味はありますか？
        // 👉️staticの理由：Form1のインスタンスが複数作られても、同じFavPathを使用するよという意味。
        // 更新出来ないようにreadonlyを使っている。
        private static readonly string FavPath = Path.Combine(@"D:\Dev", "MyWeather", "favorites.json");

        // TODO: List<string>はなに？👉️お気に入りの都市がstringで、それのList
        private async Task SaveFavoritesAsync(List<string> favs)
        {
            // favorites.jsonというディレクトリを作成する（登録処理）
            // TODO: FavPathがnull参照引数になっているらしい。でもFavPathは文字列では？
            // 👉️Yes。Path.GetDirectoryName()の戻り値がstring?。nullの可能性もある
            string? directoryPath = Path.GetDirectoryName(FavPath);
            if (directoryPath is null)
            {
                throw new InvalidOperationException("お気に入りファイルの保存先が正しくありません");
            }
            Directory.CreateDirectory(directoryPath);
            // シリアライズをしてクラスからJSONに戻す
            // { WriteIndented = true}ってオブジェクト初期化子？👉️Yes!!!
            // WriteIndentedをtrueにすると、JSONを作成する時に、見やすいJSONになるらしい。（例：プロパティ名と値の間に空白を追加する。）
            JsonSerializerOptions option = new JsonSerializerOptions { WriteIndented = true };
            // クラスからJSONデータへ変換する、
            string json = JsonSerializer.Serialize(favs, option);
            // パスを指定して非同期でファイルを読む
            // Fileは2種類選べるようになっていて曖昧。これは指定してあげたら治るかも
            await System.IO.File.WriteAllTextAsync(FavPath, json);
        }
        // LoadFavoritesAsync()を作成する（読み込み処理）

        private async Task<List<string>> LoadFavoritesAsync()
        {
            // ファイルがないなら、空のリストを返す
            // ファイルの中身全て読んでJSON文字列にする
            // JSONからListにして、もしnullなら新しいListを作成？
            if (!System.IO.File.Exists(FavPath))
            {
                return new List<string>();
            }
            string json = await System.IO.File.ReadAllTextAsync(FavPath);
            // new()ってなんだろう？new List<string>()で空のリスト作れない？
            // JsonSerializer.Deserializeは戻り値がTValue?👉非同期じゃない。null許容型だからnull合体演算子をつけておくのがいいっぽい
            List<string> favList = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            // TODO: `.Distinct(StringComparer.OrdinalIgnoreCase)`がわからない
            // JSON手動で変更された時のために、ここにも要素チェックを入れておく
            // TODO: 共通のメソッドにしておくと便利かも。
            return favList
                .Where(favItem => !string.IsNullOrWhiteSpace(favItem))
                .Select(favItem => favItem.Trim())
                // Distinct()とは？
                // もとの LIST を書き換えないで、重複を取り除いてくれるらしい
                // 英字の大文字と小文字を区別しない比較ルール
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private async void btnFav_Click(object sender, EventArgs e)
        {
            // 都市名を綺麗に取得
            // もうすでに入っている、nullだったら早期リターン
            // コンボボックスの選択肢に追加
            // 都市名を取得してそれをListにして保存処理を実行
            string rawCityName = txtCity.Text;
            string trimmedCityName = rawCityName.Trim();
            // cmbFavorites.Items.Contains()　は戻り値bool
            // TODO: どうしてここで例外じゃなくて早期リターンなの？
            // 👉️その場で解決したほうが例外投げるより早く解決できる
            if (string.IsNullOrWhiteSpace(trimmedCityName))
            {
                MessageBox.Show("お気に入りに登録する都市名を入力してください");
                // TODO: ここって早期リターンでいいの？例外throwしなくていいの？
                // 早期リターンにしたほうが処理が早い、例外を出すとキャッチして、という手間がかかる。
                return;
            }
            // ここでボタンを押せなくするらしい
            btnFav.Enabled = false;
            try
            {
                // 非同期処理の例外をキャッチしてみよう
                // ここでJSONの内容を取得して、その中身から都市名の存在確認をしている。画面の都市名だけで判断していないのでGood！
                List<string> favariteList = await LoadFavoritesAsync();
                // どうして毎回受け取る変数を作り忘れるのだろうか？
                // 大文字・小文字区別しないで比較したいときは`StringComparison.OrdinalIgnoreCase`でルールを追加する
                bool alreadyExists = favariteList.Any(favarite => string.Equals(favarite, trimmedCityName, StringComparison.OrdinalIgnoreCase));
                if (alreadyExists)
                {
                    MessageBox.Show($"{trimmedCityName}はすでにお気に入りに登録されています");
                    return;
                }
                // Listに追加したい
                favariteList.Add(trimmedCityName);
                // 最新のお気に入りListを登録する
                await SaveFavoritesAsync(favariteList);
                // どうしてClear()？👉️クリアしてまた新しいバージョンを登録する。JSONがいつでもデータの参照先
                cmbFavorites.Items.Clear();
                cmbFavorites.Items.AddRange(favariteList.ToArray());
                MessageBox.Show($"{trimmedCityName}をお気に入りに登録しました");
            }
            catch (JsonException)
            {
                MessageBox.Show("お気に入りファイルが壊れているため、読み込めませんでした");
            }
            // UnauthorizedAccessExceptionとIOException errorの例外もキャッチする
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("お気に入りファイルを保存する権限がありません。");
            }
            catch (IOException error)
            {
                MessageBox.Show($"ファイルの読み書きに失敗しました。\n{error.Message}");
            }
            finally
            {
                btnFav.Enabled = true;
            }


            //if (string.IsNullOrWhiteSpace(trimmedCityName) || cmbFavorites.Items.Contains(trimmedCityName))
            //{
            //    return;
            //}
            //// TODO: 画面にしか登録していない。JSONに先に追加して、そこからコンボボックスを更新するといいかも
            //// TODO: 非同期処理を書くならtry-catchを書くといいかも！
            //// 引数がList<string>。trimmedCityNameをListに追加してから渡したい
            //List<string> favList = await LoadFavoritesAsync();
            //favList.Add(trimmedCityName);
            //await SaveFavoritesAsync(favList);
            //cmbFavorites.Items.Add(trimmedCityName);
        }
        // TODO: ここからやる
        private void cmbFavorites_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 選択した値をtxtCity.Textに反映したい
            object? selectedCity = cmbFavorites.SelectedItem;
            if (selectedCity is null)
            {
                return;
            }
            // objectをstringに明示的に変換
            txtCity.Text = (string)selectedCity;
        }

        private async void RemoveFavBtn_Click(object sender, EventArgs e)
        {
            // 選択した値をtxtCity.Textに反映したい
            object? selectedCity = cmbFavorites.SelectedItem;
            if (selectedCity is null)
            {
                return;
            }
            try
            {
                List<string> favariteList = await LoadFavoritesAsync();
                bool isSuccessed = favariteList.Remove((string)selectedCity);
                if (isSuccessed)
                {

                    await SaveFavoritesAsync(favariteList);
                }
                cmbFavorites.Items.Clear();
                cmbFavorites.Items.AddRange(favariteList.ToArray());
                MessageBox.Show($"お気に入りから{selectedCity}を削除しました");
                // 都市入力欄だけリセットできそう👉️出来なかった
                txtCity.Text = "";

            }
            catch (JsonException)
            {
                MessageBox.Show("お気に入りファイルが壊れているため、読み込めませんでした");
            }
            // UnauthorizedAccessExceptionとIOException errorの例外もキャッチする
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("お気に入りファイルを保存する権限がありません。");
            }
            catch (IOException error)
            {
                MessageBox.Show($"ファイルの読み書きに失敗しました。\n{error.Message}");
            }

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
