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
        }

        // これどこで使うメソッド？JSONだからクラスに直すのでは？
        private void txtRaw_TextChanged(object sender, EventArgs e)
        {
            Console.WriteLine();
        }
    }
}
