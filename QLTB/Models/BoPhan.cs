using System;
using System.Collections.Generic;

namespace QLTB.Models;

public partial class BoPhan
{
    public int Id { get; set; }

    public string? TenBoPhan { get; set; }

    public virtual ICollection<NhanVien> NhanViens { get; set; } = new List<NhanVien>();
}
