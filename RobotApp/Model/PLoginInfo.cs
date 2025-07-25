using Newtonsoft.Json;
using System;

namespace RobotApp.Model
{
    internal class PLoginInfo
    {
        /** 
	 * 登陆时提交的用户名：此用户名对框架来说可以随意，具体意义由上层逻辑决即可。
	 *  */
        [JsonProperty("loginUserId")]
        private String LoginUserId { get; set; }

        /** 登陆时提交的密码：此密码对框架来说可以随意，具体意义由上层逻辑决即可 */
        [JsonProperty("loginToken")]
        private String LoginToken { get; set; }

        /**
         * 额外信息字符串。本字段目前为保留字段，供上层应用自行放置需要的内容。
         * @since 2.1.6 */
        [JsonProperty("extra")]
        private String Extra { get; set; }

        /**
	 * 构造方法。
	 * 
	 * @param loginUserId 传递过来的准一id，保证唯一就可以通信，可能是登陆用户名、也可能是任意不重复的id等，具体意义由业务层决定
	 * @param loginToken 用于身份鉴别和合法性检查的token，它可能是登陆密码，也可能是通过前置单点登陆接口拿到的token等，具体意义由业务层决定
	 */
        public PLoginInfo(String loginUserId, String loginToken): this(loginUserId, loginToken, null)
        {
        }

        /**
         * 构造方法。
         * 
         * @param loginUserId 传递过来的准一id，保证唯一就可以通信，可能是登陆用户名、也可能是任意不重复的id等，具体意义由业务层决定
         * @param loginToken 用于身份鉴别和合法性检查的token，它可能是登陆密码，也可能是通过前置单点登陆接口拿到的token等，具体意义由业务层决定
         * @param extra 额外信息字符串。本字段目前为保留字段，供上层应用自行放置需要的内容
         */
        public PLoginInfo(String loginUserId, String loginToken, String extra)
        {
            this.LoginUserId = loginUserId;
            this.LoginToken = loginToken;
            this.Extra = extra;
        }
    }
}
