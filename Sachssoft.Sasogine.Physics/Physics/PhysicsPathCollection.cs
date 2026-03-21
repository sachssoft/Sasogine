using Clipper2Lib;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;

using Path = Sachssoft.Sasogine.Geometry.Path;

namespace Sachssoft.Sasogine.Physics;

public sealed class PhysicsPathCollection
{
    private readonly IReadOnlyList<Path> _paths;

    public PhysicsPathCollection(IEnumerable<Path> paths)
    {
        _paths = (new List<Path>(paths ?? Enumerable.Empty<Path>())).AsReadOnly();
    }

    public IReadOnlyList<Path> Paths => _paths;

    public void AddFixtures(Body body, Vector2 origin, bool inside = false, params Vertices[] clipVertices)
    {
        for (int i = 0; i < _paths.Count; i++)
        {
            var path = _paths[i];
            for (int j = 0; j < path.GetPolygonCount(); j++)
            {
                Vertices v;

                if (clipVertices.Length > 0)
                {
                    v = ClipVertices(body.Position, new Vertices(path.GetPolygonPoints(j)), inside, clipVertices);
                }
                else
                {
                    v = new Vertices(path.GetPolygonPoints(j));
                }

                v.Translate(new Vector2(-Width / 2f, -Height / 2f) + origin);

                var fixture = body.CreateChainShape(v);
                fixture.Tag = Tuple.Create(this, i, j);
                fixture.Restitution = 0f;
            }
        }
    }

    private Vertices ClipVertices(Vector2 position, Vertices vertices, bool inside, params Vertices[] clipVertices)
    {
        var cScenery = new PathsD();
        var cPath = new PathD();
        foreach (var vert in vertices)
            cPath.Add(new(vert.X, vert.Y));
        cScenery.Add(cPath);
        cScenery = Clipper.TranslatePaths(cScenery, position.X, position.Y);

        var cClip = new PathsD();
        foreach (var clipVerts in clipVertices)
        {
            var cPaths = new PathsD();
            var cFix = new PathD();
            foreach (var vert in clipVerts)
                cFix.Add(new(vert.X, vert.Y));
            cPaths.Add(cFix);

            cClip.Add(cFix);
        }

        PathsD? result;
        if (inside)
            result = Clipper.Difference(cScenery, cClip, FillRule.NonZero);
        else
            result = Clipper.Difference(cScenery, cClip, FillRule.NonZero);

        result = Clipper.TranslatePaths(result, -position.X, -position.Y);

        if (result.Count > 0)
            return new Vertices(result[0].Select(v => new Microsoft.Xna.Framework.Vector2((float)v.x, (float)v.y)));
        else
            return vertices;
    }

    public void RemoveFixtures(Body body)
    {
        var fixturesToRemove = body.FixtureList.Where(f =>
            f.Tag is Tuple<PhysicsPathCollection, int, int> t && t.Item1 == this
        ).ToList();

        foreach (var fix in fixturesToRemove)
            body.Remove(fix);
    }

    public float Width => _paths.Count == 0 ? 0f : _paths.Max(p => p.Right) - _paths.Min(p => p.Left);
    public float Height => _paths.Count == 0 ? 0f : _paths.Max(p => p.Bottom) - _paths.Min(p => p.Top);
}
