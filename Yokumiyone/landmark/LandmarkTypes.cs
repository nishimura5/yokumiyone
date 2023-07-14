using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static Yokumiyone.SceneTitles;

namespace Yokumiyone.landmark
{
    public class LandmarkTypes
    {
        private TextBox ticketNameTextBox = new();
        public string TicketName
        {
            get { return ticketNameTextBox.Text; }
            set { ticketNameTextBox.Text = value; }
        }
        private ComboBox landmarkTypeCbx = new();
        public string LandmarkType
        {
            get
            {
                var scene = (LandmarkTypeCbxItem)landmarkTypeCbx.SelectedItem;
                return scene.Name;
            }
        }

        public void SetControls(TextBox ticketNameTextBox, ComboBox landmarkTypeCbx, string currentLandmarkType, List<string> landmarkTypes)
        {
            this.ticketNameTextBox = ticketNameTextBox;
            this.ticketNameTextBox.Focus();
            this.ticketNameTextBox.SelectAll();

            // 今のタイトルがリストの中にあったらそれをselectedに、なかったらComboboxの先頭に追加
            int idx = landmarkTypes.IndexOf(currentLandmarkType);
            if (idx < 0)
            {
                idx = 0;
            }
            ObservableCollection<LandmarkTypeCbxItem> LandmarkTypes = new();
            foreach (string landmarkType in landmarkTypes)
            {
                LandmarkTypes.Add(new LandmarkTypeCbxItem(landmarkType));
            }

            this.landmarkTypeCbx = landmarkTypeCbx;
            this.landmarkTypeCbx.ItemsSource = LandmarkTypes;
            this.landmarkTypeCbx.SelectedIndex = idx;
        }

        // コンボボックスの中身
        public class LandmarkTypeCbxItem
        {
            public string Name { get; set; }
            public LandmarkTypeCbxItem(string name)
            {
                Name = name;
            }
        }
    }
}
