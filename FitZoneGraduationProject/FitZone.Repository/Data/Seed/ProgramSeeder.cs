using FitZone.Core.Entitys;
using FitZone.Core.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Repository.Data.Seed
{
    public static class ProgramSeeder
    {
        public static async Task SeedAsync(FitContext context)
        {
            if (await context.WorkoutPrograms.AnyAsync()) return;

            // ── Resolve foreign keys ──────────────────────────────────
            var tracks = await context.Tracks
                .ToDictionaryAsync(t => t.Name, t => t.ID);

            // Key coaches by their own int Id, looked up via ApplicationUser.Email.
            // Using the coach's own PK (int) as dictionary value is safe —
            // it doesn't depend on whether Email loads correctly from the nav property.
            var coachRows = await context.Coachs
                .Include(c => c.ApplicationUser)
                .ToListAsync();

            // Fail fast with a clear message if UserSeeder didn't run first
            if (!coachRows.Any())
                throw new InvalidOperationException(
                    "ProgramSeeder requires coaches to exist. " +
                    "Run UserSeeder first, or check that the previous seeding run completed.");

            // Build lookup: email → coach.Id
            var coaches = coachRows.ToDictionary(
                c => c.ApplicationUser.Email!,
                c => c.ID);

            var ex = await context.Exercises
                .ToDictionaryAsync(e => e.Name, e => e.ID);

            if (!tracks.Any())
                throw new InvalidOperationException("ProgramSeeder requires tracks. Run TrackSeeder first.");

            if (!ex.Any())
                throw new InvalidOperationException("ProgramSeeder requires exercises. Run ExerciseSeeder first.");

            await SeedPushPullLegsProgram(context, ex,
                trackId: tracks["Strength & Hypertrophy"],
                coachId: coaches["ahmed.coach@fitzone.com"]);

            await SeedFatBurnConditioningProgram(context, ex,
                trackId: tracks["Conditioning"],
                coachId: coaches["nour.coach@fitzone.com"]);
        }

        // PROGRAM 1 — Push Pull Legs Foundation
        // 4 weeks · 3 sessions/week · Strength & Hypertrophy track
        

        private static async Task SeedPushPullLegsProgram(
            FitContext context, Dictionary<string, int> ex, int trackId, int coachId)
        {
            var program = new WorkoutProgram
            {
                TrackID = trackId,
                CoachID = coachId,
                Name = "Push Pull Legs Foundation",
                Description = "A classic 3-day Push/Pull/Legs split for beginners and early intermediates " +
                                   "who want to build a solid strength base. Each session focuses on one " +
                                   "movement pattern, allowing full recovery before you train that muscle group again.",
                DurationOnWeeks = 4,
                SessionsPerWeeks = 3,
                SessionsDuration = 60,
                TrainingGoal = TrainingGoal.BuildMuscle,
                FitnessLevel = FitnessLevel.Intermediate,
                EquipmentType = EquipmentType.FullGym,
                Status = ProgramStatus.Published,
                PublishedAt = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            context.WorkoutPrograms.Add(program);
            await context.SaveChangesAsync();

            var pplWeeks = new[]
            {
                new ProgramWeek { WorkoutProgramID = program.ID, WeekNumber = 1,
                    WeekDescription = "Introductory week — learn each movement pattern with moderate load. " +
                                      "Leave 3 reps in the tank on every work set. Technique over ego.",
                    FocusArea = "Movement quality & baseline strength" },

                new ProgramWeek { WorkoutProgramID = program.ID, WeekNumber = 2,
                    WeekDescription = "Add 2.5–5 kg to the bar on any lift that felt manageable last week. " +
                                      "Leave 2 reps in the tank.",
                    FocusArea = "Load progression" },

                new ProgramWeek { WorkoutProgramID = program.ID, WeekNumber = 3,
                    WeekDescription = "Intensity ramp — aim for RPE 8 on all main work sets. " +
                                      "Your last rep should be a genuine effort.",
                    FocusArea = "Intensity ramp" },

                new ProgramWeek { WorkoutProgramID = program.ID, WeekNumber = 4,
                    WeekDescription = "Deload week — reduce all loads by 40% and focus on perfect execution. " +
                                      "Let your body consolidate the adaptations from weeks 1–3.",
                    FocusArea = "Active recovery & deload" }
            };

            context.ProgramWeeks.AddRange(pplWeeks);
            await context.SaveChangesAsync();

            await SeedPplWeek(context, ex, pplWeeks[0], rpeOffset: 0, loadNote: "Introductory load — leave 3 reps in the tank.");
            await SeedPplWeek(context, ex, pplWeeks[1], rpeOffset: 0, loadNote: "Add 2.5–5 kg vs last week where it felt easy.");
            await SeedPplWeek(context, ex, pplWeeks[2], rpeOffset: 1, loadNote: "Push to RPE 8 — your last rep should be a real effort.");
            await SeedPplDeloadWeek(context, ex, pplWeeks[3]);
        }

        private static async Task SeedPplWeek(
            FitContext context, Dictionary<string, int> ex,
            ProgramWeek week, int rpeOffset, string loadNote)
        {
            var push = new WorkoutSession
            {
                ProgramWeekID = week.ID,
                SessionTitle = "Push Day — Chest, Shoulders & Triceps",
                weekDay = WeekDay.Monday,
                EstimatedDuration = 60,
                WarmupNotes = "5 min brisk walk or bike. Then: Band Pull-Apart 2×15, Cat-Cow 2×8.",
                PrimerNotes = "Push-Up 2×10 bodyweight — pause 1 second at the bottom, feel the chest load.",
                CooldownNotes = "Chest doorframe stretch 30s each side. Cross-body shoulder stretch 30s each side."
            };

            var pull = new WorkoutSession
            {
                ProgramWeekID = week.ID,
                SessionTitle = "Pull Day — Back & Biceps",
                weekDay = WeekDay.Wednesday,
                EstimatedDuration = 60,
                WarmupNotes = "5 min bike. Band Pull-Apart 2×15. Hip 90/90 stretch 60s per side.",
                PrimerNotes = "Dead hang from pull-up bar 2×20s — feel the shoulder blades depress and pack down.",
                CooldownNotes = "Lat doorframe stretch 30s each side. Supine spinal twist 60s per side."
            };

            var legs = new WorkoutSession
            {
                ProgramWeekID = week.ID,
                SessionTitle = "Legs Day — Quads, Hamstrings & Glutes",
                weekDay = WeekDay.Friday,
                EstimatedDuration = 65,
                WarmupNotes = "5 min easy walk. Bodyweight Squat 2×10. Hip 90/90 stretch 1 min per side.",
                PrimerNotes = "Romanian Deadlift to Hip Hinge Walkout 2×8 — prime the hamstrings before loading.",
                CooldownNotes = "Standing quad stretch 30s each. Kneeling hip-flexor stretch 40s each side."
            };

            context.WorkoutSessions.AddRange(push, pull, legs);
            await context.SaveChangesAsync();

            var pushExercises = new List<SessionExercise>
            {
                // Warmup
                new SessionExercise { WorkoutSessionID = push.ID, ExerciseID = ex["Cat-Cow"],         SectionType = SectionType.Warmup,   OrderInSection = 1, Sets = 2, Reps = "8",  RestSeconds = 30, Notes = "Slow and controlled — full flexion and extension on each rep." },
                new SessionExercise { WorkoutSessionID = push.ID, ExerciseID = ex["Band Pull-Apart"], SectionType = SectionType.Warmup,   OrderInSection = 2, Sets = 2, Reps = "15", RestSeconds = 30, Notes = "Keep arms straight. This is shoulder prep, not a strength exercise." },
                // Primer
                new SessionExercise { WorkoutSessionID = push.ID, ExerciseID = ex["Push-Up"],         SectionType = SectionType.Primer,   OrderInSection = 1, Sets = 2, Reps = "10", RestSeconds = 60, Notes = "Bodyweight only — pause 1 second at the bottom. Feel the chest stretch and load." },
                // Main work
                new SessionExercise { WorkoutSessionID = push.ID, ExerciseID = ex["Barbell Bench Press"],   SectionType = SectionType.MainWork, OrderInSection = 1, Sets = 3, Reps = "8",    RestSeconds = 180, RPETarget = 7 + rpeOffset, Notes = loadNote + " Control the descent 2s, brief pause on chest, drive hard." },
                new SessionExercise { WorkoutSessionID = push.ID, ExerciseID = ex["Dumbbell Incline Press"],SectionType = SectionType.MainWork, OrderInSection = 2, Sets = 3, Reps = "10",   RestSeconds = 120, RPETarget = 7 + rpeOffset, Notes = "Bench at 30°. Full stretch at the bottom, squeeze chest at the top." },
                new SessionExercise { WorkoutSessionID = push.ID, ExerciseID = ex["Overhead Press"],        SectionType = SectionType.MainWork, OrderInSection = 3, Sets = 3, Reps = "10",   RestSeconds = 120, RPETarget = 7 + rpeOffset, Notes = "Strict — no leg drive. Brace core and squeeze glutes before each rep." },
                new SessionExercise { WorkoutSessionID = push.ID, ExerciseID = ex["Push-Up"],               SectionType = SectionType.MainWork, OrderInSection = 4, Sets = 3, Reps = "AMRAP",RestSeconds = 90,  RPETarget = 8,              Notes = "Stop 1 rep before technical failure. Note your rep count each set." },
                // Cooldown
                new SessionExercise { WorkoutSessionID = push.ID, ExerciseID = ex["Face Pull"],       SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "15", RestSeconds = 45, Notes = "Light weight — purely shoulder health. Elbows high, thumbs back at finish." },
                new SessionExercise { WorkoutSessionID = push.ID, ExerciseID = ex["Band Pull-Apart"], SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "15", RestSeconds = 30, Notes = "Final set — let the rear delts and rhomboids decompress the shoulder." }
            };

            var pullExercises = new List<SessionExercise>
            {
                // Warmup
                new SessionExercise { WorkoutSessionID = pull.ID, ExerciseID = ex["Band Pull-Apart"], SectionType = SectionType.Warmup,   OrderInSection = 1, Sets = 2, Reps = "15", RestSeconds = 30, Notes = "Pre-activate the rear delts and rhomboids before loading the back." },
                new SessionExercise { WorkoutSessionID = pull.ID, ExerciseID = ex["Cat-Cow"],         SectionType = SectionType.Warmup,   OrderInSection = 2, Sets = 2, Reps = "8",  RestSeconds = 30, Notes = "Wake up the spine before the heavy hinge." },
                // Primer
                new SessionExercise { WorkoutSessionID = pull.ID, ExerciseID = ex["Romanian Deadlift to Hip Hinge Walkout"], SectionType = SectionType.Primer, OrderInSection = 1, Sets = 2, Reps = "6", RestSeconds = 60, Notes = "Bodyweight only — feel the hamstrings load on the hinge and the lats on the walkout." },
                // Main work
                new SessionExercise { WorkoutSessionID = pull.ID, ExerciseID = ex["Pull-Up"],         SectionType = SectionType.MainWork, OrderInSection = 1, Sets = 4, Reps = "5-6", RestSeconds = 180, RPETarget = 7 + rpeOffset, Notes = loadNote + " Use band assistance if needed. Pull elbows to floor, not hands to bar." },
                new SessionExercise { WorkoutSessionID = pull.ID, ExerciseID = ex["Barbell Deadlift"],SectionType = SectionType.MainWork, OrderInSection = 2, Sets = 3, Reps = "6",   RestSeconds = 210, RPETarget = 7 + rpeOffset, Notes = loadNote + " Reset each rep — bar stays against your legs throughout." },
                new SessionExercise { WorkoutSessionID = pull.ID, ExerciseID = ex["Face Pull"],        SectionType = SectionType.MainWork, OrderInSection = 3, Sets = 3, Reps = "15",  RestSeconds = 60,  Notes = "Superset with deadlift rest if time is tight. Moderate weight." },
                // Cooldown
                new SessionExercise { WorkoutSessionID = pull.ID, ExerciseID = ex["Hip 90/90 Stretch"],SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "60s per side", RestSeconds = 15, Notes = "Breathe into the hip and hold. Do not force the range." },
                new SessionExercise { WorkoutSessionID = pull.ID, ExerciseID = ex["Cat-Cow"],           SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "8",            RestSeconds = 15, Notes = "Slow and deliberate — decompress the spine after the heavy pulls." }
            };

            var legsExercises = new List<SessionExercise>
            {
                // Warmup
                new SessionExercise { WorkoutSessionID = legs.ID, ExerciseID = ex["Bodyweight Squat"], SectionType = SectionType.Warmup,   OrderInSection = 1, Sets = 2, Reps = "12", RestSeconds = 45, Notes = "Use this to find your stance and depth — slow and controlled." },
                new SessionExercise { WorkoutSessionID = legs.ID, ExerciseID = ex["Cat-Cow"],          SectionType = SectionType.Warmup,   OrderInSection = 2, Sets = 1, Reps = "8",  RestSeconds = 30, Notes = "Wake up the lumbar spine before loading." },
                // Primer
                new SessionExercise { WorkoutSessionID = legs.ID, ExerciseID = ex["Romanian Deadlift to Hip Hinge Walkout"], SectionType = SectionType.Primer, OrderInSection = 1, Sets = 2, Reps = "8", RestSeconds = 60, Notes = "Bodyweight — feel the hamstrings load and hips hinge freely before adding the bar." },
                // Main work
                new SessionExercise { WorkoutSessionID = legs.ID, ExerciseID = ex["Barbell Back Squat"],        SectionType = SectionType.MainWork, OrderInSection = 1, Sets = 4, Reps = "6",  RestSeconds = 210, RPETarget = 7 + rpeOffset, Tempo = "3-1-1", Notes = loadNote + " 3-second descent, 1-second pause at depth, drive hard." },
                new SessionExercise { WorkoutSessionID = legs.ID, ExerciseID = ex["Dumbbell Romanian Deadlift"],SectionType = SectionType.MainWork, OrderInSection = 2, Sets = 3, Reps = "10", RestSeconds = 120, RPETarget = 7 + rpeOffset, Notes = "Feel the hamstring stretch at the bottom. Do not round the back to reach lower." },
                new SessionExercise { WorkoutSessionID = legs.ID, ExerciseID = ex["Goblet Squat"],               SectionType = SectionType.MainWork, OrderInSection = 3, Sets = 3, Reps = "12", RestSeconds = 90,  Notes = "Finisher — lighter weight, full range, feel the quads the whole way down." },
                // Cooldown
                new SessionExercise { WorkoutSessionID = legs.ID, ExerciseID = ex["Hip 90/90 Stretch"],SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "90s per side", RestSeconds = 15, Notes = "Breathe and relax into the stretch. Both hips on the floor." },
                new SessionExercise { WorkoutSessionID = legs.ID, ExerciseID = ex["Cat-Cow"],           SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "8",            RestSeconds = 15, Notes = "Finish every leg session with this — decompress the lumbar spine." }
            };

            context.SessionExercises.AddRange(pushExercises);
            context.SessionExercises.AddRange(pullExercises);
            context.SessionExercises.AddRange(legsExercises);
            await context.SaveChangesAsync();
        }

        private static async Task SeedPplDeloadWeek(
            FitContext context, Dictionary<string, int> ex, ProgramWeek week)
        {
            var push = new WorkoutSession { ProgramWeekID = week.ID, SessionTitle = "Deload Push", weekDay = WeekDay.Monday, EstimatedDuration = 40, WarmupNotes = "5 min easy walk. Band Pull-Apart 2×10.", PrimerNotes = "Push-Up 1×10 — feel the chest, no effort.", CooldownNotes = "Chest and shoulder stretches 5 min." };
            var pull = new WorkoutSession { ProgramWeekID = week.ID, SessionTitle = "Deload Pull", weekDay = WeekDay.Wednesday, EstimatedDuration = 40, WarmupNotes = "5 min easy walk. Cat-Cow 2×8.", PrimerNotes = "Dead hang 1×20s — just hang and breathe.", CooldownNotes = "Lat stretch and supine twist 5 min." };
            var legs = new WorkoutSession { ProgramWeekID = week.ID, SessionTitle = "Deload Legs", weekDay = WeekDay.Friday, EstimatedDuration = 40, WarmupNotes = "5 min easy walk. Bodyweight Squat 1×10.", PrimerNotes = "Hip 90/90 stretch 1 min per side.", CooldownNotes = "Full lower body stretch 10 min. You earned it." };

            context.WorkoutSessions.AddRange(push, pull, legs);
            await context.SaveChangesAsync();

            var ses = new List<SessionExercise>
            {
                new SessionExercise { WorkoutSessionID = push.ID, ExerciseID = ex["Band Pull-Apart"],            SectionType = SectionType.Warmup,   OrderInSection = 1, Sets = 2, Reps = "10", Notes = "Light activation only." },
                new SessionExercise { WorkoutSessionID = push.ID, ExerciseID = ex["Barbell Bench Press"],        SectionType = SectionType.MainWork,  OrderInSection = 1, Sets = 2, Reps = "10", RestSeconds = 90,  Notes = "60% of Week 3 load. Perfect bar path." },
                new SessionExercise { WorkoutSessionID = push.ID, ExerciseID = ex["Overhead Press"],             SectionType = SectionType.MainWork,  OrderInSection = 2, Sets = 2, Reps = "10", RestSeconds = 90,  Notes = "60% of Week 3 load." },
                new SessionExercise { WorkoutSessionID = push.ID, ExerciseID = ex["Face Pull"],                  SectionType = SectionType.Cooldown,  OrderInSection = 1, Sets = 2, Reps = "15", Notes = "Light. Shoulder health." },
                new SessionExercise { WorkoutSessionID = push.ID, ExerciseID = ex["Cat-Cow"],                    SectionType = SectionType.Cooldown,  OrderInSection = 2, Sets = 1, Reps = "10" },

                new SessionExercise { WorkoutSessionID = pull.ID, ExerciseID = ex["Cat-Cow"],                    SectionType = SectionType.Warmup,   OrderInSection = 1, Sets = 2, Reps = "8" },
                new SessionExercise { WorkoutSessionID = pull.ID, ExerciseID = ex["Pull-Up"],                    SectionType = SectionType.MainWork,  OrderInSection = 1, Sets = 2, Reps = "5",  RestSeconds = 120, Notes = "60% effort — use a band. Smooth, controlled reps only." },
                new SessionExercise { WorkoutSessionID = pull.ID, ExerciseID = ex["Barbell Deadlift"],           SectionType = SectionType.MainWork,  OrderInSection = 2, Sets = 2, Reps = "5",  RestSeconds = 150, Notes = "60% of Week 3 load. Reset each rep with care." },
                new SessionExercise { WorkoutSessionID = pull.ID, ExerciseID = ex["Hip 90/90 Stretch"],          SectionType = SectionType.Cooldown,  OrderInSection = 1, Sets = 2, Reps = "90s per side" },
                new SessionExercise { WorkoutSessionID = pull.ID, ExerciseID = ex["Cat-Cow"],                    SectionType = SectionType.Cooldown,  OrderInSection = 2, Sets = 1, Reps = "10" },

                new SessionExercise { WorkoutSessionID = legs.ID, ExerciseID = ex["Bodyweight Squat"],           SectionType = SectionType.Warmup,   OrderInSection = 1, Sets = 2, Reps = "10" },
                new SessionExercise { WorkoutSessionID = legs.ID, ExerciseID = ex["Barbell Back Squat"],         SectionType = SectionType.MainWork,  OrderInSection = 1, Sets = 2, Reps = "10", RestSeconds = 90,  Tempo = "3-1-1", Notes = "60% of Week 3 load. Dial in the technique." },
                new SessionExercise { WorkoutSessionID = legs.ID, ExerciseID = ex["Dumbbell Romanian Deadlift"], SectionType = SectionType.MainWork,  OrderInSection = 2, Sets = 2, Reps = "10", RestSeconds = 90,  Notes = "60% of Week 3 load. Feel every rep." },
                new SessionExercise { WorkoutSessionID = legs.ID, ExerciseID = ex["Hip 90/90 Stretch"],          SectionType = SectionType.Cooldown,  OrderInSection = 1, Sets = 2, Reps = "90s per side" },
                new SessionExercise { WorkoutSessionID = legs.ID, ExerciseID = ex["Cat-Cow"],                    SectionType = SectionType.Cooldown,  OrderInSection = 2, Sets = 2, Reps = "10" }
            };

            context.SessionExercises.AddRange(ses);
            await context.SaveChangesAsync();
        }


        // PROGRAM 2 — 8-Week Fat Burn Conditioning
        // 8 weeks · 3 sessions/week · Conditioning track
      

        private static async Task SeedFatBurnConditioningProgram(
            FitContext context, Dictionary<string, int> ex, int trackId, int coachId)
        {
            var program = new WorkoutProgram
            {
                TrackID = trackId,
                CoachID = coachId,
                Name = "8-Week Fat Burn Conditioning",
                Description = "A 3-day per week conditioning program that maximises calorie burn and " +
                                   "cardiovascular adaptation through rotating HIIT, circuit, and endurance work. " +
                                   "Volume and intensity build progressively over 8 weeks with a deload at week 4.",
                DurationOnWeeks = 8,
                SessionsPerWeeks = 3,
                SessionsDuration = 45,
                TrainingGoal = TrainingGoal.LoseFat,
                FitnessLevel = FitnessLevel.Beginner,
                EquipmentType = EquipmentType.Bodyweight,
                Status = ProgramStatus.Published,
                PublishedAt = new DateTime(2025, 12, 15, 0, 0, 0, DateTimeKind.Utc)
            };

            context.WorkoutPrograms.Add(program);
            await context.SaveChangesAsync();

            var condWeeks = new[]
            {
                new ProgramWeek { WorkoutProgramID = program.ID, WeekNumber = 1, WeekDescription = "Orientation week — learn the movements and find your baseline. Rest generously.", FocusArea = "Foundation & Movement Quality" },
                new ProgramWeek { WorkoutProgramID = program.ID, WeekNumber = 2, WeekDescription = "Reduce rest periods by 10 seconds vs last week. The circuit should feel more familiar.", FocusArea = "Reducing Rest" },
                new ProgramWeek { WorkoutProgramID = program.ID, WeekNumber = 3, WeekDescription = "Add a 4th round to Session B. Keep Sessions A and C at 3 rounds.", FocusArea = "Volume Increase" },
                new ProgramWeek { WorkoutProgramID = program.ID, WeekNumber = 4, WeekDescription = "Deload week — all sessions at 60% effort. Focus on movement quality and recovery.", FocusArea = "Deload" },
                new ProgramWeek { WorkoutProgramID = program.ID, WeekNumber = 5, WeekDescription = "Intensity block begins — shorten all work-to-rest ratios by 10s.", FocusArea = "Intensity Phase" },
                new ProgramWeek { WorkoutProgramID = program.ID, WeekNumber = 6, WeekDescription = "Add a 4th round to all three sessions this week.", FocusArea = "Peak Volume" },
                new ProgramWeek { WorkoutProgramID = program.ID, WeekNumber = 7, WeekDescription = "Max effort week — push as hard as you can on every interval.", FocusArea = "Peak Intensity" },
                new ProgramWeek { WorkoutProgramID = program.ID, WeekNumber = 8, WeekDescription = "Test and taper — one final max-effort session, then two active recovery sessions.", FocusArea = "Test & Taper" }
            };

            context.ProgramWeeks.AddRange(condWeeks);
            await context.SaveChangesAsync();

            await SeedConditioningWeek1(context, ex, condWeeks[0]);
            await SeedConditioningProgressionWeek(context, ex, condWeeks[1], weekNum: 2, extraRound: false, restReduction: "Reduce rest by 10s vs Week 1.");
            await SeedConditioningProgressionWeek(context, ex, condWeeks[2], weekNum: 3, extraRound: true, restReduction: "Session B adds a 4th round today.");
            await SeedConditioningDeloadWeek(context, ex, condWeeks[3]);
            await SeedConditioningIntensityWeek(context, ex, condWeeks[4], weekNum: 5, note: "Reduce work-to-rest ratio by 10s on all intervals.");
            await SeedConditioningIntensityWeek(context, ex, condWeeks[5], weekNum: 6, note: "4 rounds on every session today.");
            await SeedConditioningIntensityWeek(context, ex, condWeeks[6], weekNum: 7, note: "Max effort — push as hard as you can. Note every rep count.");
            await SeedConditioningFinalWeek(context, ex, condWeeks[7]);
        }

        private static async Task SeedConditioningWeek1(FitContext context, Dictionary<string, int> ex, ProgramWeek week)
        {
            var sessionA = new WorkoutSession { ProgramWeekID = week.ID, SessionTitle = "Session A — HIIT Intervals", weekDay = WeekDay.Monday, EstimatedDuration = 40, WarmupNotes = "5 min easy march in place or light jog. Cat-Cow 2×8, Bodyweight Squat 2×10.", PrimerNotes = "Mountain Climber 2×20s at easy pace — wake up the hips, not a working set.", CooldownNotes = "3 min easy walk. Hip 90/90 stretch 60s per side." };
            var sessionB = new WorkoutSession { ProgramWeekID = week.ID, SessionTitle = "Session B — Bodyweight Circuit", weekDay = WeekDay.Wednesday, EstimatedDuration = 40, WarmupNotes = "5 min march in place. Bodyweight Squat 2×10. Push-Up 2×5.", PrimerNotes = "Plank hold 2×20s — brace the core before the circuit begins.", CooldownNotes = "Full body stretching 5 min. Focus on hips and chest." };
            var sessionC = new WorkoutSession { ProgramWeekID = week.ID, SessionTitle = "Session C — Endurance & Core", weekDay = WeekDay.Friday, EstimatedDuration = 40, WarmupNotes = "5 min easy movement. Cat-Cow 2×8. Hip 90/90 60s per side.", PrimerNotes = "Mountain Climber 1×20s easy — prime the core before the endurance work.", CooldownNotes = "Hip 90/90 60s per side. Supine spinal twist 60s per side." };

            context.WorkoutSessions.AddRange(sessionA, sessionB, sessionC);
            await context.SaveChangesAsync();

            var exercises = new List<SessionExercise>
            {
                // Session A
                new SessionExercise { WorkoutSessionID = sessionA.ID, ExerciseID = ex["Bodyweight Squat"], SectionType = SectionType.Warmup,   OrderInSection = 1, Sets = 2, Reps = "10",              RestSeconds = 30, Notes = "Easy pace, full range." },
                new SessionExercise { WorkoutSessionID = sessionA.ID, ExerciseID = ex["Cat-Cow"],          SectionType = SectionType.Warmup,   OrderInSection = 2, Sets = 2, Reps = "8",               RestSeconds = 20 },
                new SessionExercise { WorkoutSessionID = sessionA.ID, ExerciseID = ex["Mountain Climber"], SectionType = SectionType.Primer,   OrderInSection = 1, Sets = 2, Reps = "20s",             RestSeconds = 30, Notes = "Easy pace — just warming up the hips." },
                new SessionExercise { WorkoutSessionID = sessionA.ID, ExerciseID = ex["Burpee"],           SectionType = SectionType.MainWork, OrderInSection = 1, Sets = 3, Reps = "30s on / 30s off",RestSeconds = 30, Notes = "3 rounds. Work hard for 30s, rest 30s." },
                new SessionExercise { WorkoutSessionID = sessionA.ID, ExerciseID = ex["Mountain Climber"], SectionType = SectionType.MainWork, OrderInSection = 2, Sets = 3, Reps = "30s on / 30s off",RestSeconds = 30 },
                new SessionExercise { WorkoutSessionID = sessionA.ID, ExerciseID = ex["Box Jump"],         SectionType = SectionType.MainWork, OrderInSection = 3, Sets = 3, Reps = "6",               RestSeconds = 60, Notes = "Step down between reps. Quality over speed." },
                new SessionExercise { WorkoutSessionID = sessionA.ID, ExerciseID = ex["Hip 90/90 Stretch"],SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "60s per side",    Notes = "Breathe into the stretch." },
                new SessionExercise { WorkoutSessionID = sessionA.ID, ExerciseID = ex["Cat-Cow"],          SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "8" },
                // Session B
                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Bodyweight Squat"], SectionType = SectionType.Warmup,   OrderInSection = 1, Sets = 2, Reps = "10", RestSeconds = 30 },
                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Push-Up"],          SectionType = SectionType.Warmup,   OrderInSection = 2, Sets = 2, Reps = "5",  RestSeconds = 30, Notes = "Light — activating the push pattern." },
                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Plank"],            SectionType = SectionType.Primer,   OrderInSection = 1, Sets = 2, Reps = "20s", RestSeconds = 30, Notes = "Brace the core before the circuit." },
                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Push-Up"],          SectionType = SectionType.MainWork, OrderInSection = 1, Sets = 3, Reps = "12", RestSeconds = 45, Notes = "3 rounds of the full circuit. 45s rest between exercises." },
                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Bodyweight Squat"], SectionType = SectionType.MainWork, OrderInSection = 2, Sets = 3, Reps = "15", RestSeconds = 45 },
                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Mountain Climber"], SectionType = SectionType.MainWork, OrderInSection = 3, Sets = 3, Reps = "20s", RestSeconds = 45 },
                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Burpee"],           SectionType = SectionType.MainWork, OrderInSection = 4, Sets = 3, Reps = "8",  RestSeconds = 60, Notes = "60s rest after Burpee before next round." },
                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Hip 90/90 Stretch"],SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "60s per side" },
                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Cat-Cow"],          SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "8" },
                // Session C
                new SessionExercise { WorkoutSessionID = sessionC.ID, ExerciseID = ex["Cat-Cow"],          SectionType = SectionType.Warmup,   OrderInSection = 1, Sets = 2, Reps = "8",        RestSeconds = 20 },
                new SessionExercise { WorkoutSessionID = sessionC.ID, ExerciseID = ex["Bodyweight Squat"], SectionType = SectionType.Warmup,   OrderInSection = 2, Sets = 2, Reps = "10",       RestSeconds = 30 },
                new SessionExercise { WorkoutSessionID = sessionC.ID, ExerciseID = ex["Mountain Climber"], SectionType = SectionType.Primer,   OrderInSection = 1, Sets = 1, Reps = "20s",      RestSeconds = 30, Notes = "Easy pace — prime the core." },
                new SessionExercise { WorkoutSessionID = sessionC.ID, ExerciseID = ex["Bodyweight Squat"], SectionType = SectionType.MainWork, OrderInSection = 1, Sets = 4, Reps = "20",       RestSeconds = 60, Notes = "Steady pace — this is endurance, not max effort." },
                new SessionExercise { WorkoutSessionID = sessionC.ID, ExerciseID = ex["Plank"],            SectionType = SectionType.MainWork, OrderInSection = 2, Sets = 3, Reps = "30s hold", RestSeconds = 45, Notes = "Build to a 60s hold by Week 4." },
                new SessionExercise { WorkoutSessionID = sessionC.ID, ExerciseID = ex["Mountain Climber"], SectionType = SectionType.MainWork, OrderInSection = 3, Sets = 3, Reps = "40s",      RestSeconds = 45 },
                new SessionExercise { WorkoutSessionID = sessionC.ID, ExerciseID = ex["Hip 90/90 Stretch"],SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "60s per side" },
                new SessionExercise { WorkoutSessionID = sessionC.ID, ExerciseID = ex["Cat-Cow"],          SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "8" }
            };

            context.SessionExercises.AddRange(exercises);
            await context.SaveChangesAsync();
        }

        private static async Task SeedConditioningProgressionWeek(FitContext context, Dictionary<string, int> ex, ProgramWeek week, int weekNum, bool extraRound, string restReduction)
        {
            int rounds = extraRound ? 4 : 3;

            var sessionA = new WorkoutSession { ProgramWeekID = week.ID, SessionTitle = $"Week {weekNum} — Session A HIIT", weekDay = WeekDay.Monday, EstimatedDuration = 42, WarmupNotes = "5 min march in place. Cat-Cow 2×8, Bodyweight Squat 2×10.", PrimerNotes = "Mountain Climber 2×20s easy pace.", CooldownNotes = "Hip 90/90 60s per side. Easy walk 2 min." };
            var sessionB = new WorkoutSession { ProgramWeekID = week.ID, SessionTitle = $"Week {weekNum} — Session B Circuit", weekDay = WeekDay.Wednesday, EstimatedDuration = 45, WarmupNotes = "5 min march. Bodyweight Squat 2×10. Push-Up 2×5.", PrimerNotes = "Plank 2×20s.", CooldownNotes = "Full body stretch 5 min." };
            var sessionC = new WorkoutSession { ProgramWeekID = week.ID, SessionTitle = $"Week {weekNum} — Session C Endurance", weekDay = WeekDay.Friday, EstimatedDuration = 42, WarmupNotes = "5 min easy movement. Cat-Cow 2×8.", PrimerNotes = "Mountain Climber 1×20s easy.", CooldownNotes = "Hip 90/90 60s per side. Supine twist 60s per side." };

            context.WorkoutSessions.AddRange(sessionA, sessionB, sessionC);
            await context.SaveChangesAsync();

            var exercises = new List<SessionExercise>
            {
                new SessionExercise { WorkoutSessionID = sessionA.ID, ExerciseID = ex["Bodyweight Squat"], SectionType = SectionType.Warmup,   OrderInSection = 1, Sets = 2, Reps = "10" },
                new SessionExercise { WorkoutSessionID = sessionA.ID, ExerciseID = ex["Mountain Climber"], SectionType = SectionType.Primer,   OrderInSection = 1, Sets = 2, Reps = "20s", Notes = "Easy pace." },
                new SessionExercise { WorkoutSessionID = sessionA.ID, ExerciseID = ex["Burpee"],           SectionType = SectionType.MainWork, OrderInSection = 1, Sets = 3, Reps = "30s on / 20s off", Notes = restReduction },
                new SessionExercise { WorkoutSessionID = sessionA.ID, ExerciseID = ex["Mountain Climber"], SectionType = SectionType.MainWork, OrderInSection = 2, Sets = 3, Reps = "30s on / 20s off" },
                new SessionExercise { WorkoutSessionID = sessionA.ID, ExerciseID = ex["Box Jump"],         SectionType = SectionType.MainWork, OrderInSection = 3, Sets = 3, Reps = "6", RestSeconds = 50 },
                new SessionExercise { WorkoutSessionID = sessionA.ID, ExerciseID = ex["Hip 90/90 Stretch"],SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "60s per side" },
                new SessionExercise { WorkoutSessionID = sessionA.ID, ExerciseID = ex["Cat-Cow"],          SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "8" },

                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Bodyweight Squat"], SectionType = SectionType.Warmup,   OrderInSection = 1, Sets = 2, Reps = "10" },
                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Plank"],            SectionType = SectionType.Primer,   OrderInSection = 1, Sets = 2, Reps = "20s" },
                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Push-Up"],          SectionType = SectionType.MainWork, OrderInSection = 1, Sets = rounds, Reps = "12", RestSeconds = 40, Notes = $"{rounds} rounds. " + restReduction },
                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Bodyweight Squat"], SectionType = SectionType.MainWork, OrderInSection = 2, Sets = rounds, Reps = "15", RestSeconds = 40 },
                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Mountain Climber"], SectionType = SectionType.MainWork, OrderInSection = 3, Sets = rounds, Reps = "20s", RestSeconds = 40 },
                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Burpee"],           SectionType = SectionType.MainWork, OrderInSection = 4, Sets = rounds, Reps = "8",  RestSeconds = 55 },
                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Hip 90/90 Stretch"],SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "60s per side" },
                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Cat-Cow"],          SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "8" },

                new SessionExercise { WorkoutSessionID = sessionC.ID, ExerciseID = ex["Cat-Cow"],          SectionType = SectionType.Warmup,   OrderInSection = 1, Sets = 2, Reps = "8" },
                new SessionExercise { WorkoutSessionID = sessionC.ID, ExerciseID = ex["Mountain Climber"], SectionType = SectionType.Primer,   OrderInSection = 1, Sets = 1, Reps = "20s", Notes = "Easy pace." },
                new SessionExercise { WorkoutSessionID = sessionC.ID, ExerciseID = ex["Bodyweight Squat"], SectionType = SectionType.MainWork, OrderInSection = 1, Sets = 4, Reps = "20", RestSeconds = 55, Notes = "Steady endurance pace." },
                new SessionExercise { WorkoutSessionID = sessionC.ID, ExerciseID = ex["Plank"],            SectionType = SectionType.MainWork, OrderInSection = 2, Sets = 3, Reps = "35s hold", RestSeconds = 40 },
                new SessionExercise { WorkoutSessionID = sessionC.ID, ExerciseID = ex["Mountain Climber"], SectionType = SectionType.MainWork, OrderInSection = 3, Sets = 3, Reps = "40s", RestSeconds = 40 },
                new SessionExercise { WorkoutSessionID = sessionC.ID, ExerciseID = ex["Hip 90/90 Stretch"],SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "60s per side" },
                new SessionExercise { WorkoutSessionID = sessionC.ID, ExerciseID = ex["Cat-Cow"],          SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "8" }
            };

            context.SessionExercises.AddRange(exercises);
            await context.SaveChangesAsync();
        }

        private static async Task SeedConditioningDeloadWeek(FitContext context, Dictionary<string, int> ex, ProgramWeek week)
        {
            var sessionA = new WorkoutSession { ProgramWeekID = week.ID, SessionTitle = "Deload — Active Recovery A", weekDay = WeekDay.Monday, EstimatedDuration = 30, WarmupNotes = "5 min easy walk.", PrimerNotes = "Bodyweight Squat 1×10 — just movement.", CooldownNotes = "Full body stretching 8 min." };
            var sessionB = new WorkoutSession { ProgramWeekID = week.ID, SessionTitle = "Deload — Active Recovery B", weekDay = WeekDay.Wednesday, EstimatedDuration = 30, WarmupNotes = "5 min easy walk. Cat-Cow 2×8.", PrimerNotes = "Plank 1×20s — just activating.", CooldownNotes = "Hip 90/90 and supine twist 5 min each side." };
            var sessionC = new WorkoutSession { ProgramWeekID = week.ID, SessionTitle = "Deload — Active Recovery C", weekDay = WeekDay.Friday, EstimatedDuration = 30, WarmupNotes = "5 min easy walk.", PrimerNotes = "Mountain Climber 1×15s — easy.", CooldownNotes = "Full body stretching 10 min. Great work this month." };

            context.WorkoutSessions.AddRange(sessionA, sessionB, sessionC);
            await context.SaveChangesAsync();

            var exercises = new List<SessionExercise>
            {
                new SessionExercise { WorkoutSessionID = sessionA.ID, ExerciseID = ex["Bodyweight Squat"], SectionType = SectionType.Warmup,   OrderInSection = 1, Sets = 1, Reps = "10" },
                new SessionExercise { WorkoutSessionID = sessionA.ID, ExerciseID = ex["Push-Up"],          SectionType = SectionType.MainWork, OrderInSection = 1, Sets = 2, Reps = "8",  RestSeconds = 90, Notes = "Half effort — 60% of your normal intensity." },
                new SessionExercise { WorkoutSessionID = sessionA.ID, ExerciseID = ex["Bodyweight Squat"], SectionType = SectionType.MainWork, OrderInSection = 2, Sets = 2, Reps = "10", RestSeconds = 90 },
                new SessionExercise { WorkoutSessionID = sessionA.ID, ExerciseID = ex["Hip 90/90 Stretch"],SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "90s per side" },
                new SessionExercise { WorkoutSessionID = sessionA.ID, ExerciseID = ex["Cat-Cow"],          SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "8" },

                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Cat-Cow"],          SectionType = SectionType.Warmup,   OrderInSection = 1, Sets = 2, Reps = "8" },
                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Plank"],            SectionType = SectionType.MainWork, OrderInSection = 1, Sets = 2, Reps = "20s", RestSeconds = 60, Notes = "Easy — this is recovery." },
                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Mountain Climber"], SectionType = SectionType.MainWork, OrderInSection = 2, Sets = 2, Reps = "20s", RestSeconds = 60 },
                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Hip 90/90 Stretch"],SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "90s per side" },
                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Cat-Cow"],          SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "8" },

                new SessionExercise { WorkoutSessionID = sessionC.ID, ExerciseID = ex["Bodyweight Squat"], SectionType = SectionType.Warmup,   OrderInSection = 1, Sets = 1, Reps = "10" },
                new SessionExercise { WorkoutSessionID = sessionC.ID, ExerciseID = ex["Bodyweight Squat"], SectionType = SectionType.MainWork, OrderInSection = 1, Sets = 2, Reps = "15", RestSeconds = 90, Notes = "Easy endurance pace." },
                new SessionExercise { WorkoutSessionID = sessionC.ID, ExerciseID = ex["Plank"],            SectionType = SectionType.MainWork, OrderInSection = 2, Sets = 2, Reps = "25s", RestSeconds = 60 },
                new SessionExercise { WorkoutSessionID = sessionC.ID, ExerciseID = ex["Hip 90/90 Stretch"],SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "90s per side" },
                new SessionExercise { WorkoutSessionID = sessionC.ID, ExerciseID = ex["Cat-Cow"],          SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "10" }
            };

            context.SessionExercises.AddRange(exercises);
            await context.SaveChangesAsync();
        }

        private static async Task SeedConditioningIntensityWeek(FitContext context, Dictionary<string, int> ex, ProgramWeek week, int weekNum, string note)
        {
            int rounds = weekNum >= 6 ? 4 : 3;

            var sessionA = new WorkoutSession { ProgramWeekID = week.ID, SessionTitle = $"Week {weekNum} — Session A Max Effort HIIT", weekDay = WeekDay.Monday, EstimatedDuration = 45, WarmupNotes = "5 min jog or march. Cat-Cow 2×8. Bodyweight Squat 2×10.", PrimerNotes = "Mountain Climber 2×20s — build to working pace.", CooldownNotes = "Hip 90/90 60s per side. Easy walk 3 min." };
            var sessionB = new WorkoutSession { ProgramWeekID = week.ID, SessionTitle = $"Week {weekNum} — Session B Full Circuit", weekDay = WeekDay.Wednesday, EstimatedDuration = 48, WarmupNotes = "5 min march. Push-Up 2×5. Bodyweight Squat 2×10.", PrimerNotes = "Plank 2×25s.", CooldownNotes = "Full body stretch 5 min." };
            var sessionC = new WorkoutSession { ProgramWeekID = week.ID, SessionTitle = $"Week {weekNum} — Session C Endurance Push", weekDay = WeekDay.Friday, EstimatedDuration = 45, WarmupNotes = "5 min easy movement. Cat-Cow 2×8.", PrimerNotes = "Mountain Climber 1×25s at working pace.", CooldownNotes = "Hip 90/90 60s per side. Supine twist 60s per side." };

            context.WorkoutSessions.AddRange(sessionA, sessionB, sessionC);
            await context.SaveChangesAsync();

            var exercises = new List<SessionExercise>
            {
                new SessionExercise { WorkoutSessionID = sessionA.ID, ExerciseID = ex["Bodyweight Squat"], SectionType = SectionType.Warmup,   OrderInSection = 1, Sets = 2, Reps = "10" },
                new SessionExercise { WorkoutSessionID = sessionA.ID, ExerciseID = ex["Mountain Climber"], SectionType = SectionType.Primer,   OrderInSection = 1, Sets = 2, Reps = "20s", Notes = "Build to working pace." },
                new SessionExercise { WorkoutSessionID = sessionA.ID, ExerciseID = ex["Burpee"],           SectionType = SectionType.MainWork, OrderInSection = 1, Sets = rounds, Reps = "40s on / 20s off", Notes = note },
                new SessionExercise { WorkoutSessionID = sessionA.ID, ExerciseID = ex["Mountain Climber"], SectionType = SectionType.MainWork, OrderInSection = 2, Sets = rounds, Reps = "40s on / 20s off" },
                new SessionExercise { WorkoutSessionID = sessionA.ID, ExerciseID = ex["Box Jump"],         SectionType = SectionType.MainWork, OrderInSection = 3, Sets = rounds, Reps = "8", RestSeconds = 45 },
                new SessionExercise { WorkoutSessionID = sessionA.ID, ExerciseID = ex["Hip 90/90 Stretch"],SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "60s per side" },
                new SessionExercise { WorkoutSessionID = sessionA.ID, ExerciseID = ex["Cat-Cow"],          SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "8" },

                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Bodyweight Squat"], SectionType = SectionType.Warmup,   OrderInSection = 1, Sets = 2, Reps = "10" },
                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Plank"],            SectionType = SectionType.Primer,   OrderInSection = 1, Sets = 2, Reps = "25s" },
                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Push-Up"],          SectionType = SectionType.MainWork, OrderInSection = 1, Sets = rounds, Reps = "15", RestSeconds = 35, Notes = note },
                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Bodyweight Squat"], SectionType = SectionType.MainWork, OrderInSection = 2, Sets = rounds, Reps = "20", RestSeconds = 35 },
                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Mountain Climber"], SectionType = SectionType.MainWork, OrderInSection = 3, Sets = rounds, Reps = "30s", RestSeconds = 35 },
                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Burpee"],           SectionType = SectionType.MainWork, OrderInSection = 4, Sets = rounds, Reps = "10", RestSeconds = 50 },
                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Hip 90/90 Stretch"],SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "60s per side" },
                new SessionExercise { WorkoutSessionID = sessionB.ID, ExerciseID = ex["Cat-Cow"],          SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "8" },

                new SessionExercise { WorkoutSessionID = sessionC.ID, ExerciseID = ex["Cat-Cow"],          SectionType = SectionType.Warmup,   OrderInSection = 1, Sets = 2, Reps = "8" },
                new SessionExercise { WorkoutSessionID = sessionC.ID, ExerciseID = ex["Mountain Climber"], SectionType = SectionType.Primer,   OrderInSection = 1, Sets = 1, Reps = "25s", Notes = "Working pace — not easy." },
                new SessionExercise { WorkoutSessionID = sessionC.ID, ExerciseID = ex["Bodyweight Squat"], SectionType = SectionType.MainWork, OrderInSection = 1, Sets = 5, Reps = "20", RestSeconds = 45, Notes = "Maintain a consistent pace across all sets." },
                new SessionExercise { WorkoutSessionID = sessionC.ID, ExerciseID = ex["Plank"],            SectionType = SectionType.MainWork, OrderInSection = 2, Sets = 3, Reps = "45s hold", RestSeconds = 35 },
                new SessionExercise { WorkoutSessionID = sessionC.ID, ExerciseID = ex["Mountain Climber"], SectionType = SectionType.MainWork, OrderInSection = 3, Sets = 4, Reps = "40s", RestSeconds = 35 },
                new SessionExercise { WorkoutSessionID = sessionC.ID, ExerciseID = ex["Hip 90/90 Stretch"],SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "60s per side" },
                new SessionExercise { WorkoutSessionID = sessionC.ID, ExerciseID = ex["Cat-Cow"],          SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "8" }
            };

            context.SessionExercises.AddRange(exercises);
            await context.SaveChangesAsync();
        }

        private static async Task SeedConditioningFinalWeek(FitContext context, Dictionary<string, int> ex, ProgramWeek week)
        {
            var testSession = new WorkoutSession { ProgramWeekID = week.ID, SessionTitle = "Week 8 — Final Test (Max Effort)", weekDay = WeekDay.Monday, EstimatedDuration = 45, WarmupNotes = "5 min jog. Cat-Cow 2×8. Bodyweight Squat 2×10.", PrimerNotes = "Mountain Climber 2×20s at working pace.", CooldownNotes = "5 min easy walk. Full stretching 10 min. Celebrate." };
            var taperA = new WorkoutSession { ProgramWeekID = week.ID, SessionTitle = "Week 8 — Active Recovery A", weekDay = WeekDay.Wednesday, EstimatedDuration = 25, WarmupNotes = "5 min easy walk.", CooldownNotes = "Full body stretching 10 min." };
            var taperB = new WorkoutSession { ProgramWeekID = week.ID, SessionTitle = "Week 8 — Active Recovery B", weekDay = WeekDay.Friday, EstimatedDuration = 25, WarmupNotes = "5 min easy walk. Cat-Cow 2×8.", CooldownNotes = "Full body stretching 10 min. Programme complete." };

            context.WorkoutSessions.AddRange(testSession, taperA, taperB);
            await context.SaveChangesAsync();

            var exercises = new List<SessionExercise>
            {
                new SessionExercise { WorkoutSessionID = testSession.ID, ExerciseID = ex["Bodyweight Squat"], SectionType = SectionType.Warmup,   OrderInSection = 1, Sets = 2, Reps = "10" },
                new SessionExercise { WorkoutSessionID = testSession.ID, ExerciseID = ex["Mountain Climber"], SectionType = SectionType.Primer,   OrderInSection = 1, Sets = 2, Reps = "20s" },
                new SessionExercise { WorkoutSessionID = testSession.ID, ExerciseID = ex["Burpee"],           SectionType = SectionType.MainWork, OrderInSection = 1, Sets = 4, Reps = "45s on / 15s off", Notes = "Push everything you have. This is your final test." },
                new SessionExercise { WorkoutSessionID = testSession.ID, ExerciseID = ex["Mountain Climber"], SectionType = SectionType.MainWork, OrderInSection = 2, Sets = 4, Reps = "45s on / 15s off" },
                new SessionExercise { WorkoutSessionID = testSession.ID, ExerciseID = ex["Box Jump"],         SectionType = SectionType.MainWork, OrderInSection = 3, Sets = 4, Reps = "10", RestSeconds = 40 },
                new SessionExercise { WorkoutSessionID = testSession.ID, ExerciseID = ex["Plank"],            SectionType = SectionType.MainWork, OrderInSection = 4, Sets = 3, Reps = "60s hold", Notes = "Finish strong." },
                new SessionExercise { WorkoutSessionID = testSession.ID, ExerciseID = ex["Hip 90/90 Stretch"],SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "90s per side" },
                new SessionExercise { WorkoutSessionID = testSession.ID, ExerciseID = ex["Cat-Cow"],          SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "10" },
                    
                new SessionExercise { WorkoutSessionID = taperA.ID, ExerciseID = ex["Bodyweight Squat"], SectionType = SectionType.MainWork, OrderInSection = 1, Sets = 2, Reps = "10", RestSeconds = 90, Notes = "Easy movement only." },
                new SessionExercise { WorkoutSessionID = taperA.ID, ExerciseID = ex["Plank"],            SectionType = SectionType.MainWork, OrderInSection = 2, Sets = 2, Reps = "20s" },
                new SessionExercise { WorkoutSessionID = taperA.ID, ExerciseID = ex["Hip 90/90 Stretch"],SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "90s per side" },
                new SessionExercise { WorkoutSessionID = taperA.ID, ExerciseID = ex["Cat-Cow"],          SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "10" },

                new SessionExercise { WorkoutSessionID = taperB.ID, ExerciseID = ex["Cat-Cow"],          SectionType = SectionType.Warmup,   OrderInSection = 1, Sets = 2, Reps = "8" },
                new SessionExercise { WorkoutSessionID = taperB.ID, ExerciseID = ex["Bodyweight Squat"], SectionType = SectionType.MainWork, OrderInSection = 1, Sets = 2, Reps = "10", Notes = "Light movement. Your body is recovering." },
                new SessionExercise { WorkoutSessionID = taperB.ID, ExerciseID = ex["Plank"],            SectionType = SectionType.MainWork, OrderInSection = 2, Sets = 2, Reps = "20s" },
                new SessionExercise { WorkoutSessionID = taperB.ID, ExerciseID = ex["Hip 90/90 Stretch"],SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "90s per side" },
                new SessionExercise { WorkoutSessionID = taperB.ID, ExerciseID = ex["Cat-Cow"],          SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "10", Notes = "Programme complete. Well done." }
            };

            context.SessionExercises.AddRange(exercises);
            await context.SaveChangesAsync();
        }
    }
}
