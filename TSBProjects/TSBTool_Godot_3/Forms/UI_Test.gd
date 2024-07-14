extends Panel

var gridContainer :GridContainer
var displayLabel:Label 
var backspaceLabel:Label
var eventLog:TextEdit
 
func log_(msg):
	#eventLog.text += msg + "\n"
	eventLog.text = msg + "\n" + eventLog.text
	eventLog.cursor_set_column(0)
	eventLog.cursor_set_line(0)

##### Screen reduction stuff start
func check_screen_size():
	var screen_size = OS.get_window_size()
	#displayLabel.text = "Window Size: %d %d" %[screen_size.x, screen_size.y]
	if screen_size.x < 806:
		gridContainer.columns = 1
	else:
		gridContainer.columns = 2

##### Screen reduction stuff end

# Called when the node enters the scene tree for the first time.
func _ready():
	eventLog = $MarginContainer/VBoxContainer/eventLog
	gridContainer = $MarginContainer/VBoxContainer/GridContainer
	backspaceLabel = $MarginContainer/VBoxContainer/GridContainer/Panel3/HBoxContainer/backSpaceLabel
	backspaceLabel.connect("gui_input", self, "_on_backspace_label_input")
	$MarginContainer/VBoxContainer/GridContainer/Panel/HBoxContainer/LineEdit.connect("gui_input", self, "_on_control_gui_input")
	$MarginContainer/VBoxContainer/GridContainer/Panel2/HBoxContainer/MarginContainer/SpinBox.connect("gui_input", self, "_on_control_gui_input")
	$MarginContainer/VBoxContainer/GridContainer/Panel2/HBoxContainer/MarginContainer/SpinBox.get_line_edit().connect("gui_input", self, "_on_control_gui_input")
	$MarginContainer/VBoxContainer/GridContainer/Panel3/HBoxContainer/TextEdit.connect("gui_input", self, "_on_control_gui_input")
	$MarginContainer/VBoxContainer/GridContainer/Panel4/HBoxContainer/sendAndroidBackspaceLabel.connect("gui_input", self, "_on_send_android_bs")
	$MarginContainer/VBoxContainer/GridContainer/Panel4/HBoxContainer/javascriptKeyboardLabel.connect("gui_input", self, "_on_show_keyboard_js")
	#displayLabel = $MarginContainer/GridContainer/displayLabel
	check_screen_size()
	get_tree().root.connect("size_changed", self, "check_screen_size")

func _on_show_keyboard_js(event):
	if event is InputEventMouseButton and event.pressed:
		log_("_on_show_keyboard_js")
		JavaScript.eval("if(window.showKeyboard != null) window.showKeyboard();")
	
func _on_send_android_bs(event):
	if event is InputEventMouseButton and event.pressed:
		log_("_on_send_android_bs")
		var backspace_event = InputEventKey.new()
		backspace_event.scancode = 33554431
		backspace_event.pressed = true
		get_focus_owner()._gui_input(backspace_event)
		
	
func _on_closeButton_pressed():
	SceneManager.pop_scene()

func _on_control_gui_input(event):
	if event is InputEventMouseMotion:
		return
	var ke := (event as InputEventKey)
	if ke != null:
		log_("pressed: %d scan_code: %d unicode: %d psc %d" % [ke.pressed as int, ke.scancode, ke.unicode, ke.physical_scancode] )
		if ke.pressed and ke.scancode == 33554431:
			_send_backspace_key(get_focus_owner())
	else:
		log_(event.to_string())

## backspace key testing
func _on_backspace_label_input(event):
	if event is InputEventMouseButton and event.pressed:
		log_("_on_backspace_label_input")
		var focused_control = get_focus_owner()
		if focused_control is LineEdit:
			_send_backspace_key(focused_control)
		elif focused_control is TextEdit:
			_send_backspace_key(focused_control)
		elif focused_control is SpinBox:
			_send_backspace_key(focused_control.get_line_edit())
		
func _send_backspace_key(control):
	var backspace_event = InputEventKey.new()
	backspace_event.scancode = KEY_BACKSPACE
	backspace_event.pressed = true
	control._gui_input(backspace_event)


func _on_close_button_pressed():
	SceneManager.pop_scene()


func _on_clear_button_pressed():
	eventLog.text = ""
