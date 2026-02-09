using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Repository.Entities
{
    public class SongTagFrequency
    {
        [Key]
        public int FrequencyID { get; set; }

        public int SongID { get; set; }
        [ForeignKey("SongID")]
        public virtual Song Song { get; set; }

        public int TagID { get; set; }
        [ForeignKey("TagID")]
        public virtual Tag Tag { get; set; }

        public int Frequency { get; set; }
    }
}
