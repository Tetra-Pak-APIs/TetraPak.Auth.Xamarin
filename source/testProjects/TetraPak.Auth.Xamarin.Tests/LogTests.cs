using System;
using System.Net;
using System.Text;
using TetraPak.Auth.Xamarin.logging;
using Xunit;

namespace TetraPak.Auth.Xamarin.Tests
{
    public class LogTests
    {
        [Fact]
        public void RequestTest()
        {
            var r = (HttpWebRequest) WebRequest.Create("https://call.me/please");
            r.Headers.Add("x-one", "value-one");
            var log = new TestLog();
            log.DebugWebRequest(r, "hello wold!");
            var text = log.ToString();
            var nl = Environment.NewLine;
            Assert.Equal($@"GET https://call.me/please{nl}x-one=value-one{nl}{nl}hello wold!{nl}", text);
        }
    }
    
    class TestLog : ILog
    {
        readonly StringBuilder _sb = new StringBuilder();
        
        public event EventHandler<TextLogEventArgs> Logged;
        
        public QueryAsyncDelegate QueryAsync { get; set; }

        public void Debug(string message)
        {
            _sb.AppendLine(message);
        }

        public void Info(string message)
        {
            _sb.AppendLine(message);
        }

        public void Error(Exception exception, string message = null)
        {
            _sb.AppendLine(exception.ToString());
            if (message != null)
                _sb.AppendLine();
        }

        public override string ToString() => _sb.ToString();
    }
    

}