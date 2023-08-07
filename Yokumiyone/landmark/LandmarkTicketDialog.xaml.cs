using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using Yokumiyone.tables;
using Yokumiyone.landmark;
using System.Text.Json;
using System.Diagnostics;

namespace Yokumiyone
{
    /// <summary>
    /// LandmarkTicketDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class LandmarkTicketDialog
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

            public void RemovePoints(int idx)
            {
                if (idx >= _Points.Count || idx < 0)
                {
                    return;
                }
                _Points.RemoveAt(idx);
            }
            public void RemoveStdPoints(int idx)
            {
                if (idx >= _StdPoints.Count || idx < 0)
                {
                    return;
                }
                _StdPoints.RemoveAt(idx);
            }
        }
        internal Bind _Bind;
        #endregion

        // mouse overした点
        readonly MouseoverPoint mouseoverPoint = new();
         Point postPoint = new();

        Ellipse landmarkSelectedCircle = new();
        Landmarks baseLandpack = new();
        readonly SelectedPoints selectedPoints = new(60, 60, 230);
        readonly SelectedPoints selectedPointsOnGrid = new(60, 230, 60);

        readonly string videoPath = "";
        readonly float fps=1;
        private string landmarkType = "";
        private string currentTicketName = "";
        readonly SceneProp scene;
        private readonly int _mouseOverThreshold = 5;
        private TicketNames ctrl = new();

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

            PutLandpack("mediapipe_face_mesh_landmarks", 4);

            LandareaTable landareatable = new();
            List<string> names = landareatable.GetNames();
            ctrl.SetControls(ticketNames, names);

            videoName.Text = System.IO.Path.GetFileName(srcVideoPath);
            sceneTitle.Text = scene.Title;
            startTime.Text = scene.StartTimeStr;
            endTime.Text = scene.EndTimeStr;
        }

        private void PutLandpack(string fileName, int radius)
        {
            canvas.Children.Clear();
            string jsonText = File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "3rd", "landpack", fileName+".json"));
            var options = new JsonSerializerOptions { WriteIndented = true };
            List<Point3d>? points = JsonSerializer.Deserialize<List<Point3d>>(jsonText, options);
            baseLandpack = new Landmarks(points);

            Image img = new();
            BitmapImage bmpImage = new BitmapImage();
            using (FileStream stream = File.OpenRead(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "3rd", "landpack", fileName+".png")))
            {
                bmpImage.BeginInit();
                bmpImage.StreamSource = stream;
                bmpImage.CacheOption = BitmapCacheOption.OnLoad;
                bmpImage.CreateOptions = BitmapCreateOptions.None;
                bmpImage.EndInit();
                bmpImage.Freeze();
            }
            img.Source = bmpImage;
            img.Height = 700;

            canvas.Children.Add(img);
            foreach (var lpoint in baseLandpack.Points)
            {
                Ellipse circle = lpoint.SetCircle(radius, Brushes.White, Brushes.Gray);
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

            var stdNames = _Bind.StdPoints.Select(n=>n.Name);
            var Names = _Bind.Points.Select(n => n.Name);

            string prefix = "Untitled";
            int number = 1;
            string newName = "";
            for (int i = 0;i<100;i++)
            {
                newName = prefix + number.ToString();
                if (stdNames.Contains(newName) || Names.Contains(newName))
                {
                    number ++;
                }
                else
                {
                    break;
                }
            }

            Landmarks newLandarea = new(selectedPoints.Points, newName);
            Landmarks selectedRow = new();
            
            var button = (System.Windows.Controls.Button)sender;
            if (button.Name == "standardButton")
            {
                _Bind.StdPoints.Add(newLandarea);
                _Bind.StdPoints = new ObservableCollection<Landmarks>(_Bind.StdPoints.OrderBy(n => n.Name));
                this.stdPointsGrid.SelectedItem = newLandarea;
                selectedRow = (Landmarks)this.stdPointsGrid.SelectedItem;
            }
            else if(button.Name == "appendButton")
            {
                _Bind.Points.Add(newLandarea);
                _Bind.Points = new ObservableCollection<Landmarks>(_Bind.Points.OrderBy(n => n.Name));
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
        private void LandareaMenu_Opening(object sender, DependencyPropertyChangedEventArgs e)
        {
            var selectedRow = this.pointsGrid.SelectedItem;
            if (selectedRow == null)
            {
                removePoints.IsEnabled = false;
                editName.IsEnabled = false;
            }
            else
            {
                removePoints.IsEnabled = true;
                editName.IsEnabled = true;
            }
        }
        private void StdLandareaMenu_Opening(object sender, DependencyPropertyChangedEventArgs e)
        {
            var selectedRow = this.stdPointsGrid.SelectedItem;
            if (selectedRow == null)
            {
                removeStdPoints.IsEnabled = false;
                editStdName.IsEnabled = false;
            }
            else
            {
                removeStdPoints.IsEnabled = true;
                editStdName.IsEnabled = true;
            }
        }
        private void RemovePoints_Click(object sender, RoutedEventArgs e)
        {
            int idx = this.pointsGrid.SelectedIndex;
            _Bind.RemovePoints(idx);

        }
        private void RemoveStdPoints_Click(Object sender, RoutedEventArgs e)
        {
            int idx = this.stdPointsGrid.SelectedIndex;
            _Bind.RemoveStdPoints(idx);
        }

        private void EditName_Click(object sender, RoutedEventArgs e)
        {
            Landmarks selectedRow = (Landmarks)this.pointsGrid.SelectedItem;
            NameEditDialog nameEditDialog = new(this, selectedRow.Name);
            var res = nameEditDialog.ShowDialog();
            selectedRow.Name = nameEditDialog.LandareaNameText;
            _Bind.Points = new ObservableCollection<Landmarks>(_Bind.Points.OrderBy(n => n.Name));
        }
        private void EditStdName_Click(object sender, RoutedEventArgs e)
        {
            Landmarks selectedRow = (Landmarks)this.stdPointsGrid.SelectedItem;
            NameEditDialog nameEditDialog = new(this, selectedRow.Name);
            var res = nameEditDialog.ShowDialog();
            selectedRow.Name = nameEditDialog.LandareaNameText;
            _Bind.StdPoints = new ObservableCollection<Landmarks>(_Bind.StdPoints.OrderBy(n => n.Name));
        }
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            LandareaTable landareaTable = new();

            List<string> landmarkTypes = landareaTable.GetLandmarkTypes();
            TicketCreateDialog ticketCreateDialog = new(this, landmarkTypes, landmarkType);
            var res = ticketCreateDialog.ShowDialog();
            if (res == true)
            {
                if(landmarkType != ticketCreateDialog.LandmarkType)
                {
                    _Bind.Points.Clear();
                    _Bind.StdPoints.Clear();
                }
                landmarkType  = ticketCreateDialog.LandmarkType;
                if (landmarkType == "mediapipe_face_mesh_landmarker")
                {
                    PutLandpack("mediapipe_face_mesh_landmarks", 4);
                }
                else if (landmarkType == "mediapipe_pose_landmarker")
                {
                    PutLandpack("mediapipe_pose_landmarks", 5);
                }
                currentTicketName = ticketCreateDialog.NewTicketName;
                ctrl.AddTicketName(currentTicketName);
            }
        }
        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            // DBからlandareaを取得
            LandareaTable landareaTable = new();
            currentTicketName = ctrl.TicketNameSelected;
            landareaTable.GetLandarea(currentTicketName);

            if(landmarkType != landareaTable.LandmarkType)
            {
                _Bind.Points.Clear();
                _Bind.StdPoints.Clear();
            }
            landmarkType = landareaTable.LandmarkType;
            if (landmarkType == "mediapipe_face_mesh_landmarker")
            {
                PutLandpack("mediapipe_face_mesh_landmarks", 4);
            }
            else if (landmarkType == "mediapipe_pose_landmarker")
            {
                PutLandpack("mediapipe_pose_landmarks", 8);
            }

            Landareas landAreas = new(landmarkType, baseLandpack);
            // 標準領域を追加
            foreach (KeyValuePair<string, List<string>> landarea in landareaTable.StandardLandarea)
            {
                landAreas.AddStandardLandareaByPointNames(landarea.Key, landarea.Value);
            }
            foreach (Landmarks landarea in landAreas.StandardLandmarks)
            {
                _Bind.StdPoints.Add(landarea);
            }

            // 分析領域を追加
            foreach (KeyValuePair<string, List<string>> landarea in landareaTable.TargetLandarea)
            {
                landAreas.AddTargetLandareaByPointNames(landarea.Key, landarea.Value);
            }
            foreach (Landmarks landarea in landAreas.TargetLandmarks)
            {
                _Bind.Points.Add(landarea);
            }
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Landareaの名前の重複チェック
            var names = _Bind.Points.GroupBy(n => n.Name)
                .Where(g => g.Count() > 1)
                .Select(x => x.Key)
                .ToList();
            var stdNames = _Bind.StdPoints.GroupBy(n => n.Name)
                .Where(g => g.Count() > 1)
                .Select(x => x.Key)
                .ToList();
            if (stdNames.Count > 0 || names.Count > 0)
            {
                return;
            }

            Landareas landAreas = new(landmarkType, baseLandpack);

            // Landareaのデータ詰め替え
            foreach(Landmarks landarea in _Bind.StdPoints)
            {
                landAreas.AddStandardLandarea(landarea);
            }
            Dictionary<string, List<string>> standardLandareaStr = landAreas.GetStandardLandareaAsPointNames();

            foreach (Landmarks landarea in _Bind.Points)
            {
                landAreas.AddTargetLandarea(landarea);
            }
            Dictionary<string, List<string>> targetLandareaStr = landAreas.GetTargetLandareaAsPointNames();

            // DBにlandareaを保存
            LandareaTable landareaTable = new();
            landareaTable.LandmarkType = this.landmarkType;
            landareaTable.StandardLandarea = standardLandareaStr;
            landareaTable.TargetLandarea = targetLandareaStr;
            landareaTable.SetLandarea(currentTicketName);
        }
        private void ClearButton_Click(object sender, EventArgs e)
        {
            _Bind.Points.Clear();
            _Bind.StdPoints.Clear();
        }
        private void ExecPythonButton_Click(object sender, RoutedEventArgs e)
        {
            // 設定の読み込み
            PreferencesTable preferencesTable = new();
            var prefString = preferencesTable.GetStringPreferences();
            string pythonPath = prefString["pythonPath"];
            LandareaTable landareaTable = new();

            var proc = new Process();
            proc.StartInfo.FileName = pythonPath;
            
            string workingDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "3rd", "landpack", "landmark_detector");
            proc.StartInfo.Arguments = $"landmarks_grapher.py --src_video_path \"{videoPath}\" --init_time_min \"{scene.StartTimeStr}\" --init_time_max \"{scene.EndTimeStr}\" --ticket_name \"{currentTicketName}\" --fps \"{fps}\" --db_path \"{landareaTable.DbPath}\"";
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.WorkingDirectory = workingDirectory;
            proc.Start();
//            string rawResults = proc.StandardOutput.ReadToEnd();
//            proc.WaitForExit();
//            proc.Close();
//            proc.Dispose();
        }
    }
}
