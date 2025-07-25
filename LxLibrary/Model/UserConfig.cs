using System;

namespace ImLibrary.Model
{
    public class UserConfig
    {
        /** 参数主键 */
        public long configId {  get; set; }

        /** 用户ID */
        public long userId { get; set; }

        /** 参数名称 */
        public String configName { get; set; }

        /** 参数键名 */
        public String configKey { get; set; }

        /** 参数键值 */
        public String configValue { get; set; }
    }
}
