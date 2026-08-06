using System;
using System.Collections.Generic;
using System.Text;
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
        // // TODO: List<string>はなに？👉️お気に入りの都市がstringで、それのList
        // public async Task SaveFavoritesAsync(List<string> favs)
        // {
        //     // favorites.jsonというディレクトリを作成する（登録処理）
        //     // TODO: FavPathがnull参照引数になっているらしい。でもFavPathは文字列では？
        //     // 👉️Yes。Path.GetDirectoryName()の戻り値がstring?。nullの可能性もある
        //     string? directoryPath = Path.GetDirectoryName(_filePath);
        //     if (directoryPath is null)
        //     {
        //         throw new InvalidOperationException("お気に入りファイルの保存先が正しくありません");
        //     }
        //     Directory.CreateDirectory(directoryPath);
        //     // シリアライズをしてクラスからJSONに戻す
        //     // { WriteIndented = true}ってオブジェクト初期化子？👉️Yes!!!
        //     // WriteIndentedをtrueにすると、JSONを作成する時に、見やすいJSONになるらしい。（例：プロパティ名と値の間に空白を追加する。）
        //     JsonSerializerOptions option = new JsonSerializerOptions { WriteIndented = true };
        //     // クラスからJSONデータへ変換する、
        //     string json = JsonSerializer.Serialize(favs, option);
        //     // パスを指定して非同期でファイルを読む
        //     // Fileは2種類選べるようになっていて曖昧。これは指定してあげたら治るかも
        //     // TODO: ここでJSON上書きをしている。FavoriteRepositoryを作る時に一時ファイルに保存したりして修正してみたい
        //     await System.IO.File.WriteAllTextAsync(_filePath, json);
        // }
        // ＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝＝
        // SaveJsonSafelyAsyncメソッドを作成する
        // 引数はpath, ジェネリックのvalieをもらってくる。CancellationTokenにdefaultをいれる
        // pathからディレクトリを作成する
        // ディレクトリがなかったら作る
        // tempPathを作る
        // FileStreamを使ってみる。ファイル読み書きをするために。引数は考える。👉️FileStreamは調べたら引数もわかりそうだよ
        // streamクラスからJSONに変換する。しかもFlushAsyncっていうものをやるらしい
        // パスが存在していたら、元のファイルをbackupに入れる。新しいものとファイルごと入れ替える
        // もしパスがあった場合は、パスにファイルを保存する。
        // 失敗した場合は捨てる
        public async Task SaveJsonSafelyAsync(string path, List<string> favorites)
        {
            // 一時ファイルのパスを作成
            // オプションつけて、ListをJSONに直す
            // 一時ファイルのパスを指定して、ファイル保存
            // それが成功したら、一時ファイルで正式なファイルを上書き
            string tempPath = path + ".temp";
            // JSONの出力結果に見やすいインデントや改行（整形印刷）を付けるための設定プロパティ
            var json = JsonSerializer.Serialize(favorites, new JsonSerializerOptions { WriteIndented = true });
            // 一時ファイルの保存
            await File.WriteAllTextAsync(tempPath, json);
            // tempPathのファイルをpathに移動させる、上書きOK
            File.Move(tempPath, path, overwrite: true);

            // var directory = Path.GetDirectoryName(path);
            // if (!Directory.Exists(directory))
            // {
            //     // どうしてディレクトリを作成するの？
            //     // ここ、DirectoryInfoを受け取り忘れました
            //     // 戻り値は、「return new DirectoryInfo(path, fullPath, isNormalized: true);」
            //     Directory.CreateDirectory(path);
            // }
            // // tempPathファイルを作成する
            // var tempPath = File.Create(path);
            // FileStream fileStream = new FileStream();
            // // 一時ファイルに書き込む
            // // それが例外なく、問題なく実行できたらそのファイルを正式なファイルにする

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

// Chappy が言ってたやつ
//- [] パスを文字列で持っておくフィールド
//- [] これに引数でパスを受け取れるようにしておく
//- [] Load と Save の処理をここに移動させる