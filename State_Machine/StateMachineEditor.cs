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
    private static ScrollContainer ParamContainer;
    private static GUIContianiner AddParamContainer;
    private static GUIContianiner ActiveMachineContainer;

    private static StateMachine StateRef;
    
    private enum ParamType
    {
        _int, _string, _float, _bool
    }

    private static ParamType TypeToAdd;
    public static string ParamName;

    private static Control LastParamInteracted;    

    [UnityEditor.MenuItem("Tools/State Machine Editor")]
    private static void Init()
    {
        var window = ScriptableObject.CreateInstance<StateMachineEditor>();
        window.Show();

        ControlsContianer = new GUIContianiner()
        {
            Name = "Container",
            Position = new Rect(0, 0, 325, 100),
            ControlPadding = new Vector2(5, 5)
        };

        ActiveMachineContainer = new AutoResizeContainer()
        {
            Name = "Active Machine",
            Position = new Rect(0, 0, 315, 100),
            ControlPadding = new Vector2(5, 5),
            DrawColor = Color.cyan
        };

        var newObjectField = new EditorObjectField()
        {
            Name = "State Machine",
            Position = new Rect(0, 0, 300, 25),
            AcceptableType = typeof(StateMachine)
        };

        ActiveMachineContainer.AddControl(newObjectField);

        ParamContainer = new ScrollContainer()
        {
            Name = "Parameters",
            Position = new Rect(0, 0, 315, 200),
            DrawColor = Color.blue,
            ControlPadding = new Vector2(5, 5)
        };
        AddParamContainer = new AutoResizeContainer()
        {
            Name = "Add Param Container",
            Position = new Rect(0, 0, 315, 100),
            ControlPadding = new Vector2(5, 5),
            DrawColor = Color.green
        };

        var AddParam = new EditorButton() { Name = "Add Param", Position = new Rect(0, 0, 125, 25), Text = "Add Parameter" };
        AddParam.OnClicked = AddParameter;
        var RemoveParam = new EditorButton() { Name = "Delte Param", Position = new Rect(0, 0, 125, 25), Text = "Delete Parameter", IsInteractable = true };
        RemoveParam.OnClicked = DeleteParameter;

        var typeEnum = new EditorEnumField() { Name = "Paramater Type", Position = new Rect(0, 0, 300, 25), Value = TypeToAdd };
        var newParamName = new EditorStringField()
        {
            Name = "Paramater Name",
            Position = new Rect(0, 0, 300, 25),
            Value = ""
        };

        AddParamContainer.AddControl(RemoveParam);
        AddParamContainer.AddControl(AddParam);
        AddParamContainer.AddControl(typeEnum);
        AddParamContainer.AddControl(newParamName);


        ControlsContianer.AddControl(ActiveMachineContainer);
        ControlsContianer.AddControl(ParamContainer);
        ControlsContianer.AddControl(AddParamContainer);
    }

    private void OnGUI()
    {        
        ValidateControls();
        DrawControls();

        if (StateRef != null)
            EditorUtility.SetDirty(StateRef);
        EditorUtility.SetDirty(this);
    }    

    public void DrawControls()
    {
        ControlsContianer.Position.size = new Vector2(ControlsContianer.Position.size.x, maxSize.y);

        DrawParameters();

        if (StateRef != null)
            foreach(var p in StateRef.Parameters)
            {
                if (p.Value.GetType() == typeof(Visual_Test.IntParameter))
                    ((Visual_Test.IntParameter)p.Value).SetValue(((EditorIntField)ParamContainer.GetControl(p.Key)).Value);
                if (p.Value.GetType() == typeof(Visual_Test.StringParameter))
                    ((Visual_Test.StringParameter)p.Value).SetValue(((EditorStringField)ParamContainer.GetControl(p.Key)).Value);
                if (p.Value.GetType() == typeof(Visual_Test.BooleanParamater))
                    ((Visual_Test.BooleanParamater)p.Value).SetValue(((EditorBooleanField)ParamContainer.GetControl(p.Key)).Value);
                if (p.Value.GetType() == typeof(Visual_Test.FloatParameter))
                    ((Visual_Test.FloatParameter)p.Value).SetValue(((EditorFloatField)ParamContainer.GetControl(p.Key)).Value);
            }

        if(((EditorObjectField)ActiveMachineContainer.GetControl("State Machine")).Value != null)
            StateRef = ((EditorObjectField)ActiveMachineContainer.GetControl("State Machine")).Value as StateMachine;
        if ((EditorButton)AddParamContainer.GetControl("Add Param") != null)
            ((EditorButton)AddParamContainer.GetControl("Add Param")).IsInteractable = ParamName != "";
        if((EditorStringField)AddParamContainer.GetControl("Paramater Name") != null)
            ParamName = ((EditorStringField)AddParamContainer.GetControl("Paramater Name")).Value;
        if((EditorEnumField)AddParamContainer.GetControl("Paramater Type") != null)
            TypeToAdd = (ParamType)((EditorEnumField)AddParamContainer.GetControl("Paramater Type")).Value;
        
        ControlsContianer.Draw(Rect.zero);          
    }

    void DrawParameters()
    {

        if (StateRef != null)
        {
            foreach (var p in StateRef.Parameters)
            {
                if (p.Value.GetType() == typeof(Visual_Test.IntParameter))
                {
                    EditorIntField intField = new EditorIntField()
                    {
                        Name = p.Key,
                        Position = new Rect(0, 0, 300, 25),                          
                        Value = ((Visual_Test.IntParameter)p.Value).InternalValue,
                        OnInteract = ControlLastInteracted
                    };
                    ParamContainer.AddControl(intField);
                    p.Value.SetValue(((EditorIntField)ParamContainer.GetControl(p.Key)).Value);
                }
                if (p.Value.GetType() == typeof(Visual_Test.FloatParameter))
                {
                    var floatField = new EditorFloatField()
                    {
                        Name = p.Key,
                        Position = new Rect(0, 0, 300, 25),
                        Value = ((Visual_Test.FloatParameter)p.Value).InternalValue,
                        OnInteract = ControlLastInteracted
                    };                    
                    ParamContainer.AddControl(floatField);
                    p.Value.SetValue(((EditorFloatField)ParamContainer.GetControl(p.Key)).Value);
                }
                if (p.Value.GetType() == typeof(Visual_Test.StringParameter))
                {
                    var stringField = new EditorStringField()
                    {
                        Name = p.Key,
                        Position = new Rect(0, 0, 300, 25),
                        Value = ((Visual_Test.StringParameter)p.Value).InternalValue,
                        OnInteract = ControlLastInteracted
                    };                    
                    ParamContainer.AddControl(stringField);
                    p.Value.SetValue(((EditorStringField)ParamContainer.GetControl(p.Key)).Value);
                }
                if (p.Value.GetType() == typeof(Visual_Test.BooleanParamater))
                {
                    var boolField = new EditorBooleanField()
                    {
                        Name = p.Key,
                        Position = new Rect(0, 0, 300, 25),
                        Value = ((Visual_Test.BooleanParamater)p.Value).InternalValue,
                        OnInteract = ControlLastInteracted
                    };                    
                    ParamContainer.AddControl(boolField);
                    p.Value.SetValue(((EditorBooleanField)ParamContainer.GetControl(p.Key)).Value);
                }

            }
        }
    }

    /// <summary>
    /// If any of the core controls are null create all new controls
    /// </summary>
    void ValidateControls()
    {
        if(ControlsContianer == null || ParamContainer == null || AddParamContainer == null || ActiveMachineContainer == null)
        {
            ControlsContianer = new GUIContianiner()
            {
                Name = "Container",
                Position = new Rect(0, 0, 325, maxSize.y),
                ControlPadding = new Vector2(5, 5)
            };

            ActiveMachineContainer = new AutoResizeContainer()
            {
                Name = "Active Machine",
                Position = new Rect(0, 0, 315, 100),
                ControlPadding = new Vector2(5, 5),
                DrawColor = Color.cyan
            };

            var newObjectField = new EditorObjectField()
            {
                Name = "State Machine",
                Position = new Rect(0, 0, 300, 25),
                AcceptableType = typeof(StateMachine)
            };

            ActiveMachineContainer.AddControl(newObjectField);

            ParamContainer = new ScrollContainer()
            {
                Name = "Parameters",
                Position = new Rect(0, 0, 315, 200),
                DrawColor = Color.blue,
                ControlPadding = new Vector2(5, 5)
            };
            AddParamContainer = new AutoResizeContainer()
            {
                Name = "Add Param Container",
                Position = new Rect(0, 0, 315, 100),
                ControlPadding = new Vector2(5, 5),
                DrawColor = Color.green
            };
            var AddParam = new EditorButton() { Name = "Add Param", Position = new Rect(0, 0, 125, 25), Text = "Add Parameter" };
            AddParam.OnClicked = AddParameter;
            var RemoveParam = new EditorButton() { Name = "Delte Param", Position = new Rect(0, 0, 125, 25), Text = "Delete Parameter", IsInteractable = true };
            RemoveParam.OnClicked = DeleteParameter;

            var typeEnum = new EditorEnumField() { Name = "Paramater Type", Position = new Rect(0, 0, 300, 25), Value = TypeToAdd };
            var newParamName = new EditorStringField()
            {
                Name = "Paramater Name",
                Position = new Rect(0, 0, 300, 25),
                Value = ""
            };

            AddParamContainer.AddControl(RemoveParam);
            AddParamContainer.AddControl(AddParam);
            AddParamContainer.AddControl(typeEnum);
            AddParamContainer.AddControl(newParamName);

            ControlsContianer.AddControl(ActiveMachineContainer);
            ControlsContianer.AddControl(ParamContainer);
            ControlsContianer.AddControl(AddParamContainer);            
        }
    }

    public void ControlLastInteracted(Control control)
    {        
        LastParamInteracted = control;
    }

    public static void DeleteParameter()
    {
        if (LastParamInteracted == null)
            return;
        ParamContainer.RemoveControl(LastParamInteracted.Name);
        StateRef.RemoveParamter(LastParamInteracted.Name);
    }

    public static void AddParameter()
    {
        if (StateRef == null)
            return;
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

