
create table ThietBi
(
	IDThietBi int primary key identity,
	TenThietBi nvarchar(100),
	LoaiThietBi nvarchar(100),
	BaoHanhDinhKy int, -- tính theo tháng 
	DonViThoiGian int, -- 0: Phút 1: Giờ 2: Ngày 3: Tháng 4: Năm 
	DonViSanXuat nvarchar(100),
	SoLuong int, -- số lượng tổng (số lượng còn lại hay số lượng đang sử dụng sẽ được tính dựa trên cái này)
	Gia float, -- giá (trên mỗi thiết bị) 
	NgayNhapThietBi DateTime default GetDate()
)

alter table ThietBi
add NgayNhapThietBi DateTime
alter table ThietBi
add constraint DF_Ngay
Default GetDate() for NgayNhapThietBi
alter table ThietBi 
add DonViThoiGian int

select * from ThietBi
create table PhongBan 
(
	IDPhong int primary key identity,
	TenPhong nvarchar(100),
	ViTri int, -- tầng mấy 
)


create table ChiTietThietBi 
(
	IDThietBi int,
	SoSeri varchar(10), -- phân biệt giữa các thiết bị cùng loại 
	TinhTrang nvarchar(50) default 'Tốt', -- có 3 trạng thái là Tốt, Lỗi và Đang bảo trì
	IDPhongBan int, -- thiết bị đang được sử dụng ở phòng ban nào (mặc định ở phòng kho) 
	-- hình ảnh thiết bị
	Primary key(IDThietBi, SoSeri),

	Foreign key (IDThietBi) references ThietBi(IDThietBi),
	Foreign key (IDPhongBan) references PhongBan(IDPhong)
)

alter table ChiTietThietBi
drop constraint DF__ChiTietTh__NgayN__3C69FB99
alter table ChiTietThietBi
drop column NgayNhapThietBi

create table BoPhan
(
	ID int primary key identity,
	TenBoPhan nvarchar(50),
)

create table ChucDanh
(
	ID int primary key identity,
	TenChucDanh nvarchar(50),
)

create table NhanVien 
(
	IDNhanVien int primary key identity,
	HoTen nvarchar(50),
	SDT varchar(10),
	Email varchar(50),
	ChuyenMon nvarchar(100), -- Quan ly la chuyen mon rieng ,
	TinhTrang nvarchar(50), -- đang rảnh, đang bận, nghỉ việc, ...
	IDBoPhan int,
	IDChucDanh int,

	foreign key (IDBoPhan) references BoPhan(ID),
	foreign key (IDChucDanh) references ChucDanh(ID),
)

alter table NhanVien
add IDBoPhan int 

alter table NhanVien 
add IDChucDanh int

alter table NhanVien
add constraint FK_BoPhan
foreign key (IDBoPhan) references BoPhan(ID)

alter table NhanVien
add constraint FK_ChucDanh
foreign key (IDChucDanh) references ChucDanh(ID)




create table TaiKhoan
(
	TenTaiKhoan varchar(50) primary key,
	MatKhau varchar(50),
	Email varchar(50), -- không được sử dụng trùng email 
	LoaiTaiKhoan int, --0: Giam doc, 1: Quan ly kho, 2: Nhan vien binh thuong 
	DuocXacThuc int, --0: chua duoc, 1: duoc 
)
	
alter table TaiKhoan 
drop column Avatar

select * from TaiKhoan

create table DichVuBaoTri 
(
	IDDichVu int primary key identity,
	TenDichVu nvarchar(100),
	GiaDichVu float default 0,
	Value int default 0,
	Unit int default 0 -- 0: Phút 1: Giờ 2: Ngày 3: Tháng 4: Năm 
)

alter table DichVuBaoTri
add Value int 

alter table DichVuBaoTri
add Unit int

alter table DichVuBaoTri
add constraint Value_Default
Default 0 for Value

alter table DichVuBaoTri
add constraint Unity_Default 
Default 0 for Unit

