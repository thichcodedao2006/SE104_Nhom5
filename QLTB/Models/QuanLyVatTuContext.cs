using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QLTB.Models;

public partial class QuanLyVatTuContext : DbContext
{
    public QuanLyVatTuContext()
    {
    }

    public QuanLyVatTuContext(DbContextOptions<QuanLyVatTuContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BaoCaoSuaChua> BaoCaoSuaChuas { get; set; }

    public virtual DbSet<BaoTri> BaoTris { get; set; }

    public virtual DbSet<BoPhan> BoPhans { get; set; }

    public virtual DbSet<ChiTietThietBi> ChiTietThietBis { get; set; }

    public virtual DbSet<ChucDanh> ChucDanhs { get; set; }

    public virtual DbSet<DichVuBaoTri> DichVuBaoTris { get; set; }

    public virtual DbSet<FogetPass> FogetPasses { get; set; }

    public virtual DbSet<NhanVien> NhanViens { get; set; }

    public virtual DbSet<PhongBan> PhongBans { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<ThietBi> ThietBis { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("workstation id=QuanLyVatTu.mssql.somee.com;packet size=4096;user id=thichcodedao_SQLLogin_1;pwd=sb4659th3x;data source=QuanLyVatTu.mssql.somee.com;persist security info=False;initial catalog=QuanLyVatTu;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaoCaoSuaChua>(entity =>
        {
            entity.HasKey(e => e.IdbaoCao).HasName("PK__BaoCaoSu__BC216EF001A0D302");

            entity.ToTable("BaoCaoSuaChua");

            entity.Property(e => e.IdbaoCao).HasColumnName("IDBaoCao");
            entity.Property(e => e.GhiChu).HasMaxLength(100);
            entity.Property(e => e.IdthietBi).HasColumnName("IDThietBi");
            entity.Property(e => e.NgayBaoCao).HasColumnType("datetime");
            entity.Property(e => e.Sdt)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("SDT");
            entity.Property(e => e.SoSeri)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.TenNguoiBaoCao).HasMaxLength(50);

            entity.HasOne(d => d.ChiTietThietBi).WithMany(p => p.BaoCaoSuaChuas)
                .HasForeignKey(d => new { d.IdthietBi, d.SoSeri })
                .HasConstraintName("FK__BaoCaoSuaChua__47DBAE45");
        });

        modelBuilder.Entity<BaoTri>(entity =>
        {
            entity.HasKey(e => e.IdbaoTri).HasName("PK__BaoTri__BBE08E29462CDAFE");

            entity.ToTable("BaoTri");

            entity.Property(e => e.IdbaoTri).HasColumnName("IDBaoTri");
            entity.Property(e => e.GhiChu).HasMaxLength(100);
            entity.Property(e => e.IddichVu).HasColumnName("IDDichVu");
            entity.Property(e => e.IdnhanVien).HasColumnName("IDNhanVien");
            entity.Property(e => e.IdthietBi).HasColumnName("IDThietBi");
            entity.Property(e => e.NgayBaoTri).HasColumnType("datetime");
            entity.Property(e => e.SoSeri)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.TinhTrangBaoTri).HasMaxLength(50);

            entity.HasOne(d => d.IddichVuNavigation).WithMany(p => p.BaoTris)
                .HasForeignKey(d => d.IddichVu)
                .HasConstraintName("FK__BaoTri__IDDichVu__619B8048");

            entity.HasOne(d => d.IdnhanVienNavigation).WithMany(p => p.BaoTris)
                .HasForeignKey(d => d.IdnhanVien)
                .HasConstraintName("FK__BaoTri__IDNhanVi__628FA481");

            entity.HasOne(d => d.ChiTietThietBi).WithMany(p => p.BaoTris)
                .HasForeignKey(d => new { d.IdthietBi, d.SoSeri })
                .HasConstraintName("FK__BaoTri__60A75C0F");
        });

        modelBuilder.Entity<BoPhan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BoPhan__3214EC27020524A3");

            entity.ToTable("BoPhan");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.TenBoPhan).HasMaxLength(50);
        });

        modelBuilder.Entity<ChiTietThietBi>(entity =>
        {
            entity.HasKey(e => new { e.IdthietBi, e.SoSeri }).HasName("PK__ChiTietT__03FF6CEE6B4B42DC");

            entity.ToTable("ChiTietThietBi");

            entity.Property(e => e.IdthietBi).HasColumnName("IDThietBi");
            entity.Property(e => e.SoSeri)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.IdphongBan).HasColumnName("IDPhongBan");
            entity.Property(e => e.TinhTrang)
                .HasMaxLength(50)
                .HasDefaultValue("T?t");

            entity.HasOne(d => d.IdphongBanNavigation).WithMany(p => p.ChiTietThietBis)
                .HasForeignKey(d => d.IdphongBan)
                .HasConstraintName("FK__ChiTietTh__IDPho__3E52440B");

            entity.HasOne(d => d.IdthietBiNavigation).WithMany(p => p.ChiTietThietBis)
                .HasForeignKey(d => d.IdthietBi)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietTh__IDThi__3D5E1FD2");
        });

        modelBuilder.Entity<ChucDanh>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChucDanh__3214EC27B8BA1EA6");

            entity.ToTable("ChucDanh");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.TenChucDanh).HasMaxLength(50);
        });

        modelBuilder.Entity<DichVuBaoTri>(entity =>
        {
            entity.HasKey(e => e.IddichVu).HasName("PK__DichVuBa__C0C95928E57B0A38");

            entity.ToTable("DichVuBaoTri");

            entity.Property(e => e.IddichVu).HasColumnName("IDDichVu");
            entity.Property(e => e.GiaDichVu).HasDefaultValue(0.0);
            entity.Property(e => e.TenDichVu).HasMaxLength(100);
            entity.Property(e => e.Unit).HasDefaultValue(0);
            entity.Property(e => e.Value).HasDefaultValue(0);
        });

        modelBuilder.Entity<FogetPass>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__FogetPas__3214EC27A5F36EF8");

            entity.ToTable("FogetPass");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ExpiredTime)
                .HasDefaultValueSql("(dateadd(minute,(5),getdate()))")
                .HasColumnType("datetime");
            entity.Property(e => e.Otp)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("OTP");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.UsernameNavigation).WithMany(p => p.FogetPasses)
                .HasForeignKey(d => d.Username)
                .HasConstraintName("FK__FogetPass__Usern__5165187F");
        });

        modelBuilder.Entity<NhanVien>(entity =>
        {
            entity.HasKey(e => e.IdnhanVien).HasName("PK__NhanVien__7AC2D9F78D7F890F");

            entity.ToTable("NhanVien");

            entity.Property(e => e.IdnhanVien).HasColumnName("IDNhanVien");
            entity.Property(e => e.ChuyenMon).HasMaxLength(100);
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.HoTen).HasMaxLength(50);
            entity.Property(e => e.IdboPhan).HasColumnName("IDBoPhan");
            entity.Property(e => e.IdchucDanh).HasColumnName("IDChucDanh");
            entity.Property(e => e.Sdt)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("SDT");
            entity.Property(e => e.TinhTrang).HasMaxLength(50);

            entity.HasOne(d => d.IdboPhanNavigation).WithMany(p => p.NhanViens)
                .HasForeignKey(d => d.IdboPhan)
                .HasConstraintName("FK__NhanVien__IDBoPh__5CD6CB2B");

            entity.HasOne(d => d.IdchucDanhNavigation).WithMany(p => p.NhanViens)
                .HasForeignKey(d => d.IdchucDanh)
                .HasConstraintName("FK__NhanVien__IDChuc__5DCAEF64");
        });

        modelBuilder.Entity<PhongBan>(entity =>
        {
            entity.HasKey(e => e.Idphong).HasName("PK__PhongBan__81CB1152E35AE360");

            entity.ToTable("PhongBan");

            entity.Property(e => e.Idphong).HasColumnName("IDPhong");
            entity.Property(e => e.TenPhong).HasMaxLength(100);
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.TenTaiKhoan).HasName("PK__TaiKhoan__B106EAF9C21AA190");

            entity.ToTable("TaiKhoan");

            entity.Property(e => e.TenTaiKhoan)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MatKhau)
                .HasMaxLength(80)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ThietBi>(entity =>
        {
            entity.HasKey(e => e.IdthietBi).HasName("PK__ThietBi__1376CB7BB4726DAC");

            entity.ToTable("ThietBi");

            entity.Property(e => e.IdthietBi).HasColumnName("IDThietBi");
            entity.Property(e => e.DonViSanXuat).HasMaxLength(100);
            entity.Property(e => e.LoaiThietBi).HasMaxLength(100);
            entity.Property(e => e.NgayNhapThietBi)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TenThietBi).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
