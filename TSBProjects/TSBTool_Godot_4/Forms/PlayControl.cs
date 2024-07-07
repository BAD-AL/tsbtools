using Godot;
using System;

public partial class PlayControl : Panel
{
	private TextureRectWithMouse? playImage = null;
	private SpinBox? playNumberSpinBox = null;
	private PlayType playType = PlayType.NONE;

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
			playNumber = value;
			if (playNumberSpinBox != null)
			{
				playNumberSpinBox.Value = value;
				playNumber = (int)playNumberSpinBox.Value;
			}
		}
	}

	// 1-4 
	[Export]
	public int PlaySlot { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		playImage = GetNode<TextureRectWithMouse>("MarginContainer/VBox/playImageTextureRectWithMouse");
		playNumberSpinBox = GetNode<SpinBox>("MarginContainer/VBox/playIndexSpinBox");
		playNumberSpinBox.ValueChanged += PlayNumberSpinBox_ValueChanged;
		UpdateState ();
	}

	private void PlayNumberSpinBox_ValueChanged(double value)
	{
		playNumber = (int)value;
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
		playImage.Texture = GD.Load<Texture2D>(filePath);
		/* 
		Image image = new Image();
		image.Load(filePath);
		ImageTexture imageTexture = ImageTexture.CreateFromImage(image);
		playImage.Texture = imageTexture;
		*/
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}

public enum PlayType
{
	NONE=0,
	RUN,
	PASS
}