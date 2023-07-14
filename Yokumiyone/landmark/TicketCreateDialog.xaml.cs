using System;
using System.Collections.Generic;
using System.Windows;

namespace Yokumiyone.landmark
{
    /// <summary>
    /// TicketCreateDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class TicketCreateDialog : Window
    {
        private LandmarkTypes ctrl = new();

        public string LandmarkType { get; set; }
        public string NewTicketName { get; set; }
        public TicketCreateDialog(Window owner, List<string> srcLandmarkTypes, string currentLandmarkType)
        {
            InitializeComponent();

            ctrl.SetControls(ticketNameTextBox, landmarkTypes, currentLandmarkType, srcLandmarkTypes);
        }

        public void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(ctrl.TicketName.Trim()) == false)
            {
                LandmarkType = ctrl.LandmarkType;
                NewTicketName = ctrl.TicketName;
                this.DialogResult = true;
            }
            else
            {
                this.DialogResult = false;
            }
        }
        public void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;

        }
    }
}
