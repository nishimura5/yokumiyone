using System.Windows;
using System.IO;
using System.Reflection;
using Common;
using System;
using Microsoft.WindowsAPICodePack.Shell;

namespace Yokumiyone
{
    /// <summary>
    /// SettingsDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingsDialog : Window
    {
        public SettingsDialog(Window owner)
        {
            InitializeComponent();

            this.DataContext = this;
            this.SizeToContent = SizeToContent.Height;

            this.Owner = owner;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yokumiyone.exe");
            using (var file = ShellFile.FromFilePath(exePath))
            {
                file.Thumbnail.FormatOption = ShellThumbnailFormatOption.IconOnly;
                IconImage.Source = file.Thumbnail.BitmapSource; // 256x256
            }

            string? versionStr = Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString();
            version.Text = $"Version {versionStr}";
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
        }
    }
}
