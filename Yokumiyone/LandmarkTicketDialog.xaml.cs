using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows;
using Newtonsoft.Json;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Yokumiyone
{
    /// <summary>
    /// LandmarkTicketDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class LandmarkTicketDialog : Window
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

            private ObservableCollection<Landmarks> _StdPoints;
            public ObservableCollection<Landmarks> StdPoints
            {
                get { return _StdPoints; }
                set { _StdPoints = value; OnPropertyChanged(nameof(StdPoints)); }
            }

            private ObservableCollection<Landmarks> _Points;
            public ObservableCollection<Landmarks> Points
            {
                get { return _Points; }
                set { _Points = value; OnPropertyChanged(nameof(Points)); }
            }
        }
        internal Bind _Bind;
        #endregion

        // mouse overした点
        readonly MouseoverPoint mouseoverPoint = new();
         Point postPoint = new();

        Ellipse landmarkSelectedCircle = new();
        readonly Landmarks baseLandpack = new();
        readonly SelectedPoints selectedPoints = new(60, 60, 230);
        readonly SelectedPoints selectedPointsOnGrid = new(60, 230, 60);

        readonly string videoPath = "";
        readonly float fps=1;
        private string? dstFolderPath;
        string dstFilePath = "";
        readonly SceneProp scene;
        private readonly int _mouseOverThreshold = 5;


        public LandmarkTicketDialog(Window owner, SceneProp scene, string srcVideoPath, float fps)
        {
            InitializeComponent();
            _Bind = new Bind();
            this.DataContext = _Bind;
            _Bind.StdPoints = new ObservableCollection<Landmarks>();
            _Bind.Points = new ObservableCollection<Landmarks>();

            this.videoPath = srcVideoPath;
            this.fps = fps;
            this.scene = scene;

            string jsonText = File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "3rd", "face_mesh_landmarks.json"));
            List<Point3d>? points = JsonConvert.DeserializeObject<List<Point3d>>(jsonText);

            baseLandpack = new Landmarks(points);
            baseLandpack.Strip(50);

            foreach (var lpoint in baseLandpack.Points)
            {
                Ellipse circle = lpoint.SetCircle(4, Brushes.White, Brushes.Gray);
                canvas.Children.Add(circle);
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point _currentPoint = e.GetPosition(canvas);

            if (postPoint == _currentPoint)
            {
                return;
            }

            canvas.Children.Remove(mouseoverPoint.Circle);
            canvas.Children.Remove(mouseoverPoint.Text);

            foreach (LandPoint lpoint in baseLandpack.Points)
            {
                if ((_currentPoint.X > (lpoint.X - _mouseOverThreshold)) &&
                    (_currentPoint.X < (lpoint.X + _mouseOverThreshold)) &&
                    (_currentPoint.Y > (lpoint.Y - _mouseOverThreshold)) &&
                    (_currentPoint.Y < (lpoint.Y + _mouseOverThreshold)))
                {
                    mouseoverPoint.SetPoint(lpoint);

                    canvas.Children.Add(mouseoverPoint.Circle);
                    canvas.Children.Add(mouseoverPoint.Text);
                    break;
                }
            }
            postPoint = _currentPoint;
        }

        private void Canvas_MouseClick(object sender, MouseButtonEventArgs e)
        {
            if (mouseoverPoint.Point == null)
            {
                return;
            }
            canvas.Children.Remove(landmarkSelectedCircle);
            landmarkSelectedCircle = mouseoverPoint.Point.SetCircle(6, Brushes.Blue, Brushes.Black);
            canvas.Children.Add(landmarkSelectedCircle);

            canvas.Children.Remove(selectedPoints.SelectedPolygon);
            selectedPoints.SetPoint(mouseoverPoint.Point);
            canvas.Children.Add(selectedPoints.SelectedPolygon);
        }

        private void PointsRow_Click(object sender, EventArgs e)
        {
            if (this.pointsGrid.CurrentColumn == null)
            {
                return;
            }
            Landmarks selectedRow = (Landmarks)this.pointsGrid.SelectedItem;

            canvas.Children.Remove(selectedPointsOnGrid.SelectedPolygon);
            selectedPointsOnGrid.UpdateSelectedPoints(selectedRow);
            canvas.Children.Add(selectedPointsOnGrid.SelectedPolygon);
        }
        private void StdPointsRow_Click(object sender, EventArgs e)
        {
            if (this.stdPointsGrid.CurrentColumn == null)
            {
                return;
            }
            Landmarks selectedRow = (Landmarks)this.stdPointsGrid.SelectedItem;

            canvas.Children.Remove(selectedPointsOnGrid.SelectedPolygon);
            selectedPointsOnGrid.UpdateSelectedPoints(selectedRow);
            canvas.Children.Add(selectedPointsOnGrid.SelectedPolygon);
        }
        private void AppendButton_Click(object sender, EventArgs e)
        {
            Landmarks newLandarea = new(selectedPoints.Points);
            Landmarks selectedRow = new();
            
            var button = (System.Windows.Controls.Button)sender;
            if (button.Name == "standardButton")
            {
                _Bind.StdPoints.Add(newLandarea);
                _Bind.StdPoints = new ObservableCollection<Landmarks>(_Bind.StdPoints.OrderBy(n => n.NumOfPoints));
                this.stdPointsGrid.SelectedItem = newLandarea;
                selectedRow = (Landmarks)this.stdPointsGrid.SelectedItem;
            }
            else if(button.Name == "appendButton")
            {
                _Bind.Points.Add(newLandarea);
                _Bind.Points = new ObservableCollection<Landmarks>(_Bind.Points.OrderBy(n => n.NumOfPoints));
                this.pointsGrid.SelectedItem = newLandarea;
                selectedRow = (Landmarks)this.pointsGrid.SelectedItem;
            }

            canvas.Children.Remove(selectedPoints.SelectedPolygon);
            selectedPoints.Clear();

            canvas.Children.Remove(selectedPointsOnGrid.SelectedPolygon);
            selectedPointsOnGrid.UpdateSelectedPoints(selectedRow);
            canvas.Children.Add(selectedPointsOnGrid.SelectedPolygon);
        }

        // 右クリック操作
        private void PointsMenu_Opening(object sender, DependencyPropertyChangedEventArgs e)
        {
            var selectedRow = this.pointsGrid.SelectedItem;
            if (selectedRow == null)
            {
                removeStdPoints.IsEnabled = false;
                removePoints.IsEnabled = false;
            }
            else
            {
                removeStdPoints.IsEnabled = true;
                removePoints.IsEnabled = true;
            }
        }

        private void RemoveStdPoints_Click(object sender, RoutedEventArgs e)
        {
            int idx = this.stdPointsGrid.SelectedIndex;

            if (idx >= _Bind.StdPoints.Count || idx < 0)
            {
                return;
            }
            _Bind.StdPoints.RemoveAt(idx);
        }
        private void RemovePoints_Click(object sender, RoutedEventArgs e)
        {
            int idx = this.pointsGrid.SelectedIndex;

            if (idx >= _Bind.Points.Count || idx < 0)
            {
                return;
            }
            _Bind.Points.RemoveAt(idx);
        }
        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            string targetFolderPath = System.IO.Path.GetDirectoryName(this.videoPath);
            string targetFilePath = "";
            var dialog = new CommonOpenFileDialog()
            {
                Title = "ファイルを選択してください",
                InitialDirectory = targetFolderPath,
            };
            dialog.Filters.Add(new CommonFileDialogFilter("Json files", ".json"));

            // ダイアログを表示
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                    targetFilePath = dialog.FileName;
            }
            this.Topmost = true;
            this.Topmost = false;

            LandmarkCalcJson import = new();

            using(var sr = new StreamReader(targetFilePath))
            {
                var jsonData = sr.ReadToEnd();
                import = JsonConvert.DeserializeObject<LandmarkCalcJson>(jsonData);
            }

            // 標準領域を追加
            foreach(List<string> landareas in import.StandardLandarea)
            {
                Landmarks landarea = new();
                foreach(string pointName in landareas)
                {
                    LandPoint point = baseLandpack.FindByName(pointName);
                    landarea.Points.Add(point);
                }
                _Bind.StdPoints.Add(landarea);
            }

            // 分析領域を追加
            foreach (List<string> landareas in import.Landarea)
            {
                Landmarks landarea = new();
                foreach (string pointName in landareas)
                {
                    LandPoint point = baseLandpack.FindByName(pointName);
                    landarea.Points.Add(point);
                }
                _Bind.Points.Add(landarea);
            }
        }
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {

            List<List<string>> landareaStr = new();
            foreach(var landarea in _Bind.Points)
            {
                List<string> points = new();
                foreach(var point in landarea.Points)
                {
                    points.Add(point.Name);
                }
                landareaStr.Add(points);
            }
            List<List<string>> stdLandareaStr = new();
            foreach (var landarea in _Bind.StdPoints)
            {
                List<string> points = new();
                foreach (var point in landarea.Points)
                {
                    points.Add(point.Name);
                }
                stdLandareaStr.Add(points);
            }

            LandmarkCalcJson export = new()
            {
                VideoPath = this.videoPath,
                Fps = this.fps,
                StartTimeStr = scene.StartTimeStr,
                EndTimeStr = scene.EndTimeStr,
                LandmarkType="mediapipe_face_landmarker",
                StandardLandarea = stdLandareaStr,
                Landarea = landareaStr,
            };

            if (string.IsNullOrEmpty(dstFolderPath) == true)
            {
                dstFolderPath = System.IO.Path.GetDirectoryName(this.videoPath);
            }

            string dstJsonFileName = System.IO.Path.GetFileNameWithoutExtension(this.videoPath);
            var dialog = new CommonSaveFileDialog()
            {
                Title = "出力先を選択してください",
                DefaultDirectory = dstFolderPath,
                DefaultFileName = string.Concat(dstJsonFileName, "_", scene.Title, "_landmarks.json"),
            };
            dialog.Filters.Add(new CommonFileDialogFilter("Json files", ".json"));

            // ダイアログを表示
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                dstFilePath = dialog.FileName;
            }
            this.Topmost = true;
            this.Topmost = false;

            export.SerializeToFile(dstFilePath);
        }
        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
