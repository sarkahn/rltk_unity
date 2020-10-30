using RLTK.MonoBehaviours;
using Unity.Mathematics;
using UnityEngine;

namespace RLTK.Samples
{
    public class TestPrint : MonoBehaviour
    {
        [SerializeField]
        Color _color = Color.blue;

        public SimpleConsoleProxy _console;

        public int2 _position;

        public string _text = "Hello, world!";

        private void Awake()
        {
            _console = GetComponent<SimpleConsoleProxy>();

        }

        // Update is called once per frame
        void Update()
        { 
            _console.ClearScreen();
            _console.PrintColor(_position.x, _position.y, _text, _color);
        }
    }
}