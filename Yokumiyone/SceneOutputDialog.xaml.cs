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
        private TimeSpan duration;
        private string srcVideoPath;
        private string dstFolderPath = "";

        public SceneOutputDialog(Window owner, SceneProp scene, string srcVideoPath)
        {
            InitializeComponent();
            this.srcVideoPath = srcVideoPath;
            this.scene = scene;
            this.duration = scene.CalcMidTime();
        }
        private void DstSelectButton_Click(object sender, RoutedEventArgs e){
            if (dstFolderPath == "")
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
        }

        private void ExecOutput_Click(object sender, RoutedEventArgs e)
        {
            if (dstFolderPath == "")
            {
                dstFolderPath = Path.GetDirectoryName(srcVideoPath);
            }
            var ffmpeg = new Ffmpeg(srcVideoPath);
            ffmpeg.ExportPng(scene.StartTimeStr, scene.SceneDuration, "1", dstFolderPath);

            return;
        }
    }
}
