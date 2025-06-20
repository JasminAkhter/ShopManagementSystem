using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShopManagementSystem
{
    public partial class ShowEditMember : Form
    {
        public ShowEditMember()
        {
            InitializeComponent();
        }

        private void ShowEditMember_Load(object sender, EventArgs e)
        {
            LoadGridData();
        }


        #region LoadGridData
        private void LoadGridData()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
            {
                con.Open();
                SqlDataAdapter sda = new SqlDataAdapter(@"SELECT m.MemberId,m.MemberName,m.Phone,m.Email,m.Picture,mt.Name FROM Members m INNER JOIN MemberType mt ON m.MemberTypeId=mt.Id", con);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                this.dataGridView1.DataSource = dt;
                con.Close();
            }
        }
        #endregion


        #region btnPicture_Click
        private void btnPicture_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Image img = Image.FromFile(openFileDialog1.FileName);
                this.pictureBox1.Image = img;
                txtPicturePath.Text = openFileDialog1.FileName;
            }
        }
        #endregion


        #region btnUpdate_Click
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            #region Validations
            string memberId = txtId.Text.Trim();
            if (string.IsNullOrEmpty(memberId))
            {
                MessageBox.Show("Please select a member for Update.");
                return;
            }

            // Validation Unique MemberId
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
            {
                SqlCommand checkMemberIdCmd = new SqlCommand("SELECT COUNT(*) FROM Members WHERE MemberId = @memberId", con);
                checkMemberIdCmd.Parameters.AddWithValue("@memberId", memberId);
                con.Open();
                int memberIdCount = (int)checkMemberIdCmd.ExecuteScalar();
                con.Close();

                if (memberIdCount == 0)
                {
                    MessageBox.Show("Member ID does not exist.");
                    return;
                }
            }

            // Validation MemberName
            string memberName = txtName.Text.Trim();
            if (string.IsNullOrEmpty(memberName))
            {
                MessageBox.Show("Please input member name.");
                return;
            }

            // Validation Phone
            string phone = txtPhone.Text.Trim();
            if (phone.Length < 11 || phone.Length > 14)
            {
                MessageBox.Show("Phone number must be between 11 and 14 characters.");
                return;
            }

            // Validation phone number is unique
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
            {
                SqlCommand checkPhoneCmd = new SqlCommand("SELECT COUNT(*) FROM Members WHERE Phone = @phone AND MemberId != @memberId", con);
                checkPhoneCmd.Parameters.AddWithValue("@phone", phone);
                checkPhoneCmd.Parameters.AddWithValue("@memberId", memberId);
                con.Open();
                int phoneCount = (int)checkPhoneCmd.ExecuteScalar();
                con.Close();

                if (phoneCount > 0)
                {
                    MessageBox.Show("This phone number is already in use.");
                    return;
                }
            }

            // Validation Email
            string email = txtEmail.Text.Trim();
            if (!email.Contains("@"))
            {
                MessageBox.Show("Please input a valid email.");
                return;
            }

            // Email unique validation
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
            {
                SqlCommand checkEmailCmd = new SqlCommand("SELECT COUNT(*) FROM Members WHERE Email = @email AND MemberId != @memberId", con);
                checkEmailCmd.Parameters.AddWithValue("@email", email);
                checkEmailCmd.Parameters.AddWithValue("@memberId", memberId);
                con.Open();
                int emailCount = (int)checkEmailCmd.ExecuteScalar();
                con.Close();

                if (emailCount > 0)
                {
                    MessageBox.Show("This email address is already in use.");
                    return;
                }
            }
            #endregion

            try
            {
                string picUrl = txtPicturePath.Text;
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
                {
                    MemoryStream ms = null;
                    if (!string.IsNullOrEmpty(picUrl))
                    {
                        Image img = Image.FromFile(picUrl);
                        ms = new MemoryStream();
                        img.Save(ms, ImageFormat.Bmp);
                    }

                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;

                    if (ms == null)
                    {
                        cmd.CommandText = "UPDATE Members SET MemberName=@n, Phone=@p, Email=@e, MemberTypeId=@mtId WHERE MemberId=@memberId";
                        cmd.Parameters.AddWithValue("@pic", DBNull.Value);
                    }
                    else
                    {
                        cmd.CommandText = "UPDATE Members SET MemberName=@n, Phone=@p, Email=@e, Picture=@pic, MemberTypeId=@mtId WHERE MemberId=@memberId";
                        cmd.Parameters.Add(new SqlParameter("@pic", SqlDbType.VarBinary) { Value = ms.ToArray() });
                    }

                    cmd.Parameters.AddWithValue("@memberId", txtId.Text);
                    cmd.Parameters.AddWithValue("@n", txtName.Text);
                    cmd.Parameters.AddWithValue("@p", txtPhone.Text);
                    cmd.Parameters.AddWithValue("@e", txtEmail.Text);
                    cmd.Parameters.AddWithValue("@mtId", cmbSub.SelectedValue);

                    con.Open();
                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        MessageBox.Show("Data Updated Successfully!!!");
                        LoadGridData();
                        ClearAll();
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }
        #endregion


        #region btnDelete_Click
        private void btnDelete_Click(object sender, EventArgs e)
        {
            string memberId = txtId.Text.Trim();

            // Check MemberId selected
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
            {
                SqlCommand checkMemberIdCmd = new SqlCommand("SELECT COUNT(*) FROM Members WHERE MemberId = @memberId", con);
                checkMemberIdCmd.Parameters.AddWithValue("@memberId", memberId);
                con.Open();
                int memberIdCount = (int)checkMemberIdCmd.ExecuteScalar();
                con.Close();

                if (memberIdCount == 0)
                {
                    MessageBox.Show("Pleae select a member for Delete.");
                    return;
                }
            }

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("DELETE FROM Members WHERE MemberId=@id", con))
                    {
                        cmd.Parameters.AddWithValue("@id", txtId.Text);
                        con.Open();
                        if (cmd.ExecuteNonQuery() > 0)
                        {
                            MessageBox.Show("Data Deleted Successfully!!!");
                            LoadGridData();
                            ClearAll();
                        }
                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }
        #endregion


        #region ClearAll
        private void ClearAll()
        {
            txtId.Clear();
            txtName.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            txtPicturePath.Clear();
            cmbSub.SelectedIndex = -1;
            pictureBox1.Image = null;
        }
        #endregion


        #region dataGridView1_SelectionChanged
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int id = (int)dataGridView1.SelectedRows[0].Cells[0].Value;

                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Members WHERE MemberId=@memberId", con))
                    {

                        using (SqlDataAdapter sda = new SqlDataAdapter("SELECT DISTINCT * FROM MemberType", con))
                        {
                            DataSet ds = new DataSet();
                            sda.Fill(ds);
                            cmbSub.DataSource = ds.Tables[0];
                            cmbSub.DisplayMember = "Name";
                            cmbSub.ValueMember = "Id";
                        }


                        con.Open();
                        cmd.Parameters.AddWithValue("@memberId", id);
                        SqlDataReader dr = cmd.ExecuteReader();
                        if (dr.Read())
                        {
                            txtId.Text = dr.GetInt32(0).ToString();
                            txtId.Enabled = false;
                            txtName.Text = dr.GetString(1).ToString();
                            txtPhone.Text = dr.GetString(2).ToString();
                            txtEmail.Text = dr.GetString(3).ToString();

                            MemoryStream ms = new MemoryStream((byte[])dr[4]);
                            Image img = Image.FromStream(ms);
                            pictureBox1.Image = img;

                            cmbSub.SelectedValue = dr.GetInt32(5).ToString();
                        }

                        con.Close();
                    }
                }
            }
        }
        #endregion
    }
}
