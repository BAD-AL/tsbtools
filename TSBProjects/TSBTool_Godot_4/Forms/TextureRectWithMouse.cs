using Godot;
using System;

public partial class TextureRectWithMouse : TextureRect
{

	[Signal]
	public delegate void MouseDownEventHandler(InputEventMouseButton mbe);

	[Signal]
	public delegate void MouseUpEventHandler(InputEventMouseButton mbe);

	public override void _Input(InputEvent the_event)
	{
		base._Input(the_event);
		InputEventMouseButton? mbe = the_event as InputEventMouseButton;
		if(InsideMe(mbe))
		{
			if ( mbe.Pressed )
				EmitSignal(SignalName.MouseDown, mbe);
			else if(mbe.IsReleased() )
				EmitSignal(SignalName.MouseUp, mbe);
		}
	}

	private bool InsideMe(InputEventMouseButton? mbe)
	{
		if (mbe != null &&
			mbe.Position.X > this.GlobalPosition.X && mbe.Position.X < (this.GlobalPosition.X + this.Size.X ) &&
			mbe.Position.Y > this.GlobalPosition.Y && mbe.Position.Y < (this.GlobalPosition.Y + this.Size.Y)    )
		{
			return true;
		}
		return false;
	}

}
