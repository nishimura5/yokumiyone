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

namespace Yokumiyone
{
    /// <summary>
    /// sceneOutputDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class SceneOutputDialog : Window
    {
        private SceneProp scene = new SceneProp();
        private string srcVideoPath;

        public SceneOutputDialog(Window owner, SceneProp scene, string srcVideoPath)
        {
            InitializeComponent();
            this.srcVideoPath = srcVideoPath;
            this.scene = scene;
        }
        private void ExecOutput_Click(object sender, RoutedEventArgs e)
        {
            var ffmpeg = new Ffmpeg(srcVideoPath);
            string dstDir = Path.GetDirectoryName(srcVideoPath);
            ffmpeg.ExportPng(scene.StartTimeStr, scene.SceneDuration, "1", dstDir);

            return;
        }
    }
}
