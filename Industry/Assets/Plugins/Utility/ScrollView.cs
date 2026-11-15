using System;
using System.Collections.Generic;
using System.Threading;
 
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
 
using Cysharp.Threading.Tasks;

// Works with or without a ScrollRect.
//
// With a ScrollRect (that has a Content) on the same object, the ScrollRect does the
// scrolling and this script only watches it.
//
// Without one, this script scrolls the Content itself: drag, mouse wheel, inertia and
// elastic ends. If Content is not set it is the parent of the child named Template, and
// if Viewport is not set it is the parent of the Content.
public class ScrollView : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler, IInitializePotentialDragHandler
{
    private const float Edge
    = 0.001f;
 
    [HideInInspector]
    private bool Ready
    = false;
    private bool Wait
    = false;
    private bool Cancellation
    = false;
    private bool Dragging
    = false;
    private bool Standalone
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
 
    // Only used without a ScrollRect. Every number is on the up and down axis of the Viewport.
    private float Velocity
    = 0f;
    private float Moved
    = 0f;
    private float Pointer
    = 0f;
    private float ContentTop
    = 0f;
    private float ContentBottom
    = 0f;
    private float ViewTop
    = 0f;
    private float ViewBottom
    = 0f;
    private readonly Vector3[] Corners
    = new Vector3[4];
 
    [Tooltip("Rows loaded each time you reach the top or the bottom. The list keeps up to 2 x Rows rows.")]
    public int Rows = 0;
 
    [Tooltip("Keep the rows you are looking at in the same place when new rows load.")]
    public bool KeepPosition = true;
 
    [Header("Without a ScrollRect")]
    [Tooltip("The area the Content moves in. Empty means the parent of the Content.")]
    public RectTransform Viewport;
 
    [Tooltip("Adds a clear Image (so drags are caught) and a RectMask2D (so rows are clipped) when they are missing.")]
    public bool AutoSetup = true;
 
    [Tooltip("How far one step of the mouse wheel moves the list. A step is about 1 with the old Input Manager and about 6 with the new Input System.")]
    public float Wheel = 20f;
 
    [Tooltip("The list keeps moving after you let go.")]
    public bool Inertia = true;
 
    [Tooltip("How much of the speed is left after 1 second. 0 stops at once, 1 never slows down.")]
    [Range(0f, 1f)]
    public float Deceleration = 0.135f;
 
    [Tooltip("The list can be pulled past the top and the bottom and springs back.")]
    public bool Elastic = true;
 
    [Tooltip("Seconds the list needs to spring back.")]
    public float Elasticity = 0.1f;
 
    public int Length { get; private set; } = 0;
 
    // Func(item, row, up) fills the row with the item.
    // up is true when the row is added above the others.
    // The row is inactive when Func is called, so Func has to activate it.
    public Func<object, GameObject, Boolean, ScrollView, UniTask> Func;
 
    private ScrollRect ScrollRect;
 
    [Tooltip("The object that holds the rows. A ScrollRect gives it. Without a ScrollRect, set it here or leave it empty to use the parent of the child named Template.")]
    public RectTransform Content;
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
 
        Velocity
        = 0f;
 
