using FitZone.Core.Entitys;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Repository.Data.Seed
{
    public static class TrackSeeder
    {
        public static async Task SeedAsync(FitContext context)
        {
            if (await context.Tracks.AnyAsync()) return;

            var tracks = new List<Track>
            {
                new Track
                {
                    Name        = "Strength & Hypertrophy",
                    Description = "Progressive resistance training focused on building muscle mass and " +
                                  "developing raw strength through compound and isolation movements."
                },
                new Track
                {
                    Name        = "Conditioning",
                    Description = "Cardiovascular endurance and work-capacity training. Programs in this " +
                                  "track build your aerobic base, improve recovery speed, and develop " +
                                  "sustained output across long efforts."
                },
                new Track
                {
                    Name        = "Functional Fitness",
                    Description = "Mixed-modal training that combines strength, conditioning, and skill " +
                                  "work to improve overall athleticism and real-world movement quality."
                },
                new Track
                {
                    Name        = "Mobility & Recovery",
                    Description = "Targeted flexibility, joint health, and active recovery work designed " +
                                  "to complement any training programme and prevent injury."
                }
            };

            context.Tracks.AddRange(tracks);
            await context.SaveChangesAsync();
        }
    }
}
