using System;
using System.Collections.Generic;

namespace QLTB.Models;

public partial class BaoTri
{
    public int IdbaoTri { get; set; }

    public int? IdnhanVien { get; set; }

    public DateTime? NgayBaoTri { get; set; }

    public string? GhiChu { get; set; }

    public string? DoUuTien { get; set; }

    public string? TinhTrangBaoTri { get; set; }

    public virtual NhanVien? IdnhanVienNavigation { get; set; }

    public virtual ICollection<ChiTietBaoTri> ChiTietBaoTris { get; set; } = new List<ChiTietBaoTri>();
}