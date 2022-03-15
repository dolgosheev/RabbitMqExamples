#pragma warning disable CS0618

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Serializer;

[Serializable]
public class Data
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public static MemoryStream SerializeToStream(Data o)
    {
        var stream = new MemoryStream();
        IFormatter formatter = new BinaryFormatter();
        formatter.Serialize(stream, o);
        return stream;
    }

    public static T? DeserializeFromStream<T>(MemoryStream stream) where T : class
    {
        try
        {
            IFormatter formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            var o = (T) formatter.Deserialize(stream);
            return o;
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error has been occured | Exception {e}");
        }

        return default;
    }

    public static T? DeserializeFromStream<T>(byte[] byteStream)
    {
        try
        {
            Stream stream = new MemoryStream(byteStream);
            IFormatter formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            var o = (T) formatter.Deserialize(stream);
            return o;
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error has been occured | Exception {e}");
        }

        return default;
    }
}