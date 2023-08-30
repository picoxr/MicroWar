
namespace MicroWar.Extensions
{
    public static class ExtensionMethods
    {
        public static float RemapClamped(this float value, float sourceFrom, float sourceTo, float destinationFrom, float destinationTo)
        {
            if (value < sourceFrom) value = sourceFrom;
            if (value > sourceTo) value = sourceTo;

            return destinationFrom + (value - sourceFrom) * (destinationTo - destinationFrom) / (sourceTo - sourceFrom);
        }
    }
}
