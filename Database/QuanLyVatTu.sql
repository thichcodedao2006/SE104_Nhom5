
create table ThietBi
(
	IDThietBi int primary key identity,
	TenThietBi nvarchar(100),
	LoaiThietBi nvarchar(100),
	BaoHanhDinhKy int, -- tính theo tháng 
	DonViThoiGian int, -- 0: Phút 1: Giờ 2: Ngày 3: Tháng 4: Năm 
	DonViSanXuat nvarchar(100),
	SoLuong int, -- số lượng tổng (số lượng còn lại hay số lượng đang sử dụng sẽ được tính dựa trên cái này)
	Gia float -- giá (trên mỗi thiết bị) 
	NgayNhapThietBi DateTime default GetDate(),
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

	Primary key(IDThietBi, SoSeri),

	Foreign key (IDThietBi) references ThietBi(IDThietBi),
	Foreign key (IDPhongBan) references PhongBan(IDPhong)
)

alter table ChiTietThietBi
drop constraint DF__ChiTietTh__NgayN__3C69FB99
alter table ChiTietThietBi
drop column NgayNhapThietBi

create table NhanVien 
(
	IDNhanVien int primary key identity,
	HoTen nvarchar(50),
	SDT varchar(10),
	Email varchar(50),
	ChuyenMon nvarchar(100) -- Quan ly la chuyen mon rieng ,
	TinhTrang nvarchar(50), -- đang rảnh, đang bận, nghỉ việc, ...
)




create table TaiKhoan
(
	TenTaiKhoan varchar(50) primary key,
	MatKhau varchar(50),
	Email varchar(50), -- không được sử dụng trùng email 
	LoaiTaiKhoan int, --0: Giam doc, 1: Quan ly kho, 2: Nhan vien binh thuong 
	DuocXacThuc int, --0: chua duoc, 1: duoc 
	Avatar varchar(100)
)

alter table TaiKhoan 
add Avatar varchar(100)

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
	GhiChu nvarchar(100),

	foreign key (IDThietBi, SoSeri) references ChiTietThietBi(IDThietBi, SoSeri)
)

create table BaoTri 
(
	IDBaoTri int primary key identity,
	IDThietBi int,
	SoSeri varchar(10),
	IDDichVu int,
	IDNhanVien int,
	NgayBaoTri DateTime,
	GhiChu nvarchar(100),
	TinhTrangBaoTri nvarchar(50), 

	foreign key (IDThietBi, SoSeri) references ChiTietThietBi(IDThietBi, SoSeri),
	foreign key (IDDichVu) references DichVuBaoTri(IDDichVu),
	foreign key (IDNhanVien) references NhanVien(IDNhanVien)
)


------- Insert -------
INSERT INTO PhongBan (TenPhong, ViTri) VALUES 
(N'Phòng Hành chính - Nhân sự', 1), -- Tầng 1 (Tiện cho ứng viên, tiếp tân)
(N'Phòng Kế toán - Tài chính', 2),  -- Tầng 2
(N'Phòng Kinh doanh (Sales)', 3),   -- Tầng 3
(N'Phòng Marketing', 3),            -- Tầng 3 (Chung tầng với Sales để dễ phối hợp)
(N'Phòng Công nghệ thông tin (IT)', 4), -- Tầng 4
(N'Phòng Nghiên cứu & Phát triển (R&D)', 5), -- Tầng 5 (Cần yên tĩnh)
(N'Ban Giám đốc', 6);               -- Tầng 6 (Tầng cao nhất)

insert into NhanVien 
values
(
	'Phạm Đan Trường', '0358002806', '24521898@gm.uit.edu.vn', N'Phần mềm', N'Đang rảnh'
)


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
where TenTaiKhoan = 'TruongPham123'

update TaiKhoan 
set Email ='24521898@gm.uit.edu.vn'
where TenTaiKhoan = 'admin'

select * from TaiKhoan
select * from NhanVien
select * from FogetPass