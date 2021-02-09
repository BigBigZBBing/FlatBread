using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlatBread.Enum
{
    /// <summary>
    /// 封包消息类型
    /// </summary>
    public enum MessageMode
    {
        None = 0,
        Message = 1 | 2 | 3,
        Disconect = 0xFF,
        Reconnect = 0xFE,
    }
}
