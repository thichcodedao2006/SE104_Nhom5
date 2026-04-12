using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Data.SqlClient;

namespace QLTB
{
    /// <summary>
    /// Interaction logic for fmDangNhap.xaml
    /// </summary>
    public partial class fmDangNhap : Window
    {
        string connectionString = @"Server=QuanLyVatTu.mssql.somee.com;
                                    Database=QuanLyVatTu;
                                    User Id=thichcodedao_SQLLogin_1;
                                    Password=sb4659th3x;
                                    Encrypt=True;
                                    TrustServerCertificate=True;";
        public fmDangNhap()
        {
            InitializeComponent();
        }
        private SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private bool KTDangNhap(string name, string pass)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                string query = @"Select count(*) from TaiKhoan where TenTaiKhoan= @name and MatKhau = @pass and DuocXacThuc=1";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@pass", pass);
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btn_DangNhap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(KTDangNhap(txtTenDangNhap.Text, txtMatKhau.Password.Trim()))
                {
                    MessageBox.Show("Đăng nhập thành công!");
                }  
                else
                {
                    MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng, hoặc tài khoản chưa được xác thực!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối cơ sở dữ liệu: " + ex.Message);
            }
        }
        private void QuenMatKhau_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Vui lòng liên hệ quản trị viên để được hỗ trợ!");
        }

        private void btnDangKy_Click(object sender, RoutedEventArgs e)
        {
            fmDangKy dk = new fmDangKy();
            this.Hide();
            dk.ShowDialog();
            this.Show();
        }
    }
}
   
