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
        /// jslib �̃R�[�h�X�g���b�v�΍�Ő錾�̂ݕK�v
        /// </summary>
        [DllImport("__Internal")]private static extern void Save2IDB_Async_requireFiler();

        [SerializeField] internal string _indexedDBName = "Save2IDB/{md5_hash_of_data_path}";

        private void OnEnable()
        {
            hideFlags = HideFlags.DontSaveInEditor; // ���̃I�u�W�F�N�g��ҏW�����Ƃ��V�[���� Dirty �ɂȂ�Ȃ��悤�ɂ���
#if UNITY_WEBGL && !UNITY_EDITOR
            var hash = GetHash();
            var indexedDBName = GetName(_indexedDBName, hash);
            Save2IDB_Async_Initialize(indexedDBName);
#endif
        }

        private static string GetHash()
        {
            var dataPath = Save2IDB_Async_GetDataPath(); // Application.dataPath ����� absoluteURL �͏������Ɏ��Ԃ�������̂� .jslib �Ăяo�����g�p
            var pathBytes = Encoding.ASCII.GetBytes(dataPath); // �n�b�V�������̂���byte�z��ɕϊ�

            using var md5 = new MD5CryptoServiceProvider();
            var hashBytes = md5.ComputeHash(pathBytes); // �n�b�V�����i�[����byte�z��𐶐�
            var hash = string.Join("", hashBytes.Select(x => x.ToString("x2"))); // byte�z���16�i���\�L�ŕ�����ɕϊ�
            return hash;
        }

        private static string GetName(string format, string hash)
        {
            // {companyname} ��u��������B {{ ����� }} �ŃG�X�P�[�v�\�i{{companyname} �� {companyname}} �͒u�������Ȃ��j
            format = Regex.Replace(format, @"(?<!\{)\{companyname\}(?!\})", Application.companyName);
            format = Regex.Replace(format, @"(?<!\{)\{productname\}(?!\})", Application.productName);
            format = Regex.Replace(format, @"(?<!\{)\{md5_hash_of_data_path\}(?!\})", hash);
            format = format.Replace("{{", "{"); // �G�X�P�[�v���ꂽ { ��߂�
            format = format.Replace("}}", "}"); // �G�X�P�[�v���ꂽ } ��߂�
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
            asset.isSavedOnReset = true; // PlayerSettings �Ƃ��ēǂݍ��܂ꂽ�ꍇ�̂݁A���Z�b�g���ɃZ�[�u����B�i�v���Z�b�g�ł̓Z�[�u���Ȃ��j
            return asset;
        }

        internal void Save()
        {
            UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(new[] { this }, assetPath, true);
        }

        private void Reset()
        {
            // SettingsProvider ��@Editor �Ȃǂ̒��ł̓��Z�b�g��̏������ł��Ȃ��i�v���؁j���߂����ŕۑ�����B
            // �iAssetSettingsProvider �̃��Z�b�g����ł� OnValidate ���Ă΂�Ȃ��j
            if (isSavedOnReset) { Save(); }
        }
#endif
    }
}