-- Dùng để báo cáo hư hỏng thiết bị
create table BaoCaoSuaChua 
(
	IDBaoCao int primary key identity,
	IDThietBi int,
	SoSeri varchar(10),
	TenNguoiBaoCao nvarchar(50),
	SDT varchar(10),
	NgayBaoCao DateTime, 
	MucDoNghiemTrong nvarchar(50), -- 4 mức độ: Nghiêm trọng, Cao, Trung bình, thấp
	TrangThai nvarchar(20) default N'Vừa cập nhật' , -- 3 trạng thái: Vừa cập nhật, đang xử lý, đã giải quyết.
	GhiChu nvarchar(100),
	IdBaoTri int default -1, -- mặc định là không có bảo trì nào cả 

	foreign key (IDThietBi, SoSeri) references ChiTietThietBi(IDThietBi, SoSeri)
)


alter table BaoCaoSuaChua 
add TrangThai nvarchar(20)
alter table BaoCaoSuaChua
add constraint DF_State
default N'Vừa cập nhật' for TrangThai
alter table BaoCaoSuaChua
add MucDoNghiemTrong nvarchar(50)

create table BaoTri 
(
	IDBaoTri int primary key identity,
	IDThietBi int ,
	SoSeri varchar(10),
	IDDichVu int,
	IDNhanVien int,
	NgayBaoTri DateTime,
	GhiChu nvarchar(100),
	DoUuTien nvarchar(20), -- Độ ưu tiên cần thực hiện công việc: Cao, Thấp, Trung bình
	TinhTrangBaoTri nvarchar(50), -- 3 tình trạng: Hoàn thành, Quá hạn, Đang xử lý

	foreign key (IDThietBi, SoSeri) references ChiTietThietBi(IDThietBi, SoSeri),
	foreign key (IDDichVu) references DichVuBaoTri(IDDichVu),
	foreign key (IDNhanVien) references NhanVien(IDNhanVien)
)
alter table BaoTri 
add DoUuTien nvarchar(20)


------- Insert -------
INSERT INTO PhongBan (TenPhong, ViTri) VALUES 
(N'Phòng Hành chính - Nhân sự', 1), -- Tầng 1 (Tiện cho ứng viên, tiếp tân)
(N'Phòng Kế toán - Tài chính', 2),  -- Tầng 2
(N'Phòng Kinh doanh (Sales)', 3),   -- Tầng 3
(N'Phòng Marketing', 3),            -- Tầng 3 (Chung tầng với Sales để dễ phối hợp)
(N'Phòng Công nghệ thông tin (IT)', 4), -- Tầng 4
(N'Phòng Nghiên cứu & Phát triển (R&D)', 5), -- Tầng 5 (Cần yên tĩnh)
(N'Ban Giám đốc', 6);               -- Tầng 6 (Tầng cao nhất)



-- ========================================================
-- 2. CHÈN DỮ LIỆU MẪU CHO BẢNG: ThietBi
-- ========================================================
-- Lưu ý: Sửa lại lỗi thiếu dấu phẩy trước trường NgayNhapThietBi trong cấu trúc gốc của bạn
SET IDENTITY_INSERT ThietBi ON;

INSERT INTO ThietBi (IDThietBi, TenThietBi, LoaiThietBi, BaoHanhDinhKy, DonViThoiGian, DonViSanXuat, SoLuong, Gia, NgayNhapThietBi) VALUES
(1, N'Máy tính Dell OptiPlex', N'Thiết bị điện tử', 12, 3, N'Dell Global', 15, 15000000, '2025-01-15 08:00:00'),
(2, N'Máy in Canon LBP2900', N'Thiết bị văn phòng', 6, 3, N'Canon Việt Nam', 5, 4500000, '2025-03-10 09:30:00'),
(3, N'Điều hòa Daikin 12000 BTU', N'Thiết bị điện lạnh', 2, 4, N'Daikin Thailand', 8, 12500000, '2024-06-01 14:00:00'),
(4, N'Router Cisco ISR 1100', N'Thiết bị mạng', 24, 3, N'Cisco Systems', 3, 22000000, '2025-02-20 10:15:00'),
(5, N'Ghế xoay văn phòng Hòa Phát', N'Nội thất', 1, 4, N'Hòa Phát', 30, 850000, '2025-05-01 11:00:00');

SET IDENTITY_INSERT ThietBi OFF;

