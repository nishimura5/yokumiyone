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

namespace Yokumiyone.landmark
{
    internal class Landmarks
    {
        private readonly List<LandPoint> landmarks = new();

        public List<LandPoint> Points { get { return landmarks; } }
        public string LandmarkType { get; set; }
        public int NumOfPoints { get { return Points.Count; } }
        public string Name { get; set; }

        public Landmarks()
        {
            Name = string.Empty;
        }
        public Landmarks(string name)
        {
            Name = name;
        }

        public Landmarks(List<LandPoint> landmarks, string name)
        {
            Name = name;
            this.landmarks = landmarks;
        }

        public Landmarks(List<Point3d> points)
        {
            int scale = 1;
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
            LandPoint retPoint = new(name, 0, 0, 1);
            foreach (var point in landmarks)
            {
                if (point.Name == name)
                {
                    retPoint = point;
                    break;
                }
            }
            return retPoint;
        }

        public List<string> GetPointNames()
        {
            return Points.Select(p => p.Name).ToList();
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

            Canvas.SetLeft(circle, X - radius / 2);
            Canvas.SetTop(circle, Y - radius / 2);
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
        private Landmarks selectedPoly = new();
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

        public void UpdateSelectedPoints(Landmarks selectedPoly)
        {
            this.selectedPoly = selectedPoly;
            var mediaColor = Color.FromArgb(127, r, g, b);
            var brush = new SolidColorBrush(mediaColor);
            selectedPolygon = selectedPoly.SetPolygon(brush);
        }

        public void SetPoint(LandPoint point)
        {
            selectedPoly.Add(point);
            var mediaColor = Color.FromArgb(127, r, g, b);
            var brush = new SolidColorBrush(mediaColor);
            selectedPolygon = selectedPoly.SetPolygon(brush);
        }

        public void Clear()
        {
            selectedPoly = new Landmarks();
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
