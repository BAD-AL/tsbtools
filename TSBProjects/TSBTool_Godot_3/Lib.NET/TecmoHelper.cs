using Godot;
using System;
using System.Text;
using TSBTool;

//https://github.com/godotengine/godot-csharp-visualstudio

//GDScript can only interact with objects extending Godot types

// To DEBUG Godot exe, export windows exe; set debug target to the exe
public partial class TecmoHelper : Node
{
	public static TecmoHelper Instance { get; private set; }

	public TecmoHelper()
	{
		if( Instance == null)
			Instance = this;
		if (StaticUtils.sMessageGiver == null)
			StaticUtils.sMessageGiver = new Godot3MessageGiver();
		TecmoTool.ShowPlaybook = true;
		TecmoTool.ShowTeamFormation = true;
	}

	private ITecmoContent tool = null;
	
	public void LoadRom(string path)
	{
		tool = TecmoToolFactory.GetToolForRom(StaticUtils.ReadRom( path));
		if(tool != null)
			GD.Print("TecmoHelper: got valid tool");
		else
			GD.Print("TecmoHelper: got null tool from 'TecmoToolFactory'");
		tool.ShowOffPref = true;
	}

	public void LoadRomBytesFromBase64String(string base64String)
	{
		GD.Print($"LoadRomBytesFromBase64String: len = {base64String.Length}");
		byte[] bytes = Convert.FromBase64String(base64String);
		tool = TecmoToolFactory.GetToolForRom(bytes);
		tool.ShowOffPref = true;
	}

	public string GetBase64RomString()
	{
		string retVal = Convert.ToBase64String(tool.OutputRom);
		return retVal;
	}

	public string GetAll(int season)
	{
		StringBuilder sb = new StringBuilder();
		sb.Append(tool.GetKey());
		sb.Append(tool.GetAll(season));
		sb.Append(tool.GetSchedule(season));
		return sb.ToString();
	}
	public string GetKey()						{ return tool.GetKey(); }
	public string GetProBowlPlayers(int season)	{ return tool.GetProBowlPlayers(season); }
	public void   ProcessText(string text)		{ tool.ProcessText(text); }
	public void   SaveRom(string path)          { tool.SaveRom(path); }
	public string GetRomVersion()				{ return tool.RomVersion.ToString(); }
	
	public static int GetItemIndex(OptionButton optionButton, string targetText)
	{
		for (int i = 0; i < optionButton.GetItemCount(); i++)
		{
			if (optionButton.GetItemText(i) == targetText)
			{
				return i;
			}
		}
		return -1; // Return -1 if the item is not found
	}

	public string GetMessageFromCSharp()
	{
		return "This message is from the C# side";
	}
}


public class Godot3MessageGiver : Node , MessageGiver
{
	public void LogMessage(string message)
	{
		GD.Print(message);
	}

	public string PromptForSetUserInput(string input)
	{
		GD.Print("PromptForSetUserInput not supported in this Version of TSBTool");
		return null;
	}

	public bool ShowConfirmationDialog(string title, string message)
	{
		// Easy to implement in Winforms, trickier with the callback stuff
		GD.Print("\nAutoContinue = true");
		GD.Print(message);
		return true;
	}

	AcceptDialog messageBox = null;
	public void ShowError(string title, string message)
	{
		messageBox = new AcceptDialog();
		messageBox.Name = "MessageBox";
		messageBox.DialogText = message;
		messageBox.WindowTitle = title;
		//messageBox.PopupExclusive = true;

		// Add the AcceptDialog to the scene tree
		GetTree().Root.AddChild(messageBox);

		// Show the AcceptDialog
		messageBox.PopupCentered();

		// Optional: Connect the confirmed signal to a method if you want to handle the OK button press
		messageBox.Connect("confirmed", this, nameof(OnMessageBoxConfirmed));
		messageBox.Connect("popup_hide", this, nameof(OnMessageBoxConfirmed));
	}

	

	public void ShowMessageBox(string title, string message)
	{
		// Create the AcceptDialog
		messageBox = new AcceptDialog();
		messageBox.Name = "MessageBox";
		messageBox.DialogText = message;
		messageBox.WindowTitle = title;
		//messageBox.PopupExclusive = true;

		// Add the AcceptDialog to the scene tree
		GetTree().Root.AddChild(messageBox);

		// Show the AcceptDialog
		messageBox.PopupCentered();

		// Optional: Connect the confirmed signal to a method if you want to handle the OK button press
		messageBox.Connect("confirmed", this, nameof(OnMessageBoxConfirmed));
		messageBox.Connect("popup_hide", this, nameof(OnMessageBoxConfirmed));
	}

	private void OnMessageBoxConfirmed()
	{
		// Handle the OK button press if needed
		GD.Print("Message box confirmed!");

		// Optionally, you can remove the message box from the scene tree after it's confirmed
		if (messageBox != null)
		{
			messageBox.QueueFree();
			messageBox = null;
		}
	}
}