namespace Game.Editor
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEditor;
    using UnityEditor.Profiling;
    using UnityEditorInternal;
    using UnityEngine;

    /// <summary>
    /// Reads a Profiler capture (a player's binary log, <c>.raw</c>) and writes where the managed
    /// allocations of its heaviest frames came from: per frame, the main thread's call tree, each
    /// marker with the bytes allocated under it, down to the markers that allocate; first, over
    /// the whole capture, which markers allocate directly (the parent of each <c>GC.Alloc</c>). Used on the
    /// load capture the allocation scan writes (docs/systems/logging.md, "Diagnostics report").
    /// Batch mode: <c>-executeMethod Game.Editor.ProfileCaptureReport.FromCommandLine -profileCapture in.raw -reportOut out.txt</c>.
    /// </summary>
    public static class ProfileCaptureReport
    {
        private const int Frames = 6;
        private const int MaxDepth = 14;
        private const float ShownBytes = 32 * 1024;
        private const int MainThread = 0;
        private const int TopMarkers = 25;
        private const string GcAlloc = "GC.Alloc";

        /// <summary>Entry point for -executeMethod; exits with 0 when the report is written.</summary>
        public static void FromCommandLine()
        {
            string input = CommandLine.Value("-profileCapture");
            string output = CommandLine.Value("-reportOut");
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(output))
            {
                Debug.LogError("ProfileCaptureReport: -profileCapture <in.raw> and -reportOut <out.txt> are required.");
                EditorApplication.Exit(2);
                return;
            }
            if (!ProfilerDriver.LoadProfile(input, false))
            {
                Debug.LogError($"ProfileCaptureReport: could not load {input}.");
                EditorApplication.Exit(1);
                return;
            }
            File.WriteAllText(output, Build());
            Debug.Log($"ProfileCaptureReport: written to {output}");
            EditorApplication.Exit(0);
        }

        private static string Build()
        {
            int first = ProfilerDriver.firstFrameIndex;
            int last = ProfilerDriver.lastFrameIndex;
            var totals = new List<(int frame, float bytes, float ms)>();
            var byMarker = new Dictionary<string, double>();
            long all = 0;
            for (int f = first; f <= last; f++)
            {
                using HierarchyFrameDataView view = View(f);
                if (!view.valid)
                {
                    continue;
                }
                float bytes = view.GetItemColumnDataAsFloat(view.GetRootItemID(), HierarchyFrameDataView.columnGcMemory);
                all += (long)bytes;
                totals.Add((f, bytes, view.frameTimeMs));
                AddDirect(view, view.GetRootItemID(), byMarker, new List<int>());
            }

            var sb = new StringBuilder();
            sb.Append("profile capture: frames ").Append(first).Append("..").Append(last)
              .Append(", main thread allocated ").Append(Kb(all)).Append(" in all\n");
            var markers = new List<KeyValuePair<string, double>>(byMarker);
            markers.Sort((a, b) => b.Value.CompareTo(a.Value));
            sb.Append("\nallocating directly, whole capture:\n");
            for (int i = 0; i < markers.Count && i < TopMarkers; i++)
            {
                sb.Append("  ").Append(Kb(markers[i].Value).PadLeft(8)).Append("  ").Append(markers[i].Key).Append('\n');
            }
            totals.Sort((a, b) => b.bytes.CompareTo(a.bytes));
            for (int i = 0; i < totals.Count && i < Frames; i++)
            {
                (int frame, float bytes, float ms) = totals[i];
                sb.Append("\nframe ").Append(frame).Append(": ").Append(Kb(bytes)).Append(", ")
                  .Append(ms.ToString("0.0")).Append(" ms\n");
                using HierarchyFrameDataView view = View(frame);
                Tree(sb, view, view.GetRootItemID(), 0, new List<int>());
            }
            return sb.ToString();
        }

        /// <summary>Each child that allocates at least <see cref="ShownBytes"/>, indented by depth, with what it allocated.</summary>
        private static void Tree(StringBuilder sb, HierarchyFrameDataView view, int item, int depth, List<int> children)
        {
            if (depth >= MaxDepth)
            {
                return;
            }
            view.GetItemChildren(item, children);
            var kept = new List<int>(children);
            kept.Sort((a, b) => view.GetItemColumnDataAsFloat(b, HierarchyFrameDataView.columnGcMemory)
                .CompareTo(view.GetItemColumnDataAsFloat(a, HierarchyFrameDataView.columnGcMemory)));
            foreach (int child in kept)
            {
                float bytes = view.GetItemColumnDataAsFloat(child, HierarchyFrameDataView.columnGcMemory);
                if (bytes < ShownBytes)
                {
                    break;
                }
                sb.Append(' ', 2 + depth * 2).Append(view.GetItemName(child)).Append("  ").Append(Kb(bytes)).Append('\n');
                Tree(sb, view, child, depth + 1, children);
            }
        }

        /// <summary>Adds each <c>GC.Alloc</c> item's bytes to its parent marker's name.</summary>
        private static void AddDirect(HierarchyFrameDataView view, int item, Dictionary<string, double> byMarker, List<int> children)
        {
            view.GetItemChildren(item, children);
            foreach (int child in new List<int>(children))
            {
                float bytes = view.GetItemColumnDataAsFloat(child, HierarchyFrameDataView.columnGcMemory);
                if (bytes <= 0f)
                {
                    continue;
                }
                if (view.GetItemName(child) == GcAlloc)
                {
                    string parent = item == view.GetRootItemID() ? "(frame)" : view.GetItemName(item);
                    byMarker.TryGetValue(parent, out double sum);
                    byMarker[parent] = sum + bytes;
                }
                else
                {
                    AddDirect(view, child, byMarker, children);
                }
            }
        }

        private static HierarchyFrameDataView View(int frame) =>
            ProfilerDriver.GetHierarchyFrameDataView(frame, MainThread,
                HierarchyFrameDataView.ViewModes.MergeSamplesWithTheSameName,
                HierarchyFrameDataView.columnGcMemory, false);

        private static string Kb(double bytes) => (bytes / 1024.0).ToString("0") + " kB";
    }
}
