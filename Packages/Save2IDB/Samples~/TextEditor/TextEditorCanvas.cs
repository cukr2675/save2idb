using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.IO.Compression;
using System.Text;
using System.Net.Mime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using SimpleFileBrowser;

namespace Save2IDB.Samples
{
    public class TextEditorCanvas : MonoBehaviour
    {
        [SerializeField] private Text _fileNameText = null;
        [SerializeField] private InputField _inputField = null;
        [SerializeField] private Dropdown _dropdownMenu = null;
        [SerializeField] private Button _exitButton = null;
        [SerializeField] private DialogCanvas _dialogCanvas = null;

        private TextFile currentFile;

        private static readonly Encoding encoding = Encoding.UTF8;

        private void Start()
        {
            _inputField.onValueChanged.AddListener(_ => SetDirty());
            _dropdownMenu.options.Add(new Dropdown.OptionData("Open file..."));
            _dropdownMenu.options.Add(new Dropdown.OptionData("Save as..."));
            _dropdownMenu.options.Add(new Dropdown.OptionData("Import file"));
            _dropdownMenu.options.Add(new Dropdown.OptionData("Import files to IDB"));
            _dropdownMenu.options.Add(new Dropdown.OptionData("Export file"));
            _dropdownMenu.options.Add(new Dropdown.OptionData("Export files from IDB..."));
            _dropdownMenu.options.Add(new Dropdown.OptionData("Exit"));
            ResetDropdown();
            _dropdownMenu.onValueChanged.AddListener(value =>
            {
                if (_dropdownMenu.value == 0) { Load(); }
                else if (_dropdownMenu.value == 1) { Save(); }
                else if (_dropdownMenu.value == 2) { StartCoroutine(ImportFile().ToCoroutine()); }
                else if (_dropdownMenu.value == 3) { StartCoroutine(ImportFilesToIDB().ToCoroutine()); }
                else if (_dropdownMenu.value == 4) { ExportFile(); }
                else if (_dropdownMenu.value == 5) { ExportFilesFromIDB(); }
                else if (_dropdownMenu.value == 6) { Exit(); }
                ResetDropdown();
            });
            _exitButton.onClick.AddListener(Exit);
            SetNewFile();
        }

        private void ResetDropdown()
        {
            _dropdownMenu.options.Add(new Dropdown.OptionData("File"));
            _dropdownMenu.SetValueWithoutNotify(_dropdownMenu.options.Count - 1);
            _dropdownMenu.options.RemoveAt(_dropdownMenu.options.Count - 1);
        }

        private void SetNewFile()
        {
            currentFile = new TextFile(null);
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
            FileBrowser.ShowLoadDialog(paths =>
            {
                StartCoroutine(LoadFileAsync(paths[0]).ToCoroutine());
            },
            null, FileBrowser.PickMode.Files);
        }

        private void Save()
        {
            FileBrowser.ShowSaveDialog(paths =>
            {
                StartCoroutine(SaveFileAsync(paths[0]).ToCoroutine());
            },
            null, FileBrowser.PickMode.Files);
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

            var fi = new FileInfo(path);
            Debug.Log(
                $"Loaded: {fi.FullName}, attr: {fi.Attributes}, ro: {fi.IsReadOnly}, len: {fi.Length}, " +
                $"at: {fi.LastAccessTime}, mt: {fi.LastWriteTime}, ct: {fi.CreationTime}");

            using var fileStream = IDBFile.Open(path, FileMode.Open);
            //var fileStream = System.IO.File.Open(path, FileMode.Open);
            var streamReader = new StreamReader(fileStream);
            _inputField.SetTextWithoutNotify(streamReader.ReadToEnd());
            _fileNameText.text = fileStream.Name;
            currentFile = new TextFile(path);
            //fileStream.Close();
        }

        private UniTask SaveFileAsync(string path)
        {
            using var fileStream = IDBFile.Open(path, FileMode.Create);
            //var fileStream = System.IO.File.Open(path, FileMode.Create);
            var streamWriter = new StreamWriter(fileStream);
            streamWriter.Write(_inputField.text);
            streamWriter.Flush();
            _fileNameText.text = fileStream.Name;
            currentFile = new TextFile(path);
            //fileStream.Close();
            return new UniTask();
        }

        private async UniTask CloseFileAsync(bool exit)
        {
            if (!currentFile.IsDirty) return;

            // ファイルが未保存のとき保存確認ダイアログ表示
            var dialogResult = await _dialogCanvas.ShowAsync($"Do you want to save changes to \"{currentFile.Name}\"?");
            if (dialogResult == DialogCanvas.Result.Yes)
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
            else if (dialogResult == DialogCanvas.Result.Cancel)
            {
                // Cancel が押されたときは中断する
                return;
            }
            // No が押されたときはそのまま続ける

            // Yes または No が押されたとき新規ファイルをセット
            SetNewFile();

            if (exit) { SceneManager.LoadScene(0); }
        }

        private async UniTask ImportFile()
        {
            await CloseFileAsync(false);
            if (currentFile.IsDirty) return;

            using var importer = IDBImporter.ToMemoryStream();

            await importer.ShowDialog();
            if (importer.Status == IDBOperationStatus.Failed || importer.Result.Length == 0) return;

            var readOperation = importer.Result[0].OpenMemoryStreamAsync();

            await readOperation;
            if (readOperation.Status == IDBOperationStatus.Failed) return;

            _inputField.text = encoding.GetString(readOperation.Result.GetBuffer(), 0, (int)readOperation.Result.Length);
            _fileNameText.text = importer.Result[0].FileName;
            currentFile = new TextFile(importer.Result[0].FileName);
        }

        private async UniTask ImportFilesToIDB()
        {
            using var importer = IDBImporter.InToDirectory(Application.persistentDataPath);
            importer.Multiselect = true;
            await importer.ShowDialog();
        }

        private void ExportFile()
        {
            var bytes = encoding.GetBytes(_inputField.text);
            var exporter = IDBExporter.FromBytes(bytes, 0, bytes.Length);
            exporter.FileNameAs = currentFile.Name;
            exporter.Export();
        }

        private void ExportFilesFromIDB()
        {
            FileBrowser.ShowLoadDialog(paths =>
            {
                if (paths.Length == 1)
                {
                    var exporter = IDBExporter.FromFile(paths[0]);
                    exporter.Export();
                }
                else if (paths.Length >= 2)
                {
                    using var memoryStream = new MemoryStream();
                    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var path in paths)
                        {
                            var entry = archive.CreateEntry(Path.GetFileName(path));
                            using var writer = new BinaryWriter(entry.Open());
                            writer.Write(File.ReadAllBytes(path));
                        }
                    }

                    memoryStream.Position = 0;
                    var exporter = IDBExporter.FromBytes(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
                    exporter.FileNameAs = "Archive.zip";
                    exporter.ContentType = MediaTypeNames.Application.Zip;
                    exporter.Export();
                }
            },
            null, FileBrowser.PickMode.Files, true);
        }

        private class TextFile
        {
            public string Path { get; }
            public string Name { get; }
            public string DirtyName { get; }
            public bool IsDirty { get; private set; }

            public TextFile(string path)
            {
                Path = path;
                Name = path != null ? System.IO.Path.GetFileName(path) : "New File.txt";
                DirtyName = Name + "*";
                IsDirty = false;
            }

            public void SetDirty() => IsDirty = true;
        }
    }
}
