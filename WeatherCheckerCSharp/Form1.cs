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
        // クリック系は戻り値voidでOK。それ以外はTaskらしい。（なぜ？）
        private async void btnSearch_Click(object sender, EventArgs e)
        {

            // 入力されたテキストから前後から空白文字をすべて削除
            string city = txtCity.Text.Trim();
            // 都市名 → 緯度経度（日本語で検索、上位1件）
            string geoUrl = $"https://geocoding-api.open-meteo.com/v1/search" +
        $"?name={Uri.EscapeDataString(city)}&count=1&language=ja&format=json";
            lblStatus.Text = geoUrl;

            // Webに対して，URLのでデータを文字列でほしいです。
            // 時間がかかったらあとからやってねと言っている（？）
            // 👉ここで結果を待つ。でも待っている間、画面全体は止めない。
            // これってどうして画面が止まらないのですか？
            string geoJson = await http.GetStringAsync(geoUrl);

            txtRaw.Text = geoJson;
        }

        private void txtRaw_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
