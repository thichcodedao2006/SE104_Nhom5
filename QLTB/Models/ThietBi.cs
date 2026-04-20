using System;
using System.Collections.Generic;

namespace QLTB.Models;

public partial class ThietBi
{
    public int IdthietBi { get; set; }

    public string? TenThietBi { get; set; }

    public string? LoaiThietBi { get; set; }

    public int? BaoHanhDinhKy { get; set; }

    public string? DonViSanXuat { get; set; }

    public int? SoLuong { get; set; }

    public double? Gia { get; set; }

    public virtual ICollection<ChiTietThietBi> ChiTietThietBis { get; set; } = new List<ChiTietThietBi>();
}
