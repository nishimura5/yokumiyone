using MaterialDesignThemes.Wpf;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using static Yokumiyone.MainWindow;

namespace Yokumiyone
{
    class VideoList
    {
        private ProgressBar loadProgressBar = new ProgressBar();
        private Expander expander = new Expander();
        private ObservableCollection<VideoProp> videoProps = new ObservableCollection<VideoProp>();
        public ObservableCollection<VideoProp> VideoProps { get { return videoProps; } }

        public void SetProgressBar(ProgressBar loadProgressBar)
        {
            this.loadProgressBar = loadProgressBar;
            this.loadProgressBar.IsIndeterminate = false;
        }

        public void SetExpander(Expander expander)
        {
            this.expander = expander;
        }

        public async Task Load(string targetFolderPath)
        {
            try
            {
                this.expander.Header = targetFolderPath;
                videoProps.Clear();
                BindingOperations.EnableCollectionSynchronization(videoProps, new object());

                // フォルダ内を検索してファイルパス一覧を取得
                DirectoryInfo d = new DirectoryInfo(targetFolderPath);
                FileInfo[] mp4Files = d.GetFiles("*.mp4");
                FileInfo[] movFiles = d.GetFiles("*.mov");
                FileInfo[] Files = mp4Files.Concat(movFiles).ToArray();
                string[] fileList = Files.Select(f => f.FullName).ToArray();

                // DBからファイルパス一覧と動画prop一覧を取得
                VideoPropTable videoPropDb = new VideoPropTable();
                ScenePathTable scenePathDb = new ScenePathTable();
                videoPropDb.SelectFolder(targetFolderPath);

                // DBにあって実際にはないデータをDELETE
                List<VideoProp> videoPropsDbPre = new List<VideoProp>(videoPropDb.VideoPropList);
                foreach (VideoProp prop in videoPropsDbPre)
                {
                    if (fileList.Contains(prop.FilePath) == false)
                    {
                        videoPropDb.Delete(prop.FilePath);
                        scenePathDb.DeletePath(prop.FilePath);
                    }
                }

                // 再SELECT
                videoPropDb.SelectFolder(targetFolderPath);
                ObservableCollection<VideoProp> videoPropsDb = new ObservableCollection<VideoProp>(videoPropDb.VideoPropList);

                this.loadProgressBar.Visibility = Visibility.Visible;
                IProgress<string> progress = new Progress<string>(onProgressChanged);
                this.loadProgressBar.Value = 0;
                this.loadProgressBar.Maximum = Files.Length;

                await Task.Run(() =>
                {
                    videoProps = videoPropsDb;
                    foreach (FileInfo file in Files)
                    {
                        progress.Report(file.ToString());
                        if (videoPropDb.VideoPathList.Contains(file.ToString()) == true)
                        {
                            continue;
                        }
                        VideoProp prop = new VideoProp(file.ToString());
                        videoProps.Add(prop);
                        videoPropDb.Insert(prop);
                    }
                });
                this.loadProgressBar.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void onProgressChanged(string file)
        {
            this.loadProgressBar.Value++;
        }

        public void OpenExpander()
        {
            this.expander.IsExpanded = true;
        }

    }
}
