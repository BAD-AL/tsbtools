using Godot;
using System;
using System.Text.Json.Nodes;

public partial class FacePickerDialog : ConfirmationDialog
{
	public string SelectedItem = null;

	TextureRectWithMouse faceTextureRect = null;

	[Signal]
	public delegate void ItemSelectedEventHandler(string selectedItem);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		faceTextureRect = GetNode<TextureRectWithMouse>("Panel/VBoxContainer/faceTextureRect");
		faceTextureRect.MouseDown += FaceTextureRect_MouseDown;
		var children = GetChildren();
		foreach(var child in children)
		{
			GD.Print("FacePicker child: " + child.GetClass());
		}
	}

	private void FaceTextureRect_MouseDown(InputEventMouseButton mouseEvent)
	{
		if (mouseEvent.Pressed)
		{
			Vector2 mousePosition = faceTextureRect.GetLocalMousePosition();// mouseEvent.Position;
			string itemName = GetImageName((int)mousePosition.X, (int)mousePosition.Y);

			if (!String.IsNullOrEmpty(itemName))
			{
				SelectedItem = itemName;
				GD.Print($"EmitSignal: {itemName}");
				Hide();
				EmitSignal("ItemSelected", itemName);
				EmitSignal("confirmed");
			}
		}
	}

	private string GetImageName(int x, int y)
	{
		// 14 columns 12 rows
		string retVal = "";
		int col = x / 32;
		int row = y / 32;
		
		int imageNum = row * 14 + col;
		if ( imageNum > 0x52 )
		{
			imageNum += (0x80-0x53);
		}
		retVal = String.Format("{0:X2}",imageNum);
		
		//GD.Print("GetImageName: " + retVal);
		return retVal;
	}

}
