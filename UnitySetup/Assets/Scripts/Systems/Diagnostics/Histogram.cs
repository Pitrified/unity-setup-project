namespace Game.Systems
{
    using System;

    /// <summary>
    /// Fixed-bin histogram for descriptive statistics over a long run with constant memory and no
    /// allocation per sample: count, exact min, max and mean, and quantiles to within one bin.
    /// Values under 0 count in the first bin; values past the last bin count in an overflow bin
    /// whose quantile is the exact max. Used by <see cref="DiagnosticsRecorder"/>.
    /// </summary>
    public sealed class Histogram
    {
        private readonly int[] _bins;
        private readonly float _binWidth;
        private int _overflow;

        /// <summary>Samples added.</summary>
        public long Count { get; private set; }

        /// <summary>Smallest sample, exact.</summary>
        public float Min { get; private set; } = float.PositiveInfinity;

        /// <summary>Largest sample, exact.</summary>
        public float Max { get; private set; } = float.NegativeInfinity;

        /// <summary>Sum of the samples, for the mean.</summary>
        public double Sum { get; private set; }

        /// <summary>Mean of the samples, or NaN when empty.</summary>
        public float Mean => Count == 0 ? float.NaN : (float)(Sum / Count);

        /// <summary>A histogram of <paramref name="bins"/> bins of <paramref name="binWidth"/> from 0.</summary>
        public Histogram(int bins, float binWidth)
        {
            if (bins <= 0 || binWidth <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(bins), "A histogram needs bins and a positive bin width.");
            }
            _bins = new int[bins];
            _binWidth = binWidth;
        }

        /// <summary>Adds one sample.</summary>
        public void Add(float value)
        {
            Count++;
            Sum += value;
            if (value < Min) Min = value;
            if (value > Max) Max = value;
            int bin = (int)(value / _binWidth);
            if (bin < 0)
            {
                bin = 0;
            }
            if (bin >= _bins.Length)
            {
                _overflow++;
                return;
            }
            _bins[bin]++;
        }

        /// <summary>
        /// The value under which a fraction <paramref name="q"/> (0 to 1) of the samples fall, to within
        /// one bin (the bin's middle, kept inside the exact min and max); NaN when empty.
        /// </summary>
        public float Quantile(float q)
        {
            if (Count == 0)
            {
                return float.NaN;
            }
            long target = (long)Math.Ceiling(Math.Clamp(q, 0f, 1f) * Count);
            if (target < 1) target = 1;
            long seen = 0;
            for (int i = 0; i < _bins.Length; i++)
            {
                seen += _bins[i];
                if (seen >= target)
                {
                    float mid = (i + 0.5f) * _binWidth;
                    return Math.Clamp(mid, Min, Max);
                }
            }
            return Max; // in the overflow bin
        }

        /// <summary>Empties it.</summary>
        public void Clear()
        {
            Array.Clear(_bins, 0, _bins.Length);
            _overflow = 0;
            Count = 0;
            Sum = 0;
            Min = float.PositiveInfinity;
            Max = float.NegativeInfinity;
        }
    }
}
