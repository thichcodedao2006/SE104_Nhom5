using System;
using System.Collections.Generic;

namespace QLTB.Models;

public partial class PhongBan
{
    public int Idphong { get; set; }

    public string? TenPhong { get; set; }

    public int? ViTri { get; set; }

    public virtual ICollection<ChiTietThietBi> ChiTietThietBis { get; set; } = new List<ChiTietThietBi>();
}
