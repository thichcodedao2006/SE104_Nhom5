using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace QLTB.Helpers
{
    public class ConvertAccount : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "Chưa xác định";

            int role = (int)value;

            switch (role)
            {
                case 0: // Hoặc case "Admin": nếu bạn lưu chuỗi
                    return "Admin";
                case 1: // Hoặc case "Technician":
                    return "Quản lý cấp cao";
                case 2:
                    return "Nhân viên bảo trì";
                default:
                    return "Chưa xác định";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Thường TextBlock chỉ hiển thị (OneWay) nên không cần code hàm này
            throw new NotImplementedException();
        }
    }
}
