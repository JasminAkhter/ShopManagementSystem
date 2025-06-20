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
    public partial class ViewMasterDetailsForm : Form
    {
        string currentFile = string.Empty;
        List<SaleInfo> details = new List<SaleInfo>();
        string oldFile = "";

        public ViewMasterDetailsForm()
        {
            InitializeComponent();
        }


        public EditMasterDetailsForm TheForm { get; set; }
        public int IdToEdit { get; set; }


        #region btnBrowse_Click
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK) // Manually added
            {
                currentFile = openFileDialog1.FileName;
                pictureBox1.Image = Image.FromFile(currentFile);
            }
        }
        #endregion


        #region btnAdd_Click
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


        #region ViewMasterDetailsForm_Load
        private void ViewMasterDetailsForm_Load(object sender, EventArgs e)
        {
            dataGridView1.AutoGenerateColumns = false;
            LoadInForm();
            LoadProducts();
        }
        #endregion


        #region LoadInForm
        private void LoadInForm()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM Customer WHERE CustomerId=@cid", con))
                {
                    cmd.Parameters.AddWithValue("@cid", IdToEdit);
                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        txtCustomerName.Text = dr.GetString(1); 
                        txtPurchaseDate.Value = dr.GetDateTime(2).Date;  
                        checkBox1.Checked = dr.GetBoolean(3);  
                        pictureBox1.Image = Image.FromFile(@"..\..\Pictured\" + dr.GetString(4));  
                        oldFile = dr.GetString(4); 
                    }
                    dr.Close(); 

                    // Get Sale (Product) Data for this Customer
                    cmd.CommandText = "SELECT p.ProductId, p.ProductName, s.Quantity, s.UnitPrice, s.TotalPrice " +
                                      "FROM Sales s INNER JOIN Products p ON s.ProductId = p.ProductId WHERE s.CustomerId=@cid";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@cid", IdToEdit);

                    SqlDataReader dr2 = cmd.ExecuteReader();
                    details.Clear();  
                    while (dr2.Read())
                    {
                        details.Add(new SaleInfo
                        {
                            ProductId = dr2.GetInt32(0), 
                            ProductName = dr2.GetString(1),
                            Quantity = dr2.GetInt32(2), 
                            UnitPrice = dr2.GetDecimal(3), 
                            TotalPrice = dr2.GetDecimal(4)  
                        });
                    }
                    dr2.Close(); 

                    SetDataSource();

                    con.Close(); 
                }
            }
        }
        #endregion


        #region SetDataSource
        private void SetDataSource()
        {
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = details;
        }
        #endregion


        #region dataGridView1_CellContentClick
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 4)
            {
                details.RemoveAt(e.RowIndex);
                dataGridView1.DataSource = null;
                dataGridView1.DataSource = details;
            }
        }
        #endregion


        #region btnUpdate_Click
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
            {
                MessageBox.Show("Please input customer name!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (details.Count == 0)
            {
                MessageBox.Show("Please add at least one product!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                        string f = oldFile;
                        if (currentFile != "")
                        {
                            string ext = Path.GetExtension(currentFile);
                            f = Path.GetFileNameWithoutExtension(DateTime.Now.Ticks.ToString()) + ext;

                            string savePath = @"..\..\Pictured\" + f;
                            MemoryStream ms = new MemoryStream(File.ReadAllBytes(currentFile));
                            byte[] bytes = ms.ToArray();
                            FileStream fs = new FileStream(savePath, FileMode.Create);
                            fs.Write(bytes, 0, bytes.Length);
                            fs.Close();
                        }

                        cmd.CommandText = "UPDATE Customer SET CustomerName=@cn, PurchaseDate=@pd, IsMember=@ism,Picture=@pic WHERE CustomerId=@cid";
                        cmd.Parameters.AddWithValue("@cid", IdToEdit);
                        cmd.Parameters.AddWithValue("@cn", txtCustomerName.Text);
                        cmd.Parameters.AddWithValue("@pd", txtPurchaseDate.Value);
                        cmd.Parameters.AddWithValue("@ism", checkBox1.Checked);
                        cmd.Parameters.AddWithValue("@pic", f);

                        try
                        {
                            cmd.ExecuteNonQuery();
                            cmd.CommandText = "DELETE FROM Products WHERE CustomerId=@cid";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@cid", IdToEdit);
                            cmd.ExecuteNonQuery();
                            foreach (var product in details)
                            {
                                // Validation
                                //if (string.IsNullOrWhiteSpace(product.ProductName))
                                //{
                                //    MessageBox.Show("Please input product name!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                //    trx.Rollback();
                                //    return;
                                //}
                                //if (product.MFGDate > product.EXPDate)
                                //{
                                //    MessageBox.Show("MFG date cannot be later than EXP Date!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                //    trx.Rollback();
                                //    return;
                                //}
                                //if (product.Price <= 0)
                                //{
                                //    MessageBox.Show("Price must be greater than 0!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                //    trx.Rollback();
                                //    return;
                                //}


                                //cmd.CommandText = @"INSERT INTO Products(ProductName,MfgDate,ExpDate,Price,CustomerId) VALUES(@pn,@md,@ed,@p,@cid)";
                                //cmd.Parameters.Clear();
                                //cmd.Parameters.AddWithValue("@pn", product.ProductName);
                                //cmd.Parameters.AddWithValue("@md", product.MFGDate);
                                //cmd.Parameters.AddWithValue("@ed", product.EXPDate);
                                //cmd.Parameters.AddWithValue("@p", product.Price);
                                cmd.Parameters.AddWithValue("@cid", IdToEdit);
                                cmd.ExecuteNonQuery();
                            }
                            trx.Commit();
                            TheForm.LoadDataBindingSource();
                            MessageBox.Show("Data Updated successfully!!");
                            // Reset master and details list
                            ResetFields();
                            details.Clear();

                            // Reset the DataGridView data source
                            dataGridView1.DataSource = null;
                            dataGridView1.Rows.Clear();
                        }
                        catch (Exception)
                        {
                            trx.Rollback();
                        }
                    }
                }
            }
        }
        #endregion


        #region btnDelete_Click
        private void btnDelete_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
            {
                con.Open();
                using (SqlTransaction trx = con.BeginTransaction())
                {
                    string sql = @"DELETE FROM Products WHERE CustomerId=@cid";
                    using (SqlCommand cmd = new SqlCommand(sql, con, trx))
                    {
                        cmd.Parameters.AddWithValue("@cid", IdToEdit);
                        try
                        {
                            cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                            cmd.CommandText = "DELETE FROM Customer WHERE CustomerId=@cid";
                            cmd.Parameters.AddWithValue("@cid", IdToEdit);
                            cmd.ExecuteNonQuery();
                            trx.Commit();
                            TheForm.LoadDataBindingSource();
                            MessageBox.Show("Data Deleted successfully!!");
                            details.Clear();
                            this.Close();
                        }
                        catch (Exception)
                        {
                            trx.Rollback();
                            MessageBox.Show("Failed to delete data!!");
                        }
                        con.Close();
                    }
                }
            }
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
    }
}
