using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IVocalSeparatorService
    {
        /// <summary>
        /// Uploads an audio file, processes it to extract vocals, and saves the resulting WAV to a local temporary path.
        /// </summary>
        Task<string> SeparateVocalsAsync(string inputFilePath, CancellationToken ct = default);

        /// <summary>
        /// Requests the remote server to delete all temporary output files associated with a specific job ID.
        /// </summary>
        Task DeleteRemoteJobAsync(string jobId, CancellationToken ct = default);

        /// <summary>
        /// Deletes the local temporary WAV file created during the separation process.
        /// </summary>
        void CleanupOutput(string vocalsPath);
    }
}
