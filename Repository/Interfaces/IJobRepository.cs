using Repository.Entities;


namespace Repository.Interfaces
{
    public interface IJobRepository
    {
        Task UpdateStatusAsync(Guid jobId, JobStatus status, string? error = null);
        Task CompleteJobAsync(Guid jobId, string lyrics, string category);
        Task<JobState?> GetNextQueuedJobAsync();
    }
}
