namespace OrthoSpineAI.Domain.Enums;

public enum ORT100Measurement
{
    MEAS_NULL = 0,
    MEAS_NM,       // Nachylenie miednicy (Pelvic tilt)
    MEAS_LL,       // Lordoza lędźwiowa
    MEAS_KW,       // Kifoza wstępująca
    MEAS_KZ,       // Kifoza zstępująca
    MEAS_KP,       // Kifoza piersiowa (KW+KZ)
    MEAS_PC7,      // Kalibracja I standing: C7
    MEAS_PT6,
    MEAS_PT12,
    MEAS_PL3,
    MEAS_PSIPS,    // S1 standing
    MEAS_SC7,      // Kalibracja II bending: C7
    MEAS_ST6,
    MEAS_ST12,
    MEAS_SL3,
    MEAS_SSIPS,    // S1 bending
    MEAS_AC7,      // Adams test: C7
    MEAS_AT6,
    MEAS_AT12,
    MEAS_AL3,
    MEAS_ASIPS,    // Adams test: S1
    MEAS_EXTENSION,
    MEAS_FLEXION,
    MEAS_ABDUCTION,
    MEAS_ADDUCTION,
    MEAS_INTERNAL_ROTATION,
    MEAS_EXTERNAL_ROTATION
}
