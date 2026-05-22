using System;
using System.Collections.Generic;

namespace QLTB.Models;

public partial class NhanVien
{
    public int IdnhanVien { get; set; }

    public string? HoTen { get; set; }

    public string? Sdt { get; set; }

    public string? Email { get; set; }

    public string? ChuyenMon { get; set; }

    public string? TinhTrang { get; set; }

    public int? IdboPhan { get; set; }

    public int? IdchucDanh { get; set; }

    public virtual ICollection<BaoTri> BaoTris { get; set; } = new List<BaoTri>();

    public virtual BoPhan? IdboPhanNavigation { get; set; }

    public virtual ChucDanh? IdchucDanhNavigation { get; set; }
}
