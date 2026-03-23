using Group6_PRN222_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Group6_PRN222_Project.Pages.Organizer.Events
{
    public class TimelineModel : PageModel
    {
        private readonly ProjectPrn222Context _context;

        public TimelineModel(ProjectPrn222Context context)
        {
            _context = context;
        }

        public Event Event { get; set; } = default!;
        
        [BindProperty]
        public List<string> StepTimes { get; set; } = new List<string>();

        [BindProperty]
        public List<string> StepNames { get; set; } = new List<string>();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var ev = await _context.Events.FirstOrDefaultAsync(m => m.EventId == id);
            if (ev == null) return NotFound();

            Event = ev;
            
            var concept = ev.Concept ?? "";
            
            // Parsing logic
            if (!string.IsNullOrWhiteSpace(concept))
            {
                if (concept.Contains("::") || concept.Contains("|"))
                {
                    var items = concept.Split('|', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in items)
                    {
                        var parts = item.Split("::", 2);
                        if (parts.Length == 2)
                        {
                            StepTimes.Add(parts[0]);
                            StepNames.Add(parts[1]);
                        }
                        else
                        {
                            StepTimes.Add("");
                            StepNames.Add(item);
                        }
                    }
                }
                else
                {
                    // Legacy format: step1, step2
                    var items = concept.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in items)
                    {
                        StepTimes.Add("");
                        StepNames.Add(item.Trim());
                    }
                }
            }

            if(StepNames.Count == 0 && StepTimes.Count == 0)
            {
                StepNames.AddRange(new[] { "Draft", "Planning", "Execution", "Completed" });
                StepTimes.AddRange(new[] { "", "", "", "" });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateConceptAsync(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            var conceptParts = new List<string>();
            var count = Math.Min(StepTimes?.Count ?? 0, StepNames?.Count ?? 0);
            
            for(int i = 0; i < count; i++)
            {
                if(!string.IsNullOrWhiteSpace(StepNames[i]))
                {
                    conceptParts.Add($"{StepTimes[i]}::{StepNames[i]}");
                }
            }

            ev.Concept = string.Join("|", conceptParts);
            await _context.SaveChangesAsync();
            return RedirectToPage(new { id = id });
        }

        public async Task<IActionResult> OnPostChangeStatusAsync(int id, string stepName)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            ev.Status = stepName;
            await _context.SaveChangesAsync();
            return RedirectToPage(new { id = id });
        }
    }
}
