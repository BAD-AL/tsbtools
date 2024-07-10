using Godot;
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using TSBTool;

public partial class EditTeam : Control
{
	private OptionButton m_TeamsComboBox;
	private OptionButton m_FormationComboBox;
	private OptionButton m_OffensivePrefomComboBox;
	private SpinBox m_SimDefenseUpDown;
	private SpinBox m_SimOffenseUpDown;


	private PlayControl P1;
	private PlayControl P2;
	private PlayControl P3;
	private PlayControl P4;

	private PlayControl R1;
	private PlayControl R2;
	private PlayControl R3;
	private PlayControl R4;

	private Button closeButton;

	private Regex m_PlaybookRegex = new Regex("PLAYBOOK (R[1-8]{4})\\s*,\\s*(P[1-8]{4})");
	private Regex m_OffensiveFormationRegex = new Regex("OFFENSIVE_FORMATION\\s*=\\s*([a-zA-Z1234_]+)");

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		m_TeamsComboBox = GetNode<OptionButton>("MarginContainer/mPanel/VBoxContainer/teamOptionButton");
		m_FormationComboBox = GetNode<OptionButton>("MarginContainer/mPanel/VBoxContainer/runPassFormationPanel/HBoxContainer/m_FormationComboBox");
		m_OffensivePrefomComboBox = GetNode<OptionButton>("MarginContainer/mPanel/VBoxContainer/runPassFormationPanel/HBoxContainer/m_OffensivePrefomComboBox");
		m_SimDefenseUpDown = GetNode<SpinBox>("MarginContainer/mPanel/VBoxContainer/simPanel/HBoxContainer/m_SimDefenseUpDown");
		m_SimOffenseUpDown = GetNode<SpinBox>("MarginContainer/mPanel/VBoxContainer/simPanel/HBoxContainer/m_SimOffenseUpDown");
		P1 = GetNode<PlayControl>("MarginContainer/mPanel/VBoxContainer/Panel/HBoxContainer/PlayControlP1");
		P2 = GetNode<PlayControl>("MarginContainer/mPanel/VBoxContainer/Panel/HBoxContainer/PlayControlP2");
		P3 = GetNode<PlayControl>("MarginContainer/mPanel/VBoxContainer/Panel/HBoxContainer/PlayControlP3");
		P4 = GetNode<PlayControl>("MarginContainer/mPanel/VBoxContainer/Panel/HBoxContainer/PlayControlP4");

		R1 = GetNode<PlayControl>("MarginContainer/mPanel/VBoxContainer/Panel2/HBoxContainer/PlayControlR1");
		R2 = GetNode<PlayControl>("MarginContainer/mPanel/VBoxContainer/Panel2/HBoxContainer/PlayControlR2");
		R3 = GetNode<PlayControl>("MarginContainer/mPanel/VBoxContainer/Panel2/HBoxContainer/PlayControlR3");
		R4 = GetNode<PlayControl>("MarginContainer/mPanel/VBoxContainer/Panel2/HBoxContainer/PlayControlR4");
		closeButton = GetNode<Button>("MarginContainer/mPanel/closeButton");

		LoadData();

		m_TeamsComboBox.Connect("item_selected", this, "M_TeamsComboBox_ItemSelected");
		closeButton.Connect("pressed", this, nameof(CloseButton_Pressed));

		m_SimOffenseUpDown.Connect("value_changed", this, nameof(SimValueChanged));
		m_SimDefenseUpDown.Connect("value_changed", this, nameof(SimValueChanged));
		m_OffensivePrefomComboBox.Connect("item_selected", this, "M_OffensivePrefomComboBox_ItemSelected");
		m_FormationComboBox.Connect("item_selected", this, "M_FormationComboBox_ItemSelected");

		R1.Connect("value_changed", this, nameof(OnPlayChanged));
		R2.Connect("value_changed", this, nameof(OnPlayChanged));
		R3.Connect("value_changed", this, nameof(OnPlayChanged));
		R4.Connect("value_changed", this, nameof(OnPlayChanged));

