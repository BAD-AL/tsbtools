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
		TecmoTool.ShowPlaybook = true;
		TecmoTool.ShowTeamFormation = true;
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
}
/*
	public interface ITecmoContent {
		bool ShowOffPref { get; set; }
		byte[] OutputRom { get; set; }
		ROM_TYPE RomVersion { get; }
		void SaveRom(string fileName);
		string GetKey();
		string GetAll(int season);
		string GetProBowlPlayers(int season);
		string GetSchedule(int season);
		void ProcessText(string text);
		void ApplySet(string line);
		void SetByte(int location, byte b);
	}
*/
