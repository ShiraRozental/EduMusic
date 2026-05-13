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
        /// מפריד את הקול (Vocals) מתוך קובץ שמע ומחזיר את הנתיב לקובץ ה-WAV שנוצר.
        /// </summary>
        Task<string> SeparateVocalsAsync(string inputFilePath, CancellationToken ct = default);

        /// <summary>
        /// מוחק את תיקיית הפלט הזמנית שנוצרה על ידי Demucs.
        /// </summary>
        void CleanupOutput(string vocalsPath);
    }
}
