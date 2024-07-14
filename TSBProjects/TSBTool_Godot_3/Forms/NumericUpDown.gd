extends Panel
var lineEdit:LineEdit

export var value := 0 
export var step := 1
export var min_value := 0
export var max_value := 100

signal value_changed

# goals: functional on all platforms, up, down buttons; up, down keys, int only
# 
# KEY_0 = 48 , KeyList KEY_9 = 57; KEY_KP_0 = 16777350, KEY_KP_9 = 16777359

func is_numeric_scancode(code:int) -> bool:
	if code >= KEY_0 && code <= KEY_9:
		return true
	if code >= KEY_KP_0 && code <= KEY_KP_9:
		return true
	return false 

func _on_gui_input(event):
	var ke := event as InputEventKey
	if ke == null:
		return
	print( "KeyEvent: "+ ke.as_text())
	if ke.pressed:
		if ke.scancode == KEY_UP:
			increment()
			accept_event()
		elif ke.scancode == KEY_DOWN:
			decrement()
			accept_event()
		elif ! is_numeric_scancode(ke.scancode):
			accept_event()
			update_text()
		

func increment():
	if( value + step > max_value):
		value = max_value
	else:
		value += step
	update_text()
	
func decrement():
	if( value - step < min_value):
		value = min_value
	else:
		value -= step
	update_text()
	
func update_text():
	var newValue = str(value)
	if newValue != lineEdit.text:
		lineEdit.text = newValue
		emit_signal("value_changed", value)
	

# Called when the node enters the scene tree for the first time.
func _ready():
	lineEdit = $MarginContainer/HBoxContainer/LineEdit
	lineEdit.connect("gui_input", self, "_on_gui_input" )
