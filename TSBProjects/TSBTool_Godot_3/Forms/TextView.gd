extends Panel

var textBox:TextEdit
var base_font_size = 14
var dynamic_font : DynamicFont = null

# Called when the node enters the scene tree for the first time.
func _ready():
	textBox = $VBoxContainer/textEdit
	dynamic_font = DynamicFont.new()
	dynamic_font.font_data = load("res://Fonts/LiberationMono-Regular.ttf")
	dynamic_font.size = base_font_size
	textBox.add_font_override("font", dynamic_font)
	var extraMessage = "# OS.get_name() = " + OS.get_name() + "\n"
	
	addSyntaxHighlighting(textBox)
	textBox.text = extraMessage
	textBox.connect("gui_input", self, "_on_control_gui_input")
	if Globals.tecmoHelper != null:
		textBox.text += Globals.tecmoHelper.GetAll(1)

func addSyntaxHighlighting(tb:TextEdit):
	tb.add_color_region("# ", "",  Color.gray, true)
	tb.add_keyword_color("TEAM", Color.chocolate)
	tb.add_keyword_color("SimData", Color.chocolate)
	tb.add_keyword_color("OFFENSIVE_FORMATION", Color.chocolate)
	tb.add_keyword_color("PLAYBOOK", Color.chocolate)
	tb.add_keyword_color("2RB_2WR_1TE", Color.lightblue)
	tb.add_keyword_color("1RB_4WR", Color.lightblue)
	tb.add_keyword_color("1RB_3WR_1TE", Color.lightblue)
	tb.add_keyword_color("WEEK", Color.lightblue)
	tb.add_keyword_color("49ers",Color.white)
	

func _on_close_button_pressed():
	print("closeButton pressed")
	if Globals.tecmoHelper != null:
		Globals.tecmoHelper.ProcessText(textBox.text)
	SceneManager.pop_scene()


func _on_prow_bowl_check_box_toggled(toggled_on: bool) -> void:
	if Globals.tecmoHelper == null:
		return
	var text:String = Globals.tecmoHelper.GetAll(1)
	if(toggled_on):
		text += Globals.tecmoHelper.GetProBowlPlayers(1)
	textBox.text = text

func _input(event):
	if event is InputEventMouseButton and Input.is_key_pressed(KEY_CONTROL):
		if event.button_index == BUTTON_WHEEL_UP:
			adjust_font_size(0.25)
		elif event.button_index == BUTTON_WHEEL_DOWN:
			adjust_font_size(-0.25)

func adjust_font_size(delta):
	base_font_size += delta
	if base_font_size < 1:
		base_font_size = 1
	dynamic_font.size = base_font_size
	print("adjust_font_size: " + str(base_font_size))
	textBox.add_font_override("font", dynamic_font)

## Android HTML5 backspace no-worky-work-around
func _on_control_gui_input(event):
	var ke := (event as InputEventKey)
	if ke != null and ke.pressed and ke.scancode == 33554431:
		_send_backspace_key()
		
func _send_backspace_key():
	var backspace_event = InputEventKey.new()
	backspace_event.scancode = KEY_BACKSPACE
	backspace_event.pressed = true
	get_focus_owner()._gui_input(backspace_event)
