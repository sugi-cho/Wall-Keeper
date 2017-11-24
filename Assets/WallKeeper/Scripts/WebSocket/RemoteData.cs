using UnityEngine;
using UnityEngine.Events;

using sugi.cc;

public class RemoteData : WebSocketDataBehaviour<KinectController.ControlData> {
    [System.Serializable]
    public class ControlDataEvent : UnityEvent<KinectController.ControlData> { }

    public ControlDataEvent onDataReceived;
    private void Update()
    {
        while(0 < receivedData.Count)
        {
            var data = receivedData.Dequeue();
            onDataReceived.Invoke(data);
        }
    }
}
