using System;
using System.Collections.Generic;
using System.Threading;
 
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
 
using Cysharp.Threading.Tasks;
 
public class ScrollView : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    private const float Edge
    = 0.001f;
 
    private bool Ready
    = false;
    private bool Wait
    = false;
    private bool Cancellation
    = false;
    private bool Dragging
    = false;
    private Direction Running
    = Direction.Down;
    private float Previous
    = -1f;
 
    // The items that have a row right now are Top to Bottom (index in Keys).
    private int Top
    = 0;
    private int Bottom
    = -1;
 
    [Tooltip("Rows loaded each time you reach the top or the bottom. The list keeps up to 2 x Rows rows.")]
    public int Rows = 0;
 
    [Tooltip("Keep the rows you are looking at in the same place when new rows load.")]
    public bool KeepPosition = true;
 
    public int Length { get; private set; } = 0;
 
    // Func(item, row, up) fills the row with the item.
    // up is true when the row is added above the others.
    // The row is inactive when Func is called, so Func has to activate it.
    public Func<object, GameObject, Boolean, UniTask> Func;
 
    private ScrollRect ScrollRect;
    private RectTransform Content;
    private Transform Template;
    private CancellationToken Destroyed;
 
    private List<object> Keys
    = new List<object>();
    private HashSet<object> Known
    = new HashSet<object>();
    private List<GameObject> Pool
    = new List<GameObject>();
 
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
 
    private void Start()
    {
        Setup();
    }
 
    private void OnDisable()
    {
        Dragging
        = false;
    }
 
    private void OnDestroy()
    {
        Cancellation
        = true;
 
        if (ScrollRect != null)
        {
            ScrollRect
            .onValueChanged
            .RemoveListener
            (Scroll);
        }
    }
 
    public void OnBeginDrag
    (PointerEventData PData)
    {
        Dragging
        = true;
    }
 
    public void OnEndDrag
    (PointerEventData PData)
    {
        Dragging
        = false;
    }
 
    private bool Setup()
    {
        if (Ready)
        { return true; }
 
        ScrollRect
        = this
        .GetComponent
        <ScrollRect>();
 
        if (ScrollRect == null
        || ScrollRect.content == null)
        {
            Debug
            .LogError
            ("ScrollView needs a ScrollRect with a Content on the same object.", this);
 
            return false;
        }
 
        Content
        = ScrollRect
        .content;
 
        foreach (Transform Child
        in Content)
        {
            bool Found
            = Child
            .name
            .StartsWith
            ("Template",
            StringComparison.Ordinal);
 
            if (Found)
            {
                Template
                = Child;
                break;
            }
        }
 
        if (Template == null)
        {
            Debug
            .LogError
            ("ScrollView needs a child named Template inside Content.", this);
 
            return false;
        }
 
        Destroyed
        = this
        .GetCancellationTokenOnDestroy();
 
        ScrollRect
        .onValueChanged
        .AddListener
        (Scroll);
 
        Ready
        = true;
 
        return true;
    }
 
    // Runs when the ScrollRect moves. Only reacts when the value changes
    // and only when you are at the top or the bottom.
    private void Scroll
    (Vector2 PValue)
    {
        float Float
        = Mathf
        .Clamp01
        (PValue.y);
 
        if (Mathf.Approximately(Float, Previous))
        { return; }
 
        Previous
        = Float;
 
        if (Float >= 1f - Edge)
        {
            bool More
            = Top > 0
            && Busy(Direction.Up) == false;
 
            if (More)
            {
                Bind
                (Direction.Up)
                .Forget();
            }
        }
        else if (Float <= Edge)
        {
            bool More
            = Bottom < Keys.Count - 1
            && Busy(Direction.Down) == false;
 
            if (More)
            {
                Bind
                (Direction.Down)
                .Forget();
            }
        }
    }
 
    // True when a build for this direction is already running.
    private bool Busy
    (Direction PDirection)
    {
        return Wait
        && Cancellation == false
        && Running == PDirection;
    }
 
    public async UniTask Bind
    (Direction PDirection)
    {
        bool Skip
        = PDirection != Direction.Up
        && PDirection != Direction.Down;
 
        if (Skip || Setup() == false)
        { return; }
 
        if (Busy(PDirection))
        {
            await UniTask
            .WaitUntil(() =>
            Wait == false,
            cancellationToken: Destroyed);
 
            return;
        }
 
        if (Wait)
        {
            Cancellation
            = true;
 
            await UniTask
            .WaitUntil(() =>
            Wait == false,
            cancellationToken: Destroyed);
        }
 
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
 
        Running
        = PDirection;
 
        try
        {
            await UniTask
            .NextFrame
            (Destroyed);
 
            await Load
            (PDirection == Direction.Up);
        }
        finally
        {
            Wait
            = false;
        }
    }
 
    // Down: bind the next Rows items below the window.
    // Up: bind the Rows items above the window.
    // New rows are made until there are 2 x Rows, after that the rows
    // on the far side are moved to the near side and bound again.
    private async UniTask Load
    (bool PUp)
    {
        bool Missing
        = Rows <= 0
        || Func == null;
 
        if (Missing)
        {
            Debug
            .LogWarning
            ("ScrollView: set Rows (more than 0) and Func first.", this);
 
            return;
        }
 
        int Capacity
        = Rows
        * 2;
 
        for (int i = 0; i < Rows; i++)
        {
            // Dragging with a finger or mouse fights with the position fix,
            // so wait until it is released.
            bool Hold
            = KeepPosition
            && Dragging;
 
            if (Hold)
            {
                await UniTask
                .WaitWhile(() =>
                Dragging
                && Cancellation == false,
                cancellationToken: Destroyed);
            }
 
            if (Cancellation)
            { break; }
 
            int Index
            = PUp
            ? Top - 1
            : Bottom + 1;
 
            bool Finished
            = Index < 0
            || Index >= Keys.Count;
 
            if (Finished)
            { break; }
 
            // A row that stays, used to keep the view in the same place.
            Transform Anchor
            = null;
 
            Vector3 Before
            = Vector3
            .zero;
 
            if (Pool.Count > 0)
            {
                Anchor
                = PUp
                ? Pool[0].transform
                : Pool[Pool.Count - 1].transform;
 
                Before
                = Content
                .InverseTransformPoint
                (Anchor.position);
            }
 
            GameObject View
            = null;
 
            if (Pool.Count < Capacity)
            {
                View
                = Instantiate(
                Template
                .gameObject,
                Content);
            }
            else if (PUp)
            {
                View
                = Pool
                [Pool.Count - 1];
 
                Pool
                .RemoveAt
                (Pool.Count - 1);
 
                Bottom--;
            }
            else
            {
                View
                = Pool
                [0];
 
                Pool
                .RemoveAt
                (0);
 
                Top++;
            }
 
            Release
            (View);
 
            if (PUp)
            {
                Pool
                .Insert
                (0, View);
 
                Top--;
            }
            else
            {
                Pool
                .Add
                (View);
 
                Bottom++;
            }
 
            if (Anchor != null)
            {
                Place
                (View.transform,
                Anchor,
                PUp == false);
            }
 
            await Func(
            Keys[Index],
            View,
            PUp);
 
            if (KeepPosition
            && Anchor != null)
            {
                LayoutRebuilder
                .ForceRebuildLayoutImmediate
                (Content);
 
                Vector3 After
                = Content
                .InverseTransformPoint
                (Anchor.position);
 
                Content.position
                -= Content
                .TransformVector
                (After - Before);
            }
 
            await UniTask
            .NextFrame
            (Destroyed);
        }
    }
 
    // Puts a row right after (or right before) another row in the hierarchy.
    private void Place
    (Transform PView,
    Transform PNeighbor,
    bool PAfter)
    {
        int View
        = PView
        .GetSiblingIndex();
 
        int Neighbor
        = PNeighbor
        .GetSiblingIndex();
 
        int Index
        = Neighbor;
 
        if (PAfter)
        {
            Index
            = View < Neighbor
            ? Neighbor
            : Neighbor + 1;
        }
        else
        {
            Index
            = View < Neighbor
            ? Neighbor - 1
            : Neighbor;
        }
 
        PView
        .SetSiblingIndex
        (Index);
    }
 
    // Hides the row and removes the listeners the old item put on it.
    private void Release
    (GameObject PView)
    {
        PView
        .SetActive(false);
 
        Selectable[] Selectables
        = PView
        .GetComponentsInChildren
        <Selectable>
        (true);
 
        foreach (Selectable Selectable
        in Selectables)
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
 
    public UniTask Add
    (object PObject)
    {
        bool Added
        = PObject != null
        && Known.Add(PObject);
 
        if (Added)
        {
            Keys
            .Add(PObject);
 
            Length
            = Keys
            .Count;
        }
 
        return UniTask
        .CompletedTask;
    }
 
    public async UniTask Clear()
    {
        if (Wait)
        {
            Cancellation
            = true;
 
            await UniTask
            .WaitUntil(() =>
            Wait == false,
            cancellationToken: Destroyed);
        }
 
        foreach (GameObject View
        in Pool)
        {
            if (View != null)
            {
                View
                .SetActive(false);
 
                Destroy
                (View);
            }
        }
 
        Pool
        .Clear();
 
        Keys
        .Clear();
 
        Known
        .Clear();
 
        Top
        = 0;
 
        Bottom
        = -1;
 
        Length
        = 0;
 
        Previous
        = -1f;
    }
}
 