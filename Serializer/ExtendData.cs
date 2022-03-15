namespace Serializer;

[Serializable]
public class ExtendData : Data 
{
    public readonly DateTime TimeStamp;
    public readonly string Message;
    
    public ExtendData(DateTime dateTime,string message)
    {
        TimeStamp = dateTime;
        Message = message;
    }
}