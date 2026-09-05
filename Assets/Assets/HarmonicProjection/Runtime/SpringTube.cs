using UnityEngine;

namespace HarmonicProjection
{
    // Reuses arrays; triangle topology changes only when resolution changes.
    public sealed class SpringTube
    {
        public const float Lead = 0.018f;
        Vector3[] vertices, normals, centers;
        Vector2[] uv;
        int[] triangles;
        int previousSegments, previousSides;

        public void Update(Mesh mesh, float length, int turns, int samples, int sides, float radius, float wire)
        {
            int helixSegments = turns * samples;
            const int leadSegments = 10;
            int segments = helixSegments + 2 * leadSegments;
            bool rebuild = vertices == null || segments != previousSegments || sides != previousSides;
            int stride = sides + 1;
            if (rebuild)
            {
                previousSegments = segments; previousSides = sides;
                centers = new Vector3[segments + 1];
                vertices = new Vector3[(segments + 1) * stride];
                normals = new Vector3[vertices.Length];
                uv = new Vector2[vertices.Length];
                triangles = new int[segments * sides * 6];
                int k = 0;
                for (int i = 0; i < segments; i++)
                    for (int j = 0; j < sides; j++)
                    {
                        int a = i * stride + j, b = a + stride;
                        triangles[k++] = a; triangles[k++] = a + 1; triangles[k++] = b;
                        triangles[k++] = a + 1; triangles[k++] = b + 1; triangles[k++] = b;
                    }
            }
            for (int i = 0; i <= segments; i++)
            {
                if (i < leadSegments)
                {
                    float t = i / (float)leadSegments;
                    centers[i] = new Vector3(radius * Mathf.SmoothStep(0f, 1f, t), -Lead * t, 0f);
                }
                else if (i <= leadSegments + helixSegments)
                {
                    float t = (i - leadSegments) / (float)helixSegments;
                    float a = t * turns * 2f * Mathf.PI;
                    centers[i] = new Vector3(radius * Mathf.Cos(a), -Lead - (length - 2f * Lead) * t, radius * Mathf.Sin(a));
                }
                else
                {
                    float t = (i - leadSegments - helixSegments) / (float)leadSegments;
                    centers[i] = new Vector3(radius * (1f - Mathf.SmoothStep(0f, 1f, t)), -length + Lead * (1f - t), 0f);
                }
            }
            // Parallel transport frame avoids twisting abruptly at connector bends.
            Vector3 n = Vector3.forward;
            for (int i = 0; i <= segments; i++)
            {
                Vector3 tangent = (centers[Mathf.Min(i + 1, segments)] - centers[Mathf.Max(0, i - 1)]).normalized;
                n = (n - Vector3.Dot(n, tangent) * tangent).normalized;
                if (n.sqrMagnitude < 0.1f) n = Vector3.Cross(tangent, Vector3.right).normalized;
                Vector3 b = Vector3.Cross(tangent, n).normalized;
                for (int j = 0; j <= sides; j++)
                {
                    float a = j * 2f * Mathf.PI / sides;
                    Vector3 normal = Mathf.Cos(a) * n + Mathf.Sin(a) * b;
                    int index = i * stride + j;
                    vertices[index] = centers[i] + wire * normal;
                    normals[index] = normal;
                    uv[index] = new Vector2(j / (float)sides, i / (float)segments);
                }
            }
            if (rebuild) mesh.Clear();
            mesh.vertices = vertices;
            mesh.normals = normals;
            if (rebuild) { mesh.uv = uv; mesh.triangles = triangles; }
            mesh.RecalculateBounds();
        }
    }
}
