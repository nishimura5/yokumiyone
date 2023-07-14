using System.Windows;

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
