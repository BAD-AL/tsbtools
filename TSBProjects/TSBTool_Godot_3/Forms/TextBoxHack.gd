extends LineEdit

func _on_control_gui_input(event):
	var ke := event as InputEventKey
	if ke != null:
		print( "pressed: %d scan_code: %d unicode: %s" % [ke.pressed as int, ke.scancode, ke.unicode] )
		if ke.pressed and ke.scancode == 33554431:
			_send_backspace_key(get_focus_owner())

func _send_backspace_key(control):
	var backspace_event = InputEventKey.new()
	backspace_event.scancode = KEY_BACKSPACE
	backspace_event.pressed = true
	control._gui_input(backspace_event)
	
# Called when the node enters the scene tree for the first time.
func _ready():
	print("TextBoxHack ready")
	self.connect("gui_input", self,"_on_control_gui_input")