-- ========================================================
-- 3. CHÈN DỮ LIỆU MẪU CHO BẢNG: ChiTietThietBi (Mỗi dòng là 1 máy)
-- ========================================================
INSERT INTO ChiTietThietBi (IDThietBi, SoSeri, TinhTrang, IDPhongBan) VALUES
-- Máy tính Dell (IDThietBi = 1)
(1, 'DELL0001', N'Tốt', 2),
(1, 'DELL0002', N'Tốt', 2),
(1, 'DELL0003', N'Lỗi', 3),         -- Đang bị lỗi ở phòng kế toán
(1, 'DELL0004', N'Tốt',  1), -- Đang bảo trì, đem về kho vật tư
(1, 'DELL0005', N'Tốt', 4),
-- Máy in Canon (IDThietBi = 2)
(2, 'CANO0001', N'Tốt', 3),
(2, 'CANO0002', N'Tốt', 4),
(2, 'CANO0003', N'Tốt', 1),
-- Điều hòa Daikin (IDThietBi = 3)
(3, 'DAIK0001', N'Tốt', 2),
(3, 'DAIK0002', N'Tốt', 3),
(3, 'DAIK0003', N'Tốt', 5),         -- Điều hòa phòng họp bị lỗi
-- Router Cisco (IDThietBi = 4)
(4, 'CISC0001', N'Tốt', 2),
-- Ghế Hòa Phát (IDThietBi = 5)
(5, 'HOAP0001', N'Tốt', 2),
(5, 'HOAP0002', N'Tốt', 3),
(5, 'HOAP0003', N'Tốt', 4);

-- ========================================================
-- 4. CHÈN DỮ LIỆU MẪU CHO BẢNG: NhanVien
-- ========================================================
SET IDENTITY_INSERT NhanVien ON;

INSERT INTO NhanVien (HoTen, SDT, Email, ChuyenMon, TinhTrang, IDBoPhan, IDChucDanh) 
VALUES
(N'Phạm Đan Trường', '0358002806', '24521898@gm.uit.edu.vn', N'Phần mềm', N'Đang rảnh', 2, 3), -- Phòng Kỹ thuật & Bảo trì | KT viên bậc cao
(N'Hà Gia Bảo', '0912345678', 'thang.nv@company.com', N'Quản lý', N'Đang rảnh', 1, 1),        -- Phòng QL Thiết bị | Trưởng phòng
(N'Bùi Bá Bổng', '0987654321', 'tung.tt@company.com', N'Sửa chữa máy tính', N'Đang bận', 2, 4), -- Phòng Kỹ thuật & Bảo trì | KT viên (Junior)
(N'Hà Tuấn Hùng', '0933445566', 'mai.lt@company.com', N'Bảo trì điện lạnh', N'Đang rảnh', 2, 6), -- Phòng Kỹ thuật & Bảo trì | Kỹ sư cơ điện
(N'Phạm Hồng Sơn', '0944556677', 'son.ph@company.com', N'Quản trị mạng Cisco', N'Đang bận', 2, 3),-- Phòng Kỹ thuật & Bảo trì | KT viên bậc cao
(N'Vũ Hoàng Long', '0955667788', 'long.vh@company.com', N'Sửa chữa thiết bị VP', N'Nghỉ việc', 2, 4);-- Phòng Kỹ thuật & Bảo trì | KT viên (Junior)


SET IDENTITY_INSERT NhanVien OFF;

-- ========================================================
-- 5. CHÈN DỮ LIỆU MẪU CHO BẢNG: DichVuBaoTri
-- ========================================================
SET IDENTITY_INSERT DichVuBaoTri ON;

INSERT INTO DichVuBaoTri (IDDichVu, TenDichVu, GiaDichVu, Value, Unit) VALUES
(1, N'Cài hệ điều hành & Phần mềm văn phòng', 150000, 2, 1), -- Hạn 2 Giờ (Unit = 1)
(2, N'Vệ sinh & Tra keo tản nhiệt máy tính', 100000, 6, 3),  -- Định kỳ 6 Tháng (Unit = 3)
(3, N'Bơm gas & Vệ sinh lưới lọc điều hòa', 450000, 1, 4),  -- Định kỳ 1 Năm (Unit = 4)
(4, N'Sửa kẹt giấy & Thay trống mực máy in', 250000, 3, 2),  -- Hạn xử lý trong 3 Ngày (Unit = 2)
(5, N'Cấu hình VLAN & Khắc phục sự cố mạng', 500000, 30, 0); -- Hạn xử lý khẩn cấp trong 30 Phút (Unit = 0);

