using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
//using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using TSBTool;

public partial class EditPlayer : Control
{
	private bool m_AutoUpdatePlayers = false;
	private StateEnum m_CurrentState = StateEnum.NONE;
	private int m_ImageNumber = 0x00;
	private SimStuff m_SimStuff = new SimStuff();

	#region control references
	Button? closeButton = null;
	Button m_PrevPicture;
	Button m_NextPicture;
	TextureRectWithMouse m_FaceBox;
	Label m_FaceLabel;
	private LineEdit m_FirstNameTextBox;
	private LineEdit m_LastNameTextBox;
	private SpinBox m_JerseyNumberUpDown;
	private TecmoAttributeControl[] m_Attributes = null;
	private SpinBox[] m_SimAttrs = null;

	private TecmoAttributeControl m_RSBox;
	private TecmoAttributeControl m_RPBox;
	private TecmoAttributeControl m_MSBox;
	private TecmoAttributeControl m_HPBox;
	private TecmoAttributeControl m_PC_REC_QU_KABox;
	private TecmoAttributeControl m_PS_BC_PI_KABox;
	private TecmoAttributeControl m_ACCBox;
	private TecmoAttributeControl m_APBBox;

	private SpinBox m_Sim1UpDown;
	private SpinBox m_Sim2UpDown;
	private SpinBox m_Sim3UpDown;
	private SpinBox m_Sim4UpDown;
	private Label m_Sim1Label;
	private Label m_Sim2Label;
	private Label m_Sim3Label;
	private Label m_Sim4Label;

	private OptionButton? m_TeamsComboBox;
	private OptionButton? m_PositionComboBox;
	#endregion


	/// <summary>
	/// Returns the index of 'val'.
	/// </summary>
	/// <param name="val"></param>
	/// <returns></returns>
	private static int AttrIndex(string val)
	{
		int ret = -1;
		string[] attrs = { "6", "13", "19", "25", "31", "38", "44", "50", "56", "63", "69", "75", "81", "88", "94", "100" };
		for (int i = 0; i < attrs.Length; i++)
		{
			if (attrs[i] == val)
			{
				ret = i;
				break;
			}
		}
		return ret;
	}

	private AtlasTexture faceAtlasTexture;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var faceTexture = GD.Load<Texture2D>("res://Images/faces.png");
		faceAtlasTexture = new AtlasTexture { Atlas = faceTexture, Region = new Rect2(0,0,32,32) }; // initialize on first face

		closeButton = GetNode<Button>("marginContainer/pPanel/bottomPanel/closeButton");
		
		m_PrevPicture = GetNode<Button>("marginContainer/pPanel/hBoxContainer/leftPanel/leftVBoxContainer/facePanel/HBoxContainer/m_PrevPicture");
		m_NextPicture = GetNode<Button>("marginContainer/pPanel/hBoxContainer/leftPanel/leftVBoxContainer/facePanel/HBoxContainer/m_NextPicture");
		m_FaceBox = GetNode<TextureRectWithMouse>("marginContainer/pPanel/hBoxContainer/leftPanel/leftVBoxContainer/facePanel/HBoxContainer/m_FaceBox");
		m_FaceBox.Texture = faceAtlasTexture;
		//m_FaceLabel = GetNode<Label>("marginContainer/pPanel/hBoxContainer/leftPanel/leftVBoxContainer/facePanel/HBoxContainer/spacerPanel2/m_FaceLabel");
		m_FaceLabel = FindChild("m_FaceLabel") as Label;

