using System.Text;

namespace Tadmor.Logging
{
    public class StringLogger
    {
        private readonly StringBuilder _stringBuilder = new StringBuilder();

        public void Log(string value)
        {
            _stringBuilder.AppendLine(value);
        }

        public override string ToString()
        {
            return _stringBuilder.ToString();
        }
    }
}