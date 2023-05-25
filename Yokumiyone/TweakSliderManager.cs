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
        private Slider tweakSlider = new Slider();
        private ComboBox rangeCbx = new ComboBox();
        private int sliderRange = 500;

        public void SetControls(Slider tweakSlider, ComboBox rangeCbx) {
            this.tweakSlider = tweakSlider;
            this.tweakSlider = tweakSlider;
            this.tweakSlider.Maximum = sliderRange;
            this.tweakSlider.Minimum = -sliderRange;
            this.tweakSlider.Value = 0;
            this.rangeCbx = rangeCbx;
            this.rangeCbx.ItemsSource = RangeCombos;
            this.rangeCbx.SelectedIndex = 2;
        }

        public void SetPreMs(double msecond) {
            preMs = msecond;
        }

        public double GetPostMs()
        {
            if(tweakSlider == null)
            {
                return 0;
            }
            SecCbxItem range = (SecCbxItem)rangeCbx.SelectedItem;
            return preMs + this.tweakSlider.Value / sliderRange * range.Sec * 1000;
        }
        private ObservableCollection<SecCbxItem> RangeCombos
        {
            get
            {
                return
                new ObservableCollection<SecCbxItem> {
                new SecCbxItem(5, "±5秒"),
                new SecCbxItem(10, "±10秒"),
                new SecCbxItem(60, "±1分"),
                new SecCbxItem(180, "±3分"),
                new SecCbxItem(300, "±5分"),
                };
            }
        }
        private class SecCbxItem
        {
            public int Sec { get; set; }
            public string Name { get; set; }
            public SecCbxItem(int sec, string name)
            {
                Sec = sec;
                Name = name;
            }
        }
    }
}
