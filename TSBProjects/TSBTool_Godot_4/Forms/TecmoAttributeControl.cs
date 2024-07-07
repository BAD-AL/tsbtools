using Godot;
using System;
using System.Collections.Generic;

public partial class TecmoAttributeControl : Control
{
	// important child controls; null til '_Ready()'
	private Label? attributeLabel = null;
	private OptionButton? valueCombo = null;

	private static List<byte> sOptions = new List<byte>()
		{6, 13, 19, 25, 31, 38, 44, 50, 56, 63, 69, 75, 81, 88, 94, 100 };

	[Export]
	public String AttributeName
	{
		get
		{
			string retVal = "";
			if (attributeLabel != null)
				retVal = attributeLabel.Text;
			else
				Console.WriteLine("TecmoAttributeControl.AttributeName; getting label name before we're ready!");
			return retVal;
		}

		set
		{
			if (attributeLabel != null)
				attributeLabel.Text = value;
			else
				Console.WriteLine("TecmoAttributeControl.AttributeName; Setting label name before we're ready!");
		}
	}

	[Export]
	public byte AttributeValue
	{ 
		get
		{
			byte retVal = 6;
			if(valueCombo != null )
			{
				if (!byte.TryParse(valueCombo.Text, out retVal))
					Console.WriteLine($"TecmoAttributeControl.AttributeValue; parse error for: {valueCombo.Text}");
			}
			return retVal;
		}
		set
		{
			if(valueCombo != null && sOptions.IndexOf(value) != -1)
				valueCombo.Text = value.ToString();
			else
				Console.WriteLine($"TecmoAttributeControl.AttributeValue; Setting value: {value}");
		}
	}

	[Export]
	public int SelectedIndex
	{
		get
		{
			return valueCombo.Selected;
		}
		set
		{
			valueCombo.Selected = value;
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		attributeLabel = FindChild("attrLabel") as Label; // may need to be "*attrLabel", check it
		valueCombo = FindChild("valueCombo") as OptionButton;
	}

	//// Called every frame. 'delta' is the elapsed time since the previous frame.
	//public override void _Process(double delta)
	//{
	//}
}