		P1.Connect("value_changed", this, nameof(OnPlayChanged));
		P2.Connect("value_changed", this, nameof(OnPlayChanged));
		P3.Connect("value_changed", this, nameof(OnPlayChanged));
		P4.Connect("value_changed", this, nameof(OnPlayChanged));
	}

	public void OnPlayChanged(int newPlayNumber)
	{
		UpdateData();
	}
	
	private void LoadData()
	{
		int season = 1;
		Data = TecmoHelper.Instance.GetAll(season);
	}

	private string m_Data = "";
	/// <summary>
	/// The data to work on.
	/// </summary>
	public string Data
	{
		get { return m_Data; }

		set
		{
			m_Data = value;
			if (m_Data.IndexOf("TEAM") != -1)
			{
				SetupTeams();
				ShowTeamValues();
			}
		}
	}

	public string CurrentTeam
	{
		get { return this.m_TeamsComboBox.Text; }

		set
		{
			int index = TecmoHelper.GetItemIndex(m_TeamsComboBox, value);
			if (index > -1)
			{
				m_TeamsComboBox.Select(index);
			}
		}
	}

	private void SetupTeams()
	{
		Regex teamRegex = new Regex("TEAM\\s*=\\s*([a-z0-9]+)");
		MatchCollection mc = teamRegex.Matches(m_Data);

		m_TeamsComboBox.Clear();
		foreach (Match m in mc)
		{
			string team = m.Groups[1].Value;
			m_TeamsComboBox.AddItem(team);
		}
		if (m_TeamsComboBox.GetItemCount() > 0)
		{
			m_TeamsComboBox.Select(0);
		}
	}

	/// <summary>
	/// Returns the currently selected offensive formation string.
	/// </summary>
	/// <returns></returns>
	private string GetOffensiveFormation()
	{
		string ret = string.Format("OFFENSIVE_FORMATION = {0}",
			m_FormationComboBox.Text);
		return ret;
	}

	/// <summary>
	/// Gets the current playbook string.
	/// </summary>
	/// <returns></returns>
	private string GetCurrentPlaybook()
	{
		string retVal = string.Format(
			"PLAYBOOK R{0}{1}{2}{3}, P{4}{5}{6}{7}",
			R1.PlayNumber, R2.PlayNumber, R3.PlayNumber, R4.PlayNumber,
			P1.PlayNumber, P2.PlayNumber, P3.PlayNumber, P4.PlayNumber
			);
		return retVal;
	}

	/// <summary>
	/// Shows the data for the current team.
	/// </summary>
	private void ShowTeamValues()
	{
		string team = m_TeamsComboBox.Text;
		string line = GetTeamString(team);
		
		if (line != null)
		{
			int[] vals = InputParser.GetSimData(line);

			if (vals != null && vals[1] > -1 && vals[1] < 4)
				m_OffensivePrefomComboBox.Select(vals[1]);

			byte[] simVals = GetNibbles(vals[0]);
			m_SimOffenseUpDown.Value = simVals[0];
			m_SimDefenseUpDown.Value = simVals[1];

			Match ofMatch = m_OffensiveFormationRegex.Match(line);
			Match pbMatch = m_PlaybookRegex.Match(line);
			if (ofMatch != Match.Empty)
			{
				string val = ofMatch.Groups[1].ToString();
				int index = TecmoHelper.GetItemIndex(m_FormationComboBox, val);
				if (index > -1)
					m_FormationComboBox.Select( index);
			}
			if (pbMatch != Match.Empty)
			{
				string runs = pbMatch.Groups[1].ToString();
				string passes = pbMatch.Groups[2].ToString();
				SetRuns(runs);
				SetPasses(passes);
			}
		}
	}

	private void SetRuns(string runs)
	{
		if (runs != null && runs.Length == 5)
		{
			R1.PlayNumber = Int32.Parse("" + runs[1]);
			R2.PlayNumber = Int32.Parse("" + runs[2]);
			R3.PlayNumber = Int32.Parse("" + runs[3]);
			R4.PlayNumber= Int32.Parse("" + runs[4]);
		}
	}

	private void SetPasses(string passes)
	{
		if (passes != null && passes.Length == 5)
		{
			P1.PlayNumber = Int32.Parse("" + passes[1]);
			P2.PlayNumber = Int32.Parse("" + passes[2]);
			P3.PlayNumber = Int32.Parse("" + passes[3]);
			P4.PlayNumber = Int32.Parse("" + passes[4]);
		}
	}

	/// <summary>
	/// Gets a string like:
	/// "TEAM = bills SimData=0xab0"
	/// that is currently from m_Data.
	/// </summary>
	/// <param name="team"></param>
	/// <returns></returns>
	private string GetTeamString(string team)
	{
		string theTeam = string.Format("TEAM = {0}", team);
		int teamIndex = m_Data.IndexOf(theTeam);
		int newLine = -1;
		string line = null;

		if (teamIndex > -1 && (newLine = m_Data.IndexOf('\n', teamIndex)) > -1)
		{
			line = m_Data.Substring(teamIndex, newLine - teamIndex);
		}
		Match m = m_PlaybookRegex.Match(m_Data, newLine);
		line = line + "\n" + m.Value;
		return line;
	}

	/// <summary>
	/// Gwts a string that represents the values set in the UI.
	/// Like:
	/// "TEAM = bills SimData=0xab0"
	/// </summary>
	/// <returns></returns>
	private string GetCurrentValues()
	{
		
		string ret = string.Format("{0:x}{1:x}{2}",
			(int)m_SimOffenseUpDown.Value,
			(int)m_SimDefenseUpDown.Value,
			m_OffensivePrefomComboBox.GetSelectedId());

		return ret;
	}

	/// <summary>
	/// Gets the text representation of the current UI.
	/// </summary>
	/// <returns></returns>
	private string GetCurrentTeamString()
	{
		string vals = GetCurrentValues();
		string ret = string.Format("TEAM = {0} SimData=0x{1}",
			m_TeamsComboBox.Text,
			vals);
		ret = ret + ", " + GetOffensiveFormation();
		ret = ret + "\n" + GetCurrentPlaybook();

		return ret;
	}

	private void UpdateData()
	{
		string team = m_TeamsComboBox.Text;
		string oldValue = GetTeamString(team);
		string newValue = GetCurrentTeamString();

		m_Data = m_Data.Replace(oldValue, newValue);
	}

	/// <summary>
	/// Returns the associated nibbles for the value passed (assuming it's a byte).
	/// </summary>
	/// <param name="val"></param>
	/// <returns></returns>
	private byte[] GetNibbles(int val)
	{
		byte[] ret = new byte[2];
		byte byteValue = (byte)val;
		ret[1] = (byte)(byteValue & 0x0F); // lo byte
		ret[0] = (byte)(byteValue >> 4);
		return ret;
	}

	// event handlers
	private void M_FormationComboBox_ItemSelected(long index)
	{
		UpdateData();
	}

	private void M_OffensivePrefomComboBox_ItemSelected(long index)
	{
		UpdateData();
	}

	private void SimValueChanged(double value)
	{
		UpdateData();
	}

	private void CloseButton_Pressed()
	{
		UpdateData(); // text operation
		TecmoHelper.Instance.ProcessText(Data);
		var sceneManager = GetNode("/root/SceneManager");
		sceneManager.Call("pop_scene");
	}

	private void M_TeamsComboBox_ItemSelected(long index)
	{
		ShowTeamValues();
	}

}
