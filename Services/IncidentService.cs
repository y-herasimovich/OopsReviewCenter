using Microsoft.EntityFrameworkCore;
using OopsReviewCenter.Data;
using OopsReviewCenter.Models;

namespace OopsReviewCenter.Services;

/// <summary>
/// Service for managing incidents with automatic timeline logging
/// </summary>
public class IncidentService
{
    private readonly ApplicationDbContext _dbContext;

    public IncidentService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Get incidents with optional status filtering
    /// </summary>
    public async Task<List<Incident>> GetIncidentsAsync(string? statusFilter = null)
    {
        var query = _dbContext.Incidents
            .Include(i => i.IncidentTags)
            .ThenInclude(it => it.Tag)
            .OrderByDescending(i => i.OccurredAt);

        if (!string.IsNullOrEmpty(statusFilter))
        {
            return await query.Where(i => i.Status == statusFilter).ToListAsync();
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// Get paged incidents with filtering and sorting
    /// </summary>
    public async Task<PagedIncidentsResult> GetIncidentsPagedAsync(
        int page = 1,
        int pageSize = 25,
        string? statusFilter = null,
        string? severityFilter = null,
        List<int>? tagIds = null,
        bool showResolved = false)
    {
        var query = _dbContext.Incidents
            .Include(i => i.IncidentTags)
            .ThenInclude(it => it.Tag)
            .AsQueryable();

        // Apply filters
        // Default: hide Resolved incidents unless explicitly requested
        if (!showResolved && string.IsNullOrEmpty(statusFilter))
        {
            query = query.Where(i => i.Status != "Resolved");
        }
        else if (!string.IsNullOrEmpty(statusFilter))
        {
            // If status filter is set, use it (overrides showResolved)
            query = query.Where(i => i.Status == statusFilter);
        }

        if (!string.IsNullOrEmpty(severityFilter))
        {
            query = query.Where(i => i.Severity == severityFilter);
        }

        if (tagIds != null && tagIds.Any())
        {
            // Filter incidents that have ANY of the selected tags
            query = query.Where(i => i.IncidentTags.Any(it => tagIds.Contains(it.TagId)));
        }

        // Get total count before paging
        var totalCount = await query.CountAsync();

        // Apply sorting with explicit ordering
        // Primary: Severity (Critical > High > Medium > Low)
        // Secondary: Status (Open/Investigating before Resolved/Closed)
        // Tertiary: OccurredAt (most recent first)
        query = query
            .OrderBy(i => i.Severity == "Critical" ? 0 :
                          i.Severity == "High" ? 1 :
                          i.Severity == "Medium" ? 2 :
                          i.Severity == "Low" ? 3 : 4)
            .ThenBy(i => i.Status == "Open" ? 0 :
                         i.Status == "Investigating" ? 1 :
                         i.Status == "Resolved" ? 2 :
                         i.Status == "Closed" ? 3 : 4)
            .ThenByDescending(i => i.OccurredAt);

        // Apply paging
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedIncidentsResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Get a single incident by ID with all related data
    /// </summary>
    public async Task<Incident?> GetIncidentByIdAsync(int incidentId)
    {
        return await _dbContext.Incidents
            .Include(i => i.TimelineEvents)
            .Include(i => i.ActionItems)
            .Include(i => i.IncidentTags)
            .ThenInclude(it => it.Tag)
            .Include(i => i.ResolvedByUser)
            .FirstOrDefaultAsync(i => i.Id == incidentId);
    }

    /// <summary>
    /// Update incident information with automatic timeline logging
    /// </summary>
    public async Task<bool> UpdateIncidentInfoAsync(
        int incidentId,
        string title,
        string description,
        string status,
        string severity,
        string? rootCause,
        string? impact,
        int? userId,
        string? username)
    {
        var incident = await _dbContext.Incidents
            .Include(i => i.TimelineEvents)
            .FirstOrDefaultAsync(i => i.Id == incidentId);

        if (incident == null)
        {
            return false;
        }

        var changes = new List<string>();
        var author = username ?? $"User {userId}";

        // Track changes and update fields
        if (incident.Title != title)
        {
            changes.Add($"Title changed from '{incident.Title}' to '{title}'");
            incident.Title = title;
        }

        if (incident.Description != description)
        {
            changes.Add("Description updated");
            incident.Description = description;
        }

        if (incident.Status != status)
        {
            changes.Add($"Status: {incident.Status} → {status}");
            incident.Status = status;

            // If status is Resolved or Closed, set ResolvedAt and ResolvedByUserId (if userId provided)
            if ((status == "Resolved" || status == "Closed") && !incident.ResolvedAt.HasValue)
            {
                incident.ResolvedAt = DateTime.UtcNow;
                if (userId.HasValue)
                {
                    incident.ResolvedByUserId = userId;
                }
            }
            // If status changes back to Open/Investigating, clear resolution info
            else if (status == "Open" || status == "Investigating")
            {
                incident.ResolvedAt = null;
                incident.ResolvedByUserId = null;
            }
        }

        if (incident.Severity != severity)
        {
            changes.Add($"Severity: {incident.Severity} → {severity}");
            incident.Severity = severity;
        }

        if (incident.RootCause != rootCause)
        {
            changes.Add("Root Cause updated");
            incident.RootCause = rootCause;
        }

        if (incident.Impact != impact)
        {
            changes.Add("Impact updated");
            incident.Impact = impact;
        }

        // Create timeline entries for changes
        if (changes.Any())
        {
            foreach (var change in changes)
            {
                var timelineEvent = new TimelineEvent
                {
                    IncidentId = incidentId,
                    OccurredAt = DateTime.UtcNow,
                    Description = change,
                    Author = author
                };
                _dbContext.TimelineEvents.Add(timelineEvent);
            }

            await _dbContext.SaveChangesAsync();
        }

        return true;
    }

    /// <summary>
    /// Update incident tags with automatic timeline logging
    /// </summary>
    public async Task<bool> UpdateIncidentTagsAsync(
        int incidentId,
        List<int> tagIds,
        int? userId,
        string? username)
    {
        var incident = await _dbContext.Incidents
            .Include(i => i.IncidentTags)
            .ThenInclude(it => it.Tag)
            .FirstOrDefaultAsync(i => i.Id == incidentId);

        if (incident == null)
        {
            return false;
        }

        var author = username ?? $"User {userId}";
        var currentTagIds = incident.IncidentTags.Select(it => it.TagId).ToList();

        // Find added and removed tags
        var addedTagIds = tagIds.Except(currentTagIds).ToList();
        var removedTagIds = currentTagIds.Except(tagIds).ToList();

        if (!addedTagIds.Any() && !removedTagIds.Any())
        {
            return true; // No changes
        }

        // Remove old tags
        var tagsToRemove = incident.IncidentTags
            .Where(it => removedTagIds.Contains(it.TagId))
            .ToList();

        foreach (var tagToRemove in tagsToRemove)
        {
            _dbContext.IncidentTags.Remove(tagToRemove);
        }

        // Add new tags
        foreach (var tagId in addedTagIds)
        {
            var incidentTag = new IncidentTag
            {
                IncidentId = incidentId,
                TagId = tagId
            };
            _dbContext.IncidentTags.Add(incidentTag);
        }

        // Create timeline entry
        var changes = new List<string>();

        if (addedTagIds.Any())
        {
            var addedTags = await _dbContext.Tags
                .Where(t => addedTagIds.Contains(t.Id))
                .Select(t => t.Name)
                .ToListAsync();
            changes.Add($"Tags added: {string.Join(", ", addedTags)}");
        }

        if (removedTagIds.Any())
        {
            var removedTags = incident.IncidentTags
                .Where(it => removedTagIds.Contains(it.TagId))
                .Select(it => it.Tag.Name)
                .ToList();
            changes.Add($"Tags removed: {string.Join(", ", removedTags)}");
        }

        if (changes.Any())
        {
            var timelineEvent = new TimelineEvent
            {
                IncidentId = incidentId,
                OccurredAt = DateTime.UtcNow,
                Description = string.Join("; ", changes),
                Author = author
            };
            _dbContext.TimelineEvents.Add(timelineEvent);
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Get all available tags
    /// </summary>
    public async Task<List<Tag>> GetAllTagsAsync()
    {
        return await _dbContext.Tags
            .OrderBy(t => t.Name)
            .ToListAsync();
    }
}
