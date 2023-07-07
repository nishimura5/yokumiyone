using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace Yokumiyone
{
    internal class Landmark
    {
        private readonly List<LandPoint> landmarks = new();

        public List<LandPoint> Points { get { return landmarks; } }
        public string LandmarkType { get; set; }
        public int PointNum { get { return Points.Count; } }
        public string PointsCsv { get { return GenCsv(); } }

        public Landmark() { 
        
        }

        public Landmark(List<LandPoint> landmarks)
        {
            this.landmarks = landmarks;
        }

        public Landmark(List<Point3d> points, double scale)
        {
            int idx = 0;
            foreach (var point in points)
            {
                LandPoint p = new(idx.ToString(), point.X, point.Y, scale);
                landmarks.Add(p);
                idx++;
            }
        }

        public void Add(LandPoint point)
        {
            if (landmarks.Contains(point) == false)
            {
                landmarks.Add(point);
            }
        }

        // 空白を切り詰める
        public void Strip(int padding)
        {
            var minX = landmarks.Select(d => d.X).Min() - padding;
            var minY = landmarks.Select(d => d.Y).Min() - padding;
            foreach (var point in landmarks)
            {
                point.Offset(minX, minY);
            }
        }

        public Polygon SetPolygon(Brush fill)
        {
            PointCollection pointCollection = new();
            foreach (var point in landmarks)
            {
                pointCollection.Add(new Point(point.X, point.Y));
            }
            Polygon polygon = new()
            {
                Points = pointCollection,
                Fill = fill,
                StrokeThickness = 1,
            };
            return polygon;
        }

        public LandPoint FindByName(string name)
        {
            LandPoint retPoint = new(name ,0, 0, 1);
            foreach (var point in landmarks)
            {
                if(point.Name == name)
                {
                    retPoint = point;
                    break;
                }
            }
            return retPoint;
        }

        private string GenCsv()
        {
            string dstCsv = "";
            foreach (var point in Points)
            {
                dstCsv += point.Name + ",";
            }
            return dstCsv;
        }
    }

    class LandPoint
    {
        public string? Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        private readonly double scale = 1;

        public LandPoint(string? name, double x, double y, double scale)
        {
            Name = name;
            X = (int)(x * scale);
            Y = (int)(y * scale);
            this.scale = scale;
        }
        public void Offset(int x, int y)
        {
            X -= x;
            Y -= y;
        }

        public Ellipse SetCircle(int radius, Brush fill, Brush stroke)
        {
            Ellipse circle = new()
            {
                Width = radius,
                Height = radius,
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = 1
            };

            Canvas.SetLeft(circle, X-(int)(radius/2));
            Canvas.SetTop(circle, Y-(int)(radius/2));
            return circle;
        }

        public TextBlock SetText(int fontSize)
        {
            TextBlock text = new()
            {
                FontSize = fontSize,
                Text = Name
            };
            Canvas.SetLeft(text, X);
            Canvas.SetTop(text, Y - 18);
            return text;
        }
    }

    // canvas描画用
    class SelectedPoints
    {
        private Landmark selectedPoly = new();
        private Polygon? selectedPolygon;
        private readonly byte r;
        private readonly byte g;
        private readonly byte b;

        public Polygon? SelectedPolygon { get { return selectedPolygon; } }
        public List<LandPoint> Points { get { return selectedPoly.Points; } }

        public SelectedPoints(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public void UpdateSelectedPoints(Landmark selectedPoly)
        {
            this.selectedPoly = selectedPoly;
            var mediaColor = System.Windows.Media.Color.FromArgb(127, r, g, b);
            var brush = new System.Windows.Media.SolidColorBrush(mediaColor);
            selectedPolygon = selectedPoly.SetPolygon(brush);
        }

        public void SetPoint(LandPoint point)
        {
            selectedPoly.Add(point);
            var mediaColor = System.Windows.Media.Color.FromArgb(127, r, g, b);
            var brush = new System.Windows.Media.SolidColorBrush(mediaColor);
            selectedPolygon = selectedPoly.SetPolygon(brush);
        }

        public void Clear()
        {
            selectedPoly = new Landmark();
            selectedPolygon = null;
        }
    }

    class MouseoverPoint
    {
        private LandPoint? point;
        private TextBlock? text;
        private Ellipse? circle;

        public LandPoint? Point { get { return point; } }
        public TextBlock? Text { get { return text; } }
        public Ellipse? Circle { get { return circle; } }

        public void SetPoint(LandPoint point)
        {
            circle = point.SetCircle(6, Brushes.Red, Brushes.Black);
            text = point.SetText(14);
            this.point = point;
        }
    }

    class Point3d
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }
}
