using QLTB.Helpers;
using System;
using System.Windows;
using System.Windows.Input;
using QLTB.Models;

namespace QLTB.ViewModel
{
    public class MaintenanceDetailViewModel : BaseViewModel
    {
        public string IDBaoTri { get; set; }
        public string TenThietBi { get; set; }
        public string SoSeri { get; set; }
        public string TenDichVu { get; set; }
        public string ChiPhi { get; set; }
        public string KyThuatVien { get; set; }
        public string NgayHoanThanh { get; set; }
        public string DoUuTien { get; set; }
        public string GhiChu { get; set; }

        public ICommand CloseCommand { get; set; }

        public MaintenanceDetailViewModel(BaoTri record)
        {
            if (record != null)
            {
                var detail = record.ChiTietBaoTris?.FirstOrDefault();

                IDBaoTri = $"#BT-{record.IdbaoTri}";

                TenThietBi = detail?.ChiTietThietBi?
                                    .IdthietBiNavigation?
                                    .TenThietBi ?? "Không rõ tên";

                SoSeri = detail?.SoSeri ?? "N/A";

                TenDichVu = detail?.IddichVuNavigation?
                                      .TenDichVu ?? "Dịch vụ tự do";

                ChiPhi = ((decimal?)detail?.IddichVuNavigation?.GiaDichVu ?? 0)
                    .ToString("N0") + " VNĐ";

                KyThuatVien = record.IdnhanVienNavigation?.HoTen ?? "Chưa phân công";

                NgayHoanThanh = record.NgayBaoTri?
                    .ToString("dd/MM/yyyy HH:mm") ?? "N/A";

                DoUuTien = record.DoUuTien ?? "Trung bình";

                GhiChu = record.GhiChu ?? "Không có ghi chú nào.";
            }

            CloseCommand = new RelayCommand(o =>
            {
                if (o is Window currentWindow)
                {
                    currentWindow.Close();
                }
            });
        }
    }
}