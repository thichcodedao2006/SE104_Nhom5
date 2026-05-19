using System;
using System.Collections.Generic;

namespace QLTB.Models;

public partial class TaiKhoan
{
    public string TenTaiKhoan { get; set; } = null!;

    public string? MatKhau { get; set; }

    public string? Email { get; set; }

    public int? LoaiTaiKhoan { get; set; }

    public int? DuocXacThuc { get; set; }

    public virtual ICollection<FogetPass> FogetPasses { get; set; } = new List<FogetPass>();
}
