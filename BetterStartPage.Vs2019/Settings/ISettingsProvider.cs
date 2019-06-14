namespace BetterStartPage.Settings
{
    public interface ISettingsProvider
    {
        void WriteInt32(string name, int value);
        int ReadInt32(string name, int defaultValue = 0);

        void WriteBytes(string name, byte[] value);
        byte[] ReadBytes(string name, byte[] defaultValue = null);

        void WriteString(string name, string value);
        string ReadString(string name, string defaultValue = "");

        void WriteBool(string name, bool value);
        bool ReadBool(string name, bool defaultValue = false);
        double ReadDouble(string name, double value = double.NaN);
        void WriteDouble(string name, double value);
        void Reset();

    }
}
