using Microsoft.Win32;
using QLTB.Helpers;
using QLTB.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace QLTB.ViewModel
{
    public class DeviceFormViewModel : BaseViewModel
    {
        private readonly QuanLyVatTuContext _context;

        public ObservableCollection<PhongBan> PhongBanList { get; set; }
        public ObservableCollection<string> CategoryList { get; set; }

        private bool _isNewDevice = true;
        public bool IsNewDevice
        {
            get => _isNewDevice;
            set
            {
                _isNewDevice = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEditMode));
            }
        }

        public bool IsEditMode => !IsNewDevice;

        private string _name;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        private string _manufacturer;
        public string Manufacturer { get => _manufacturer; set { _manufacturer = value; OnPropertyChanged(); } }

        private string _selectedCategory;
        public string SelectedCategory { get => _selectedCategory; set { _selectedCategory = value; OnPropertyChanged(); } }

        private string _serial;
        public string Serial { get => _serial; set { _serial = value; OnPropertyChanged(); } }

        private int? _selectedPhongBanId;
        public int? SelectedPhongBanId { get => _selectedPhongBanId; set { _selectedPhongBanId = value; OnPropertyChanged(); } }

        private string _status = "Đang hoạt động";
        public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }

        public bool IsSaved { get; private set; } = false;
        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand ImportFileCommand { get; set; }

        public DeviceFormViewModel() : this(null) { }

        public DeviceFormViewModel(int? existingDeviceId)
        {
            _context = new QuanLyVatTuContext();

            PhongBanList = new ObservableCollection<PhongBan>(_context.PhongBans.ToList());
            CategoryList = new ObservableCollection<string>(_context.ThietBis.Select(t => t.LoaiThietBi).Distinct().ToList());

            if (existingDeviceId.HasValue)
            {
                IsNewDevice = false;
            }

            SaveCommand = new RelayCommand(async o =>
            {
                if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Serial))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ Tên thiết bị và Số Serial!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    bool isSerialExist = _context.ChiTietThietBis.Any(ct => ct.SoSeri.ToLower() == Serial.Trim().ToLower());
                    if (isSerialExist)
                    {
                        MessageBox.Show("Số Serial đã tồn tại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var parent = _context.ThietBis.FirstOrDefault(t => t.TenThietBi.ToLower() == Name.Trim().ToLower()
                                                                    && t.DonViSanXuat.ToLower() == Manufacturer.Trim().ToLower());
                    if (parent == null)
                    {
                        parent = new ThietBi
                        {
                            TenThietBi = Name.Trim(),
                            LoaiThietBi = string.IsNullOrWhiteSpace(SelectedCategory) ? "Thiết bị điện tử" : SelectedCategory.Trim(),
                            DonViSanXuat = Manufacturer.Trim(),
                            NgayNhapThietBi = DateTime.Now
                        };
                        _context.ThietBis.Add(parent);
                        await _context.SaveChangesAsync();
                    }

                    var detail = new ChiTietThietBi
                    {
                        IdthietBi = parent.IdthietBi,
                        SoSeri = Serial.Trim(),
                        TinhTrang = Status == "Đang hoạt động" ? "Tốt" : Status,
                        IdphongBan = SelectedPhongBanId
                    };

                    _context.ChiTietThietBis.Add(detail);
                    await _context.SaveChangesAsync();

                    IsSaved = true;
                    MessageBox.Show("Lưu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (o is Window w) w.Close();
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            });

            ImportFileCommand = new RelayCommand(async o =>
            {
                if (string.IsNullOrWhiteSpace(Name))
                {
                    MessageBox.Show("Điền thông tin chung trước khi import!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "CSV Files|*.csv" };
                if (openFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        var lines = File.ReadAllLines(openFileDialog.FileName);
                        var localPhongBans = _context.PhongBans.ToList();

                        var parent = _context.ThietBis.FirstOrDefault(t => t.TenThietBi.ToLower() == Name.Trim().ToLower()
                                                                        && t.DonViSanXuat.ToLower() == Manufacturer.Trim().ToLower());
                        if (parent == null)
                        {
                            parent = new ThietBi
                            {
                                TenThietBi = Name.Trim(),
                                LoaiThietBi = string.IsNullOrWhiteSpace(SelectedCategory) ? "Thiết bị điện tử" : SelectedCategory.Trim(),
                                DonViSanXuat = Manufacturer.Trim(),
                                NgayNhapThietBi = DateTime.Now
                            };
                            _context.ThietBis.Add(parent);
                            await _context.SaveChangesAsync();
                        }

                        int importCount = 0;
                        for (int i = 1; i < lines.Length; i++)
                        {
                            if (string.IsNullOrWhiteSpace(lines[i])) continue;
                            var cols = lines[i].Split(',');
                            if (cols.Length >= 2)
                            {
                                string csvSeri = cols[0].Trim();
                                string csvPhong = cols[1].Trim();

                                bool isDup = _context.ChiTietThietBis.Any(ct => ct.SoSeri.ToLower() == csvSeri.ToLower());
                                if (isDup) continue;

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
                        MessageBox.Show($"Nạp file thành công! Thêm {importCount} thiết bị.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        if (o is Window w) w.Close();
                    }
                    catch (Exception ex) { MessageBox.Show("Lỗi Import: " + ex.Message); }
                }
            });

            CancelCommand = new RelayCommand(o => { IsSaved = false; if (o is Window w) w.Close(); });
        }
    }
}