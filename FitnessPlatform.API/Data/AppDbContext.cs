using FitnessPlatform.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FitnessPlatform.API.Data
{
   public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<TrainingPlan> TrainingPlans { get; set; }
    public DbSet<NutritionPlan> NutritionPlans { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // RefreshToken -> User
        builder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RefreshToken>()
            .HasIndex(rt => rt.Token)
            .IsUnique();

        // Trainer -> Clients
        builder.Entity<AppUser>()
            .HasOne(u => u.Trainer)
            .WithMany(t => t.Clients)
            .HasForeignKey(u => u.TrainerId)
            .OnDelete(DeleteBehavior.Restrict);

        // TrainingPlan -> Client
        builder.Entity<TrainingPlan>()
            .HasOne(tp => tp.User)
            .WithMany(u => u.ClientTrainingPlans)
            .HasForeignKey(tp => tp.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        // TrainingPlan -> PT
        builder.Entity<TrainingPlan>()
            .HasOne(tp => tp.CreatedByPt)
            .WithMany(u => u.CreatedTrainingPlans)
            .HasForeignKey(tp => tp.CreatedByPtId)
            .OnDelete(DeleteBehavior.Restrict);

        // NutritionPlan -> Client
        builder.Entity<NutritionPlan>()
            .HasOne(np => np.User)
            .WithMany(u => u.ClientNutritionPlans)
            .HasForeignKey(np => np.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        // NutritionPlan -> PT
        builder.Entity<NutritionPlan>()
            .HasOne(np => np.CreatedByPt)
            .WithMany(u => u.CreatedNutritionPlans)
            .HasForeignKey(np => np.CreatedByPtId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
}