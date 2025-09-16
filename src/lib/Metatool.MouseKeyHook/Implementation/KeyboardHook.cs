using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service;
using Metatool.Service.MouseKey;
using Metatool.UI;
using Microsoft.Extensions.Logging;

namespace Metatool.Input.MouseKeyHook;

public delegate void KeyEventHandler(object sender, IKeyEventArgs e);

public delegate void KeyPressEventHandler(object sender, IKeyPressEventArgs e);

public class KeyboardHook
{
	private readonly INotify               _notify;
	private readonly ILogger<KeyboardHook> _logger;
	private readonly IKeyboardMouseEvents  _eventSource;
	private          bool                  _isRunning;

	public bool Disable
	{
		get => _eventSource.Disable;
		set => _eventSource.Disable = value;
	}
	public bool DisableDownEvent
	{
		get => _eventSource.DisableDownEvent;
		set => _eventSource.DisableDownEvent = value;
	}
	public bool DisableUpEvent
	{
		get => _eventSource.DisableUpEvent;
		set => _eventSource.DisableUpEvent = value;
	}
	public bool DisablePressEvent
	{
		get => _eventSource.DisablePressEvent;
		set => _eventSource.DisablePressEvent = value;
	}

	internal void DisableChord(Chord chord, string stateTree = null)
	{
		if (stateTree == null)
		{
			foreach (var tree in KeyStateTree.StateTrees.Values)
			{
				tree.DisableChord(chord);
			}
			return;
		}
		var tre = KeyStateTree.GetOrCreateStateTree(stateTree);
		tre.DisableChord(chord);
	}

	internal void EnableChord(Chord chord, string stateTree = null)
	{
		if (stateTree == null)
		{
			foreach (var tree in KeyStateTree.StateTrees.Values)
			{
				tree.EnableChord(chord);
			}
			return;
		}
		var tre = KeyStateTree.GetOrCreateStateTree(stateTree);
		tre.EnableChord(chord);
	}

	public KeyboardHook(ILogger<KeyboardHook> logger, INotify notify)
	{
		_notify             = notify;
		KeyStateTree.Notify = notify;
		_logger             = logger;
		_eventSource        = Hook.GlobalEvents();
	}

	public IMetaKey Add(IList<ICombination> combinations, KeyEventCommand command,
		string stateTree = KeyStateTrees.Default)
	{
		var keyStateTree = KeyStateTree.GetOrCreateStateTree(stateTree);
		return keyStateTree.Add(combinations, command);
	}

	private readonly List<KeyEventHandler> _keyUpHandlers = new();

	public event KeyEventHandler KeyUp
	{
		add => _keyUpHandlers.Add(value);
		remove => _keyUpHandlers.Remove(value);
	}

	private readonly List<KeyPressEventHandler> _keyPressHandlers = new();

	public event KeyPressEventHandler KeyPress
	{
		add => _keyPressHandlers.Add(value);
		remove => _keyPressHandlers.Remove(value);
	}

	public bool HandleVirtualKey
	{
		get => _eventSource.HandleVirtualKey;
		set => _eventSource.HandleVirtualKey = value;
	}

	private readonly List<KeyEventHandler> _keyDownHandlers = new();

	public event KeyEventHandler KeyDown
	{
		add => _keyDownHandlers.Add(value);
		remove => _keyDownHandlers.Remove(value);
	}

	public void ShowTip(bool ifRootThenEmpty = false)
	{
		var tips = KeyStateTree.StateTrees.Values.SelectMany(m => m.Tips(ifRootThenEmpty)).ToArray();
		if (tips.Length > 0)
			_notify?.ShowKeysTip(tips);
		else
		{
			_notify?.CloseKeysTip();
		}
	}

