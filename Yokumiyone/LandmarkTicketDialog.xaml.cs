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

            private ObservableCollection<Landmark> _StdPoints;
            public ObservableCollection<Landmark> StdPoints
            {
                get { return _StdPoints; }
                set { _StdPoints = value; OnPropertyChanged("StdPoints"); }
            }

            private ObservableCollection<Landmark> _Points;
            public ObservableCollection<Landmark> Points
            {
                get { return _Points; }
                set { _Points = value; OnPropertyChanged("Points"); }
            }
        }
        internal Bind _Bind;
        #endregion

        // mouse overした点
        MouseoverPoint mouseoverPoint = new MouseoverPoint();
        Point postPoint = new Point();

        Ellipse landmarkSelectedCircle = new Ellipse();
        Landmark baseLandmarks = new Landmark();
        SelectedPoints selectedPoints = new SelectedPoints(60, 60, 230);
        SelectedPoints selectedPointsOnGrid = new SelectedPoints(60, 230, 60);

        string videoPath = "";
        float fps=1;
        private string? dstFolderPath;
        string dstFilePath = "";
        SceneProp scene;
        private int _mouseOverThreshold = 5;


        public LandmarkTicketDialog(Window owner, SceneProp scene, string srcVideoPath, float fps)
        {
            InitializeComponent();
            _Bind = new Bind();
            this.DataContext = _Bind;
            _Bind.StdPoints = new ObservableCollection<Landmark>();
            _Bind.Points = new ObservableCollection<Landmark>();

            this.videoPath = srcVideoPath;
            this.fps = fps;
            this.scene = scene;

            string jsonText = File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "3rd", "face_mesh_landmarks.json"));
            List<Point3d>? points = JsonConvert.DeserializeObject<List<Point3d>>(jsonText);

            baseLandmarks = new Landmark(points, 800);
            baseLandmarks.Strip(50);

            foreach (var lpoint in baseLandmarks.Points)
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

            foreach (LandPoint lpoint in baseLandmarks.Points)
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
            Landmark selectedRow = (Landmark)this.pointsGrid.SelectedItem;

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
            Landmark selectedRow = (Landmark)this.stdPointsGrid.SelectedItem;

            canvas.Children.Remove(selectedPointsOnGrid.SelectedPolygon);
            selectedPointsOnGrid.UpdateSelectedPoints(selectedRow);
            canvas.Children.Add(selectedPointsOnGrid.SelectedPolygon);
        }
        private void StandardButton_Click(object sender, EventArgs e)
        {
            Landmark newPoints = new Landmark(selectedPoints.Points);
            _Bind.StdPoints.Add(newPoints);
            _Bind.StdPoints = new ObservableCollection<Landmark>(_Bind.StdPoints.OrderBy(n => n.PointNum));

            canvas.Children.Remove(selectedPoints.SelectedPolygon);
            selectedPoints.Clear();

            this.stdPointsGrid.SelectedItem = newPoints;
            Landmark selectedRow = (Landmark)this.stdPointsGrid.SelectedItem;

            canvas.Children.Remove(selectedPointsOnGrid.SelectedPolygon);
            selectedPointsOnGrid.UpdateSelectedPoints(selectedRow);
            canvas.Children.Add(selectedPointsOnGrid.SelectedPolygon);
        }

        private void AppendButton_Click(object sender, RoutedEventArgs e)
        {
            Landmark newPoints = new Landmark(selectedPoints.Points);
            _Bind.Points.Add(newPoints);
            _Bind.Points = new ObservableCollection<Landmark>(_Bind.Points.OrderBy(n => n.PointNum));

            canvas.Children.Remove(selectedPoints.SelectedPolygon);
            selectedPoints.Clear();

            this.pointsGrid.SelectedItem = newPoints;
            Landmark selectedRow = (Landmark)this.pointsGrid.SelectedItem;

            canvas.Children.Remove(selectedPointsOnGrid.SelectedPolygon);
            selectedPointsOnGrid.UpdateSelectedPoints(selectedRow);
            canvas.Children.Add(selectedPointsOnGrid.SelectedPolygon);
        }

        // 右クリック操作
        private void StdPointsMenu_Opening(object sender, DependencyPropertyChangedEventArgs e)
        {
            var selectedRow = this.pointsGrid.SelectedItem;
            if (selectedRow == null)
            {
                removeStdPoints.IsEnabled = false;
            }
            else
            {
                removeStdPoints.IsEnabled = true;
            }
        }
        private void PointsMenu_Opening(object sender, DependencyPropertyChangedEventArgs e)
        {
            var selectedRow = this.pointsGrid.SelectedItem;
            if (selectedRow == null)
            {
                removePoints.IsEnabled = false;
            }
            else
            {
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
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {

            List<List<string>> landmarksStr = new List<List<string>>();
            foreach(var landmarks in _Bind.Points)
            {
                List<string> points = new List<string>();
                foreach(var point in landmarks.Points)
                {
                    points.Add(point.Name);
                }
                landmarksStr.Add(points);
            }
            List<List<string>> stdLandmarksStr = new List<List<string>>();
            foreach (var landmarks in _Bind.StdPoints)
            {
                List<string> points = new List<string>();
                foreach (var point in landmarks.Points)
                {
                    points.Add(point.Name);
                }
                stdLandmarksStr.Add(points);
            }

            LandmarkCalcJson export = new LandmarkCalcJson
            {
                videoPath = this.videoPath,
                fps = this.fps,
                startTimeStr = scene.StartTimeStr,
                endTimeStr = scene.EndTimeStr,
                standardLandmarks = stdLandmarksStr,
                landmarks = landmarksStr,
            };

            if (string.IsNullOrEmpty(dstFolderPath) == true)
            {
                dstFolderPath = System.IO.Path.GetDirectoryName(this.videoPath);
            }
            var dialog = new CommonSaveFileDialog()
            {
                Title = "出力先を選択してください",
                DefaultDirectory = dstFolderPath,
                DefaultFileName = "landmarks.json",
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
