namespace SimpleCopyPasteTool.Model
{
    public class HotKeyOptionsItem
    {
        private int _id;
        private string _value;

        public HotKeyOptionsItem(int id, string value)
        {
            _id = id;
            _value = value;
        }

        public int Id
        {
            get => _id;
            set => _id = value;
        }

        public string Value
        {
            get => _value;
            set => _value = value;
        }
    }
}