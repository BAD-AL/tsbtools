extends Panel

var textBox:TextEdit

# Called when the node enters the scene tree for the first time.
func _ready():
	var extraMessage = "# OS.get_name() = " + OS.get_name() + "\n"
	textBox = $VBoxContainer/textEdit
	print("scrollbar size.x " +  str(textBox.get_v_scroll_bar().size.x))
	textBox.text = extraMessage
	if Globals.tecmoHelper != null:
		textBox.text += Globals.tecmoHelper.GetAll(1)
	

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _on_close_button_pressed():
	print("closeButton pressed")
	Globals.tecmoHelper.ProcessText(textBox.text)
	SceneManager.pop_scene()


func _on_prow_bowl_check_box_toggled(toggled_on: bool) -> void:
	if Globals.tecmoHelper == null:
		return
	var text:String = Globals.tecmoHelper.GetAll(1)
	if(toggled_on):
		text += Globals.tecmoHelper.GetProBowlPlayers(1)
	textBox.text = text
