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

            // �t�@�C�������ۑ��̂Ƃ��ۑ��m�F�_�C�A���O�\��
            var dialogResult = await _dialogCanvas.ShowAsync($"Do you want to save changes to \"{currentFile.Name}\"?");
            if (dialogResult == IDBA_DialogCanvas.Result.Yes)
            {
                // Yes �������ꂽ�Ƃ��͕ۑ�
                if (currentFile.Path == null)
                {
                    // �V�K�t�@�C���̂Ƃ��̓t�@�C���G�N�X�v���[����W�J
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
                // Cancel �������ꂽ�Ƃ��͒��f����
                return;
            }
            // No �������ꂽ�Ƃ��͂��̂܂ܑ�����

            // Yes �܂��� No �������ꂽ�Ƃ��V�K�t�@�C�����Z�b�g
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
