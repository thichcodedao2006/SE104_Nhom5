using Microsoft.Win32;
using QLTB.Helpers;
using QLTB.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;

namespace QLTB.ViewModel
{
    public class DeviceFormViewModel : BaseViewModel
    {
        private readonly QuanLyVatTuContext _context;

        // Bảng Cha: ThietBi
        private string _name;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        private string _manufacturer;
        public string Manufacturer { get => _manufacturer; set { _manufacturer = value; OnPropertyChanged(); } }

        private string _category;
        public string Category { get => _category; set { _category = value; OnPropertyChanged(); } }

        // Bảng Con: ChiTietThietBi
        private string _serial;
        public string Serial { get => _serial; set { _serial = value; OnPropertyChanged(); } }

        private string _department;
        public string Department { get => _department; set { _department = value; OnPropertyChanged(); } }

        private string _status = "Đang hoạt động";
        public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }

        public bool IsSaved { get; private set; } = false;
        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand ImportFileCommand { get; set; }

        public DeviceFormViewModel()
        {
            _context = new QuanLyVatTuContext();

            // LUỒNG 1: XỬ LÝ LƯU THỦ CÔNG ĐƠN LẺ
            SaveCommand = new RelayCommand(async o =>
            {
                if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Serial))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ Tên thiết bị và Số Serial!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    // Kiểm tra ràng buộc trùng mã số Serial dưới hệ thống con trước
                    bool isSerialExist = _context.ChiTietThietBis.Any(ct => ct.SoSeri.ToLower() == Serial.Trim().ToLower());
                    if (isSerialExist)
                    {
                        MessageBox.Show($"Số Serial [{Serial}] đã tồn tại trong kho hệ thống. Không thể thêm trùng lặp thực thể!", "Xung đột dữ liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Thẩm định kiểm tra mẫu thiết bị cha
                    var parent = _context.ThietBis.FirstOrDefault(t => t.TenThietBi.ToLower() == Name.Trim().ToLower()
                                                                    && t.DonViSanXuat.ToLower() == Manufacturer.Trim().ToLower());
                    if (parent == null)
                    {
                        parent = new ThietBi
                        {
                            TenThietBi = Name.Trim(),
                            LoaiThietBi = string.IsNullOrWhiteSpace(Category) ? "Thiết bị điện tử" : Category.Trim(),
                            DonViSanXuat = Manufacturer.Trim(),
                            NgayNhapThietBi = DateTime.Now
                        };
                        _context.ThietBis.Add(parent);
                        await _context.SaveChangesAsync(); // Đẩy lên để lấy ID thật
                    }

                    int? pbId = null;
                    if (!string.IsNullOrWhiteSpace(Department))
                    {
                        var pb = _context.PhongBans.FirstOrDefault(p => p.TenPhong.ToLower() == Department.Trim().ToLower());
                        pbId = pb?.Idphong;
                    }

                    var detail = new ChiTietThietBi
                    {
                        IdthietBi = parent.IdthietBi,
                        SoSeri = Serial.Trim(),
                        TinhTrang = Status == "Đang hoạt động" ? "Tốt" : Status,
                        IdphongBan = pbId
                    };

                    _context.ChiTietThietBis.Add(detail);
                    await _context.SaveChangesAsync();

                    IsSaved = true;
                    MessageBox.Show("Thêm thiết bị đơn lẻ vào kho thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (o is Window w) w.Close();
                }
                catch (Exception ex) { MessageBox.Show("Lỗi lưu dữ liệu: " + ex.Message); }
            });

            // LUỒNG 2: XỬ LÝ IMPORT HÀNG LOẠT SERIAL THEO THÔNG TIN CHUNG PHÍA TRÊN
            ImportFileCommand = new RelayCommand(async o =>
            {
                if (string.IsNullOrWhiteSpace(Name))
                {
                    MessageBox.Show("Vui lòng điền trước Thông tin chung (Tên thiết bị, Nhà sản xuất) ở phần trên trước khi nạp file số Serial con!", "Nhắc nhở nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "CSV Files|*.csv" };
                if (openFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        var lines = File.ReadAllLines(openFileDialog.FileName);
                        var localPhongBans = _context.PhongBans.ToList();

                        // Kiểm tra / Tạo mới bảng cha duy nhất một lần dựa trên form nhập phía trên
                        var parent = _context.ThietBis.FirstOrDefault(t => t.TenThietBi.ToLower() == Name.Trim().ToLower()
                                                                        && t.DonViSanXuat.ToLower() == Manufacturer.Trim().ToLower());
                        if (parent == null)
                        {
                            parent = new ThietBi
                            {
                                TenThietBi = Name.Trim(),
                                LoaiThietBi = string.IsNullOrWhiteSpace(Category) ? "Thiết bị điện tử" : Category.Trim(),
                                DonViSanXuat = Manufacturer.Trim(),
                                NgayNhapThietBi = DateTime.Now
                            };
                            _context.ThietBis.Add(parent);
                            await _context.SaveChangesAsync();
                        }

                        int importCount = 0;
                        // Cấu trúc File CSV test case gọn nhẹ khi import tại đây: SoSeri,PhongBan
                        for (int i = 1; i < lines.Length; i++)
                        {
                            if (string.IsNullOrWhiteSpace(lines[i])) continue;
                            var cols = lines[i].Split(',');
                            if (cols.Length >= 2)
                            {
                                string csvSeri = cols[0].Trim();
                                string csvPhong = cols[1].Trim();

                                // Kiểm tra ràng buộc chặn trùng số Serial
                                bool isDup = _context.ChiTietThietBis.Any(ct => ct.SoSeri.ToLower() == csvSeri.ToLower());
                                if (isDup) continue; // Trùng mã số thì bỏ qua, đọc dòng kế tiếp

                                var pb = localPhongBans.FirstOrDefault(p => p.TenPhong.Equals(csvPhong, StringComparison.OrdinalIgnoreCase));

                                var detail = new ChiTietThietBi
                                {
                                    IdthietBi = parent.IdthietBi,
                                    SoSeri = csvSeri,
                                    TinhTrang = "Tốt",
                                    IdphongBan = pb?.Idphong
                                };
                                _context.ChiTietThietBis.Add(detail);
                                importCount++;
                            }
                        }

                        await _context.SaveChangesAsync();
                        IsSaved = true;
                        MessageBox.Show($"Nạp file thành công! Đã chèn thêm {importCount} số Serial con vào dòng máy [{Name}].", "Import hoàn tất", MessageBoxButton.OK, MessageBoxImage.Information);
                        if (o is Window w) w.Close();
                    }
                    catch (Exception ex) { MessageBox.Show("Lỗi phân tích tệp CSV: " + ex.Message, "Lỗi Import", MessageBoxButton.OK, MessageBoxImage.Error); }
                }
            });

            CancelCommand = new RelayCommand(o => { IsSaved = false; if (o is Window w) w.Close(); });
        }
    }
}