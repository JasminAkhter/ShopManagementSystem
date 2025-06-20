using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShopManagementSystem
{
    public partial class HomeForm : Form
    {
        public HomeForm()
        {
            InitializeComponent();
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddMasterDetailsForm addMasterDetailsForm = new AddMasterDetailsForm();
            addMasterDetailsForm.Show();
        }

        private void editDeleteShowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditMasterDetailsForm editMasterDetailsForm = new EditMasterDetailsForm();
            editMasterDetailsForm.Show();
        }

        private void addToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AddMember addMember = new AddMember();
            addMember.Show();
        }

        private void editDeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowEditMember showEditMember = new ShowEditMember();
            showEditMember.Show();
        }

        private void CRUDMenuTab_Click(object sender, EventArgs e)
        {
            Product product = new Product();
            product.Show();
        }
    }
}
