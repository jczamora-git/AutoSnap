namespace AutoSnap.Audio;

public static class AudioLevelAnalyzer
{
    public static float CalculateRms16BitMono(byte[] buffer, int offset, int count)
    {
        if (count < 2) return 0f;

        long sumSquares = 0;
        int sampleCount = count / 2;

        for (int i = 0; i < sampleCount; i++)
        {
            short sample = BitConverter.ToInt16(buffer, offset + i * 2);
            sumSquares += (long)sample * sample;
        }

        double meanSquare = (double)sumSquares / sampleCount;
        double rms = Math.Sqrt(meanSquare) / 32768.0;
        return (float)rms;
    }

    public static bool IsSilent(float rms, float silenceThreshold = 0.003f)
    {
        return rms < silenceThreshold;
    }
}
