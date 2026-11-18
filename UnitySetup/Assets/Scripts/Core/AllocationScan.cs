#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Game.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Unity.Profiling;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.LowLevel;
    using UnityEngine.InputSystem.OnScreen;
    using UnityEngine.SceneManagement;
    using UnityEngine.UIElements;
    using Game.Systems;

    /// <summary>
    /// Development builds only: finds what allocates managed memory every frame, in a real player
    /// (the Editor's own allocations drown it there). Started by <c>-allocationScan &lt;file&gt;</c> on the
    /// player's command line: plays a new game, measures the Profiler's "GC Allocated In Frame"
    /// with everything on, then with each component type disabled in turn, writes what each one
    /// accounts for; then the same with a simulated finger circling on the on-screen stick (touch
    /// input is where a phone allocates and a desktop run does not); writes it all and quits.
    /// See docs/systems/logging.md, "Diagnostics report".
    /// </summary>
    public sealed class AllocationScan : MonoBehaviour
    {
        private const string Argument = "-allocationScan";
        private const int SettleFrames = 120;
        private const int MeasureFrames = 180;
        private const int WaitFrames = 1800;

        private const float DragRadius = 60f;

        private string _output;
        private bool _dragging;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartIfAsked()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            int index = System.Array.IndexOf(args, Argument);
            if (index < 0 || index + 1 >= args.Length)
            {
                return;
            }
            // A desktop player pauses without focus, and a headless run never has it.
            Application.runInBackground = true;
            var go = new GameObject("AllocationScan");
            DontDestroyOnLoad(go);
            go.AddComponent<AllocationScan>()._output = args[index + 1];
        }

        private IEnumerator Start()
        {
            for (int i = 0; i < WaitFrames && (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Menu); i++)
            {
                yield return null;
            }
            var start = GameManager.Instance.StartNewGameAsync();
            while (!start.IsCompleted)
            {
                yield return null;
            }
            for (int i = 0; i < SettleFrames; i++)
            {
                yield return null;
            }

            var report = new StringBuilder();
            report.Append("allocation scan: ").Append(MeasureFrames).Append(" frames a step, ")
                  .Append(Application.platform).Append(", ").Append(SystemInfo.deviceModel).Append('\n');
            yield return Scan("no touch", report);

            Touchscreen touch = InputSystem.AddDevice<Touchscreen>();
            _dragging = true;
            StartCoroutine(Drag(touch, StickCentre()));
            for (int i = 0; i < SettleFrames; i++)
            {
                yield return null;
            }
            Vector3 avatarBefore = AvatarPosition();
            yield return Scan("a finger dragging the on-screen stick", report);
            report.Append("  the avatar moved ").Append((AvatarPosition() - avatarBefore).magnitude.ToString("0.0"))
                  .Append(" m during the drag (0 means the simulated finger did not reach the stick)\n");
            _dragging = false;
            yield return null;
            InputSystem.RemoveDevice(touch);

            File.WriteAllText(_output, report.ToString());
            Log.Warn(LogCat.Boot, "AllocationScan: written to {0}", _output);
            Application.Quit();
        }

        private IEnumerator Scan(string title, StringBuilder report)
        {
            var result = new long[2];
            yield return Measure(result);
            long baseline = result[0];
            report.Append('\n').Append(title).Append(": baseline ").Append(baseline / MeasureFrames).Append(" B/frame, ")
                  .Append(result[1]).Append(" GC collections\n");

            var byType = new SortedDictionary<string, List<Behaviour>>();
            foreach (Behaviour b in AllBehaviours())
            {
                if (!b.enabled || b == this || b is Camera || b is UIDocument)
                {
                    continue; // a UIDocument rebuilds its panel when re-enabled, and the UI would hold stale elements
                }
                string key = b.GetType().FullName;
                if (!byType.TryGetValue(key, out List<Behaviour> list))
                {
                    byType[key] = list = new List<Behaviour>();
                }
                list.Add(b);
            }

            foreach (KeyValuePair<string, List<Behaviour>> entry in byType)
            {
                foreach (Behaviour b in entry.Value)
                {
                    if (b != null) b.enabled = false;
                }
                yield return Measure(result);
                int gone = 0;
                foreach (Behaviour b in entry.Value)
                {
                    // A step can make the game reload a scene, destroying what was disabled.
                    if (b != null) b.enabled = true;
                    else gone++;
                }
                long without = result[0] / MeasureFrames;
                long accounts = baseline / MeasureFrames - without;
                if (accounts < 16)
                {
                    continue; // only what accounts for something
                }
                report.Append("  without ").Append(entry.Key).Append(" x").Append(entry.Value.Count).Append(": ")
                      .Append(without).Append(" B/frame (accounts for ").Append(accounts).Append(')');
                if (gone > 0) report.Append(", ").Append(gone).Append(" destroyed during the step");
                report.Append('\n');
            }
        }

        /// <summary>Moves a simulated finger in circles around <paramref name="centre"/> every frame while dragging.</summary>
        private IEnumerator Drag(Touchscreen touch, Vector2 centre)
        {
            int frame = 0;
            while (_dragging)
            {
                // Press on the knob first, then move out into a circle: a touch that starts off the knob does not grab it.
                float a = frame * 0.12f;
                float radius = Mathf.Min(frame * 4f, DragRadius);
                InputSystem.QueueStateEvent(touch, new TouchState
                {
                    touchId = 1,
                    phase = frame == 0 ? UnityEngine.InputSystem.TouchPhase.Began : UnityEngine.InputSystem.TouchPhase.Moved,
                    position = centre + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius,
                });
                frame++;
                yield return null;
            }
            InputSystem.QueueStateEvent(touch, new TouchState { touchId = 1, phase = UnityEngine.InputSystem.TouchPhase.Ended, position = centre });
        }

        private Vector3 AvatarPosition()
        {
            foreach (Behaviour b in AllBehaviours())
            {
                if (b is Game.Gameplay.AvatarController)
                {
                    return b.transform.position;
                }
            }
            return Vector3.zero;
        }

        /// <summary>Where the on-screen stick is, in screen pixels; the screen's lower right when there is none.</summary>
        private Vector2 StickCentre()
        {
            foreach (Behaviour b in AllBehaviours())
            {
                if (b is OnScreenStick stick)
                {
                    return RectTransformUtility.WorldToScreenPoint(null, ((RectTransform)stick.transform).position);
                }
            }
            return new Vector2(Screen.width * 0.85f, Screen.height * 0.25f);
        }

        private static IEnumerator Measure(long[] result)
        {
            yield return null;
            using var recorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", MeasureFrames + 4);
            int gcBefore = System.GC.CollectionCount(0);
            for (int i = 0; i < MeasureFrames; i++)
            {
                yield return null;
            }
            long sum = 0;
            for (int i = 0; i < recorder.Count; i++)
            {
                sum += recorder.GetSample(i).Value;
            }
            result[0] = sum;
            result[1] = System.GC.CollectionCount(0) - gcBefore;
        }

        private IEnumerable<Behaviour> AllBehaviours()
        {
            var roots = new List<GameObject>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                roots.AddRange(SceneManager.GetSceneAt(i).GetRootGameObjects());
            }
            roots.AddRange(gameObject.scene.GetRootGameObjects()); // DontDestroyOnLoad: GameManager and the like
            foreach (GameObject root in roots)
            {
                foreach (Behaviour b in root.GetComponentsInChildren<Behaviour>(false))
                {
                    yield return b;
                }
            }
        }
    }
}
#endif
