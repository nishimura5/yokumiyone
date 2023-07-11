using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Yokumiyone.tables;

namespace Yokumiyone
{
    /// <summary>
    /// TitleEditDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class TitleEditDialog : Window
    {
        private SceneTitles ctrl = new();

        private string sceneTitle;
        public string SceneTitle {
            get { return sceneTitle; }
            set { sceneTitle = value; }
        }

        public TitleEditDialog(Window owner, string title)
        {
            InitializeComponent();
            this.DataContext = this;
            this.SizeToContent = SizeToContent.Height;

            this.Owner = owner;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            ScenePathTable db = new();
            db.CleanRows();
            List<string> Scenes = db.GetScenes();

            ctrl.SetControls(titleTextBox, sceneTitles, title, Scenes);
        }

        private void SceneTitle_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ctrl.Title = ctrl.SceneTitleSelected;
            sceneTitle = ctrl.Title;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
