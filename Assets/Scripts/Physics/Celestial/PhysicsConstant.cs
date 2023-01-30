
public static class PhysicsConstant {
    public const double LengthUnits = 2e4d;
    public const double LengthUnitsSqr = LengthUnits * LengthUnits;
    public const float G = (float)(6.67e-11d * (RefMass / (LengthUnitsSqr * LengthUnits)));
    public const double RefMass = 1e22d;    // kg

    public static float AbsoluteMass(float relative) {
        return (float)((double)relative * RefMass);
    }
}