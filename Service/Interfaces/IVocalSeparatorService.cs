using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    /// <summary>
    /// Contract for separating vocals from a audio file.
    /// </summary>
    public interface IVocalSeparatorService
    {
        /// <summary>
        /// Sends the audio file path to the Python Flask service,
        /// which runs UVR-MDX-NET separation and returns the vocals-only WAV path.
        /// </summary>
        Task<string> SeparateVocalsAsync(string filePath, CancellationToken ct);
    }
}
