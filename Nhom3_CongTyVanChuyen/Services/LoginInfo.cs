// File: Services/LoginInfo.cs
using System;

namespace Nhom3_CongTyVanChuyen.Services
{
    public static class LoginInfo
    {
        // Thông tin người dùng hiện tại
        public static string CurrentUserId { get; set; } = "vietvietabc";
        public static DateTime CurrentTime { get; set; } = DateTime.Parse("2025-05-09 11:16:06");
    }
}