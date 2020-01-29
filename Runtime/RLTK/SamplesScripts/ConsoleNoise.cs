using RLTK.MonoBehaviours;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


namespace RLTK.Samples
{
    public class ConsoleNoise : MonoBehaviour
    {
        SimpleConsoleProxy _console;

        JobHandle _noiseJob;

        NativeArray<Tile> _buffer;

        float2 p = new float2();

        public float _scale = 1f;

        public float _scrollSpeed = 1f;

        private void OnEnable()
        {
            _console = GetComponent<SimpleConsoleProxy>();
            if (_console == null)
                Debug.LogError($"Couldn't find a console on this object", gameObject);
        }

        void InitBuffer()
        {
            if (!_buffer.IsCreated || _buffer.Length != _console.CellCount)
            {
                if (_buffer.IsCreated)
                    _buffer.Dispose();

                _buffer = new NativeArray<Tile>( _console.CellCount, Allocator.Persistent);
            }
        }


        private void OnDisable()
        {
            _noiseJob.Complete();

            if (_buffer.IsCreated)
                _buffer.Dispose();
        }

        private void Update()
        {

            p.x += Time.deltaTime * (_scrollSpeed * 10);


            if (_noiseJob.IsCompleted)
            {
                //// Required by the safety system
                _noiseJob.Complete();



                InitBuffer();
                
                _noiseJob = new NoiseJob
                {
                    width = _console.Width,
                    height = _console.Height,
                    originX = (int)p.x,
                    originY = (int)p.y,
                    buffer = _buffer,
                    scale = _scale,
                }.Schedule(_buffer.Length, 64, _noiseJob);

                _noiseJob = _console.ScheduleWriteTiles(_buffer, _noiseJob);

                Color fg = new Color32(0xDF, 0xD6, 0x20, 255);
                Color bg = Color.black;//new Color32(0x40, 0x64, 0xBF, 0xFF);
                int x = _console.Width / 2 - 19 / 2;
                int y = _console.Height - 6;
                _console.PrintColor(x, y + 0, "                   ", fg, bg);
                _console.PrintColor(x, y + 2, "      RLTK         ", fg, bg);
                _console.PrintColor(x, y + 1, " Roguelike Toolkit ", fg, bg);
                _console.PrintColor(x, y + 3, "                   ", fg, bg);
            }
            
        }

        static byte Select(float v)
        {
            if (v >= .75f)
                return CodePage437.ToCP437('█');
            else if (v < .75f && v >= .55f)
                return CodePage437.ToCP437('▓');
            else if (v < .55f && v >= .35f)
                return CodePage437.ToCP437('▒');
            else if (v < .35f && v >= .15f)
                return CodePage437.ToCP437('░');
            return 0;
        }

        [BurstCompile]
        struct NoiseJob : IJobParallelFor
        {
            public int width;
            public int height;

            public int originX;
            public int originY;

            public float scale;

            public NativeArray<Tile> buffer;

            public void Execute(int index)
            {
                int x = index % width;
                int y = width - (index / width);
                
                x += originX;
                y += originY;
                
                float2 cell = new float2((float)x / width, (float)y / height);

                cell *= scale;

                float v = noise.snoise(cell) / 2f + .5f;


                var t = buffer[index];

                t.fgColor = Color.white * v;
                t.bgColor = Color.blue * v;
                t.glyph = Select(v);

                buffer[index] = t;
            }

        }
    }
}