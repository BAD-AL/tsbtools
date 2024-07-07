extends Node


var tecmoHelper

var message_box: AcceptDialog = null;

func show_message(message_text: String, title: String = "Information"):
	message_box = AcceptDialog.new()
	message_box.name = "message_box"
	message_box.title = title
	message_box.dialog_text = message_text
	#message_box.theme = load("res://Themes/tecmo_menu_theme.tres")
	message_box.connect("confirmed", _on_message_box_confirmed)
	message_box.connect("canceled", _on_message_box_confirmed)
	#message_box.popup_centered() # Show the dialog
	message_box.popup_exclusive_centered(get_tree().root)

func _on_message_box_confirmed():
	if message_box != null:
		print("cleanup message_box")
		message_box.queue_free()
		message_box = null
