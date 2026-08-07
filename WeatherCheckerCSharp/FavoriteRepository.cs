// using System;
// using System.Collections.Generic;
// using System.Text;
using System.Text.Json;

namespace WeatherCheckerCSharp
{
    public class FavoriteRepository
    {
        // お気に入り登録処理
        // TODO: おそらく、各自の環境のApplicationDataフォルダがあるパスを取得
        //  "MyWeather", "favorites.json"とかとくっつけてFavPathに代入する
        // どうしてstaticなの？変数ってstaticにする意味はありますか？
        // 👉️staticの理由：Form1のインスタンスが複数作られても、同じFavPathを使用するよという意味。
        // 更新出来ないようにreadonlyを使っている。
        // TODO: ユーザーが保存先を選べる機能を作る
        // ここのパスは外部から受け取って変更させない。
        private readonly string _filePath;
        public FavoriteRepository(string filePath)
        {
            _filePath = filePath;
        }

        public async Task SaveJsonSafelyAsync(List<string> favorites)
        {
            // 一時ファイルのパスを作成
            // オプションつけて、ListをJSONに直す
            // 一時ファイルのパスを指定して、ファイル保存
            // それが成功したら、一時ファイルで正式なファイルを上書き
            string tempPath = _filePath + ".temp";
            // JSONの出力結果に見やすいインデントや改行（整形印刷）を付けるための設定プロパティ
            var json = JsonSerializer.Serialize(favorites, new JsonSerializerOptions { WriteIndented = true });
            // 一時ファイルの保存
            // WriteAllTextAsyncはいったん中身消してから上書きする処理
            await File.WriteAllTextAsync(tempPath, json);
            // tempPathのファイルをpathに移動させる、上書きOK
            File.Move(tempPath, _filePath, overwrite: true);
        }


        public async Task<List<string>> LoadFavoritesAsync()
        {
            // ファイルがないなら、空のリストを返す
            // ファイルの中身全て読んでJSON文字列にする
            // JSONからListにして、もしnullなら新しいListを作成？
            if (!System.IO.File.Exists(_filePath))
            {
                return new List<string>();
            }
            string json = await System.IO.File.ReadAllTextAsync(_filePath);
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
    }
}