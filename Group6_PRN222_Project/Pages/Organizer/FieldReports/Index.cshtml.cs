using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Project_PRN222.Helpers;

namespace Group6_PRN222_Project.Pages.Organizer.FieldReports
{
    public class MarketingReportDetailVm
    {
        public MarketingFieldReportPayload Payload { get; set; } = new();
        public Dictionary<int, string> TaskNames { get; set; } = new();
    }

    public class IndexModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public IndexModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        public IList<FieldReport> Reports { get; set; } = new List<FieldReport>();

        public Dictionary<int, MarketingReportDetailVm> MarketingDetails { get; set; } = new();

        public async System.Threading.Tasks.Task<IActionResult> OnGetAsync()
        {
            if (!SessionHelper.IsOrganizer(HttpContext.Session) && !SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Admin/Login");

            var selectedId = HttpContext.Session.GetInt32("SelectedEventId");

            var query = _context.FieldReports
                .Include(r => r.Event)
                .Include(r => r.Staff)
                .Where(r => r.Status != "Draft")
                .AsQueryable();

            if (selectedId.HasValue)
                query = query.Where(r => r.EventId == selectedId);

            Reports = await query.OrderByDescending(r => r.ReportTime).ToListAsync();

            var marketing = Reports.Where(r => r.ReportType == "Marketing").ToList();
            var allTaskIds = marketing
                .SelectMany(r => MarketingFieldReportContent.Parse(r.Content).TaskEstimates.Select(t => t.TaskId))
                .Distinct()
                .ToList();

            var taskNameById = allTaskIds.Count == 0
                ? new Dictionary<int, string>()
                : await _context.Tasks.AsNoTracking()
                    .Where(t => allTaskIds.Contains(t.TaskId))
                    .ToDictionaryAsync(t => t.TaskId, t => t.TaskName ?? $"Task #{t.TaskId}");

            MarketingDetails = new Dictionary<int, MarketingReportDetailVm>();
            foreach (var r in marketing)
            {
                var payload = MarketingFieldReportContent.Parse(r.Content);
                MarketingDetails[r.ReportId] = new MarketingReportDetailVm
                {
                    Payload = payload,
                    TaskNames = payload.TaskEstimates
                        .Select(e => e.TaskId)
                        .Distinct()
                        .ToDictionary(
                            id => id,
                            id => taskNameById.GetValueOrDefault(id, $"Task #{id}"))
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(int reportId)
        {
            if (!SessionHelper.IsOrganizer(HttpContext.Session) && !SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Admin/Login");

            var report = await _context.FieldReports
                .Include(r => r.Event)
                .FirstOrDefaultAsync(r => r.ReportId == reportId);

            if (report != null)
            {
                report.Status = "Approved";

                // Automation for Equipment Rental Request
                if (report.ReportType == "EquipmentRentalRequest" && !string.IsNullOrEmpty(report.Content))
                {
                    try 
                    {
                        // Regex to parse EquipmentId and Quantity
                        var idMatch = System.Text.RegularExpressions.Regex.Match(report.Content, @"\(ID:(\d+)\)");
                        var qtyMatch = System.Text.RegularExpressions.Regex.Match(report.Content, @"Số lượng cần thuê: (\d+)");

                        if (idMatch.Success && qtyMatch.Success)
                        {
                            int equipId = int.Parse(idMatch.Groups[1].Value);
                            int qty = int.Parse(qtyMatch.Groups[1].Value);

                            // 1. Ensure a default Vendor exists
                            var vendor = await _context.Vendors.FirstOrDefaultAsync() 
                                         ?? new Vendor { VendorName = "Default Partner", Phone = "0123456789", Address = "Hà Nội", IsActive = true };
                            if (vendor.VendorId == 0) {
                                _context.Vendors.Add(vendor);
                                await _context.SaveChangesAsync();
                            }

                            // 2. Create Outsource Rental record
                            var outsource = new OutsourceRental
                            {
                                EventId = report.EventId,
                                EquipmentId = equipId,
                                VendorId = vendor.VendorId,
                                RentQuantity = qty,
                                ExpectedReturnDate = report.Event?.EndDate?.AddDays(1) ?? DateTime.Now.AddDays(7),
                                Status = "Pending"
                            };
                            _context.OutsourceRentals.Add(outsource);

                            // 3. Update EventEquipment status
                            var evtEquip = await _context.EventEquipments
                                .FirstOrDefaultAsync(ee => ee.EventId == report.EventId && ee.EquipmentId == equipId);
                            
                            if (evtEquip != null)
                            {
                                evtEquip.Status = "Approved";
                                evtEquip.ApprovedQuantity = evtEquip.RequestedQuantity; 
                                // Add to note
                                evtEquip.Note = (evtEquip.Note ?? "") + $" [Approved via Report #{reportId}]";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        TempData["Error"] = "Lỗi xử lý tự động: " + ex.Message;
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Báo cáo #{reportId} đã được duyệt và hệ thống đã tự động tạo phiếu thuê ngoài.";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy báo cáo.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(int reportId)
        {
            if (!SessionHelper.IsOrganizer(HttpContext.Session) && !SessionHelper.IsAdmin(HttpContext.Session))
                return RedirectToPage("/Admin/Login");

            var report = await _context.FieldReports.FindAsync(reportId);
            if (report != null)
            {
                report.Status = "Rejected";
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Báo cáo #{reportId} đã bị từ chối.";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy báo cáo.";
            }
            return RedirectToPage();
        }
    }
}
