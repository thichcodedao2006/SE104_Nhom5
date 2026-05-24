using System;
using System.Collections.Generic;

namespace QLTB.Models;

public partial class BaoTri
{
    public int IdbaoTri { get; set; }

    public int? IdthietBi { get; set; }

    public string? SoSeri { get; set; }

    public int? IddichVu { get; set; }
    
    public int? IdnhanVien { get; set; }

    public DateTime? NgayBaoTri { get; set; }

    public string? GhiChu { get; set; }

    public string? TinhTrangBaoTri { get; set; }

    public string? DoUuTien { get; set; }

    public virtual ChiTietThietBi? ChiTietThietBi { get; set; }

    public virtual DichVuBaoTri? IddichVuNavigation { get; set; }

    public virtual NhanVien? IdnhanVienNavigation { get; set; }
}
