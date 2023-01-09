using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Yokumiyone
{
    /// <summary>
    /// TitleEditDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class TitleEditDialog : Window
    {
        private SceneTitles ctrl = new SceneTitles();
        public string SceneTitle { get { return ctrl.Title; } }

        public TitleEditDialog(Window owner, string title)
        {
            InitializeComponent();
            this.SizeToContent = SizeToContent.Height;

            this.Owner = owner;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            ScenePathTable db = new ScenePathTable();
            db.CleanRows();
            List<string> Scenes = db.GetScenes();

            ctrl.SetControls(titleText, sceneTitles, title, Scenes);
        }

        private void SceneTitle_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ctrl.Title = ctrl.SceneTitleSelected;
        }

        private void titleOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
        private void titleCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
