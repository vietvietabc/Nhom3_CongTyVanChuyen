using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Nhom3_CongTyVanChuyen.Data;

namespace Nhom3_CongTyVanChuyen
{
    public class MyDbContextFactory : IDesignTimeDbContextFactory<MyDbContext>
    {
        public MyDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MyDbContext>();

            // 👉 Thay đổi chuỗi kết nối theo máy của bạn nếu cần
            optionsBuilder.UseSqlServer("Data Source=TRANDINHVIET\\TRANDINHVIET;Initial Catalog=CongTyVanChuyen;Integrated Security=True;Trust Server Certificate=True");

            return new MyDbContext(optionsBuilder.Options);
        }
    }
}
