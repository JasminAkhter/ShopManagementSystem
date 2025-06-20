using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShopManagementSystem
{
    public partial class AddMasterDetailsForm : Form
    {
        string currentFile = string.Empty;
        List<SaleInfo> details = new List<SaleInfo>();

        public AddMasterDetailsForm()
        {
            InitializeComponent();
        }


        #region btnBrowse_Click
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (openFileDialog2.ShowDialog() == DialogResult.OK) 
            {
                currentFile = openFileDialog2.FileName;
                pictureBox1.Image = Image.FromFile(currentFile);
            }
        }
        #endregion


        #region btnAd_Click
        private void btnAdd_Click(object sender, EventArgs e)
        {
            // Validation
            if (comboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a product!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (numQuantity.Value <= 0)
            {
                MessageBox.Show("Quantity must be greater than 0!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (numUnitPrice.Value <= 0)
            {
                MessageBox.Show("Unit price must be greater than 0!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Retrieve selected ProductId from comboBox1
            int productId = Convert.ToInt32(comboBox1.SelectedValue); 

            // Retrieve quantity and unit price
            int quantity = (int)numQuantity.Value;
            decimal unitPrice = numUnitPrice.Value;

            // Calculate total price
            decimal totalPrice = quantity * unitPrice;

            // Add the sale to the details list 
            details.Add(new SaleInfo
            {
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = unitPrice,
                TotalPrice = totalPrice
            });

            // Bind updated details list to DataGridView
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = details;

            // Clear input fields for the next sale
            comboBox1.SelectedIndex = -1; // Deselect the product
            numQuantity.Value = numQuantity.Minimum;
            numUnitPrice.Value = numUnitPrice.Minimum;
        }
        #endregion


        #region btnSaveAll_Click
        private void btnSaveAll_Click(object sender, EventArgs e)
        {
            // Validate customer name
            if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
            {
                MessageBox.Show("Please input customer name!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Validate picture selection
            if (string.IsNullOrWhiteSpace(currentFile))
            {
                MessageBox.Show("Please select a file!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (details.Count == 0)
            {
                MessageBox.Show("Please add at least one sales!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
            {
                con.Open();
                using (SqlTransaction trx = con.BeginTransaction())
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.Transaction = trx;

                        // Save customer picture
                        string ext = Path.GetExtension(currentFile);
                        string f = Path.GetFileNameWithoutExtension(DateTime.Now.Ticks.ToString()) + ext;
                        string savePath = @"..\..\Pictured\" + f;
                        MemoryStream ms = new MemoryStream(File.ReadAllBytes(currentFile));
                        byte[] bytes = ms.ToArray();
                        FileStream fs = new FileStream(savePath, FileMode.Create);
                        fs.Write(bytes, 0, bytes.Length);
                        fs.Close();

                        // Insert customer data and get CustomerId
                        cmd.CommandText = "INSERT INTO Customer(CustomerName,PurchaseDate,IsMember,Picture) VALUES(@cn,@pd,@ism,@pic); SELECT SCOPE_IDENTITY()";
                        cmd.Parameters.AddWithValue("@cn", txtCustomerName.Text);
                        cmd.Parameters.AddWithValue("@pd", txtPurchaseDate.Value);
                        cmd.Parameters.AddWithValue("@ism", checkBox1.Checked);
                        cmd.Parameters.AddWithValue("@pic", f);
                        var customerId = cmd.ExecuteScalar();

                        // Insert sale data for each product
                        foreach (var p in details)
                        {
                            // Calculate total price for each product
                            decimal totalPrice = p.Quantity * p.UnitPrice;

                            // Insert into Sales table
                            cmd.CommandText = "INSERT INTO Sales(CustomerId, ProductId, Quantity, UnitPrice, TotalPrice) VALUES(@cid, @pid, @qty, @up, @tp)";
                            cmd.Parameters.Clear(); // Clear the previous parameters
                            cmd.Parameters.AddWithValue("@cid", customerId); 
                            cmd.Parameters.AddWithValue("@pid", p.ProductId); 
                            cmd.Parameters.AddWithValue("@qty", p.Quantity);  
                            cmd.Parameters.AddWithValue("@up", p.UnitPrice); 
                            cmd.Parameters.AddWithValue("@tp", totalPrice);
                            cmd.ExecuteNonQuery(); 
                        }

                        trx.Commit();
                        MessageBox.Show("Data Saved successfully!!");
                        ResetFields();
                        details.Clear();
                        dataGridView1.DataSource = null;
                    }
                }
                con.Close();
            }
        }
        #endregion


        #region dataGridView1_CellContentClick
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        #endregion


        #region ResetFields
        private void ResetFields()
        {
            txtCustomerName.Text = string.Empty;
            txtPurchaseDate.Value = DateTime.Now;
            checkBox1.Checked = false;
            currentFile = string.Empty;

            string defaultImagePath = Path.Combine(@"..\..\", "Pictured", "no-camera-icon-vector.jpg");

            if (File.Exists(defaultImagePath))
            {
                pictureBox1.Image = Image.FromFile(defaultImagePath);
            }
            else
            {
                MessageBox.Show("Default image not found! Path: " + defaultImagePath);
            }
        }
        #endregion


        #region LoadProducts
        private void LoadProducts()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
            {
                using (SqlDataAdapter sda = new SqlDataAdapter("SELECT ProductId, ProductName FROM Products", con))
                {
                    DataSet ds = new DataSet();
                    sda.Fill(ds);
                    comboBox1.DataSource = ds.Tables[0];
                    comboBox1.DisplayMember = "ProductName";
                    comboBox1.ValueMember = "ProductId";
                }
            }
        }
        #endregion


        #region AddMasterDetailsForm_Load
        private void AddMasterDetailsForm_Load(object sender, EventArgs e)
        {
            LoadProducts();
        }
        #endregion
    }
}
