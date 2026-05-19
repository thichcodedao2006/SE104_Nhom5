using System;
using System.Collections.Generic;

namespace QLTB.Models;

public partial class FogetPass
{
    public int Id { get; set; }

    public string? Username { get; set; }

    public string? Otp { get; set; }

    public DateTime? ExpiredTime { get; set; }

    public virtual TaiKhoan? UsernameNavigation { get; set; }
}
