using System;
using System.Collections.Generic;

namespace QLTB.Models;

public partial class ChiTietThietBi
{
    public int IdthietBi { get; set; }

    public string SoSeri { get; set; } = null!;

    public string? TinhTrang { get; set; }

    public int? IdphongBan { get; set; }

    public virtual ICollection<BaoCaoSuaChua> BaoCaoSuaChuas { get; set; } = new List<BaoCaoSuaChua>();

    // XÓA dòng BaoTris cũ
    // public virtual ICollection<BaoTri> BaoTris { get; set; } = new List<BaoTri>();

    // THÊM dòng mới
    public virtual ICollection<ChiTietBaoTri> ChiTietBaoTris { get; set; } = new List<ChiTietBaoTri>();

    public virtual PhongBan? IdphongBanNavigation { get; set; }

    public virtual ThietBi IdthietBiNavigation { get; set; } = null!;
}