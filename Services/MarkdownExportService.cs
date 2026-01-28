using Microsoft.EntityFrameworkCore;
using OopsReviewCenter.Data;
using OopsReviewCenter.Models;
using System.Text;

namespace OopsReviewCenter.Services;

public class MarkdownExportService
{
    private readonly ApplicationDbContext _context;

    public MarkdownExportService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> ExportIncidentToMarkdown(int incidentId)
    {
        var incident = await _context.Incidents
            .Include(i => i.TimelineEvents)
            .Include(i => i.ActionItems)
            .Include(i => i.IncidentTags)
            .ThenInclude(it => it.Tag)
            .FirstOrDefaultAsync(i => i.Id == incidentId);

        if (incident == null)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        
        // Header
        sb.AppendLine($"# Incident Report: {incident.Title}");
        sb.AppendLine();
        
        // Metadata
        sb.AppendLine("## Metadata");
        sb.AppendLine($"- **ID**: {incident.Id}");
        sb.AppendLine($"- **Severity**: {incident.Severity}");
        sb.AppendLine($"- **Status**: {incident.Status}");
        sb.AppendLine($"- **Occurred At**: {incident.OccurredAt:yyyy-MM-dd HH:mm:ss UTC}");
        sb.AppendLine($"- **Created At**: {incident.CreatedAt:yyyy-MM-dd HH:mm:ss UTC}");
        if (incident.ResolvedAt.HasValue)
        {
            sb.AppendLine($"- **Resolved At**: {incident.ResolvedAt.Value:yyyy-MM-dd HH:mm:ss UTC}");
        }
        
        // Tags
        if (incident.IncidentTags.Any())
        {
            sb.AppendLine($"- **Tags**: {string.Join(", ", incident.IncidentTags.Select(it => it.Tag.Name))}");
        }
        sb.AppendLine();
        
        // Description
        sb.AppendLine("## Description");
        sb.AppendLine(incident.Description);
        sb.AppendLine();
        
        // Impact
        if (!string.IsNullOrEmpty(incident.Impact))
        {
            sb.AppendLine("## Impact");
            sb.AppendLine(incident.Impact);
            sb.AppendLine();
        }
        
        // Timeline
        if (incident.TimelineEvents.Any())
        {
            sb.AppendLine("## Timeline");
            foreach (var evt in incident.TimelineEvents.OrderBy(e => e.OccurredAt))
            {
                sb.AppendLine($"### {evt.OccurredAt:yyyy-MM-dd HH:mm:ss UTC}");
                sb.AppendLine(evt.Description);
                if (!string.IsNullOrEmpty(evt.Author))
                {
                    sb.AppendLine($"*Author: {evt.Author}*");
                }
                sb.AppendLine();
            }
        }
        
        // Root Cause
        if (!string.IsNullOrEmpty(incident.RootCause))
        {
            sb.AppendLine("## Root Cause");
            sb.AppendLine(incident.RootCause);
            sb.AppendLine();
        }
        
        // Action Items
        if (incident.ActionItems.Any())
        {
            sb.AppendLine("## Action Items");
            foreach (var action in incident.ActionItems.OrderBy(a => a.Priority))
            {
                var checkbox = action.Status == "Completed" ? "[x]" : "[ ]";
                sb.AppendLine($"{checkbox} **{action.Title}** - {action.Status} (Priority: {action.Priority})");
                if (!string.IsNullOrEmpty(action.Description))
                {
                    sb.AppendLine($"   - {action.Description}");
                }
                if (!string.IsNullOrEmpty(action.AssignedTo))
                {
                    sb.AppendLine($"   - Assigned to: {action.AssignedTo}");
                }
                if (action.DueDate.HasValue)
                {
                    sb.AppendLine($"   - Due: {action.DueDate.Value:yyyy-MM-dd}");
                }
                sb.AppendLine();
            }
        }
        
        return sb.ToString();
    }
}
