using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Save2IDB.Async
{
    internal class Save2IDB_Async_Settings : ScriptableObject
    {
        [DllImport("__Internal")] private static extern string Save2IDB_Async_GetDataPath();
        [DllImport("__Internal")] private static extern void Save2IDB_Async_Initialize(string indexedDBName);

        /// <summary>
        /// jslib のコードストリップ対策で宣言のみ必要
        /// </summary>
        [DllImport("__Internal")]private static extern void Save2IDB_Async_requireFiler();

        [SerializeField] internal string _indexedDBName = "Save2IDB/{md5_hash_of_data_path}";

        private void OnEnable()
        {
            hideFlags = HideFlags.DontSaveInEditor; // このオブジェクトを編集したときシーンが Dirty にならないようにする
#if UNITY_WEBGL && !UNITY_EDITOR
            var hash = GetHash();
            var indexedDBName = GetName(_indexedDBName, hash);
            Save2IDB_Async_Initialize(indexedDBName);
#endif
        }

        private static string GetHash()
        {
            var dataPath = Save2IDB_Async_GetDataPath(); // Application.dataPath および absoluteURL は初期化に時間がかかるので .jslib 呼び出しを使用
            var pathBytes = Encoding.ASCII.GetBytes(dataPath); // ハッシュ生成のためbyte配列に変換

            using var md5 = new MD5CryptoServiceProvider();
            var hashBytes = md5.ComputeHash(pathBytes); // ハッシュを格納したbyte配列を生成
            var hash = string.Join("", hashBytes.Select(x => x.ToString("x2"))); // byte配列を16進数表記で文字列に変換
            return hash;
        }

        private static string GetName(string format, string hash)
        {
            // {companyname} を置き換える。 {{ および }} でエスケープ可能（{{companyname} や {companyname}} は置き換えない）
            format = Regex.Replace(format, @"(?<!\{)\{companyname\}(?!\})", Application.companyName);
            format = Regex.Replace(format, @"(?<!\{)\{productname\}(?!\})", Application.productName);
            format = Regex.Replace(format, @"(?<!\{)\{md5_hash_of_data_path\}(?!\})", hash);
            format = format.Replace("{{", "{"); // エスケープされた { を戻す
            format = format.Replace("}}", "}"); // エスケープされた } を戻す
            return format;
        }

#if UNITY_EDITOR
        private string prevIndexedDBName;
        private bool isSavedOnReset;

        private static readonly string assetPath = $"ProjectSettings/{typeof(Save2IDB_Async_Settings).Name}.asset";

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(_indexedDBName)) { _indexedDBName = prevIndexedDBName; }
            else { prevIndexedDBName = _indexedDBName; }
        }

        internal static Save2IDB_Async_Settings Load()
        {
            var asset = (Save2IDB_Async_Settings)UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(assetPath).FirstOrDefault();
            if (asset == null)
            {
                asset = CreateInstance<Save2IDB_Async_Settings>();
            }
            asset.isSavedOnReset = true; // PlayerSettings として読み込まれた場合のみ、リセット時にセーブする。（プリセットではセーブしない）
            return asset;
        }

        internal void Save()
        {
            UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(new[] { this }, assetPath, true);
        }

        private void Reset()
        {
            // SettingsProvider や　Editor などの中ではリセット後の処理ができない（要検証）ためここで保存する。
            // （AssetSettingsProvider のリセット操作では OnValidate も呼ばれない）
            if (isSavedOnReset) { Save(); }
        }
#endif
    }
}
