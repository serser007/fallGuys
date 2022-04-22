using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Splines;
using UnityEngine;
using UnityEngine.Serialization;

internal class FallGuysSetup: MonoBehaviour
{
    private class FallGuyObject
    {
        public enum AnimatorKeys
        {
            Run,
            Victory,
            Die
        }

        private const string DeadTag = "Untagged";

        private const string RunKey = "Run";
        private const string VictoryKey = "Victory";
        private const string DieKey = "die";

        public readonly GameObject GameObject;
        private readonly Animator animator;
        private readonly Rigidbody rigidbody;

        public FallGuyObject(GameObject o)
        {
            GameObject = o;
            animator = o.GetComponentInChildren<Animator>();
            rigidbody = o.GetComponent<Rigidbody>();
                
            GameObject.transform.SetParent(Instance.transform);
        }

        public void Kill()
        {
            rigidbody.constraints = RigidbodyConstraints.None;
            rigidbody.velocity = Vector3.forward * Instance.speed1;
            GameObject.transform.SetParent(null);
            GameObject.transform.tag = DeadTag;
        }

        public void RunAnimation(AnimatorKeys key)
        {
            switch (key)
            {
                case AnimatorKeys.Run:
                    animator.SetTrigger(RunKey);
                    break;
                case AnimatorKeys.Victory:
                    animator.SetTrigger(VictoryKey);
                    break;
                case AnimatorKeys.Die:
                    animator.SetTrigger(DieKey);
                    break;
            }
        }

        public void MoveTo(Vector3 point)
        {
            Instance.StartCoroutine(MoveFallGuy(point));
        }

        public void SetLocalPosition(Vector3 point)
        {
            GameObject.transform.localPosition = point;
        }

        private IEnumerator MoveFallGuy(Vector3 point)
        {
            var transform = GameObject.transform;
            var pos = transform.localPosition;
            var startTime = Time.time;
            while ((transform.localPosition - point).magnitude > 0.001f && transform.transform.CompareTag("Player"))
            {
                transform.localPosition = Vector3.Lerp(pos, point, (Time.time - startTime) * Instance.speed);
                yield return new WaitForEndOfFrame();
            }
        }
    }
        
    [SerializeField]
    private float xScale;
    [SerializeField]
    private float yScale;
    [SerializeField]
    private float speed = 10;
    [SerializeField]
    private SplineFollower follower;
    [SerializeField]
    private float speed1 = 1;
    [FormerlySerializedAs("deactivate")]
    [SerializeField]
    private List<GameObject> destroyOnFinish;
    [SerializeField]
    private float size;
    [SerializeField]
    private int row;

    private bool isStarted;
    private bool isFinished;

    private readonly List<FallGuyObject> fallGuys = new List<FallGuyObject>();

    public static FallGuysSetup Instance;

    private void Awake()
    {
        Instance = this;
        DrawScript.GetPoints += DrawScriptOnGetPoints;
        follower.followSpeed = 0;
    }

    public void Add(GameObject o)
    {
        var fallGuy = new FallGuyObject(o);
        
        if (isStarted)
            fallGuy.RunAnimation(FallGuyObject.AnimatorKeys.Run);
        else
            SetRowPosition(fallGuy, fallGuys.Count);

        fallGuys.Add(fallGuy);
    }

    public void Remove(GameObject o)
    {
        var fallGuy = fallGuys.First(fg => fg.GameObject == o);
        fallGuys.Remove(fallGuy);
        fallGuy.Kill();
        fallGuy.RunAnimation(FallGuyObject.AnimatorKeys.Die);

        if (fallGuys.Count == 0)
            GameOver();
    }

    public void Victory()
    {
        isFinished = true;
        destroyOnFinish.ForEach(Destroy);
        for (var i = 0; i < fallGuys.Count; i++)
        {
            SetRowPosition(fallGuys[i], i, true, false);
            fallGuys[i].RunAnimation(FallGuyObject.AnimatorKeys.Victory);
        }
    }

    private void GameOver()
    {
        isFinished = true;
        follower.followSpeed = 0;
        destroyOnFinish.ForEach(Destroy);
    }

    private void SetRowPosition(FallGuyObject fallGuy, int idx, bool backwards=false, bool instance = true)
    {
        var sz = backwards ? size * 1.1f : size;
        var x = -(sz * (row - 1)) / 2 + sz * (idx % row);
        var y = (-yScale * (backwards ? 0 : 1) + sz * (idx / row)) * (backwards ? -1 : 1);
        var point = new Vector3(x, 0, y);
        if (instance)
            fallGuy.SetLocalPosition(point);
        else
            fallGuy.MoveTo(point);
    }

    private void StartRun()
    {
        if (isStarted)
            return;
        isStarted = true;
        fallGuys.ForEach(o => o.RunAnimation(FallGuyObject.AnimatorKeys.Run));
        follower.followSpeed = speed1;
    }

    private void DrawScriptOnGetPoints(Vector2[] obj)
    {
        if (isFinished)
            return;
        var calculatedPoints = new List<Vector2> { obj[0] };

        if (obj.Length == 1)
        {
            while (calculatedPoints.Count <  fallGuys.Count)
            {
                calculatedPoints.Add(obj[0]);   
            }
        }
        else
        {
            var length = 0f;
            for (var k = 1; k < obj.Length; k++)
            {
                length += (obj[k - 1] - obj[k]).magnitude;
            }

            length = fallGuys.Count < 2 ? length + 1 : length / (fallGuys.Count - 1);
            var pointPosition = 0f;
        
            var segmentLength = 0f;
            for (var k = 1; k < obj.Length; k++)
            {
                var magnitude = (obj[k - 1] - obj[k]).magnitude;

                while (pointPosition + length <
                       segmentLength + magnitude + (k == obj.Length - 1 ? float.PositiveInfinity : 0))
                {
                    if (fallGuys.Count <= calculatedPoints.Count)
                        break;
                    pointPosition += length;

                    var point = obj[k - 1] + (obj[k] - obj[k - 1]) * ((pointPosition - segmentLength) / magnitude);
                    calculatedPoints.Add(point);
                }

                segmentLength += magnitude;
            }
        }

        StartRun();

        StopAllCoroutines();
        for (var i = 0; i < fallGuys.Count; i++)
        {
            var point = (Vector3)calculatedPoints[i];
            point = new Vector3(point.x * xScale, 0, point.y * yScale);
            fallGuys[i].MoveTo(point);
        }
    }
}