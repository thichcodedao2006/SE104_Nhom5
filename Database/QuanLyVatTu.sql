
create table ThietBi
(
	IDThietBi int primary key identity,
	TenThietBi nvarchar(100),
	LoaiThietBi nvarchar(100),
	BaoHanhDinhKy int, -- tính theo tháng 
	DonViSanXuat nvarchar(100),
	SoLuong int, -- số lượng tổng (số lượng còn lại hay số lượng đang sử dụng sẽ được tính dựa trên cái này)
	Gia float -- giá (trên mỗi thiết bị) 
)

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
	TinhTrang nvarchar(50) default 'Tốt', 
	NgayNhapThietBi DateTime default GetDate(),
	IDPhongBan int, -- thiết bị đang được sử dụng ở phòng ban nào (mặc định ở phòng kho) 

	Primary key(IDThietBi, SoSeri),

	Foreign key (IDThietBi) references ThietBi(IDThietBi),
	Foreign key (IDPhongBan) references PhongBan(IDPhong)
)


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
)

create table DichVuBaoTri 
(
	IDDichVu int primary key identity,
	TenDichVu nvarchar(100),
	GiaDichVu float default 0,
)

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

	foreign key (IDThietBi, SoSeri) references ChiTietThietBi(IDThietBi, SoSeri),
	foreign key (IDDichVu) references DichVuBaoTri(IDDichVu),
	foreign key (IDNhanVien) references NhanVien(IDNhanVien)
)