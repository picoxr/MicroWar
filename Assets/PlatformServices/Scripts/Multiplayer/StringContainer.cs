using System.ComponentModel;
using Unity.Netcode;


namespace MicroWar.Multiplayer
{
    public class StringContainer : INetworkSerializable
    {
        public string text;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsWriter)
            {
                serializer.GetFastBufferWriter().WriteValueSafe(text);
            }
            else
            {
                serializer.GetFastBufferReader().ReadValueSafe(out text);
            }
        }

        public static StringContainer[] ArrayConvert(string[] stringArray)
        {
            if (stringArray == null) return null;

            StringContainer[] converted = new StringContainer[stringArray.Length];

            for(int i = 0; i < stringArray.Length; i++)
            {
                converted[i] = new StringContainer() { text = stringArray[i] };
            }

            return converted;
        }
    }
}