SET IDENTITY_INSERT DichVuBaoTri OFF;

INSERT INTO BaoTri (IDThietBi, SoSeri, IDNhanVien, IDDichVu, NgayBaoTri, TinhTrangBaoTri) VALUES
(1, 'DELL0004', 2, 1, '2026-05-20 14:00:00', N'Đang xử lý'), -- Sửa máy tính Dell
(2, 'CANO0003', 3, 4, '2026-05-19 09:00:00', N'Đang xử lý'), -- Sửa máy in Canon
(3, 'DAIK0003', 4, 3, '2026-05-15 08:30:00', N'Hoàn thành');

delete from BaoTri
SET IDENTITY_INSERT BoPhan ON;
INSERT INTO BoPhan (ID, TenBoPhan) VALUES
(1, N'Phòng Quản lý Thiết bị & Vật tư'),
(2, N'Phòng Kỹ thuật & Bảo trì'),
(3, N'Phòng Hành chính - Nhân sự'),
(4, N'Phòng Kế toán - Tài chính'),
(5, N'Phân xưởng Sản xuất');
SET IDENTITY_INSERT BoPhan OFF;


SET IDENTITY_INSERT ChucDanh ON;

INSERT INTO ChucDanh (ID, TenChucDanh) VALUES
(1, N'Trưởng phòng'),
(2, N'Phó phòng'),
(3, N'Kỹ thuật viên bảo trì bậc cao (Senior)'),
(4, N'Kỹ thuật viên bảo trì (Junior)'),
(5, N'Chuyên viên kiểm kê vật tư'),
(6, N'Kỹ sư cơ điện (M&E)'),
(7, N'Nhân viên hành chính'),
(8, N'Quản đốc phân xưởng');

SET IDENTITY_INSERT ChucDanh OFF;


alter table BaoTri 
add TinhTrangBaoTri nvarchar(50)

create table FogetPass 
(
	ID int primary key identity,
	Username varchar(50),
	OTP varchar(6),
	ExpiredTime DateTime default DATEADD(minute, 5, GETDATE()),

	foreign key (Username) references TaiKhoan(TenTaiKhoan)
)



update TaiKhoan
set MatKhau = '15e2b0d3c33891ebb0f1ef609ec419420c20e320ce94c65fbc8c3312448eb225'
where TenTaiKhoan = 'admin'

Delete from TaiKhoan 
where TenTaiKhoan = 'TruongPham'

update TaiKhoan 
set DuocXacThuc = 1
where DuocXacThuc = 0


select * from TaiKhoan
select * from NhanVien
select * from FogetPass
select * from ChiTietThietBi
select * from BaoTri
select * from BoPhan
select * from ChucDanh
select * from ThietBi
select * from BaoCaoSuaChua
select * from DichVuBaoTri

delete from BaoCaoSuaChua
delete from BaoTri
where IDBaoTri = 4
delete from NhanVien
drop table NhanVien
drop table BaoTri

update NhanVien
set TinhTrang = N'Đang rảnh'
where IDNhanVien = 3

drop table BaoTri

create table BaoTri 
(
    IDBaoTri int primary key identity,
    IDNhanVien int,
    NgayBaoTri DateTime, 
    GhiChu nvarchar(100),
    DoUuTien nvarchar(20), 
    TinhTrangBaoTri nvarchar(50),

    foreign key (IDNhanVien) references NhanVien(IDNhanVien)
);

create table ChiTietBaoTri
(
    IDBaoTri int,
    IDThietBi int,
    SoSeri varchar(10),
    IDDichVu int,
    GhiChuThietBi nvarchar(255),
    TienDo nvarchar(50),
    KetQua nvarchar(255),

    primary key (IDBaoTri, IDThietBi, SoSeri),

    foreign key (IDBaoTri) references BaoTri(IDBaoTri),
    foreign key (IDThietBi, SoSeri) references ChiTietThietBi(IDThietBi, SoSeri),
    foreign key (IDDichVu) references DichVuBaoTri(IDDichVu)
);