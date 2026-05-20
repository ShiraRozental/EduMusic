using Repository.Entities;


namespace Repository.Interfaces
{
    public interface IJobRepository
    {
        Task UpdateStatusAsync(Guid jobId, JobStatus status, string? error = null);
        Task CompleteJobAsync(Guid jobId);
        Task<JobState?> GetNextQueuedJobAsync();
        Task AddJobAsync(JobState job);
        Task<JobState?> GetByIdAsync(Guid id);
    }
}
