using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nagexym
{
    /// <summary>
    /// ツールの状態を管理する
    /// </summary>
    internal class ToolStateManager
    {
        /// <summary>
        /// ツールの状態
        /// </summary>
        internal enum ToolState
        {
            Init,
            StartReadExcel,
            EndReadExcel,
            StartCheck,
            Checking,
            EndCheck,
            StartSend,
            EndSend,
            Complete
        }

        /// <summary>
        /// ツールの状態
        /// </summary>
        private ToolState _toolState;


        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal ToolStateManager()
        {
            _toolState = ToolState.Init;
        }


        /// <summary>
        /// ツールの状態を返す
        /// </summary>
        /// <returns></returns>
        internal ToolState GetState()
        {
            return _toolState;
        }

        /// <summary>
        /// 次の状態へ以降する
        /// </summary>
        /// <param name="state"></param>
        internal void SetState(ToolState state)
        {
            _toolState= state;
        }
    }
}
