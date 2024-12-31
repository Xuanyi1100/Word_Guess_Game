// server/DatabaseModels.cs
// map to database tables
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server.Models
{
    [Table("User")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID { get; set; }

        [Required]
        [StringLength(255)]
        public string UserName { get; set; }

        [Required]
        [StringLength(45)]
        public string UserIP { get; set; }

        public int UserPort { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<GameSession> Sessions { get; set; }
        public virtual ICollection<SpeedRecord> SpeedRecords { get; set; }
    }

    [Table("Session")]
    public class GameSession
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SessionID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        [Required]
        public string Status { get; set; }

        public int GameStringID { get; set; }

        [ForeignKey("GameStringID")]
        public virtual GameString GameString { get; set; }

        public int WordsFound { get; set; }

        public virtual ICollection<Guess> Guesses { get; set; }
        public virtual ICollection<SpeedRecord> SpeedRecords { get; set; }
    }

    public enum SessionStatus
    {
        Win,
        Quit,
        Lose,
        Active
    }

    [Table("Guess")]
    public class Guess
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GuessID { get; set; }

        public int SessionID { get; set; }

        [ForeignKey("SessionID")]
        public virtual GameSession Session { get; set; }

        [Required]
        [StringLength(255)]
        public string GuessText { get; set; }

        public bool IsCorrect { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    [Table("GameWord")]
    public class GameWord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GameWordID { get; set; }

        [Required]
        [StringLength(255)]
        public string GameWordText { get; set; }

        public virtual ICollection<GameStringGameWord> GameStringAssociations { get; set; }
    }

    [Table("GameString")]
    public class GameString
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GameStringID { get; set; }

        [Required]
        [StringLength(80)]
        public string GameStringText { get; set; }

        public virtual ICollection<GameStringGameWord> GameWordAssociations { get; set; }
    }

    [Table("SpeedRecord")]
    public class SpeedRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SpeedRecordID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        public int SessionID { get; set; }

        [ForeignKey("SessionID")]
        public virtual GameSession Session { get; set; }

        public int TimeUse { get; set; }

        public int GameStringID { get; set; }

        [ForeignKey("GameStringID")]
        public virtual GameString GameString { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    [Table("GameStringGameWord")]
    public class GameStringGameWord
    {
        [Key]
        [Column(Order = 1)]
        public int GameWordID { get; set; }

        [ForeignKey("GameWordID")]
        public virtual GameWord GameWord { get; set; }

        [Key]
        [Column(Order = 2)]
        public int GameStringID { get; set; }

        [ForeignKey("GameStringID")]
        public virtual GameString GameString { get; set; }
    }
}