		var dude =  GetNode("marginContainer/pPanel/hBoxContainer/leftPanel/leftVBoxContainer/m_RSBox");
		var box = dude as TecmoAttributeControl;
		m_RSBox = GetNode<TecmoAttributeControl>("marginContainer/pPanel/hBoxContainer/leftPanel/leftVBoxContainer/m_RSBox");
		m_RPBox = GetNode<TecmoAttributeControl>("marginContainer/pPanel/hBoxContainer/rightPanel/rightVBoxContainer/m_RPBox");
		m_MSBox = GetNode<TecmoAttributeControl>("marginContainer/pPanel/hBoxContainer/leftPanel/leftVBoxContainer/m_MSBox");
		m_HPBox = GetNode<TecmoAttributeControl>("marginContainer/pPanel/hBoxContainer/rightPanel/rightVBoxContainer/m_HPBox");
		m_PC_REC_QU_KABox = GetNode<TecmoAttributeControl>("marginContainer/pPanel/hBoxContainer/rightPanel/rightVBoxContainer/m_PC_REC_QU_KABox");
		m_PS_BC_PI_KABox = GetNode<TecmoAttributeControl>("marginContainer/pPanel/hBoxContainer/leftPanel/leftVBoxContainer/m_PS_BC_PI_KABox");
		m_ACCBox = GetNode<TecmoAttributeControl>("marginContainer/pPanel/hBoxContainer/leftPanel/leftVBoxContainer/m_ACCBox");
		m_APBBox = GetNode< TecmoAttributeControl>("marginContainer/pPanel/hBoxContainer/rightPanel/rightVBoxContainer/m_APBBox");
		m_Attributes = new TecmoAttributeControl[8];
		m_Attributes[0] = m_RSBox;
		m_Attributes[1] = m_RPBox;
		m_Attributes[2] = m_MSBox;
		m_Attributes[3] = m_HPBox;
		m_Attributes[4] = m_PS_BC_PI_KABox;
		m_Attributes[5] = m_PC_REC_QU_KABox;
		m_Attributes[6] = m_ACCBox;
		m_Attributes[7] = m_APBBox;

		m_Sim1UpDown = GetNode<SpinBox>("marginContainer/pPanel/hBoxContainer/leftPanel/leftVBoxContainer/Panel1/m_Sim1UpDown");
		m_Sim2UpDown = GetNode<SpinBox>("marginContainer/pPanel/hBoxContainer/rightPanel/rightVBoxContainer/Panel2/m_Sim2UpDown");
		m_Sim3UpDown = GetNode<SpinBox>("marginContainer/pPanel/hBoxContainer/leftPanel/leftVBoxContainer/Panel3/m_Sim3UpDown");
		m_Sim4UpDown = GetNode<SpinBox>("marginContainer/pPanel/hBoxContainer/rightPanel/rightVBoxContainer/Panel4/m_Sim4UpDown");
		m_SimAttrs = new SpinBox[4];
		m_SimAttrs[0] = m_Sim1UpDown;
		m_SimAttrs[1] = m_Sim2UpDown;
		m_SimAttrs[2] = m_Sim3UpDown;
		m_SimAttrs[3] = m_Sim4UpDown;

		m_Sim1Label = FindChild("m_Sim1Label") as Label;
		m_Sim2Label = FindChild("m_Sim2Label") as Label;
		m_Sim3Label = FindChild("m_Sim3Label") as Label;
		m_Sim4Label = FindChild("m_Sim4Label") as Label;

		m_TeamsComboBox = GetNode<OptionButton>("marginContainer/pPanel/hBoxContainer/leftPanel/leftVBoxContainer/m_TeamsComboBox");
		m_PositionComboBox = GetNode<OptionButton>("marginContainer/pPanel/hBoxContainer/rightPanel/rightVBoxContainer/m_PositionComboBox");

		m_FirstNameTextBox = GetNode<LineEdit>("marginContainer/pPanel/hBoxContainer/leftPanel/leftVBoxContainer/m_FirstNameTextBox");
		m_LastNameTextBox = GetNode<LineEdit>("marginContainer/pPanel/hBoxContainer/rightPanel/rightVBoxContainer/m_LastNameTextBox");
		m_JerseyNumberUpDown = GetNode<SpinBox>("marginContainer/pPanel/hBoxContainer/rightPanel/rightVBoxContainer/jerseyPanel/m_JerseyNumberDropUpDown");

		m_PositionComboBox.Selected = 0;
		m_TeamsComboBox.Selected = 0;

