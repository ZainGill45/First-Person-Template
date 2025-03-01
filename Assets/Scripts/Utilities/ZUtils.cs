using UnityEngine;

namespace Utilities
{
    public static class ZUtils
    {
        private const float THRESHOLD = 0.01f;

        public const float DEFAULT_AR = 16 / 9f;
        public const float VERY_LARGE_THRESHOLD = 1f;
        public const float LARGE_THRESHOLD = 0.1f;
        public const float SMALL_THRESHOLD = 0.001f;
        public const float VERY_SMALL_THRESHOLD = 0.0001f;
        public const float DECAY = 10;

        public static bool Approx(float a, float b, float threshold = THRESHOLD) => (a - b < 0 ? (a - b) * -1 : a - b) <= threshold;
        public static bool Approx(Vector2 a, Vector2 b, float threshold = THRESHOLD) => Approx(a.x, b.x, threshold) && Approx(a.y, b.y, threshold);
        public static bool Approx(Vector3 a, Vector3 b, float threshold = THRESHOLD) => Approx(a.x, b.x, threshold) && Approx(a.y, b.y, threshold) && Approx(a.z, b.z, threshold);
        public static bool Approx(Quaternion a, Quaternion b, float threshold = THRESHOLD) => Approx(a.x, b.x, threshold) && Approx(a.y, b.y, threshold) && Approx(a.z, b.z, threshold) && Approx(a.w, b.w, threshold);

        public static Vector3 GenerateRandomVector(float inclusiveMin, float exclusiveMax) => new(Random.Range(inclusiveMin, exclusiveMax), Random.Range(inclusiveMin, exclusiveMax), Random.Range(inclusiveMin, exclusiveMax));
        public static Quaternion GenerateRandomQuaternion(float inclusiveMin, float exclusiveMax) => new(Random.Range(inclusiveMin, exclusiveMax), Random.Range(inclusiveMin, exclusiveMax), Random.Range(inclusiveMin, exclusiveMax), Random.Range(inclusiveMin, exclusiveMax));
    }
}