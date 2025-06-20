using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace ShopManagementSystem
{
    public partial class Product : Form
    {
        string conString = "Server=DESKTOP-FRGGIBC\\SQLEXPRESS;Database=ShopManagementDB;Integrated Security=True";
        SqlConnection sqlCon;
        SqlCommand cmd;
        string productId = "";

        public Product()
        {
            InitializeComponent();
            sqlCon = new SqlConnection(conString);
            sqlCon.Open();
        }

        private void Product_Load(object sender, EventArgs e)
        {
            LoadGrid();
        }

        private void LoadGrid()
        {
            dataGridView1.DataSource = ShowAllProductData();
        }

        private DataTable ShowAllProductData()
        {
            if (sqlCon.State == ConnectionState.Closed)
            {
                sqlCon.Open();
            }
            DataTable dtData = new DataTable();
            cmd = new SqlCommand("sp_Product", sqlCon);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@actionType", "ShowAllData");
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            sda.Fill(dtData);
            return dtData;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                MessageBox.Show("Enter product name!!");
                txtProductName.Select();
                return;
            }
            if (mfgDate.Value >= expDate.Value)
            {
                MessageBox.Show("MFG date cannot be later than or same as EXP Date!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else if (comboBox1.SelectedIndex <= -1)
            {
                MessageBox.Show("Select product type!!");
                comboBox1.Select();
                return;
            }
            else
            {
                try
                {
                    if (sqlCon.State == ConnectionState.Closed)
                    {
                        sqlCon.Open();
                    }
                    DataTable dtData = new DataTable();
                    cmd = new SqlCommand("sp_Product", sqlCon);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@actionType", "SaveData");
                    cmd.Parameters.AddWithValue("@productId", productId );
                    cmd.Parameters.AddWithValue("@productName", txtProductName.Text);
                    cmd.Parameters.AddWithValue("@mfgDate", mfgDate.Value.Date);
                    cmd.Parameters.AddWithValue("@expDate", expDate.Value.Date);
                    cmd.Parameters.AddWithValue("@productType", comboBox1.Text);

                    int numRes = cmd.ExecuteNonQuery();
                    if (numRes > 0)
                    {
                        MessageBox.Show("Data saved successfully!!!");
                        LoadGrid();
                        ClearAll();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }


        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearAll();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(productId))
            {
                try
                {
                    if (sqlCon.State == ConnectionState.Closed)
                    {
                        sqlCon.Open();
                    }
                    DataTable dtData = new DataTable();
                    cmd = new SqlCommand("sp_Product", sqlCon);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@actionType", "DeleteData");
                    cmd.Parameters.AddWithValue("@productId", productId);
                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    int numRes = cmd.ExecuteNonQuery();
                    if (numRes > 0)
                    {
                        MessageBox.Show("Data deleted successfully!!!");
                        LoadGrid();
                        ClearAll();
                    }
                    else
                    {
                        MessageBox.Show("Please try again!!");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: - " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Please select a record!!");
            }
        }


        private DataTable ShowEmpRecordById(string empId)
        {
            if (sqlCon.State == ConnectionState.Closed)
            {
                sqlCon.Open();
            }
            DataTable dtData = new DataTable();
            cmd = new SqlCommand("sp_Product", sqlCon);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@actionType", "ShowAllDataById");
            cmd.Parameters.AddWithValue("@productId", empId);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            sda.Fill(dtData);
            return dtData;
        }


        private void ClearAll()
        {
            btnSave.Text = "Save";
            txtProductName.Clear();
            comboBox1.SelectedIndex = -1;
            productId = "";
            LoadGrid();
        }

        private void dataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                btnSave.Text = "Update";
                productId = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                DataTable dtData = ShowEmpRecordById(productId);
                if (dtData.Rows.Count > 0)
                {
                    productId = dtData.Rows[0][0].ToString();
                    txtProductName.Text = dtData.Rows[0][1].ToString();
                    mfgDate.Text = dtData.Rows[0][2].ToString();
                    expDate.Text = dtData.Rows[0][3].ToString();
                    comboBox1.Text = dtData.Rows[0][4].ToString();
                }
            }
        }
    }
}
