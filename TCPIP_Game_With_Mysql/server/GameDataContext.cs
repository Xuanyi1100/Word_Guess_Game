// server/GameDataContext.cs
// represent  database and will be used to interact with the database using EF Core.
using System.Data.Entity;
using server.Models;
using MySql.Data.EntityFramework;


namespace server
{
    // Database context for Word Guess Game
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class GameDataContext : DbContext
    {
        // Constructor to specify connection string name
        public GameDataContext() : base("Server=localhost;Database=WordGuessGame;User=root;Password=root;")
        {
            // Disable lazy loading to prevent unexpected database calls
            this.Configuration.LazyLoadingEnabled = false;
        }

        // Database set definitions
        public DbSet<User> Users { get; set; }
        public DbSet<GameSession> Sessions { get; set; }
        public DbSet<GameString> GameStrings { get; set; }

        public DbSet<GameWord> GameWords { get; set; }
        public DbSet<SpeedRecord> SpeedRecords { get; set; }

        public DbSet<GameStringGameWord> GameStringGameWords { get; set; }
        public DbSet<SessionGuessWord> SessionGuessWords { get; set; }
        

            //Configure model relationships and constraints
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Fluent API configurations can be added here if needed
            base.OnModelCreating(modelBuilder);
        }
    }
}
