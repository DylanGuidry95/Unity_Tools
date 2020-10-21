using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Xml.Serialization;
using UnityEngine.Rendering;
using System.Net.Http.Headers;
using Visual_Test;
using System.Diagnostics.Tracing;
using System;

public class StateMachineEditor : EditorWindow
{
    private static GUIContianiner ControlsContianer;
    private static GUIContianiner paramContainer;
    private static GUIContianiner addParamContainer;

    private static StateMachine StateRef;
    
    private enum ParamType
    {
        _int, _string, _float, _bool
    }

    private ParamType TypeToAdd;
    public string ParamName;

    private Control LastParamInteracted;

    [UnityEditor.MenuItem("Tools/State Machine Editor")]
    private static void Init()
    {
        var window = ScriptableObject.CreateInstance<StateMachineEditor>();
        window.Show();

        ControlsContianer = new GUIContianiner()
        {
            Name = "Container",
            Position = new Rect(0, 0, 325, 500),
            ControlPadding = new Vector2(5, 5)
        };
        paramContainer = new GUIContianiner()
        {
            Name = "Parameters",
            Position = new Rect(0, 0, 315, 500),
            DrawColor = Color.blue,
            ControlPadding = new Vector2(5, 5)
        };
        addParamContainer = new AutoResizeContainer()
        {
            Name = "Add Param Container",
            Position = new Rect(0, 0, 315, 100),
            ControlPadding = new Vector2(5, 5),
            DrawColor = Color.green
        };

        StateRef = new StateMachine();
    }

    private void OnGUI()
    {
        DrawControls();
    }    

    public void DrawControls()
    {
        if (ControlsContianer == null)
        {
            ControlsContianer = new GUIContianiner()
            {
                Name = "Container",
                Position = new Rect(0, 0, 325, maxSize.y),
                ControlPadding = new Vector2(5, 5)
            };
        }

        ControlsContianer.Position.size = new Vector2(ControlsContianer.Position.size.x, maxSize.y);

        if(paramContainer == null)
            paramContainer = new GUIContianiner()
            {
                Name = "Parameters",
                Position = new Rect(0, 0, 315, 500),
                DrawColor = Color.blue,
                ControlPadding = new Vector2(5,5)
            };
        
        if(addParamContainer == null)
        {
            addParamContainer = new AutoResizeContainer()
            {
                Name = "Add Param Container",
                Position = new Rect(0, 0, 315, 100),
                ControlPadding = new Vector2(5, 5),
                DrawColor = Color.green
            };
        }

        if(StateRef == null)
        {
            StateRef = new StateMachine();
        }

        if(StateRef != null)
            foreach(var p in StateRef.Parameters)
            {
                if(p.Value.GetType() == typeof(Visual_Test.IntParameter))
                {
                    var intField = new EditorIntField()
                    {
                        Name = p.Key,
                        Position = new Rect(0, 0, 300, 25),
                        Value = (int)p.Value.Value,
                        OnInteract = ControlLastInteracted
                    };
                    paramContainer.AddControl(intField);
                    p.Value.SetValue(((EditorIntField)paramContainer.GetControl(p.Key)).Value);
                }
                if(p.Value.GetType() == typeof(Visual_Test.FloatParameter))
                {
                    var floatField = new EditorFloatField()
                    {
                        Name = p.Key,
                        Position = new Rect(0, 0, 300, 25),
                        Value = (float)p.Value.Value,
                        OnInteract = ControlLastInteracted
                    };                    
                    paramContainer.AddControl(floatField);
                    p.Value.SetValue(((EditorFloatField)paramContainer.GetControl(p.Key)).Value);                    
                }
                if(p.Value.GetType() == typeof(Visual_Test.StringParameter))
                {
                    var stringField = new EditorStringField()
                    {
                        Name = p.Key,
                        Position = new Rect(0, 0, 300, 25),
                        Value = (string)p.Value.Value,
                        OnInteract = ControlLastInteracted
                    };
                    paramContainer.AddControl(stringField);
                    p.Value.SetValue(((EditorStringField)paramContainer.GetControl(p.Key)).Value);
                }
                if(p.Value.GetType() == typeof(Visual_Test.BooleanParamater))
                {
                    var boolField = new EditorBooleanField()
                    {
                        Name = p.Key,
                        Position = new Rect(0, 0, 300, 25),
                        Value = (bool)p.Value.Value,
                        OnInteract = ControlLastInteracted
                    };
                    paramContainer.AddControl(boolField);
                    p.Value.SetValue(((EditorBooleanField)paramContainer.GetControl(p.Key)).Value);                    
                }
            }

        var AddParam = new EditorButton() { Name = "Add Param", Position = new Rect(0, 0, 125, 25), Text = "Add Parameter" };        
        AddParam.OnClicked = AddParameter;
        var RemoveParam = new EditorButton() { Name = "Delte Param", Position = new Rect(0, 0, 125, 25), Text = "Delete Parameter", IsInteractable = true };
        RemoveParam.OnClicked = DeleteParameter;

        var testEnum = new EditorEnumField() { Name = "Tester", Position = new Rect(0, 0, 300, 25), Value = TypeToAdd };
        var testString = new EditorStringField()
        {
            Name = "Param Name",
            Position = new Rect(0, 0, 300, 25),
            Value = ""            
        };        

        ControlsContianer.AddControl(paramContainer);
        ControlsContianer.AddControl(addParamContainer);
        addParamContainer.AddControl(RemoveParam);
        addParamContainer.AddControl(AddParam);        
        addParamContainer.AddControl(testEnum);
        addParamContainer.AddControl(testString);        
                

        ((EditorButton)addParamContainer.GetControl("Add Param")).IsInteractable = ParamName != "";
        ParamName = ((EditorStringField)addParamContainer.GetControl("Param Name")).Value;
        TypeToAdd = (ParamType)((EditorEnumField)addParamContainer.GetControl("Tester")).Value;
        ControlsContianer.Draw(Rect.zero);       
    }

    public void ControlLastInteracted(Control control)
    {        
        LastParamInteracted = control;
    }

    public void DeleteParameter()
    {
        if (LastParamInteracted == null)
            return;
        paramContainer.RemoveControl(LastParamInteracted.Name);
        StateRef.RemoveParamter(LastParamInteracted.Name);
    }

    public void AddParameter()
    {
        switch(TypeToAdd)
        {
            case ParamType._bool:
                StateRef.AddParameter(ParamName, true);
                break;
            case ParamType._string:
                StateRef.AddParameter(ParamName, "");
                break;
            case ParamType._float:
                StateRef.AddParameter(ParamName, 0.0f);
                break;
            case ParamType._int:
                StateRef.AddParameter(ParamName, 0);
                break;
        }
    }
}

