namespace Metatool.Service.MouseKey;

public class KeyStateTrees
{
	// map one key to another
	public const string HardMap = "HardMap";
	// the key mapped is just to be used in Chord not in Trigger (i.e. A and B in A+B+Trigger)
	public const string ChordMap = "ChordMap";
	public const string Default = "Default";
	/// <summary>
	///  i.e. GK+1 => F1
	/// </summary>
	public const string Map = "Map";
	// hot string i.e. after typing "tks" => "thank you very much"
	public const string HotString = "HotString";
}