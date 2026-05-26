# 🎉 TÓM TẮT FIX LỖI MAINTENANCEPLAN DETAIL

## ❌ VẤN ĐỀ BAN ĐẦU
Khi click "Xem chi tiết" trong màn hình Kế hoạch bảo trì → **Crash ngay lập tức** mà không hiển thị lỗi.

---

## 🔍 NGUYÊN NHÂN GỐC RỄ

### 1. **PopUpService không kế thừa Resources từ Application**
Khi `PopUpService.ShowPopUp()` tạo Window mới với `WindowStyle.None`, Window đó **không tự động kế thừa Resources** từ `Application.Resources`.

Khi XAML parser gặp:
```xml
<Border Background="{StaticResource MainBackgroundBrush}">
```

Mà không tìm thấy `MainBackgroundBrush` → **XamlParseException** → Crash ngay lập tức.

### 2. **DataContext bị ghi đè**
Code cũ:
```csharp
Window dialogWindow = new Window
{
    Content = vm,
    DataContext = vm  // ❌ Ghi đè DataContext của UserControl
};
```

Điều này gây xung đột vì `MaintenancePlanDetailView` đã set `DataContext` trong constructor.

### 3. **Properties dạng Expression-bodied**
Code cũ:
```csharp
public string Title => Plan.Title;  // ❌ Nếu Plan null → crash
```

Nếu `Plan` chưa được khởi tạo khi XAML binding → NullReferenceException.

---

## ✅ GIẢI PHÁP ĐÃ ÁP DỤNG

### 1. **Fix PopUpService.cs**
```csharp
Window dialogWindow = new Window
{
    Content = vm,
    Resources = Application.Current.Resources  // ✅ Kế thừa Resources
    // ❌ KHÔNG set DataContext = vm
};
```

**Lý do:**
- `Resources = Application.Current.Resources` đảm bảo Window có thể resolve `{StaticResource ...}`
- Không set `DataContext` để UserControl tự quản lý DataContext của mình

### 2. **Fix MaintenancePlanDetailViewModel.cs**
Thay đổi từ expression-bodied properties sang full properties:

**Trước:**
```csharp
public string Title => Plan.Title;  // ❌ Crash nếu Plan null
```

**Sau:**
```csharp
private string _title;
public string Title
{
    get => _title;
    set
    {
        _title = value;
        OnPropertyChanged(nameof(Title));
    }
}

// Trong constructor:
Title = Plan?.Title ?? "N/A";  // ✅ Null-safe
```

**Lý do:**
- Tránh NullReferenceException khi binding trước khi Plan được khởi tạo
- Cho phép set giá trị default "N/A" nếu Plan null
- Hỗ trợ INotifyPropertyChanged đúng cách

### 3. **Thêm Null-Safety**
```csharp
private async Task LoadDevices()
{
    if (Plan == null)  // ✅ Kiểm tra null
    {
        MessageBox.Show("Lỗi: Không có thông tin kế hoạch bảo trì.");
        return;
    }
    // ...
}
```

### 4. **Thêm Default Values**
```csharp
TenThietBi = x.ChiTietThietBi?.IdthietBiNavigation?.TenThietBi ?? "N/A",
SoSeri = x.SoSeri ?? "N/A",
TinhTrang = x.ChiTietThietBi?.TinhTrang ?? "N/A",
// ...
```

---

## 📊 KẾT QUẢ

✅ Không còn crash khi click "Xem chi tiết"  
✅ Popup hiển thị đúng thông tin kế hoạch bảo trì  
✅ DataGrid load dữ liệu thiết bị thành công  
✅ Button "Đóng" hoạt động bình thường  

---

## 🎓 BÀI HỌC

### 1. **Window Resources trong WPF**
Khi tạo Window động (runtime), phải explicitly set `Resources`:
```csharp
new Window { Resources = Application.Current.Resources }
```

### 2. **StaticResource vs DynamicResource**
- `{StaticResource}`: Resolve lúc compile/load XAML → Crash nếu không tìm thấy
- `{DynamicResource}`: Resolve lúc runtime → Không crash nhưng không hiển thị

### 3. **DataContext Inheritance**
- UserControl nên tự quản lý DataContext trong constructor
- Không nên ghi đè DataContext từ bên ngoài

### 4. **Null-Safety trong MVVM**
- Luôn dùng `?.` và `??` cho navigation properties
- Dùng full properties thay vì expression-bodied khi có khả năng null
- Kiểm tra null trước khi truy cập database

### 5. **Debugging WPF Crashes**
- Bật "Break on all exceptions" trong Visual Studio
- Xem Output window để tìm XamlParseException
- Thêm try-catch và Debug.WriteLine để trace
- Test với XAML đơn giản để isolate vấn đề

---

## 🔧 CÁC FILE ĐÃ SỬA

1. ✅ `Helpers/PopUpService.cs` - Thêm `Resources = Application.Current.Resources`
2. ✅ `ViewModel/MaintenancePlanDetailViewModel.cs` - Đổi sang full properties, thêm null-safety
3. ✅ `ViewModel/MaintenancePlan.cs` - Cleanup error handling
4. ✅ `UserControlFolder/Maintenance/MaintenancePlanDetailView.xaml.cs` - Cleanup debug code

---

## 🚀 APPLY CHO CÁC POPUP KHÁC

Nếu có popup khác cũng bị crash tương tự, áp dụng:

1. **Đảm bảo PopUpService có `Resources = Application.Current.Resources`** ✅ (Đã fix)
2. **Không set DataContext trong PopUpService** ✅ (Đã fix)
3. **UserControl tự set DataContext trong constructor**
4. **Dùng full properties thay vì expression-bodied**
5. **Thêm null-safety cho tất cả navigation properties**

---

## 📝 GHI CHÚ

- PopUpService giờ đã an toàn cho tất cả popup trong project
- Không cần thêm fallback resources vào từng UserControl nữa
- Code đã được cleanup, bỏ hết debug logging

**Ngày fix:** 2026-05-26  
**Người fix:** Kiro AI Assistant  
**Status:** ✅ RESOLVED
