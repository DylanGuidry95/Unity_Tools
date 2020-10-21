using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.PostProcessing;
using UnityEditor.ShaderGraph.Internal;
using System.Xml.Serialization;

public class Control
{
    public string Name;
    public Rect Position;
    public Color DrawColor;
    public Rect DrawRect;
    public bool IsVisible;

    public delegate void Interact(Control cotnrol);
    public Interact OnInteract;

    public Control()
    {
        Name = "";
        Position = Rect.zero;
        DrawColor = GUI.color;
        DrawRect = Rect.zero;
        IsVisible = true;
    }

    public virtual void Draw(Rect homeRect)
    {
        if (!IsVisible)
            return;
        GUI.color = DrawColor;        
    }

    public virtual void OnMouseClick()
    {
        var mousePosition = Event.current.mousePosition;
        if(DrawRect.Contains(mousePosition))
        {            
            if (Event.current.type == EventType.Used || Event.current.type == EventType.MouseDown)
            {                
                if(OnInteract != null)
                    OnInteract.Invoke(this);
                Event.current.Use();
            }            
        }

    }

    public void ToggleView()
    {
        IsVisible = !IsVisible;
    }
}

public class GUIContianiner : Control
{
    public List<Control> Controls;

    public Vector2 ControlPadding; 

    public GUIContianiner()
    {
        DrawColor = GUI.color;
        ControlPadding = Vector2.zero;
        Controls = new List<Control>();
        DrawRect = Rect.zero;
    }

    public override void Draw(Rect homeRect)
    {
        base.Draw(homeRect);
        DrawRect = Position;
        if (homeRect != Rect.zero)
        {
            var DrawPosition = new Rect(homeRect.position + Position.position,
                Position.size);
            DrawRect = DrawPosition;
            GUI.Box(DrawPosition, "");
        }
        else
            GUI.Box(Position, "");
        Vector2 lastDrawControl = DrawRect.min;
        foreach (var c in Controls)
        {
            if (c == Controls[0])
                c.Draw(new Rect(DrawRect.position + ControlPadding, Position.size));
            else
                c.Draw(new Rect(lastDrawControl, c.Position.size));
            lastDrawControl = c.DrawRect.max + ControlPadding;
            lastDrawControl.x = DrawRect.min.x + ControlPadding.x;
        }
    }

    public Control GetControl(string name)
    {
        foreach(var c in Controls)
        {
            if (c.Name == name)
                return c;
        }
        return null;
    }

    public void AddControl(Control control)
    {
        foreach (var c in Controls)
        {
            if (c.Name == control.Name)
                return;
        }
        if (!Controls.Contains(control) && control != this)
            Controls.Add(control);
    }

    public void RemoveControl(string controlName)
    {
        Control removedControl = null;
        foreach(var c in Controls)
        {
            if (c.Name == controlName)
            {
                removedControl = c;
            }
        }
        if (removedControl != null)
            Controls.Remove(removedControl);
    }
}

public class AutoResizeContainer : GUIContianiner
{
    public override void Draw(Rect homeRect)
    {
        base.Draw(homeRect);
        float total_height = 0;
        foreach(var c in Controls)
        {
            total_height += c.Position.height;
        }
        Position.size = new Vector2(Position.size.x, total_height + (ControlPadding.y * (Controls.Count + 1)));
    }
}

public class EditorButton : Control
{
    public delegate void ButtonClicked();
    public ButtonClicked OnClicked;

    public string Text;
    public bool IsInteractable;

    public override void Draw(Rect homeRect)
    {
        base.Draw(homeRect);
        var DrawPosition = new Rect(homeRect.position + Position.position, Position.size);
        if (!IsInteractable)
            DrawColor = Color.gray;
        else
            DrawColor = Color.white;
        DrawRect = DrawPosition;
        if (GUI.Button(DrawPosition, Text))
        {
            if (OnClicked != null && IsInteractable)
                OnClicked.Invoke();
        }        
    }
}

public class EditorFloatField : Control
{
    public bool IsReadOnly;    
    public float Value;

    public override void Draw(Rect homeRect)
    {
        GUI.color = Color.white;                
        DrawRect = new Rect(homeRect.position + Position.position, Position.size);
        float localStorage = EditorGUI.FloatField(new Rect(DrawRect.position, Position.size), Name, Value);
        if (localStorage != Value && OnInteract != null)
            OnInteract.Invoke(this);
        Value = localStorage;
        OnMouseClick();
    }
}

public class EditorIntField : Control
{
    public bool IsReadOnly;
    public int Value;

    public override void Draw(Rect homeRect)
    {
        GUI.color = Color.white;
        DrawRect = new Rect(homeRect.position + Position.position, Position.size);        
        int localStorage = EditorGUI.IntField(new Rect(DrawRect.position, Position.size), Name, Value);
        if (localStorage != Value && OnInteract != null)
            OnInteract.Invoke(this);
        Value = localStorage;
        OnMouseClick();
    }
}

public class EditorStringField : Control
{
    public bool IsReadOnly;
    public string Value;

    public override void Draw(Rect homeRect)
    {
        GUI.color = Color.white;
        DrawRect = new Rect(homeRect.position + Position.position, Position.size);
        string localStorage = EditorGUI.TextField(new Rect(DrawRect.position, Position.size), Name, Value);
        if (localStorage != Value && OnInteract != null)
            OnInteract.Invoke(this);
        Value = localStorage;
        OnMouseClick();
    }
}

public class EditorBooleanField : Control
{
    public bool IsReadOnly;
    public bool Value;

    public override void Draw(Rect homeRect)
    {
        GUI.color = Color.white;
        DrawRect = new Rect(homeRect.position + Position.position, Position.size);
        bool localStorage = EditorGUI.Toggle(new Rect(DrawRect.position, Position.size), Name, Value);
        if (localStorage != Value && OnInteract != null)
            OnInteract.Invoke(this);
        Value = localStorage;
        OnMouseClick();
    }
}

public class EditorEnumField : Control
{
    public bool IsReadOnly;
    public System.Enum Value;

    public override void Draw(Rect homeRect)
    {
        base.Draw(homeRect);
        GUI.color = Color.white;
        DrawRect = new Rect(homeRect.position + Position.position, Position.size);
        System.Enum localStorage = EditorGUI.EnumPopup(DrawRect, Name, Value);
        if (localStorage != Value && OnInteract != null)
            OnInteract.Invoke(this);
        Value = localStorage;
        OnMouseClick();
    }
}