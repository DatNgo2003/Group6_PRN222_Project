namespace Group6_PRN222_Project.Helpers;

/// <summary>
/// Map RoleName trong DB (có thể ngắn gọn: Staff, Marketing…) sang role nội bộ mà app và JWT dùng.
/// </summary>
public static class InternalRoleResolver
{
    public const string Admin = "Admin";
    public const string Organizer = "Organizer";
    public const string StaffSecurity = "Staff(Security)";
    public const string StaffMkt = "Staff(MKT)";
    public const string StaffLogistics = "Staff(Logistics)";

    /// <summary>
    /// Trả về true nếu có thể đăng nhập nội bộ; <paramref name="appRole"/> là giá trị ghi vào JWT / session.
    /// </summary>
    public static bool TryResolveAppRole(string? roleNameFromDb, out string appRole)
    {
        appRole = "";
        if (string.IsNullOrWhiteSpace(roleNameFromDb))
            return false;

        var raw = roleNameFromDb.Trim();

        // Đã đúng chuẩn app
        if (raw is Admin or Organizer or StaffSecurity or StaffMkt or StaffLogistics)
        {
            appRole = raw;
            return true;
        }

        // Bỏ khoảng trắng thừa quanh dấu ngoặc: "Staff ( Security )"
        var compact = string.Concat(raw.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (string.Equals(compact, "Staff(Security)", StringComparison.OrdinalIgnoreCase))
        {
            appRole = StaffSecurity;
            return true;
        }

        if (string.Equals(compact, "Staff(MKT)", StringComparison.OrdinalIgnoreCase))
        {
            appRole = StaffMkt;
            return true;
        }

        if (string.Equals(compact, "Staff(Logistics)", StringComparison.OrdinalIgnoreCase))
        {
            appRole = StaffLogistics;
            return true;
        }

        // Tên role ngắn thường gặp trong DB môn học / script SQL
        if (string.Equals(raw, "Staff", StringComparison.OrdinalIgnoreCase))
        {
            appRole = StaffSecurity;
            return true;
        }

        if (string.Equals(raw, "Security", StringComparison.OrdinalIgnoreCase))
        {
            appRole = StaffSecurity;
            return true;
        }

        if (string.Equals(raw, "Marketing", StringComparison.OrdinalIgnoreCase)
            || string.Equals(raw, "MKT", StringComparison.OrdinalIgnoreCase)
            || string.Equals(raw, "Marketer", StringComparison.OrdinalIgnoreCase))
        {
            appRole = StaffMkt;
            return true;
        }

        if (string.Equals(raw, "Logistics", StringComparison.OrdinalIgnoreCase)
            || string.Equals(raw, "Hậu cần", StringComparison.OrdinalIgnoreCase))
        {
            appRole = StaffLogistics;
            return true;
        }

        return false;
    }
}
