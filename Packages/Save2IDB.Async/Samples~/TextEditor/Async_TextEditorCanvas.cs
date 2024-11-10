using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using SimpleFileBrowser;

namespace Save2IDB.Async.Samples
{
    public class Async_TextEditorCanvas : MonoBehaviour
    {
        [SerializeField] private Text _fileNameText = null;
        [SerializeField] private InputField _inputField = null;
        [SerializeField] private Button _loadButton = null;
        [SerializeField] private Button _saveButton = null;
        [SerializeField] private Button _exitButton = null;
        [SerializeField] private IDBA_DialogCanvas _dialogCanvas = null;

        private File currentFile;

        private void Awake()
        {
            _inputField.onValueChanged.AddListener(_ => SetDirty());
            _loadButton.onClick.AddListener(Load);
            _saveButton.onClick.AddListener(Save);
            _exitButton.onClick.AddListener(Exit);
            SetNewFile();
        }

        private void SetNewFile()
        {
            currentFile = new File(null, "New File");
            _fileNameText.text = currentFile.Name;
            _inputField.SetTextWithoutNotify(string.Empty);
        }

        private void SetDirty()
        {
            _fileNameText.text = currentFile.DirtyName;
            currentFile.SetDirty();
        }

        private void Load()
        {
            StartCoroutine(
            IDBA_FileBrowser.ShowLoadDialog(paths =>
            {
                StartCoroutine(LoadFileAsync(paths[0]).ToCoroutine());
            },
            null, IDBA_FileBrowser.PickMode.Files).ToCoroutine());
        }

        private void Save()
        {
            StartCoroutine(
            IDBA_FileBrowser.ShowSaveDialog(paths =>
            {
                StartCoroutine(SaveFileAsync(paths[0]).ToCoroutine());
            },
            null, IDBA_FileBrowser.PickMode.Files).ToCoroutine());
        }

        private void Exit()
        {
            if (currentFile.IsDirty) { StartCoroutine(CloseFileAsync(true).ToCoroutine()); }
            else { SceneManager.LoadScene(0); }
        }

        private async UniTask LoadFileAsync(string path)
        {
            await CloseFileAsync(false);
            if (currentFile.IsDirty) return;

            await using var fileStream = await IDBFileTask.OpenAsync(path, FileMode.Open);
            //var fileStream = System.IO.File.Open(path, FileMode.Open);
            var streamReader = new StreamReader(fileStream);
            _inputField.SetTextWithoutNotify(streamReader.ReadToEnd());
            _fileNameText.text = fileStream.Name;
            currentFile = new File(path, fileStream.Name);
            //fileStream.Close();
        }

        private async UniTask SaveFileAsync(string path)
        {
            await using var fileStream = await IDBFileTask.OpenAsync(path, FileMode.Create);
            //var fileStream = System.IO.File.Open(path, FileMode.Create);
            var streamWriter = new StreamWriter(fileStream);
            streamWriter.Write(_inputField.text);
            streamWriter.Flush();
            _fileNameText.text = fileStream.Name;
            currentFile = new File(path, fileStream.Name);
            //fileStream.Close();
            //return new UniTask();
        }

        private async UniTask CloseFileAsync(bool exit)
        {
            if (!currentFile.IsDirty) return;

            // ファイルが未保存のとき保存確認ダイアログ表示
            var dialogResult = await _dialogCanvas.ShowAsync($"Do you want to save changes to \"{currentFile.Name}\"?");
            if (dialogResult == IDBA_DialogCanvas.Result.Yes)
            {
                // Yes が押されたときは保存
                if (currentFile.Path == null)
                {
                    // 新規ファイルのときはファイルエクスプローラを展開
                    Save();
                    return;
                }
                else
                {
                    await SaveFileAsync(currentFile.Path);
                }
            }
            else if (dialogResult == IDBA_DialogCanvas.Result.Cancel)
            {
                // Cancel が押されたときは中断する
                return;
            }
            // No が押されたときはそのまま続ける

            // Yes または No が押されたとき新規ファイルをセット
            SetNewFile();

            if (exit) { SceneManager.LoadScene(0); }
        }

        private class File
        {
            public string Path { get; }
            public string Name { get; }
            public string DirtyName { get; }
            public bool IsDirty { get; private set; }

            public File(string path, string name)
            {
                Path = path;
                Name = name;
                DirtyName = name + "*";
                IsDirty = false;
            }

            public void SetDirty() => IsDirty = true;
        }
    }
}
