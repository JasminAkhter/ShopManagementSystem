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
    public partial class AddMember : Form
    {
        public AddMember()
        {
            InitializeComponent();
        }

        private void AddMember_Load(object sender, EventArgs e)
        {
            LoadCombo();
        }


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


        #region LoadCombo
        private void LoadCombo()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
            {
                using (SqlDataAdapter sda = new SqlDataAdapter("SELECT * FROM MemberType", con))
                {
                    DataSet ds = new DataSet();
                    sda.Fill(ds);
                    cmbSub.DataSource = ds.Tables[0];
                    cmbSub.DisplayMember = "Name";
                    cmbSub.ValueMember = "Id";
                }
            }
        }
        #endregion


        #region btnSave_Click
        private void btnSave_Click(object sender, EventArgs e)
        {
            #region Validations
            // Validation MemberId
            string memberId = txtId.Text.Trim();
            if (string.IsNullOrEmpty(memberId))
            {
                MessageBox.Show("Please input member Id.");
                return;
            }

            // Validation MemberId is unique
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
            {
                SqlCommand checkMemberIdCmd = new SqlCommand("SELECT COUNT(*) FROM Members WHERE MemberId = @memberId", con);
                checkMemberIdCmd.Parameters.AddWithValue("@memberId", memberId);
                con.Open();
                int memberIdCount = (int)checkMemberIdCmd.ExecuteScalar();
                con.Close();

                if (memberIdCount > 0)
                {
                    MessageBox.Show("This Member ID already exists.");
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

            // Validation for phone number is unique
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
            {
                SqlCommand checkPhoneCmd = new SqlCommand("SELECT COUNT(*) FROM Members WHERE Phone = @phone", con);
                checkPhoneCmd.Parameters.AddWithValue("@phone", phone);
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

            // Validation for email is unique
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
            {
                SqlCommand checkEmailCmd = new SqlCommand("SELECT COUNT(*) FROM Members WHERE Email = @email", con);
                checkEmailCmd.Parameters.AddWithValue("@email", email);
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
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
                {
                    Image img = Image.FromFile(txtPicturePath.Text);
                    MemoryStream ms = new MemoryStream();
                    img.Save(ms, ImageFormat.Bmp);

                    con.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = "INSERT INTO Members(MemberId,MemberName,Phone,Email,Picture,MemberTypeId) VALUES(@id,@n,@p,@e,@pic,@mtid)";
                    cmd.Parameters.AddWithValue("@id", txtId.Text);
                    cmd.Parameters.AddWithValue("@n", txtName.Text);
                    cmd.Parameters.AddWithValue("@p", txtPhone.Text);
                    cmd.Parameters.AddWithValue("@e", txtEmail.Text);
                    cmd.Parameters.Add(new SqlParameter("@pic", SqlDbType.VarBinary) { Value = ms.ToArray() });
                    cmd.Parameters.AddWithValue("@mtid", cmbSub.SelectedValue);

                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        MessageBox.Show("Data Inserted Successfully!!!");
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
    }
}
