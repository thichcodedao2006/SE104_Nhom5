using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLTB.Models
{
    public partial class ChiTietBaoTri
    {
        public int IdbaoTri { get; set; }

        public int IdthietBi { get; set; }

        public string SoSeri { get; set; } = null!;

        public int? IddichVu { get; set; }

        public string? GhiChuThietBi { get; set; }

        public string? TienDo { get; set; }

        public string? KetQua { get; set; }

        public virtual BaoTri IdbaoTriNavigation { get; set; } = null!;

        public virtual ChiTietThietBi ChiTietThietBi { get; set; } = null!;

        public virtual DichVuBaoTri? IddichVuNavigation { get; set; }
    }
}
