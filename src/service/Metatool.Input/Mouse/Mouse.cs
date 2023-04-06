using System.Drawing;
using Metatool.Service;
using Metatool.WindowsInput;

namespace Metatool.Input;

public class Mouse: IMouse
{
	static IMouseSimulator Simu = InputSimu.Inst.Mouse;

	public Point Position
	{
		get => Simu.Position;
		set => Simu.Position = value;
	}

	public IMouse LeftClick()
	{
		Simu.LeftClick();
		return this;
	}

	public IMouse RightClick()
	{
		Simu.RightClick();
		return this;
	}

	public IMouse MoveToLikeUser(int x, int y)
	{
		Simu.MoveToLikeUser(x, y);
		return this;
	}

	public IMouse MoveByLikeUser(int deltaX, int deltaY)
	{
		Simu.MoveByLikeUser(deltaX, deltaY);
		return this;
	}

	public IMouse VerticalScroll(int scrollAmountInClicks)
	{
		Simu.VerticalScroll(scrollAmountInClicks);
		return this;
	}
	public IMouse HorizontalScroll(int scrollAmountInClicks)
	{
		Simu.HorizontalScroll(scrollAmountInClicks);
		return this;
	}

}