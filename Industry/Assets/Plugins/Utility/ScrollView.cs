using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Cysharp.Threading.Tasks;

public class ScrollView : MonoBehaviour
{
    private bool Wait
    = false;
    private bool Cancellation
    = false;
    public int Rows = 0;
    public int Length { get; private set; } = 0;
    private Dictionary<object, GameObject> Data
    = new Dictionary<object, GameObject>();

    public Func<object, GameObject, UniTask> Func;

    private async UniTask Start()
    {
        ScrollRect ScrollRect
        = this
        .GetComponent
        <ScrollRect>();

        ScrollRect
        .verticalScrollbar
        .onValueChanged
        .AddListener(async 
        (value) =>
        {
            float Float
            = value;

            {
                bool Boolean
                = Float
                >= 1f;

                switch (Boolean)
                {
                    case true:
                        {
                            await Bind
                            (Direction
                            .Up);
                        }
                        break;
                }
            }

            {
                bool Boolean
                = Float
                <= 0f;

                switch (Boolean)
                {
                    case true:
                        {
                            await Bind
                            (Direction
                            .Down);
                        }
                        break;
                }
            }
        });
    }

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public async UniTask Bind
    (Direction PDirection)
    {
        Cancellation
        = true;

        await UniTask
        .WaitUntil(()=> Wait
        == false);

        await Build
        (PDirection);
    }

    private async UniTask Build
    (Direction PDirection)
    {
        Cancellation
        = false;

        Wait
        = true;

        await UniTask
        .NextFrame();

        Transform Transform
        = this
        .GetComponent
        <ScrollRect>()
        .content
        .transform;

        GameObject GameObject
        = null;

        switch (PDirection)
        {
            case Direction
            .Down:
                {
                    int Offset
                    = 0;

                    int Columns
                    = Length;

                    {
                        for (int i = Columns - 1; i >= 0; i--)
                        {   
                            bool Boolean
                            = Data
                            .ElementAt(i)
                            .Value
                            != null;

                            if (Boolean)
                            {
                                Offset
                                = i;
                                break;
                            }
                        }
                    }

                    {
                        Columns
                        = Rows;

                        Columns
                        += Offset;

                        Offset
                        = Math
                        .Min(
                        Offset,
                        Length);

                        Columns
                        = Math
                        .Min(
                        Columns,
                        Length);

                        for (int i = Offset; i < Columns; i++)
                        {
                            if (Cancellation)
                            { break; }

                            KeyValuePair
                            <object, GameObject> 
                            KeyValuePair
                            = Data
                            .ElementAt(i);

                            foreach (Transform _Transform
                            in Transform)
                            {
                                await UniTask
                                .NextFrame();

                                string String
                                = _Transform
                                .gameObject
                                .name;

                                bool Boolean
                                = Length
                                < (Rows * 2);

                                if (Boolean)
                                {
                                    Boolean
                                    = String
                                    .StartsWith
                                    ("Template");

                                    if ( Boolean)
                                    {
                                        GameObject
                                        = Instantiate(
                                        _Transform
                                        .gameObject,
                                        Transform);
                                        break;
                                    }   
                                }
                                else
                                {
                                    Boolean
                                    = new string[]
                                    {
                                        "Template",
                                        "Progress"
                                    }.Any(
                                    x => String
                                    .StartsWith(x)
                                    == false);

                                    if (Boolean)
                                    {
                                        GameObject
                                        = _Transform
                                        .gameObject;

                                        {
                                            for (int x = 0; x < Length; x++)
                                            {
                                                KeyValuePair
                                                <object, GameObject> 
                                                _KeyValuePair
                                                = Data
                                                .ElementAt(x);

                                                Boolean
                                                = GameObject
                                                == _KeyValuePair
                                                .Value;

                                                if (Boolean)
                                                {
                                                    Data
                                                    [_KeyValuePair
                                                    .Key]
                                                    = null;
                                                    break;
                                                }
                                            }
                                        }

                                        break;
                                    }
                                }
                            }

                            {                                        
                                {
                                    Data
                                    [KeyValuePair
                                    .Key] 
                                    = GameObject;

                                    GameObject
                                    .SetActive(false);
                                }

                                {
                                    Array Array
                                    = GameObject.GetComponentsInChildren
                                    <Selectable>
                                    (true);

                                    foreach (Selectable Selectable
                                    in Array)
                                    {
                                        switch (Selectable)
                                        {
                                            case Button Button:
                                                Button
                                                .onClick
                                                .RemoveAllListeners();
                                                break;

                                            case Toggle Toggle:
                                                Toggle
                                                .onValueChanged
                                                .RemoveAllListeners();
                                                break;

                                            case Slider Slider:
                                                Slider
                                                .onValueChanged
                                                .RemoveAllListeners();
                                                break;

                                            case Scrollbar Scrollbar:
                                                Scrollbar
                                                .onValueChanged
                                                .RemoveAllListeners();
                                                break;

                                            case Dropdown Dropdown:
                                                Dropdown
                                                .onValueChanged
                                                .RemoveAllListeners();
                                                break;
                                        }
                                    }
                                }

                                {
                                    object Object 
                                    = KeyValuePair
                                    .Key;

                                    await  Func(
                                    Object,
                                    GameObject);
                                }
                            }            
                        }
                    }
                }
                break;
        }

        Wait
        = false;
    }

    public async UniTask Add
    (object PObject)
    {
        bool Boolean
        = Data
        .ContainsKey
        (PObject)
        == false;

        switch (Boolean)
        {
            case true:
                {
                    Data
                    .Add(PObject,
                    null);

                    Length
                    = Data
                    .Count;
                }
                break;
        }

        await 
        UniTask
        .CompletedTask;
    }

    public async UniTask Clear()
    {
        // for (int i = Length; i >= 0; i--)
        // {
        //     if (Cancellation)
        //     { break; }

        //     Data
        //     .RemoveAt(i);

        //     Length--;
        // }
    }
}
