using System;
using System.Collections.Generic;
using System.Text;
using NEO_Block_API.lib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NEO_Block_API.Controllers;
using ThinNeo;
using Microsoft.Extensions.Configuration;

namespace Block_API.Controllers
{
    class Business
    {
        mongoHelper mh = new mongoHelper();

        Contract ct = new Contract();

        public string hashSDUSD_prinet = string.Empty;
        public string hashSAR4C_prinet = string.Empty;
        public string hashSNEO_prinet = string.Empty;
        public string hashORACLE_prinet = string.Empty;
        public string addrSAR4C_prinet = string.Empty;

        public string hashSDUSD_testnet = string.Empty;
        public string hashSAR4C_testnet = string.Empty;
        public string hashSNEO_testnet = string.Empty;
        public string hashORACLE_testnet = string.Empty;
        public string addrSAR4C_testnet = string.Empty;

        public string hashSDUSD_mainnet = string.Empty;
        public string hashSAR4C_mainnet = string.Empty;
        public string hashSNEO_mainnet = string.Empty;
        public string hashORACLE_mainnet = string.Empty;
        public string addrSAR4C_mainnet = string.Empty;


        public Business()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection()    //将配置文件的数据加载到内存中
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())   //指定配置文件所在的目录
                .AddJsonFile("addrsettings.json", optional: true, reloadOnChange: false)  //指定加载的配置文件
                .Build();    //编译成对象  

            hashSDUSD_prinet = config["hashSDUSD_prinet"];
            hashSAR4C_prinet = config["hashSAR4C_prinet"];
            hashSNEO_prinet = config["hashSNEO_prinet"];
            hashORACLE_prinet = config["hashORACLE_prinet"];
            addrSAR4C_prinet = config["addrSAR4C_prinet"];

            hashSDUSD_testnet = config["hashSDUSD_testnet"];
            hashSAR4C_testnet = config["hashSAR4C_testnet"];
            hashSNEO_testnet = config["hashSNEO_testnet"];
            hashORACLE_testnet = config["hashORACLE_testnet"];
            addrSAR4C_testnet = config["addrSAR4C_testnet"];

            hashSDUSD_mainnet = config["hashSDUSD_mainnet"];
            hashSAR4C_mainnet = config["hashSAR4C_mainnet"];
            hashSNEO_mainnet = config["hashSNEO_mainnet"];
            hashORACLE_mainnet = config["hashORACLE_mainnet"];
            addrSAR4C_mainnet = config["addrSAR4C_mainnet"];
        }

        public JObject getNgdBlockcount(string neoRpcUrl_mainnet)
        {
            httpHelper hh = new httpHelper();
            var resp = hh.Post(neoRpcUrl_mainnet, "{'jsonrpc':'2.0','method':'getblockcount','params':[],'id':1}", System.Text.Encoding.UTF8, 1);

            string ret = (string)JObject.Parse(resp)["result"];
            JObject ob = new JObject();
            ob.Add("blockcount", ret);
            return ob;
        }

        public JObject getOperatedFee(string mongodbConnStr, string mongodbDatabase, string addr)
        {
            string findFliter = string.Empty;
            if (addr.Length>0)
            {
                findFliter = "{addr:'" + addr + "'}";
            }
            else
            {
                findFliter = "{}";
            }
            JArray arry = mh.GetData(mongodbConnStr, mongodbDatabase, "operatedFee", findFliter);
           
            decimal sds_value = 0; //所有sdsFee总值
            decimal sdusd_value = 0; //所有sdusdFee总值
            foreach (JObject operated in arry)
            {
                sds_value += (decimal)operated["sdsFee"];
                sdusd_value += (decimal)operated["sdusdFee"];
            }
            JObject ob = new JObject();
            ob.Add("sdsFee", sds_value);
            ob.Add("sdusdFee", sdusd_value);
            return ob;
        }

        public JObject getStaticReport(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl,string hashSDUSD,string hashSNEO, string hashSAR4C,string hashORACLE,
            string addrSAR4C)
        {
            JObject ob = new JObject();

            //SDUSD totalSupply
            string script = invokeScript(new Hash160(hashSDUSD), "totalSupply",null);
            JObject jo = ct.invokeScript(neoCliJsonRPCUrl,script);
            string balanceType = (string)((JArray)jo["stack"])[0]["type"];
            string balanceStr = (string)((JArray)jo["stack"])[0]["value"];
            string balanceBigint = NEO_Block_API.NEP5.getNumStrFromStr(balanceType, balanceStr, 8);
            decimal sdusdTotal = decimal.Parse(balanceBigint);

            //SAR4C bond
            script = invokeScript(new Hash160(hashSAR4C), "getBondGlobal");
            jo = ct.invokeScript(neoCliJsonRPCUrl, script);
            balanceType = (string)((JArray)jo["stack"])[0]["type"];
            balanceStr = (string)((JArray)jo["stack"])[0]["value"];
            balanceBigint = NEO_Block_API.NEP5.getNumStrFromStr(balanceType, balanceStr, 8);
            decimal bondTotal = decimal.Parse(balanceBigint);
            ob.Add("sdusdTotal", (sdusdTotal - bondTotal));


            //SNEO lock
            script = invokeScript(new Hash160(hashSNEO), "balanceOf", "(addr)"+addrSAR4C);
            jo = ct.invokeScript(neoCliJsonRPCUrl, script);
            balanceType = (string)((JArray)jo["stack"])[0]["type"];
            balanceStr = (string)((JArray)jo["stack"])[0]["value"];
            balanceBigint = NEO_Block_API.NEP5.getNumStrFromStr(balanceType, balanceStr, 8);
            decimal sneoLocked =  decimal.Parse(balanceBigint);
            ob.Add("lockedTotal", sneoLocked);

            //SDS fee
            ob.Add("sdsFeeTotal", getOperatedFee(mongodbConnStr,mongodbDatabase,"")["sdsFee"]);

            //Oracle sneo_price
            script = invokeScript(new Hash160(hashORACLE), "getTypeB", "(str)sneo_price");
            jo = ct.invokeScript(neoCliJsonRPCUrl, script);
            balanceType = (string)((JArray)jo["stack"])[0]["type"];
            balanceStr = (string)((JArray)jo["stack"])[0]["value"];
            balanceBigint = NEO_Block_API.NEP5.getNumStrFromStr(balanceType, balanceStr,4);

            decimal sneo_price = decimal.Parse(balanceBigint);

            if ((sdusdTotal - bondTotal) > 0)
            {
                ob.Add("mortgageRate", decimal.Round((sneoLocked * sneo_price)/ ((sdusdTotal - bondTotal)*10000),4)*100+"%");
            }
            else {
                ob.Add("mortgageRate", "150%");
            }

            return ob;
        }

        private string invokeScript(Hash160 scripthash, string methodname, params string[] subparam)
        {
            byte[] data = null;
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                MyJson.JsonNode_Array array = new MyJson.JsonNode_Array();
                if (subparam != null && subparam.Length > 0)
                {
                    for (var i = 0; i < subparam.Length; i++)
                    {
                        array.AddArrayValue(subparam[i]);
                    }
                }
                sb.EmitParamJson(array);
                sb.EmitPushString(methodname);
                sb.EmitAppCall(scripthash);
                data = sb.ToArray();
            }
            string script = ThinNeo.Helper.Bytes2HexString(data);

            return script;
        }

    }
}
