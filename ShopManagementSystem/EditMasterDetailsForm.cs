using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShopManagementSystem
{
    public partial class EditMasterDetailsForm : Form
    {
        BindingSource bsS = new BindingSource();
        BindingSource bsC = new BindingSource();
        DataSet ds = new DataSet();

        public EditMasterDetailsForm()
        {
            InitializeComponent();
        }


        #region LoadDataBindingSource
        public void LoadDataBindingSource()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
            {
                using (SqlDataAdapter sda = new SqlDataAdapter("SELECT * FROM Customer", con))
                {
                    ds = new DataSet();
                    sda.Fill(ds, "Customer");
                    sda.SelectCommand.CommandText = "SELECT * FROM Sales";
                    sda.Fill(ds, "Sales");

                    ds.Tables["Customer"].Columns.Add(new DataColumn("image", typeof(byte[])));
                    for (int i = 0; i < ds.Tables["Customer"].Rows.Count; i++)
                    {
                        ds.Tables["Customer"].Rows[i]["image"] = File.ReadAllBytes($@"..\..\Pictured\{ds.Tables["Customer"].Rows[i]["picture"]}");
                    }

                    DataRelation rel = new DataRelation("FK_S_C", ds.Tables["Customer"].Columns["CustomerId"], ds.Tables["Sales"].Columns["CustomerId"]);
                    ds.Relations.Add(rel);
                    bsS.DataSource = ds;
                    bsS.DataMember = "Customer";

                    bsC.DataSource = bsS;
                    bsC.DataMember = "FK_S_C";
                    dataGridView1.DataSource = bsC;
                    AddDataBindings();
                }
            }
        }
        #endregion


        #region AddDataBindings
        private void AddDataBindings()
        {
            lblId.DataBindings.Clear();
            lblId.DataBindings.Add("Text", bsS, "CustomerId");

            lblName.DataBindings.Clear();
            lblName.DataBindings.Add("Text", bsS, "CustomerName");

            lblPurchaseDate.DataBindings.Clear();
            lblPurchaseDate.DataBindings.Add("Text", bsS, "PurchaseDate");
            Binding bm = new Binding("Text", bsS, "PurchaseDate", true);
            bm.Format += Bm_Format;
            lblPurchaseDate.DataBindings.Clear();
            lblPurchaseDate.DataBindings.Add(bm);

            pictureBox1.DataBindings.Clear();
            pictureBox1.DataBindings.Add(new Binding("Image", bsS, "image", true));

            checkBox1.DataBindings.Clear();
            checkBox1.DataBindings.Add("Checked", bsS, "IsMember", true);
        }
        #endregion



        private void btnEdit_Click(object sender, EventArgs e)
        {
            // Validation
            if (bsS.Current == null)
            {
                MessageBox.Show("No customer selected for edit.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int v = int.Parse((bsS.Current as DataRowView).Row[0].ToString());
            new ViewMasterDetailsForm
            {
                TheForm = this,
                IdToEdit = v
            }.ShowDialog();
        }


        private void Bm_Format(object sender, ConvertEventArgs e)
        {
            DateTime d = (DateTime)e.Value;
            e.Value = d.ToString("dd-MM-yyyy");
        }

        private void EditMasterDetailsForm_Load(object sender, EventArgs e)
        {
            LoadDataBindingSource();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            bsS.MoveFirst();
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            bsS.MoveLast();
        }

        private void btnPre_Click(object sender, EventArgs e)
        {
            if (bsS.Position > 0)
            {
                bsS.MovePrevious();
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (bsS.Position < bsS.Count - 1)
            {
                bsS.MoveNext();
            }
        }
    }
}
