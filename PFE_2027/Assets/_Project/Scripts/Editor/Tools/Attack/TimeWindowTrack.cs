using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PFE.Editor._Project.Scripts.Editor.Tools.Attack
{
    // Une piste de timeline : la barre d'une TimeWindow (début/fin normalisés), déplaçable et étirable à la souris.
    public sealed class TimeWindowTrack : VisualElement
    {
        private const float EDGE_GRAB_PIXELS = 6f;

        private enum DragMode { None, Move, Start, End }

        private readonly SerializedProperty start;
        private readonly SerializedProperty end;
        private readonly int totalFrames;
        private readonly VisualElement bar;
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
            bar.pickingMode = PickingMode.Ignore; // les clics traversent la barre jusqu'à la piste
            Add(bar);

            playhead = new VisualElement();
            playhead.AddToClassList("attack-track__playhead");
            playhead.pickingMode = PickingMode.Ignore;
            Add(playhead);

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);

            Refresh();
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

            float x = evt.localPosition.x;
            float startX = start.floatValue * layout.width;
            float endX = end.floatValue * layout.width;

            if (Mathf.Abs(x - startX) <= EDGE_GRAB_PIXELS)
                dragMode = DragMode.Start;
            else if (Mathf.Abs(x - endX) <= EDGE_GRAB_PIXELS)
                dragMode = DragMode.End;
            else if (x > startX && x < endX)
            {
                dragMode = DragMode.Move;
                grabOffset = ToTime(x) - start.floatValue;
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
            Undo.CollapseUndoOperations(undoGroup); // tout le glisser = un seul Ctrl+Z
        }
    }
}