		string[] positions = new string[] {
					"QB1","QB2","RB1","RB2","RB3","RB4","WR1","WR2","WR3","WR4","TE1","TE2",
					"C","LG","RG","LT","RT",
					"RE","NT","LE","ROLB","RILB","LILB","LOLB","RCB","LCB","FS","SS","K","P"
					};
		foreach (string position in positions )
			m_PositionComboBox.AddItem(position);

		// signals hook up
		m_PrevPicture.Connect(Button.SignalName.Pressed, Callable.From(OnPevPicturePressed));
		m_NextPicture.Connect(Button.SignalName.Pressed, Callable.From(OnNextPicturePressed));
		closeButton.Connect(Button.SignalName.Pressed, Callable.From(OnCloseButtonPressed));
		//m_PositionComboBox.Connect(OptionButton.SignalName.ItemSelected, Callable.From(m_PositionComboBox_SelectedIndexChanged));
		//m_TeamsComboBox.Connect(OptionButton.SignalName.ItemSelected, Callable.From(m_TeamsComboBox_SelectedIndexChanged));
		m_FirstNameTextBox.Connect(LineEdit.SignalName.FocusExited, Callable.From(m_FirstNameTextBox_Leave));
		m_LastNameTextBox.Connect(LineEdit.SignalName.FocusExited, Callable.From(m_LastNameTextBox_Leave));
		m_FaceBox.MouseDown += M_FaceBox_MouseDown;
		//m_FaceBox.GuiInput += M_FaceBox_GuiInput;

		

		// Connect the button pressed signal to a method using an anonymous function (lambda)
		//button.Connect("pressed", this, nameof(OnButtonPressed), new Godot.Collections.Array { button.Name });
		m_RSBox.AttributeName = "RS";
		m_RPBox.AttributeName = "RP";
		m_MSBox.AttributeName = "MS";
		m_HPBox.AttributeName = "HP";
		//m_PC_REC_QU_KABox.AttributeName = "PC";
		//m_PS_BC_PI_KABox.AttributeName = "PS";
		//m_ACCBox.AttributeName = "ACC";
		//m_APBBox.AttributeName = "APB";

