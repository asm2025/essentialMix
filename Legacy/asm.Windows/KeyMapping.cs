using System.Windows.Forms;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Windows
{
	/// <summary>
	/// A KeyMapping defines how a key combination should
	/// be mapped to a SendKeys message.
	/// </summary>
	public struct KeyMapping
	{
		/// <summary>
        /// Initializes a new instance of the <see cref="KeyMapping"/> class.
        /// </summary>
        /// <param name="keyCode">The key code.</param>
        /// <param name="sendKeysMapping">The send keys mapping.</param>
        /// <param name="streamMapping">The stream mapping.</param>
        public KeyMapping(Keys keyCode, string sendKeysMapping, string streamMapping)
			: this(false, false, false, keyCode, sendKeysMapping, streamMapping)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyMapping"/> class.
        /// </summary>
        /// <param name="control">if set to <c>true</c> [control].</param>
        /// <param name="alt">if set to <c>true</c> [alt].</param>
        /// <param name="shift">if set to <c>true</c> [shift].</param>
        /// <param name="keyCode">The key code.</param>
        /// <param name="sendKeysMapping">The send keys mapping.</param>
        /// <param name="streamMapping">The stream mapping.</param>
        public KeyMapping(bool control, bool alt, bool shift, Keys keyCode, string sendKeysMapping, string streamMapping)
        {
            //  Set the member variables.
            IsControlPressed = control;
            IsAltPressed = alt;
            IsShiftPressed = shift;
            KeyCode = keyCode;
            SendKeysMapping = sendKeysMapping;
            StreamMapping = streamMapping;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is control pressed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is control pressed; otherwise, <c>false</c>.
        /// </value>
        public bool IsControlPressed
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether alt is pressed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is alt pressed; otherwise, <c>false</c>.
        /// </value>
        public bool IsAltPressed
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is shift pressed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is shift pressed; otherwise, <c>false</c>.
        /// </value>
        public bool IsShiftPressed
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the key code.
        /// </summary>
        /// <value>
        /// The key code.
        /// </value>
        public Keys KeyCode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the send keys mapping.
        /// </summary>
        /// <value>
        /// The send keys mapping.
        /// </value>
        public string SendKeysMapping
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the stream mapping.
        /// </summary>
        /// <value>
        /// The stream mapping.
        /// </value>
        public string StreamMapping
        {
            get;
            set;
        }

		public bool Equals(KeyMapping other)
		{
			return KeyCode == other.KeyCode && IsAltPressed == other.IsAltPressed && IsControlPressed == other.IsControlPressed && IsShiftPressed == other.IsShiftPressed;
		}

		public bool Equals([NotNull] KeyEventArgs other)
		{
			return KeyCode == other.KeyCode && IsAltPressed == other.Alt && IsControlPressed == other.Control && IsShiftPressed == other.Shift;
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			switch (obj)
			{
				case null:
					return false;
				case KeyMapping mapping:
					return Equals(mapping);
				case KeyEventArgs mappingArgs:
					return Equals(mappingArgs);
				default:
					return false;
			}
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 397 ^ (int)KeyCode;
				hash = (hash * 397) ^ IsControlPressed.Value();
				hash = (hash * 397) ^ IsAltPressed.Value();
				hash = (hash * 397) ^ IsShiftPressed.Value();
				return hash;
			}
		}

		public static bool operator ==(KeyMapping keyMapping, KeyEventArgs keyEventArgs) { return keyEventArgs != null && keyMapping.Equals(keyEventArgs); }

		public static bool operator !=(KeyMapping keyMapping, KeyEventArgs keyEventArgs) { return !(keyMapping == keyEventArgs); }
	}
}