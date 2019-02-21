using System;
using System.Collections.Generic;
using System.Text;
using NEO_Block_API.lib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NEO_Block_API;
using ThinNeo;
using Microsoft.Extensions.Configuration;
using System.Numerics;
using NEO_Block_API.Controllers;

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
        public string oldAddrSAR4C_prinet = string.Empty;
        public string hashSAR4B_prinet = string.Empty;
        public string hashSDS_prinet = string.Empty;
        public string hashTokenized_prinet = string.Empty;

        public string hashSDUSD_testnet = string.Empty;
        public string hashSAR4C_testnet = string.Empty;
        public string hashSNEO_testnet = string.Empty;
        public string hashORACLE_testnet = string.Empty;
        public string addrSAR4C_testnet = string.Empty;
        public string oldAddrSAR4C_testnet = string.Empty;
        public string hashSAR4B_testnet = string.Empty;
        public string hashSDS_testnet = string.Empty;
        public string hashTokenized_testnet = string.Empty;

        public string hashSDUSD_mainnet = string.Empty;
        public string hashSAR4C_mainnet = string.Empty;
        public string hashSNEO_mainnet = string.Empty;
        public string hashORACLE_mainnet = string.Empty;
        public string addrSAR4C_mainnet = string.Empty;
        public string oldAddrSAR4C_mainnet = string.Empty;
        public string hashSAR4B_mainnet = string.Empty;
        public string hashSDS_mainnet = string.Empty;
        public string hashTokenized_mainnet = string.Empty;

        private const int EIGHT_ZERO = 100000000; 

        private const int SIX_ZERO = 1000000; 
        private const int TWO_ZERO = 100;
        private const ulong SIXTEEN_ZERO = 10000000000000000;

        private const string GAS_ASSET = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";

        private const string NEO_ASSET = "0xc56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b";

        public Business()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection()    //将配置文件的数据加载到内存中
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())   //指定配置文件所在的目录
                .AddJsonFile("addrsettings.json", optional: true, reloadOnChange: false)  //指定加载的配置文件
                .Build();    //编译成对象  

            hashSDUSD_prinet = config["hashSDUSD_prinet"];
            hashSAR4C_prinet = config["hashSAR4C_prinet"];
            hashSAR4B_prinet = config["hashSAR4B_prinet"];
            hashSNEO_prinet = config["hashSNEO_prinet"];
            hashORACLE_prinet = config["hashORACLE_prinet"];
            addrSAR4C_prinet = config["addrSAR4C_prinet"];
            oldAddrSAR4C_prinet = config["oldAddrSAR4C_prinet"];
            hashSDS_prinet = config["hashSDS_prinet"];
            hashTokenized_prinet = config["hashTokenized_prinet"];

            hashSDUSD_testnet = config["hashSDUSD_testnet"];
            hashSAR4C_testnet = config["hashSAR4C_testnet"];
            hashSAR4B_testnet = config["hashSAR4B_testnet"];
            hashSNEO_testnet = config["hashSNEO_testnet"];
            hashORACLE_testnet = config["hashORACLE_testnet"];
            addrSAR4C_testnet = config["addrSAR4C_testnet"];
            oldAddrSAR4C_testnet = config["oldAddrSAR4C_testnet"];
            hashSDS_testnet = config["hashSDS_testnet"];
            hashTokenized_testnet = config["hashTokenized_testnet"];

            hashSDUSD_mainnet = config["hashSDUSD_mainnet"];
            hashSAR4C_mainnet = config["hashSAR4C_mainnet"];
            hashSAR4B_mainnet = config["hashSAR4B_mainnet"];
            hashSNEO_mainnet = config["hashSNEO_mainnet"];
            hashORACLE_mainnet = config["hashORACLE_mainnet"];
            addrSAR4C_mainnet = config["addrSAR4C_mainnet"];
            oldAddrSAR4C_mainnet = config["oldAddrSAR4C_mainnet"];
            hashSDS_mainnet = config["hashSDS_mainnet"];
            hashTokenized_mainnet = config["hashTokenized_mainnet"];
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
            if (addr.Length > 0)
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

        public JObject getStaticReport(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string hashSDUSD, string hashSNEO, string hashSAR4C, string hashORACLE,
            string addrSAR4C,string oldAddrSAR4C)
        {
            JObject ob = new JObject();

            //SDUSD totalSupply
            string script = invokeScript(new Hash160(hashSDUSD), "totalSupply", null);
            JObject jo = ct.invokeScript(neoCliJsonRPCUrl, script);
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
            script = invokeScript(new Hash160(hashSNEO), "balanceOf", "(addr)" + addrSAR4C);
            jo = ct.invokeScript(neoCliJsonRPCUrl, script);
            balanceType = (string)((JArray)jo["stack"])[0]["type"];
            balanceStr = (string)((JArray)jo["stack"])[0]["value"];
            balanceBigint = NEO_Block_API.NEP5.getNumStrFromStr(balanceType, balanceStr, 8);

            //原SAR合约中锁定的SNEO
            string balanceBigintOld = "0";
            if (oldAddrSAR4C != "") {
                script = invokeScript(new Hash160(hashSNEO), "balanceOf", "(addr)" + oldAddrSAR4C);
                jo = ct.invokeScript(neoCliJsonRPCUrl, script);
                balanceType = (string)((JArray)jo["stack"])[0]["type"];
                balanceStr = (string)((JArray)jo["stack"])[0]["value"];
                balanceBigintOld = NEO_Block_API.NEP5.getNumStrFromStr(balanceType, balanceStr, 8);
            }

            decimal sneoLocked = decimal.Parse(balanceBigint) + decimal.Parse(balanceBigintOld);
            ob.Add("lockedTotal", sneoLocked);

            //SDS fee
            ob.Add("sdsFeeTotal", getOperatedFee(mongodbConnStr, mongodbDatabase, "")["sdsFee"]);

            //Oracle sneo_price
            script = invokeScript(new Hash160(hashORACLE), "getTypeB", "(str)sneo_price");
            jo = ct.invokeScript(neoCliJsonRPCUrl, script);
            balanceType = (string)((JArray)jo["stack"])[0]["type"];
            balanceStr = (string)((JArray)jo["stack"])[0]["value"];
            balanceBigint = NEO_Block_API.NEP5.getNumStrFromStr(balanceType, balanceStr, 0);

            decimal sneo_price = decimal.Parse(balanceBigint);

            if ((sdusdTotal - bondTotal) > 0)
            {
                ob.Add("mortgageRate", decimal.Round((sneoLocked * sneo_price) / ((sdusdTotal - bondTotal) * 100000000), 4) * 100 + "%");
            }
            else {
                ob.Add("mortgageRate", "150%");
            }

            //Oracle liquidate_line_rate_c    150
            script = invokeScript(new Hash160(hashORACLE), "getTypeA", "(str)liquidate_line_rate_c");
            jo = ct.invokeScript(neoCliJsonRPCUrl, script);
            balanceType = (string)((JArray)jo["stack"])[0]["type"];
            balanceStr = (string)((JArray)jo["stack"])[0]["value"];
            ob.Add("liquidationRate", NEO_Block_API.NEP5.getNumStrFromStr(balanceType, balanceStr, 0) + "%");

            //Oracle liquidate_dis_rate_c   90
            script = invokeScript(new Hash160(hashORACLE), "getTypeA", "(str)liquidate_dis_rate_c");
            jo = ct.invokeScript(neoCliJsonRPCUrl, script);
            balanceType = (string)((JArray)jo["stack"])[0]["type"];
            balanceStr = (string)((JArray)jo["stack"])[0]["value"];
            string disRateC = NEO_Block_API.NEP5.getNumStrFromStr(balanceType, balanceStr, 0);
            ob.Add("liquidationDiscount", (100 - decimal.Parse(disRateC)) + "%");

            //2% sds fee
            ob.Add("feeRate", "2%");

            //Oracle debt_top_c
            script = invokeScript(new Hash160(hashORACLE), "getTypeA", "(str)debt_top_c");
            jo = ct.invokeScript(neoCliJsonRPCUrl, script);
            balanceType = (string)((JArray)jo["stack"])[0]["type"];
            balanceStr = (string)((JArray)jo["stack"])[0]["value"];
            ob.Add("issueingCeiling", decimal.Parse(NEO_Block_API.NEP5.getNumStrFromStr(balanceType, balanceStr, 8)));



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

        private BigInteger getNEOPrice(string neoCliJsonRPCUrl, string hashORACLE) {
            //Oracle sneo_price
            string script = invokeScript(new Hash160(hashORACLE), "getTypeB", "(str)sneo_price");
            JObject jo = ct.invokeScript(neoCliJsonRPCUrl, script);
            string balanceType = (string)((JArray)jo["stack"])[0]["type"];
            string balanceStr = (string)((JArray)jo["stack"])[0]["value"];
            string balanceBigint = NEO_Block_API.NEP5.getNumStrFromStr(balanceType, balanceStr, 0);
            return BigInteger.Parse(balanceBigint);
        }

        private BigInteger getSDSPrice(string neoCliJsonRPCUrl, string hashORACLE)
        {
            //Oracle sneo_price
            string script = invokeScript(new Hash160(hashORACLE), "getTypeB", "(str)sds_price");
            JObject jo = ct.invokeScript(neoCliJsonRPCUrl, script);
            string balanceType = (string)((JArray)jo["stack"])[0]["type"];
            string balanceStr = (string)((JArray)jo["stack"])[0]["value"];
            string balanceBigint = NEO_Block_API.NEP5.getNumStrFromStr(balanceType, balanceStr, 0);
            return BigInteger.Parse(balanceBigint);
        }

        private BigInteger getTypeBPrice(string neoCliJsonRPCUrl, string hashORACLE,string priceKey)
        {
            string balanceBigint = "0";
            try
            {
                string script = invokeScript(new Hash160(hashORACLE), "getTypeB", "(str)" + priceKey);
                JObject jo = ct.invokeScript(neoCliJsonRPCUrl, script);
                string balanceType = (string)((JArray)jo["stack"])[0]["type"];
                string balanceStr = (string)((JArray)jo["stack"])[0]["value"];
                balanceBigint = NEO_Block_API.NEP5.getNumStrFromStr(balanceType, balanceStr, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine("getTypeBPrice:"+e.Message);
            }
            return BigInteger.Parse(balanceBigint);
        }

        private BigInteger getTypeA(string neoCliJsonRPCUrl, string hashORACLE,string key) {
            string script = invokeScript(new Hash160(hashORACLE), "getTypeA", "(str)"+key);
            JObject jo = ct.invokeScript(neoCliJsonRPCUrl, script);
            string type = (string)((JArray)jo["stack"])[0]["type"];
            string value = (string)((JArray)jo["stack"])[0]["value"];
            string bigint = NEO_Block_API.NEP5.getNumStrFromStr(type, value, 0);
            return BigInteger.Parse(bigint);
            
        }

        private long getCurrBlock(string mongodbConnStr,string mongodbDatabase) {
            return (long)(mh.GetData(mongodbConnStr, mongodbDatabase, "system_counter", "{counter:'block'}")[0]["lastBlockindex"]) + 1;
        }

        public JArray processSARDetail(JArray arrs, string mongodbConnStr, string mongodbDatabase,string neoCliJsonRPCUrl,string hashSAR4C,string hashORACLE)
        {
            JArray ret = new JArray();

            foreach (JObject ob in arrs) {
                string addr = (string)ob["addr"];
                if (!string.IsNullOrEmpty(addr))
                {
                    try
                    {
                        string script = invokeScript(new Hash160(hashSAR4C), "getSAR4C", "(addr)" + addr);
                        JObject jo = ct.invokeScript(neoCliJsonRPCUrl, script);
                        JObject sarDetail = new JObject();
                        //stack arrays
                        string type = (string)((JArray)jo["stack"])[0]["type"];
                        JArray sar = (JArray)((JArray)jo["stack"])[0]["value"];

                        //SAR地址
                        string ownerValue = (string)sar[0]["value"];
                        string owner = ThinNeo.Helper.GetAddressFromScriptHash(ThinNeo.Helper.HexString2Bytes(ownerValue));
                        sarDetail.Add("owner",owner);
                        Console.WriteLine("owner:"+owner);
                        //创建SAR的txid
                        string txidValue = (string)sar[1]["value"];
                        string txid = "0x" + ThinNeo.Helper.Bytes2HexString(ThinNeo.Helper.HexString2Bytes(txidValue).Reverse().ToArray());
                        sarDetail.Add("txid",txid);
                        Console.WriteLine("txid:" + txid);

                        //hasLocked
                        type = (string)sar[2]["type"];
                        string value = (string)sar[2]["value"];
                        string lockedSrc = NEP5.getNumStrFromStr(type, value, 0);
                        string locked = NEP5.getNumStrFromStr(type, value, 8);
                        sarDetail.Add("locked", locked);
                        Console.WriteLine("locked:" + locked);

                        //hasDrawed
                        type = (string)sar[3]["type"];
                        value = (string)sar[3]["value"];
                        string hasDrawedSrc = NEP5.getNumStrFromStr(type, value, 0);
                        string hasDrawed = NEP5.getNumStrFromStr(type, value, 8);
                        sarDetail.Add("hasDrawed", hasDrawed);
                        Console.WriteLine("hasDrawed:" + hasDrawed);

                        //资产类型
                        type = (string)sar[4]["type"];
                        value = (string)sar[4]["value"];
                        string assetType = System.Text.Encoding.UTF8.GetString(ThinNeo.Helper.HexString2Bytes(value));
                        sarDetail.Add("assetType", assetType);
                        Console.WriteLine("assetType:" + assetType);


                        //bondLocked
                        type = (string)sar[6]["type"];
                        value = (string)sar[6]["value"];
                        string bondLocked = NEP5.getNumStrFromStr(type, value, 8);
                        sarDetail.Add("bondLocked", bondLocked);
                        Console.WriteLine("bondLocked:" + bondLocked);

                        //bondDrawed
                        type = (string)sar[7]["type"];
                        value = (string)sar[7]["value"];
                        string bondDrawed = NEP5.getNumStrFromStr(type, value, 8);
                        sarDetail.Add("bondDrawed", bondDrawed);
                        Console.WriteLine("bondDrawed:" + bondDrawed);

                        //lastHeight
                        type = (string)sar[8]["type"];
                        value = (string)sar[8]["value"];
                        string lastHeight = NEP5.getNumStrFromStr(type, value, 0);
                        sarDetail.Add("lastHeight", lastHeight);
                        Console.WriteLine("lastHeight:" + lastHeight);

                        //fee
                        type = (string)sar[9]["type"];
                        value = (string)sar[9]["value"];
                        string feeSrc = NEP5.getNumStrFromStr(type, value, 0);
                        string fee = NEP5.getNumStrFromStr(type, value, 8);
                        sarDetail.Add("fee", fee);

                        //sdsFee
                        type = (string)sar[10]["type"];
                        value = (string)sar[10]["value"];
                        string sdsFee = NEP5.getNumStrFromStr(type, value, 8);
                        sarDetail.Add("sdsFee", sdsFee);

                        //计算剩余可发行量SDUSD
                        BigInteger neo_price = getNEOPrice(neoCliJsonRPCUrl,hashORACLE);
                        BigInteger rate = getTypeA(neoCliJsonRPCUrl,hashORACLE, "liquidate_line_rate_c");
                        if (lockedSrc != "0")
                        {
                            BigInteger totalPublish = neo_price * BigInteger.Parse(lockedSrc) / (rate * SIX_ZERO);
                            BigInteger remainPublish = totalPublish - BigInteger.Parse(hasDrawedSrc);
                            if (remainPublish >= 0)
                            {
                                sarDetail.Add("remainDrawed", NEP5.getNumStrFromStr("Integer", remainPublish.ToString(), 8));
                                Console.WriteLine("remainDrawed");

                                //剩余发行大于0，才有剩余可提现SNEO
                                BigInteger remainDrawedNEO = BigInteger.Parse(lockedSrc) - (BigInteger.Parse(hasDrawedSrc) * rate * SIX_ZERO / neo_price);
                                if (remainDrawedNEO >= 0)
                                {
                                    sarDetail.Add("remainDrawedNEO", NEP5.getNumStrFromStr("Integer", remainDrawedNEO.ToString(), 8));
                                    Console.WriteLine("remainDrawedNEO");
                                }
                                else
                                {
                                    sarDetail.Add("remainDrawedNEO", "0");
                                }
                            }
                            else {
                                sarDetail.Add("remainDrawed", "0");
                                sarDetail.Add("remainDrawedNEO", "0");
                            }
                        }
                        else {
                            sarDetail.Add("remainDrawed", "0");
                            sarDetail.Add("remainDrawedNEO", "0");
                        }

                        //计算当前抵押率
                        if (hasDrawedSrc != "0")
                        {
                            //乘以100的值
                            decimal mortgagerate = Decimal.Parse(neo_price.ToString()) * Decimal.Parse(lockedSrc) / (Decimal.Parse(hasDrawedSrc) * SIX_ZERO);
                            sarDetail.Add("mortgageRate", mortgagerate);
                            Console.WriteLine("mortgageRate");

                            //SAR状态 1:安全，2:不安全
                            int status = 1;
                            if (mortgagerate.CompareTo(Decimal.Parse(rate.ToString())) < 0) {
                                status = 2;
                            }
                            sarDetail.Add("status", status);

                            decimal liqPrice = Decimal.Parse(hasDrawedSrc) * Decimal.Parse(rate.ToString()) / (Decimal.Parse(lockedSrc) * TWO_ZERO);
                            sarDetail.Add("liqPrice", liqPrice);
                            Console.WriteLine("liqPrice");

                            //计算待缴手续费
                            long blockHeight = getCurrBlock(mongodbConnStr,mongodbDatabase);
                            BigInteger fee_rate = getTypeA(neoCliJsonRPCUrl, hashORACLE, "fee_rate_c");
                            BigInteger sdsPrice = getSDSPrice(neoCliJsonRPCUrl, hashORACLE);
                            if (blockHeight > BigInteger.Parse(lastHeight))
                            {
                                BigInteger currFee = (blockHeight - BigInteger.Parse(lastHeight)) * BigInteger.Parse(hasDrawedSrc) * fee_rate / SIXTEEN_ZERO;

                                BigInteger needUSDFee = currFee + BigInteger.Parse(feeSrc);
                                BigInteger needFee = needUSDFee * EIGHT_ZERO / sdsPrice;
                                sarDetail.Add("toPay", NEP5.getNumStrFromStr("Integer", needFee.ToString(), 8));

                            }
                            else {
                                sarDetail.Add("toPay", "0");
                            }

                        }
                        else {
                            sarDetail.Add("mortgageRate", "0");
                            sarDetail.Add("liqPrice", "0");
                            sarDetail.Add("status", 1);

                        }
                        ret.Add(sarDetail);

                    }
                    catch (Exception e) {
                        Console.WriteLine("error"+e.Message);
                    }
                }
            }

            return ret;
        }

        public JArray processSAR4BDetail(JArray arrs, string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string hashSAR4B, string hashORACLE)
        {
            JArray ret = new JArray();

            foreach (JObject ob in arrs)
            {
                string addr = (string)ob["addr"];
                if (!string.IsNullOrEmpty(addr))
                {
                    try
                    {
                        string script = invokeScript(new Hash160(hashSAR4B), "getSAR4B", "(addr)" + addr);
                        JObject jo = ct.invokeScript(neoCliJsonRPCUrl, script);
                        JObject sarDetail = new JObject();
                        //stack arrays
                        string type = (string)((JArray)jo["stack"])[0]["type"];
                        JArray sar = (JArray)((JArray)jo["stack"])[0]["value"];

                        //name
                        type = (string)sar[0]["type"];
                        string value = (string)sar[0]["value"];
                        string name = System.Text.Encoding.UTF8.GetString(ThinNeo.Helper.HexString2Bytes(value));
                        sarDetail.Add("name", name);
                        Console.WriteLine("name:" + name);

                        //symbol
                        type = (string)sar[1]["type"];
                        value = (string)sar[1]["value"];
                        string symbol = System.Text.Encoding.UTF8.GetString(ThinNeo.Helper.HexString2Bytes(value));
                        sarDetail.Add("symbol", symbol);
                        Console.WriteLine("symbol:" + symbol);

                        //decimals
                        type = (string)sar[2]["type"];
                        value = (string)sar[2]["value"];
                        string decimals = NEP5.getNumStrFromStr(type, value, 0);
                        sarDetail.Add("decimals", decimals);
                        Console.WriteLine("decimals:" + decimals);

                        //SAR地址
                        string ownerValue = (string)sar[3]["value"];
                        string owner = ThinNeo.Helper.GetAddressFromScriptHash(ThinNeo.Helper.HexString2Bytes(ownerValue));
                        sarDetail.Add("owner", owner);
                        Console.WriteLine("owner:" + owner);

                        //创建SAR的txid
                        string txidValue = (string)sar[4]["value"];
                        string txid = "0x" + ThinNeo.Helper.Bytes2HexString(ThinNeo.Helper.HexString2Bytes(txidValue).Reverse().ToArray());
                        sarDetail.Add("txid", txid);
                        Console.WriteLine("txid:" + txid);

                        //hasLocked
                        type = (string)sar[5]["type"];
                        value = (string)sar[5]["value"];
                        string lockedSrc = NEP5.getNumStrFromStr(type, value, 0);
                        string locked = NEP5.getNumStrFromStr(type, value, 8);
                        sarDetail.Add("locked", locked);
                        Console.WriteLine("locked:" + locked);

                        //hasDrawed
                        type = (string)sar[6]["type"];
                        value = (string)sar[6]["value"];
                        string hasDrawedSrc = NEP5.getNumStrFromStr(type, value, 0);
                        string hasDrawed = NEP5.getNumStrFromStr(type, value, 8);
                        sarDetail.Add("hasDrawed", hasDrawed);
                        Console.WriteLine("hasDrawed:" + hasDrawed);

                        //anchor
                        type = (string)sar[8]["type"];
                        value = (string)sar[8]["value"];
                        string anchor = System.Text.Encoding.UTF8.GetString(ThinNeo.Helper.HexString2Bytes(value));
                        sarDetail.Add("anchor", anchor);
                        Console.WriteLine("anchor:" + anchor);

                        //计算当前抵押率
                        BigInteger sdsPrice = getSDSPrice(neoCliJsonRPCUrl, hashORACLE);
                        BigInteger rate = getTypeA(neoCliJsonRPCUrl, hashORACLE, "liquidate_line_rate_b");
                        if (hasDrawedSrc != "0")
                        {
                            //乘以100的值
                            decimal mortgagerate = Decimal.Parse(sdsPrice.ToString()) * Decimal.Parse(lockedSrc) / (Decimal.Parse(hasDrawedSrc) * SIX_ZERO);
                            sarDetail.Add("mortgageRate", mortgagerate);
                            Console.WriteLine("mortgageRate");

                            //SAR状态 1:安全，2:不安全
                            int status = 1;
                            if (mortgagerate.CompareTo(Decimal.Parse(rate.ToString())) < 0)
                            {
                                status = 2;
                            }
                            sarDetail.Add("status", status);
                        }
                        else
                        {
                            sarDetail.Add("mortgageRate", "0");
                            sarDetail.Add("status", 1);

                        }
                        ret.Add(sarDetail);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("error" + e.Message);
                    }
                }
            }

            return ret;
        }

        public JArray processOrderMortgageRate(JArray arrs, string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string hashSAR4C, string hashORACLE, bool desc = false) {
            JArray lists = processSARDetail(arrs, mongodbConnStr, mongodbDatabase,neoCliJsonRPCUrl, hashSAR4C,hashORACLE);

            IOrderedEnumerable<JToken> query;

            if (desc)
            {
                query = from detail in lists.Children()
                        orderby (decimal)detail["mortgageRate"] descending  //从大到小排序
                        select detail;
            }
            else
            {
                query = from detail in lists.Children()
                        orderby (decimal)detail["mortgageRate"]            //从小到大排序
                        select detail;
            }

            JArray results = new JArray();
            foreach (JObject de in query) {
                results.Add(de);
            }
            return results;
        }

        public JArray getAllassetBalance(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string address,string hashSAR4B,string NEP55asset,
            string hashORACLE,string hashSDUSD,string hashSNEO,string hashSDS)
        {
            JArray ret = new JArray();

            //NEO,GAS
            try
            {
                JArray globalBalance = getGlobalBalance(mongodbConnStr, mongodbDatabase, address);
                
                foreach(JObject global in globalBalance) {
                    JObject gl = new JObject();
                    string assetType = (string)global["asset"];
                    decimal balance = (decimal)global["balance"];
                    if (assetType == NEO_ASSET)
                    {
                        BigInteger neoPrice = getTypeBPrice(neoCliJsonRPCUrl, hashORACLE, "neo_price");
                        decimal price =  decimal.Parse(neoPrice.ToString()) / EIGHT_ZERO;
                        decimal value = balance * price;
                        gl.Add("assetid", assetType);
                        gl.Add("balance",balance);
                        gl.Add("value",value);
                        gl.Add("price", price);
                        gl.Add("symbol","NEO");
                    }
                    else if (assetType == GAS_ASSET)
                    {
                        BigInteger gasPrice = getTypeBPrice(neoCliJsonRPCUrl, hashORACLE, "gas_price");
                        decimal price = decimal.Parse(gasPrice.ToString()) / EIGHT_ZERO;
                        decimal value = balance * price;
                        gl.Add("assetid", assetType);
                        gl.Add("balance", balance);
                        gl.Add("value", value);
                        gl.Add("price", price);
                        gl.Add("symbol", "GAS");
                    }
                    gl.Add("type", "global");
                    ret.Add(gl);
                }

            }catch (Exception e) {
                Console.WriteLine("globalBalance error:" + e.Message);
            }

            //NEP5
            try
            {
                JArray nep5Balance = getNEP5Balance(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, address, true);

                foreach (JObject nep5Ob in nep5Balance)
                {
                    JObject nep5 = new JObject();
                    string assetid = (string)nep5Ob["assetid"];
                    string symbol = (string)nep5Ob["symbol"];
                    string balance = (string)nep5Ob["balance"];
                    decimal price = 0;
                    decimal value = 0;

                    if (assetid == hashSNEO)
                    {
                        BigInteger neoPrice = getTypeBPrice(neoCliJsonRPCUrl, hashORACLE, "neo_price");
                        price = decimal.Parse(neoPrice.ToString()) / EIGHT_ZERO;
                    }
                    else if (assetid == hashSDS)
                    {
                        BigInteger sdsPrice = getTypeBPrice(neoCliJsonRPCUrl, hashORACLE, "sds_price");
                        price = decimal.Parse(sdsPrice.ToString()) / EIGHT_ZERO;
                    }
                    else if (assetid == hashSDUSD)
                    {
                        price = 1;
                    }
                    value = decimal.Parse(balance) * price;
                    nep5.Add("assetid", assetid);
                    nep5.Add("balance", decimal.Parse(balance));
                    nep5.Add("value", value);
                    nep5.Add("price", price);
                    nep5.Add("symbol", symbol);
                    nep5.Add("type", "nep5");
                    ret.Add(nep5);
                }
            }
            catch (Exception e) {
                Console.WriteLine("nep5Balance error:" + e.Message);
            }

            //NEP55
            try
            {
                JArray nep55Balance = getNEP55Balance(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl,address, NEP55asset, true);
                foreach (JObject nep55Ob in nep55Balance)
                {
                    JObject nep55 = new JObject();
                    string assetid = (string)nep55Ob["assetid"];
                    string symbol = (string)nep55Ob["symbol"];
                    string name = (string)nep55Ob["name"];
                    string balance = (string)nep55Ob["balance"];
                    decimal price = 0;
                    decimal value = 0;

                    BigInteger anchorPrice = getAnchor(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl,assetid,name,hashORACLE);

                    price = decimal.Parse(anchorPrice.ToString()) / EIGHT_ZERO;
                    value = decimal.Parse(balance) * price;
                    nep55.Add("assetid", assetid);
                    nep55.Add("balance", decimal.Parse(balance));
                    nep55.Add("value", value);
                    nep55.Add("price", price);
                    nep55.Add("symbol", name);
                    nep55.Add("type", "nep55");
                    ret.Add(nep55);
                }
            }
            catch (Exception e) {
                Console.WriteLine("error:" + e.Message);
            }

            return ret;
        }

        private  BigInteger getAnchor(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string assetid, string name,string hashORACLE)
        {
            string anchor = "anchor_type_usd";
            string findFliter = "{name:'" + name + "',asset:'"+assetid+"',status:1}";
            JArray sars = mh.GetData(mongodbConnStr, mongodbDatabase, "SAR4B", findFliter);
            if (sars.Count > 0) {
                JObject sar = (JObject)sars[0];
                anchor = (string)sar["anchor"];
            }

            return getTypeBPrice(neoCliJsonRPCUrl,hashORACLE,anchor);
        }

        public JArray getGlobalBalance(string mongodbConnStr,string mongodbDatabase, string address) {
            string findFliter = "{addr:'" + address + "',used:''}";
            JArray utxos = mh.GetData(mongodbConnStr, mongodbDatabase, "utxo", findFliter);
            Dictionary<string, decimal> balance = new Dictionary<string, decimal>();
            foreach (JObject j in utxos)
            {
                if (!balance.ContainsKey((string)j["asset"]))
                {
                    balance.Add((string)j["asset"], (decimal)j["value"]);
                }
                else
                {
                    balance[(string)j["asset"]] += (decimal)j["value"];
                }
            }
            JArray balanceJA = new JArray();
            foreach (KeyValuePair<string, decimal> kv in balance)
            {
                JObject j = new JObject();
                j.Add("asset", kv.Key);
                j.Add("balance", kv.Value);
                JObject asset = (JObject)mh.GetData(mongodbConnStr, mongodbDatabase, "asset", "{id:'" + kv.Key + "'}")[0];
                JArray name = (JArray)asset["name"];
                j.Add("name", name);
                balanceJA.Add(j);
            }

            return balanceJA;
        }


        public JArray getNEP5Balance(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string NEP5addr, bool isNeedBalance)
        {
            JArray result = new JArray();

            //按资产汇集收到的钱(仅资产ID)
            string findTransferTo = "{ to:'" + NEP5addr + "'}";
            JArray transferToJA = mh.GetData(mongodbConnStr, mongodbDatabase, "NEP5transfer", findTransferTo);
            List<NEP5.Transfer> tfts = new List<NEP5.Transfer>();
            foreach (JObject tfJ in transferToJA)
            {
                tfts.Add(new NEP5.Transfer(tfJ));
            }
            var queryTo = from tft in tfts
                          group tft by tft.asset into tftG
                          select new { assetid = tftG.Key };
            var assetAdds = queryTo.ToList();

            //如果需要余额，则通过cli RPC批量获取余额
            List<NEP5.AssetBalanceOfAddr> addrAssetBalances = new List<NEP5.AssetBalanceOfAddr>();
            if (isNeedBalance)
            {
                List<NEP5.AssetBalanceOfAddr> addrAssetBalancesTemp = new List<NEP5.AssetBalanceOfAddr>();
                foreach (var assetAdd in assetAdds)
                {
                    string findNep5Asset = "{assetid:'" + assetAdd.assetid + "'}";
                    JArray Nep5AssetJA = mh.GetData(mongodbConnStr, mongodbDatabase, "NEP5asset", findNep5Asset);
                    string Symbol = (string)Nep5AssetJA[0]["symbol"];

                    addrAssetBalancesTemp.Add(new NEP5.AssetBalanceOfAddr(assetAdd.assetid, Symbol, string.Empty));
                }

                List<string> nep5Hashs = new List<string>();
                JArray queryParams = new JArray();
                byte[] NEP5allAssetOfAddrHash = ThinNeo.Helper.GetPublicKeyHashFromAddress(NEP5addr);
                string NEP5allAssetOfAddrHashHex = ThinNeo.Helper.Bytes2HexString(NEP5allAssetOfAddrHash.Reverse().ToArray());
                foreach (var abt in addrAssetBalancesTemp)
                {
                    nep5Hashs.Add(abt.assetid);
                    queryParams.Add(JArray.Parse("['(str)balanceOf',['(hex)" + NEP5allAssetOfAddrHashHex + "']]"));
                }
                JArray NEP5allAssetBalanceJA = (JArray)ct.callContractForTestMulti(neoCliJsonRPCUrl, nep5Hashs, queryParams)["stack"];
                var a = Newtonsoft.Json.JsonConvert.SerializeObject(NEP5allAssetBalanceJA);
                foreach (var abt in addrAssetBalancesTemp)
                {
                    string allBalanceStr = (string)NEP5allAssetBalanceJA[addrAssetBalancesTemp.IndexOf(abt)]["value"];
                    string allBalanceType = (string)NEP5allAssetBalanceJA[addrAssetBalancesTemp.IndexOf(abt)]["type"];
                    //获取NEP5资产信息，获取精度
                    NEP5.Asset NEP5asset = new NEP5.Asset(mongodbConnStr, mongodbDatabase, abt.assetid);

                    abt.balance = NEP5.getNumStrFromStr(allBalanceType, allBalanceStr, NEP5asset.decimals);
                }

                //去除余额为0的资产
                foreach (var abt in addrAssetBalancesTemp)
                {
                    if (abt.balance != string.Empty && abt.balance != "0")
                    {
                        addrAssetBalances.Add(abt);
                    }
                }
            }

            if (!isNeedBalance)
            {
                result = JArray.FromObject(assetAdds);
            }
            else
            {
                result = JArray.FromObject(addrAssetBalances);
            }
            return result;
        }


        public JArray getNEP55Balance(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string NEP55addr,string NEP55asset,bool isNeedBalance)
        {
            JArray result = new JArray();

            string findTransferTo = "{ to:'" + NEP55addr + "',asset:'" + NEP55asset + "'}";
            JArray transferSARToJA = mh.GetData(mongodbConnStr, mongodbDatabase, "transferSAR", findTransferTo);
            List<NEP55.Transfer55> tfts55 = new List<NEP55.Transfer55>();
            foreach (JObject tfJ in transferSARToJA)
            {
                tfts55.Add(new NEP55.Transfer55(tfJ));
            }
            var queryTo55 = from tft in tfts55
                            group tft by tft.name into tftG
                            select new { name = tftG.Key };
            var nameAdds = queryTo55.ToList();

            //如果需要余额，则通过cli RPC批量获取余额
            List<NEP55.AssetBalanceOfAddr> AssetBalances = new List<NEP55.AssetBalanceOfAddr>();
            if (isNeedBalance)
            {
                List<NEP55.AssetBalanceOfAddr> addrAssetBalancesTemp = new List<NEP55.AssetBalanceOfAddr>();
                foreach (var nameAdd in nameAdds)
                {
                    addrAssetBalancesTemp.Add(new NEP55.AssetBalanceOfAddr(NEP55asset, nameAdd.name, nameAdd.name, string.Empty));
                }

                List<string> nep55Contract = new List<string>();
                JArray queryParams = new JArray();
                byte[] NEP5allAssetOfAddrHash = ThinNeo.Helper.GetPublicKeyHashFromAddress(NEP55addr);
                string NEP5allAssetOfAddrHashHex = ThinNeo.Helper.Bytes2HexString(NEP5allAssetOfAddrHash.Reverse().ToArray());
                foreach (var abt in addrAssetBalancesTemp)
                {
                    nep55Contract.Add(NEP55asset);
                    queryParams.Add(JArray.Parse("['(str)balanceOf',['(str)" + abt.name + "','(hex)" + NEP5allAssetOfAddrHashHex + "']]"));
                }
                JArray NEP55allAssetBalanceJA = (JArray)ct.callContractForTest(neoCliJsonRPCUrl, nep55Contract, queryParams)["stack"];
                var a = Newtonsoft.Json.JsonConvert.SerializeObject(NEP55allAssetBalanceJA);
                foreach (var abt in addrAssetBalancesTemp)
                {
                    string allBalanceStr = (string)NEP55allAssetBalanceJA[addrAssetBalancesTemp.IndexOf(abt)]["value"];
                    string allBalanceType = (string)NEP55allAssetBalanceJA[addrAssetBalancesTemp.IndexOf(abt)]["type"];

                    abt.balance = NEP5.getNumStrFromStr(allBalanceType, allBalanceStr, 8);
                }

                //去除余额为0的资产
                foreach (var abt in addrAssetBalancesTemp)
                {
                    if (abt.balance != string.Empty && abt.balance != "0")
                    {
                        AssetBalances.Add(abt);
                    }
                }
            }

            if (!isNeedBalance)
            {
                result = JArray.FromObject(nameAdds);
            }
            else
            {
                result = JArray.FromObject(AssetBalances);
            }

            return result;
        }

        public JArray getOracleConfig(string neoCliJsonRPCUrl,string hashORACLE,string type,string key)
        {
            JArray result = new JArray();

            //默认取所有价格 typeB
            if (type == "" || type == "typeB")
            {
                if (key == "")
                {
                    BigInteger neoPrice =  getTypeBPrice(neoCliJsonRPCUrl,hashORACLE,"neo_price");
                    JObject o = new JObject();
                    o.Add("key","neo_price");
                    o.Add("value",decimal.Parse(neoPrice.ToString())/EIGHT_ZERO);
                    result.Add(o);

                    JObject o2 = new JObject();
                    o2.Add("key", "sneo_price");
                    o2.Add("value", decimal.Parse(neoPrice.ToString()) / EIGHT_ZERO);
                    result.Add(o2);

                    BigInteger gasPrice = getTypeBPrice(neoCliJsonRPCUrl, hashORACLE, "gas_price");
                    JObject o3 = new JObject();
                    o3.Add("key", "gas_price");
                    o3.Add("value", decimal.Parse(gasPrice.ToString()) / EIGHT_ZERO);
                    result.Add(o3);

                    BigInteger sdsPrice = getTypeBPrice(neoCliJsonRPCUrl, hashORACLE, "sds_price");
                    JObject o4 = new JObject();
                    o4.Add("key", "sds_price");
                    o4.Add("value", decimal.Parse(sdsPrice.ToString()) / EIGHT_ZERO);
                    result.Add(o4);

                    BigInteger anchorUsdPrice = getTypeBPrice(neoCliJsonRPCUrl, hashORACLE, "anchor_type_usd");
                    JObject o5 = new JObject();
                    o5.Add("key", "anchor_type_usd");
                    o5.Add("value", decimal.Parse(anchorUsdPrice.ToString()) / EIGHT_ZERO);
                    result.Add(o5);

                    BigInteger anchorEurPrice = getTypeBPrice(neoCliJsonRPCUrl, hashORACLE, "anchor_type_eur");
                    JObject o6 = new JObject();
                    o6.Add("key", "anchor_type_eur");
                    o6.Add("value", decimal.Parse(anchorEurPrice.ToString()) / EIGHT_ZERO);
                    result.Add(o6);

                    BigInteger anchorJpyPrice = getTypeBPrice(neoCliJsonRPCUrl, hashORACLE, "anchor_type_jpy");
                    JObject o7 = new JObject();
                    o7.Add("key", "anchor_type_jpy");
                    o7.Add("value", decimal.Parse(anchorJpyPrice.ToString()) / EIGHT_ZERO);
                    result.Add(o7);

                    BigInteger anchorGbpPrice = getTypeBPrice(neoCliJsonRPCUrl, hashORACLE, "anchor_type_gbp");
                    JObject o8 = new JObject();
                    o8.Add("key", "anchor_type_gbp");
                    o8.Add("value", decimal.Parse(anchorGbpPrice.ToString()) / EIGHT_ZERO);
                    result.Add(o8);

                    BigInteger anchorGoldPrice = getTypeBPrice(neoCliJsonRPCUrl, hashORACLE, "anchor_type_gold");
                    JObject o9 = new JObject();
                    o9.Add("key", "anchor_type_gold");
                    o9.Add("value", decimal.Parse(anchorGoldPrice.ToString()) / EIGHT_ZERO);
                    result.Add(o9);
                }
                else {
                    BigInteger price = getTypeBPrice(neoCliJsonRPCUrl, hashORACLE, key);
                    JObject o = new JObject();
                    o.Add("key", key);
                    o.Add("value", decimal.Parse(price.ToString()) / EIGHT_ZERO);
                    result.Add(o);
                }
            }
            else if(type=="typeA"){
                if (key == "")
                {
                    BigInteger config = getTypeA(neoCliJsonRPCUrl, hashORACLE, "liquidate_line_rate_b");
                    JObject o = new JObject();
                    o.Add("key", "liquidate_line_rate_b");
                    o.Add("value", decimal.Parse(config.ToString()));
                    result.Add(o);

                    BigInteger config2 = getTypeA(neoCliJsonRPCUrl, hashORACLE, "liquidate_line_rate_c");
                    JObject o2 = new JObject();
                    o2.Add("key", "liquidate_line_rate_c");
                    o2.Add("value", decimal.Parse(config2.ToString()));
                    result.Add(o2);

                    BigInteger config3 = getTypeA(neoCliJsonRPCUrl, hashORACLE, "liquidate_dis_rate_c");
                    JObject o3 = new JObject();
                    o3.Add("key", "liquidate_dis_rate_c");
                    o3.Add("value", decimal.Parse(config3.ToString()));
                    result.Add(o3);

                    BigInteger config4 = getTypeA(neoCliJsonRPCUrl, hashORACLE, "liquidate_top_rate_c");
                    JObject o4 = new JObject();
                    o4.Add("key", "liquidate_top_rate_c");
                    o4.Add("value", decimal.Parse(config4.ToString()));
                    result.Add(o4);

                    BigInteger config5 = getTypeA(neoCliJsonRPCUrl, hashORACLE, "debt_top_c");
                    JObject o5 = new JObject();
                    o5.Add("key", "debt_top_c");
                    o5.Add("value", decimal.Parse(config5.ToString()));
                    result.Add(o5);

                    BigInteger config6 = getTypeA(neoCliJsonRPCUrl, hashORACLE, "liquidate_line_rateT_c");
                    JObject o6 = new JObject();
                    o6.Add("key", "liquidate_line_rateT_c");
                    o6.Add("value", decimal.Parse(config6.ToString()));
                    result.Add(o6);

                    BigInteger config7 = getTypeA(neoCliJsonRPCUrl, hashORACLE, "fee_rate_c");
                    JObject o7 = new JObject();
                    o7.Add("key", "fee_rate_c");
                    o7.Add("value", decimal.Parse(config7.ToString()));
                    result.Add(o7);

                    BigInteger config8 = getTypeA(neoCliJsonRPCUrl, hashORACLE, "issuing_fee_b");
                    JObject o8 = new JObject();
                    o8.Add("key", "issuing_fee_b");
                    o8.Add("value", decimal.Parse(config8.ToString()));
                    result.Add(o8);

                }
                else {
                    BigInteger config = getTypeA(neoCliJsonRPCUrl, hashORACLE, key);
                    JObject o = new JObject();
                    o.Add("key", key);
                    o.Add("value", decimal.Parse(config.ToString()));
                    result.Add(o);
                }
            }
            return result;
        }
    }
}
