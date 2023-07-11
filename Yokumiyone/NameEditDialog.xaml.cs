using System;
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
    /// NameEditDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class NameEditDialog : Window
    {
        private string nameText;
        public string LandareaNameText
        {
            get { return nameText; }
            set { nameText = value; }
        }

        public NameEditDialog(Window owner, string name)
        {
            InitializeComponent();
            this.DataContext = this;
            this.SizeToContent = SizeToContent.Height;

            this.Owner = owner;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            nameText = name;
            this.nameTextBox.Text = nameText;

            this.nameTextBox.Focus();
            this.nameTextBox.SelectAll();
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
