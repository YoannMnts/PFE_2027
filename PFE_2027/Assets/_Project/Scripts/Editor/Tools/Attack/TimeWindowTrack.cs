using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PFE.Editor._Project.Scripts.Editor.Tools.Attack
{
    // A timeline track: the bar of a TimeWindow (normalized start/end).
    // Dragging the bar moves it, dragging one of the edge handles stretches or shortens it.
    public sealed class TimeWindowTrack : VisualElement
    {
        private enum DragMode { None, Move, Start, End }

        private readonly SerializedProperty start;
        private readonly SerializedProperty end;
        private readonly int totalFrames;
        private readonly VisualElement bar;
        private readonly VisualElement startGrip;
        private readonly VisualElement endGrip;
        private readonly VisualElement playhead;

        private DragMode dragMode;
        private float grabOffset;
        private int undoGroup;

        public TimeWindowTrack(SerializedProperty window, Color color, int totalFrames)
        {
            start = window.FindPropertyRelative(AttackPropertyPaths.Start);
            end = window.FindPropertyRelative(AttackPropertyPaths.End);
            this.totalFrames = Mathf.Max(1, totalFrames);

            AddToClassList("attack-track");

            bar = new VisualElement();
            bar.AddToClassList("attack-track__bar");
            bar.style.backgroundColor = color;
            Add(bar);

            // Edge handles, children of the bar: they follow both of its ends
            startGrip = CreateGrip("attack-track__grip--start");
            endGrip = CreateGrip("attack-track__grip--end");
            bar.Add(startGrip);
            bar.Add(endGrip);

            playhead = new VisualElement();
            playhead.AddToClassList("attack-track__playhead");
            playhead.pickingMode = PickingMode.Ignore;
            Add(playhead);

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);

            Refresh();
        }

        private static VisualElement CreateGrip(string sideClass)
        {
            VisualElement grip = new();
            grip.AddToClassList("attack-track__grip");
            grip.AddToClassList(sideClass);
            grip.tooltip = "Drag to resize";
            return grip;
        }

        public void Refresh()
        {
            bar.style.left = Length.Percent(start.floatValue * 100f);
            bar.style.width = Length.Percent(Mathf.Max(0f, end.floatValue - start.floatValue) * 100f);
        }

        public void SetPlayhead(float normalizedTime)
            => playhead.style.left = Length.Percent(Mathf.Clamp01(normalizedTime) * 100f);

        private float ToTime(float localX) => layout.width > 0f ? Mathf.Clamp01(localX / layout.width) : 0f;

        private float Snap(float t) => Mathf.Round(t * totalFrames) / totalFrames;

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0)
                return;

            start.serializedObject.Update();

            // Clicks on the bar and its handles bubble up to the track: evt.target tells what was clicked
            if (evt.target == startGrip)
                dragMode = DragMode.Start;
            else if (evt.target == endGrip)
                dragMode = DragMode.End;
            else if (evt.target == bar)
            {
                dragMode = DragMode.Move;
                grabOffset = ToTime(evt.localPosition.x) - start.floatValue;
            }
            else
                return;

            undoGroup = Undo.GetCurrentGroup();
            this.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (dragMode == DragMode.None || !this.HasPointerCapture(evt.pointerId))
                return;

            start.serializedObject.Update();
            float t = ToTime(evt.localPosition.x);
            float s = start.floatValue;
            float e = end.floatValue;

            switch (dragMode)
            {
                case DragMode.Start:
                    s = Mathf.Min(Snap(t), e);
                    break;
                case DragMode.End:
                    e = Mathf.Max(Snap(t), s);
                    break;
                case DragMode.Move:
                    float length = e - s;
                    s = Mathf.Clamp(Snap(t - grabOffset), 0f, 1f - length);
                    e = s + length;
                    break;
            }

            start.floatValue = s;
            end.floatValue = e;
            start.serializedObject.ApplyModifiedProperties();

            Refresh();
            SceneView.RepaintAll();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!this.HasPointerCapture(evt.pointerId))
                return;

            this.ReleasePointer(evt.pointerId);
            dragMode = DragMode.None;
            Undo.CollapseUndoOperations(undoGroup); // the whole drag = a single Ctrl+Z
        }
    }
}
