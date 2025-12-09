using System.Collections.Concurrent;
using ScraperService.Core.DTOs;

namespace ScraperService.Core.Services;

public interface IScrapeJobTracker
{
    string CreateJob(string? siteName, string? city);
    void UpdateJobStatus(string jobId, string status, int eventsFound = 0, string? error = null);
    void AddProcessedSite(string jobId, string siteName);
    void CompleteJob(string jobId, int totalEvents);
    void FailJob(string jobId, string error);
    ScrapeStatusDto? GetJobStatus(string jobId);
    IEnumerable<ScrapeStatusDto> GetRecentJobs(int count = 10);
}

public class ScrapeJobTracker : IScrapeJobTracker
{
    private readonly ConcurrentDictionary<string, ScrapeJob> _jobs = new();
    private const int MaxJobsToKeep = 100;

    public string CreateJob(string? siteName, string? city)
    {
        CleanupOldJobs();

        var jobId = Guid.NewGuid().ToString("N")[..8];
        var job = new ScrapeJob
        {
            JobId = jobId,
            Status = "Queued",
            StartedAt = DateTime.UtcNow,
            SiteName = siteName,
            City = city
        };

        _jobs.TryAdd(jobId, job);
        return jobId;
    }

    public void UpdateJobStatus(string jobId, string status, int eventsFound = 0, string? error = null)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            job.Status = status;
            job.EventsFound += eventsFound;
            if (error != null) job.ErrorMessage = error;
        }
    }

    public void AddProcessedSite(string jobId, string siteName)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            job.ProcessedSites.Add(siteName);
        }
    }

    public void CompleteJob(string jobId, int totalEvents)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            job.Status = "Completed";
            job.CompletedAt = DateTime.UtcNow;
            job.EventsFound = totalEvents;
        }
    }

    public void FailJob(string jobId, string error)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            job.Status = "Failed";
            job.CompletedAt = DateTime.UtcNow;
            job.ErrorMessage = error;
        }
    }

    public ScrapeStatusDto? GetJobStatus(string jobId)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            return MapToDto(job);
        }
        return null;
    }

    public IEnumerable<ScrapeStatusDto> GetRecentJobs(int count = 10)
    {
        return _jobs.Values
            .OrderByDescending(j => j.StartedAt)
            .Take(count)
            .Select(MapToDto);
    }

    private static ScrapeStatusDto MapToDto(ScrapeJob job)
    {
        return new ScrapeStatusDto
        {
            JobId = job.JobId,
            Status = job.Status,
            StartedAt = job.StartedAt,
            CompletedAt = job.CompletedAt,
            EventsFound = job.EventsFound,
            ErrorMessage = job.ErrorMessage,
            ProcessedSites = job.ProcessedSites.ToList()
        };
    }

    private void CleanupOldJobs()
    {
        if (_jobs.Count > MaxJobsToKeep)
        {
            var oldJobs = _jobs.Values
                .OrderBy(j => j.StartedAt)
                .Take(_jobs.Count - MaxJobsToKeep + 10)
                .Select(j => j.JobId)
                .ToList();

            foreach (var id in oldJobs)
            {
                _jobs.TryRemove(id, out _);
            }
        }
    }

    private class ScrapeJob
    {
        public string JobId { get; set; } = default!;
        public string Status { get; set; } = "Queued";
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int EventsFound { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SiteName { get; set; }
        public string? City { get; set; }
        public List<string> ProcessedSites { get; set; } = new();
    }
}
