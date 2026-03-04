using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Core.Enums
{
    public enum UserRole
    {
        Trainee = 0,
        Coach = 1,
        Admin = 2
    }

    public enum WeekDay
    {
        Saturday,
        Sunday,
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday
    }

    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard
    }

    public enum TrainingGoal
    {
        LoseFat ,
        BuildMuscle ,
        GetStronger ,
        ImproveEndurance ,
        MoveBetter ,
        GeneralFitness ,
        MaintainWeight
    }
    public enum FitnessLevel
    {
        Beginner ,
        Intermediate ,
        Advanced
    }
    public enum EquipmentType
    {
        FullGym ,
        Dumbbells ,
        Home ,
        Bodyweight ,
        Bands 
    }

    public enum SectionType
    {
        Warmup ,
        Primer ,
        MainWork ,
        Cooldown 
    }

    public enum EnrollmentStatus
    {
        Active ,
        Completed ,
        Cancelled   
    }
    public enum ProgramStatus
    {
        Draft,
        PendingReview,
        Published,
        Rejected
    }
}
