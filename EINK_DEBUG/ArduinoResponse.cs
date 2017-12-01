namespace EINK_DEBUG
{
    public class ArduinoResponse
    {
        public byte[] Data;

        public string StringData => System.Text.Encoding.ASCII.GetString(Data);

        public ArduinoResponse(byte[] data)
        {
            Data = data;
        }
    }
}
