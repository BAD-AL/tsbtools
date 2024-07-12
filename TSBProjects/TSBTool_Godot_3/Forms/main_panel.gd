extends Panel

var myCallback:JavaScriptObject
var textureRect:TextureRect
var titleLabel:Label
var dynamic_font : DynamicFont = null
var font_size :=30
	
##### Screen reduction stuff start
func check_screen_size():
	var screen_width = OS.get_window_size().x
	#print("check_screen_size")
	if screen_width < 800:
		get_smaller()
	else:
		get_bigger()

func change_title_font_size(the_size:int):
	if the_size != font_size:
		font_size = the_size
		dynamic_font.size = font_size
		titleLabel.add_font_override("font", dynamic_font)
		

func get_smaller():
	textureRect.hide()
	change_title_font_size(20)

func get_bigger():
	textureRect.show()
	change_title_font_size(30)
	
##### Screen reduction stuff end

# Called when the node enters the scene tree for the first time.
func _ready():
	disable_buttons()
	OS.set_window_title("TSBTool")
	#setup 
	textureRect = $MarginContainer/VBoxContainer/middlePanel/HBoxContainer/TextureRect
	titleLabel = $MarginContainer/VBoxContainer/Panel/titleLabel
	dynamic_font = DynamicFont.new()
	dynamic_font.font_data = load("res://Fonts/GF-TecmoNarrow.TTF")
	print("main_panel._ready() os :" + OS.get_name())
	var tecmoControl = $MarginContainer/VBoxContainer/bottomPanel/tecmoControl
	print("main_panel._ready() C# message:" + tecmoControl.TecmoHelper.GetMessageFromCSharp())
	Globals.tecmoHelper = tecmoControl.TecmoHelper
	# Screen size stuff 
	check_screen_size()
	get_tree().get_root().connect("size_changed", self, "check_screen_size")
	
	if OS.get_name() == "HTML5":
		myCallback = JavaScript.create_callback(self, "fileLoaded")
		var window = JavaScript.get_interface("window")
		window.fileLoaded = myCallback
		insert_html_and_js()

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
	print("_on_load_rom_button_pressed: OS = " + OS.get_name())
	prompt_user_for_file("LOAD")

func _on_save_button_pressed() -> void:
	print("SAVE ROM clicked")
	prompt_user_for_file("SAVE")
	
# used for file selection (LOAD & SAVE)
var file_dialog: FileDialog = null

func prompt_user_for_file(operation := "LOAD"):
	print("prompt_user_for_file: OS = " + OS.get_name())
	if OS.get_name() == "HTML5":
		handle_html5_file_prompting(operation)
		return

	file_dialog = FileDialog.new()
	file_dialog.rect_min_size = get_viewport().size
	file_dialog.rect_min_size.x -= 20
	file_dialog.rect_min_size.y -= 40
	file_dialog.access = FileDialog.ACCESS_FILESYSTEM
	file_dialog.filters = ["*.nes", "*.snes"]
	
	if operation == "LOAD":
		file_dialog.mode =FileDialog.MODE_OPEN_FILE
		file_dialog.connect("file_selected",self, "_on_load_file_selected")
	else:
		file_dialog.mode = FileDialog.MODE_SAVE_FILE
		file_dialog.connect("file_selected", self, "_on_save_file_selected")
	#file_dialog.connect("canceled",self, "_cleanup_file_dialog")
	file_dialog.connect("popup_hide", self, "_cleanup_file_dialog")
	
	if OS.get_name() == "Android" or  OS.get_name() == "iOS":	
		#might need one of these instead this for android/iOS
		file_dialog.access = FileDialog.ACCESS_USERDATA
		# file_dialog.access = FileDialog.ACCESS_RESOURCES
	get_tree().root.add_child(file_dialog)
	file_dialog.popup_centered()

func _on_load_file_selected(path:String):
	print("(Load) Selected file: " + path)
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


func _on_aboutButton_pressed():
		Globals.show_message( 
	"""This tool is intended for Quick Edits of TSB ROMS. 
	For full featured modification support use 'TSBToolSupreme.exe' (Windows)
	(also Works on Linux and Mac under 'mono' [https://www.mono-project.com/download/stable/])
	GitHub repo: https://github.com/BAD-AL/tsbtools
	Forum Discussion: https://tecmobowl.org/forums/topic/11106-tsb-editor-tsbtool-supreme-season-generator/
	""",
	"TSBTool, by BAD-AL" )

""" 
	HTML5 related Stuff:
"""

func handle_html5_file_prompting(operation:String):
	if operation == "LOAD":
		print("Prompt user to select file to Load> ")
		JavaScript.eval("document.getElementById('fileInput').click();")
	else:
		var fileType := "nes"
		if Globals.tecmoHelper.GetRomVersion().find("SNES") > -1 :
			fileType = "snes"
		var base64_string:String = Globals.tecmoHelper.GetBase64RomString()
		var window = JavaScript.get_interface("window")
		window.saveRomFile(base64_string, fileType)

func insert_html_and_js():
	var js_code = """
	// Create and insert the file input element
	var fileInput = document.createElement('input');
	fileInput.type = 'file';
	fileInput.id = 'fileInput';
	fileInput.accept = '.nes, .snes';
	fileInput.style.display = 'none';
	
	document.body.appendChild(fileInput); // Append the file input element to the body

	function onFileSelected(event) {
		var file = event.target.files[0];
		window.romFilePath = file;
		var reader = new FileReader();
		reader.onload = function(e) {
			var arrayBuffer = e.target.result;
			var bytes = new Uint8Array(arrayBuffer);
			// Convert to Base64
			var binaryString = '';
			for (var i = 0; i < bytes.byteLength; i++) {
				binaryString += String.fromCharCode(bytes[i]);
			}
			var base64String = btoa(binaryString);
			//window.fileBytesBase64 = base64String;
			//console.log("onFileSelected: fileBytes.length:" + bytes.length );
			window.fileLoaded(base64String); // Pass the bytes to Godot callback
		};
		reader.readAsArrayBuffer(file);
	}

	window.saveRomFile = function (base64String, saveType) {
		// Decode the Base64 string
		var binaryString = atob(base64String);
		var len = binaryString.length;
		var bytes = new Uint8Array(len);
		for (var i = 0; i < len; i++) {
			bytes[i] = binaryString.charCodeAt(i);
		}
		
		var blob = new Blob([bytes], { type: 'application/octet-stream' }); // Create a Blob from the bytes

		var link = document.createElement('a'); // link click stuff
		link.href = window.URL.createObjectURL(blob);
		var initial = 'modified_rom.' + saveType;
		// Prompt the user for a filename
		var filename = prompt('Enter the filename to save:', initial);
		if (filename) {
			link.download = filename;
			document.body.appendChild(link);
			link.click();
			document.body.removeChild(link);
		} else {
			alert('Save cancelled.');
		}
	}

	// Add event listener to the file input element
	document.getElementById('fileInput').addEventListener('change', onFileSelected);
	"""
	JavaScript.eval(js_code)
	
func fileLoaded(args:Array):
	var base64String = args[0]
	print("fileLoaded called back from JavaScript! ")
	Globals.tecmoHelper.LoadRomBytesFromBase64String(base64String)
	enable_buttons()




func _on_deleteMeButton_pressed():
	SceneManager.push_scene("res://Forms/EditPlayer2.tscn")