        Moved
        = 0f;
    }
 
    private void OnDestroy()
    {
        Cancellation
        = true;
 
        if (Standalone == false
        && ScrollRect != null)
        {
            ScrollRect
            .onValueChanged
            .RemoveListener
            (Scroll);
        }
    }
 
    // Only used without a ScrollRect: inertia, springing back,
    // and telling Check where the list is.
    private void LateUpdate()
    {
        float Step
        = Time
        .unscaledDeltaTime;
 
        bool Skip
        = Ready == false
        || Standalone == false
        || Step <= 0f
        || Content == null
        || Viewport == null;
 
        if (Skip)
        { return; }
 
        Measure();
 
        // How far the list is past the top or the bottom (0 when it is inside).
        float Gap
        = Outside(0f);
 
        if (Dragging == false
        && (Gap != 0f
        || Velocity != 0f))
        {
            float Move
            = 0f;
 
            if (Elastic
            && Gap != 0f)
            {
                float Speed
                = Velocity;
 
                Move
                = Mathf
                .SmoothDamp
                (0f,
                Gap,
                ref Speed,
                Elasticity,
                Mathf.Infinity,
                Step);
 
                if (Mathf.Abs(Speed) < 1f)
                { Speed = 0f; }
 
                Velocity
                = Speed;
            }
            else if (Inertia)
            {
                Velocity
                *= Mathf
                .Pow
                (Deceleration,
                Step);
 
                if (Mathf.Abs(Velocity) < 1f)
                { Velocity = 0f; }
 
                Move
                = Velocity
                * Step;
            }
            else
            {
                Velocity
                = 0f;
            }
 
            if (Elastic == false)
            {
                // Without elastic ends the list stops at the top and the bottom.
                float Back
                = Outside(Move);
 
                if (Back != 0f)
                {
                    Move
                    += Back;
 
                    Velocity
                    = 0f;
                }
            }
 
            Shift
            (Move);
        }
 
        // The speed you let go with is the speed of the last moments of the drag.
        if (Dragging
        && Inertia)
        {
            Velocity
            = Mathf
            .Lerp
            (Velocity,
            Moved / Step,
            Step * 10f);
        }
 
        Moved
        = 0f;
 
        Check
        (Value());
    }
 
    public void OnInitializePotentialDrag
    (PointerEventData PData)
    {
        bool Stop
        = Standalone
        && PData.button == PointerEventData.InputButton.Left;
 
        // Touching the list stops it.
        if (Stop)
        {
            Velocity
            = 0f;
        }
    }
 
    public void OnBeginDrag
    (PointerEventData PData)
    {
        Dragging
        = true;
 
        bool Skip
        = Standalone == false
        || PData.button != PointerEventData.InputButton.Left;
 
        if (Skip)
        { return; }
 
        Velocity
        = 0f;
 
        Moved
        = 0f;
 
        Point
        (PData,
        out Pointer);
    }
 
    // Only used without a ScrollRect. Moves the list by how far the pointer moved
    // since the last call, so the position fix of KeepPosition is never overwritten.
    public void OnDrag
    (PointerEventData PData)
    {
        bool Skip
        = Standalone == false
        || Dragging == false
        || PData.button != PointerEventData.InputButton.Left;
 
        float Local
        = 0f;
 
        if (Skip
        || Point(PData, out Local) == false)
        { return; }
 
        float Delta
        = Local
        - Pointer;
 
        Pointer
        = Local;
 
        Measure();
 
        float Gap
        = Outside(0f);
 
        if (Elastic)
        {
            // Pulling further past the top or the bottom gets harder the further you are past it.
            if (Gap * Delta < 0f)
            {
                float Free
                = Mathf
                .Clamp01
                (1f
                - Mathf.Abs(Gap)
                / Mathf.Max(ViewTop - ViewBottom, 1f));
 
                Delta
                *= 0.55f
                * Free
                * Free;
            }
        }
        else
        {
            Delta
            += Outside(Delta);
        }
 
        Shift
        (Delta);
 
        Moved
        += Delta;
    }
 
    public void OnEndDrag
    (PointerEventData PData)
    {
        Dragging
        = false;
    }
 
    // Only used without a ScrollRect. The wheel never moves the list past the top or the bottom.
    public void OnScroll
    (PointerEventData PData)
    {
        if (Standalone == false)
        { return; }
 
        Measure();
 
        // Wheel up moves the list down, so you see what is above.
        float Delta
        = -PData.scrollDelta.y
        * Wheel;
 
        Delta
        += Outside(Delta);
 
        Velocity
        = 0f;
 
        Shift
        (Delta);
    }
 
    private bool Setup()
    {
        if (Ready)
        { return true; }
 
        ScrollRect
        = this
        .GetComponent
        <ScrollRect>();
 
        // A ScrollRect that has a Content does the scrolling.
        // Without one this script does the scrolling itself.
        Standalone
        = ScrollRect == null
        || ScrollRect.content == null;
 
        if (Standalone
        && ScrollRect != null)
        {
            Debug
            .LogWarning
            ("ScrollView: the ScrollRect has no Content, so the list is scrolled without it.", this);
        }
 
        if (Standalone == false)
        {
            Content
            = ScrollRect
            .content;
        }
        else if (Content == null)
        {
            Content
            = Search();
        }
 
        if (Content == null)
        {
            Debug
            .LogError
            ("ScrollView needs a ScrollRect with a Content, or a Content that has a child named Template.", this);
 
            return false;
        }
 
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
 
        if (Standalone
        && Prepare() == false)
        { return false; }
 
        Destroyed
        = this
        .GetCancellationTokenOnDestroy();
 
        if (Standalone == false)
        {
            ScrollRect
            .onValueChanged
            .AddListener
            (Scroll);
        }
 
        Ready
        = true;
 
        return true;
    }
 
    // Only used without a ScrollRect: the Content is the parent of the child named Template.
    private RectTransform Search()
    {
        foreach (Transform Child
        in this
        .GetComponentsInChildren
        <Transform>(true))
        {
            bool Found
            = Child != this.transform
            && Child.parent != this.transform
            && Child
            .name
            .StartsWith
            ("Template",
            StringComparison.Ordinal);
 
            if (Found)
            {
                return Child
                .parent
                as RectTransform;
            }
        }
 
        return null;
    }
 
    // Only used without a ScrollRect: finds the Viewport and adds what a list needs
    // to be clipped and to get the drag and wheel events.
    private bool Prepare()
    {
        if (Viewport == null)
        {
            Viewport
            = Content
            .parent
            as RectTransform;
        }
 
        if (Viewport == null)
        {
            Viewport
            = this
            .transform
            as RectTransform;
        }
 
        if (Viewport == null)
        {
            Debug
            .LogError
            ("ScrollView needs a RectTransform to move the Content in.", this);
 
            return false;
        }
 
        if (AutoSetup)
        {
            bool Blank
            = this
            .GetComponent
            <Graphic>() == null;
 
            if (Blank)
            {
                Image Catcher
                = this
                .gameObject
                .AddComponent
                <Image>();
 
                Catcher.color
                = Color.clear;
            }
 
            bool Unclipped
            = Viewport
            .GetComponent
            <RectMask2D>() == null
            && Viewport
            .GetComponent
            <Mask>() == null;
 
            if (Unclipped)
            {
                Viewport
                .gameObject
                .AddComponent
                <RectMask2D>();
            }
        }
 
        return true;
    }
 
    // Runs when the ScrollRect moves.
    private void Scroll
    (Vector2 PValue)
    {
        Check
        (PValue.y);
    }
 
    // PValue is 1 at the top and 0 at the bottom. Only reacts when the value
    // changes and only when you are at the top or the bottom.
    private void Check
    (float PValue)
    {
        float Float
        = Mathf
        .Clamp01
        (PValue);
 
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
            // A ScrollRect sets the position from where the drag began, so dragging
            // fights with the position fix and we wait until it is released.
            // The own scrolling only moves by the change of the pointer, so it does not wait.
            bool Hold
            = KeepPosition
            && Dragging
            && Standalone == false;
 
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
            PUp,
            this);
 
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
 
        Velocity
        = 0f;
    }
 
    // Only used without a ScrollRect. Puts the Content and the Viewport on the
    // same axis: the top and the bottom of both as numbers.
    // A Content that is shorter than the Viewport is stretched around its pivot
    // to the height of the Viewport, the same as ScrollRect does,
    // so a short list stays at the top when its pivot is at the top.
    private void Measure()
    {
        Rect Area
        = Viewport
        .rect;
 
        ViewTop
        = Area
        .yMax;
 
        ViewBottom
        = Area
        .yMin;
 
        Content
        .GetWorldCorners
        (Corners);
 
        float Low
        = float.MaxValue;
 
        float High
        = float.MinValue;
 
        for (int i = 0; i < 4; i++)
        {
            float Y
            = Viewport
            .InverseTransformPoint
            (Corners[i])
            .y;
 
            Low
            = Mathf.Min(Low, Y);
 
            High
            = Mathf.Max(High, Y);
        }
 
        float Height
        = ViewTop
        - ViewBottom;
 
        float Excess
        = Height
        - (High - Low);
 
        if (Excess > 0f)
        {
            float Middle
            = (High + Low)
            * 0.5f
            - Excess
            * (Content.pivot.y - 0.5f);
 
            High
            = Middle
            + Height
            * 0.5f;
 
            Low
            = Middle
            - Height
            * 0.5f;
        }
 
        ContentTop
        = High;
 
        ContentBottom
        = Low;
    }
 
    // How far the list has to move to be inside the Viewport again,
    // after it moved PDelta. Up is positive, down is negative, 0 is inside.
    private float Outside
    (float PDelta)
    {
        float Up
        = ViewTop
        - (ContentTop + PDelta);
 
        float Down
        = ViewBottom
        - (ContentBottom + PDelta);
 
        if (Up > 0.001f)
        { return Up; }
 
        if (Down < -0.001f)
        { return Down; }
 
        return 0f;
    }
 
    // 1 is the top and 0 is the bottom, the same as the value of a ScrollRect.
    // A list that fits in the Viewport has nothing to scroll and gives 0.
    private float Value()
    {
        float Travel
        = (ContentTop - ContentBottom)
        - (ViewTop - ViewBottom);
 
        if (Travel <= 0.001f)
        { return 0f; }
 
        return Mathf
        .Clamp01
        ((ViewBottom - ContentBottom)
        / Travel);
    }
 
    // Moves the Content up (positive) or down (negative) inside the Viewport.
    private void Shift
    (float PDelta)
    {
        if (PDelta == 0f)
        { return; }
 
        Content.position
        += Viewport
        .TransformVector
        (0f, PDelta, 0f);
 
        ContentTop
        += PDelta;
 
        ContentBottom
        += PDelta;
    }
 
    // The up and down position of the pointer inside the Viewport.
    private bool Point
    (PointerEventData PData,
    out float PY)
    {
        Vector2 Local;
 
        bool Hit
        = RectTransformUtility
        .ScreenPointToLocalPointInRectangle
        (Viewport,
        PData.position,
        PData.pressEventCamera,
        out Local);
 
        PY
        = Local.y;
 
        return Hit;
    }
}