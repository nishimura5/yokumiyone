
using Common;
using Microsoft.WindowsAPICodePack.Dialogs;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Yokumiyone.tables;

namespace Yokumiyone
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
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

            private ObservableCollection<VideoProp> _VideoProps;
            public ObservableCollection<VideoProp> VideoProps
            {
                get { return _VideoProps; }
                set { _VideoProps = value; OnPropertyChanged(nameof(VideoProps)); }
            }

            private ObservableCollection<SceneProp> _Scenes;
            public ObservableCollection<SceneProp> Scenes
            {
                get { return _Scenes; }
                set { _Scenes = value; OnPropertyChanged(nameof(Scenes)); }
            }
        }
        internal Bind _Bind;
        #endregion

        private string targetFolderPath = "";
        private string targetVideoPath = "";
        private int writingVideoNum = 0;
        private readonly TargetVideo targetVideo = new();
        private readonly TweakSliderManager tweak = new();
        private readonly VideoMetaData metadata = new();
        private readonly SkipPlayControl skipPlayControl = new();
        private readonly CruisePlayControl cruisePlayControl = new();
        private readonly VideoList videoList = new();

        private ReactivePropertySlim<DoubleCollection> _SceneStarts = new();
        public ReactivePropertySlim<DoubleCollection> SceneStarts
        {
            get { return _SceneStarts; }
            set { _SceneStarts = value; }
        }

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                _Bind = new Bind();
                this.DataContext = _Bind;
                _Bind.VideoProps = new ObservableCollection<VideoProp>();
                _Bind.Scenes = new ObservableCollection<SceneProp>();
                SceneStarts.Value = new DoubleCollection() { 0, 500, 1000 };
                this.progressSlider.Ticks = SceneStarts.Value;

                skipPlayControl.SetControls(this.skipSpans, this.skipPlays, this.isSkipMode);
                cruisePlayControl.SetControls(this.isCruiseMode, this.isFastMode);
                tweak.SetControls(tweakSlider, tweakRangeCombo);

                videoList.SetProgressBar(this.loadProgressBar);
                videoList.SetExpander(this.videoPropExpander);

                targetVideo.SetControl(
                    progressSlider,
                    playPauseButton,
                    progressButton,
                    skipPlayControl,
                    cruisePlayControl);

                // コマンドライン引数
                string[] args = Environment.GetCommandLineArgs();
                if (args.Length > 1)
                {
                    targetVideoPath = args[1];
                    targetFolderPath = new FileInfo(targetVideoPath).Directory.FullName;
                    VideoProp tarProp = new(targetVideoPath);
                    LoadVideo(tarProp);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, this.GetType().Name);
            }
        }

        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (targetFolderPath == "")
                {
                    targetFolderPath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                }
                var dialog = new CommonOpenFileDialog()
                {
                    Title = "フォルダを選択してください",
                    IsFolderPicker = true,
                    InitialDirectory = targetFolderPath,
                };
                // ダイアログを表示
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    if (Directory.Exists(dialog.FileName) == true)
                    {
                        targetFolderPath = dialog.FileName;
                    }
                }

                // チャプター情報をクリア
                SceneStarts.Value.Clear();
                SceneStarts.Value.Add(0);
                _Bind.Scenes.Clear();
                this.sceneGrid.IsReadOnly = true;

                targetVideo.Release();

                // ファイルリストをクリア
                videoList.CloseExpander();
                _Bind.VideoProps.Clear();
                await videoList.Load(targetFolderPath);
                _Bind.VideoProps = videoList.VideoProps;
                videoList.OpenExpander();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, this.GetType().Name);
            }
        }

        private async void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsDialog settingsDialog = new(this);
            var _ = settingsDialog.ShowDialog();
        }

        private void VideoItem_Click(object sender, MouseButtonEventArgs e)
        {
            VideoProp selectedRow = (VideoProp)this.videoPropDataGrid.SelectedItem;
            LoadVideo(selectedRow);
        }

        private void LoadVideo(VideoProp targetProp)
        {
            if (targetProp == null)
            {
                return;
            }
            // チャプター情報をクリア
            SceneStarts.Value.Clear();
            SceneStarts.Value.Add(0);
            _Bind.Scenes.Clear();
            this.sceneGrid.IsReadOnly = true;
            targetProp.SetState("selected");

            // 動画を読み込み
            targetVideoPath = targetProp.FilePath;
            movieWindowMediaElement.Source = new System.Uri(targetVideoPath, UriKind.Absolute);
            targetVideo.SetMediaElement(movieWindowMediaElement);

            // metadataをxmpファイルから読み込み
            metadata.LoadMetaDataFile(targetVideoPath);
            foreach (var sceneProp in metadata.ScenePropOc)
            {
                SceneStarts.Value.Add(sceneProp.StartTime.TotalSeconds / targetProp.Duration.TotalSeconds * 1000);
                _Bind.Scenes.Add(sceneProp.DeepCopy());
            }
            _Bind.Scenes = new ObservableCollection<SceneProp>(_Bind.Scenes.OrderBy(n => n.StartTime));

            targetVideo.Rotate(targetProp.Rotation);
            cruisePlayControl.SetScenes(_Bind.Scenes);

            targetVideo.Play();
        }

        private async void WriteMetadataButton_Click(object sender, EventArgs e)
        {
            if (this.videoPropDataGrid.SelectedItems.Count > 0)
            {
                VideoProp selectedRow = (VideoProp)this.videoPropDataGrid.SelectedItem;
                this.videoPropDataGrid.SelectedItem = null;
                await WriteMetadata(selectedRow);
            }
        }

        // 選択が外れたときにMP4への埋め込みとDBの更新を行う
        private async void VideoItem_Unselected(object sender, EventArgs e)
        {
            VideoProp selectedRow = (VideoProp)this.videoPropDataGrid.SelectedItem;
            await WriteMetadata(selectedRow);
        }

        private async Task WriteMetadata(VideoProp selectedRow)
        {
            if (selectedRow == null)
            {
                return;
            }
            // シーン数を更新
            int NumOfScene = _Bind.Scenes.Count;
            selectedRow.SetNumOfScene(NumOfScene);

            List<string> sceneList = new();
            foreach (var sceneProp in _Bind.Scenes)
            {
                sceneList.Add(sceneProp.Title);
            }
            ScenePathTable scenePathDb = new(targetVideoPath);
            scenePathDb.Update(sceneList);

            targetVideoPath = selectedRow.FilePath;

            // mp4の上書き中はフォルダ選択をdisableにする
            folderLoadButton.IsEnabled = false;
            writingVideoNum++;

            // localAppPathにあるtxtを上書きしてmp4に埋め込み
            selectedRow.SetState("updating");
            await metadata.UpdateMetaData(_Bind.Scenes);
            selectedRow.SetState("unselected");

            // DB更新
            VideoPropTable videoPropDb = new();
            videoPropDb.UpdateSceneCnt(targetVideoPath, NumOfScene.ToString());

            // mp4上書きが終わったらフォルダ選択をenableにする
            writingVideoNum--;
            if (writingVideoNum < 1)
            {
                folderLoadButton.IsEnabled = true;
            }
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (targetVideo.GetState() == MediaState.Play)
            {
                targetVideo.Pause();
            }
            else if (targetVideo.GetState() == MediaState.Pause)
            {
                targetVideo.Play();
            }

            // チャプター選択を解除、終了時刻の誤上書き防止
            this.sceneGrid.SelectedItem = null;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                PlayPauseButton_Click((object)sender, e);
            }
            else if (e.Key == Key.A)
            {
                targetVideo.Step(-1000);
            }
            else if (e.Key == Key.D)
            {
                targetVideo.Step(1000);
            }
}

        // 時刻ボタンをクリックするとチャプター追加
        // 終了時刻列が選択されていたら終了時刻を追加
        private void ProgressButton_Click(object sender, RoutedEventArgs e)
        {
            string progressButtonTime = progressButton.Content.ToString() ?? "";
            if (progressButtonTime.Length == 0)
            {
                return;
            }
            progressButtonTime = progressButtonTime[1..];

            string default_scene_title = "Untitled";
            SceneProp sceneProp = new(progressButtonTime, "", default_scene_title);
            SceneStarts.Value.Add(sceneProp.StartTime.TotalSeconds / targetVideo.TotalSec * 1000);
            _Bind.Scenes.Add(sceneProp);
            _Bind.Scenes = new ObservableCollection<SceneProp>(_Bind.Scenes.OrderBy(n => n.StartTime));
            // シーン選択
            this.sceneGrid.SelectedItem = sceneProp;

            cruisePlayControl.SetScenes(_Bind.Scenes);
        }
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            string progressButtonTime = progressButton.Content.ToString() ?? "";
            if (progressButtonTime.Length == 0)
            {
                return;
            }
            progressButtonTime = progressButtonTime[1..];

            // 行が選択されていたら終了時刻を更新
            if (this.sceneGrid.SelectedItem != null)
            {
                SceneProp selectedRow = (SceneProp)this.sceneGrid.SelectedItem;
                selectedRow.StartTimeStr = progressButtonTime;
                _Bind.Scenes = new ObservableCollection<SceneProp>(_Bind.Scenes.OrderBy(n => n.StartTime));
                cruisePlayControl.SetScenes(_Bind.Scenes);
            }
        }
        private void EndButton_Click(object sender, RoutedEventArgs e)
        {
            string progressButtonTime = progressButton.Content.ToString() ?? "";
            if (progressButtonTime.Length == 0)
            {
                return;
            }
            progressButtonTime = progressButtonTime[1..];

            // 行が選択されていたら終了時刻を更新
            if (this.sceneGrid.SelectedItem != null)
            {
                SceneProp selectedRow = (SceneProp)this.sceneGrid.SelectedItem;
                selectedRow.EndTimeStr = progressButtonTime;
                _Bind.Scenes = new ObservableCollection<SceneProp>(_Bind.Scenes.OrderBy(n => n.StartTime));
                cruisePlayControl.SetScenes(_Bind.Scenes);
            }
        }

        private void Movie_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                targetVideo.Step(100);
            }
            else if (e.Delta < 0)
            {
                targetVideo.Step(-100);
            }
        }

        private void MovieOpened(object sender, RoutedEventArgs e)
        {
            targetVideo.TimerStart();
            // ↓ファイルからアプリを開いたときにpauseアイコンをボタンに表示させるため
            targetVideo.Pause();
        }

        private void Tweak_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (targetVideo.IsMovieEnable() == false)
            {
                return;
            }
            targetVideo.Pause();
            tweak.SetPreMs(targetVideo.GetMovieProgressMs());
        }

        private void Tweak_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue != 0 && targetVideo.IsMovieEnable() == true)
            {
                targetVideo.Jump(tweak.GetPostMs());
            }
        }
        private void Tweak_MouseUp(object sender, MouseButtonEventArgs e)
        {
            tweakSlider.Value = 0;
        }

        private void SceneRow_Click(object sender, MouseButtonEventArgs e)
        {
            if (this.sceneGrid.CurrentColumn == null)
            {
                return;
            }
            int selectedColNum = this.sceneGrid.CurrentColumn.DisplayIndex;
            SceneProp selectedRow = (SceneProp)this.sceneGrid.SelectedItem;

            // タイトル列クリックではJumpしない
            if (selectedColNum == 0)
            {
                targetVideo.Jump(selectedRow.StartTime);
                this.sceneGrid.SelectedItem = null;
            }
            else if (selectedColNum == 1 && selectedRow.EndTimeStr != "")
            {
                targetVideo.Jump(selectedRow.EndTime);
                this.sceneGrid.SelectedItem = null;
            }
        }

        // 右クリック操作
        private void SceneMenu_Opening(object sender, DependencyPropertyChangedEventArgs e)
        {
            var selectedRow = this.sceneGrid.SelectedItem;
            if (selectedRow == null)
            {
                editSceneTitle.IsEnabled = false;
                sceneOutput.IsEnabled = false;
                removeScene.IsEnabled = false;
            }
            else
            {
                editSceneTitle.IsEnabled = true;
                sceneOutput.IsEnabled = true;
                removeScene.IsEnabled = true;
            }
        }
        private void EditSceneTitle_Click(object sender, RoutedEventArgs e)
        {
            SceneProp selectedRow = (SceneProp)this.sceneGrid.SelectedItem;
            TitleEditDialog titleEditDialog = new(this, selectedRow.Title);
            var res = titleEditDialog.ShowDialog();
            if (res == true)
            {
                selectedRow.Title = titleEditDialog.SceneTitle;
            }
            _Bind.Scenes = new ObservableCollection<SceneProp>(_Bind.Scenes.OrderBy(n => n.StartTime));
        }

        private void SceneOutput_Click(object sender, RoutedEventArgs e)
        {
            SceneProp selectedRow = (SceneProp)this.sceneGrid.SelectedItem;
            SceneOutputDialog sceneOutputDialog = new(this, selectedRow, targetVideoPath);
            var _ = sceneOutputDialog.ShowDialog();
        }

        private void LandmarkTicket_Click(object sender, RoutedEventArgs e)
        {
            VideoProp selectedVideo = (VideoProp)this.videoPropDataGrid.SelectedItem;
            SceneProp selectedScene = (SceneProp)this.sceneGrid.SelectedItem;
            if(selectedVideo == null) {
                return;
            }
            LandmarkTicketDialog landmarkTicketDialog = new(this, selectedScene, targetVideoPath, selectedVideo.Fps);
            var _ = landmarkTicketDialog.ShowDialog();
        }

        private void RemoveScene_Click(object sender, RoutedEventArgs e)
        {

            SceneProp selectedRow = (SceneProp)this.sceneGrid.SelectedItem;
            string title = selectedRow.Title;

            int idx = this.sceneGrid.SelectedIndex;

            if (idx >= _Bind.Scenes.Count || idx < 0)
            {
                return;
            }
            _Bind.Scenes.RemoveAt(idx);

            cruisePlayControl.SetScenes(_Bind.Scenes);
        }
    }
}
