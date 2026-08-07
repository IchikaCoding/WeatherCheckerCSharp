using System.Diagnostics;
using System.Text.Json;
using System.IO;

// using System.Globalization;
// using System.Reflection.Emit;
// using System.Security.Policy;
// //using System.IO;
// using System.Threading.Channels;
// using WeatherCheckerCSharp;
// using static System.Net.WebRequestMethods;

namespace WeatherCheckerCSharp
{
    public partial class Form1 : Form
    {
        // これフィールド。
        // どうしてここでconst ができないの？定数じゃありません！って言われました
        // 👉️プログラムを実行する前から決まっている値のみが `const` で宣言可能
        // TODO:環境変数の"WEATHER_CHECKER_DATA_ROOT"のパスを取得する
        // JSON保存したいフォルダ名を作成する
        // そのフォルダにJSONファイルを作る
        // private static string? dataRoot = Environment.GetEnvironmentVariable("WEATHER_CHECKER_DATA_ROOT");
        // // TODO: dataRootがnullの可能性がある
        // private static string? path = Path.Combine(dataRoot, "MyWeather");
        // private static DirectoryInfo dir = Directory.CreateDirectory(path);
        // private string jsonPath = Path.Combine(dir);
        // File.Create(dir);

        // private static readonly string _favPath = Path.Combine(@"D:\Dev", "MyWeather", "favorites.json");
        // Repositoryを保持しておくためのフィールドを作成。
        private readonly FavoriteRepository _favoriteRepository;
        private WeatherApiClient weatherApiClient = new WeatherApiClient();
        // readonlyつけわすれ。これは変更しないから
        private readonly string _favPath;
        public Form1()
        {
            InitializeComponent();
            string? rootPath = Environment.GetEnvironmentVariable("WEATHER_CHECKER_DATA_ROOT");
            if (string.IsNullOrWhiteSpace(rootPath))
            {
                // メソッドの呼び出しを許容できない場合の例外
                throw new InvalidOperationException("rootPathが見つかりません");
            }
            string myWeatherPath = Path.Combine(rootPath, "MyWeather");
            // ディレクトリ作成が目的なので戻り値のDirectoryInfoをもらわなくてもOK
            Directory.CreateDirectory(myWeatherPath);
            string favPath = Path.Combine(myWeatherPath, "favorites.json");
            // これはどうしてreadonlyなのに代入できるの？👉️コンストラクター内でならOK
            _favPath = favPath;
            _favoriteRepository = new FavoriteRepository(_favPath);
            // 参照先を表示
            // リンクラベルの表示を書き換える
            // TODO: イベントについて学んだあとにもう一度このコードを見直す
            linkLabel1.Text = "Weather data by Open-Meteo.com";
            // リンクがクリックされたら、、
            // System.Diagnostics.Process.Start("起動したいアプリ")
            // 引数はsenderとeventカナ？
            linkLabel1.LinkClicked += (s, e) => Process.Start(
            // ここで外部アプリを開く処理を設定している
            // シェルを使用する必要がある場合はtrueにするらしい
            // UseShellExecuteがtrueだと、シェルを使って処理を実行したいっていう設定？
            new ProcessStartInfo("https://open-meteo.com/") { UseShellExecute = true });
            linkLabel2.Text = "🌻いちかどんのGitHubのページ🌻";
            //linkLabel2.LinkClicked += (s, e) => System.Diagnostics.Process.Start(
            //    new System.Diagnostics.ProcessStartInfo("https://github.com/IchikaCoding?tab=repositories") { UseShellExecute = true }
            //    );
            // これはアプリ起動する場合のコード↓
            // linkLabel3.LinkClicked += (s, e) =>  Process.Start("notepad");

            linkLabel2.LinkClicked += (s, e) => Process.Start(new ProcessStartInfo("https://github.com/IchikaCoding?tab=repositories") { UseShellExecute = true });
            Debug.WriteLine(new ProcessStartInfo("https://github.com/IchikaCoding?tab=repositories"));
        }

