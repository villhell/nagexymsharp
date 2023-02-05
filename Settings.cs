using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nagexym
{
    /// <summary>
    /// 設定値を保持するためのクラス
    /// </summary>
    internal class Settings
    {
        internal int ExcelColAccount { get; set; }
        internal int ExcelColTwitter { get; set; }
        internal int ExcelColAddress { get; set; }
        internal int ExcelColXym { get; set; }
        internal int ExcelColMessage { get; set; }

        internal ulong Fee { get; set; }

        internal int InnerTxCount { get; set; }
        internal Settings()
        {
            ExcelColAccount = 1;
            ExcelColTwitter = 2;
            ExcelColAddress = 5;
            ExcelColXym = 7;
            ExcelColMessage = 8;
            Fee= 1000000;
            InnerTxCount= 49;
        }
    }
}
