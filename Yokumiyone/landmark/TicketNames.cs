using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Yokumiyone.landmark
{
    internal class TicketNames
    {
        private ComboBox ticketNameCbx = new();
        public string TicketNameSelected
        {
            get
            {
                var ticket = (TicketNameCbxItem)ticketNameCbx.SelectedItem;
                return ticket.Name;
            }
        }
        public void SetControls(ComboBox ticketNameCbx, List<string> ticketNames)
        {
            this.ticketNameCbx = ticketNameCbx;

            foreach (string name in ticketNames)
            {
                this.ticketNameCbx.Items.Add(new TicketNameCbxItem(name));
            }

            this.ticketNameCbx.SelectedIndex = 0;
        }
        public void AddTicketName(string newTicketName)
        {
            if (String.IsNullOrEmpty(newTicketName.Trim()) == false)
            {
                this.ticketNameCbx.Items.Insert(0, new TicketNameCbxItem(newTicketName));
                this.ticketNameCbx.SelectedIndex = 0;
            }
        }

        // コンボボックスの中身
        public class TicketNameCbxItem
        {
            public string Name { get; set; }
            public TicketNameCbxItem(string name)
            {
                Name = name;
            }
        }
    }
}
