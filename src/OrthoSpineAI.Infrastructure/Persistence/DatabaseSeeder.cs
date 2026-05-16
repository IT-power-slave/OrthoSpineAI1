using Microsoft.EntityFrameworkCore;
using OrthoSpineAI.Domain.Entities;
using OrthoSpineAI.Domain.Enums;
using OrthoSpineAI.Infrastructure.Persistence;

namespace OrthoSpineAI.Infrastructure.Persistence;

/// <summary>
/// Seeds required reference data: Clinic, SystemUser admin, and all MedTestDefinitions with stages.
/// Idempotent — guarded by sentinel key checks per documentation §10.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        await SeedClinicAsync(db);
        await SeedAdminUserAsync(db);
        await SeedSurveysAsync(db);

        await db.SaveChangesAsync();
    }

    private static async Task SeedClinicAsync(AppDbContext db)
    {
        if (await db.Clinics.AnyAsync()) return;
        db.Clinics.Add(new Clinic
        {
            ClinicId = 1,
            Name = "Ośrodek Rehabilitacji Leczniczej 'Troniny'",
            Address = string.Empty
        });
        await db.SaveChangesAsync();
    }

    private static async Task SeedAdminUserAsync(AppDbContext db)
    {
        if (await db.SystemUsers.AnyAsync(u => u.Login == "admin")) return;
        db.SystemUsers.Add(new SystemUser
        {
            Login = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
            ClinicId = 1
        });
        await db.SaveChangesAsync();
    }

    private static async Task SeedSurveysAsync(AppDbContext db)
    {
        await SeedBackboneAsync(db);
        await SeedSpineFlexibilityAsync(db);
        await SeedSpineScreeningAsync(db);
        await SeedShoulderAsync(db);
        await SeedElbowAsync(db);
        await SeedHipAsync(db);
        await SeedKneeAsync(db);
        await SeedWristAsync(db);
        await SeedAnkleAsync(db);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // BACKBONE
    // ──────────────────────────────────────────────────────────────────────────
    private static async Task SeedBackboneAsync(AppDbContext db)
    {
        if (await db.MedTestDefinitions.AnyAsync(d => d.Key == "backbone.summary")) return;

        var root = Def(db, "backbone", "Ocena postawy ciała");
        Stage(db, root, "Przygotowanie do badania", ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE, ORT100Button.BTN_NEXT, ORT100Measurement.MEAS_NULL, MedTestPlane.SAGGITTAL_PLANE);

        var b1 = Def(db, "backbone.1", "Ocena płaszczyzny strzałkowej");
        Stage(db, b1, "Ocena nachylenia miednicy",               ORT100Mode.MODE_SEQ_A1, ORT100ResetFlag.ZERO_WAY_ANGLE_DEF, ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_NM,  MedTestPlane.SAGGITTAL_PLANE, tip: "Przyłóż urządzenie krawędzią A do miednicy pacjenta.");
        Stage(db, b1, "Ocena kąta lordozy lędźwiowej",           ORT100Mode.MODE_SEQ_A2, ORT100ResetFlag.ZERO_ANGLE,         ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_LL,  MedTestPlane.SAGGITTAL_PLANE);
        Stage(db, b1, "Ocena kąta kifozy piersiowej wstępującej",ORT100Mode.MODE_SEQ_A3, ORT100ResetFlag.ZERO_ANGLE,         ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_KW,  MedTestPlane.SAGGITTAL_PLANE);
        Stage(db, b1, "Ocena kąta kifozy piersiowej zstępującej",ORT100Mode.MODE_SEQ_A4, ORT100ResetFlag.ZERO_ANGLE,         ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_KZ,  MedTestPlane.SAGGITTAL_PLANE);
        Stage(db, b1, "Ocena kąta kifozy piersiowej (KP=KW+KZ)", ORT100Mode.MODE_SEQ_A4, ORT100ResetFlag.NONE,               ORT100Button.BTN_NEXT,   ORT100Measurement.MEAS_KP,  MedTestPlane.SAGGITTAL_PLANE);

        var b2 = Def(db, "backbone.2", "Pomiar symetrii pleców w Teście Adamsa");
        Stage(db, b2, "Przygotowanie do Testu Adamsa",    ORT100Mode.MODE_SEQ_LS1, ORT100ResetFlag.ZERO_WAY_ANGLE_DEF, ORT100Button.BTN_NEXT,   ORT100Measurement.MEAS_NULL,  MedTestPlane.TRANSVERSE_PLANE);
        // Calibration I — standing
        Stage(db, b2, "Kalibracja I — C7, pozycja stojąca",  ORT100Mode.MODE_SEQ_LS1, ORT100ResetFlag.NONE, ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_PC7,   MedTestPlane.TRANSVERSE_PLANE, ortState: ORT100ControlState.HIGHLIGHT_C7);
        Stage(db, b2, "Kalibracja I — T6, pozycja stojąca",  ORT100Mode.MODE_SEQ_LS2, ORT100ResetFlag.NONE, ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_PT6,   MedTestPlane.TRANSVERSE_PLANE, ortState: ORT100ControlState.HIGHLIGHT_TH6);
        Stage(db, b2, "Kalibracja I — T12, pozycja stojąca", ORT100Mode.MODE_SEQ_LS3, ORT100ResetFlag.NONE, ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_PT12,  MedTestPlane.TRANSVERSE_PLANE, ortState: ORT100ControlState.HIGHLIGHT_TH12);
        Stage(db, b2, "Kalibracja I — L3, pozycja stojąca",  ORT100Mode.MODE_SEQ_LS4, ORT100ResetFlag.NONE, ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_PL3,   MedTestPlane.TRANSVERSE_PLANE, ortState: ORT100ControlState.HIGHLIGHT_L3);
        Stage(db, b2, "Kalibracja I — S1, pozycja stojąca",  ORT100Mode.MODE_SEQ_LS5, ORT100ResetFlag.NONE, ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_PSIPS, MedTestPlane.TRANSVERSE_PLANE, ortState: ORT100ControlState.HIGHLIGHT_S1);
        Stage(db, b2, "Przejście do kalibracji II",           ORT100Mode.MODE_SEQ_LS5, ORT100ResetFlag.NONE, ORT100Button.BTN_NEXT,   ORT100Measurement.MEAS_NULL,  MedTestPlane.TRANSVERSE_PLANE);
        // Calibration II — bending
        Stage(db, b2, "Kalibracja II — C7, skłon",  ORT100Mode.MODE_SEQ_LB1, ORT100ResetFlag.ZERO_WAY_ANGLE_DEF, ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_SC7,   MedTestPlane.TRANSVERSE_PLANE, ortState: ORT100ControlState.HIGHLIGHT_C7);
        Stage(db, b2, "Kalibracja II — T6, skłon",  ORT100Mode.MODE_SEQ_LB2, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_ST6,   MedTestPlane.TRANSVERSE_PLANE, ortState: ORT100ControlState.HIGHLIGHT_TH6);
        Stage(db, b2, "Kalibracja II — T12, skłon", ORT100Mode.MODE_SEQ_LB3, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_ST12,  MedTestPlane.TRANSVERSE_PLANE, ortState: ORT100ControlState.HIGHLIGHT_TH12);
        Stage(db, b2, "Kalibracja II — L3, skłon",  ORT100Mode.MODE_SEQ_LB4, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_SL3,   MedTestPlane.TRANSVERSE_PLANE, ortState: ORT100ControlState.HIGHLIGHT_L3);
        Stage(db, b2, "Kalibracja II — S1, skłon",  ORT100Mode.MODE_SEQ_LB5, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_SSIPS, MedTestPlane.TRANSVERSE_PLANE, ortState: ORT100ControlState.HIGHLIGHT_S1);
        Stage(db, b2, "Przejście do Testu Adamsa",   ORT100Mode.MODE_SEQ_LB5, ORT100ResetFlag.NONE,               ORT100Button.BTN_NEXT,   ORT100Measurement.MEAS_NULL,  MedTestPlane.TRANSVERSE_PLANE);
        // Adams test — continuous
        Stage(db, b2, "Test Adamsa — C7 (start)", ORT100Mode.MODE_SEQ_AD1, ORT100ResetFlag.ZERO_WAY_ANGLE_DEF, ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_AC7,   MedTestPlane.TRANSVERSE_PLANE, continuous: false, ortState: ORT100ControlState.HIGHLIGHT_C7);
        Stage(db, b2, "Test Adamsa — T6",          ORT100Mode.MODE_SEQ_AD2, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_AT6,   MedTestPlane.TRANSVERSE_PLANE, continuous: true,  ortState: ORT100ControlState.HIGHLIGHT_TH6);
        Stage(db, b2, "Test Adamsa — T12",         ORT100Mode.MODE_SEQ_AD3, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_AT12,  MedTestPlane.TRANSVERSE_PLANE, continuous: true,  ortState: ORT100ControlState.HIGHLIGHT_TH12);
        Stage(db, b2, "Test Adamsa — L3",          ORT100Mode.MODE_SEQ_AD4, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_AL3,   MedTestPlane.TRANSVERSE_PLANE, continuous: true,  ortState: ORT100ControlState.HIGHLIGHT_L3);
        Stage(db, b2, "Test Adamsa — S1",          ORT100Mode.MODE_SEQ_AD5, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_ASIPS, MedTestPlane.TRANSVERSE_PLANE, continuous: true,  ortState: ORT100ControlState.HIGHLIGHT_S1);

        Def(db, "backbone.summary", "Podsumowanie oceny postawy ciała");
        await db.SaveChangesAsync();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // SPINE FLEXIBILITY
    // ──────────────────────────────────────────────────────────────────────────
    private static async Task SeedSpineFlexibilityAsync(AppDbContext db)
    {
        if (await db.MedTestDefinitions.AnyAsync(d => d.Key == "spineFlexibility.1.summary")) return;

        var sf1 = Def(db, "spineFlexibility.1", "Badanie elastyczności kręgosłupa");
        Stage(sf1, "Przygotowanie", ORT100Mode.MODE_SEQ_LS1, ORT100ResetFlag.ZERO_WAY_ANGLE_DEF, ORT100Button.BTN_NEXT,   ORT100Measurement.MEAS_NULL);
        Stage(sf1, "Pomiar I — C7, stojąca",  ORT100Mode.MODE_SEQ_LS1, ORT100ResetFlag.NONE, ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_PC7,   plane: MedTestPlane.TRANSVERSE_PLANE);
        Stage(sf1, "Pomiar I — T6, stojąca",  ORT100Mode.MODE_SEQ_LS2, ORT100ResetFlag.NONE, ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_PT6,   plane: MedTestPlane.TRANSVERSE_PLANE);
        Stage(sf1, "Pomiar I — T12, stojąca", ORT100Mode.MODE_SEQ_LS3, ORT100ResetFlag.NONE, ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_PT12,  plane: MedTestPlane.TRANSVERSE_PLANE);
        Stage(sf1, "Pomiar I — L3, stojąca",  ORT100Mode.MODE_SEQ_LS4, ORT100ResetFlag.NONE, ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_PL3,   plane: MedTestPlane.TRANSVERSE_PLANE);
        Stage(sf1, "Pomiar I — S1, stojąca",  ORT100Mode.MODE_SEQ_LS5, ORT100ResetFlag.NONE, ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_PSIPS, plane: MedTestPlane.TRANSVERSE_PLANE);
        Stage(sf1, "Przejście do pomiaru II",  ORT100Mode.MODE_SEQ_LS5, ORT100ResetFlag.NONE, ORT100Button.BTN_NEXT,   ORT100Measurement.MEAS_NULL);
        Stage(sf1, "Pomiar II — C7, skłon",  ORT100Mode.MODE_SEQ_LB1, ORT100ResetFlag.ZERO_WAY_ANGLE_DEF, ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_SC7,   plane: MedTestPlane.TRANSVERSE_PLANE);
        Stage(sf1, "Pomiar II — T6, skłon",  ORT100Mode.MODE_SEQ_LB2, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_ST6,   plane: MedTestPlane.TRANSVERSE_PLANE);
        Stage(sf1, "Pomiar II — T12, skłon", ORT100Mode.MODE_SEQ_LB3, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_ST12,  plane: MedTestPlane.TRANSVERSE_PLANE);
        Stage(sf1, "Pomiar II — L3, skłon",  ORT100Mode.MODE_SEQ_LB4, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_SL3,   plane: MedTestPlane.TRANSVERSE_PLANE);
        Stage(sf1, "Pomiar II — S1, skłon",  ORT100Mode.MODE_SEQ_LB5, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_SSIPS, plane: MedTestPlane.TRANSVERSE_PLANE);

        Def(db, "spineFlexibility.1.summary", "Podsumowanie elastyczności");
        await db.SaveChangesAsync();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // SPINE SCREENING
    // ──────────────────────────────────────────────────────────────────────────
    private static async Task SeedSpineScreeningAsync(AppDbContext db)
    {
        if (await db.MedTestDefinitions.AnyAsync(d => d.Key == "spineScreening.1.summary")) return;

        var ss1 = Def(db, "spineScreening.1", "Badanie przesiewowe — płaszczyzna strzałkowa");
        Stage(ss1, "Ocena nachylenia miednicy",             ORT100Mode.MODE_SEQ_A1, ORT100ResetFlag.ZERO_WAY_ANGLE_DEF, ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_NM, MedTestPlane.SAGGITTAL_PLANE);
        Stage(ss1, "Ocena lordozy lędźwiowej",              ORT100Mode.MODE_SEQ_A2, ORT100ResetFlag.ZERO_ANGLE,         ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_LL, MedTestPlane.SAGGITTAL_PLANE);
        Stage(ss1, "Ocena kifozy piersiowej wstępującej",   ORT100Mode.MODE_SEQ_A3, ORT100ResetFlag.ZERO_ANGLE,         ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_KW, MedTestPlane.SAGGITTAL_PLANE);
        Stage(ss1, "Ocena kifozy zstępującej",              ORT100Mode.MODE_SEQ_A4, ORT100ResetFlag.ZERO_ANGLE,         ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_KZ, MedTestPlane.SAGGITTAL_PLANE);
        Stage(ss1, "KP = KW + KZ",                          ORT100Mode.MODE_SEQ_A4, ORT100ResetFlag.NONE,               ORT100Button.BTN_NEXT,   ORT100Measurement.MEAS_KP, MedTestPlane.SAGGITTAL_PLANE);

        var ss2 = Def(db, "spineScreening.2", "Badanie przesiewowe — Test Adamsa");
        Stage(ss2, "Test Adamsa — C7",  ORT100Mode.MODE_SEQ_AD1, ORT100ResetFlag.ZERO_WAY_ANGLE_DEF, ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_AC7,   MedTestPlane.TRANSVERSE_PLANE, continuous: false);
        Stage(ss2, "Test Adamsa — T6",  ORT100Mode.MODE_SEQ_AD2, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_AT6,   MedTestPlane.TRANSVERSE_PLANE, continuous: true);
        Stage(ss2, "Test Adamsa — T12", ORT100Mode.MODE_SEQ_AD3, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_AT12,  MedTestPlane.TRANSVERSE_PLANE, continuous: true);
        Stage(ss2, "Test Adamsa — L3",  ORT100Mode.MODE_SEQ_AD4, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_AL3,   MedTestPlane.TRANSVERSE_PLANE, continuous: true);
        Stage(ss2, "Test Adamsa — S1",  ORT100Mode.MODE_SEQ_AD5, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_ASIPS, MedTestPlane.TRANSVERSE_PLANE, continuous: true);

        Def(db, "spineScreening.1.summary", "Podsumowanie badania przesiewowego");
        await db.SaveChangesAsync();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // JOINT SURVEYS — SHOULDER, ELBOW, HIP, KNEE, WRIST, ANKLE
    // ──────────────────────────────────────────────────────────────────────────
    private static async Task SeedShoulderAsync(AppDbContext db)
    {
        if (await db.MedTestDefinitions.AnyAsync(d => d.Key == "shoulder.summary")) return;
        var s = Def(db, "shoulder", "Pomiary stawu barkowego");
        Stage(s, "Przygotowanie",           ORT100Mode.MODE_MANUAL, ORT100ResetFlag.ZERO_WAY_ANGLE_DEF, ORT100Button.BTN_NEXT,   ORT100Measurement.MEAS_NULL,      MedTestPlane.SAGGITTAL_PLANE,  isom1: 50,  isom3: 170);
        Stage(s, "Wyprost",                 ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_EXTENSION, MedTestPlane.SAGGITTAL_PLANE);
        Stage(s, "Zgięcie — reset",         ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_RESET,  ORT100Measurement.MEAS_EXTENSION, MedTestPlane.SAGGITTAL_PLANE);
        Stage(s, "Zgięcie",                 ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_FLEXION,   MedTestPlane.SAGGITTAL_PLANE);
        Stage(s, "Odwodzenie — reset",      ORT100Mode.MODE_MANUAL, ORT100ResetFlag.ZERO_WAY_ANGLE_DEF, ORT100Button.BTN_RESET,  ORT100Measurement.MEAS_NULL,      MedTestPlane.FRONTAL_PLANE,    isom1: 170);
        Stage(s, "Odwodzenie",              ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_ABDUCTION, MedTestPlane.FRONTAL_PLANE);
        Stage(s, "Przywodzenie — reset",    ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_RESET,  ORT100Measurement.MEAS_ABDUCTION, MedTestPlane.FRONTAL_PLANE);
        Stage(s, "Przywodzenie",            ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_ADDUCTION, MedTestPlane.FRONTAL_PLANE);
        Stage(s, "Rotacja — przygotowanie", ORT100Mode.MODE_MANUAL, ORT100ResetFlag.ZERO_WAY_ANGLE_DEF, ORT100Button.BTN_RESET,  ORT100Measurement.MEAS_NULL,      MedTestPlane.ROTATION_PLANE_90, isom1: 90, isom3: 80);
        Stage(s, "Rotacja zewnętrzna",      ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_EXTERNAL_ROTATION, MedTestPlane.ROTATION_PLANE_90);
        Stage(s, "Rotacja wewnętrzna — reset", ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,           ORT100Button.BTN_RESET,  ORT100Measurement.MEAS_EXTERNAL_ROTATION, MedTestPlane.ROTATION_PLANE_90);
        Stage(s, "Rotacja wewnętrzna",      ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_INTERNAL_ROTATION, MedTestPlane.ROTATION_PLANE_90);
        Def(db, "shoulder.summary", "Podsumowanie barku");
        await db.SaveChangesAsync();
    }

    private static async Task SeedElbowAsync(AppDbContext db)
    {
        if (await db.MedTestDefinitions.AnyAsync(d => d.Key == "elbow.summary")) return;
        var e = Def(db, "elbow", "Pomiary stawu łokciowego");
        Stage(e, "Przygotowanie",           ORT100Mode.MODE_MANUAL, ORT100ResetFlag.ZERO_WAY_ANGLE_DEF, ORT100Button.BTN_RESET,  ORT100Measurement.MEAS_NULL,      MedTestPlane.SAGGITTAL_PLANE,  isom1: 0,   isom3: 150);
        Stage(e, "Wyprost",                 ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_EXTENSION, MedTestPlane.SAGGITTAL_PLANE);
        Stage(e, "Zgięcie — reset",         ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_RESET,  ORT100Measurement.MEAS_EXTENSION, MedTestPlane.SAGGITTAL_PLANE);
        Stage(e, "Zgięcie",                 ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_FLEXION,   MedTestPlane.SAGGITTAL_PLANE);
        Stage(e, "Rotacja — przygotowanie", ORT100Mode.MODE_MANUAL, ORT100ResetFlag.ZERO_WAY_ANGLE_DEF, ORT100Button.BTN_RESET,  ORT100Measurement.MEAS_NULL,      MedTestPlane.ROTATION_PLANE_90, isom1: 90, isom3: 80);
        Stage(e, "Supinacja",               ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_EXTERNAL_ROTATION, MedTestPlane.ROTATION_PLANE_90);
        Stage(e, "Pronacja — reset",        ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_RESET,  ORT100Measurement.MEAS_EXTERNAL_ROTATION, MedTestPlane.ROTATION_PLANE_90);
        Stage(e, "Pronacja",                ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_INTERNAL_ROTATION, MedTestPlane.ROTATION_PLANE_90);
        Def(db, "elbow.summary", "Podsumowanie łokcia");
        await db.SaveChangesAsync();
    }

    private static async Task SeedHipAsync(AppDbContext db)
    {
        if (await db.MedTestDefinitions.AnyAsync(d => d.Key == "hip.summary")) return;
        var h = Def(db, "hip", "Pomiary stawu biodrowego");
        Stage(h, "Przygotowanie",        ORT100Mode.MODE_MANUAL, ORT100ResetFlag.ZERO_WAY_ANGLE_DEF, ORT100Button.BTN_RESET,  ORT100Measurement.MEAS_NULL,      MedTestPlane.SAGGITTAL_PLANE,  isom1: 20,  isom3: 120);
        Stage(h, "Wyprost",              ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_EXTENSION, MedTestPlane.SAGGITTAL_PLANE);
        Stage(h, "Zgięcie — reset",      ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_RESET,  ORT100Measurement.MEAS_EXTENSION, MedTestPlane.SAGGITTAL_PLANE);
        Stage(h, "Zgięcie",              ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_FLEXION,   MedTestPlane.SAGGITTAL_PLANE);
        Stage(h, "Odwodzenie — reset",   ORT100Mode.MODE_MANUAL, ORT100ResetFlag.ZERO_WAY_ANGLE_DEF, ORT100Button.BTN_RESET,  ORT100Measurement.MEAS_NULL,      MedTestPlane.FRONTAL_PLANE,    isom1: 45,  isom3: 30);
        Stage(h, "Odwodzenie",           ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_ABDUCTION, MedTestPlane.FRONTAL_PLANE);
        Stage(h, "Przywodzenie — reset", ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_RESET,  ORT100Measurement.MEAS_ABDUCTION, MedTestPlane.FRONTAL_PLANE);
        Stage(h, "Przywodzenie",         ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_ADDUCTION, MedTestPlane.FRONTAL_PLANE);
        Stage(h, "Rot. zewn. — reset",   ORT100Mode.MODE_MANUAL, ORT100ResetFlag.ZERO_WAY_ANGLE_DEF, ORT100Button.BTN_RESET,  ORT100Measurement.MEAS_NULL,      MedTestPlane.ROTATION_PLANE_0, isom1: 45,  isom3: 45);
        Stage(h, "Rotacja zewnętrzna",   ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_EXTERNAL_ROTATION, MedTestPlane.ROTATION_PLANE_0);
        Stage(h, "Rot. wewn. — reset",   ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_RESET,  ORT100Measurement.MEAS_EXTERNAL_ROTATION, MedTestPlane.ROTATION_PLANE_0);
        Stage(h, "Rotacja wewnętrzna",   ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_INTERNAL_ROTATION, MedTestPlane.ROTATION_PLANE_0);
        Def(db, "hip.summary", "Podsumowanie biodra");
        await db.SaveChangesAsync();
    }

    private static async Task SeedKneeAsync(AppDbContext db)
    {
        if (await db.MedTestDefinitions.AnyAsync(d => d.Key == "knee.summary")) return;
        var k = Def(db, "knee", "Pomiary stawu kolanowego");
        Stage(k, "Przygotowanie",    ORT100Mode.MODE_MANUAL, ORT100ResetFlag.ZERO_WAY_ANGLE_DEF, ORT100Button.BTN_RESET,  ORT100Measurement.MEAS_NULL,      MedTestPlane.SAGGITTAL_PLANE, isom1: 0,   isom3: 135);
        Stage(k, "Wyprost",          ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_EXTENSION, MedTestPlane.SAGGITTAL_PLANE);
        Stage(k, "Zgięcie — reset",  ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_RESET,  ORT100Measurement.MEAS_EXTENSION, MedTestPlane.SAGGITTAL_PLANE);
        Stage(k, "Zgięcie",          ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_FLEXION,   MedTestPlane.SAGGITTAL_PLANE);
        Def(db, "knee.summary", "Podsumowanie kolana");
        await db.SaveChangesAsync();
    }

    private static async Task SeedWristAsync(AppDbContext db)
    {
        if (await db.MedTestDefinitions.AnyAsync(d => d.Key == "wrist.summary")) return;
        var w = Def(db, "wrist", "Pomiary stawu promieniowo-nadgarstkowego");
        Stage(w, "Przygotowanie",         ORT100Mode.MODE_MANUAL, ORT100ResetFlag.ZERO_WAY_ANGLE_DEF, ORT100Button.BTN_RESET,  ORT100Measurement.MEAS_NULL,      MedTestPlane.SAGGITTAL_PLANE, isom1: 70,  isom3: 80);
        Stage(w, "Wyprost (dorsalny)",    ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_EXTENSION, MedTestPlane.SAGGITTAL_PLANE);
        Stage(w, "Zgięcie — reset",       ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_RESET,  ORT100Measurement.MEAS_EXTENSION, MedTestPlane.SAGGITTAL_PLANE);
        Stage(w, "Zgięcie (dłoniowe)",    ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_FLEXION,   MedTestPlane.SAGGITTAL_PLANE);
        Stage(w, "Odchylenie — reset",    ORT100Mode.MODE_MANUAL, ORT100ResetFlag.ZERO_WAY_ANGLE_DEF, ORT100Button.BTN_RESET,  ORT100Measurement.MEAS_NULL,      MedTestPlane.FRONTAL_PLANE,   isom1: 20,  isom3: 30);
        Stage(w, "Odchylenie promieniowe",ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_ABDUCTION, MedTestPlane.FRONTAL_PLANE);
        Stage(w, "Odchylenie łokciowe — reset", ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,        ORT100Button.BTN_RESET,  ORT100Measurement.MEAS_ABDUCTION, MedTestPlane.FRONTAL_PLANE);
        Stage(w, "Odchylenie łokciowe",   ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_ADDUCTION, MedTestPlane.FRONTAL_PLANE);
        Def(db, "wrist.left",    "Lewy nadgarstek");
        Def(db, "wrist.right",   "Prawy nadgarstek");
        Def(db, "wrist.summary", "Podsumowanie nadgarstka");
        await db.SaveChangesAsync();
    }

    private static async Task SeedAnkleAsync(AppDbContext db)
    {
        if (await db.MedTestDefinitions.AnyAsync(d => d.Key == "ankle.summary")) return;
        var a = Def(db, "ankle", "Pomiary stawu skokowo-goleniowego");
        Stage(a, "Przygotowanie",           ORT100Mode.MODE_MANUAL, ORT100ResetFlag.ZERO_WAY_ANGLE_DEF, ORT100Button.BTN_RESET,  ORT100Measurement.MEAS_NULL,      MedTestPlane.SAGGITTAL_PLANE, isom1: 20,  isom3: 45);
        Stage(a, "Wyprost (zgięcie dors.)", ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_EXTENSION, MedTestPlane.SAGGITTAL_PLANE);
        Stage(a, "Zgięcie — reset",         ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_RESET,  ORT100Measurement.MEAS_EXTENSION, MedTestPlane.SAGGITTAL_PLANE);
        Stage(a, "Zgięcie (podeszwowe)",    ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_FLEXION,   MedTestPlane.SAGGITTAL_PLANE);
        Stage(a, "Supinacja — reset",       ORT100Mode.MODE_MANUAL, ORT100ResetFlag.ZERO_WAY_ANGLE_DEF, ORT100Button.BTN_RESET,  ORT100Measurement.MEAS_NULL,      MedTestPlane.FRONTAL_PLANE,   isom1: 35,  isom3: 25);
        Stage(a, "Supinacja",               ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_ABDUCTION, MedTestPlane.FRONTAL_PLANE);
        Stage(a, "Pronacja — reset",        ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_RESET,  ORT100Measurement.MEAS_ABDUCTION, MedTestPlane.FRONTAL_PLANE);
        Stage(a, "Pronacja",                ORT100Mode.MODE_MANUAL, ORT100ResetFlag.NONE,               ORT100Button.BTN_SAMPLE, ORT100Measurement.MEAS_ADDUCTION, MedTestPlane.FRONTAL_PLANE);
        Def(db, "ankle.summary", "Podsumowanie skoku");
        await db.SaveChangesAsync();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────────────────
    private static MedTestDefinition Def(AppDbContext db, string key, string name)
    {
        var def = new MedTestDefinition { Key = key, Name = name };
        db.MedTestDefinitions.Add(def);
        return def;
    }

    private static MedTestDefinition Def(string key, string name) =>
        new() { Key = key, Name = name };

    private static void Stage(AppDbContext db, MedTestDefinition def, string name,
        ORT100Mode mode, ORT100ResetFlag reset, ORT100Button button,
        ORT100Measurement meas, MedTestPlane plane = MedTestPlane.SAGGITTAL_PLANE,
        bool continuous = false, double? isom1 = null, double? isom3 = null,
        ORT100ControlState ortState = ORT100ControlState.HIGHLIGHT_NONE, string tip = "")
    {
        def.Stages.Add(new MedTestStage
        {
            SortOrder = def.Stages.Count + 1,
            Name = name, Tip = tip, OrtMode = mode,
            OrtResetFlag = reset, OrtNextStepButton = button,
            OrtMeas = meas, Plane = plane,
            OrtContinousMeas = continuous,
            ValueISOM1 = isom1, ValueISOM3 = isom3,
            OrtState = ortState
        });
    }

    // Overload without AppDbContext (for pre-attached definitions)
    private static void Stage(MedTestDefinition def, string name,
        ORT100Mode mode, ORT100ResetFlag reset, ORT100Button button,
        ORT100Measurement meas, MedTestPlane plane = MedTestPlane.SAGGITTAL_PLANE,
        bool continuous = false, double? isom1 = null, double? isom3 = null,
        ORT100ControlState ortState = ORT100ControlState.HIGHLIGHT_NONE, string tip = "")
    {
        def.Stages.Add(new MedTestStage
        {
            SortOrder = def.Stages.Count + 1,
            Name = name, Tip = tip, OrtMode = mode,
            OrtResetFlag = reset, OrtNextStepButton = button,
            OrtMeas = meas, Plane = plane,
            OrtContinousMeas = continuous,
            ValueISOM1 = isom1, ValueISOM3 = isom3,
            OrtState = ortState
        });
    }
}
