using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary
{
    public class IMConstant
    {
        public const string PERMISSION_PRIVATE_KEY = "<RSAKeyValue><Modulus>yW5c+ttKY0hc/bYMNxVCTzd55HkPYN6ha05NHiuvfKLsTJgCThqmtvvDiLyoCQB+N8w8KPRQ1HIYAoRTz59VJvN2BTwYuEPUZ3HjWrz/DyAdyyO1CIs3WSQjKZmXWVmJJUjD9aDqAwHYr2nuq7RlF0NqqiGiXAUVMOp140GLlUmhqk3mYeNiqg3qunTN30omLCRawqZv8jZcO1L1SKP/KiSSfU97fUiY31yqC41a/mKFfbWwVtDlgseGxnuH88ugAIwkPobpqp8QX/aeW5RAxGwLBcrgV2Gbfhj0sck7CidSz8bg7so4XVDjLTvieRUu3o55GbVfTebVLjw6aFNzaQ==</Modulus><Exponent>AQAB</Exponent><P>7NDMSYqXkglEaArVGqUQ/bClEKBNHhBWb1n5ZV6PWzp18c8jPDhkYzQrRQQ8t58hPvRH6jrR9taO6RP8+/eRslPFiWGa/ag4Rk02iIoKM28bdLAGOdnHpKfaEdQkOrMBHCShqlQzrFznQysXclG0WkPLR5s1adtpMe11daAZlyc=</P><Q>2b++SrwPtrl5ztvklyzPE+5F7gELlLsI0YAtDBkEIymcAmI+0MdNytSy2MSJ2r2wPFGLdwYjGcco7mx7WvGzt4ILdIbu54w62odaJhNGnbLRmG4PkHjkSSj0XpGe1TVvSLyIpNeVb23ylYOwWBepph8hruezViWImIz7RQybuu8=</Q><DP>FYWUS0qxI7B76wiw/U7rEGMxXSV0XLsXX99JbxD87kmN1oAAr8RgOzPOiuMrsXRgzRvePUdDNa+iJUSPxUZmk1JRrX6VW2AiXSE/R4FkE/CRCjXFGxTTs/8dLmmdMUh7XVdm6dflKlD3I3+TDeym+10V6FgOrqQmF0eBWUNHkBk=</DP><DQ>h3dmp5AAJqmVQkBDIlnaKogeMUetMMZ7YfrgC5q0nDuSt1jvw2t61iklav461T8VmTESFuZWh/8DU/FhfN7J8+yPu9sGXj87jiCO0QlE8W8CPlaakELloy47eWoW6oXnydShHgyRB1XbiXD4EJYIETVk+y9ivsFzDZUH7Zk+eTk=</DQ><InverseQ>y3pIcYfA84pN7370LSNEhCFXFiEMNSaYbKefXdVSbch2UuDI+zPVmbqxkWWhL8H+/36FmWgXN/msFZM9zFEGaDrgZDAl+aLXEqe68xmfdUfbamHLlt8ZcrbcBxLWAtkAFa9F2E1PzewfTJUcBbS4GNh/0bkiQEklOv5Lsg9xxbI=</InverseQ><D>gV2Z5Zp02fKdlmwWyEGlBo9Qf4WodMRG003EhX31BIz2SfLOeC7xp09mMylGmhFw/pDyijLuvqJP/T3TPeLMo2l1AMOp9J3LimxAYgRDowkzWUWjfMs8TsnAs3TrGP1B8WocQGKKKEhFS9My75+51Qj9NirHxWWWSIYsFDoCVWuCsbcZwlgFIuOzUGY5C/x3GoQeQV1V/93ivbQDLH/21D8c3xKsBYhXMU6w8ioMrIckEuQLerRmtH4wSTy/gEzV+PedFVicRvDMtbBQGtfgmHIKy4cnjvrHgiX7dX/4r3x2ruprStwp538AhFo3IdC5z7jYz9S28ovlxbcKYAqcyQ==</D></RSAKeyValue>";
        public const string PERMISSION_PUBLIC_KEY = "<RSAKeyValue><Modulus>yW5c+ttKY0hc/bYMNxVCTzd55HkPYN6ha05NHiuvfKLsTJgCThqmtvvDiLyoCQB+N8w8KPRQ1HIYAoRTz59VJvN2BTwYuEPUZ3HjWrz/DyAdyyO1CIs3WSQjKZmXWVmJJUjD9aDqAwHYr2nuq7RlF0NqqiGiXAUVMOp140GLlUmhqk3mYeNiqg3qunTN30omLCRawqZv8jZcO1L1SKP/KiSSfU97fUiY31yqC41a/mKFfbWwVtDlgseGxnuH88ugAIwkPobpqp8QX/aeW5RAxGwLBcrgV2Gbfhj0sck7CidSz8bg7so4XVDjLTvieRUu3o55GbVfTebVLjw6aFNzaQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        public static readonly string UserDataPath = @"C:\AI28";

        //public const string ROBOT_SERVER = "http://robot.pc28.space";
        public const string ROBOT_SERVER = "http://robot.67365.cyou";
        //public const string ROBOT_SERVER = "http://localhost:8888";
        public const string API_URL = "http://imserver.lx011.com/api";        
        public const string SERVER_URL = "http://imserver.lx011.com/imapi";
        public const string WS_URL = "ws://imserver.lx011.com/msg_gateway";


        /*public const string ROBOT_SERVER = "http://localhost:8888";
        public const string API_URL = "https://sb.weiyuchat.com/";
        public const string SERVER_URL = "https://sa.weiyuchat.com";
        public const string SERVER_IP = "16.162.248.99";
        public const int SERVER_PORT = 9801;*/
    }
}
