﻿using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Numerics;
using NEO_Block_API.lib;

namespace NEO_Block_API
{
    public class NEP55
    {

        [BsonIgnoreExtraElements]
        public class Asset55
        {
            public Asset55(string mongodbConnStr, string mongodbDatabase, string assetID,string name)
            {
                mongoHelper mh = new mongoHelper();
                JArray JA = mh.GetData(mongodbConnStr, mongodbDatabase, "NEP55asset", "{assetid:'" + assetID + "',name:'"+name+"'}");
                JObject J = (JObject)JA[0];
                assetid = (string)J["assetid"];
                totalsupply = (string)J["totalsupply"];
                name = (string)J["name"];
                symbol = (string)J["symbol"];
                decimals = 8;
            }

            public ObjectId _id { get; set; }
            public string assetid { get; set; }
            public string totalsupply { get; set; }
            public string name { get; set; }
            public string symbol { get; set; }
            public int decimals { get; set; }
        }

        public class AssetBalanceOfAddr
        {
            public AssetBalanceOfAddr(string Assetid,string Name, string Symbol, string Balance)
            {
                assetid = Assetid;
                name = Name;
                symbol = Symbol;
                balance = Balance;
            }

            public string assetid { get; set; }
            public string name { get; set; }
            public string symbol { get; set; }
            public string balance { get; set; }
        }

        [BsonIgnoreExtraElements]
        public class Operator
        {
            public Operator(int Blockindex, string Name, string Txid, int N, JObject notification, int decimals, DateTime time)
            {
                blockindex = Blockindex;
                txid = Txid;
                n = N;
                name = Name;
                asset = (string)notification["contract"];

                JArray JA = (JArray)notification["state"]["value"];

                //name = (string)JA[1]["value"];

                addr = getAddrFromScriptHash((string)JA[2]["value"]);

                string sar = (string)JA[3]["value"];
                sarTxid = "0x" + ThinNeo.Helper.Bytes2HexString(ThinNeo.Helper.HexString2Bytes(sar).Reverse().ToArray());

                type = int.Parse((string)JA[5]["value"]);

                string valueType = (string)JA[6]["type"];
                string valueString = (string)JA[6]["value"];
                if (valueType == "ByteArray")//标准nep5
                {
                    value = decimal.Parse(getNumStrFromHexStr(valueString, decimals));
                }
                else if (valueType == "Integer")//变种nep5
                {
                    value = decimal.Parse(getNumStrFromIntStr(valueString, decimals));
                }
                else//未知情况用-1表示
                {
                    value = -1;
                }

                blocktime = time;

            }
            public ObjectId _id { get; set; }
            public string asset { get; set; }
            public int blockindex { get; set; }
            public string name { get; set; }
            public string addr { get; set; }
            public string sarTxid { get; set; }
            public string txid { get; set; }
            public int n { get; set; }
            public int type { get; set; }
            public decimal value { get; set; }
            public DateTime blocktime { get; set; }
        }

        [BsonIgnoreExtraElements]
        public class OperatorSAR4C
        {
            public OperatorSAR4C(int Blockindex, string Txid, int N, JObject notification, int decimals, DateTime time)
            {
                blockindex = Blockindex;
                txid = Txid;
                n = N;
                asset = (string)notification["contract"];

                JArray JA = (JArray)notification["state"]["value"];

                //name = (string)JA[1]["value"];

                addr = getAddrFromScriptHash((string)JA[1]["value"]);

                string sar = (string)JA[2]["value"];
                sarTxid = "0x" + ThinNeo.Helper.Bytes2HexString(ThinNeo.Helper.HexString2Bytes(sar).Reverse().ToArray());

                type = int.Parse((string)JA[4]["value"]);

                string valueType = (string)JA[5]["type"];
                string valueString = (string)JA[5]["value"];
                if (valueType == "ByteArray")//标准nep5
                {
                    value = decimal.Parse(getNumStrFromHexStr(valueString, decimals));
                }
                else if (valueType == "Integer")//变种nep5
                {
                    value = decimal.Parse(getNumStrFromIntStr(valueString, decimals));
                }
                else//未知情况用-1表示
                {
                    value = -1;
                }

                blocktime = time;

            }
            public ObjectId _id { get; set; }
            public string asset { get; set; }
            public int blockindex { get; set; }
            public string addr { get; set; }
            public string sarTxid { get; set; }
            public string txid { get; set; }
            public int n { get; set; }
            public int type { get; set; }
            public decimal value { get; set; }
            public DateTime blocktime { get; set; }
        }

        [BsonIgnoreExtraElements]
        public class SAR4C
        {
            public SAR4C(int Blockindex, string Txid, int N, JObject notification, int decimals, DateTime time)
            {
                blockindex = Blockindex;
                txid = Txid;
                n = N;
                asset = (string)notification["contract"];

                JArray JA = (JArray)notification["state"]["value"];

                addr = getAddrFromScriptHash((string)JA[1]["value"]);

                string sar = (string)JA[2]["value"];
                sarTxid = "0x" + ThinNeo.Helper.Bytes2HexString(ThinNeo.Helper.HexString2Bytes(sar).Reverse().ToArray());

                status = 1;
                blocktime = time;

            }
            public ObjectId _id { get; set; }
            public string asset { get; set; }
            public int blockindex { get; set; }
            public string addr { get; set; }
            public string sarTxid { get; set; }
            public string txid { get; set; }
            public int n { get; set; }
            public int status { get; set; }
            public DateTime blocktime { get; set; }

            public static implicit operator SAR4C(BsonDocument v)
            {
                throw new NotImplementedException();
            }
        }

