using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Windows.Kinect;

using MessagePack;
using sugi.cc;

public class KinectController : MonoBehaviour
{
    KinectSensor kinect;
    MultiSourceFrameReader reader;

    CameraSpacePoint[] cameraSpacePoints;
    Body[] bodyData;
    public static JointType[] KeyJoints = new[] { JointType.HandLeft, JointType.HandRight, JointType.Head };

    int depthDataLength;

    [Header("Compute Shader")]
    public ComputeShader kinectCS;
    [SerializeField] Texture2D bodyIndexTex;
    ComputeBuffer cameraSpacePointBuffer;
    ComputeBuffer atackPointBuffer;
    ComputeBuffer vertexBuffer;

    public Material meshVisalizer;
    public int downsample = 0;

    [Header("send kinect data to remote")]
    public RemoteData remote;
    public Setting setting;

    ControlData controllData;

    void Start()
    {
        SettingManager.AddSettingMenu(setting, name + "_kinectSetting.json");
        kinect = KinectSensor.GetDefault();
        controllData = new ControlData();

        if (kinect != null)
        {
            reader = kinect.OpenMultiSourceFrameReader(FrameSourceTypes.Depth | FrameSourceTypes.Body | FrameSourceTypes.BodyIndex);

            var depthDesc = kinect.DepthFrameSource.FrameDescription;
            depthDataLength = (int)depthDesc.LengthInPixels;

            controllData.depthData = new ushort[depthDataLength];
            cameraSpacePoints = new CameraSpacePoint[depthDataLength];

            cameraSpacePointBuffer = new ComputeBuffer(depthDataLength, Marshal.SizeOf(typeof(CameraSpacePoint)));
            atackPointBuffer = new ComputeBuffer(3 * 6, Marshal.SizeOf(typeof(Vector3)));
            vertexBuffer = new ComputeBuffer(depthDataLength, Marshal.SizeOf(typeof(Vector3)));

            var bodyIndexDesc = kinect.BodyIndexFrameSource.FrameDescription;
            controllData.bodyIndexData = new byte[bodyIndexDesc.LengthInPixels * bodyIndexDesc.BytesPerPixel];
            bodyIndexTex = new Texture2D(bodyIndexDesc.Width, bodyIndexDesc.Height, TextureFormat.R8, false);
            bodyData = new Body[kinect.BodyFrameSource.BodyCount];

            if (!kinect.IsOpen)
                kinect.Open();

            kinectCS.SetVector("_ResetRot", new UnityEngine.Vector4(0, 0, 0, 1));
        }

        controllData.trackedIds = Enumerable.Repeat((ulong)0, 6).ToArray();
        controllData.atackPoints = Enumerable.Repeat(Vector3.zero, 3 * 6).ToArray();
        controllData.floorClipPlane = UnityEngine.Vector4.zero;
    }

    private void OnApplicationQuit()
    {
        new[] { cameraSpacePointBuffer, atackPointBuffer, vertexBuffer }
        .ToList().ForEach(b => b.Release());
        reader.Dispose();
        if (kinect != null)
            if (kinect.IsOpen)
                kinect.Close();
    }

    void Update()
    {
        if (reader != null)
        {
            var dataUpdated = false;
            var frame = reader.AcquireLatestFrame();
            if (frame != null)
            {
                var depthFrame = frame.DepthFrameReference.AcquireFrame();
                var bodyFrame = frame.BodyFrameReference.AcquireFrame();
                var bodyIndexFrame = frame.BodyIndexFrameReference.AcquireFrame();

                if (depthFrame != null)
                {
                    depthFrame.CopyFrameDataToArray(controllData.depthData);
                    depthFrame.Dispose();
                    dataUpdated = true;
                }
                if (bodyFrame != null)
                {
                    var newFloorClipPlane = bodyFrame.FloorClipPlane;
                    controllData.floorClipPlane.x = newFloorClipPlane.X;
                    controllData.floorClipPlane.y = newFloorClipPlane.Y;
                    controllData.floorClipPlane.z = newFloorClipPlane.Z;
                    if (newFloorClipPlane.W != 0)
                        controllData.floorClipPlane.w = newFloorClipPlane.W;


                    bodyFrame.GetAndRefreshBodyData(bodyData);
                    for (var i = 0; i < 6; i++)
                    {
                        var body = bodyData[i];
                        var shoulderHeight = body.Joints[JointType.SpineShoulder].Position.Y;
                        controllData.trackedIds[i] = body.IsTracked ? body.TrackingId : 0;
                        for (var j = 0; j < 3; j++)
                        {
                            var key = KeyJoints[j];
                            var joint = body.Joints[key];
                            if (body.IsTracked && joint.TrackingState != TrackingState.NotTracked && shoulderHeight < joint.Position.Y)
                            {
                                controllData.atackPoints[i * 3 + j].x = joint.Position.X;
                                controllData.atackPoints[i * 3 + j].y = joint.Position.Y;
                                controllData.atackPoints[i * 3 + j].z = joint.Position.Z;
                            }
                            else
                            {
                                controllData.atackPoints[i * 3 + j].x = 0;
                                controllData.atackPoints[i * 3 + j].y = 0;
                                controllData.atackPoints[i * 3 + j].z = 0;
                            }
                        }
                    }

                    bodyFrame.Dispose();
                    dataUpdated = true;
                }
                if (bodyIndexFrame != null)
                {
                    bodyIndexFrame.CopyFrameDataToArray(controllData.bodyIndexData);
                    bodyIndexFrame.Dispose();

                    dataUpdated = true;
                }
                if (dataUpdated)
                    SetData(controllData);
            }
        }
    }

