using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Yokumiyone
{
    internal class SceneTitles
    {
        private TextBox titleTextBox = new();
        public string Title
        {
            get { return titleTextBox.Text; }
            set { titleTextBox.Text = value; }
        }

        private ComboBox titleCbx = new();
        public string SceneTitleSelected
        {
            get
            {
                var scene = (TitleCbxItem)titleCbx.SelectedItem;
                return scene.Name;
            }
        }

        public void SetControls(TextBox titleTextBox, ComboBox sceneTitleCbx, string currentTitle, List<string> sceneTitleList)
        {
            this.titleTextBox = titleTextBox;
            this.titleTextBox.Focus();
            this.titleTextBox.SelectAll();

            ObservableCollection<TitleCbxItem> SceneTitles = new();

            // 今のタイトルがリストの中にあったらそれをselectedに、なかったらComboboxの先頭に追加
            int idx = sceneTitleList.IndexOf(currentTitle);
            if (idx < 0)
            {
                SceneTitles.Add(new TitleCbxItem(currentTitle));
                idx = 0;
            }

            foreach (string SceneTitle in sceneTitleList)
            {
                SceneTitles.Add(new TitleCbxItem(SceneTitle));
            }

            this.titleCbx = sceneTitleCbx;
            this.titleCbx.ItemsSource = SceneTitles;
            this.titleCbx.SelectedIndex = idx;
        }

        // コンボボックスの中身
        public class TitleCbxItem
        {
            public string Name { get; set; }
            public TitleCbxItem(string name)
            {
                Name = name;
            }
        }
    }
}

