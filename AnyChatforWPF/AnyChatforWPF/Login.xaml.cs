using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace AnyChatforWPF
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        InfoController infoc = new InfoController();
        
        public Login(InfoController info)
        {
            this.infoc = info;
            InitializeComponent();
        }
        /*
        private void Button_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void register_button_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }*/

        private void login_button_Click(object sender, RoutedEventArgs e)
        {
            string ConString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            SqlConnection db = new SqlConnection(ConString);
            infoc.idnum = Convert.ToInt32(id_text.Text);
            string sqlCheck = "SELECT id FROM users WHERE id = @idnum";

            db.Open();
            using (db)
            {
                SqlCommand cmdChk = new SqlCommand(sqlCheck, db);
                cmdChk.Parameters.AddWithValue("@idnum", id_text.Text);
                object result = cmdChk.ExecuteScalar();
                cmdChk.Parameters.Clear();
                if (result != DBNull.Value && result != null)
                {
                    string sqlIns = "INSERT INTO online (User_ID) VALUES (@idnumber)";
                    SqlCommand cmdIns = new SqlCommand(sqlIns, db);
                    cmdIns.Parameters.AddWithValue("@idnumber", id_text.Text);
                    cmdIns.ExecuteNonQuery();
                    cmdIns.Parameters.Clear();
                    Close();
                }
                else
                {
                    messageLabel.Content = "This is not a valid user.";
                }
                db.Close();
            }
        }

        private void register_button_Click(object sender, RoutedEventArgs e)
        {
            string ConString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            SqlConnection db = new SqlConnection(ConString);
            infoc.username = username_text.Text;
            string sqlIns = "INSERT INTO users (username) VALUES (@username)";
            db.Open();
            using (db)
            {
                SqlCommand cmdIns = new SqlCommand(sqlIns, db);
                cmdIns.Parameters.AddWithValue("@username", username_text.Text);
                cmdIns.ExecuteNonQuery();

                cmdIns.Parameters.Clear();
                cmdIns.CommandText = "SELECT @@IDENTITY";

                // Get the last inserted id.
                string insertID = Convert.ToString(cmdIns.ExecuteScalar());
                string onlineIns = "INSERT INTO online (User_ID) VALUES (@idnumber)";
                SqlCommand cmdIns2 = new SqlCommand(onlineIns, db);
                cmdIns2.Parameters.AddWithValue("@idnumber", insertID);
                cmdIns2.ExecuteNonQuery();
                cmdIns2.Parameters.Clear();
            }
            Close();
        }
    }
}
