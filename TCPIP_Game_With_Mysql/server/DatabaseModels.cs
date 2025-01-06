// server/DatabaseModels.cs
// map to database tables
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlX.XDevAPI;

namespace server.Models
{
    [Table("User")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID { get; set; }

        public string UserName { get; set; }

        public string UserIP { get; set; }

        public int UserPort { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<GameSession> Sessions { get; set; }
        public virtual ICollection<SpeedRecord> SpeedRecords { get; set; }
    }

    [Table("GameSession")]
    public class GameSession
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GameSessionID { get; set; }
        public string SessionID { get; set; }

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public string Status { get; set; } // ENUM ('win', 'quit', 'lose', 'active')

        public int GameStringID { get; set; }

        [ForeignKey("GameStringID")]
        public virtual GameString GameString { get; set; }

        public int WordsToFound { get; set; } // Renamed from WordsFound
        public int TotalWords { get; set; }

        public virtual ICollection<SpeedRecord> SpeedRecords { get; set; }
    }

    public enum SessionStatus
    {
        Win,
        Quit,
        Lose,
        Active
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

        public int GameSessionID { get; set; }

        [ForeignKey("GameSessionID")]
        public virtual GameSession GameSession { get; set; }

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
        public int GameStringID { get; set; }

        [ForeignKey("GameStringID")]
        public virtual GameString GameString { get; set; }

        [Key]
        [Column(Order = 2)]
       
        public int GameWordID { get; set; }

        [ForeignKey("GameWordID")]
        public virtual GameWord GameWord { get; set; }
    }

    [Table("SessionGuessWord")]
    public class SessionGuessWord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SessionGuessWordID { get; set; }
        public int GameSessionID { get; set; }
        
        [ForeignKey("GameSessionID")]
        public virtual GameSession GameSession { get; set; } 
        public string GuessWord { get; set; }

        
    }

}
