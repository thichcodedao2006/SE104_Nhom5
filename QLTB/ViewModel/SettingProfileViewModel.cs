using Microsoft.EntityFrameworkCore;
using QLTB.Data;
using QLTB.Helpers;
using QLTB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QLTB.ViewModel
{
    public class SettingProfileViewModel : BaseViewModel
    {
        private TaiKhoan userAccount;
        private NhanVien userProfile;
        private string currentAvatar;
        private string _filePath = null;
        public SettingProfileViewModel(TaiKhoan t, NhanVien n)
        {
            UserAccount = t;
            UserProfile = n;
            LoadCommand();
            _ = Initialize();
        }

        public TaiKhoan UserAccount { get => userAccount; set
            {

                userAccount = value;
                OnPropertyChanged(nameof(UserAccount));
            }
                }

        public NhanVien UserProfile { get => userProfile; set
            {
                userProfile = value;
                OnPropertyChanged(nameof(UserProfile));
            }
                }


        public ICommand SaveChangesCommand { get; set; }
        public ICommand UploadPhotoCommand { get; set; }
        public string CurrentAvatar { get => currentAvatar; set
            {
                currentAvatar = value;
                OnPropertyChanged(nameof(CurrentAvatar));
            }
                }


        private void LoadCommand()
        {
            SaveChangesCommand = new RelayCommand<object>
                (
                    (p) => true,  async (p) =>  await SaveUserData()
                );

            UploadPhotoCommand = new RelayCommand<object>

                (
                    (p) => true, (p) => OpenPhoto()
                );
        }

        private async Task SaveUserData()
        {
            var nv = await DataProvider.Instance.DB.NhanViens.FindAsync(UserProfile.IdnhanVien);
            if (nv != null)
            {
                nv.HoTen = UserProfile.HoTen;
                nv.Sdt = UserProfile.Sdt;
                await DataProvider.Instance.DB.SaveChangesAsync();
                if (_filePath != null)
                {
                    string newLink = await CloudinaryService.UploadImageAsync(_filePath, KeyData.AvatarFolder, KeyData.NhanVienTag + UserProfile.IdnhanVien);

                    CurrentAvatar = newLink;
                    _filePath = null;

                    EventSystem.AvatarChange?.Invoke(newLink); // thông báo có thay đổi Avatar
                }    
                MessageBox.Show("Cập nhật dữ liệu thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OpenPhoto()
        {
            // Khởi tạo hộp thoại chọn file
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            // Cài đặt bộ lọc: Chỉ cho phép chọn file có đuôi .jpg, .jpeg hoặc .png
            openFileDialog.Filter = "Image Files (*.jpg; *.jpeg; *.png)|*.jpg; *.jpeg; *.png";

            // Tùy chọn: Thêm tiêu đề cho cửa sổ
            openFileDialog.Title = "Chọn ảnh đại diện";

            // Hiển thị hộp thoại và chờ người dùng thao tác
            bool? result = openFileDialog.ShowDialog();

            // Nếu người dùng chọn file và bấm OK (hoặc Open)
            if (result == true)
            {
                // Lấy được đường dẫn tuyệt đối của file
                string filePath = openFileDialog.FileName;

                _filePath = filePath;

                CurrentAvatar = filePath; // khi bấm thì chỉ thay đổi ở đây.
            }
        }

        private async Task Initialize()
        {
            CurrentAvatar = CloudinaryService.GetImageUrl(KeyData.AvatarFolder, KeyData.NhanVienTag + UserProfile.IdnhanVien);
        }
        
    }
}