		LoadData();
		CurrentState = StateEnum.QB;
		AddListeners();
	}

	private void AddListeners()
	{
		foreach(TecmoAttributeControl control in m_Attributes)
		{
			control.value_changed += attributel_value_changed;
		}
		foreach(SpinBox sb in m_SimAttrs)
		{
			sb.ValueChanged += attributel_value_changed;
		}
	}


	private void attributel_value_changed(double value)
	{
		ReplacePlayer();
	}

	private void RemoveListeners()
	{
		foreach (TecmoAttributeControl control in m_Attributes)
		{
			control.value_changed -= attributel_value_changed;
		}
		foreach (SpinBox sb in m_SimAttrs)
		{
			sb.ValueChanged -= attributel_value_changed;
		}
	}

	private void M_FaceBox_MouseDown(InputEventMouseButton mouseEvent)
	{
		if (mouseEvent != null && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left )
		{
			ShowFacePicker();
		}
	}

	FacePickerDialog? facePickerDialog = null;
	private void ShowFacePicker()
	{
		//facePickerDialog = new FacePickerDialog();
		PackedScene facePickerScene = (PackedScene)ResourceLoader.Load("res://Forms/FacePickerDialog.tscn");
		facePickerDialog = (FacePickerDialog)facePickerScene.Instantiate();

		facePickerDialog.Size = new Vector2I((int)this.Size.X-30, (int)this.Size.Y-30);
		facePickerDialog.Confirmed += facePicker_Closed;
		facePickerDialog.Canceled += facePicker_Closed;
		AddChild(facePickerDialog);
		facePickerDialog.PopupCentered();
	}

	private void facePicker_Closed()
	{
		// "02.BMP":{ "x":64, "y":0, "height":32, "width":32},
		if (facePickerDialog.SelectedItem != null)
		{
			string selectedItem = facePickerDialog.SelectedItem.Replace(".BMP","");
			if (Int32.TryParse(selectedItem, System.Globalization.NumberStyles.AllowHexSpecifier, null, out m_ImageNumber))
			{
				ShowCurrentFace();
			}
			else
			{
				GD.Print($"Error parsing '{selectedItem}' to Int32");
			}
		}
		
		facePickerDialog.QueueFree();
		facePickerDialog = null;
	}

	private void LoadData()
	{
		int season = 1;
		Data = TecmoHelper.Instance.GetAll(season);
	}

	private void OnPevPicturePressed()
	{
		if (m_ImageNumber == 0x00)
			m_ImageNumber = 0xD4;
		else if (m_ImageNumber == 0x80)
			m_ImageNumber = 0x52;
		else
			m_ImageNumber--;
		ShowCurrentFace();
	}
	private void OnNextPicturePressed()
	{
		if (m_ImageNumber == 0xD4)
			m_ImageNumber = 0;
		else if (m_ImageNumber == 0x52)
			m_ImageNumber = 0x80;
		else
			m_ImageNumber++;
		ShowCurrentFace();
	}
	private void OnCloseButtonPressed()
	{
		ReplacePlayer();// save the current player stuff
		TecmoHelper.Instance.ProcessText(Data);
		var sceneManager = GetNode("/root/SceneManager");
		sceneManager.Call("pop_scene");
	}

	private void m_LastNameTextBox_Leave()
	{
		m_LastNameTextBox.Text = m_LastNameTextBox.Text.ToUpper();
	}

	private void m_FirstNameTextBox_Leave()
	{
		m_FirstNameTextBox.Text = m_FirstNameTextBox.Text.ToLower();
	}


	private void AutoUpdateRoster()
	{
		//if(m_DoneInit)
			if ( m_AutoUpdatePlayers)
			{
				ReplacePlayer();
			}
	}

	/// <summary>
	/// Replaces the current player with the values specified.
	/// </summary>
	private void ReplacePlayer()
	{
		GD.Print("EditPlayer.ReplacePlayer()");
		string oldPlayer = GetPlayerString(m_TeamsComboBox.Text,
			m_PositionComboBox.Text);
		if (oldPlayer == null)
			return;

		string newPlayer = GetPlayerString_UI();
		string team = m_TeamsComboBox.GetItemText(m_TeamsComboBox.Selected);
		ReplacePlayer(team, oldPlayer, newPlayer);
	}


	private void ReplacePlayer(string team, string oldPlayer, string newPlayer)
	{
		int nextTeamIndex = -1;
		int currentTeamIndex = -1;
		string nextTeam = null;

		Regex findTeamRegex = new Regex("TEAM\\s*=\\s*" + team);

		Match m = findTeamRegex.Match(m_Data);
		if (!m.Success)
			return;

		currentTeamIndex = m.Groups[1].Index;

		//int test = m_TeamsComboBox.Items.IndexOf(team);
		int test = TecmoHelper.GetItemIndex(m_TeamsComboBox, team);

		if (test != m_TeamsComboBox.ItemCount - 1)
		{
			nextTeam = string.Format("TEAM\\s*=\\s*{0}", m_TeamsComboBox.GetItemText(test +1));
			Regex nextTeamRegex = new Regex(nextTeam);
			Match nt = nextTeamRegex.Match(m_Data);
			if (nt.Success)
				nextTeamIndex = nt.Index;
			//nextTeamIndex = m_Data.IndexOf(nextTeam);
		}
		if (nextTeamIndex < 0)
			nextTeamIndex = m_Data.Length;


		int playerIndex = m_Data.IndexOf(oldPlayer, currentTeamIndex);
		if (playerIndex > -1)
		{
			int endLine = m_Data.IndexOf('\n', playerIndex);
			string start = m_Data.Substring(0, playerIndex);
			string last = m_Data.Substring(endLine);

			StringBuilder tmp = new StringBuilder(m_Data.Length + 200);
			tmp.Append(start);
			tmp.Append(newPlayer);
			tmp.Append(last);

			m_Data = tmp.ToString();
			//m_Data = start + newPlayer + last;
		}
		else
		{
			string error = string.Format(
@"An error occured looking up player
     '{0}'
Please verify that this player's attributes are correct.", oldPlayer);
			//MessageBox.Show(error); // TODO: FIXME
		}
	}

	/// <summary>
	/// Will update the current player when the index changes.
	/// </summary>
	private void _on_m_teams_combo_box_item_selected(int newIndex)
	{
		RemoveListeners();
		//if (m_DoneInit)
		{
			// do a replace player on Auto update HERE!
			SetCurrentPlayer();
		}
		AddListeners();
	}

	/// <summary>
	/// Sets the current state based on the position selected.
	/// </summary>
	private void _on_m_position_combo_box_item_selected(int newIndex)
	{
		RemoveListeners();
		//if (m_DoneInit)
		{
			TSBPlayer pos = (TSBPlayer)m_PositionComboBox.Selected;
			switch (pos)
			{
				case TSBPlayer.QB1:
				case TSBPlayer.QB2:
					CurrentState = StateEnum.QB;
					break;
				case TSBPlayer.RB1:
				case TSBPlayer.RB2:
				case TSBPlayer.RB3:
				case TSBPlayer.RB4:
				case TSBPlayer.WR1:
				case TSBPlayer.WR2:
				case TSBPlayer.WR3:
				case TSBPlayer.WR4:
				case TSBPlayer.TE1:
				case TSBPlayer.TE2:
					CurrentState = StateEnum.SKILL;
					break;
				case TSBPlayer.RT:
				case TSBPlayer.RG:
				case TSBPlayer.C:
				case TSBPlayer.LG:
				case TSBPlayer.LT:
					CurrentState = StateEnum.OLINE;
					break;
				case TSBPlayer.RE:
				case TSBPlayer.LE:
				case TSBPlayer.NT:
				case TSBPlayer.ROLB:
				case TSBPlayer.RILB:
				case TSBPlayer.LILB:
				case TSBPlayer.LOLB:
				case TSBPlayer.RCB:
				case TSBPlayer.LCB:
				case TSBPlayer.SS:
				case TSBPlayer.FS:
					CurrentState = StateEnum.DEFENSE;
					break;
				case TSBPlayer.P:
				case TSBPlayer.K:
					CurrentState = StateEnum.KICKER;
					break;
			}
			SetCurrentPlayer();
		}
		AddListeners();
	}

	/// <summary>
	/// Sets the current state. 
	/// Will enable, disable and modify the contents of the GUI.
	/// </summary>
	public StateEnum CurrentState
	{
		get { return m_CurrentState; }

		set
		{
			//m_DoneInit = false;
			switch (value)
			{
				case StateEnum.QB:
					m_PS_BC_PI_KABox.AttributeName = "PS";  // m_A1Label.Text = "PS";
					m_PC_REC_QU_KABox.AttributeName = "PC"; // m_A2Label.Text = "PC";
					m_ACCBox.AttributeName = "ACC";         // m_A3Label.Text = "ACC";
					m_APBBox.AttributeName = "APB";         // m_A4Label.Text = "APB";

					m_Sim1Label.Text = "Sim Run";
					m_Sim2Label.Text = "Sim Pass";
					m_Sim3Label.Text = "Sim Pocket";
					m_Sim4Label.Text = "";
					
					m_Sim1UpDown.Visible = true;
					m_Sim2UpDown.Visible = true;
					m_Sim3UpDown.Visible = true;
					m_Sim4UpDown.Visible = false;
					m_Sim1UpDown.MaxValue = 15;
					m_Sim2UpDown.MaxValue = 15;
					m_Sim3UpDown.MaxValue = 15;
					m_Sim4UpDown.MaxValue = 15;
					m_PS_BC_PI_KABox .Visible = true;
					m_PC_REC_QU_KABox.Visible = true;
					m_ACCBox.Visible = true;
					m_APBBox.Visible = true;
					break;
				case StateEnum.SKILL:
					m_PS_BC_PI_KABox.AttributeName = "BC";   // m_A1Label.Text = "BC";
					m_PC_REC_QU_KABox.AttributeName = "REC"; // m_A2Label.Text = "REC";
					m_ACCBox.AttributeName = "";             // m_A3Label.Text = "";
					m_APBBox.AttributeName = "";             // m_A4Label.Text = "";
					m_Sim1Label.Text = "Sim Rush";
					m_Sim2Label.Text = "Sim Catch";
					m_Sim3Label.Text = "Sim Yards per catch";
					m_Sim4Label.Text = "Sim Targets";
					m_Sim1UpDown.Visible = true;
					m_Sim2UpDown.Visible = true;
					m_Sim3UpDown.Visible = true;
					m_Sim4UpDown.Visible = true;
					m_Sim1UpDown.MaxValue = 15;
					m_Sim2UpDown.MaxValue = 15;
					m_Sim3UpDown.MaxValue = 15;
					m_Sim4UpDown.MaxValue = 15;
					m_PS_BC_PI_KABox .Visible = true;
					m_PC_REC_QU_KABox.Visible = true;
					m_ACCBox.Visible = false;
					m_APBBox.Visible = false;
					break;
				case StateEnum.OLINE:
					m_PS_BC_PI_KABox.AttributeName = "";  //  m_A1Label.Text = "";
					m_PC_REC_QU_KABox.AttributeName = ""; //  m_A2Label.Text = "";
					m_ACCBox.AttributeName = "";          //  m_A3Label.Text = "";
					m_APBBox.AttributeName = "";          // m_A4Label.Text = "";
					m_Sim1Label.Text = "";
					m_Sim2Label.Text = "";
					m_Sim3Label.Text = "";
					m_Sim4Label.Text = "";
					m_Sim1UpDown.Visible = false;
					m_Sim2UpDown.Visible = false;
					m_Sim3UpDown.Visible = false;
					m_Sim4UpDown.Visible = false;
					m_PS_BC_PI_KABox. Visible = false;
					m_PC_REC_QU_KABox.Visible = false;
					m_ACCBox.Visible = false;
					m_APBBox.Visible = false;
					break;
				case StateEnum.DEFENSE:
					m_PS_BC_PI_KABox.AttributeName = "PI"; //  m_A1Label.Text = "PI";
					m_PC_REC_QU_KABox.AttributeName = "QU"; //  m_A2Label.Text = "QU";
					m_ACCBox.AttributeName = ""; //  m_A3Label.Text = "";
					m_APBBox.AttributeName = ""; // m_A4Label.Text = "";
					m_Sim1Label.Text = "Sim Pass rush";
					m_Sim2Label.Text = "Sim Coverage";
					m_Sim3Label.Text = "";
					m_Sim4Label.Text = "";
					m_Sim1UpDown.Visible = true;
					m_Sim2UpDown.Visible = true;
					m_Sim3UpDown.Visible = false;
					m_Sim4UpDown.Visible = false;
					m_Sim1UpDown.MaxValue = 255;
					m_Sim2UpDown.MaxValue = 255;
					m_Sim3UpDown.MaxValue = 255;
					m_Sim4UpDown.MaxValue = 255;
					m_PS_BC_PI_KABox. Visible = true;
					m_PC_REC_QU_KABox.Visible = true;
					m_ACCBox.Visible = false;
					m_APBBox.Visible = false;
					break;
				case StateEnum.KICKER:
					m_PS_BC_PI_KABox.AttributeName = "KA";   //   m_A1Label.Text = "KA";
					m_PC_REC_QU_KABox.AttributeName = "AKB"; //   m_A2Label.Text = "AKB";
					m_ACCBox.AttributeName = "";             //   m_A3Label.Text = "";
					m_APBBox.AttributeName = "";             // m_A4Label.Text = "";
					m_Sim1Label.Text = "Sim KA";
					m_Sim2Label.Text = "";
					m_Sim3Label.Text = "";
					m_Sim4Label.Text = "";

					m_Sim1UpDown.Call("Disabled", true);
					m_Sim1UpDown.Visible = true;
					m_Sim2UpDown.Visible = false;
					m_Sim3UpDown.Visible = false;
					m_Sim4UpDown.Visible = false;
					m_Sim1UpDown.MaxValue = 15;
					m_Sim2UpDown.MaxValue = 15;
					m_Sim3UpDown.MaxValue = 15;
					m_Sim4UpDown.MaxValue = 15;
					m_PS_BC_PI_KABox. Visible = true;
					m_PC_REC_QU_KABox.Visible = true;
					m_ACCBox.Visible = false;
					m_APBBox.Visible = false;
					break;
			}
			m_CurrentState = value;
			//m_DoneInit = true;
		}
	}

	private string m_Data = "";
	/// <summary>
	/// The text data to work on and retrieve.
	/// </summary>
	public string Data
	{
		get { return m_Data; }

		set
		{
			m_Data = value;
			if (m_Data != null && m_Data.Length > 0)
			{
				SetupTeams();
				SetCurrentPlayer();
			}
		}
	}


	/// <summary>
	/// Get and set the current position for the Form.
	/// </summary>
	public string CurrentPosition
	{
		get { return this.m_PositionComboBox.Text; }

		set
		{
			int index = TecmoHelper.GetItemIndex(m_PositionComboBox, value);
			if (index > -1)
				m_PositionComboBox.Selected = index;
		}
	}

	/// <summary>
	/// Get and set the current team.
	/// </summary>
	public string CurrentTeam
	{
		get { return this.m_TeamsComboBox.Text; }

		set
		{
			int index = TecmoHelper.GetItemIndex(m_TeamsComboBox, value);
			if (index > -1)
				m_TeamsComboBox.Selected = index;
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
		if (m_TeamsComboBox.ItemCount > 0)
		{
			m_TeamsComboBox.Select(0);
		}
	}


	/// <summary>
	/// Returns the text representation of what the GUI is presenting.
	/// </summary>
	/// <returns></returns>
	private string GetPlayerString_UI()
	{
		string ret = GetPlayerString_UI(m_PositionComboBox.Text);
		return ret;
	}


	/// <summary>
	/// Returns the text representation of what the GUI is presenting.
	/// </summary>
	/// <returns></returns>
	public string GetPlayerString_UI(string position)
	{
		StringBuilder sb = new StringBuilder(60);
		sb.Append(position);
		sb.Append(", ");
		sb.Append(m_FirstNameTextBox.Text);
		sb.Append(" ");
		sb.Append(m_LastNameTextBox.Text);
		sb.Append(", ");
		sb.Append(string.Format("Face=0x{0:x}, #{1}, ",
			m_ImageNumber, m_JerseyNumberUpDown.Value));
		// attrs
		for (int i = 0; i < m_Attributes.Length; i++)
		{
			
			if (m_Attributes[i].Visible)
			{
				sb.Append(m_Attributes[i].AttributeValue.ToString());
				sb.Append(", ");
			}
		}
		//sim attrs
		if (m_SimAttrs[0].Visible)
			sb.Append("[");
		for (int i = 0; i < m_SimAttrs.Length; i++)
		{
			if (m_SimAttrs[i].Visible)
			{
				sb.Append(m_SimAttrs[i].Value);
				sb.Append(", ");
			}
		}
		if (m_SimAttrs[0].Visible)
		{
			sb.Remove(sb.Length - 2, 2);
			sb.Append(" ]");
		}

		string ret = sb.ToString();
		char[] chars = { ' ', ',' };
		ret = ret.Trim(chars);
		return ret;
	}


	/// </summary>
	/// <param name="team"></param>
	/// <param name="position"></param>
	/// <returns></returns>
	private string GetPlayerString(string team, string position)
	{
		string pattern = "TEAM\\s*=\\s*" + team;
		Regex findTeamRegex = new Regex(pattern);
		Match m = findTeamRegex.Match(m_Data);
		if (m != Match.Empty)
		{
			int teamIndex = m.Index;
			//int teamIndex = m_Data.IndexOf("TEAM = "+team);
			if (teamIndex == -1)
				return null;
			//int playerIndex = m_Data.IndexOf("\n"+position+",", teamIndex);
			int playerIndex = -1;
			Regex endLineRegex = new Regex(string.Format("\n\\s*{0}\\s*,", position));
			Match eol = endLineRegex.Match(m_Data, teamIndex);
			if (eol != Match.Empty)
				playerIndex = eol.Index;
			playerIndex++;

			if (playerIndex == 0)
				return null;
			int lineEnd = m_Data.IndexOf("\n", playerIndex);
			string playerLine = m_Data.Substring(playerIndex, lineEnd - playerIndex);
			return playerLine;
		}
		return null;
	}

	/// <summary>
	/// Updates the GUI with the current player.
	/// </summary>
	private void SetCurrentPlayer()
	{
		if (m_TeamsComboBox.Selected > -1)
		{
			//string team = m_TeamsComboBox.SelectedItem.ToString();
			string team = m_TeamsComboBox.Text;
			//string position = m_PositionComboBox.SelectedItem.ToString();
			string position = m_PositionComboBox.Text;
			string playerData = GetPlayerString(team, position);
			if (playerData != null)
				SetPlayerData(playerData);
		}
	}



	/// <summary>
	/// Updates the GUI with the player 'line' passed.
	/// </summary>
	/// <param name="playerLine"></param>
	private void SetPlayerData(string playerLine)
	{
		string fName = InputParser.GetFirstName(playerLine);
		string lName = InputParser.GetLastName(playerLine);
		int face = InputParser.GetFace(playerLine);
		int jerseyNumber = InputParser.GetJerseyNumber(playerLine);
		int[] attrs = InputParser.GetInts(playerLine);
		int[] simData = InputParser.GetSimVals(playerLine);

		m_FirstNameTextBox.Text = fName;
		m_LastNameTextBox.Text = lName;
		m_ImageNumber = face;
		if (jerseyNumber > -1 && jerseyNumber < 0x100)
			m_JerseyNumberUpDown.Value = Int32.Parse(string.Format("{0:x}", jerseyNumber));

		if (attrs != null)
		{
			int attrIndex = 0;
			for (int i = 0; i < attrs.Length && i < m_Attributes.Length; i++)
			{
				attrIndex = AttrIndex(attrs[i].ToString());
				if (attrIndex > -1)
					m_Attributes[i].SelectedIndex = attrIndex;
			}
		}
		if (simData != null)
		{
			for (int i = 0; i < simData.Length; i++)
			{
				m_SimAttrs[i].Value = Int32.Parse(simData[i].ToString());
			}
		}
		if (jerseyNumber > -1 && jerseyNumber < 0x100)
		{
			m_JerseyNumberUpDown.Value = Int32.Parse(string.Format("{0:x}", jerseyNumber));
			ShowCurrentFace();
		}
	}

	private JsonNode? _imageData;
	/// <summary>
	/// Gets the proper face for a given player, and updates the displaying image.
	/// </summary>
	private void ShowCurrentFace()
	{
		if(_imageData == null)
		{
			var data = FileAccess.GetFileAsString("res://Images/faces.png.json");
			_imageData = JsonNode.Parse(data);
		}
		int x=0, y=0, height=0, width=0;
		string file = string.Format("{0:x2}.BMP", m_ImageNumber).ToUpper();
		if(_imageData != null && _imageData[file] != null) 
		{
			x = ((int)_imageData[file]["x"]);
			y = ((int)_imageData[file]["y"]);
			height = ((int)_imageData[file]["height"]);
			width = ((int)_imageData[file]["width"]);
			faceAtlasTexture.Region = new Rect2(x, y, width, height);
			m_FaceLabel.Text = file.Replace(".BMP", "");
		}
		//AutoUpdateRoster(); // what's this do?
	}

}

public enum StateEnum
{
	QB = 0,
	SKILL,
	OLINE,
	DEFENSE,
	KICKER,
	NONE
}
