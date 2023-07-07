using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Yokumiyone.tables;

namespace Yokumiyone
{
    class VideoList
    {
        private ProgressBar loadProgressBar = new();
        private Expander expander = new();
        private ObservableCollection<VideoProp> videoProps = new();
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
                DirectoryInfo d = new(targetFolderPath);
                FileInfo[] mp4Files = d.GetFiles("*.mp4");
                FileInfo[] movFiles = d.GetFiles("*.mov");
                FileInfo[] Files = mp4Files.Concat(movFiles).ToArray();
                string[] fileList = Files.Select(f => f.FullName).ToArray();

                // DBからファイルパス一覧と動画prop一覧を取得
                VideoPropTable videoPropDb = new();
                ScenePathTable scenePathDb = new();
                videoPropDb.SelectFolder(targetFolderPath);

                List<VideoProp> videoPropsDbPre = new(videoPropDb.VideoPropList);
                foreach (VideoProp prop in videoPropsDbPre)
                {
                    // DBにあって実際にはないデータをDELETE
                    if (fileList.Contains(prop.FilePath) == false)
                    {
                        videoPropDb.Delete(prop.FilePath);
                        scenePathDb.DeletePath(prop.FilePath);
                    }
                    else
                    {
                        // 更新日が変わっている(外部で変更された可能性のある)データをDELETE
                        DateTime writeTimeFile = File.GetLastWriteTime(prop.FilePath);
                        DateTime writeTimeDb = prop.ModifiedDatetime;
                        if (Math.Abs((writeTimeFile - writeTimeDb).TotalSeconds) > 1)
                        {
                            videoPropDb.Delete(prop.FilePath);
                            scenePathDb.DeletePath(prop.FilePath);
                        }
                    }
                }

                // 再SELECT
                videoPropDb.SelectFolder(targetFolderPath);
                ObservableCollection<VideoProp> videoPropsDb = new(videoPropDb.VideoPropList);

                this.loadProgressBar.Visibility = Visibility.Visible;
                IProgress<string> progress = new Progress<string>(OnProgressChanged);
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
                        VideoProp prop = new(file.ToString());
                        videoProps.Add(prop);
                        videoPropDb.Insert(prop);
                    }
                });
                this.loadProgressBar.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.GetType().Name);
            }
        }

        private void OnProgressChanged(string file)
        {
            this.loadProgressBar.Value++;
        }

        public void OpenExpander()
        {
            this.expander.IsExpanded = true;
        }

        public void CloseExpander()
        {
            this.expander.IsExpanded = false;
        }
    }
}
