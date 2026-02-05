using System;

namespace AIATC.Domain.Models;

/// <summary>
/// Simple 2D vector for position and direction calculations.
/// Coordinates are in nautical miles (NM) for positions.
/// </summary>
public struct Vector2
{
    public float X { get; set; }
    public float Y { get; set; }

    public Vector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    public float Magnitude => MathF.Sqrt(X * X + Y * Y);

    public float MagnitudeSquared => X * X + Y * Y;

    public Vector2 Normalized
    {
        get
        {
            var mag = Magnitude;
            return mag > 0 ? new Vector2(X / mag, Y / mag) : new Vector2(0, 0);
        }
    }

    public static Vector2 FromPolar(float magnitude, float angleRadians)
    {
        return new Vector2(
            magnitude * MathF.Cos(angleRadians),
            magnitude * MathF.Sin(angleRadians)
        );
    }

    public float ToAngleRadians()
    {
        return MathF.Atan2(Y, X);
    }

    public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
    public static Vector2 operator *(Vector2 v, float scalar) => new(v.X * scalar, v.Y * scalar);
    public static Vector2 operator *(float scalar, Vector2 v) => new(v.X * scalar, v.Y * scalar);
    public static Vector2 operator /(Vector2 v, float scalar) => new(v.X / scalar, v.Y / scalar);
    public static Vector2 operator -(Vector2 v) => new(-v.X, -v.Y);

    public static float Distance(Vector2 a, Vector2 b)
    {
        var dx = b.X - a.X;
        var dy = b.Y - a.Y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    public static float Dot(Vector2 a, Vector2 b) => a.X * b.X + a.Y * b.Y;

    public override string ToString() => $"({X:F2}, {Y:F2})";
}
