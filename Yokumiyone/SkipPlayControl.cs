using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Yokumiyone
{
    class SkipPlayControl
    {
        private ComboBox skipCombo = new ComboBox();
        private ComboBox playCombo = new ComboBox();
        private ToggleButton isSkip = new ToggleButton();

        public bool? IsSkipMode { 
            get { return isSkip.IsChecked; }
            set { isSkip.IsChecked = value; }
        }

        private ObservableCollection<SecCombo> SkipCombos
        {
            get
            {
                return
                new ObservableCollection<SecCombo> {
                new SecCombo(10, "10秒"),
                new SecCombo(30, "30秒"),
                new SecCombo(60, "1分"),
                new SecCombo(120, "2分"),
                new SecCombo(180, "3分"),
                new SecCombo(300, "5分"),
                };
            }
        }
        private ObservableCollection<SecCombo> PlayCombos
        {
            get
            {
                return
                new ObservableCollection<SecCombo> {
                new SecCombo(1, "1秒"),
                new SecCombo(2, "2秒"),
                new SecCombo(3, "3秒"),
                new SecCombo(4, "4秒"),
                new SecCombo(5, "5秒"),
                };
            }
        }

        public int SkipSec
        {
            get
            {
                var skip = (SecCombo)skipCombo.SelectedItem;
                return skip.Sec;
            }
        }
        public int PlaySec
        {
            get
            {
                var play = (SecCombo)playCombo.SelectedItem;
                return play.Sec;
            }
        }

        public void SetControls(ComboBox skip, ComboBox play, ToggleButton isSkip) {
            skipCombo = skip;
            playCombo = play;
            this.isSkip = isSkip;
            skipCombo.ItemsSource = SkipCombos;
            playCombo.ItemsSource = PlayCombos;
            skipCombo.SelectedIndex = 0;
            playCombo.SelectedIndex = 0;
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
