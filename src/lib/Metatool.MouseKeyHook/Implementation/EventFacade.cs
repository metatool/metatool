

using System;
using System.Windows.Forms;

namespace Metatool.Input.MouseKeyHook.Implementation;

internal abstract class EventFacade : IKeyboardMouseEvents
{
	private KeyListener _keyListener;
	private MouseListener _mouseListener;

	public event KeyEventHandler KeyDown
	{
		add => KeyListener.KeyDown += value;
		remove => KeyListener.KeyDown -= value;
	}
	public bool HandleVirtualKey
	{
		get => KeyListener.HandleVirtualKey;
		set => KeyListener.HandleVirtualKey = value;
	}

	public event KeyPressEventHandler KeyPress
	{
		add => KeyListener.KeyPress += value;
		remove => KeyListener.KeyPress -= value;
	}

	public event KeyEventHandler KeyUp
	{
		add => KeyListener.KeyUp += value;
		remove => KeyListener.KeyUp -= value;
	}

	public event MouseEventHandler MouseMove
	{
		add => MouseListener.MouseMove += value;
		remove => MouseListener.MouseMove -= value;
	}

	public event EventHandler<MouseEventExtArgs> MouseMoveExt
	{
		add => MouseListener.MouseMoveExt += value;
		remove => MouseListener.MouseMoveExt -= value;
	}

	public event MouseEventHandler MouseClick
	{
		add => MouseListener.MouseClick += value;
		remove => MouseListener.MouseClick -= value;
	}

	public event MouseEventHandler MouseDown
	{
		add => MouseListener.MouseDown += value;
		remove => MouseListener.MouseDown -= value;
	}

	public event EventHandler<MouseEventExtArgs> MouseDownExt
	{
		add => MouseListener.MouseDownExt += value;
		remove => MouseListener.MouseDownExt -= value;
	}

	public event MouseEventHandler MouseUp
	{
		add => MouseListener.MouseUp += value;
		remove => MouseListener.MouseUp -= value;
	}

	public event EventHandler<MouseEventExtArgs> MouseUpExt
	{
		add => MouseListener.MouseUpExt += value;
		remove => MouseListener.MouseUpExt -= value;
	}

	public event MouseEventHandler MouseWheel
	{
		add => MouseListener.MouseWheel += value;
		remove => MouseListener.MouseWheel -= value;
	}

	public event EventHandler<MouseEventExtArgs> MouseWheelExt
	{
		add => MouseListener.MouseWheelExt += value;
		remove => MouseListener.MouseWheelExt -= value;
	}

	public event MouseEventHandler MouseDoubleClick
	{
		add => MouseListener.MouseDoubleClick += value;
		remove => MouseListener.MouseDoubleClick -= value;
	}

	public event MouseEventHandler MouseDragStarted
	{
		add => MouseListener.MouseDragStarted += value;
		remove => MouseListener.MouseDragStarted -= value;
	}

	public event EventHandler<MouseEventExtArgs> MouseDragStartedExt
	{
		add => MouseListener.MouseDragStartedExt += value;
		remove => MouseListener.MouseDragStartedExt -= value;
	}

	public event MouseEventHandler MouseDragFinished
	{
		add => MouseListener.MouseDragFinished += value;
		remove => MouseListener.MouseDragFinished -= value;
	}

	public event EventHandler<MouseEventExtArgs> MouseDragFinishedExt
	{
		add => MouseListener.MouseDragFinishedExt += value;
		remove => MouseListener.MouseDragFinishedExt -= value;
	}

	public bool Disable
	{
		get => MouseListener.Disable && KeyListener.Disable;
		set
		{
			MouseListener.Disable = value;
			KeyListener.Disable = value;
		}
	}

	public bool DisableDownEvent
	{
		get => KeyListener.DisableDownEvent;
		set => KeyListener.DisableDownEvent = value;
	}
	public bool DisableUpEvent
	{
		get => KeyListener.DisableUpEvent;
		set => KeyListener.DisableUpEvent = value;
	}
	public bool DisablePressEvent
	{
		get => KeyListener.DisablePressEvent;
		set => KeyListener.DisablePressEvent = value;
	}
	public void Dispose()
	{
		if (_mouseListener != null) _mouseListener.Dispose();
		if (_keyListener != null) _keyListener.Dispose();
	}

	private KeyListener KeyListener
	{
		get {
			if (_keyListener != null)
				return _keyListener;
			_keyListener = CreateKeyListener();
			return _keyListener;
		}
	}

	private MouseListener MouseListener
	{
        get
        {
            if (_mouseListener != null)
                return _mouseListener;
            _mouseListener = CreateMouseListener();
            return _mouseListener;
        }
    }

	protected abstract MouseListener CreateMouseListener();
	protected abstract KeyListener CreateKeyListener();
}