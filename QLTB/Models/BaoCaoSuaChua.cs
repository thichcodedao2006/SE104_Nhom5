using System;
using System.Collections.Generic;

namespace QLTB.Models;

public partial class BaoCaoSuaChua
{
    public int IdbaoCao { get; set; }

    public int? IdthietBi { get; set; }

    public string? SoSeri { get; set; }

    public string? TenNguoiBaoCao { get; set; }

    public string? Sdt { get; set; }

    public DateTime? NgayBaoCao { get; set; }

    public string? GhiChu { get; set; }

    public string? MucDoNghiemTrong { get; set; }

    public string? TrangThai { get; set; }

    public int? IdBaoTri { get; set; }

    public virtual ChiTietThietBi? ChiTietThietBi { get; set; }
}
