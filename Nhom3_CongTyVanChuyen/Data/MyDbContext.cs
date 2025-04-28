using Microsoft.EntityFrameworkCore;
namespace Nhom3_CongTyVanChuyen.Data
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        public DbSet<DonHang> DonHangs { get; set; }
        public DbSet<KhachHang> KhachHangs { get; set; }
        public DbSet<NguoiNhan> NguoiNhans { get; set; }
        public DbSet<NhanVien> NhanViens { get; set; }
        public DbSet<SoNha> SoNhas { get; set; }
        public DbSet<PhuongXa> PhuongXas { get; set; }
        public DbSet<QuanHuyen> QuanHuyens { get; set; }
        public DbSet<TinhThanhPho> TinhThanhPhos { get; set; }
        public DbSet<VaiTro> VaiTros { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public DbSet<HangHoa> HangHoas { get; set; }
        public DbSet<DanhMuc> DanhMucs { get; set; }
        public DbSet<ThongBao> ThongBaos { get; set; }
        public DbSet<NhanVienThongBao> NhanVienThongBaos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình khóa chính tổng hợp
            modelBuilder.Entity<NhanVienThongBao>()
                .HasKey(nt => new { nt.MaNhanVien, nt.MaThongBao });

            modelBuilder.Entity<ChiTietDonHang>()
                .HasKey(c => c.MaChiTietDonHang); // Nếu là string thì không cần composite key

            // Cấu hình tránh multiple cascade paths
            modelBuilder.Entity<NguoiNhan>()
                .HasOne(n => n.KhachHang)
                .WithMany(k => k.NguoiNhans)
                .HasForeignKey(n => n.MaKhachHang)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<NguoiNhan>()
                .HasOne(n => n.SoNha)
                .WithMany(s => s.NguoiNhans)
                .HasForeignKey(n => n.MaSoNha)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<KhachHang>()
                .HasOne(k => k.SoNha)
                .WithMany(s => s.KhachHangs)
                .HasForeignKey(k => k.MaSoNha)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NhanVien>()
                .HasOne(nv => nv.SoNha)
                .WithMany(sn => sn.NhanViens)
                .HasForeignKey(nv => nv.MaSoNha)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NhanVien>()
                .HasOne(nv => nv.VaiTro)
                .WithMany(vt => vt.NhanViens)
                .HasForeignKey(nv => nv.MaVaiTro)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SoNha>()
                .HasOne(sn => sn.PhuongXa)
                .WithMany(px => px.SoNhas)
                .HasForeignKey(sn => sn.MaPhuongXa)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PhuongXa>()
                .HasOne(px => px.QuanHuyen)
                .WithMany(qh => qh.PhuongXas)
                .HasForeignKey(px => px.MaQuanHuyen)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QuanHuyen>()
                .HasOne(qh => qh.TinhThanhPho)
                .WithMany(t => t.QuanHuyens)
                .HasForeignKey(qh => qh.MaTinhTP)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(ct => ct.DonHang)
                .WithMany(dh => dh.ChiTietDonHangs)
                .HasForeignKey(ct => ct.MaDonHang)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(ct => ct.HangHoa)
                .WithMany(hh => hh.ChiTietDonHangs)
                .HasForeignKey(ct => ct.MaHangHoa)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HangHoa>()
                .HasOne(hh => hh.DanhMuc)
                .WithMany(dm => dm.HangHoas)
                .HasForeignKey(hh => hh.MaDanhMuc)
                .OnDelete(DeleteBehavior.Restrict);

            // Nếu cần thêm cho DonHang -> KhachHang, NhanVien
            modelBuilder.Entity<DonHang>()
                .HasOne(d => d.KhachHang)
                .WithMany(kh => kh.DonHangs)
                .HasForeignKey(d => d.MaKhachHang)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DonHang>()
                .HasOne(d => d.NhanVien)
                .WithMany(nv => nv.DonHangs)
                .HasForeignKey(d => d.MaNhanVien)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
