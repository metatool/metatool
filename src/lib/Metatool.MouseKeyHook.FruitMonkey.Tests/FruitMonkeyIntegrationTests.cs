using Metatool.Input;
using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.MouseKeyHook.FruitMonkey.Tests.Mocks;
using Metatool.Service.MouseKey;
using FruitMonkeyClass = Metatool.Input.FruitMonkey;

namespace Metatool.MouseKeyHook.FruitMonkey.Tests;

public class FruitMonkeyIntegrationTests
{
    private readonly MockKeyTipNotifier _notifier;
    private readonly MockLogger _logger;
    private readonly MockKeyboardState _keyboardState;
    private readonly FruitMonkeyClass _fruitMonkey;

    public FruitMonkeyIntegrationTests()
    {
        _notifier = new MockKeyTipNotifier();
        _logger = new MockLogger();
        _keyboardState = new MockKeyboardState();
        _fruitMonkey = new FruitMonkeyClass(_logger, _notifier);
    }

    private KeyCommand CreateTrackingCommand(List<string> executionLog, string name) =>
        new KeyCommand(_ => executionLog.Add(name)) { Description = name };

    private KeyEventCommand CreateDownCommand(List<string> log, string name) =>
        new KeyEventCommand(KeyEventType.Down, CreateTrackingCommand(log, name));

    private KeyEventCommand CreateUpCommand(List<string> log, string name) =>
        new KeyEventCommand(KeyEventType.Up, CreateTrackingCommand(log, name));

    private MockKeyEventArgs SimulateKeyDown(KeyCodes keyCode)
    {
        _keyboardState.SetKeyDown(keyCode);
        return new MockKeyEventArgs(keyCode, KeyEventType.Down, _keyboardState);
    }

    private MockKeyEventArgs SimulateKeyUp(KeyCodes keyCode)
    {
        _keyboardState.SetKeyUp(keyCode);
        return new MockKeyEventArgs(keyCode, KeyEventType.Up, _keyboardState);
    }

    #region Single Key Tests

    [Fact]
    public void ClimbTree_SingleKeyDown_ShouldExecuteCommand()
    {
        var executionLog = new List<string>();
        var combination = new Combination(KeyCodes.A);
        _fruitMonkey.Forest.Add([combination], CreateDownCommand(executionLog, "A_Down"));

        // Simulate: A key down
        var args = SimulateKeyDown(KeyCodes.A);
        _fruitMonkey.ClimbTree(args);

        Assert.Contains("A_Down", executionLog);
    }

