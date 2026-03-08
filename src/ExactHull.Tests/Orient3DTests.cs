using Xunit;
using ExactHull.ExactGeometry;

namespace ExactHull.Tests;

public sealed class Orient3DTests
{
    [Fact]
    public void Orient3D_Positive_ForBasicTetrahedron()
    {
        var a = new Exact3(0.0, 0.0, 0.0);
        var b = new Exact3(1.0, 0.0, 0.0);
        var c = new Exact3(0.0, 1.0, 0.0);
        var d = new Exact3(0.0, 0.0, 1.0);

        Exact result = ExactGeometry3D.Orient3D(a, b, c, d);

        Assert.True(result.Sign() > 0);
    }

    [Fact]
    public void Orient3D_Negative_WhenTwoPointsAreSwapped()
    {
        var a = new Exact3(0.0, 0.0, 0.0);
        var b = new Exact3(1.0, 0.0, 0.0);
        var c = new Exact3(0.0, 1.0, 0.0);
        var d = new Exact3(0.0, 0.0, 1.0);

        Exact result = ExactGeometry3D.Orient3D(a, c, b, d);

        Assert.True(result.Sign() < 0);
    }

    [Fact]
    public void Orient3D_IsZero_ForCoplanarPoint()
    {
        var a = new Exact3(0.0, 0.0, 0.0);
        var b = new Exact3(1.0, 0.0, 0.0);
        var c = new Exact3(0.0, 1.0, 0.0);
        var d = new Exact3(0.25, 0.25, 0.0);

        Exact result = ExactGeometry3D.Orient3D(a, b, c, d);

        Assert.True(result.IsZero());
    }

    [Fact]
    public void Orient3D_IsZero_WhenPointIsDuplicated()
    {
        var a = new Exact3(0.0, 0.0, 0.0);
        var b = new Exact3(1.0, 0.0, 0.0);
        var c = new Exact3(0.0, 1.0, 0.0);
        var d = new Exact3(0.0, 1.0, 0.0);

        Exact result = ExactGeometry3D.Orient3D(a, b, c, d);

        Assert.True(result.IsZero());
    }

    [Fact]
    public void Orient3D_IsTranslationInvariant()
    {
        var a1 = new Exact3(0.0, 0.0, 0.0);
        var b1 = new Exact3(1.0, 0.0, 0.0);
        var c1 = new Exact3(0.0, 1.0, 0.0);
        var d1 = new Exact3(0.0, 0.0, 1.0);

        var offset = new Exact3(10.0, -7.0, 3.5);

        var a2 = a1 + offset;
        var b2 = b1 + offset;
        var c2 = c1 + offset;
        var d2 = d1 + offset;

        Exact r1 = ExactGeometry3D.Orient3D(a1, b1, c1, d1);
        Exact r2 = ExactGeometry3D.Orient3D(a2, b2, c2, d2);

        Assert.Equal(r1.Sign(), r2.Sign());
        Assert.Equal(r1, r2);
    }

    [Fact]
    public void Orient3D_ScalesPredictably()
    {
        var a1 = new Exact3(0.0, 0.0, 0.0);
        var b1 = new Exact3(1.0, 0.0, 0.0);
        var c1 = new Exact3(0.0, 1.0, 0.0);
        var d1 = new Exact3(0.0, 0.0, 1.0);

        Exact s = Exact.FromDouble(2.0);

        var a2 = Scale(a1, s);
        var b2 = Scale(b1, s);
        var c2 = Scale(c1, s);
        var d2 = Scale(d1, s);

        Exact r1 = ExactGeometry3D.Orient3D(a1, b1, c1, d1);
        Exact r2 = ExactGeometry3D.Orient3D(a2, b2, c2, d2);

        Assert.True(r1.Sign() > 0);
        Assert.True(r2.Sign() > 0);

        // Orient3D is cubic in uniform scale: scaling by 2 scales volume by 8.
        Assert.Equal(r1 * Exact.FromDouble(8.0), r2);
    }

    [Fact]
    public void Orient3D_HandlesNearlyCoplanarInputExactly()
    {
        var a = new Exact3(0.0, 0.0, 0.0);
        var b = new Exact3(1.0, 0.0, 0.0);
        var c = new Exact3(0.0, 1.0, 0.0);
        var d = new Exact3(0.25, 0.25, 1e-300);

        Exact result = ExactGeometry3D.Orient3D(a, b, c, d);

        Assert.True(result.Sign() > 0);
    }

    [Fact]
    public void Orient3D_HandlesNegativeNearlyCoplanarInputExactly()
    {
        var a = new Exact3(0.0, 0.0, 0.0);
        var b = new Exact3(1.0, 0.0, 0.0);
        var c = new Exact3(0.0, 1.0, 0.0);
        var d = new Exact3(0.25, 0.25, -1e-300);

        Exact result = ExactGeometry3D.Orient3D(a, b, c, d);

        Assert.True(result.Sign() < 0);
    }

    private static Exact3 Scale(Exact3 v, Exact s)
    {
        return new Exact3(v.X * s, v.Y * s, v.Z * s);
    }
}
