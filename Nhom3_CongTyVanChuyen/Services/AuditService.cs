using System;

namespace Nhom3_CongTyVanChuyen.Services
{
    public class AuditService
    {
        // Người dùng hiện tại
        public string GetCurrentUser()
        {
            return "vietvietabcfull"; // Người dùng hiện tại
        }

        // Thời gian hiện tại
        public DateTime GetCurrentTime()
        {
            return DateTime.Parse("2025-05-09 11:40:07"); // Thời gian hiện tại
        }
    }
}