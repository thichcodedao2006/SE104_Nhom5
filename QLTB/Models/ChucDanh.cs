using System;
using System.Collections.Generic;

namespace QLTB.Models;

public partial class ChucDanh
{
    public int Id { get; set; }

    public string? TenChucDanh { get; set; }

    public virtual ICollection<NhanVien> NhanViens { get; set; } = new List<NhanVien>();
}
