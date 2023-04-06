namespace Metatool.Service;

public interface IToggleKey
{
	ToggleKeyState State { get; }
	void AlwaysOn();
	void AlwaysOff();
	void Off();
	void On();
}