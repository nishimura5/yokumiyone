using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Yokumiyone
{
    class TweakSliderManager
    {
        public double NowMs { set; get; }
        private double preMs = 0.0f;
        private Slider TweakSlider = new Slider();
        public ComboBox RangeCombo = new ComboBox();
        private int sliderRange = 500;

        public void SetControls(Slider tweakSlider, ComboBox rangeCombo) {
            TweakSlider = tweakSlider;
            TweakSlider.Maximum = sliderRange;
            TweakSlider.Minimum = -sliderRange;
            TweakSlider.Value = 0;
            RangeCombo = rangeCombo;
            RangeCombo.ItemsSource = RangeCombos;
            RangeCombo.SelectedIndex = 2;
        }

        public void SetPreMs(double msecond) {
            preMs = msecond;
        }

        public double GetPostMs()
        {
            if(TweakSlider == null)
            {
                return 0;
            }
            SecCombo range = (SecCombo)RangeCombo.SelectedItem;
            return preMs + TweakSlider.Value / sliderRange * range.Sec * 1000;
        }
        private ObservableCollection<SecCombo> RangeCombos
        {
            get
            {
                return
                new ObservableCollection<SecCombo> {
                new SecCombo(5, "±5秒"),
                new SecCombo(10, "±10秒"),
                new SecCombo(60, "±1分"),
                new SecCombo(180, "±3分"),
                new SecCombo(300, "±5分"),
                };
            }
        }
        private class SecCombo
        {
            public int Sec { get; set; }
            public string Name { get; set; }
            public SecCombo(int sec, string name)
            {
                Sec = sec;
                Name = name;
            }
        }
    }
}
