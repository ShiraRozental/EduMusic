using Microsoft.EntityFrameworkCore;
using Repository.Entities;
using Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Repositories
{
    public class JobRepository(IContext _context) : IJobRepository
    {

        public async Task UpdateStatusAsync(Guid jobId, JobStatus status, string? error = null)
        {
            var job = await _context.Jobs.FindAsync(jobId);
            if (job != null)
            {
                job.Status = status;
                if (error != null) job.ErrorMessage = error;
                if (status == JobStatus.Failed || status == JobStatus.Completed) job.CompletedAt = DateTime.UtcNow;
                await _context.Save();
            }
        }

        public async Task CompleteJobAsync(Guid jobId, string lyrics, string category)
        {
            var job = await _context.Jobs.FindAsync(jobId);
            if (job != null)
            {
                job.Lyrics = lyrics;
                job.Category = category;
                job.Status = JobStatus.Completed;
                job.CompletedAt = DateTime.UtcNow;
                await _context.Save();
            }
        }

        public async Task<JobState?> GetNextQueuedJobAsync()
        {
            return await _context.Jobs
                .Where(j => j.Status == JobStatus.Queued)
                .OrderBy(j => j.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task AddJobAsync(JobState job)
        {
            await _context.Jobs.AddAsync(job);
            await _context.Save();
        }

        public async Task<JobState?> GetByIdAsync(Guid id)
        {
            return await _context.Jobs.FindAsync(id);
        }
    }
}
