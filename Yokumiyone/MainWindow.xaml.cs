
using Common;
using Microsoft.WindowsAPICodePack.Dialogs;
using Reactive.Bindings;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Forms;
using System.Collections.Generic;

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
                    handler(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion

            internal Bind() { }

            private ObservableCollection<VideoProp> _VideoProps;
            public ObservableCollection<VideoProp> VideoProps
            {
                get { return _VideoProps; }
                set { _VideoProps = value; OnPropertyChanged("VideoProps"); }
            }

            private ObservableCollection<SceneProp> _Scenes;
            public ObservableCollection<SceneProp> Scenes
            {
                get { return _Scenes; }
                set { _Scenes = value; OnPropertyChanged("Scenes"); }
            }
        }
        internal Bind _Bind;
        #endregion

        private string targetVideoPath = "";
        private TargetVideo targetVideo = new TargetVideo();
        private TweakSliderManager tweak = new TweakSliderManager();
        private VideoMetaData metadata = new VideoMetaData();
        private SkipPlayControl skipPlayControl = new SkipPlayControl();
        private CruisePlayControl cruisePlayControl = new CruisePlayControl();
        private VideoList videoList = new VideoList();

        private ReactivePropertySlim<DoubleCollection> _SceneStarts = new ReactivePropertySlim<DoubleCollection>();
        public ReactivePropertySlim<DoubleCollection> SceneStarts
        {
            get { return _SceneStarts; }
            set { _SceneStarts = value; }
        }

        public MainWindow()
        {
            InitializeComponent();
            _Bind = new Bind();
            this.DataContext = _Bind;
            _Bind.VideoProps = new ObservableCollection<VideoProp>();
            _Bind.Scenes = new ObservableCollection<SceneProp>();
            SceneStarts.Value = new DoubleCollection() { 0, 500, 1000};
            this.progressSlider.Ticks = SceneStarts.Value;

            skipPlayControl.SetControls(this.SkipSpans, this.SkipPlays, this.IsSkipMode);
            cruisePlayControl.SetControls(this.IsCruiseMode, this.IsFastMode);
            tweak.SetControls(TweakSlider, TweakRangeCombo);

            videoList.SetProgressBar(this.loadProgressBar);
            videoList.SetExpander(this.videoPropExpander);

            targetVideo.SetControl(
                progressSlider,
                playPauseButton,
                progressButton,
                skipPlayControl,
                cruisePlayControl);
        }

        private async void LoadButton_Click(object sender, RoutedEventArgs e) {
            string targetFolderPath = "C:/";
            var dialog = new CommonOpenFileDialog()
            {
                Title = "フォルダを選択してください",
                IsFolderPicker = true,
                InitialDirectory=@"E:/",
            };
            // ダイアログを表示
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (Directory.Exists(dialog.FileName) == true)
                {
                    targetFolderPath = dialog.FileName;
                }
            }

            _Bind.VideoProps.Clear();
            await videoList.Load(targetFolderPath);
            _Bind.VideoProps = videoList.VideoProps;
            videoList.OpenExpander();
        }

        private void VideoItem_Click(object sender, MouseButtonEventArgs e)
        {
            VideoProp selectedRow = (VideoProp)this.videoPropDataGrid.SelectedItem;

            // チャプター情報をクリア
            SceneStarts.Value.Clear();
            SceneStarts.Value.Add(0);
            _Bind.Scenes.Clear();
            this.sceneGrid.IsReadOnly = true;

            selectedRow.SetState("selected");
            if (selectedRow == null)
            {
                return;
            }
            // 動画を読み込み
            targetVideoPath = selectedRow.FilePath;
            movieWindowMediaElement.Source = new System.Uri(targetVideoPath, UriKind.Absolute);
            targetVideo.SetMediaElement(movieWindowMediaElement);

            // metadataを動画ファイルから読み込み
            metadata.LoadMetaDataFile(targetVideoPath);
            foreach(var chapter in metadata.ScenePropOc)
            {
                SceneStarts.Value.Add(chapter.StartTime.TotalSeconds / selectedRow.Duration.TotalSeconds * 1000);
                _Bind.Scenes.Add(chapter.DeepCopy());
            }
            _Bind.Scenes = new ObservableCollection<SceneProp>(_Bind.Scenes.OrderBy(n => n.StartTime));

            cruisePlayControl.SetScenes(_Bind.Scenes);

            targetVideo.Play();
        }

        // 選択が外れたときにMP4への埋め込みとDBの更新を行う
        private async void VideoItem_Unselected(object sender, EventArgs e)
        {
            VideoProp selectedRow = (VideoProp)this.videoPropDataGrid.SelectedItem;
            if (selectedRow == null)
            {
                return;
            }
            // シーン数を更新
            int NumOfScene = _Bind.Scenes.Count();
            selectedRow.SetNumOfScene(NumOfScene);

            // DB更新用データ
            targetVideoPath = selectedRow.FilePath;
            VideoPropTable videoPropDb = new VideoPropTable();
            videoPropDb.UpdateSceneCnt(targetVideoPath, NumOfScene.ToString());

            List<string> sceneList = new List<string>();
            foreach (var chapter in _Bind.Scenes)
            {
                sceneList.Add(chapter.Title);
            }
            ScenePathTable scenePathDb = new ScenePathTable(targetVideoPath);
            scenePathDb.Update(sceneList);

            // localAppPathにあるtxtを上書きしてmp4に埋め込み
            selectedRow.SetState("updating");
            await metadata.UpdateMetaData(_Bind.Scenes);
            selectedRow.SetState("unselected");
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

        // 時刻ボタンをクリックするとチャプター追加
        // 終了時刻列が選択されていたら終了時刻を追加
        private void ProgressButton_Click(object sender, RoutedEventArgs e)
        {
            string progressButtonTime = progressButton.Content.ToString() ?? "";
            if (progressButtonTime.Length == 0)
            {
                return;
            }
            progressButtonTime = progressButtonTime.Substring(1);

            // 行が選択されていたら終了時刻を追加
            if (this.sceneGrid.SelectedItem != null)
            {
                SceneProp selectedRow = (SceneProp)this.sceneGrid.SelectedItem;
                selectedRow.EndTimeStr = progressButtonTime;
            }
            // 行が選択されていなかったら新しい開始時刻を追加
            else
            {
                string default_scene_title = "Untitled";
                SceneProp chapter = new SceneProp(progressButtonTime, "", default_scene_title);
                SceneStarts.Value.Add(chapter.StartTime.TotalSeconds / targetVideo.TotalSec * 1000);
                _Bind.Scenes.Add(chapter);
            }
            _Bind.Scenes = new ObservableCollection<SceneProp>(_Bind.Scenes.OrderBy(n => n.StartTime));
            // チャプター選択を解除、終了時刻の誤上書き防止
            this.sceneGrid.SelectedItem = null;
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
            if(targetVideo.IsMovieEnable() == false)
            {
                return;
            }
            targetVideo.Pause();
            tweak.SetPreMs(targetVideo.GetMovieProgressMs());
            // チャプター選択を解除、終了時刻の誤上書き防止
            this.sceneGrid.SelectedItem = null;
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
            TweakSlider.Value = 0;
        }

        private void SelectChapter_Click(object sender, MouseButtonEventArgs e)
        {
            if (this.sceneGrid.CurrentColumn == null)
            {
                return;
            }
            int selectedColNum = this.sceneGrid.CurrentColumn.DisplayIndex;
            SceneProp selectedRow = (SceneProp)this.sceneGrid.SelectedItem;

            // タイトル列クリックではJumpしない
            if(selectedColNum == 0)
            {
                targetVideo.Jump(selectedRow.StartTime);
                this.sceneGrid.SelectedItem = null;
            }
            else if(selectedColNum == 1 && selectedRow.EndTimeStr != "")
            {
                targetVideo.Jump(selectedRow.EndTime);
                this.sceneGrid.SelectedItem = null;
            }
        }

        // 右クリック操作
        private void SceneMenu_Opening(object sender, DependencyPropertyChangedEventArgs e)
        {
            var selectedRow = this.sceneGrid.SelectedItem;
            if(selectedRow == null)
            {
                editSceneTitle.IsEnabled = false;
                removeScene.IsEnabled = false;
            }
            else
            {
                editSceneTitle.IsEnabled = true;
                removeScene.IsEnabled = true;
            }
        }
        private void EditSceneTitle_Click(object sender, RoutedEventArgs e)
        {
            SceneProp selectedRow = (SceneProp)this.sceneGrid.SelectedItem;
            TitleEditDialog titleEditDialog = new TitleEditDialog(this, selectedRow.Title);
            var res = titleEditDialog.ShowDialog();
            if(res == true)
            {
                selectedRow.Title = titleEditDialog.SceneTitle;
            }
            _Bind.Scenes = new ObservableCollection<SceneProp>(_Bind.Scenes.OrderBy(n => n.StartTime));
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
        }
    }
}
