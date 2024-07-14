using Godot;
using System;
using System.Security.Cryptography;

public class NumericUpDown : Panel
{
    Timer timer = new Timer();
    bool incrementButtonPressed = false;
	bool decrementButtonPressed = false;
	
    private LineEdit lineEdit;

    private int mValue = 0;

    [Export]
    public int Value
    {
        get { return mValue; }
        set 
        {
            if (mValue != value)
            {
                mValue = value;
                UpdateText();
            }
        }
    }

    [Export]
    public int Step = 1;

    [Export]
    public int MaxValue = 100;

	[Export]
	public int MinValue = 0;

	[Signal]
	public delegate void value_changed(int newValue);

    public LineEdit GetLineEdit() { return lineEdit; }

    private static bool isNumeric(uint scancode)
    {
        if( scancode >= (uint)KeyList.Key0 && scancode <= (uint)KeyList.Key9)
            return true;
		if (scancode >= (uint)KeyList.Kp1 && scancode <= (uint)KeyList.Kp9)
			return true;

		return false;
    }

    private void UpdateText()
    {
        if (lineEdit != null)
        {
            string newVal = Value.ToString();
            if (newVal != lineEdit.Text)
            {
                lineEdit.Text = newVal;
                lineEdit.CaretPosition = lineEdit.Text.Length;
                EmitSignal("value_changed", Value);
            }
        }
    }

    private void Increment()
    {
        int newValue = Value + Step;
        if (newValue > MaxValue)
            newValue = MaxValue;
        Value = newValue;
        UpdateText();    
    }

    private void Decrement()
    {
        int newValue = Value - Step;
        if (newValue < MinValue)
            newValue = MinValue;
        Value = newValue;
        UpdateText();
    }

	public void OnGuiInput(InputEvent inputEvent)
    {
        InputEventKey ke = inputEvent as InputEventKey;
        if( ke != null && ke.Pressed)
        {
            if (ke.Scancode == (uint)KeyList.Up)
            {
                Increment();
                AcceptEvent();
            }
            else if (ke.Scancode == (uint)KeyList.Down)
            {
                Decrement();
                AcceptEvent();
            }
            else if (isNumeric(ke.Scancode) || ke.Scancode == (uint)KeyList.Backspace || ke.Scancode == (uint)KeyList.Delete  )
            {
                // cool
            }
            else
            {
				AcceptEvent();
				UpdateText();
			}
        }
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        lineEdit = GetNode<LineEdit>("HBoxContainer/LineEdit");
        lineEdit.Connect("gui_input", this, nameof(OnGuiInput));
        lineEdit.Connect("focus_exited", this,nameof(OnFocusLost));
        Button btn = GetNode<Button>("HBoxContainer/Panel/VBoxContainer/upButton");
        btn.Connect("button_down", this, nameof(IncrementButtonDown));
		btn.Connect("button_up", this, nameof(IncrementButtonUp));

		btn = GetNode<Button>("HBoxContainer/Panel/VBoxContainer/downButton");
		btn.Connect("button_down", this, nameof(DecrementButtonDown));
		btn.Connect("button_up", this, nameof(DecrementButtonUp));
		lineEdit.Text = Value.ToString();
        timer.WaitTime = 0.50f;
        timer.OneShot = false;
        timer.Connect("timeout", this, "OnTimerTick");
        AddChild(timer);
	}

    int numTicks = 0;

    public void OnTimerTick()
    {
        GD.Print("TimerTick");
        numTicks++;
        if (numTicks > 2)
        {
			timer.WaitTime = 0.10f;
		}
        if (incrementButtonPressed)
            Increment();
        else if (decrementButtonPressed)
            Decrement();
        else
        {
            timer.Stop();
            numTicks = 0;
        }
    }

    public void IncrementButtonDown()
    { 
        incrementButtonPressed = true;
        Increment();
        timer.Start(0.50f);
    }
	public void IncrementButtonUp() { incrementButtonPressed = false; }

	public void DecrementButtonDown()
	{
		decrementButtonPressed = true;
        Decrement();
		timer.Start(0.50f);
	}
	public void DecrementButtonUp() { decrementButtonPressed = false; }


	public void OnFocusLost()
    {
        string textValue = "";
        foreach(char c in lineEdit.Text)
        {
            if(char.IsDigit(c))
                textValue += c;
        }
        Value = Int32.Parse( textValue);
    }
//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
