using System;
using System.Text;
using System.Windows.Forms;

namespace NinjaTrader.Strategy
{
    public static class CSTUtilities
    {
        public static void ExceptionMessage(Exception e)
        {
            StringBuilder sb = new StringBuilder("We have exception ");
            sb.AppendLine("Hello this is message from Yura send screenshot with that messege to yura and pay 50$ to fix that");
            sb.AppendLine("Message " + e.Message);
            sb.AppendLine("Data "+e.Data);
            sb.AppendLine("From : " + e.StackTrace);
            sb.AppendLine("Source" + e.Source);
            MessageBox.Show(sb.ToString()); 
        }
    }
}