        [BsonIgnoreExtraElements]
        public class Operator4C
        {
            public Operator4C(int Blockindex, string Txid, int N, JObject notification, int decimals, DateTime time)
            {
                blockindex = Blockindex;
                txid = Txid;
                n = N;
                asset = (string)notification["contract"];

                JArray JA = (JArray)notification["state"]["value"];

                addr = getAddrFromScriptHash((string)JA[2]["value"]);

                string valueType = (string)JA[3]["type"];
                string valueString = (string)JA[3]["value"];
                if (valueType == "ByteArray")//标准nep5
                {
                    value = decimal.Parse(getNumStrFromHexStr(valueString, decimals));
                }
                else if (valueType == "Integer")//变种nep5
                {
                    value = decimal.Parse(getNumStrFromIntStr(valueString, decimals));
                }
                else//未知情况用-1表示
                {
                    value = -1;
                }

                type = int.Parse((string)JA[4]["value"]);

                blocktime = time;
                status = 1;
            }
            public ObjectId _id { get; set; }
            public string asset { get; set; }
            public int blockindex { get; set; }
            public string addr { get; set; }
            public string txid { get; set; }
            public int n { get; set; }
            public int type { get; set; }
            public decimal value { get; set; }
            public DateTime blocktime { get; set; }
            //1:有效   2:无效
            public int status { get; set; }
        }

        [BsonIgnoreExtraElements]
        public class Transfer55
        {
            public Transfer55(JObject tfJ)
            {
                blockindex = (int)tfJ["blockindex"];
                name = (string)tfJ["name"];
                txid = (string)tfJ["txid"];
                n = (int)tfJ["n"];
                asset = (string)tfJ["asset"];
                from = (string)tfJ["from"];
                to = (string)tfJ["to"];
                value = (decimal)tfJ["value"];
                //blocktime = (DateTime)tfJ["blocktime"];
            }

            public Transfer55(int Blockindex, string Name, string Txid, int N, JObject notification, int decimals, DateTime time)
            {
                blockindex = Blockindex;
                txid = Txid;
                n = N;
                name = Name;
                asset = (string)notification["contract"];

                JArray JA = (JArray)notification["state"]["value"];

                from = getAddrFromScriptHash((string)JA[2]["value"]);
                to = getAddrFromScriptHash((string)JA[3]["value"]);

                string valueType = (string)JA[4]["type"];
                string valueString = (string)JA[4]["value"];
                if (valueType == "ByteArray")//标准nep5
                {
                    value = decimal.Parse(getNumStrFromHexStr(valueString, decimals));
                }
                else if (valueType == "Integer")//变种nep5
                {
                    value = decimal.Parse(getNumStrFromIntStr(valueString, decimals));
                }
                else//未知情况用-1表示
                {
                    value = -1;
                }

                blocktime = time;

            }

            public ObjectId _id { get; set; }
            public int blockindex { get; set; }
            public string name { get; set; }
            public string txid { get; set; }
            public int n { get; set; }
            public string asset { get; set; }
            public string from { get; set; }
            public string to { get; set; }
            public decimal value { get; set; }
            public DateTime blocktime { get; set; }
        }

        private static string getAddrFromScriptHash(string scripitHash)
        {
            if (scripitHash != string.Empty)
            {
                return ThinNeo.Helper.GetAddressFromScriptHash(ThinNeo.Helper.HexString2Bytes(scripitHash));
            }
            else
            { return string.Empty; } //ICO mintToken 等情况    
        }

        //十六进制转数值（考虑精度调整）
        private static string getNumStrFromHexStr(string hexStr, int decimals)
        {
            //小头换大头
            byte[] bytes = ThinNeo.Helper.HexString2Bytes(hexStr).Reverse().ToArray();
            string hex = ThinNeo.Helper.Bytes2HexString(bytes);
            //大整数处理，默认第一位为符号位，0代表正数，需要补位
            hex = "0" + hex;

            BigInteger bi = BigInteger.Parse(hex, System.Globalization.NumberStyles.AllowHexSpecifier);

            return changeDecimals(bi, decimals);
        }

        //大整数文本转数值（考虑精度调整）
        private static string getNumStrFromIntStr(string intStr, int decimals)
        {
            BigInteger bi = BigInteger.Parse(intStr);

            return changeDecimals(bi, decimals);
        }

        //根据精度处理小数点（大整数模式处理）
        private static string changeDecimals(BigInteger value, int decimals)
        {
            BigInteger bi = BigInteger.DivRem(value, BigInteger.Pow(10, decimals), out BigInteger remainder);
            string numStr = bi.ToString();
            if (remainder != 0)//如果余数不为零才添加小数点
            {
                //按照精度，处理小数部分左侧补零与右侧去零
                int AddLeftZeoCount = decimals - remainder.ToString().Length;
                string remainderStr = cloneStr("0", AddLeftZeoCount) + removeRightZero(remainder);

                numStr = string.Format("{0}.{1}", bi, remainderStr);
            }

            return numStr;
        }

        //生成左侧补零字符串
        private static string cloneStr(string str, int cloneCount)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i <= cloneCount; i++)
            {
                sb.Append(str);
            }
            return sb.ToString();
        }

        //去除大整数小数（余数）部分的右侧0
        private static BigInteger removeRightZero(BigInteger bi)
        {
            string strReverse0 = strReverse(bi.ToString());
            BigInteger bi0 = BigInteger.Parse(strReverse0);
            string strReverse1 = strReverse(bi0.ToString());

            return BigInteger.Parse(strReverse1);
        }

        //反转字符串
        private static string strReverse(string str)
        {
            char[] arr = str.ToCharArray();
            Array.Reverse(arr);

            return new string(arr);
        }
    }
}