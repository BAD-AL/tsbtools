using Godot;
using System;

public partial class PlayControl : Panel
{
	private TextureRectWithMouse? playImage = null;
	private PlayType playType = PlayType.NONE;
	private Label displayLabel;
	private Button decreaseButton;
	private Button increaseButton;

	[Signal]
	public delegate void value_changed(int value);

	[Export]
	public PlayType PlayType
	{
		get { return playType; }
		set
		{ 
			if(value != playType)
			{
				playType = value;
				UpdateState();
			}
		}
	}

	int playNumber = 1;
	// 1-8
	[Export]
	public int PlayNumber
	{
		get { return playNumber; }
		set
		{
			if (value != playNumber)
			{
				playNumber = value;
				UpdateState();
				EmitSignal(nameof(value_changed), playNumber);
			}
		}
	}

	// 1-4 
	[Export]
	public int PlaySlot { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		playImage = GetNode<TextureRectWithMouse>("VBoxContainer/playImageTextureRectWithMouse");
		decreaseButton = GetNode<Button>("VBoxContainer/bottomPanel/HBoxContainer/decreaseButton");
		increaseButton = GetNode<Button>("VBoxContainer/bottomPanel/HBoxContainer/increaseButton");
		displayLabel = GetNode<Label>("VBoxContainer/bottomPanel/HBoxContainer/displayLabel");

		increaseButton.Connect("pressed", this, nameof(_on_increase_play_number));
		decreaseButton.Connect("pressed", this, nameof(_on_decrease_play_number));
		UpdateState ();
	}

	private void _on_decrease_play_number()
	{
		if( PlayNumber > 1) // 1 ==> min
			PlayNumber --;
		UpdateState();
	}

	private void _on_increase_play_number()
	{
		if (PlayNumber < 8) // 8 ==> max
			PlayNumber++;
		UpdateState();
	}

	private void PlayNumber_ValueChanged(double value)
	{
		PlayNumber = (int)value;
		UpdateState();
	}

	private void UpdateState()
	{
		if (this.PlayType == PlayType.NONE || PlayNumber < 1 || PlaySlot < 1)
			return;

		string prefix = "P";
		if (this.PlayType == PlayType.RUN)
			prefix = "R";
		// path be like -> "res://Images/PLAYS/P1-0.BMP"
		string filePath = $"res://Images/PLAYS/{prefix}{PlaySlot}-{PlayNumber-1}.BMP";
		playImage.Texture = GD.Load<Texture>(filePath);
		displayLabel.Text = PlayNumber.ToString();
	}

}

public enum PlayType
{
	NONE=0,
	RUN,
	PASS
}