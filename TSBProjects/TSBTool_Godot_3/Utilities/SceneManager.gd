extends Node
#class_name SceneManager

var scene_stack := []

func _ready():
	# Add the current main scene to the stack
	var main_scene = get_tree().current_scene
	if main_scene:
		scene_stack.append(main_scene)

func push_scene(resource_path: String):
	var new_scene = load(resource_path).instance()
	if scene_stack.size() > 0:
		scene_stack[-1].hide()
	add_child(new_scene)
	scene_stack.append(new_scene)

func pop_scene():
	if scene_stack.size() > 1:
		var top_scene = scene_stack.pop_back()
		remove_child(top_scene)
		top_scene.queue_free()
		scene_stack[-1].show()
	elif scene_stack.size() == 1:
		print("Can't pop the last scene!")

func get_current_scene():
	return scene_stack[-1] if scene_stack.size() > 0 else null
