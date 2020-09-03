//using System.Windows.Input; obsolete
//using Xamarin.Forms;
//using Xamarin.Essentials;

//namespace authClient.views
//{
//    public partial class TokenView : ContentView
//    {
//        #region Bindable: Token
//        public static readonly BindableProperty TokenProperty = BindableProperty.Create(
//            "Token",
//            typeof(string),
//            typeof(TokenView),
//            default(string));

//        public string Token
//        {
//            get => (string)GetValue(TokenProperty);
//            set => SetValue(TokenProperty, value);
//        }
//        #endregion

//        #region Bindable: ButtonText
//        public static readonly BindableProperty ButtonTextProperty = BindableProperty.Create(
//            "ButtonText",
//            typeof(string),
//            typeof(TokenView),
//            "Copy");

//        public string ButtonText
//        {
//            get => (string)GetValue(ButtonTextProperty);
//            set => SetValue(ButtonTextProperty, value);
//        }
//        #endregion

//        #region Bindable: Command
//        public static readonly BindableProperty CommandProperty = BindableProperty.Create(
//            "Command",
//            typeof(ICommand),
//            typeof(TokenView),
//            null
//            );

//        public ICommand Command
//        {
//            get => (ICommand)GetValue(CommandProperty);
//            set => SetValue(CommandProperty, value);
//        }
//        #endregion

//        public TokenView()
//        {
//            InitializeComponent();
//            Command = new Command(() => Clipboard.SetTextAsync(Token));
//        }
//    }
//}
