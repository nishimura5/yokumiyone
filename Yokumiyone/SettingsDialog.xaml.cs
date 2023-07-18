using System.Windows;
using System.IO;
using System.Reflection;
using Common;
using System;
using Microsoft.WindowsAPICodePack.Shell;
using Yokumiyone.tables;
using static Yokumiyone.LandmarkTicketDialog;
using System.ComponentModel;
using System.Collections.ObjectModel;
using static Yokumiyone.MainWindow;

namespace Yokumiyone
{
    /// <summary>
    /// SettingsDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingsDialog : Window
    {
        #region Binding
        internal class Bind : INotifyPropertyChanged
        {
            #region INotifyPropertyChanged
            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChangedEventHandler? handler = this.PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
            }
            #endregion
            internal Bind() { }
            private bool _EnableLandpackDialog;
            public bool EnableLandpackDialog
            {
                get { return _EnableLandpackDialog; }
                set { _EnableLandpackDialog = value; OnPropertyChanged(nameof(EnableLandpackDialog)); }
            }
            private bool _EnableSceneExport;
            public bool EnableSceneExport
            {
                get { return _EnableSceneExport; }
                set { _EnableSceneExport = value; OnPropertyChanged(nameof(EnableSceneExport)); }
            }
        }
        internal Bind _Bind;
        #endregion

        PreferencesTable preferencesTable = new();

        public SettingsDialog(Window owner)
        {
            InitializeComponent();
            _Bind = new Bind();
            this.DataContext = _Bind;

            this.SizeToContent = SizeToContent.Height;

            this.Owner = owner;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // アプリケーションの情報
            string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yokumiyone.exe");
            using (var file = ShellFile.FromFilePath(exePath))
            {
                file.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
                IconImage.Source = file.Thumbnail.BitmapSource; // 256x256
            }
            string? versionStr = Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString();
            version.Text = $"Version {versionStr}";

            // 設定の読み込み
            var pref = preferencesTable.GetBoolPreferences();
            _Bind.EnableSceneExport = pref["enableSceneExport"];
            _Bind.EnableLandpackDialog = pref["enableLandpackDialog"];
            var prefString = preferencesTable.GetStringPreferences();
            pythonPathTextBox.Text = prefString["pythonPath"];
        }

        private void RemoveCashButton_Click(object sender, RoutedEventArgs e)
        {
            // XMPフォルダの削除
            string targetXmpFolderPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "Yokumiyone", "xmp");
            if (Directory.Exists(targetXmpFolderPath))
            {
                Directory.Delete(targetXmpFolderPath, true);
            }

            string targetSqlitePath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "Yokumiyone", "yokumiyone.sqlite3");
            // ファイルが存在しているか判断する
            if (File.Exists(targetSqlitePath))
            {
                // SQLiteファイルの削除
                File.Delete(targetSqlitePath);
                // SQLiteファイルの再生成
                YokumiyoneDb db = new YokumiyoneDb();
                db.CreateDb();
            }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            preferencesTable.SetEnableSceneExport(_Bind.EnableSceneExport);
            preferencesTable.SetEnableLandpackDialog(_Bind.EnableLandpackDialog);
            preferencesTable.SetPythonPath(pythonPathTextBox.Text);
        }
    }
}