    [Fact]
    public void ClimbTree_SingleKeyUp_ShouldExecuteCommand()
    {
        var executionLog = new List<string>();
        var combination = new Combination(KeyCodes.A);
        _fruitMonkey.Forest.Add([combination], CreateUpCommand(executionLog, "A_Up"));

        // Simulate: A key down then up
        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.A));
        _fruitMonkey.ClimbTree(SimulateKeyUp(KeyCodes.A));

        Assert.Contains("A_Up", executionLog);
    }

    [Fact]
    public void ClimbTree_SingleKeyDownAndUp_ShouldExecuteBothCommands()
    {
        var executionLog = new List<string>();
        var combination = new Combination(KeyCodes.A);
        _fruitMonkey.Forest.Add([combination], CreateDownCommand(executionLog, "A_Down"));
        _fruitMonkey.Forest.Add([combination], CreateUpCommand(executionLog, "A_Up"));

        // Simulate: A key down then up
        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.A));
        _fruitMonkey.ClimbTree(SimulateKeyUp(KeyCodes.A));

        Assert.Equal(2, executionLog.Count);
        Assert.Equal("A_Down", executionLog[0]);
        Assert.Equal("A_Up", executionLog[1]);
    }

    [Fact]
    public void ClimbTree_UnregisteredKey_ShouldNotExecuteAnyCommand()
    {
        var executionLog = new List<string>();
        var combination = new Combination(KeyCodes.A);
        _fruitMonkey.Forest.Add([combination], CreateDownCommand(executionLog, "A_Down"));

        // Simulate: B key down (not registered)
        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.B));

        Assert.Empty(executionLog);
    }

    #endregion

    #region Chord Combination Tests

    [Fact]
    public void ClimbTree_ChordCombination_ShouldExecuteCommand()
    {
        var executionLog = new List<string>();
        var combination = new Combination(KeyCodes.A, KeyCodes.ControlKey);
        _fruitMonkey.Forest.Add([combination], CreateDownCommand(executionLog, "Ctrl+A_Down"));

        // Simulate: Ctrl down, then A down
        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.ControlKey));
        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.A));

        Assert.Contains("Ctrl+A_Down", executionLog);
    }

    [Fact]
    public void ClimbTree_ChordCombination_WrongOrder_ShouldNotExecute()
    {
        var executionLog = new List<string>();
        var combination = new Combination(KeyCodes.A, KeyCodes.ControlKey);
        _fruitMonkey.Forest.Add([combination], CreateDownCommand(executionLog, "Ctrl+A_Down"));

        // Simulate: A down without Ctrl (chord not active)
        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.A));

        Assert.DoesNotContain("Ctrl+A_Down", executionLog);
    }

    [Fact]
    public void ClimbTree_ChordWithMultipleModifiers_ShouldExecuteCommand()
    {
        var executionLog = new List<string>();
        var combination = new Combination(KeyCodes.S, [KeyCodes.ControlKey, KeyCodes.ShiftKey]);
        _fruitMonkey.Forest.Add([combination], CreateDownCommand(executionLog, "Ctrl+Shift+S_Down"));

        // Simulate: Ctrl down, Shift down, S down
        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.ControlKey));
        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.ShiftKey));
        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.S));

        Assert.Contains("Ctrl+Shift+S_Down", executionLog);
    }

    [Fact]
    public void ClimbTree_ChordUp_ShouldExecuteUpCommand()
    {
        var executionLog = new List<string>();
        var combination = new Combination(KeyCodes.A, KeyCodes.ControlKey);
        _fruitMonkey.Forest.Add([combination], CreateUpCommand(executionLog, "Ctrl+A_Up"));

        // Simulate: Ctrl down, A down, A up (Ctrl still down)
        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.ControlKey));
        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.A));
        _fruitMonkey.ClimbTree(SimulateKeyUp(KeyCodes.A));

        Assert.Contains("Ctrl+A_Up", executionLog);
    }

    #endregion

    #region Sequence Tests

    [Fact]
    public void ClimbTree_Sequence_ShouldExecuteCommandAtEndOfSequence()
    {
        var executionLog = new List<string>();
        var path = new List<ICombination>
        {
            new Combination(KeyCodes.A),
            new Combination(KeyCodes.B)
        };
        _fruitMonkey.Forest.Add(path, CreateUpCommand(executionLog, "A_B_Up"));

        // Simulate: A down, A up, B down, B up
        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.A));
        _fruitMonkey.ClimbTree(SimulateKeyUp(KeyCodes.A));
        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.B));
        _fruitMonkey.ClimbTree(SimulateKeyUp(KeyCodes.B));

        Assert.Contains("A_B_Up", executionLog);
    }

    [Fact]
    public void ClimbTree_Sequence_PartialMatch_ShouldNotExecute()
    {
        var executionLog = new List<string>();
        var path = new List<ICombination>
        {
            new Combination(KeyCodes.A),
            new Combination(KeyCodes.B)
        };
        _fruitMonkey.Forest.Add(path, CreateUpCommand(executionLog, "A_B_Up"));

        // Simulate: A down, A up only (incomplete sequence)
        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.A));
        _fruitMonkey.ClimbTree(SimulateKeyUp(KeyCodes.A));

        Assert.DoesNotContain("A_B_Up", executionLog);
    }

    [Fact]
    public void ClimbTree_Sequence_WrongKey_ShouldReset()
    {
        var executionLog = new List<string>();
        var path = new List<ICombination>
        {
            new Combination(KeyCodes.A),
            new Combination(KeyCodes.B)
        };
        _fruitMonkey.Forest.Add(path, CreateUpCommand(executionLog, "A_B_Up"));

        // Simulate: A down, A up, C down (wrong key), C up
        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.A));
        _fruitMonkey.ClimbTree(SimulateKeyUp(KeyCodes.A));
        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.C));
        _fruitMonkey.ClimbTree(SimulateKeyUp(KeyCodes.C));

        Assert.DoesNotContain("A_B_Up", executionLog);
    }

    [Fact]
    public void ClimbTree_LongerSequence_ShouldExecuteAtEnd()
    {
        var executionLog = new List<string>();
        var path = new List<ICombination>
        {
            new Combination(KeyCodes.A),
            new Combination(KeyCodes.B),
            new Combination(KeyCodes.C)
        };
        _fruitMonkey.Forest.Add(path, CreateUpCommand(executionLog, "A_B_C_Up"));

        // Simulate: A, B, C sequence
        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.A));
        _fruitMonkey.ClimbTree(SimulateKeyUp(KeyCodes.A));
        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.B));
        _fruitMonkey.ClimbTree(SimulateKeyUp(KeyCodes.B));
        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.C));
        _fruitMonkey.ClimbTree(SimulateKeyUp(KeyCodes.C));

        Assert.Contains("A_B_C_Up", executionLog);
    }

    #endregion

    #region Mixed Sequence with Chords Tests

    [Fact]
    public void ClimbTree_ChordThenKey_Sequence_ShouldExecute()
    {
        var executionLog = new List<string>();
        var path = new List<ICombination>
        {
            new Combination(KeyCodes.A, KeyCodes.ControlKey),
            new Combination(KeyCodes.B)
        };
        _fruitMonkey.Forest.Add(path, CreateUpCommand(executionLog, "Ctrl+A_B_Up"));

        // Simulate: Ctrl+A, then B
        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.ControlKey));
        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.A));
        _fruitMonkey.ClimbTree(SimulateKeyUp(KeyCodes.A));
        _fruitMonkey.ClimbTree(SimulateKeyUp(KeyCodes.ControlKey));
        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.B));
        _fruitMonkey.ClimbTree(SimulateKeyUp(KeyCodes.B));

        Assert.Contains("Ctrl+A_B_Up", executionLog);
    }

    #endregion

    #region Multiple Commands on Same Path Tests

    [Fact]
    public void ClimbTree_MultipleCommandsOnSameKey_ShouldExecuteAll()
    {
        var executionLog = new List<string>();
        var combination = new Combination(KeyCodes.A);
        _fruitMonkey.Forest.Add([combination], CreateDownCommand(executionLog, "Command1"));
        _fruitMonkey.Forest.Add([combination], CreateDownCommand(executionLog, "Command2"));

        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.A));

        Assert.Equal(2, executionLog.Count);
        Assert.Contains("Command1", executionLog);
        Assert.Contains("Command2", executionLog);
    }

    #endregion

    #region Different Trees Tests

    [Fact]
    public void ClimbTree_DifferentTrees_ShouldExecuteInPriority()
    {
        var executionLog = new List<string>();
        var combination = new Combination(KeyCodes.A);

        // Add to HardMap (higher priority)
        _fruitMonkey.Forest.Add([combination], CreateDownCommand(executionLog, "HardMap_A"), KeyStateTrees.HardMap);
        // Add to Default (lower priority)
        _fruitMonkey.Forest.Add([combination], CreateDownCommand(executionLog, "Default_A"), KeyStateTrees.Default);

        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.A));

        // Both should execute since they're in different trees
        Assert.Contains("HardMap_A", executionLog);
        Assert.Contains("Default_A", executionLog);
    }

    #endregion

    #region Reset Tests

    [Fact]
    public void Reset_ShouldClearTreeState()
    {
        var executionLog = new List<string>();
        var path = new List<ICombination>
        {
            new Combination(KeyCodes.A),
            new Combination(KeyCodes.B)
        };
        _fruitMonkey.Forest.Add(path, CreateUpCommand(executionLog, "A_B_Up"));

        // Start sequence
        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.A));
        _fruitMonkey.ClimbTree(SimulateKeyUp(KeyCodes.A));

        // Reset before completing
        _fruitMonkey.Reset();

        // Try to complete sequence - should not work since state was reset
        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.B));
        _fruitMonkey.ClimbTree(SimulateKeyUp(KeyCodes.B));

        Assert.DoesNotContain("A_B_Up", executionLog);
    }

    #endregion

    #region Command CanExecute Tests

    [Fact]
    public void ClimbTree_CommandWithCanExecuteFalse_ShouldNotExecute()
    {
        var executionLog = new List<string>();
        var combination = new Combination(KeyCodes.A);
        var command = new KeyCommand(_ => executionLog.Add("ShouldNotRun"))
        {
            Description = "Conditional",
            CanExecute = _ => false
        };
        _fruitMonkey.Forest.Add([combination], new KeyEventCommand(KeyEventType.Down, command));

        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.A));

        Assert.Empty(executionLog);
    }

    [Fact]
    public void ClimbTree_CommandWithCanExecuteTrue_ShouldExecute()
    {
        var executionLog = new List<string>();
        var combination = new Combination(KeyCodes.A);
        var command = new KeyCommand(_ => executionLog.Add("ShouldRun"))
        {
            Description = "Conditional",
            CanExecute = _ => true
        };
        _fruitMonkey.Forest.Add([combination], new KeyEventCommand(KeyEventType.Down, command));

        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.A));

        Assert.Contains("ShouldRun", executionLog);
    }

    #endregion

    #region Handled Flag Tests

    [Fact]
    public void ClimbTree_CommandSetsNoFurtherProcess_ShouldStopProcessing()
    {
        var executionLog = new List<string>();
        var combination = new Combination(KeyCodes.A);

        var stoppingCommand = new KeyCommand(args =>
        {
            executionLog.Add("Stopping");
            args.NoFurtherProcess = true;
        }) { Description = "Stopping" };

        _fruitMonkey.Forest.Add([combination],
            new KeyEventCommand(KeyEventType.Down, stoppingCommand), KeyStateTrees.HardMap);
        _fruitMonkey.Forest.Add([combination],
            CreateDownCommand(executionLog, "ShouldNotRun"), KeyStateTrees.Default);

        _fruitMonkey.ClimbTree(SimulateKeyDown(KeyCodes.A));

        Assert.Contains("Stopping", executionLog);
        Assert.DoesNotContain("ShouldNotRun", executionLog);
    }

    #endregion
}
