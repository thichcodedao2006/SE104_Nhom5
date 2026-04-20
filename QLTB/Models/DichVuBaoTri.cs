using System;
using System.Collections.Generic;

namespace QLTB.Models;

public partial class DichVuBaoTri
{
    public int IddichVu { get; set; }

    public string? TenDichVu { get; set; }

    public double? GiaDichVu { get; set; }

    public virtual ICollection<BaoTri> BaoTris { get; set; } = new List<BaoTri>();
}
