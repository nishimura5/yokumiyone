using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Yokumiyone
{
    /// <summary>
    /// sceneOutputDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class SceneOutputDialog : Window
    {
        private SceneProp scene = new SceneProp();
        private SceneOutput ctrl = new SceneOutput();

        private string framerateOut = "1";
        public string Framerate
        {
            get { return framerateOut; }
            set { framerateOut = value; }
        }

        private string srcVideoPath;
        private string? dstFolderPath;

        public SceneOutputDialog(Window owner, SceneProp scene, string srcVideoPath)
        {
            InitializeComponent();
            this.srcVideoPath = srcVideoPath;
            this.scene = scene;

            this.DataContext = this;
            this.SizeToContent = SizeToContent.Height;

            this.Owner = owner;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            ctrl.SetControls(framerate);
        }

        private void titleOk_Click(object sender, RoutedEventArgs e){
            if (string.IsNullOrEmpty(dstFolderPath) == true)
            {
                dstFolderPath = Path.GetDirectoryName(srcVideoPath);
            }
            var dialog = new CommonOpenFileDialog()
            {
                Title = "出力先フォルダを選択してください",
                IsFolderPicker = true,
                InitialDirectory = dstFolderPath,
            };
            // ダイアログを表示
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (Directory.Exists(dialog.FileName) == true)
                {
                    dstFolderPath = dialog.FileName;
                }
            }
            this.Topmost = true;
            this.Topmost = false;

            var ffmpeg = new Ffmpeg(srcVideoPath, scene.Title, scene.StartTimeStr, scene.SceneDuration);
            ffmpeg.ExportPng(ctrl.Framerate.ToString(), dstFolderPath);
            this.DialogResult = true;
        }
        private void titleCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
