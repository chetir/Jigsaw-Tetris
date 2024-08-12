namespace GameInput
{
    public partial class InputControl
    {
        private static InputControl _instance;

        public static InputControl Instance
        {
            get
            {
                if (_instance == null) {
                    _instance = new InputControl();
                    _instance.Enable();
                }

                return _instance;
            }
            private set => _instance = value;
        }
    }
}