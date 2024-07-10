using Godot;
using System;

public class TecmoControl : Label
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }
	public TecmoHelper TecmoHelper
    {
        get {
            if (TecmoHelper.Instance == null)
				 new TecmoHelper();
            GD.Print($"TecmoHelper.Instance == null: {TecmoHelper.Instance == null}");
            return TecmoHelper.Instance;
		}
    }
}
