﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Yokumiyone
{
    /// <summary>
    /// sceneOutputDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class SceneOutputDialog : Window
    {
        private SceneProp sceneProp = new SceneProp();
        public SceneOutputDialog(Window owner, SceneProp scene, string srcVideoPath)
        {
            InitializeComponent();
        }
        private void ExecOutput_Click(object sender, RoutedEventArgs e)
        {
            return;
        }
    }
}
