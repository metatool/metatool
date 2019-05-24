using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Metaseed.Input
{
    public class KeyEventArgsExt : KeyEventArgs
    {
        public KeyEventArgsExt(Keys keyData)
            : base(keyData)
        {
        }

        internal KeyEventArgsExt(Gma.System.MouseKeyHook.KeyEventArgsExt e) : this(e.KeyData)
        {
            this.ScanCode = e.ScanCode;
            this.Timestamp = e.Timestamp;
            this.IsExtendedKey = e.IsExtendedKey;
            this.IsKeyDown = e.IsKeyDown;
            this.IsKeyUp = e.IsKeyUp;
        }

        internal KeyEventArgsExt(
            Keys keyData,
            int  scanCode,
            int  timestamp,
            bool isKeyDown,
            bool isKeyUp,
            bool isExtendedKey)
            : this(keyData)
        {
            this.ScanCode      = scanCode;
            this.Timestamp     = timestamp;
            this.IsKeyDown     = isKeyDown;
            this.IsKeyUp       = isKeyUp;
            this.IsExtendedKey = isExtendedKey;
        }

        public int ScanCode { get; }

        public int Timestamp { get; }

        public bool IsKeyDown { get; }

        public bool IsKeyUp { get; }

        public bool IsExtendedKey { get; }
    }
}