        // 非同期処理だけどTask型はLoadに登録できない。だからVoidにした
        // ファイルが壊れているかもしれない。読み込む権限がないかもしれない。読み込み失敗しているかもしれない
        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                List<string> favs = await _favoriteRepository.LoadFavoritesAsync();
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
            // TODO: 取得中はここでだそうかね？
            // JSON文字列がない、、、
            // この処理結果を代入しておく必要がある
            string cityName = txtCity.Text.Trim();
            // ＝＝＝＝＝実行させる場所＝＝＝＝＝
            if (string.IsNullOrEmpty(cityName))
            {
                MessageBox.Show("都市名を入力してください🙇‍");
                return;
            }
            // ボタン無効
            btnSearch.Enabled = false;
            lblStatus.Text = "取得中・・・";
            try
            {
                // GeoCodeAsyncで取得した戻り値をawait して実行
                // Nullじゃなかった場合、例外を返す
                GeoInfo? geoInfo = await weatherApiClient.GeoCodeAsync(cityName);
                if (geoInfo is null)
                {
                    return;
                }
                List<DayForecast> dayForecasts = await weatherApiClient.DayForecastAsync(geoInfo);
                Debug.WriteLine($"dayForecasts: {dayForecasts}");
                ShowDayForecast(dayForecasts);
                // 成功したら、それをユーザーに通知する・
                // TODO: この処理はShowDayForecastに入れたほうがいいかも。表示をまとめたい
                lblStatus.Text = $"{cityName}の天気予報を取得しました";
            }
            catch (CityNotFoundException error)
            {
                // GeoCodeAsyncから切り離してこっちで表示されるように修正
                lblStatus.Text = $"「{cityName}」の検索結果がnullでした";
                MessageBox.Show(error.Message);
            }
            catch (HttpRequestException error)
            {
                lblStatus.Text = "天気予報が取得出来ませんでした";
                MessageBox.Show($"通信エラーです！！！{error.Message}");
            }
            catch (JsonException error)
            {
                lblStatus.Text = "天気予報が取得出来ませんでした";
                MessageBox.Show($"天気データの形式が想定と違います。{error.Message}");
            }
            finally
            {
                btnSearch.Enabled = true;
            }

            void ShowDayForecast(List<DayForecast> dayForecasts)
            {
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
                    string stringDayForecast = $"{day.Time}\n{emoji} {label}\n 最高気温：{day.Max}℃\n 最低気温：{day.Min}℃\n 降水確率：{day.Prob}%";
                    // ここでラベルに追加
                    forecastLabels[i].Text = stringDayForecast;
                }

                // sbはToString()で文字列として表示できるらしい
                // lblStatus.Text = sb.ToString();
                //this.BackColor = days[0].Code == 0 ? Color.FromArgb(255, 247, 224): Color.FromArgb(232, 238, 245);
                // TODO:　thisってだれのこと？　ArgbのAって何が由来なの？　この色探しをするツールを探す
                // ここはそもそもFrom1のクラス内。つまり、thisはForm1のインスタンスのこと
                this.BackColor = Color.FromArgb(255, 247, 224);
            }
        }

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
                List<string> favariteList = await _favoriteRepository.LoadFavoritesAsync();
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
                await _favoriteRepository.SaveJsonSafelyAsync(favariteList);
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
                MessageBox.Show("削除するお気に入り都市を選択してください。");
                return;
            }
            // ボタン無効化はtryの直前にしないとfinallyが実行されない可能性があるらしい💦
            RemoveFavBtn.Enabled = false;
            try
            {
                // favariteListはJSONデータ
                List<string> favariteList = await _favoriteRepository.LoadFavoritesAsync();
                bool wasRemoved = favariteList.Remove((string)selectedCity);
                // もしJSONにお気に入りがなかったら存在しませんっていって早期リターンする
                if (!wasRemoved)
                {
                    // 保存済みから見つからない。更新しておきますと言っておく
                    MessageBox.Show($"{selectedCity}はJSONデータのお気に入りから見つかりませんでした。" + "\n コンボボックスの一覧を更新しておきます。");
                    // コンボボックスをクリア
                    cmbFavorites.Items.Clear();
                    cmbFavorites.Items.AddRange(favariteList.ToArray());
                    // コンボボックスに新しいバージョンのJSONを表示
                    return;
                }

                await _favoriteRepository.SaveJsonSafelyAsync(favariteList);
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
            finally
            {
                RemoveFavBtn.Enabled = true;
            }
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
