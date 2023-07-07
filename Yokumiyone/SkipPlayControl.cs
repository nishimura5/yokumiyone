using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Yokumiyone
{
    class SkipPlayControl
    {
        private ComboBox skipCbx = new();
        private ComboBox playCbx = new();
        private ToggleButton isSkip = new();

        public bool? IsSkipMode
        {
            get { return isSkip.IsChecked; }
            set { isSkip.IsChecked = value; }
        }

        private ObservableCollection<SecCbxItem> SkipCombos
        {
            get
            {
                return
                new ObservableCollection<SecCbxItem> {
                new SecCbxItem(10, "10秒"),
                new SecCbxItem(30, "30秒"),
                new SecCbxItem(60, "1分"),
                new SecCbxItem(120, "2分"),
                new SecCbxItem(180, "3分"),
                new SecCbxItem(300, "5分"),
                };
            }
        }
        private ObservableCollection<SecCbxItem> PlayCombos
        {
            get
            {
                return
                new ObservableCollection<SecCbxItem> {
                new SecCbxItem(1, "1秒"),
                new SecCbxItem(2, "2秒"),
                new SecCbxItem(3, "3秒"),
                new SecCbxItem(4, "4秒"),
                new SecCbxItem(5, "5秒"),
                };
            }
        }

        public int SkipSec
        {
            get
            {
                var skip = (SecCbxItem)skipCbx.SelectedItem;
                return skip.Sec;
            }
        }
        public int PlaySec
        {
            get
            {
                var play = (SecCbxItem)playCbx.SelectedItem;
                return play.Sec;
            }
        }
        public void SetControls(ComboBox skip, ComboBox play, ToggleButton isSkip)
        {
            skipCbx = skip;
            playCbx = play;
            this.isSkip = isSkip;
            skipCbx.ItemsSource = SkipCombos;
            playCbx.ItemsSource = PlayCombos;
            skipCbx.SelectedIndex = 0;
            playCbx.SelectedIndex = 0;
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