	public void Run()
	{
		if (_isRunning) return;
		Debug.Assert(System.Windows.Application.Current.Dispatcher != null,
			"System.Windows.Application.Current.Dispatcher != null");
		var access = System.Windows.Application.Current.Dispatcher.CheckAccess();
		if (!access)
		{
			System.Windows.Application.Current.Dispatcher.BeginInvoke((Action) Run);
			return;
		}

		_isRunning = true;
		_logger.LogInformation($"Keyboard hook is running...");

		foreach (var stateTree in KeyStateTree.StateTrees.Values) stateTree.Reset();
		var selectedTrees = new List<KeyStateTree.SelectionResult>();

		//eventType is only Down or Up
		void ClimbTree(KeyEventType eventType, IKeyEventArgs args,
			ILogger logger)
		{
			// if machine_1 has A+B and machine_2's A and B, press A+B on machine_1 would be processed
			// if machine_1 has A and machine_2 has A, both should be processed.
			// runs like all are in the same tree, but provide state jump for every tree
			// // continue process this event on current machine
			bool reprocess;
			do
			{
				reprocess = false;
				var onGround = false;
				if (selectedTrees.Count == 0)
				{
					onGround      = true;
					selectedTrees = SelectTree(eventType, args, logger);
				}

				var hasSelectedNodes = selectedTrees.Count > 0;
				if (!hasSelectedNodes) goto @return;

				var trees = selectedTrees.GetRange(0, selectedTrees.Count);
				foreach (var c in trees)
				{
					var selectedTree = c; // should not remove this line
					if (!onGround)
					{
						var result = selectedTree.Tree.TrySelect(eventType, args);
						var index  = selectedTrees.IndexOf(selectedTree);
						selectedTrees[index] = result;
						selectedTree         = result;
					}

					var rt = selectedTree.Tree.Climb(eventType, args, selectedTree.CandidateNode,
						selectedTree.DownInChord);
					logger.LogInformation($"\t={rt}${selectedTree.Tree.Name}@{selectedTree.Tree.CurrentNode}");
					if (rt == KeyProcessState.Continue)
					{
					}
					else if (rt == KeyProcessState.Done)
					{
						selectedTrees.Remove(selectedTree);
					}
					else if (rt == KeyProcessState.NoFurtherProcess)
					{
						selectedTrees.Remove(selectedTree);
						goto @return;
					}
					else if (rt == KeyProcessState.Reprocess || rt == KeyProcessState.Yield)
					{
						selectedTrees.Remove(selectedTree);
						reprocess = true;
					}
					else
					{
						throw new ArgumentOutOfRangeException();
					}
				}
			} while (selectedTrees.Count == 0 && /*no KeyProcessState.Continue*/
			         reprocess /*Yield or Reprocess*/);

			@return:
			foreach (var stateTree in KeyStateTree.StateTrees.Values) stateTree.MarkDoneIfYield();
		}

		_eventSource.KeyDown += (sender, args) =>
		{
			ClimbTree(KeyEventType.Down, args, _logger);
			// a copy, so newly added would be handled in next event.
			var handlers = new List<KeyEventHandler>(_keyDownHandlers);
			handlers.ForEach(h => h?.Invoke(sender, args));
		};

		_eventSource.KeyPress += (sender, args) =>
		{
			var handlers = new List<KeyPressEventHandler>(_keyPressHandlers); // a copy
			handlers.ForEach(h => h?.Invoke(sender, args));
		};

        _eventSource.KeyUp += (sender, args) =>
        {
            ClimbTree(KeyEventType.Up, args, _logger);
            var handlers = new List<KeyEventHandler>(_keyUpHandlers); // a copy
            handlers.ForEach(h => h?.Invoke(sender, args));
        };
    }

	// the list count is one currently
	static List<KeyStateTree.SelectionResult> SelectTree(KeyEventType eventTypeType, IKeyEventArgs args, ILogger logger)
	{
		var selectedNodes = new List<KeyStateTree.SelectionResult>();
		//all on root, find current trees
		foreach (var stateTree in KeyStateTree.StateTrees.Values)
		{
			Debug.Assert(stateTree.IsOnRoot);
			if (stateTree.ProcessState == KeyProcessState.Yield) continue;

			var selectionResult = stateTree.TrySelect(eventTypeType, args);
			if (selectionResult.CandidateNode == null) continue;

			if (selectedNodes.Count == 0)
			{
				selectedNodes.Add(selectionResult);
			}
			else if (selectionResult.CandidateNode.Key.ChordLength > selectedNodes[0].CandidateNode.Key.ChordLength)
			{
				selectedNodes.Clear();
				selectedNodes.Add(selectionResult);
			}
		}

		if (selectedNodes.Count > 0)
			logger.LogInformation(
				$"ToClimb:{string.Join(",", selectedNodes.Select(t => $"${t.Tree.Name}_{t.CandidateNode}"))}");
		return selectedNodes;
	}
}