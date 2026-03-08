using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;
using ExactHull;
using ExactHull.ExactGeometry;
using static Raylib_cs.Raylib;
using System.Runtime.InteropServices.JavaScript;

namespace ExactHull.WebDemo;

public class Playground
{
    private Camera3D camera;
    private Shader shader;

    private readonly List<(double X, double Y, double Z)> points = new();
    private Hull3D? hull;
    private readonly Random rng = new();

    public Playground()
    {
        const int screenWidth = 800;
        const int screenHeight = 600;

        SetConfigFlags(ConfigFlags.Msaa4xHint);
        InitWindow(screenWidth, screenHeight, "ExactHull WebDemo");

        camera = new()
        {
            Position = new Vector3(4.0f, 4.0f, -8.0f),
            Target = new Vector3(0.0f, 0.0f, 0.0f),
            Up = new Vector3(0.0f, 1.0f, 0.0f),
            FovY = 45.0f,
            Projection = CameraProjection.Perspective
        };

        shader = LoadShader("assets/lighting.vs", "assets/lighting.fs");

        unsafe
        {
            shader.Locs[(int)ShaderLocationIndex.MatrixModel] = GetShaderLocation(shader, "matModel");
            shader.Locs[(int)ShaderLocationIndex.VectorView] = GetShaderLocation(shader, "viewPos");
        }

        int ambientLoc = GetShaderLocation(shader, "ambient");
        Raylib.SetShaderValue(shader, ambientLoc,
            new float[] { 0.3f, 0.3f, 0.3f, 1.0f }, ShaderUniformDataType.Vec4);

        Rlights.CreateLight(0, LightType.Point, new Vector3(4, 8, -4), Vector3.Zero, Color.White, shader);

        SetTargetFPS(60);

        GenerateRandomPointsAndBuildHull();
    }

    public void Close()
    {
        UnloadShader(shader);
        CloseWindow();
    }

    private void GenerateRandomPointsAndBuildHull()
    {
        points.Clear();
        hull = null;

        int count = rng.Next(20, 121);
        for (int i = 0; i < count; i++)
        {
            double x = (rng.NextDouble() - 0.5) * 4.0;
            double y = (rng.NextDouble() - 0.5) * 4.0;
            double z = (rng.NextDouble() - 0.5) * 4.0;
            points.Add((x, y, z));
        }

        try
        {
            hull = ExactHull3D.Build(points);
        }
        catch
        {
            hull = null;
        }
    }

    public void UpdateFrame()
    {
        UpdateCamera(ref camera, CameraMode.Orbital);

        if (IsKeyPressed(KeyboardKey.G) || IsMouseButtonDown(MouseButton.Left))
        {
            GenerateRandomPointsAndBuildHull();
        }

        unsafe
        {
            Raylib.SetShaderValue(shader,
                shader.Locs[(int)ShaderLocationIndex.VectorView],
                camera.Position, ShaderUniformDataType.Vec3);
        }

        BeginDrawing();
        ClearBackground(Color.DarkGray);

        BeginMode3D(camera);

        // Draw coordinate axes
        DrawLine3D(new Vector3(-10, 0, 0), new Vector3(10, 0, 0), Color.Red);
        DrawLine3D(new Vector3(0, -10, 0), new Vector3(0, 10, 0), Color.Green);
        DrawLine3D(new Vector3(0, 0, -10), new Vector3(0, 0, 10), Color.Blue);

        // Draw all points as small spheres
        foreach (var p in points)
        {
            DrawSphere(new Vector3((float)p.X, (float)p.Y, (float)p.Z), 0.05f, Color.Yellow);
        }

        if (hull != null)
        {
            var pts = hull.Points;

            var faceDrawData = new List<(Vector3 A, Vector3 B, Vector3 C, float DistanceSq)>(hull.Faces.Length);

            foreach (var face in hull.Faces)
            {
                pts[face.A].X.TryToDouble(out double ax);
                pts[face.A].Y.TryToDouble(out double ay);
                pts[face.A].Z.TryToDouble(out double az);

                pts[face.B].X.TryToDouble(out double bx);
                pts[face.B].Y.TryToDouble(out double by);
                pts[face.B].Z.TryToDouble(out double bz);

                pts[face.C].X.TryToDouble(out double cx);
                pts[face.C].Y.TryToDouble(out double cy);
                pts[face.C].Z.TryToDouble(out double cz);

                Vector3 va = new((float)ax, (float)ay, (float)az);
                Vector3 vb = new((float)bx, (float)by, (float)bz);
                Vector3 vc = new((float)cx, (float)cy, (float)cz);

                Vector3 centroid = (va + vb + vc) / 3.0f;
                float distanceSq = Vector3.DistanceSquared(camera.Position, centroid);

                faceDrawData.Add((va, vb, vc, distanceSq));
            }

            // Back-to-front for alpha blending
            faceDrawData.Sort((a, b) => b.DistanceSq.CompareTo(a.DistanceSq));

            // Draw transparent faces
            Rlgl.DisableBackfaceCulling(); // optional, helps if you want both sides visible
            Rlgl.DisableDepthMask();

            foreach (var face in faceDrawData)
            {
                DrawTriangle3D(face.A, face.B, face.C, new Color(50, 130, 220, 140));
            }

            Rlgl.EnableDepthMask();
            Rlgl.EnableBackfaceCulling();

            // Draw edges afterwards
            foreach (var face in faceDrawData)
            {
                DrawLine3D(face.A, face.B, Color.White);
                DrawLine3D(face.B, face.C, Color.White);
                DrawLine3D(face.C, face.A, Color.White);
            }
        }

        EndMode3D();

        DrawText($"{GetFPS()} fps", 10, 10, 20, Color.RayWhite);
        DrawText($"Points: {points.Count}", 10, 35, 20, Color.RayWhite);

        if (hull != null)
            DrawText($"Hull faces: {hull.Faces.Length}", 10, 60, 20, Color.RayWhite);

        EndDrawing();
    }
}

public partial class Application
{
    private static Playground _playground;

    [JSExport]
    public static void UpdateFrame()
    {
        _playground?.UpdateFrame();
    }

    [JSExport]
    public static void Resize(int width, int height)
    {
        SetWindowSize(width, height);
    }

    public static void Main()
    {
        _playground = new Playground();
    }
}
