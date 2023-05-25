using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Yokumiyone
{
    internal class SceneTitles
    {
        private TextBox sceneTitleBox = new TextBox();
        public string Title
        {
            get { return sceneTitleBox.Text; }
            set { sceneTitleBox.Text = value; }
        }

        private ComboBox sceneTitleCombo = new ComboBox();
        public string SceneTitleSelected
        {
            get
            {
                var scene = (SceneTitleCombo)sceneTitleCombo.SelectedItem;
                return scene.Name;
            }
        }

        public void SetControls(TextBox titleTextBox, ComboBox sceneTitles, string currentTitle, List<string> sceneTitleList)
        {
            sceneTitleBox = titleTextBox;
            sceneTitleBox.Focus();
            sceneTitleBox.SelectAll();

            ObservableCollection<SceneTitleCombo> SceneTitleCombos = new ObservableCollection<SceneTitleCombo>();

            // 今のタイトルがリストの中にあったらそれをselectedに、なかったらComboboxの先頭に追加
            int idx = sceneTitleList.IndexOf(currentTitle);
            if (idx < 0)
            {
                SceneTitleCombos.Add(new SceneTitleCombo(currentTitle));
                idx = 0;
            }

            foreach (string SceneTitle in sceneTitleList)
            {
                SceneTitleCombos.Add(new SceneTitleCombo(SceneTitle));
            }

            sceneTitleCombo = sceneTitles;
            sceneTitleCombo.ItemsSource = SceneTitleCombos;
            sceneTitleCombo.SelectedIndex = idx;
        }

        public class SceneTitleCombo
        {
            public string Name { get; set; }
            public SceneTitleCombo(string name)
            {
                Name = name;
            }
        }
    }
}