    public void SetData(ControlData controllData)
    {
        kinect.CoordinateMapper.MapDepthFrameToCameraSpace(controllData.depthData, cameraSpacePoints);
        cameraSpacePointBuffer.SetData(cameraSpacePoints);

        var kinectRot = Quaternion.FromToRotation(controllData.floorClipPlane, Vector3.up);
        var kinectHeight = controllData.floorClipPlane.w;

        kinectCS.SetVector("_ResetRot", new UnityEngine.Vector4(kinectRot.x, kinectRot.y, kinectRot.z, kinectRot.w));
        kinectCS.SetFloat("_KinectHeight", kinectHeight);

        for (var i = 0; i < controllData.atackPoints.Length; i++)
            if (0 < controllData.atackPoints[i].sqrMagnitude)
            {
                controllData.atackPoints[i] = kinectRot * controllData.atackPoints[i];
                controllData.atackPoints[i].y += kinectHeight;
                if (setting.noMirror)
                    controllData.atackPoints[i].x *= -1;
                controllData.atackPoints[i] *= setting.kinectScale;
                controllData.atackPoints[i] += setting.kinectOffset;
            }
        atackPointBuffer.SetData(controllData.atackPoints);

        bodyIndexTex.LoadRawTextureData(controllData.bodyIndexData);
        bodyIndexTex.Apply();

        var kernel = kinectCS.FindKernel("buildVertex");
        kinectCS.SetBool("_NoMirror", setting.noMirror);
        kinectCS.SetFloat("_Scale", setting.kinectScale);
        kinectCS.SetVector("_Offset", setting.kinectOffset);
        kinectCS.SetBuffer(kernel, "_CameraSpacePointData", cameraSpacePointBuffer);
        kinectCS.SetBuffer(kernel, "_VertexDataBuffer", vertexBuffer);
        kinectCS.Dispatch(kernel, depthDataLength / 8, 1, 1);
    }

    private void OnRenderObject()
    {
        if (meshVisalizer == null)
            return;

        meshVisalizer.SetTexture("_BodyIdxTex", bodyIndexTex);
        meshVisalizer.SetBuffer("_VertexData", vertexBuffer);
        meshVisalizer.SetBuffer("_AtackPoints", atackPointBuffer);
        meshVisalizer.SetPass(0);
        Graphics.DrawProcedural(MeshTopology.Points, depthDataLength);
    }

    private void OnDrawGizmos()
    {
        if (controllData.atackPoints != null)
            foreach (var pos in controllData.atackPoints)
                if (0 < pos.sqrMagnitude)
                    Gizmos.DrawWireSphere(pos, 0.1f);
    }

    [MessagePackObject(true), System.Serializable]
    public struct ControlData
    {
        public ushort[] depthData;
        public byte[] bodyIndexData;
        public UnityEngine.Vector4 floorClipPlane;
        public ulong[] trackedIds;
        public Vector3[] atackPoints;
    }

    [System.Serializable]
    public class Setting : SettingManager.Setting
    {
        public bool noMirror;
        public Vector3 kinectOffset = Vector3.zero;
        public float kinectScale = 1f;
    }
}
