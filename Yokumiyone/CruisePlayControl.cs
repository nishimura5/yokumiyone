using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls.Primitives;

namespace Yokumiyone
{
    internal class CruisePlayControl
    {
        private List<SceneProp> sceneProps = new List<SceneProp>();
        public ToggleButton isCruise = new ToggleButton();
        private ToggleButton isFast = new ToggleButton();

        public bool? IsCruiseMode { get { return isCruise.IsChecked; } }
        public bool? IsFastMode
        {
            get { return isFast.IsChecked; }
            set { isFast.IsChecked = value; }
        }
        public void SetControls(ToggleButton isCruise, ToggleButton isFast)
        {
            this.isCruise = isCruise;
            this.isFast = isFast;
        }
        public void SetScenes(ObservableCollection<SceneProp> sceneProps)
        {
            this.sceneProps = sceneProps.ToList();
            SwitchCruiseEnable();
        }

        public void SwitchCruiseEnable()
        {
            if (this.sceneProps.Count == 0)
            {
                isCruise.IsChecked = false;
                isCruise.IsEnabled = false;
            }
            else
            {
                isCruise.IsEnabled = true;
            }
        }

        public bool IsInScene(TimeSpan now)
        {
            bool ret = false;
            foreach (SceneProp sceneProp in sceneProps)
            {
                if (now > sceneProp.StartTime && now < sceneProp.EndTime)
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }
        public TimeSpan NextScene(TimeSpan now)
        {
            if (sceneProps.Count == 0)
            {
                return new TimeSpan(0, 0, 0);
            }
            List<TimeSpan> starts = new List<TimeSpan>();
            foreach (SceneProp sceneProp in sceneProps)
            {
                starts.Add(sceneProp.StartTime);
            }
            starts.Sort();
            TimeSpan ret = starts[0];

            foreach (TimeSpan nextStart in starts)
            {
                if (now > nextStart)
                {
                    continue;
                }
                ret = nextStart;
                break;
            }

            return (TimeSpan)ret;
        }
    }
}
