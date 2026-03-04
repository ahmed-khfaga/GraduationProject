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
    public static class ExerciseSeeder
    {
        public static async Task SeedAsync(FitContext context)
        {
            if (await context.Exercises.AnyAsync()) return;

            var exercises = new List<Exercise>
            {
                // ── Compound / Strength ──────────────────────────────
                new Exercise
                {
                    Name             = "Barbell Back Squat",
                    Description      = "The king of lower-body exercises. Develops quad, glute, and hamstring strength simultaneously.",
                    PrimaryMuscles   = "Quadriceps, Glutes",
                    SecondaryMuscles = "Hamstrings, Core, Erector Spinae",
                    EquipmentNeeded  = "Barbell, Squat Rack",
                    FitnessLevel     = FitnessLevel.Intermediate,
                    VideoUrl         = "https://www.youtube.com/watch?v=ultWZbUMPL8",
                    Instructions     = "Unrack the bar with it resting on your traps. Stand with feet shoulder-width apart, " +
                                       "toes slightly out. Brace your core, break at the hips and knees simultaneously, " +
                                       "descend until thighs are at least parallel to the floor. Drive through the whole foot to stand.",
                    CommonMistakes   = "Caving knees inward (valgus), heels rising, excessive forward lean, not reaching depth."
                },
                new Exercise
                {
                    Name             = "Barbell Deadlift",
                    Description      = "A full-body pull that builds posterior chain strength and total-body power.",
                    PrimaryMuscles   = "Hamstrings, Glutes, Erector Spinae",
                    SecondaryMuscles = "Traps, Lats, Core, Forearms",
                    EquipmentNeeded  = "Barbell",
                    FitnessLevel     = FitnessLevel.Intermediate,
                    VideoUrl         = "https://www.youtube.com/watch?v=op9kVnSso6Q",
                    Instructions     = "Stand over the bar with feet hip-width apart, bar over mid-foot. Hinge at hips, " +
                                       "grip just outside your legs. Flatten your back, take a big breath, brace your core. " +
                                       "Push the floor away as you drive your hips forward to lockout.",
                    CommonMistakes   = "Rounded lower back, bar drifting away from the body, jerking the bar off the floor."
                },
                new Exercise
                {
                    Name             = "Barbell Bench Press",
                    Description      = "The primary horizontal push. Develops chest, shoulder, and tricep strength.",
                    PrimaryMuscles   = "Pectoralis Major",
                    SecondaryMuscles = "Anterior Deltoid, Triceps",
                    EquipmentNeeded  = "Barbell, Bench",
                    FitnessLevel     = FitnessLevel.Beginner,
                    VideoUrl         = "https://www.youtube.com/watch?v=vcBig73ojpE",
                    Instructions     = "Lie on a flat bench, grip the bar slightly wider than shoulder-width. " +
                                       "Lower the bar to your lower chest with control, elbows at roughly 45 degrees. " +
                                       "Press back to full extension.",
                    CommonMistakes   = "Flaring elbows out to 90 degrees, bouncing the bar off the chest, unstable wrist position."
                },
                new Exercise
                {
                    Name             = "Pull-Up",
                    Description      = "The gold-standard bodyweight pulling movement for back and bicep development.",
                    PrimaryMuscles   = "Latissimus Dorsi",
                    SecondaryMuscles = "Biceps, Rear Deltoid, Rhomboids",
                    EquipmentNeeded  = "Pull-Up Bar",
                    FitnessLevel     = FitnessLevel.Intermediate,
                    VideoUrl         = "https://www.youtube.com/watch?v=eGo4IYlbE5g",
                    Instructions     = "Hang from a bar with an overhand grip, hands slightly wider than shoulder-width. " +
                                       "Depress your shoulder blades, then pull your chest toward the bar. Lower with control.",
                    CommonMistakes   = "Using momentum / kipping, not achieving full range, shrugging the shoulders."
                },
                new Exercise
                {
                    Name             = "Overhead Press",
                    Description      = "The strict vertical push. Builds shoulder mass and overhead stability.",
                    PrimaryMuscles   = "Anterior Deltoid, Lateral Deltoid",
                    SecondaryMuscles = "Triceps, Traps, Core",
                    EquipmentNeeded  = "Barbell",
                    FitnessLevel     = FitnessLevel.Intermediate,
                    VideoUrl         = "https://www.youtube.com/watch?v=2yjwXTZQDDI",
                    Instructions     = "Hold the bar at collarbone height with a grip just outside shoulder-width. " +
                                       "Brace your core and press the bar overhead, finishing with arms fully locked out " +
                                       "and the bar directly over your mid-foot.",
                    CommonMistakes   = "Excessive lower-back extension, letting the bar drift forward, using leg drive on a strict press."
                },

                // ── Dumbbell ─────────────────────────────────────────
                new Exercise
                {
                    Name             = "Dumbbell Romanian Deadlift",
                    Description      = "A hamstring and glute hinge pattern that improves posterior-chain flexibility and strength.",
                    PrimaryMuscles   = "Hamstrings, Glutes",
                    SecondaryMuscles = "Erector Spinae, Core",
                    EquipmentNeeded  = "Dumbbells",
                    FitnessLevel     = FitnessLevel.Beginner,
                    VideoUrl         = "https://www.youtube.com/watch?v=hCDzSR6bW10",
                    Instructions     = "Hold dumbbells in front of your thighs. Push your hips back while lowering the weights " +
                                       "along your legs, keeping a flat back. Feel the stretch in your hamstrings, then drive " +
                                       "your hips forward to stand.",
                    CommonMistakes   = "Bending the knees too much (turns it into a squat), rounding the lower back."
                },
                new Exercise
                {
                    Name             = "Dumbbell Incline Press",
                    Description      = "Upper chest development with more shoulder-friendly mechanics than a barbell.",
                    PrimaryMuscles   = "Upper Pectoralis Major",
                    SecondaryMuscles = "Anterior Deltoid, Triceps",
                    EquipmentNeeded  = "Dumbbells, Incline Bench",
                    FitnessLevel     = FitnessLevel.Beginner,
                    VideoUrl         = "https://www.youtube.com/watch?v=8iPEnn-ltC8",
                    Instructions     = "Set the bench to 30–45 degrees. Press the dumbbells from chest height to full extension, " +
                                       "keeping a slight arch in your lower back and shoulder blades retracted.",
                    CommonMistakes   = "Setting the incline too steep (becomes a shoulder press), elbows flaring wide."
                },
                new Exercise
                {
                    Name             = "Goblet Squat",
                    Description      = "An excellent squat pattern for beginners that teaches depth and upright torso mechanics.",
                    PrimaryMuscles   = "Quadriceps, Glutes",
                    SecondaryMuscles = "Core, Adductors",
                    EquipmentNeeded  = "Dumbbell or Kettlebell",
                    FitnessLevel     = FitnessLevel.Beginner,
                    VideoUrl         = "https://www.youtube.com/watch?v=MxsFDhcyFyE",
                    Instructions     = "Hold a dumbbell vertically at your chest. Squat down between your knees, keeping your " +
                                       "chest tall and elbows inside your knees at the bottom. Stand explosively.",
                    CommonMistakes   = "Letting the chest fall forward, not reaching depth, heels lifting."
                },

                // ── Bodyweight ───────────────────────────────────────
                new Exercise
                {
                    Name             = "Push-Up",
                    Description      = "The foundational horizontal push movement accessible to everyone, anywhere.",
                    PrimaryMuscles   = "Pectoralis Major",
                    SecondaryMuscles = "Anterior Deltoid, Triceps, Core",
                    EquipmentNeeded  = "Bodyweight",
                    FitnessLevel     = FitnessLevel.Beginner,
                    VideoUrl         = "https://www.youtube.com/watch?v=IODxDxX7oi4",
                    Instructions     = "Place hands slightly wider than shoulder-width, body in a rigid plank. Lower your chest " +
                                       "to within an inch of the floor with elbows at 45 degrees. Press back to full extension.",
                    CommonMistakes   = "Sagging hips, flaring elbows to 90 degrees, partial range of motion."
                },
                new Exercise
                {
                    Name             = "Bodyweight Squat",
                    Description      = "Foundation of lower-body movement. Perfect for warm-ups and beginners building the pattern.",
                    PrimaryMuscles   = "Quadriceps, Glutes",
                    SecondaryMuscles = "Hamstrings, Core",
                    EquipmentNeeded  = "Bodyweight",
                    FitnessLevel     = FitnessLevel.Beginner,
                    VideoUrl         = "https://www.youtube.com/watch?v=aclHkVaku9U",
                    Instructions     = "Stand with feet shoulder-width apart, arms extended forward for balance. Sit back and down, " +
                                       "keeping chest up and knees tracking over toes. Drive through heels to stand.",
                    CommonMistakes   = "Heels rising, knees caving inward, excessive forward lean."
                },
                new Exercise
                {
                    Name             = "Plank",
                    Description      = "An isometric core exercise that builds anti-extension strength and spinal stability.",
                    PrimaryMuscles   = "Core (Transverse Abdominis, Rectus Abdominis)",
                    SecondaryMuscles = "Glutes, Shoulder Girdle",
                    EquipmentNeeded  = "Bodyweight",
                    FitnessLevel     = FitnessLevel.Beginner,
                    VideoUrl         = "https://www.youtube.com/watch?v=ASdvN_XEl_c",
                    Instructions     = "Rest on forearms and toes with a straight body from head to heels. Brace your core, " +
                                       "squeeze your glutes, and hold without sagging or piking your hips.",
                    CommonMistakes   = "Hips too high or sagging, holding the breath, neck craning up."
                },
                new Exercise
                {
                    Name             = "Burpee",
                    Description      = "A full-body conditioning exercise combining a squat, push-up, and jump into one movement.",
                    PrimaryMuscles   = "Full Body",
                    SecondaryMuscles = "Cardiovascular System",
                    EquipmentNeeded  = "Bodyweight",
                    FitnessLevel     = FitnessLevel.Intermediate,
                    VideoUrl         = "https://www.youtube.com/watch?v=JZQA08SlJnM",
                    Instructions     = "From standing, squat down and place hands on the floor. Jump feet back to a plank, " +
                                       "do a push-up, jump feet back toward your hands, then explosively jump up with arms overhead.",
                    CommonMistakes   = "Skipping the push-up, landing with stiff legs, losing a neutral spine in the plank."
                },
                new Exercise
                {
                    Name             = "Mountain Climber",
                    Description      = "A dynamic core and cardio drill that elevates heart rate while building anti-rotation strength.",
                    PrimaryMuscles   = "Core, Hip Flexors",
                    SecondaryMuscles = "Shoulders, Cardiovascular System",
                    EquipmentNeeded  = "Bodyweight",
                    FitnessLevel     = FitnessLevel.Beginner,
                    VideoUrl         = "https://www.youtube.com/watch?v=nmwgirgXLYM",
                    Instructions     = "Start in a high plank. Alternate driving knees toward your chest as fast as you can " +
                                       "while keeping your hips level and core tight.",
                    CommonMistakes   = "Piking the hips, bouncing the head, twisting the torso."
                },

                // ── Conditioning-specific ────────────────────────────
                new Exercise
                {
                    Name             = "Box Jump",
                    Description      = "Explosive plyometric power exercise that develops fast-twitch muscle fibres and vertical leap.",
                    PrimaryMuscles   = "Glutes, Quadriceps",
                    SecondaryMuscles = "Calves, Core",
                    EquipmentNeeded  = "Plyo Box",
                    FitnessLevel     = FitnessLevel.Intermediate,
                    VideoUrl         = "https://www.youtube.com/watch?v=52r_Ul5k03g",
                    Instructions     = "Stand facing the box. Bend your knees, swing your arms, and jump onto the box, " +
                                       "landing softly with both feet flat. Stand tall, then step down one foot at a time.",
                    CommonMistakes   = "Jumping down instead of stepping (increases injury risk), landing on the balls of the feet only."
                },
                new Exercise
                {
                    Name             = "Kettlebell Swing",
                    Description      = "A ballistic hip-hinge that builds posterior-chain power and cardiovascular capacity.",
                    PrimaryMuscles   = "Glutes, Hamstrings",
                    SecondaryMuscles = "Core, Lats, Shoulders",
                    EquipmentNeeded  = "Kettlebell",
                    FitnessLevel     = FitnessLevel.Intermediate,
                    VideoUrl         = "https://www.youtube.com/watch?v=YSxHifyI6s8",
                    Instructions     = "Hinge at the hips to swing the bell back between your legs, then snap your hips forward " +
                                       "explosively to drive the bell to chest height. The power comes from the hips — not the arms.",
                    CommonMistakes   = "Squatting instead of hinging, using arm strength to lift the bell, hyperextending the lower back at the top."
                },

                // ── Mobility & Accessory ─────────────────────────────
                new Exercise
                {
                    Name             = "Hip 90/90 Stretch",
                    Description      = "Improves hip internal and external rotation. Essential for anyone doing lower-body training.",
                    PrimaryMuscles   = "Hip External Rotators, Hip Flexors",
                    SecondaryMuscles = "Glutes, Adductors",
                    EquipmentNeeded  = "Bodyweight",
                    FitnessLevel     = FitnessLevel.Beginner,
                    VideoUrl         = "https://www.youtube.com/watch?v=v_ZK74OdLEw",
                    Instructions     = "Sit on the floor with both legs bent at 90 degrees, one in front and one to the side. " +
                                       "Sit tall and gently lean toward the front shin. Hold, then switch sides.",
                    CommonMistakes   = "Rounding the back, forcing the stretch past discomfort, not sitting on both sitting bones."
                },
                new Exercise
                {
                    Name             = "Cat-Cow",
                    Description      = "A gentle spinal mobility drill that warms up the thoracic and lumbar spine for training.",
                    PrimaryMuscles   = "Spinal Erectors, Multifidus",
                    SecondaryMuscles = "Core, Hip Flexors",
                    EquipmentNeeded  = "Bodyweight",
                    FitnessLevel     = FitnessLevel.Beginner,
                    VideoUrl         = "https://www.youtube.com/watch?v=kqnua4rHVVA",
                    Instructions     = "Start on all fours with wrists under shoulders and knees under hips. Inhale as you " +
                                       "drop your belly and lift your head (Cow). Exhale as you round your spine toward the ceiling (Cat).",
                    CommonMistakes   = "Moving too fast, not achieving full range in each direction."
                },
                new Exercise
                {
                    Name             = "Band Pull-Apart",
                    Description      = "A shoulder health staple that activates the rear deltoid and scapular retractors.",
                    PrimaryMuscles   = "Rear Deltoid, Rhomboids",
                    SecondaryMuscles = "Traps, Rotator Cuff",
                    EquipmentNeeded  = "Resistance Band",
                    FitnessLevel     = FitnessLevel.Beginner,
                    VideoUrl         = "https://www.youtube.com/watch?v=O7pSQ6-pBOg",
                    Instructions     = "Hold a resistance band in front of you at chest height, arms straight. Pull the band " +
                                       "apart by squeezing your shoulder blades together. Control the return.",
                    CommonMistakes   = "Bending the elbows, shrugging the shoulders, using a band with too much resistance."
                },
                new Exercise
                {
                    Name             = "Romanian Deadlift to Hip Hinge Walkout",
                    Description      = "A dynamic hamstring and hip mobility drill used as a movement primer before lower-body sessions.",
                    PrimaryMuscles   = "Hamstrings",
                    SecondaryMuscles = "Glutes, Core",
                    EquipmentNeeded  = "Bodyweight",
                    FitnessLevel     = FitnessLevel.Beginner,
                    VideoUrl         = "https://www.youtube.com/watch?v=XIZK1uLyZ9c",
                    Instructions     = "Stand tall, hinge at the hips with a flat back until you feel the hamstring stretch. " +
                                       "Place hands on the floor and walk out to a plank, hold briefly, walk back, and stand.",
                    CommonMistakes   = "Rounding the lower back, not walking out far enough to feel a plank position."
                },
                new Exercise
                {
                    Name             = "Face Pull",
                    Description      = "A cable or band exercise that builds external rotation strength and rear delt thickness.",
                    PrimaryMuscles   = "Rear Deltoid, External Rotators",
                    SecondaryMuscles = "Mid Traps, Rhomboids",
                    EquipmentNeeded  = "Cable Machine or Resistance Band",
                    FitnessLevel     = FitnessLevel.Beginner,
                    VideoUrl         = "https://www.youtube.com/watch?v=V8dZ3lL95Zs",
                    Instructions     = "Attach a rope to a cable set at face height. Pull toward your face with elbows high, " +
                                       "finishing with hands beside your ears and thumbs pointing back.",
                    CommonMistakes   = "Pulling the elbows down instead of keeping them high, using too much weight."
                }
            };

            context.Exercises.AddRange(exercises);
            await context.SaveChangesAsync();
        }
    }
}
