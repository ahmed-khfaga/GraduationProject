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

            var tracks = await context.Tracks.ToDictionaryAsync(t => t.Name, t => t.Id);
            var coaches = await context.Coachs.Include(c => c.ApplicationUser)
                .ToDictionaryAsync(c => c.ApplicationUser.Email!, c => c.Id);
            var ex = await context.Exercises.ToDictionaryAsync(e => e.Name, e => e.Id);

            if (!tracks.Any()) throw new InvalidOperationException("ProgramSeeder requires tracks.");
            if (!coaches.Any()) throw new InvalidOperationException("ProgramSeeder requires coaches.");
            if (!ex.Any()) throw new InvalidOperationException("ProgramSeeder requires exercises.");

            await SeedPushPullLegsProgram(context, ex, tracks["Strength & Hypertrophy"], coaches["ahmed.coach@fitzone.com"]);
            await SeedFatBurnProgram(context, ex, tracks["Conditioning"], coaches["nour.coach@fitzone.com"]);
        }

       
        // PROGRAM 1 — Push Pull Legs  (4 weeks)
       
        private static async Task SeedPushPullLegsProgram(FitContext context, Dictionary<string, int> ex, int trackId, int coachId)
        {
            var program = new WorkoutProgram
            {
                TrackId = trackId,
                CoachId = coachId,
                Name = "Push Pull Legs Foundation",
                Description = "A classic 3-day Push/Pull/Legs split for beginners and early intermediates who want to build a solid strength base.",
                ExpectedOutcome = "After 4 weeks you will have established a consistent training habit, improved your technique on all major lifts, and laid the foundation for long-term strength gains.",
                NextSteps = "Progress to an intermediate PPL program with a 6-day frequency, or add a specialisation block (arms, shoulders) before returning to a full-body strength phase.",
                DurationOnWeeks = 4,
                SessionsPerWeeks = 3,
                SessionsDuration = 60,
                TrainingGoal = TrainingGoal.BuildMuscle,
                FitnessLevel = FitnessLevel.Intermediate,
                EquipmentType = EquipmentType.FullGym,
                IsPublished = true,
                PublishedAt = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            context.WorkoutPrograms.Add(program);
            await context.SaveChangesAsync();

            var weeks = new[]
            {
                new ProgramWeek { WorkoutProgramId = program.Id, WeekNumber = 1, WeekDescription = "Introductory week — learn each movement pattern with moderate load. Leave 3 reps in the tank on every work set.", FocusArea = "Movement quality & baseline strength", ProgressionNote = "This is your starting point. Focus entirely on technique — the weight will come later.",                                           NextWeekPreview = "Week 2 adds 2.5–5 kg to every lift that felt comfortable. Earn those plates this week." },
                new ProgramWeek { WorkoutProgramId = program.Id, WeekNumber = 2, WeekDescription = "Add 2.5–5 kg to any lift that felt manageable last week. Leave 2 reps in the tank.",                             FocusArea = "Load progression",                   ProgressionNote = "The first progression step. Your nervous system has adapted — now load it.",                                              NextWeekPreview = "Week 3 is an intensity ramp. All main sets go to RPE 8 — prepare to work hard." },
                new ProgramWeek { WorkoutProgramId = program.Id, WeekNumber = 3, WeekDescription = "Intensity ramp — aim for RPE 8 on all main work sets. Your last rep should be a genuine effort.",                 FocusArea = "Intensity ramp",                     ProgressionNote = "Three weeks of loading done. This week you discover how strong you've actually become.",                                  NextWeekPreview = "Week 4 is a deload. Reduce all loads 40% — let your body absorb and consolidate what you've built." },
                new ProgramWeek { WorkoutProgramId = program.Id, WeekNumber = 4, WeekDescription = "Deload week — reduce all loads by 40% and focus on perfect execution. Let your body consolidate the adaptations.", FocusArea = "Active recovery & deload",            ProgressionNote = "Recovery is when growth actually happens. Lighter weights this week mean heavier weights next program.",                  NextWeekPreview = "You have completed the program. Move on to the intermediate PPL or take a full rest week before your next block." }
            };

            context.ProgramWeeks.AddRange(weeks);
            await context.SaveChangesAsync();

            await SeedPplWeek(context, ex, weeks[0], rpeOffset: 0, loadNote: "Introductory load — leave 3 reps in the tank.");
            await SeedPplWeek(context, ex, weeks[1], rpeOffset: 0, loadNote: "Add 2.5–5 kg vs last week where it felt easy.");
            await SeedPplWeek(context, ex, weeks[2], rpeOffset: 1, loadNote: "Push to RPE 8 — your last rep should be a real effort.");
            await SeedPplDeloadWeek(context, ex, weeks[3]);
        }

        private static async Task SeedPplWeek(FitContext context, Dictionary<string, int> ex, ProgramWeek week, int rpeOffset, string loadNote)
        {
            var push = new WorkoutSession { ProgramWeekId = week.Id, SessionTitle = "Push Day — Chest, Shoulders & Triceps", weekDay = WeekDay.Monday, DayOrder = 1, EstimatedDuration = 60, WarmupNotes = "5 min brisk walk or bike. Band Pull-Apart 2×15, Cat-Cow 2×8.", PrimerNotes = "Push-Up 2×10 bodyweight — pause 1s at bottom, feel the chest load.", CooldownNotes = "Chest doorframe stretch 30s each side. Cross-body shoulder stretch 30s each side." };
            var pull = new WorkoutSession { ProgramWeekId = week.Id, SessionTitle = "Pull Day — Back & Biceps", weekDay = WeekDay.Wednesday, DayOrder = 1, EstimatedDuration = 60, WarmupNotes = "5 min bike. Band Pull-Apart 2×15. Hip 90/90 stretch 60s per side.", PrimerNotes = "Dead hang from pull-up bar 2×20s — feel the shoulder blades depress.", CooldownNotes = "Lat doorframe stretch 30s each side. Supine spinal twist 60s per side." };
            var legs = new WorkoutSession { ProgramWeekId = week.Id, SessionTitle = "Legs Day — Quads, Hamstrings & Glutes", weekDay = WeekDay.Friday, DayOrder = 1, EstimatedDuration = 65, WarmupNotes = "5 min easy walk. Bodyweight Squat 2×10. Hip 90/90 stretch 1 min per side.", PrimerNotes = "Romanian Deadlift to Hip Hinge Walkout 2×8 — prime hamstrings before loading.", CooldownNotes = "Standing quad stretch 30s each. Kneeling hip-flexor stretch 40s each side." };

            context.WorkoutSessions.AddRange(push, pull, legs);
            await context.SaveChangesAsync();

            var pushEx = new List<SessionExercise>
            {
                new() { WorkoutSessionId = push.Id, ExerciseId = ex["Cat-Cow"],              SectionType = SectionType.Warmup,   OrderInSection = 1, Sets = 2, Reps = "8",    RestSeconds = 30,  Notes = "Slow and controlled — full flexion and extension on each rep." },
                new() { WorkoutSessionId = push.Id, ExerciseId = ex["Band Pull-Apart"],      SectionType = SectionType.Warmup,   OrderInSection = 2, Sets = 2, Reps = "15",   RestSeconds = 30,  Notes = "Keep arms straight. Shoulder prep, not a strength exercise." },
                new() { WorkoutSessionId = push.Id, ExerciseId = ex["Push-Up"],              SectionType = SectionType.Primer,   OrderInSection = 1, Sets = 2, Reps = "10",   RestSeconds = 60,  Notes = "Bodyweight only — pause 1s at bottom. Feel the chest stretch and load." },
                new() { WorkoutSessionId = push.Id, ExerciseId = ex["Barbell Bench Press"],  SectionType = SectionType.MainWork, OrderInSection = 1, Sets = 3, Reps = "8",    RestSeconds = 180, RPETarget = 7 + rpeOffset, Notes = loadNote + " Control the descent 2s, brief pause on chest, drive hard." },
                new() { WorkoutSessionId = push.Id, ExerciseId = ex["Dumbbell Incline Press"],SectionType = SectionType.MainWork, OrderInSection = 2, Sets = 3, Reps = "10",   RestSeconds = 120, RPETarget = 7 + rpeOffset, Notes = "Bench at 30°. Full stretch at bottom, squeeze chest at top." },
                new() { WorkoutSessionId = push.Id, ExerciseId = ex["Overhead Press"],       SectionType = SectionType.MainWork, OrderInSection = 3, Sets = 3, Reps = "10",   RestSeconds = 120, RPETarget = 7 + rpeOffset, Notes = "Strict — no leg drive. Brace core and squeeze glutes before each rep." },
                new() { WorkoutSessionId = push.Id, ExerciseId = ex["Push-Up"],              SectionType = SectionType.MainWork, OrderInSection = 4, Sets = 3, Reps = "AMRAP",RestSeconds = 90,  RPETarget = 8,             Notes = "Stop 1 rep before technical failure. Note your rep count each set." },
                new() { WorkoutSessionId = push.Id, ExerciseId = ex["Face Pull"],            SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "15",   RestSeconds = 45,  Notes = "Light weight — purely shoulder health. Elbows high, thumbs back at finish." },
                new() { WorkoutSessionId = push.Id, ExerciseId = ex["Band Pull-Apart"],      SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "15",   RestSeconds = 30,  Notes = "Final set — let rear delts and rhomboids decompress the shoulder." }
            };

            var pullEx = new List<SessionExercise>
            {
                new() { WorkoutSessionId = pull.Id, ExerciseId = ex["Band Pull-Apart"],      SectionType = SectionType.Warmup,   OrderInSection = 1, Sets = 2, Reps = "15",   RestSeconds = 30 },
                new() { WorkoutSessionId = pull.Id, ExerciseId = ex["Cat-Cow"],              SectionType = SectionType.Warmup,   OrderInSection = 2, Sets = 2, Reps = "8",    RestSeconds = 30,  Notes = "Wake up the spine before the heavy hinge." },
                new() { WorkoutSessionId = pull.Id, ExerciseId = ex["Romanian Deadlift to Hip Hinge Walkout"], SectionType = SectionType.Primer, OrderInSection = 1, Sets = 2, Reps = "6", RestSeconds = 60, Notes = "Bodyweight — feel hamstrings load on hinge and lats on walkout." },
                new() { WorkoutSessionId = pull.Id, ExerciseId = ex["Pull-Up"],              SectionType = SectionType.MainWork, OrderInSection = 1, Sets = 4, Reps = "5-6",  RestSeconds = 180, RPETarget = 7 + rpeOffset, Notes = loadNote + " Use band assistance if needed. Pull elbows to floor." },
                new() { WorkoutSessionId = pull.Id, ExerciseId = ex["Barbell Deadlift"],     SectionType = SectionType.MainWork, OrderInSection = 2, Sets = 3, Reps = "6",    RestSeconds = 210, RPETarget = 7 + rpeOffset, Notes = loadNote + " Reset each rep — bar stays against your legs throughout." },
                new() { WorkoutSessionId = pull.Id, ExerciseId = ex["Face Pull"],            SectionType = SectionType.MainWork, OrderInSection = 3, Sets = 3, Reps = "15",   RestSeconds = 60,  Notes = "Moderate weight. Superset with deadlift rest if time is tight." },
                new() { WorkoutSessionId = pull.Id, ExerciseId = ex["Hip 90/90 Stretch"],   SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "60s per side", RestSeconds = 15, Notes = "Breathe into the hip and hold. Do not force the range." },
                new() { WorkoutSessionId = pull.Id, ExerciseId = ex["Cat-Cow"],             SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "8",    RestSeconds = 15,  Notes = "Slow and deliberate — decompress the spine after heavy pulls." }
            };

            var legsEx = new List<SessionExercise>
            {
                new() { WorkoutSessionId = legs.Id, ExerciseId = ex["Bodyweight Squat"],    SectionType = SectionType.Warmup,   OrderInSection = 1, Sets = 2, Reps = "12",   RestSeconds = 45,  Notes = "Find your stance and depth — slow and controlled." },
                new() { WorkoutSessionId = legs.Id, ExerciseId = ex["Cat-Cow"],             SectionType = SectionType.Warmup,   OrderInSection = 2, Sets = 1, Reps = "8",    RestSeconds = 30,  Notes = "Wake up the lumbar spine before loading." },
                new() { WorkoutSessionId = legs.Id, ExerciseId = ex["Romanian Deadlift to Hip Hinge Walkout"], SectionType = SectionType.Primer, OrderInSection = 1, Sets = 2, Reps = "8", RestSeconds = 60 },
                new() { WorkoutSessionId = legs.Id, ExerciseId = ex["Barbell Back Squat"],  SectionType = SectionType.MainWork, OrderInSection = 1, Sets = 4, Reps = "6",    RestSeconds = 210, RPETarget = 7 + rpeOffset, Tempo = "3-1-1", Notes = loadNote + " 3s descent, 1s pause at depth, drive hard." },
                new() { WorkoutSessionId = legs.Id, ExerciseId = ex["Dumbbell Romanian Deadlift"], SectionType = SectionType.MainWork, OrderInSection = 2, Sets = 3, Reps = "10", RestSeconds = 120, RPETarget = 7 + rpeOffset, Notes = "Feel hamstring stretch at bottom. Do not round back to reach lower." },
                new() { WorkoutSessionId = legs.Id, ExerciseId = ex["Goblet Squat"],        SectionType = SectionType.MainWork, OrderInSection = 3, Sets = 3, Reps = "12",   RestSeconds = 90,  Notes = "Finisher — lighter weight, full range, feel the quads the whole way down." },
                new() { WorkoutSessionId = legs.Id, ExerciseId = ex["Hip 90/90 Stretch"],   SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "90s per side", RestSeconds = 15 },
                new() { WorkoutSessionId = legs.Id, ExerciseId = ex["Cat-Cow"],             SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "8",    RestSeconds = 15,  Notes = "Finish every leg session with this — decompress the lumbar spine." }
            };

            context.SessionExercises.AddRange(pushEx);
            context.SessionExercises.AddRange(pullEx);
            context.SessionExercises.AddRange(legsEx);
            await context.SaveChangesAsync();
        }

        private static async Task SeedPplDeloadWeek(FitContext context, Dictionary<string, int> ex, ProgramWeek week)
        {
            var push = new WorkoutSession { ProgramWeekId = week.Id, SessionTitle = "Deload Push", weekDay = WeekDay.Monday, DayOrder = 1, EstimatedDuration = 40, WarmupNotes = "5 min easy walk. Band Pull-Apart 2×10.", PrimerNotes = "Push-Up 1×10 — feel the chest, no effort.", CooldownNotes = "Chest and shoulder stretches 5 min." };
            var pull = new WorkoutSession { ProgramWeekId = week.Id, SessionTitle = "Deload Pull", weekDay = WeekDay.Wednesday, DayOrder = 1, EstimatedDuration = 40, WarmupNotes = "5 min easy walk. Cat-Cow 2×8.", PrimerNotes = "Dead hang 1×20s — just hang and breathe.", CooldownNotes = "Lat stretch and supine twist 5 min." };
            var legs = new WorkoutSession { ProgramWeekId = week.Id, SessionTitle = "Deload Legs", weekDay = WeekDay.Friday, DayOrder = 1, EstimatedDuration = 40, WarmupNotes = "5 min easy walk. Bodyweight Squat 1×10.", PrimerNotes = "Hip 90/90 stretch 1 min per side.", CooldownNotes = "Full lower body stretch 10 min. You earned it." };

            context.WorkoutSessions.AddRange(push, pull, legs);
            await context.SaveChangesAsync();

            context.SessionExercises.AddRange(
                new SessionExercise { WorkoutSessionId = push.Id, ExerciseId = ex["Band Pull-Apart"], SectionType = SectionType.Warmup, OrderInSection = 1, Sets = 2, Reps = "10", Notes = "Light activation only." },
                new SessionExercise { WorkoutSessionId = push.Id, ExerciseId = ex["Barbell Bench Press"], SectionType = SectionType.MainWork, OrderInSection = 1, Sets = 2, Reps = "10", RestSeconds = 90, Notes = "60% of Week 3 load. Perfect bar path." },
                new SessionExercise { WorkoutSessionId = push.Id, ExerciseId = ex["Overhead Press"], SectionType = SectionType.MainWork, OrderInSection = 2, Sets = 2, Reps = "10", RestSeconds = 90, Notes = "60% of Week 3 load." },
                new SessionExercise { WorkoutSessionId = push.Id, ExerciseId = ex["Face Pull"], SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "15", Notes = "Light. Shoulder health." },
                new SessionExercise { WorkoutSessionId = push.Id, ExerciseId = ex["Cat-Cow"], SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 1, Reps = "10" },
                new SessionExercise { WorkoutSessionId = pull.Id, ExerciseId = ex["Cat-Cow"], SectionType = SectionType.Warmup, OrderInSection = 1, Sets = 2, Reps = "8" },
                new SessionExercise { WorkoutSessionId = pull.Id, ExerciseId = ex["Pull-Up"], SectionType = SectionType.MainWork, OrderInSection = 1, Sets = 2, Reps = "5", RestSeconds = 120, Notes = "60% effort — use a band. Smooth, controlled reps." },
                new SessionExercise { WorkoutSessionId = pull.Id, ExerciseId = ex["Barbell Deadlift"], SectionType = SectionType.MainWork, OrderInSection = 2, Sets = 2, Reps = "5", RestSeconds = 150, Notes = "60% of Week 3 load. Reset each rep with care." },
                new SessionExercise { WorkoutSessionId = pull.Id, ExerciseId = ex["Hip 90/90 Stretch"], SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "90s per side" },
                new SessionExercise { WorkoutSessionId = pull.Id, ExerciseId = ex["Cat-Cow"], SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 1, Reps = "10" },
                new SessionExercise { WorkoutSessionId = legs.Id, ExerciseId = ex["Bodyweight Squat"], SectionType = SectionType.Warmup, OrderInSection = 1, Sets = 2, Reps = "10" },
                new SessionExercise { WorkoutSessionId = legs.Id, ExerciseId = ex["Barbell Back Squat"], SectionType = SectionType.MainWork, OrderInSection = 1, Sets = 2, Reps = "10", RestSeconds = 90, Tempo = "3-1-1", Notes = "60% of Week 3 load. Dial in the technique." },
                new SessionExercise { WorkoutSessionId = legs.Id, ExerciseId = ex["Dumbbell Romanian Deadlift"], SectionType = SectionType.MainWork, OrderInSection = 2, Sets = 2, Reps = "10", RestSeconds = 90, Notes = "60% of Week 3 load. Feel every rep." },
                new SessionExercise { WorkoutSessionId = legs.Id, ExerciseId = ex["Hip 90/90 Stretch"], SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "90s per side" },
                new SessionExercise { WorkoutSessionId = legs.Id, ExerciseId = ex["Cat-Cow"], SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "10" }
            );
            await context.SaveChangesAsync();
        }

       
        // PROGRAM 2 — 8-Week Fat Burn Conditioning
        
        private static async Task SeedFatBurnProgram(FitContext context, Dictionary<string, int> ex, int trackId, int coachId)
        {
            var program = new WorkoutProgram
            {
                TrackId = trackId,
                CoachId = coachId,
                Name = "8-Week Fat Burn Conditioning",
                Description = "A 3-day per week conditioning program that maximises calorie burn and cardiovascular adaptation through rotating HIIT, circuit, and endurance work.",
                ExpectedOutcome = "Measurably improved cardiovascular capacity, reduced body fat, and the ability to sustain higher-intensity effort for longer periods.",
                NextSteps = "Progress to an intermediate conditioning block with 4 sessions per week, or add resistance training 2× per week alongside this program for body recomposition.",
                DurationOnWeeks = 8,
                SessionsPerWeeks = 3,
                SessionsDuration = 45,
                TrainingGoal = TrainingGoal.LoseFat,
                FitnessLevel = FitnessLevel.Beginner,
                EquipmentType = EquipmentType.Bodyweight,
                IsPublished = true,
                PublishedAt = new DateTime(2025, 12, 15, 0, 0, 0, DateTimeKind.Utc)
            };

            context.WorkoutPrograms.Add(program);
            await context.SaveChangesAsync();

            var weeks = new[]
            {
                new ProgramWeek { WorkoutProgramId = program.Id, WeekNumber = 1, WeekDescription = "Orientation week — learn the movements and find your baseline.", FocusArea = "Foundation",        ProgressionNote = "Start here. Every expert was once a beginner. Focus on form, not speed.",          NextWeekPreview = "Week 2 reduces rest periods by 10s — the circuit will start to feel like real work." },
                new ProgramWeek { WorkoutProgramId = program.Id, WeekNumber = 2, WeekDescription = "Reduce rest periods by 10s vs last week.",                       FocusArea = "Reducing Rest",     ProgressionNote = "Less rest = higher average heart rate = more calories burned. You've earned this.", NextWeekPreview = "Week 3 adds a 4th round to Session B — volume is climbing." },
                new ProgramWeek { WorkoutProgramId = program.Id, WeekNumber = 3, WeekDescription = "Add a 4th round to Session B.",                                  FocusArea = "Volume Increase",   ProgressionNote = "More volume means more total work and a stronger metabolic stimulus.",               NextWeekPreview = "Week 4 is a deload. Dial it back — this is when your body adapts." },
                new ProgramWeek { WorkoutProgramId = program.Id, WeekNumber = 4, WeekDescription = "Deload week — all sessions at 60% effort.",                      FocusArea = "Deload",            ProgressionNote = "Strategic rest is not weakness — it is how your body consolidates the last 3 weeks.", NextWeekPreview = "Week 5 starts the intensity block. Shorter rest, harder intervals." },
                new ProgramWeek { WorkoutProgramId = program.Id, WeekNumber = 5, WeekDescription = "Intensity block — shorten all work-to-rest ratios by 10s.",      FocusArea = "Intensity Phase",   ProgressionNote = "You are fitter now than when you started. This week you will feel that difference.",  NextWeekPreview = "Week 6 adds a 4th round to ALL sessions — peak volume." },
                new ProgramWeek { WorkoutProgramId = program.Id, WeekNumber = 6, WeekDescription = "4 rounds on every session.",                                     FocusArea = "Peak Volume",       ProgressionNote = "Highest volume of the program. Push through — this is where fitness is made.",       NextWeekPreview = "Week 7 is maximum effort. Every set, every interval — leave nothing." },
                new ProgramWeek { WorkoutProgramId = program.Id, WeekNumber = 7, WeekDescription = "Max effort week — push as hard as possible on every interval.",  FocusArea = "Peak Intensity",    ProgressionNote = "You have been building to this. Give everything.",                                    NextWeekPreview = "Week 8 is the final test and taper. Show how far you have come." },
                new ProgramWeek { WorkoutProgramId = program.Id, WeekNumber = 8, WeekDescription = "Test and taper — one final max-effort session, then recovery.",  FocusArea = "Test & Taper",      ProgressionNote = "Compare your performance to Week 1 — the difference is your 8-week result.",         NextWeekPreview = "Program complete. Move on to an intermediate conditioning block or begin adding resistance training." }
            };

            context.ProgramWeeks.AddRange(weeks);
            await context.SaveChangesAsync();

            // Seed only Week 1 in detail — the rest follow the same structure with progressive overload
            await SeedConditioningWeek(context, ex, weeks[0],
                weekNum: 1, rounds: 3, workInterval: "30s on / 30s off", restReduction: "Rest generously — orientation week.");

            await SeedConditioningWeek(context, ex, weeks[1],
                weekNum: 2, rounds: 3, workInterval: "30s on / 20s off", restReduction: "10s less rest vs Week 1.");

            await SeedConditioningWeek(context, ex, weeks[2],
                weekNum: 3, rounds: 4, workInterval: "30s on / 20s off", restReduction: "Session B adds a 4th round today.");

            await SeedConditioningDeloadWeek(context, ex, weeks[3]);

            await SeedConditioningWeek(context, ex, weeks[4],
                weekNum: 5, rounds: 3, workInterval: "40s on / 20s off", restReduction: "Shorter rest, longer work interval.");

            await SeedConditioningWeek(context, ex, weeks[5],
                weekNum: 6, rounds: 4, workInterval: "40s on / 20s off", restReduction: "4 rounds on every session.");

            await SeedConditioningWeek(context, ex, weeks[6],
                weekNum: 7, rounds: 4, workInterval: "45s on / 15s off", restReduction: "Max effort — push everything you have.");

            await SeedConditioningFinalWeek(context, ex, weeks[7]);
        }

        private static async Task SeedConditioningWeek(FitContext context, Dictionary<string, int> ex, ProgramWeek week,
            int weekNum, int rounds, string workInterval, string restReduction)
        {
            var sessionA = new WorkoutSession { ProgramWeekId = week.Id, SessionTitle = $"Week {weekNum} — Session A HIIT", weekDay = WeekDay.Monday, DayOrder = 1, EstimatedDuration = 40, WarmupNotes = "5 min easy march. Cat-Cow 2×8. Bodyweight Squat 2×10.", PrimerNotes = "Mountain Climber 2×20s easy pace.", CooldownNotes = "Hip 90/90 60s per side. Easy walk 2 min." };
            var sessionB = new WorkoutSession { ProgramWeekId = week.Id, SessionTitle = $"Week {weekNum} — Session B Circuit", weekDay = WeekDay.Wednesday, DayOrder = 1, EstimatedDuration = 42, WarmupNotes = "5 min march. Bodyweight Squat 2×10. Push-Up 2×5.", PrimerNotes = "Plank 2×20s — brace before the circuit.", CooldownNotes = "Full body stretch 5 min." };
            var sessionC = new WorkoutSession { ProgramWeekId = week.Id, SessionTitle = $"Week {weekNum} — Session C Endurance", weekDay = WeekDay.Friday, DayOrder = 1, EstimatedDuration = 40, WarmupNotes = "5 min easy movement. Cat-Cow 2×8.", PrimerNotes = "Mountain Climber 1×20s easy.", CooldownNotes = "Hip 90/90 60s per side. Supine twist 60s per side." };

            context.WorkoutSessions.AddRange(sessionA, sessionB, sessionC);
            await context.SaveChangesAsync();

            context.SessionExercises.AddRange(
                // Session A
                new SessionExercise { WorkoutSessionId = sessionA.Id, ExerciseId = ex["Bodyweight Squat"], SectionType = SectionType.Warmup, OrderInSection = 1, Sets = 2, Reps = "10" },
                new SessionExercise { WorkoutSessionId = sessionA.Id, ExerciseId = ex["Cat-Cow"], SectionType = SectionType.Warmup, OrderInSection = 2, Sets = 2, Reps = "8" },
                new SessionExercise { WorkoutSessionId = sessionA.Id, ExerciseId = ex["Mountain Climber"], SectionType = SectionType.Primer, OrderInSection = 1, Sets = 2, Reps = "20s", Notes = "Easy pace — just warming up." },
                new SessionExercise { WorkoutSessionId = sessionA.Id, ExerciseId = ex["Burpee"], SectionType = SectionType.MainWork, OrderInSection = 1, Sets = rounds, Reps = workInterval, Notes = restReduction },
                new SessionExercise { WorkoutSessionId = sessionA.Id, ExerciseId = ex["Mountain Climber"], SectionType = SectionType.MainWork, OrderInSection = 2, Sets = rounds, Reps = workInterval },
                new SessionExercise { WorkoutSessionId = sessionA.Id, ExerciseId = ex["Box Jump"], SectionType = SectionType.MainWork, OrderInSection = 3, Sets = rounds, Reps = "6", RestSeconds = 50, Notes = "Step down between reps. Quality over speed." },
                new SessionExercise { WorkoutSessionId = sessionA.Id, ExerciseId = ex["Hip 90/90 Stretch"], SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "60s per side" },
                new SessionExercise { WorkoutSessionId = sessionA.Id, ExerciseId = ex["Cat-Cow"], SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "8" },
                // Session B
                new SessionExercise { WorkoutSessionId = sessionB.Id, ExerciseId = ex["Bodyweight Squat"], SectionType = SectionType.Warmup, OrderInSection = 1, Sets = 2, Reps = "10" },
                new SessionExercise { WorkoutSessionId = sessionB.Id, ExerciseId = ex["Plank"], SectionType = SectionType.Primer, OrderInSection = 1, Sets = 2, Reps = "20s" },
                new SessionExercise { WorkoutSessionId = sessionB.Id, ExerciseId = ex["Push-Up"], SectionType = SectionType.MainWork, OrderInSection = 1, Sets = rounds, Reps = "12", RestSeconds = 40, Notes = $"{rounds} rounds. " + restReduction },
                new SessionExercise { WorkoutSessionId = sessionB.Id, ExerciseId = ex["Bodyweight Squat"], SectionType = SectionType.MainWork, OrderInSection = 2, Sets = rounds, Reps = "15", RestSeconds = 40 },
                new SessionExercise { WorkoutSessionId = sessionB.Id, ExerciseId = ex["Mountain Climber"], SectionType = SectionType.MainWork, OrderInSection = 3, Sets = rounds, Reps = "20s", RestSeconds = 40 },
                new SessionExercise { WorkoutSessionId = sessionB.Id, ExerciseId = ex["Burpee"], SectionType = SectionType.MainWork, OrderInSection = 4, Sets = rounds, Reps = "8", RestSeconds = 55 },
                new SessionExercise { WorkoutSessionId = sessionB.Id, ExerciseId = ex["Hip 90/90 Stretch"], SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "60s per side" },
                new SessionExercise { WorkoutSessionId = sessionB.Id, ExerciseId = ex["Cat-Cow"], SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "8" },
                // Session C
                new SessionExercise { WorkoutSessionId = sessionC.Id, ExerciseId = ex["Cat-Cow"], SectionType = SectionType.Warmup, OrderInSection = 1, Sets = 2, Reps = "8" },
                new SessionExercise { WorkoutSessionId = sessionC.Id, ExerciseId = ex["Mountain Climber"], SectionType = SectionType.Primer, OrderInSection = 1, Sets = 1, Reps = "20s", Notes = "Easy pace." },
                new SessionExercise { WorkoutSessionId = sessionC.Id, ExerciseId = ex["Bodyweight Squat"], SectionType = SectionType.MainWork, OrderInSection = 1, Sets = 4, Reps = "20", RestSeconds = 55, Notes = "Steady endurance pace." },
                new SessionExercise { WorkoutSessionId = sessionC.Id, ExerciseId = ex["Plank"], SectionType = SectionType.MainWork, OrderInSection = 2, Sets = 3, Reps = "35s hold", RestSeconds = 40 },
                new SessionExercise { WorkoutSessionId = sessionC.Id, ExerciseId = ex["Mountain Climber"], SectionType = SectionType.MainWork, OrderInSection = 3, Sets = 3, Reps = "40s", RestSeconds = 40 },
                new SessionExercise { WorkoutSessionId = sessionC.Id, ExerciseId = ex["Hip 90/90 Stretch"], SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "60s per side" },
                new SessionExercise { WorkoutSessionId = sessionC.Id, ExerciseId = ex["Cat-Cow"], SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "8" }
            );
            await context.SaveChangesAsync();
        }

        private static async Task SeedConditioningDeloadWeek(FitContext context, Dictionary<string, int> ex, ProgramWeek week)
        {
            var a = new WorkoutSession { ProgramWeekId = week.Id, SessionTitle = "Deload — Active Recovery A", weekDay = WeekDay.Monday, DayOrder = 1, EstimatedDuration = 30 };
            var b = new WorkoutSession { ProgramWeekId = week.Id, SessionTitle = "Deload — Active Recovery B", weekDay = WeekDay.Wednesday, DayOrder = 1, EstimatedDuration = 30 };
            var c = new WorkoutSession { ProgramWeekId = week.Id, SessionTitle = "Deload — Active Recovery C", weekDay = WeekDay.Friday, DayOrder = 1, EstimatedDuration = 30 };

            context.WorkoutSessions.AddRange(a, b, c);
            await context.SaveChangesAsync();

            context.SessionExercises.AddRange(
                new SessionExercise { WorkoutSessionId = a.Id, ExerciseId = ex["Bodyweight Squat"], SectionType = SectionType.MainWork, OrderInSection = 1, Sets = 2, Reps = "10", RestSeconds = 90, Notes = "60% effort — recovery only." },
                new SessionExercise { WorkoutSessionId = a.Id, ExerciseId = ex["Push-Up"], SectionType = SectionType.MainWork, OrderInSection = 2, Sets = 2, Reps = "8", RestSeconds = 90 },
                new SessionExercise { WorkoutSessionId = a.Id, ExerciseId = ex["Hip 90/90 Stretch"], SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "90s per side" },
                new SessionExercise { WorkoutSessionId = a.Id, ExerciseId = ex["Cat-Cow"], SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "8" },
                new SessionExercise { WorkoutSessionId = b.Id, ExerciseId = ex["Plank"], SectionType = SectionType.MainWork, OrderInSection = 1, Sets = 2, Reps = "20s", RestSeconds = 60 },
                new SessionExercise { WorkoutSessionId = b.Id, ExerciseId = ex["Mountain Climber"], SectionType = SectionType.MainWork, OrderInSection = 2, Sets = 2, Reps = "20s", RestSeconds = 60 },
                new SessionExercise { WorkoutSessionId = b.Id, ExerciseId = ex["Hip 90/90 Stretch"], SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "90s per side" },
                new SessionExercise { WorkoutSessionId = b.Id, ExerciseId = ex["Cat-Cow"], SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "8" },
                new SessionExercise { WorkoutSessionId = c.Id, ExerciseId = ex["Bodyweight Squat"], SectionType = SectionType.MainWork, OrderInSection = 1, Sets = 2, Reps = "15", RestSeconds = 90 },
                new SessionExercise { WorkoutSessionId = c.Id, ExerciseId = ex["Plank"], SectionType = SectionType.MainWork, OrderInSection = 2, Sets = 2, Reps = "25s", RestSeconds = 60 },
                new SessionExercise { WorkoutSessionId = c.Id, ExerciseId = ex["Hip 90/90 Stretch"], SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "90s per side" },
                new SessionExercise { WorkoutSessionId = c.Id, ExerciseId = ex["Cat-Cow"], SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "10", Notes = "Decompress and recover." }
            );
            await context.SaveChangesAsync();
        }

        private static async Task SeedConditioningFinalWeek(FitContext context, Dictionary<string, int> ex, ProgramWeek week)
        {
            var test = new WorkoutSession { ProgramWeekId = week.Id, SessionTitle = "Week 8 — Final Test (Max Effort)", weekDay = WeekDay.Monday, DayOrder = 1, EstimatedDuration = 45 };
            var taperA = new WorkoutSession { ProgramWeekId = week.Id, SessionTitle = "Week 8 — Active Recovery A", weekDay = WeekDay.Wednesday, DayOrder = 1, EstimatedDuration = 25 };
            var taperB = new WorkoutSession { ProgramWeekId = week.Id, SessionTitle = "Week 8 — Active Recovery B", weekDay = WeekDay.Friday, DayOrder = 1, EstimatedDuration = 25 };

            context.WorkoutSessions.AddRange(test, taperA, taperB);
            await context.SaveChangesAsync();

            context.SessionExercises.AddRange(
                new SessionExercise { WorkoutSessionId = test.Id, ExerciseId = ex["Burpee"], SectionType = SectionType.MainWork, OrderInSection = 1, Sets = 4, Reps = "45s on / 15s off", Notes = "Push everything you have. This is your final test." },
                new SessionExercise { WorkoutSessionId = test.Id, ExerciseId = ex["Mountain Climber"], SectionType = SectionType.MainWork, OrderInSection = 2, Sets = 4, Reps = "45s on / 15s off" },
                new SessionExercise { WorkoutSessionId = test.Id, ExerciseId = ex["Box Jump"], SectionType = SectionType.MainWork, OrderInSection = 3, Sets = 4, Reps = "10", RestSeconds = 40 },
                new SessionExercise { WorkoutSessionId = test.Id, ExerciseId = ex["Plank"], SectionType = SectionType.MainWork, OrderInSection = 4, Sets = 3, Reps = "60s hold", Notes = "Finish strong." },
                new SessionExercise { WorkoutSessionId = test.Id, ExerciseId = ex["Hip 90/90 Stretch"], SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "90s per side" },
                new SessionExercise { WorkoutSessionId = test.Id, ExerciseId = ex["Cat-Cow"], SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "10" },
                new SessionExercise { WorkoutSessionId = taperA.Id, ExerciseId = ex["Bodyweight Squat"], SectionType = SectionType.MainWork, OrderInSection = 1, Sets = 2, Reps = "10", Notes = "Easy movement only." },
                new SessionExercise { WorkoutSessionId = taperA.Id, ExerciseId = ex["Plank"], SectionType = SectionType.MainWork, OrderInSection = 2, Sets = 2, Reps = "20s" },
                new SessionExercise { WorkoutSessionId = taperA.Id, ExerciseId = ex["Hip 90/90 Stretch"], SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "90s per side" },
                new SessionExercise { WorkoutSessionId = taperB.Id, ExerciseId = ex["Bodyweight Squat"], SectionType = SectionType.MainWork, OrderInSection = 1, Sets = 2, Reps = "10", Notes = "Your body is recovering. Easy." },
                new SessionExercise { WorkoutSessionId = taperB.Id, ExerciseId = ex["Plank"], SectionType = SectionType.MainWork, OrderInSection = 2, Sets = 2, Reps = "20s" },
                new SessionExercise { WorkoutSessionId = taperB.Id, ExerciseId = ex["Hip 90/90 Stretch"], SectionType = SectionType.Cooldown, OrderInSection = 1, Sets = 2, Reps = "90s per side" },
                new SessionExercise { WorkoutSessionId = taperB.Id, ExerciseId = ex["Cat-Cow"], SectionType = SectionType.Cooldown, OrderInSection = 2, Sets = 2, Reps = "10", Notes = "Programme complete. Well done." }
            );
            await context.SaveChangesAsync();
        }
    }
}
