using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Lean.Touch;

public class WritePencil : LeanFingerTrail
{
    private float _startWidth = 0.6f;
    private float _endWidth = 0.6f;
    private Color _startColor = Color.white;
    private Color _endColor = Color.white;
    private bool _IsNewLine = true;
    private Vector2 _paperPosOff = Vector2.zero;
    private float _UIScale = 1.0f;
    private Rect _WriteRect;
    [System.Serializable]
    public class WriteLink
    {
        public List<Vector2> LinePoints;
        public LineRenderer Line;
    }

    private List<WriteLink> writeLinks = new List<WriteLink>();

    public void SetPaperPosOff(Vector2 paperPosOff,float us)
    {
        _paperPosOff = paperPosOff;
        _UIScale = us;
    }

    protected override void FingerSet(LeanFinger finger)
    {
        // ignore?
        if (MaxLines > 0 && writeLinks.Count >= MaxLines)
        {
            return;
        }

        if (IgnoreStartedOverGui == true && finger.StartedOverGui == true)
        {
            return;
        }

        if (IgnoreIsOverGui == true && finger.IsOverGui == true)
        {
            return;
        }

        if (RequiredSelectable != null && RequiredSelectable.IsSelectedBy(finger) == false)
        {
            return;
        }

        if (_IsNewLine)
        {
            if (finger.Snapshots.Count < 4) return;//不能少于5个点

            if (!_WriteRect.Contains(finger.Snapshots[0].ScreenPosition)) return;

            _IsNewLine = false;

            var link = new WriteLink();

            //link.Finger = finger;
            link.Line = Instantiate(LinePrefab);

            link.Line.startWidth = _startWidth;
            link.Line.endWidth = _endWidth;
            link.Line.startColor = _startColor;
            link.Line.endColor = _endColor;
            link.LinePoints = new List<Vector2>();

            //设置线条
            for (var i = 0; i < finger.Snapshots.Count; i++)
            {
                var snapshot = finger.Snapshots[i];
                var sp = new Vector2(snapshot.ScreenPosition.x + _paperPosOff.x, snapshot.ScreenPosition.y + _paperPosOff.y);
                var worldPoint = ScreenDepth.Convert(sp, _Camera, gameObject);

                worldPoint = new Vector3((worldPoint.x + 5) / _UIScale - 5, (worldPoint.y + 5) / _UIScale - 5, worldPoint.z);

                link.LinePoints.Add(worldPoint);
            }

            writeLinks.Add(link);
        }
        else
        {
            writeLinks[writeLinks.Count - 1].LinePoints.Clear();

            for (var i = 0; i < finger.Snapshots.Count; i++)
            {
                var snapshot = finger.Snapshots[i];
                var sp = new Vector2(snapshot.ScreenPosition.x + _paperPosOff.x, snapshot.ScreenPosition.y + _paperPosOff.y);
                var worldPoint = ScreenDepth.Convert(sp, _Camera, gameObject);

                worldPoint = new Vector3((worldPoint.x + 5)/_UIScale - 5, (worldPoint.y + 5) / _UIScale - 5, worldPoint.z);

                writeLinks[writeLinks.Count - 1].LinePoints.Add(worldPoint);

            }
        }

        foreach (var wl in writeLinks)
        {
            WriteLinePos(wl);
        }
    }
    private void WriteLinePos(WriteLink wl)
    {
        wl.Line.positionCount = wl.LinePoints.Count;
        for (var i = 0; i < wl.LinePoints.Count; i++)
        {
            wl.Line.SetPosition(i, wl.LinePoints[i]);
        }
    }
    protected override void FingerUp(LeanFinger finger)
    {
        _IsNewLine = true;
    }

    public void ClearWriteLine()
    {
        for (int i = writeLinks.Count - 1; i >= 0; i--)
        {
            Destroy(writeLinks[i].Line.gameObject);
            writeLinks.RemoveAt(i);
        }
    }
    public void UpdateWriteColor(Color sc,Color ec)
    {
        _startColor = sc;
        _endColor = ec;
    }
    public void SetWriteRect(Rect wr)
    {
        _WriteRect = wr;
    }
}
