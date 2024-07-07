extends Control

# Called when the node enters the scene tree for the first time.
func _ready():
	disable_buttons()

func disable_buttons():
	$MarginContainer/VBoxContainer/middlePanel/HBoxContainer/MarginContainer/Panel/MarginContainer/VBoxContainer/editPlayersButton.disabled = true
	$MarginContainer/VBoxContainer/middlePanel/HBoxContainer/MarginContainer/Panel/MarginContainer/VBoxContainer/editTeamsButton.disabled = true
	$MarginContainer/VBoxContainer/middlePanel/HBoxContainer/MarginContainer/Panel/MarginContainer/VBoxContainer/viewTextButton.disabled = true
	$MarginContainer/VBoxContainer/middlePanel/HBoxContainer/MarginContainer/Panel/MarginContainer/VBoxContainer/saveButton.disabled = true

func enable_buttons():
	$MarginContainer/VBoxContainer/middlePanel/HBoxContainer/MarginContainer/Panel/MarginContainer/VBoxContainer/editPlayersButton.disabled = false
	$MarginContainer/VBoxContainer/middlePanel/HBoxContainer/MarginContainer/Panel/MarginContainer/VBoxContainer/editTeamsButton.disabled = false
	$MarginContainer/VBoxContainer/middlePanel/HBoxContainer/MarginContainer/Panel/MarginContainer/VBoxContainer/viewTextButton.disabled = false
	$MarginContainer/VBoxContainer/middlePanel/HBoxContainer/MarginContainer/Panel/MarginContainer/VBoxContainer/saveButton.disabled = false

func _on_about_button_pressed() -> void:
	Globals.show_message( 
	"""This tool is intended for Quick Edits of TSB ROMS. 
	For full featured modification support use 'TSBToolSupreme.exe' (Windows)
	(also Works on Linux and Mac under 'mono' [https://www.mono-project.com/download/stable/])
	GitHub repo: https://github.com/BAD-AL/tsbtools
	Forum Discussion: https://tecmobowl.org/forums/topic/11106-tsb-editor-tsbtool-supreme-season-generator/
	""",
	"TSBTool, by BAD-AL" )
	
func _on_edit_players_button_pressed():
	print("edit players clicked")
	SceneManager.push_scene("res://Forms/EditPlayer.tscn")

func _on_edit_teams_button_pressed():
	print("edit teams clicked")
	SceneManager.push_scene("res://Forms/EditTeam.tscn")

func _on_view_text_button_pressed():
	print("view text clicked")
	SceneManager.push_scene("res://Forms/TextView.tscn")

func _on_load_rom_button_pressed():
	print("Load ROM clicked")
	prompt_user_for_file_file("LOAD")

func _on_save_button_pressed() -> void:
	print("SAVE ROM clicked")
	prompt_user_for_file_file("SAVE")
	
# used for file selection (LOAD & SAVE)
var file_dialog: FileDialog = null

func prompt_user_for_file_file(operation := "LOAD"):
	file_dialog = FileDialog.new()
	file_dialog.size = get_window().size
	file_dialog.size.x -= 20
	file_dialog.size.y -= 40
	if operation == "LOAD":
		file_dialog.file_mode = FileDialog.FILE_MODE_OPEN_FILE
		file_dialog.connect("file_selected", _on_load_file_selected)
	else:
		file_dialog.file_mode = FileDialog.FILE_MODE_SAVE_FILE
		file_dialog.connect("file_selected", _on_save_file_selected)
	file_dialog.connect("canceled", _cleanup_file_dialog)
	file_dialog.access = FileDialog.ACCESS_FILESYSTEM
	if OS.get_name() == "Android" or  OS.get_name() == "iOS":	
		#might need one of these instead this for android/iOS
		file_dialog.access = FileDialog.ACCESS_USERDATA
		#file_dialog.access = FileDialog.ACCESS_RESOURCES
	file_dialog.filters = ["*.nes", "*.snes"]
	Globals.add_child(file_dialog)
	file_dialog.popup_centered()

func _on_load_file_selected(path:String):
	print("(Load) Selected file: " + path)
	var my_csharp_script = load("res://Lib.NET/TecmoHelper.cs")
	Globals.tecmoHelper = my_csharp_script.new()
	Globals.tecmoHelper.LoadRom(path);
	enable_buttons()
	_cleanup_file_dialog()

func _on_save_file_selected(path:String):
	print("(Save) Selected file: " + path)
	Globals.tecmoHelper.SaveRom(path)
	_cleanup_file_dialog()
	
func _cleanup_file_dialog():
	if file_dialog != null:
		print("cleanup file_dialog")
		file_dialog.queue_free()
		file_dialog = null
