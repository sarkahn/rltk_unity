using Sark.Common.GridUtil;

using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

using Random = Unity.Mathematics.Random;

namespace Sark.RLTK.Samples
{
    public class MapNoise
    {
        public void AddNoiseToMap(BasicPathingMap map)
        {
            Random rand = new Random((uint)UnityEngine.Random.Range(1, int.MaxValue));

            int iters = rand.NextInt(1, 16);
            float per = rand.NextFloat(0.15f, 0.75f);
            float scale = rand.NextFloat(0.01f, .2f);
            int low = rand.NextInt(0, 10);
            int high = rand.NextInt(low + 5, low + 15);
            int thresh = rand.NextInt(low, high);

            new NoiseJob
            {
                Iterations = iters,
                Persistence = per,
                Scale = scale,
                Low = low,
                High = high,
                Threshold = thresh,
                Map = map
            }.Run();
        }

        [BurstCompile]
        struct NoiseJob : IJob
        {
            public BasicPathingMap Map;
            public int Iterations;
            public float Persistence;
            public float Scale;
            public int Low;
            public int High;
            public int Threshold;

            public void Execute()
            {
                for (int i = 0; i < Map.Length; ++i)
                {
                    int2 p = Grid2D.IndexToPos(i, Map.Size.x);
                    float noise = SumOctave(p.x, p.y,
                        Iterations, Persistence, Scale, Low, High);
                    if (noise >= Threshold)
                    {
                        Map.SetIsObstacle(p.x, p.y, true);
                    }
                }
            }
        }

        static float SumOctave(
        int x, int y,
        int iterations, float persistence, float scale,
        int low, int high)
        {
            float maxAmp = 0;
            float amp = 1;
            float freq = scale;
            float v = 0;

            for (int i = 0; i < iterations; ++i)
            {
                v += noise.snoise(new float2(x * freq, y * freq)) * amp;
                maxAmp += amp;
                amp *= persistence;
                freq *= 2;
            }

            v /= maxAmp;

            v = v * (high - low) / 2f + (high + low) / 2f;

            return v;
        }
    } 
}
