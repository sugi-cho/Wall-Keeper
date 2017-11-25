using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AtackPointController : MonoBehaviour
{

    public AtackPoint[] atackPoints;
    List<ulong> trackingIdList = new List<ulong>();

    public void SetAtackPoint(ulong[] trackingIds, Vector3[] points)
    {
        var alreadyTracked = trackingIds.Where(id => id != 0 && trackingIdList.Contains(id));
        trackingIdList.Clear();
        trackingIdList.AddRange(alreadyTracked);
        var untrackedPoints = atackPoints.Where(ap => !trackingIdList.Contains(ap.trackingId));

        foreach (var atackPoint in untrackedPoints)
            atackPoint.SetUntracked();

        var count = 0;
        for (var i = 0; i < trackingIds.Length; i++)
        {
            var trackingId = trackingIds[i];
            if (trackingId == 0)
                continue;

            var pos = points[i];
            AtackPoint atackPoint = null;

            if (alreadyTracked.Contains(trackingId))
                atackPoint = atackPoints.Where(ap => ap.trackingId == trackingId).FirstOrDefault();
            if (atackPoint == null)
            {
                atackPoint = untrackedPoints.ElementAt(count++);
                trackingIdList.Add(trackingId);
                atackPoint.trackingId = trackingId;
            }
            if (atackPoint != null)
                atackPoint.SetPos(pos);
        }
    }

    // Use this for initialization
    void Start()
    {
        atackPoints = GetComponentsInChildren<AtackPoint>()
            .OrderBy(ap => ap.name).ToArray();
    }
}
