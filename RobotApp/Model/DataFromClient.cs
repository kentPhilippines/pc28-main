using Newtonsoft.Json;
using System;

namespace RobotApp.Model
{
    internal class DataFromClient
    {
        /**
	 * 客户端是否需要读取服务端返回的数据（对服务端而言就是是否需要写返回写据给客户端
	 * ，服务端将据此决定是否要写回数据），本字段对应于 HttpURLConnection.setDoInput(boolean)
	 * 并与之保持一致。
	 * 注：本字段仅用于底层数据通信，请勿作其它用途！ */
        [JsonProperty("doInput")]
        public bool DoInput { get; set; } = true;

        /** 业务处理器id
         * @see  com.eva.framework.HttpController.ends.core.Controller */
        [JsonProperty("processorId")]
        public int ProcessorId { get; private set; } = -9999999;
        /** 作业调度器id
         * @see  com.eva.framework.HttpController.ends.core.Controller */
        [JsonProperty("jobDispatchId")]
        public int JobDispatchId { get; private set; } = -9999999;
        /** 动作id
         * @see  com.eva.framework.HttpController.ends.core.Controller */
        [JsonProperty("actionId")]
        public int ActionId { get; private set; } = -9999999;

        /** 具体业务中：客端发送过来的本次修改新数据(可能为空，理论上与oldData不会同时空）*/
        [JsonProperty("newData")]
        public Object NewData { get; private set; }
        /** 具体业务中：客端发送过来的本次修改前的老数据(可能为空，理论上与newData不会同时空）*/
        [JsonProperty("oldData")]
        public Object OldData { get; private set; }

        [JsonProperty("userId")]
        public String UserId { get; set; }
        [JsonProperty("phoneCode")]
        public String PhoneCode { get; set; }

        /**
         * 可用于REST请求时客户端携带到服务端作为身份验证之用。
         * <p>
         * 本字段可由框架使用者按需使用，非EVA框架必须的。
         *
         * @since 20170223
         */
        [JsonProperty("token")]
        public String Token { get; set; }

        /**
         * 发起HTTP请求的设备类型（默认值为-1，表示未定义）.
         * 此字段为保留字段，具体意义由开发者可自行决定。
         * <p>
         * 当前默认的约定是：0 android平台、1 ios平台、2 web平台。
         */
        [JsonProperty("device")]
        public int Device { get; set; } = -1;



        public DataFromClient setNewData(Object newData)
        {
            this.NewData = newData;
            return this;
        }

        public DataFromClient setOldData(Object oldData)
        {
            this.OldData = oldData;
            return this;
        }

        public DataFromClient setProcessorId(int processorId)
        {
            this.ProcessorId = processorId;
            return this;
        }

        public DataFromClient setJobDispatchId(int jobDispatchId)
        {
            this.JobDispatchId = jobDispatchId;
            return this;
        }

        public DataFromClient setActionId(int actionId)
        {
            this.ActionId = actionId;
            return this;
        }
    }
}