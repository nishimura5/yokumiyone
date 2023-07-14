using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Yokumiyone.landmark
{
    internal class Landareas
    {
        private List<Landmarks> standardLandmarks = new();
        private List<Landmarks> targetLandmarks = new();

        public List<Landmarks> StandardLandmarks { get { return standardLandmarks; } }
        public List<Landmarks> TargetLandmarks { get { return targetLandmarks; } }
        public string Name { get; set; }
        private string landmarkType = "";
        private Landmarks baseLandpack = new();

        public Landareas(string landmarkType, Landmarks baseLandpack)
        {
            this.landmarkType = landmarkType;
            this.baseLandpack = baseLandpack;
        }

        public Dictionary<string, List<string>> GetStandardLandareaAsPointNames()
        {
            Dictionary<string, List<string>> retVal = new();
            foreach(Landmarks landmarks in standardLandmarks)
            {
                retVal[landmarks.Name] = landmarks.GetPointNames();
            }
            return retVal;
        }
        public Dictionary<string, List<string>> GetTargetLandareaAsPointNames()
        {
            Dictionary<string, List<string>> retVal = new();
            foreach (Landmarks landmarks in targetLandmarks)
            {
                retVal[landmarks.Name] = landmarks.GetPointNames();
            }
            return retVal;
        }
        public void AddStandardLandarea(Landmarks landarea)
        {
            standardLandmarks.Add(landarea);
        }
        public void AddTargetLandarea(Landmarks landarea)
        {
            targetLandmarks.Add(landarea);
        }
        public void AddStandardLandareaByPointNames(string landareaName, List<string> pointNames)
        {
            Landmarks points = new(landareaName);
            foreach (string pointName in pointNames)
            {
                LandPoint point = baseLandpack.FindByName(pointName);
                points.Add(point);
            }
            standardLandmarks.Add(points);
        }
        public void AddTargetLandareaByPointNames(string landareaName, List<string> pointNames)
        {
            Landmarks points = new(landareaName);
            foreach (string pointName in pointNames)
            {
                LandPoint point = baseLandpack.FindByName(pointName);
                points.Add(point);
            }
            targetLandmarks.Add(points);
        }
    }
}
