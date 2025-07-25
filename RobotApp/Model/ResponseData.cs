using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotApp.Model
{
    internal class ResponseData<T>
    {
        /// <summary>
        /// 
        /// </summary>
        public int status { get; set; }

        /// <summary>
        /// 成功
        /// </summary>
        public string msg { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public T data { get; set; }
    }
}
