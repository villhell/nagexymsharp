using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nagexym
{
    /// <summary>
    /// Excel行クラス
    /// </summary>
    internal class ExcelRowData
    {
        /// <summary>
        /// アカウント名
        /// </summary>
        public string? AccountName { get; set; }
        /// <summary>
        /// ツイッターのURL
        /// </summary>
        public string? TwitterUrl { get; set; }
        /// <summary>
        /// アイコン（未実装）
        /// </summary>
        public byte[]? AccountIcon { get; set; }
        /// <summary>
        /// ネームスペース
        /// </summary>
        public string? NameSpace { get; set; }
        /// <summary>
        /// アドレス
        /// </summary>
        public string? Address { get; set; }
        /// <summary>
        /// Xym
        /// </summary>
        public double Xym { get; set; }
        /// <summary>
        /// メッセージ
        /// </summary>
        public string Message { get; set; }
    }
}
