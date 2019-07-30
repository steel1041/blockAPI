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
using MongoDB.Driver;
using MongoDB.Bson;
using static NEO_Block_API.NEP55;
using System.Globalization;
using MongoDB.Bson.IO;

namespace Block_API.Controllers
{
    class Business
    {
        mongoHelper mh = new mongoHelper();

        Contract ct = new Contract();

        NEO_Block_API.Controllers.Transaction tr = new NEO_Block_API.Controllers.Transaction();

        public string hashSDUSD_prinet = string.Empty;
        public string hashSAR4C_prinet = string.Empty;
        public string hashSNEO_prinet = string.Empty;
        public string hashORACLE_prinet = string.Empty;
        public string addrSAR4C_prinet = string.Empty;
        public string oldAddrSAR4C_prinet = string.Empty;
        public string hashSAR4B_prinet = string.Empty;
        public string hashSDS_prinet = string.Empty;
        public string hashTokenized_prinet = string.Empty;
        public string wif_prinet = string.Empty;

        public string hashSDUSD_testnet = string.Empty;
        public string hashSAR4C_testnet = string.Empty;
        public string hashSNEO_testnet = string.Empty;
        public string hashORACLE_testnet = string.Empty;
        public string addrSAR4C_testnet = string.Empty;
        public string oldAddrSAR4C_testnet = string.Empty;
        public string hashSAR4B_testnet = string.Empty;
        public string hashSDS_testnet = string.Empty;
        public string hashTokenized_testnet = string.Empty;
        public string wif_testnet = string.Empty;

        public string hashSDUSD_mainnet = string.Empty;
        public string hashSAR4C_mainnet = string.Empty;
        public string hashSNEO_mainnet = string.Empty;
        public string hashORACLE_mainnet = string.Empty;
        public string addrSAR4C_mainnet = string.Empty;
        public string oldAddrSAR4C_mainnet = string.Empty;
        public string hashSAR4B_mainnet = string.Empty;
        public string hashSDS_mainnet = string.Empty;
        public string hashTokenized_mainnet = string.Empty;
        public string wif_mainnet = string.Empty;

        private const int EIGHT_ZERO = 100000000;

        private const int SIX_ZERO = 1000000;
        private const int TWO_ZERO = 100;
        private const ulong SIXTEEN_ZERO = 10000000000000000;

        private const string GAS_ASSET = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";

        private const string NEO_ASSET = "0xc56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b";

        //锁仓账户类型
        private const string LOCK_01 = "lock_01";
        private const string LOCK_02 = "lock_02";
        private const string LOCK_03 = "lock_03";
        private const string LOCK_04 = "lock_04";

        private const decimal rate_01 = 1.0M;
        private const decimal rate_02 = 1.1M;
        private const decimal rate_03 = 1.2M;
        private const decimal rate_04 = 1.3M;

        public int ten_pow = 100000000;

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
            wif_prinet = config["wif_prinet"];

            hashSDUSD_testnet = config["hashSDUSD_testnet"];
            hashSAR4C_testnet = config["hashSAR4C_testnet"];
            hashSAR4B_testnet = config["hashSAR4B_testnet"];
            hashSNEO_testnet = config["hashSNEO_testnet"];
            hashORACLE_testnet = config["hashORACLE_testnet"];
            addrSAR4C_testnet = config["addrSAR4C_testnet"];
            oldAddrSAR4C_testnet = config["oldAddrSAR4C_testnet"];
            hashSDS_testnet = config["hashSDS_testnet"];
            hashTokenized_testnet = config["hashTokenized_testnet"];
            wif_testnet = config["wif_testnet"];

            hashSDUSD_mainnet = config["hashSDUSD_mainnet"];
            hashSAR4C_mainnet = config["hashSAR4C_mainnet"];
            hashSAR4B_mainnet = config["hashSAR4B_mainnet"];
            hashSNEO_mainnet = config["hashSNEO_mainnet"];
            hashORACLE_mainnet = config["hashORACLE_mainnet"];
            addrSAR4C_mainnet = config["addrSAR4C_mainnet"];
            oldAddrSAR4C_mainnet = config["oldAddrSAR4C_mainnet"];
            hashSDS_mainnet = config["hashSDS_mainnet"];
            hashTokenized_mainnet = config["hashTokenized_mainnet"];
            wif_mainnet = config["wif_mainnet"];
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
            string addrSAR4C, string oldAddrSAR4C)
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
            if (oldAddrSAR4C != "")
            {
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
                ob.Add("mortgageRate", decimal.Round((sneoLocked * sneo_price) / ((sdusdTotal - bondTotal) * 100000000), 6) * 100 + "%");
            }
            else
            {
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

            //

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

        private BigInteger getNEOPrice(string neoCliJsonRPCUrl, string hashORACLE)
        {
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

        private BigInteger getTypeBPrice(string neoCliJsonRPCUrl, string hashORACLE, string priceKey)
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
                Console.WriteLine("getTypeBPrice:" + e.Message);
            }
            return BigInteger.Parse(balanceBigint);
        }

        private BigInteger getTypeA(string neoCliJsonRPCUrl, string hashORACLE, string key)
        {
            string script = invokeScript(new Hash160(hashORACLE), "getTypeA", "(str)" + key);
            JObject jo = ct.invokeScript(neoCliJsonRPCUrl, script);
            string type = (string)((JArray)jo["stack"])[0]["type"];
            string value = (string)((JArray)jo["stack"])[0]["value"];
            string bigint = NEO_Block_API.NEP5.getNumStrFromStr(type, value, 0);
            return BigInteger.Parse(bigint);

        }

        private long getCurrBlock(string mongodbConnStr, string mongodbDatabase)
        {
            return (long)(mh.GetData(mongodbConnStr, mongodbDatabase, "system_counter", "{counter:'block'}")[0]["lastBlockindex"]) + 1;
        }

        public JArray processSARDetail(JArray arrs, string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string hashSAR4C, string hashORACLE)
        {
            JArray ret = new JArray();

            foreach (JObject ob in arrs)
            {
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
                        sarDetail.Add("owner", owner);
                        Console.WriteLine("owner:" + owner);
                        //创建SAR的txid
                        string txidValue = (string)sar[1]["value"];
                        string txid = "0x" + ThinNeo.Helper.Bytes2HexString(ThinNeo.Helper.HexString2Bytes(txidValue).Reverse().ToArray());
                        sarDetail.Add("txid", txid);
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
                        BigInteger neo_price = getNEOPrice(neoCliJsonRPCUrl, hashORACLE);
                        BigInteger rate = getTypeA(neoCliJsonRPCUrl, hashORACLE, "liquidate_line_rate_c");
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
                            else
                            {
                                sarDetail.Add("remainDrawed", "0");
                                sarDetail.Add("remainDrawedNEO", "0");
                            }
                        }
                        else
                        {
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
                            if (mortgagerate.CompareTo(Decimal.Parse(rate.ToString())) < 0)
                            {
                                status = 2;
                            }
                            sarDetail.Add("status", status);

                            decimal liqPrice = Decimal.Parse(hasDrawedSrc) * Decimal.Parse(rate.ToString()) / (Decimal.Parse(lockedSrc) * TWO_ZERO);
                            sarDetail.Add("liqPrice", liqPrice);
                            Console.WriteLine("liqPrice");

                            //计算待缴手续费
                            long blockHeight = getCurrBlock(mongodbConnStr, mongodbDatabase);
                            BigInteger fee_rate = getTypeA(neoCliJsonRPCUrl, hashORACLE, "fee_rate_c");
                            BigInteger sdsPrice = getSDSPrice(neoCliJsonRPCUrl, hashORACLE);
                            if (blockHeight > BigInteger.Parse(lastHeight))
                            {
                                BigInteger currFee = (blockHeight - BigInteger.Parse(lastHeight)) * BigInteger.Parse(hasDrawedSrc) * fee_rate / SIXTEEN_ZERO;

                                BigInteger needUSDFee = currFee + BigInteger.Parse(feeSrc);
                                BigInteger needFee = needUSDFee * EIGHT_ZERO / sdsPrice;
                                sarDetail.Add("toPay", NEP5.getNumStrFromStr("Integer", needFee.ToString(), 8));

                            }
                            else
                            {
                                sarDetail.Add("toPay", "0");
                            }

                        }
                        else
                        {
                            sarDetail.Add("mortgageRate", "0");
                            sarDetail.Add("liqPrice", "0");
                            sarDetail.Add("status", 1);
                            sarDetail.Add("toPay", "0");

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

                        //status SAR状态 1:安全，2:不安全,3:不可用
                        type = (string)sar[7]["type"];
                        value = (string)sar[7]["value"];
                        string statusSrc = NEP5.getNumStrFromStr(type, value, 0);
                        int status = int.Parse(statusSrc);

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
                            BigInteger anchorPrice = getAnchor(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, hashSAR4B, name, hashORACLE);
                            decimal mortgagerate = Decimal.Parse(sdsPrice.ToString()) * Decimal.Parse(lockedSrc) * 100 / (Decimal.Parse(hasDrawedSrc) * Decimal.Parse(anchorPrice.ToString()));
                            sarDetail.Add("mortgageRate", mortgagerate);
                            Console.WriteLine("mortgageRate");

                            //nep55价值
                            sarDetail.Add("nep55Value", decimal.Parse(hasDrawed) * Decimal.Parse(anchorPrice.ToString()) / EIGHT_ZERO);
                            //SAR状态 1:安全，2:不安全
                            if (mortgagerate.CompareTo(Decimal.Parse(rate.ToString())) < 0)
                            {
                                status = 2;
                            }
                        }
                        else
                        {
                            sarDetail.Add("nep55Value", 0);
                            sarDetail.Add("mortgageRate", "0");

                        }
                        sarDetail.Add("status", status);
                        if (lockedSrc != "0")
                        {
                            sarDetail.Add("sdsValue", decimal.Parse(locked) * Decimal.Parse(sdsPrice.ToString()) / EIGHT_ZERO);
                        }
                        else
                        {
                            sarDetail.Add("sdsValue", 0);
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

        public JArray processOrderMortgageRate(JArray arrs, string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string hashSAR4C, string hashORACLE, bool desc = false)
        {
            JArray lists = processSARDetail(arrs, mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, hashSAR4C, hashORACLE);

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
            foreach (JObject de in query)
            {
                results.Add(de);
            }
            return results;
        }

        public JArray getAllassetBalance(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string address, string hashSAR4B, string NEP55asset,
            string hashORACLE, string hashSDUSD, string hashSNEO, string hashSDS)
        {
            JArray ret = new JArray();

            //NEO,GAS
            try
            {
                JArray globalBalance = getGlobalBalance(mongodbConnStr, mongodbDatabase, address);

                foreach (JObject global in globalBalance)
                {
                    JObject gl = new JObject();
                    string assetType = (string)global["asset"];
                    decimal balance = (decimal)global["balance"];
                    if (assetType == NEO_ASSET)
                    {
                        BigInteger neoPrice = getTypeBPrice(neoCliJsonRPCUrl, hashORACLE, "sneo_price");
                        decimal price = decimal.Parse(neoPrice.ToString()) / EIGHT_ZERO;
                        decimal value = balance * price;
                        gl.Add("assetid", assetType);
                        gl.Add("balance", balance);
                        gl.Add("value", value);
                        gl.Add("price", price);
                        gl.Add("symbol", "NEO");
                        gl.Add("type", "global");
                        ret.Add(gl);
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
                        gl.Add("type", "global");
                        ret.Add(gl);
                    }
                }

            }
            catch (Exception e)
            {
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
                        BigInteger neoPrice = getTypeBPrice(neoCliJsonRPCUrl, hashORACLE, "sneo_price");
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
            catch (Exception e)
            {
                Console.WriteLine("nep5Balance error:" + e.Message);
            }

            //NEP55
            try
            {
                JArray nep55Balance = getNEP55Balance(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, address, NEP55asset, true);
                foreach (JObject nep55Ob in nep55Balance)
                {
                    JObject nep55 = new JObject();
                    string assetid = (string)nep55Ob["assetid"];
                    string symbol = (string)nep55Ob["symbol"];
                    string name = (string)nep55Ob["name"];
                    string balance = (string)nep55Ob["balance"];
                    decimal price = 0;
                    decimal value = 0;

                    JObject sar = getSAR4B(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, hashSAR4B, name, hashORACLE);
                    string owner = (string)sar["addr"];
                    price = decimal.Parse((string)sar["price"]) / EIGHT_ZERO;
                    value = decimal.Parse(balance) * price;
                    nep55.Add("assetid", assetid);
                    nep55.Add("balance", decimal.Parse(balance));
                    nep55.Add("value", value);
                    nep55.Add("price", price);
                    nep55.Add("symbol", name);
                    nep55.Add("type", "nep55");
                    nep55.Add("owner", owner);

                    int status = 1;
                    //直接判断合约里面有没有SAR
                    string script = invokeScript(new Hash160(hashSAR4B), "getSAR4B", "(addr)" + owner);
                    JObject jo = ct.invokeScript(neoCliJsonRPCUrl, script);

                    //stack arrays
                    string type = (string)((JArray)jo["stack"])[0]["type"];
                    if (type == "Array")
                    {
                        JArray sars = (JArray)((JArray)jo["stack"])[0]["value"];
                        if (sars != null)
                        {
                            type = (string)sars[7]["type"];
                            string valueStatus = (string)sars[7]["value"];
                            string statusSrc = NEP5.getNumStrFromStr(type, valueStatus, 0);
                            status = int.Parse(statusSrc);

                        }
                    }
                    nep55.Add("status", status);
                    ret.Add(nep55);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("error:" + e.Message);
            }

            return ret;
        }

        private BigInteger getAnchor(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string assetid, string name, string hashORACLE)
        {
            string anchor = "anchor_type_usd";
            string findFliter = "{name:'" + name + "',asset:'" + assetid + "',status:1}";
            JArray sars = mh.GetData(mongodbConnStr, mongodbDatabase, "SAR4B", findFliter);
            if (sars.Count > 0)
            {
                JObject sar = (JObject)sars[0];
                anchor = (string)sar["anchor"];
            }

            return getTypeBPrice(neoCliJsonRPCUrl, hashORACLE, anchor);
        }

        private JObject getSAR4B(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string hashSAR4B, string name, string hashORACLE)
        {
            JObject sar = null;
            string anchor = "anchor_type_usd";
            string findFliter = "{name:'" + name + "',asset:'" + hashSAR4B + "',status:1}";
            JArray sars = mh.GetData(mongodbConnStr, mongodbDatabase, "SAR4B", findFliter);
            if (sars.Count > 0)
            {
                sar = (JObject)sars[0];
                anchor = (string)sar["anchor"];

            }
            BigInteger price = getTypeBPrice(neoCliJsonRPCUrl, hashORACLE, anchor);
            sar.Add("price", price.ToString());
            return sar;
        }

        public JArray getGlobalBalance(string mongodbConnStr, string mongodbDatabase, string address)
        {
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


        public JArray getNEP55Balance(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string NEP55addr, string NEP55asset, bool isNeedBalance)
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

        public JArray getGoodsBalance(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string NEP55addr, string NEP55asset, bool isNeedBalance)
        {
            JArray result = new JArray();

            string findTransferTo = "{ to:'" + NEP55addr + "',asset:'" + NEP55asset + "'}";
            JArray transferSARToJA = mh.GetData(mongodbConnStr, mongodbDatabase, "goodsTransfer", findTransferTo);
            List<NEP55.GoodsTransfer> tfts55 = new List<NEP55.GoodsTransfer>();
            foreach (JObject tfJ in transferSARToJA)
            {
                tfts55.Add(new NEP55.GoodsTransfer(tfJ));
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



        public JArray getOracleConfig(string neoCliJsonRPCUrl, string hashORACLE, string type, string key)
        {
            JArray result = new JArray();

            //默认取所有价格 typeB
            if (type == "" || type == "typeB")
            {
                if (key == "")
                {
                    BigInteger neoPrice = getTypeBPrice(neoCliJsonRPCUrl, hashORACLE, "sneo_price");
                    JObject o = new JObject();
                    o.Add("key", "neo_price");
                    o.Add("value", decimal.Parse(neoPrice.ToString()) / EIGHT_ZERO);
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
                else
                {
                    BigInteger price = getTypeBPrice(neoCliJsonRPCUrl, hashORACLE, key);
                    JObject o = new JObject();
                    o.Add("key", key);
                    o.Add("value", decimal.Parse(price.ToString()) / EIGHT_ZERO);
                    result.Add(o);
                }
            }
            else if (type == "typeA")
            {
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
                else
                {
                    BigInteger config = getTypeA(neoCliJsonRPCUrl, hashORACLE, key);
                    JObject o = new JObject();
                    o.Add("key", key);
                    o.Add("value", decimal.Parse(config.ToString()));
                    result.Add(o);
                }
            }
            return result;
        }

        public JArray processSingleSAR4BDetail(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string addr, string hashSAR4B, string hashORACLE)
        {
            JArray result = new JArray();
            //直接判断合约里面有没有SAR
            string script = invokeScript(new Hash160(hashSAR4B), "getSAR4B", "(addr)" + addr);
            JObject jo = ct.invokeScript(neoCliJsonRPCUrl, script);

            JArray adds = new JArray();
            JObject sarDetail = new JObject();
            //stack arrays
            string type = (string)((JArray)jo["stack"])[0]["type"];
            if (type == "Array")
            {
                JArray sar = (JArray)((JArray)jo["stack"])[0]["value"];
                if (sar != null)
                {
                    string ownerValue = (string)sar[3]["value"];
                    string owner = ThinNeo.Helper.GetAddressFromScriptHash(ThinNeo.Helper.HexString2Bytes(ownerValue));
                    sarDetail.Add("addr", owner);
                    adds.Add(sarDetail);

                    result = processSAR4BDetail(adds, mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, hashSAR4B, hashORACLE);
                }
            }
            else if (type == "ByteArray")
            {

            }
            return result;
        }

        public JArray getUsableUtxoForNEO(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string hashSNEO)
        {
            string addr = ThinNeo.Helper.GetAddressFromScriptHash(new Hash160(hashSNEO));
            string findFliter = "{addr:'" + addr + "',asset:'0xc56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b',used:''}";

            //查询合约地址NEO
            JArray utxos = mh.GetData(mongodbConnStr, mongodbDatabase, "utxo", findFliter);

            JArray ret = new JArray();
            foreach (JObject utxo in utxos)
            {
                string txid = (string)utxo["txid"];
                int n = (int)utxo["n"];
                if (n > 0 && !checkRefund(mongodbConnStr,mongodbDatabase,txid,n))
                {
                    ret.Add(utxo);
                    continue;
                }
                else
                {
                    //只有区块里面无记录的才符合要求
                    string script = invokeScript(new Hash160(hashSNEO), "getRefundTarget", "(hex256)" + txid);
                    JObject jo = ct.invokeScript(neoCliJsonRPCUrl, script);
                    string value = (string)((JArray)jo["stack"])[0]["value"];
                    if ((value.Length == 0 || value == "") && !checkRefund(mongodbConnStr, mongodbDatabase, txid, n))
                    {
                        ret.Add(utxo);
                    }
                }
            }
            return ret;
        }

        private bool checkRefund(string mongodbConnStr,string mongodbDatabase,string txid,int n) {
            bool ret = false;
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collBson = database.GetCollection<BsonDocument>("refundFlag");
            var findLockBson = BsonDocument.Parse("{txid:'" + txid + "',n:" + n + "}");
            var queryBson = collBson.Find(findLockBson).ToList();

            if (queryBson.Count > 0) ret = true;

            return ret;
        }

        internal JArray transferSNEOdata(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, int num)
        {
            JObject ret = new JObject();
            ret.Add("result", true);

            string addr = "AWFSdehLUrvGP34G7WKgjcWGriBLGxvi42";
            JArray results = new JArray();

            string findTransferTo = "{ asset:'0xa2aeccd6a7a7808b9959866f5463e5dcb911a578'}";
            JArray transferToJA = mh.GetData(mongodbConnStr, mongodbDatabase, "NEP5transfer", findTransferTo);

            foreach (JObject tfJ in transferToJA)
            {
                string from = (string)tfJ["from"];
                string to = (string)tfJ["to"];
                decimal value = (decimal)tfJ["value"];
                //过滤掉合约操作转账，以及转入和转出
                if (from.Length > 0 && to.Length > 0 && from != addr && to != addr)
                {
                    if (num > 0 && value > decimal.Parse(num + ""))
                    {
                        results.Add(tfJ);
                    }
                }
            }
            Console.WriteLine("size:" + results.Count);
            return results;

        }

        public JArray processSARFilterByRate(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string hashSAR4C, string hashORACLE, decimal start, decimal end)
        {
            if (start.CompareTo(0) <= 0 || end.CompareTo(0) <= 0 || start.CompareTo(end) > 0) return new JArray();

            JArray result = mh.GetData(mongodbConnStr, mongodbDatabase, "SAR4C", "{status:1}");

            JArray sarList = processSARDetail(result, mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, hashSAR4C, hashORACLE);

            JArray retList = new JArray();

            foreach (JObject sar in sarList)
            {
                decimal rate = (decimal)sar["mortgageRate"];
                if (start.CompareTo(rate) <= 0 && end.CompareTo(rate) >= 0)
                {
                    retList.Add(sar);
                }
            }

            return retList;
        }

        internal JObject predictFeeTotal(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string hashSAR4C, string hashORACLE)
        {
            JObject ret = new JObject();
            //所有有效状态的SAR
            string findFliter = "{status:1,asset:'" + hashSAR4C + "'}";

            JArray arrs = mh.GetData(mongodbConnStr, mongodbDatabase, "SAR4C", findFliter);

            BigInteger feeTotal = new BigInteger(0);
            foreach (JObject ob in arrs)
            {
                string addr = (string)ob["addr"];
                if (!string.IsNullOrEmpty(addr))
                {
                    try
                    {
                        string script = invokeScript(new Hash160(hashSAR4C), "getSAR4C", "(addr)" + addr);
                        JObject jo = ct.invokeScript(neoCliJsonRPCUrl, script);
                        //JObject sarDetail = new JObject();
                        //stack arrays
                        string type = (string)((JArray)jo["stack"])[0]["type"];
                        JArray sar = (JArray)((JArray)jo["stack"])[0]["value"];

                        //SAR地址
                        string ownerValue = (string)sar[0]["value"];
                        string owner = ThinNeo.Helper.GetAddressFromScriptHash(ThinNeo.Helper.HexString2Bytes(ownerValue));
                        //sarDetail.Add("owner", owner);
                        Console.WriteLine("owner:" + owner);
                        //创建SAR的txid
                        string txidValue = (string)sar[1]["value"];
                        string txid = "0x" + ThinNeo.Helper.Bytes2HexString(ThinNeo.Helper.HexString2Bytes(txidValue).Reverse().ToArray());
                        //sarDetail.Add("txid", txid);
                        //Console.WriteLine("txid:" + txid);

                        //hasLocked
                        type = (string)sar[2]["type"];
                        string value = (string)sar[2]["value"];
                        string lockedSrc = NEP5.getNumStrFromStr(type, value, 0);
                        string locked = NEP5.getNumStrFromStr(type, value, 8);
                        //sarDetail.Add("locked", locked);
                        //Console.WriteLine("locked:" + locked);

                        //hasDrawed
                        type = (string)sar[3]["type"];
                        value = (string)sar[3]["value"];
                        string hasDrawedSrc = NEP5.getNumStrFromStr(type, value, 0);
                        string hasDrawed = NEP5.getNumStrFromStr(type, value, 8);
                        //sarDetail.Add("hasDrawed", hasDrawed);
                        //Console.WriteLine("hasDrawed:" + hasDrawed);

                        //资产类型
                        type = (string)sar[4]["type"];
                        value = (string)sar[4]["value"];
                        string assetType = System.Text.Encoding.UTF8.GetString(ThinNeo.Helper.HexString2Bytes(value));
                        //sarDetail.Add("assetType", assetType);
                        //Console.WriteLine("assetType:" + assetType);


                        //bondLocked
                        type = (string)sar[6]["type"];
                        value = (string)sar[6]["value"];
                        string bondLocked = NEP5.getNumStrFromStr(type, value, 8);
                        //sarDetail.Add("bondLocked", bondLocked);
                        //Console.WriteLine("bondLocked:" + bondLocked);

                        //bondDrawed
                        //type = (string)sar[7]["type"];
                        //value = (string)sar[7]["value"];
                        //string bondDrawed = NEP5.getNumStrFromStr(type, value, 8);
                        //sarDetail.Add("bondDrawed", bondDrawed);
                        //Console.WriteLine("bondDrawed:" + bondDrawed);

                        //lastHeight
                        type = (string)sar[8]["type"];
                        value = (string)sar[8]["value"];
                        string lastHeight = NEP5.getNumStrFromStr(type, value, 0);
                        //sarDetail.Add("lastHeight", lastHeight);
                        //Console.WriteLine("lastHeight:" + lastHeight);

                        //fee
                        type = (string)sar[9]["type"];
                        value = (string)sar[9]["value"];
                        string feeSrc = NEP5.getNumStrFromStr(type, value, 0);
                        string fee = NEP5.getNumStrFromStr(type, value, 8);
                        //sarDetail.Add("fee", fee);


                        //计算当前抵押率
                        if (hasDrawedSrc != "0")
                        {

                            //计算待缴手续费
                            long blockHeight = getCurrBlock(mongodbConnStr, mongodbDatabase);
                            BigInteger fee_rate = getTypeA(neoCliJsonRPCUrl, hashORACLE, "fee_rate_c");
                            BigInteger sdsPrice = getSDSPrice(neoCliJsonRPCUrl, hashORACLE);
                            if (blockHeight > BigInteger.Parse(lastHeight))
                            {
                                BigInteger currFee = (blockHeight - BigInteger.Parse(lastHeight)) * BigInteger.Parse(hasDrawedSrc) * fee_rate / SIXTEEN_ZERO;

                                BigInteger needUSDFee = currFee + BigInteger.Parse(feeSrc);
                                BigInteger needFee = needUSDFee * EIGHT_ZERO / sdsPrice;
                                feeTotal = feeTotal + needFee;
                            }
                            else
                            {
                                //sarDetail.Add("toPay", "0");
                            }

                        }
                        else
                        {

                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("error" + e.Message);
                    }
                }
            }
            ret.Add("feeTotal", NEP5.getNumStrFromStr("Integer", feeTotal + "", 8));
            return ret;
        }

        internal JArray getTxDetail(string mongodbConnStr, string mongodbDatabase, string txid)
        {
            JArray result = new JArray();
            string findFliter = "{txid:'" + txid + "'}";
            JArray arrs = mh.GetData(mongodbConnStr, mongodbDatabase, "tx", findFliter);
            if (arrs.Count > 0)
            {
                JObject tx = (JObject)arrs[0];
                //InvocationTransaction、
                string type = (string)tx["type"];
                //根据交易类型来判断业务
                if (type == "InvocationTransaction")
                {
                    //是否有NEP5转账
                    findFliter = "{txid:'" + txid + "'}";
                    JArray nep5tr = mh.GetData(mongodbConnStr, mongodbDatabase, "NEP5transfer", findFliter);
                    //JArray nep5tr2 = new JArray();
                    foreach (JObject nep5 in nep5tr)
                    {
                        findFliter = "{assetid:'" + (string)nep5["asset"] + "'}";
                        JArray assets = mh.GetData(mongodbConnStr, mongodbDatabase, "NEP5asset", findFliter);
                        nep5.Add("name", (string)assets[0]["symbol"]);

                        //nep5tr2.Add(nep5);
                    }
                    tx.Add("nep5", nep5tr);
                }

                //处理输入的地址显示，输出不用管
                JArray vins = (JArray)tx["vin"];
                //JArray vins2 = new JArray();
                foreach (JObject vin in vins)
                {
                    string vtxid = (string)vin["txid"];
                    uint vout = (uint)vin["vout"];
                    JObject voutOb = getVinAddr(mongodbConnStr, mongodbDatabase, vtxid, vout);
                    vin.Add("address", (string)voutOb["address"]);
                    vin.Add("value", (string)voutOb["value"]);
                    vin.Add("asset", (string)voutOb["asset"]);
                    //vins2.Add(vin);
                }
                //tx.Add("vin2", vins2);

                //时间戳
                findFliter = "{index:" + (uint)tx["blockindex"] + "}";
                var time = (Int32)mh.GetData(mongodbConnStr, mongodbDatabase, "block", findFliter)[0]["time"];
                tx.Add("time", time);

                result.Add(tx);
            }
            return result;
        }

        private JObject getVinAddr(string mongodbConnStr, string mongodbDatabase, string vtxid, uint voutN)
        {
            JObject voutOb = new JObject();
            string findFliter = "{txid:'" + vtxid + "'}";
            JArray arrs = mh.GetData(mongodbConnStr, mongodbDatabase, "tx", findFliter);
            JObject ob = (JObject)arrs[0];
            JArray vouts = (JArray)ob["vout"];
            foreach (JObject vout in vouts)
            {
                uint n = (uint)vout["n"];
                string address = (string)vout["address"];
                if (n == voutN)
                {
                    voutOb = vout;
                    break;
                }
            }
            return voutOb;
        }

        internal JArray getLockListByAdd(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string asset, string addr)
        {
            JArray ret = new JArray();
            //依次查询4个账户
            JObject locko1 = getLockDetail(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, asset, addr, LOCK_01);
            JObject locko2 = getLockDetail(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, asset, addr, LOCK_02);
            JObject locko3 = getLockDetail(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, asset, addr, LOCK_03);
            JObject locko4 = getLockDetail(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, asset, addr, LOCK_04);
            if (locko1 != null)
            {
                ret.Add(locko1);
            }
            if (locko2 != null)
            {
                ret.Add(locko2);
            }
            if (locko3 != null)
            {
                ret.Add(locko3);
            }
            if (locko4 != null)
            {
                ret.Add(locko4);
            }
            return ret;
        }

        private JObject getLockDetail(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string asset, string addr,string lockType)
        {
            JObject detail = new JObject();

            try
            {
                string script = invokeScript(new Hash160(asset), "getLockInfo", "(addr)" + addr, "(str)" + lockType);
                JObject jo = ct.invokeScript(neoCliJsonRPCUrl, script);

                //stack arrays
                string type = (string)((JArray)jo["stack"])[0]["type"];
                JArray sar = (JArray)((JArray)jo["stack"])[0]["value"];

                //owner
                string value = (string)sar[0]["value"];
                string owner = ThinNeo.Helper.GetAddressFromScriptHash(ThinNeo.Helper.HexString2Bytes(value));
                detail.Add("owner", owner);
                Console.WriteLine("owner:" + owner);

                //创建lock的txid
                string txidValue = (string)sar[1]["value"];
                string txid = "0x" + ThinNeo.Helper.Bytes2HexString(ThinNeo.Helper.HexString2Bytes(txidValue).Reverse().ToArray());
                detail.Add("txid", txid);
                Console.WriteLine("txid:" + txid);

                //locked
                type = (string)sar[2]["type"];
                value = (string)sar[2]["value"];
                string lockedSrc = NEP5.getNumStrFromStr(type, value, 0);
                string locked = NEP5.getNumStrFromStr(type, value, 8);
                detail.Add("locked", locked);
                Console.WriteLine("locked:" + locked);

                //lockType
                type = (string)sar[3]["type"];
                value = (string)sar[3]["value"];
                string locktype = System.Text.Encoding.UTF8.GetString(ThinNeo.Helper.HexString2Bytes(value));
                detail.Add("lockType", locktype);
                Console.WriteLine("lockType:" + locktype);

                //lockTime
                type = (string)sar[4]["type"];
                value = (string)sar[4]["value"];
                string lockSrc = NEP5.getNumStrFromStr(type, value, 0);
                int lockTime = int.Parse(lockSrc);
                detail.Add("lockTime", lockTime);

                //lockHeight
                type = (string)sar[5]["type"];
                value = (string)sar[5]["value"];
                string lockHeightSrc = NEP5.getNumStrFromStr(type, value, 0);
                int lockHeight = int.Parse(lockHeightSrc);
                detail.Add("lockHeight", lockHeight);


            }
            catch(Exception e)
            {
                detail = null;
                Console.WriteLine("error:"+ e.Message);
            }

            return detail;

        }

        private decimal getLockedByType(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string asset, string addr, string lockType)
        {
            decimal ret = new decimal(0.0);

            try
            {
                string script = invokeScript(new Hash160(asset), "getLockInfo", "(addr)" + addr, "(str)" + lockType);
                JObject jo = ct.invokeScript(neoCliJsonRPCUrl, script);

                //stack arrays
                string type = (string)((JArray)jo["stack"])[0]["type"];
                JArray sar = (JArray)((JArray)jo["stack"])[0]["value"];


                //locked
                type = (string)sar[2]["type"];
                string value = (string)sar[2]["value"];
                string lockedSrc = NEP5.getNumStrFromStr(type, value, 0);
                string locked = NEP5.getNumStrFromStr(type, value, 8);
                ret = decimal.Parse(locked);
                Console.WriteLine("locked:" + locked);

            }
            catch (Exception e)
            {
                Console.WriteLine("getLockedByType error:" + e.Message);
            }
            return ret;

        }

        internal JObject getLockTypeByAdd(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string asset, string addr)
        {
            JObject ret = new JObject();
            string script = invokeScript(new Hash160(asset), "getLockAdd", "(addr)" + addr, "(str)eth");
            JObject jo = ct.invokeScript(neoCliJsonRPCUrl, script);

            JObject ob = (JObject)((JArray)jo["stack"])[0];
            //stack arrays
            string type = (string)(ob["type"]);
            string value = (string)(ob["value"]);
            if (value!=null)
            {
                string lockAddr = System.Text.Encoding.UTF8.GetString(ThinNeo.Helper.HexString2Bytes(value));
                ret.Add("eth", lockAddr);
            }
            return ret;
        }

        internal JObject statcDataProcess(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string hashSDUSD, string hashSNEO, 
            string hashSAR4C, string hashORACLE,string addrSAR4C, string oldAddrSAR4C)
        {
            DateTime now = DateTime.Now;
            string date = DateTime.Now.ToString("d");
            //2016/5/9 一天记录只能存一条

            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var coll = database.GetCollection<NEP55.Staticdata>("Staticdata");
            BsonDocument queryBson = BsonDocument.Parse("{dateKey:'" + date + "'}");
            List<NEP55.Staticdata> queryBsonList = coll.Find(queryBson).ToList();

            if (queryBsonList.Count > 0) return new JObject();

            JObject ret = getStaticReport(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, hashSDUSD.formatHexStr(), hashSNEO.formatHexStr(), hashSAR4C.formatHexStr(), hashORACLE.formatHexStr(), addrSAR4C, oldAddrSAR4C);

            JObject feeRet = predictFeeTotal(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, hashSAR4C, hashORACLE);

            //sar的总数
            string findFilter = "{status:1}";
            long sarCount = mh.GetDataCount(mongodbConnStr, mongodbDatabase, "SAR4C", findFilter);

            //sdusd的交易总数
            string findFliter = "{asset:'" + hashSDUSD + "'}";
            long sdusdCount = mh.GetDataCount(mongodbConnStr, mongodbDatabase, "NEP5transfer", findFliter);

            //sdusd地址数
            JArray arrs = mh.GetData(mongodbConnStr,mongodbDatabase, "NEP5transfer",findFliter);
            List<string> addrs = new List<string>();
            long n = 0;
            foreach (JObject ob in arrs) {
                string to = (string)ob["to"];
                if (to != "" && to != null && !addrs.Contains(to)) {
                    addrs.Add(to);
                    string balancestr = getNep5Balance(neoCliJsonRPCUrl,hashSDUSD,to);
                    if (balancestr != "0") {
                        n++;
                    }
                }
            }

            BigInteger neoPrice = getNEOPrice(neoCliJsonRPCUrl,hashORACLE);
            BigInteger sdsPrice = getSDSPrice(neoCliJsonRPCUrl,hashORACLE);
            string neoPricestr = NEP5.changeDecimals(neoPrice,8);
            string sdsPricestr = NEP5.changeDecimals(sdsPrice,8);

            //string dates = "";
            //string datee = "";
            //统计查询各操作记录 db.operatedSAR4C.find({"blocktime":{"$gte":ISODate("2019-05-12"),"$lte":ISODate("2019-05-13")}})
            //string findFilter = "{blocktime:{'$gte:':ISODate("+dates+ "),$lte:ISODate(" +datee +")}}";
            //JArray arrs =  mh.GetData(mongodbConnStr,mongodbDatabase, "operatedSAR4C", findFilter);
            //foreach (JObject ob in arrs) {
            //    int type = (int)ob["type"];
            //    decimal value = (decimal)ob["value"];
            //}
            string mortgageRate = (string)ret["mortgageRate"];
            mortgageRate = mortgageRate.Substring(0,mortgageRate.Length-1);
            NEP55.Staticdata data = new NEP55.Staticdata((decimal)ret["sdusdTotal"], (decimal)ret["lockedTotal"], (decimal)ret["sdsFeeTotal"],decimal.Parse(mortgageRate),
                (decimal)feeRet["feeTotal"],decimal.Parse(neoPricestr),decimal.Parse(sdsPricestr), sarCount,n,sdusdCount,date,DateTime.Now);

            var collectionPro = database.GetCollection<NEP55.Staticdata>("Staticdata");
            collectionPro.InsertOne(data);
            return ret;
        }

        internal JArray getAccApproveAppList(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string assetID, string addr, string username)
        {
            JArray ret = new JArray();
            string findFliter = "{'asset':'" + assetID + "','from':'" + addr + "','nameSrc':'"+username+"'}";
            JArray arrays = mh.GetData(mongodbConnStr, mongodbDatabase, "accApproveOperator", findFliter);
            Dictionary<string, string> names = new Dictionary<string, string>();
            foreach (JObject ob in arrays)
            {
                string nameDest = (string)ob["nameDest"];
                string addDest = (string)ob["to"];
                if (!names.ContainsKey(nameDest))
                {
                    names.Add(nameDest, "1");
                    JObject balance = new JObject();
                    balance.Add("name", nameDest);
                    balance.Add("addr", addDest);
                    //增加授权明细金额
                    balance.Add("approves",getApproveDetailList(mongodbConnStr,mongodbDatabase,neoCliJsonRPCUrl,assetID,addr,username,addDest,nameDest));
                    ret.Add(balance);
                }
            }
            return ret;
        }

        internal JObject setRefundFlagByTxid(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrlLocal,JArray txids,string addr)
        {
            JObject ret = new JObject();
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collBson = database.GetCollection<BsonDocument>("refundFlag");

            foreach (JObject ob in txids) {
                string txid = (string)ob["txid"];
                int n = (int)ob["n"];
                var findLockBson = BsonDocument.Parse("{txid:'" + txid + "',n:" + n + "}");
                var queryBson = collBson.Find(findLockBson).ToList();

                if (queryBson.Count == 0)//不重复才存
                {
                    NEP55.RefundFlag tf = new NEP55.RefundFlag(txid, n, addr, DateTime.Now);
                    var collOperated = database.GetCollection<NEP55.RefundFlag>("refundFlag");
                    collOperated.InsertOne(tf);
                }
            }
            ret.Add("result",true);
            return ret;
        }


        internal JObject setRefundErrorFlagByTxid(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrlLocal, JArray txids, string addr,string msg)
        {
            JObject ret = new JObject();
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collBson = database.GetCollection<BsonDocument>("refundErrorFlag");

            foreach (JObject ob in txids)
            {
                string txid = (string)ob["txid"];
                int n = (int)ob["n"];
                var findLockBson = BsonDocument.Parse("{txid:'" + txid + "',n:" + n + "}");
                var queryBson = collBson.Find(findLockBson).ToList();

                if (queryBson.Count == 0)//不重复才存
                {
                    NEP55.RefundErrorFlag tf = new NEP55.RefundErrorFlag(txid, n, addr, msg,DateTime.Now);
                    var collOperated = database.GetCollection<NEP55.RefundErrorFlag>("refundErrorFlag");
                    collOperated.InsertOne(tf);
                }
            }
            ret.Add("result", true);
            return ret;
        }

        internal JArray getSignForAdd(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string assetID, string addr,string wif)
        {
            JObject ret = new JObject();
            //db.goodsTransfer.find({"blocktime":{"$gte":ISODate("2019-06-26T00:00:00.000Z"),"$lte":ISODate("2019-06-26T23:59:59.000Z")}});
            //mongodb中存储的时间是标准时间UTC +0:00  而咱们中国的失去是+8.00 
            //当前时间减去8h
            DateTime dt = DateTime.Now;
            dt.AddHours(-8);

            string str= dt.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
            Console.WriteLine(str);
            string findFliter = "{asset:'" + assetID + "',addr:'" + addr +"',now:{'$gte':ISODate('"+ str+ "T00:00:00.000Z'),'$lte':ISODate('" + str+ "T23:59:59.000Z')}}";
            JArray arrays = mh.GetData(mongodbConnStr, mongodbDatabase, "goodsSign", findFliter);

            bool result = false;
            if (arrays.Count == 0)
            {
                //转账发送物品
                byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(wif);
                byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
                string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

                string username = getAccountName(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, assetID, address);
                string othername = getAccountName(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, assetID, addr);

                string rawdata = tr.api_SendbatchTransaction(prikey, new Hash160(assetID), "transfer",
                "(addr)" + address,
                "(str)" + username,
                "(str)" + othername,
                "(str)nep55",
                "(str)SD-NEXT",
                "(int)1000000000");

                JObject result2 = tr.sendrawtransaction(neoCliJsonRPCUrl, rawdata);
                if ((bool)result2["sendrawtransactionresult"])
                {
                    string txid = (string)result2["txid"];
                    //存入签收数据
                    var client = new MongoClient(mongodbConnStr);
                    var database = client.GetDatabase(mongodbDatabase);
                    NEP55.GoodsSign sign = new NEP55.GoodsSign(str, addr, assetID, txid,"nep55", "SD-NEXT",10,DateTime.Now);
                    var collectionPro = database.GetCollection<NEP55.GoodsSign>("goodsSign");
                    collectionPro.InsertOne(sign);
                    result = true;
                }
            }
            ret.Add("result",result);
            return mh.GetData(mongodbConnStr, mongodbDatabase, "goodsSign", findFliter);
        }

        private JObject sendTransfer(string mongodbConnStr,string mongodbDatabase, string assetID,string neoCliJsonRPCUrl,string addr,int mount,string wif) {
            JObject ret = new JObject();

            //转账发送物品
            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(wif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

            string username = getAccountName(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, assetID, addr);
            string othername = getAccountName(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, assetID, address);
            if (username == "" || othername == "")
            {
                ret.Add("sendrawtransactionresult", false);
                return ret;
            }
            //判断是否有余额以及授权
            string rawdata = tr.api_SendbatchTransaction(prikey, new Hash160(assetID), "userTransfer",
            "(addr)" + address,
            "(str)" + username,
            "(str)" + othername,
            "(str)nep55",
            "(str)SD-NEXT",
            "(int)" + mount * ten_pow);

            return tr.sendrawtransaction(neoCliJsonRPCUrl, rawdata);

        }

        internal JObject setGuessStatus(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrlLocal, string addr, string key)
        {
            //根据猜测单双规则进行发放奖励
            JObject ret = new JObject();

            string findFliter = "{addr:'" + addr + "',key:'" + key + "'}";
            BsonDocument queryBson = BsonDocument.Parse(findFliter);

            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collectionPro = database.GetCollection<NEP55.GoodsGuess>("goodsGuess");
            List<NEP55.GoodsGuess> queryBsonList = collectionPro.Find(queryBson).ToList();

            bool result = false;
            if (queryBsonList.Count >= 1)
            {
                NEP55.GoodsGuess guess = queryBsonList[0];
                guess.status = 1;
                collectionPro.ReplaceOne(queryBson, guess);
                result = true;
            }
            ret.Add("result", result);
            return ret;
        }

        internal JObject addAuctionGoods(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrlLocal, string assetID, string txid)
        {
            JObject ret = new JObject();
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);

            string findFliter = "{txid:'" + txid + "'}";
            JArray result3 = mh.GetData(mongodbConnStr, mongodbDatabase, "goodsInit",findFliter);
            if (result3.Count <= 0)
            {
                ret.Add("result", false);
                return ret;
            }

            DateTime dt = DateTime.Now;
            string key = dt.ToString(); //拍卖物的Key

            NEP55.AuctionGoods gu = new NEP55.AuctionGoods(assetID,key,txid,DateTime.Now);
            var collectionPro = database.GetCollection<NEP55.AuctionGoods>("auctionGoods");
            collectionPro.InsertOne(gu);

            ret.Add("result", true);
            return ret;
        }

        internal JArray getAuctionByStatus(string mongodbConnStr, string mongodbDatabase, JArray result)
        {
            foreach (JObject ob in result) {
                //查询拍卖物最高标价
                string txid = (string)ob["txid"];
                string asset = (string)ob["asset"];
                string key = (string)ob["key"];
                string findFliter = "{txid:'"+txid+"',key:'"+key+"',result:1}";
                string sortStr = "{now:-1}";
                JArray result2 = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "auctionRecord", sortStr, 10,1,findFliter);
                double highPrice = 0.0;
                if (result2.Count > 0)
                {
                    highPrice = (double)result2[0]["mount"];
                }

                //查询goods具体信息
                findFliter = "{txid:'" + txid + "'}";
                sortStr = "{'blockindex':-1}";
                JArray result3 = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "goodsInit", sortStr, 10,1, findFliter);
                if (result3.Count > 0)
                {
                    JObject goods = (JObject)result3[0];
                    ob.Add("symbol",(string)goods["symbol"]);
                    ob.Add("desc", (string)goods["desc"]);
                    ob.Add("name", (string)goods["name"]);
                    ob.Add("from", (string)goods["from"]);
                }

                ob.Add("highPrice", highPrice);
            }

            return result;
        }

        internal JObject processAuctionResult(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string assetID)
        {
            JObject ret = new JObject();

            //查出所有未有结果的auctionRecord数据
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);

            var collBson = database.GetCollection<NEP55.AuctionRecord>("auctionRecord");
            var findBsonBson = BsonDocument.Parse("{result:0}");
            List<NEP55.AuctionRecord> queryBson = collBson.Find(findBsonBson).ToList();

            if (queryBson.Count > 0)
            {
                foreach (NEP55.AuctionRecord oper in queryBson)
                {
                    string key = oper.key;
                    string addr = oper.addr;
                    string txid = oper.txid;
                   
                    int mount = oper.mount;
                    //根据txid判断是否收到转账，如果没有则表示扣款失败
                    var coll = database.GetCollection<BsonDocument>("accUserTransfer");
                    BsonDocument bson = BsonDocument.Parse("{txid:'" + txid + "'}");
                    var transfers = coll.Find(bson).ToList();
                    if (transfers.Count <= 0)
                        continue;

                    string findFliter = "{addr:'" + addr + "',txid:'" + txid + "'}";
                    oper.result = 1;
                    collBson.ReplaceOne(findFliter, oper);
                }
            }
            ret.Add("result","true");
            return ret;
        }

        internal JObject setAuctionResult(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string assetID, string addr, string auctionKey, int mount, string wif)
        {
            JObject ret = new JObject();

            bool result = false;

            //判断拍卖物品状态及时间
            JObject result2 = sendTransfer(mongodbConnStr, mongodbDatabase, assetID, neoCliJsonRPCUrl, addr, mount, wif);
            if ((bool)result2["sendrawtransactionresult"])
            {
                string txid = (string)result2["txid"];
                //存入拍卖数据
                var client = new MongoClient(mongodbConnStr);
                var database = client.GetDatabase(mongodbDatabase);

                string script = invokeScript(new Hash160(assetID), "getUserName", "(addr)" + addr);
                JObject jo = ct.invokeScript(neoCliJsonRPCUrl, script);
                string account = System.Text.Encoding.UTF8.GetString(ThinNeo.Helper.HexString2Bytes((string)((JArray)jo["stack"])[0]["value"]));

                NEP55.AuctionRecord gu = new NEP55.AuctionRecord(assetID,auctionKey,addr,account,txid,mount, DateTime.Now);
                var collectionPro = database.GetCollection<NEP55.AuctionRecord>("auctionRecord");
                collectionPro.InsertOne(gu);
                result = true;
            }
            ret.Add("result", result);
            return ret;
        }

        internal JObject ProcessGoodsGuessInfo(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl,string token,string wif)
        {
            JObject ret = new JObject();
            //查出所有未有猜测结果的goodsGuess数据
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);

            var collBson = database.GetCollection<NEP55.GoodsGuess>("goodsGuess");
            var findBsonBson = BsonDocument.Parse("{result:0}");
            List<NEP55.GoodsGuess> queryBson = collBson.Find(findBsonBson).ToList();

            if (queryBson.Count > 0)
            {
                foreach (NEP55.GoodsGuess oper in queryBson)
                {
                    string key = oper.key;
                    string addr = oper.addr;
                    string txid = oper.txid;
                    int guess = oper.guess; //猜测结果
                    long blockindex = oper.blockindex;//猜测区块高度
                    int mount = oper.mount;
                    //根据txid判断是否收到转账，如果没有则表示扣款失败
                    var coll = database.GetCollection<BsonDocument>("accUserTransfer");
                    BsonDocument bson = BsonDocument.Parse("{txid:'" + txid + "'}");
                    var transfers = coll.Find(bson).ToList();
                    if (transfers.Count <= 0)
                        continue;

                    //根据区块高度查询hash值，然后判断结果
                    var collutxo = database.GetCollection<BsonDocument>("block");
                    BsonDocument queryutxoBson = BsonDocument.Parse("{index:" + blockindex + "}");
                    var query = collutxo.Find(queryutxoBson).ToList();
                    if (query.Count > 0)
                    {
                        var jsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Strict };
                        JObject block = JObject.Parse(query[0].ToJson(jsonWriterSettings));

                        string hash = (string)block["hash"];
                        oper.result = checkGuess(guess, hash);
                        oper.hash = hash;
                        oper.status = 1;
                        string findFliter = "{addr:'" + addr + "',txid:'" + txid + "'}";

                        if (oper.result == 1) {
                            //猜中的则进行双倍
                            //转账发送物品
                            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(wif);
                            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
                            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

                            string username = getAccountName(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, token, address);
                            string othername = getAccountName(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, token, addr);

                            string rawdata = tr.api_SendbatchTransaction(prikey, new Hash160(token), "transfer",
                            "(addr)" + address,
                            "(str)" + username,
                            "(str)" + othername,
                            "(str)nep55",
                            "(str)SD-NEXT",
                            "(int)" + 2 * mount * ten_pow);

                            ret = tr.sendrawtransaction(neoCliJsonRPCUrl, rawdata);
                            if ((bool)ret["sendrawtransactionresult"])
                            {
                                //记录下赢的转账txid
                                txid = (string)ret["txid"];
                                oper.wintxid = txid;
                            }
                        }
                        collBson.ReplaceOne(findFliter, oper);

                    }

                }
            }
            return ret;
        }

        private static int checkGuess(int guess, string hash)
        {
            //1:正确,2:不正确
            int ret = 0;
            char[] smalls = new char[] { '1', '3', '5', '7', '9', 'A', 'C', 'E', 'G', 'I', 'K', 'M', 'O', 'Q', 'S', 'U', 'W', 'Y' };
            char[] bigs = new char[] { '2', '4', '6', '8', '0', 'B', 'D', 'F', 'H', 'J', 'L', 'N', 'P', 'R', 'T', 'V', 'X', 'Z' };
            hash = hash.ToUpper();
            int len = hash.Length;

            string ch = hash.Substring(len - 1, 1);
            Console.WriteLine(ch);

            int really = 0;
            //1:单，2:双
            foreach (char small in smalls)
            {
                if (small.ToString() == ch)
                {
                    really = 1;
                    break;
                }
            }

            foreach (char big in bigs)
            {
                if (big.ToString() == ch)
                {
                    really = 2;
                    break;
                }
            }

            if (guess == really)
                ret = 1;
            else
                ret = 2;
            return ret;
        }

        internal JObject setSignForAdd(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string assetID, string addr)
        {
            JObject ret = new JObject();

            DateTime dt = DateTime.Now;
            dt.AddHours(-8);
            string str = dt.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
            Console.WriteLine(str);
            string findFliter = "{asset:'" + assetID + "',addr:'" + addr + "',key:'" + str + "'}";
            BsonDocument queryBson = BsonDocument.Parse(findFliter);

            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collectionPro = database.GetCollection<NEP55.GoodsSign>("goodsSign");
            List<NEP55.GoodsSign> queryBsonList = collectionPro.Find(queryBson).ToList();
            
            bool result = false;
            if (queryBsonList.Count >= 1)
            {
                NEP55.GoodsSign sign = queryBsonList[0];
                sign.status = 1;
                collectionPro.ReplaceOne(queryBson, sign);
                result = true;
            }
            ret.Add("result", result);
            return ret;
        }

        internal JObject setGuessResult(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl,string assetID,string addr, int guess,int mount,string wif)
        {
            JObject ret = new JObject();
            DateTime dt = DateTime.Now;
            string str = dt.ToString();

            bool result = false;
         
            JObject result2 = sendTransfer(mongodbConnStr,mongodbDatabase,assetID,neoCliJsonRPCUrl,addr,mount,wif);
            if ((bool)result2["sendrawtransactionresult"]) {
                string txid = (string)result2["txid"];
                //存入猜测数据
                var client = new MongoClient(mongodbConnStr);
                var database = client.GetDatabase(mongodbDatabase);
                //当前高度+1
                long height = (long)(mh.GetData(mongodbConnStr, mongodbDatabase, "system_counter", "{counter:'block'}")[0]["lastBlockindex"]) + 1;

                NEP55.GoodsGuess gu = new NEP55.GoodsGuess(assetID,str,addr,txid,height,guess,mount,DateTime.Now);
                var collectionPro = database.GetCollection<NEP55.GoodsGuess>("goodsGuess");
                collectionPro.InsertOne(gu);
                result = true;
            }
            ret.Add("result", result);
            return ret;
        }

        private string getAccountName(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string assetID, string addr) {
            string name = "";
            string findFliter = "{asset:'" + assetID +"',addr:'"+addr+"',type:1}";
            JArray result = mh.GetData(mongodbConnStr, mongodbDatabase, "accOperator", findFliter);
            if (result.Count > 0) {
                name = (string)result[0]["name"];
            }
            return name;
        }
        

        private JArray getApproveDetailList(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string assetID, string addr, string username, string addDest, string nameDest)
        {
            JArray ret = new JArray();

            string findFliter = "{'asset':'" + assetID + "','from':'" + addr + "','nameSrc':'" + username+"','to':'" + addDest+ "','nameDest':'" + nameDest+"'}";
            JArray arrays = mh.GetData(mongodbConnStr, mongodbDatabase, "accApproveOperator", findFliter);

            Dictionary<string, string> names = new Dictionary<string, string>();
            foreach (JObject ob in arrays)
            {
                string assetType = (string)ob["assetType"];
                string assetName = (string)ob["assetName"];
                if (!names.ContainsKey(assetName))
                {
                    names.Add(assetName, "1");
                    JObject balance = new JObject();
                    balance.Add("assetType", assetType);
                    balance.Add("assetName", assetName);

                    string script = invokeScript(new Hash160(assetID), "getApproveInfo", "(str)" + username,"(str)"+nameDest,"(str)1","(str)"+ assetType, "(str)"+ assetName);
                    JObject jo = ct.invokeScript(neoCliJsonRPCUrl, script);
                    string balanceType = (string)((JArray)jo["stack"])[0]["type"];
                    string balanceStr = (string)((JArray)jo["stack"])[0]["value"];
                    string approveTotal = NEO_Block_API.NEP5.getNumStrFromStr(balanceType, balanceStr, 8);

                    script = invokeScript(new Hash160(assetID), "getApproveInfo", "(str)" + username, "(str)" + nameDest, "(str)2", "(str)" + assetType, "(str)" + assetName);
                    jo = ct.invokeScript(neoCliJsonRPCUrl, script);
                    balanceType = (string)((JArray)jo["stack"])[0]["type"];
                    balanceStr = (string)((JArray)jo["stack"])[0]["value"];
                    string approveSingle = NEO_Block_API.NEP5.getNumStrFromStr(balanceType, balanceStr, 8);

                    script = invokeScript(new Hash160(assetID), "getApproveInfo", "(str)" + username, "(str)" + nameDest, "(str)3", "(str)" + assetType, "(str)" + assetName);
                    jo = ct.invokeScript(neoCliJsonRPCUrl, script);
                    balanceType = (string)((JArray)jo["stack"])[0]["type"];
                    balanceStr = (string)((JArray)jo["stack"])[0]["value"];
                    string approveHz = NEO_Block_API.NEP5.getNumStrFromStr(balanceType, balanceStr, 0);

                    balance.Add("total",approveTotal);
                    balance.Add("single",approveSingle);
                    balance.Add("hz",approveHz);
                    ret.Add(balance);
                }
            }
            return ret;
        }

        internal JObject getAccAppDetail(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string assetID, string addr, string username)
        {
            JObject ret = new JObject();
            //统计已经被授权的app个数
            string findFliter = "{'asset':'" + assetID + "','to':'" + addr + "','nameDest':'" + username + "'}";
            JArray arrays = mh.GetData(mongodbConnStr, mongodbDatabase, "accApproveOperator", findFliter);
            Dictionary<string, string> names = new Dictionary<string, string>();
            JArray auths = new JArray();
            foreach (JObject ob in arrays)
            {
                string nameSrc = (string)ob["nameSrc"];
                string addSrc = (string)ob["from"];
                if (!names.ContainsKey(nameSrc))
                {
                    names.Add(nameSrc, "1");
                    JObject balance = new JObject();
                    balance.Add("name", nameSrc);
                    balance.Add("addr", addSrc);

                    auths.Add(balance);
                }
            }
            ret.Add("auths", auths.Count());

            //统计划转交易量，按照不同资产计算
            findFliter = "{$or:[{'from':'" + addr + "'},{'to':'" + addr + "'}]," + "asset:'" + assetID + "'}";

            arrays = mh.GetData(mongodbConnStr, mongodbDatabase, "accUserTransfer", findFliter);
            Dictionary<string, string> assetNames = new Dictionary<string, string>();
            JArray assetTotal = new JArray();
            foreach (JObject ob in arrays)
            {
                string assetName = (string)ob["assetName"];
                string assetType = (string)ob["assetType"];
                string key = assetType + assetName;
                if (!assetNames.ContainsKey(key))
                {
                    assetNames.Add(key, "1");

                    JObject balance = new JObject();
                    balance.Add("assetName", assetName);
                    balance.Add("assetType", assetType);
                    balance.Add("assetTotal",getAccAppTransferTotal(mongodbConnStr,mongodbDatabase,assetID,addr,assetName,assetType));
                    assetTotal.Add(balance);
                }
            }
            ret.Add("assetTotal", assetTotal);
            return ret;
        }

        private decimal getAccAppTransferTotal(string mongodbConnStr, string mongodbDatabase, string assetID, string addr, string assetName, string assetType)
        {
            string findFliter = "{asset:'" + assetID + "',assetName:'" + assetName + "',assetType:'" + assetType + "',$or:[{'to':'" + addr + "'},{'from':'" + addr + "'}]}";
            JArray arrays = mh.GetData(mongodbConnStr, mongodbDatabase, "accUserTransfer", findFliter);
            Dictionary<string, string> assetNames = new Dictionary<string, string>();
            JArray assetTotal = new JArray();
            decimal ret = 0;
            foreach (JObject ob in arrays)
            {
                string value = (string)ob["value"];
                ret = ret + decimal.Parse(value);
            }
            return ret;
        }

        internal JArray getAccBalanceByAdd(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string assetID, string addr,string username)
        {
            JArray ret = new JArray();
            string findFliter = "{'asset':'" + assetID + "','addr':'"+addr+ "',$or:[{'type':2},{'type':3}]}";
            JArray arrays = mh.GetData(mongodbConnStr, mongodbDatabase, "accOperator",findFliter);
            Dictionary<string, string> names = new Dictionary<string, string>();
            foreach (JObject ob in arrays) {
                string type = "";
                string assetName = (string)ob["name"];
                string simpleName = assetName;
                if (!names.ContainsKey(assetName)) {
                    string balanceBigint = "";
                    if (assetName.StartsWith("SD-"))
                    {
                        string script = invokeScript(new Hash160(assetID), "getBalance", "(str)" + username, "(str)nep55", "(str)" + assetName);
                        JObject jo = ct.invokeScript(neoCliJsonRPCUrl, script);
                        string balanceType = (string)((JArray)jo["stack"])[0]["type"];
                        string balanceStr = (string)((JArray)jo["stack"])[0]["value"];
                        balanceBigint = NEO_Block_API.NEP5.getNumStrFromStr(balanceType, balanceStr, 8);
                        type = "nep55";
                    }
                    //nep5资产默认sdusd
                    else if (assetName == "sdusd_account")
                    {
                        string script = invokeScript(new Hash160(assetID), "getBalance", "(str)" + username, "(str)nep5", "(str)sdusd_account");
                        JObject jo = ct.invokeScript(neoCliJsonRPCUrl, script);
                        string balanceType = (string)((JArray)jo["stack"])[0]["type"];
                        string balanceStr = (string)((JArray)jo["stack"])[0]["value"];
                        balanceBigint = NEO_Block_API.NEP5.getNumStrFromStr(balanceType, balanceStr, 8);
                        type = "nep5";
                        simpleName = "SDUSD";
                    }
                    else if (assetName == "sds_account")
                    {
                        string script = invokeScript(new Hash160(assetID), "getBalance", "(str)" + username, "(str)nep5", "(str)sds_account");
                        JObject jo = ct.invokeScript(neoCliJsonRPCUrl, script);
                        string balanceType = (string)((JArray)jo["stack"])[0]["type"];
                        string balanceStr = (string)((JArray)jo["stack"])[0]["value"];
                        balanceBigint = NEO_Block_API.NEP5.getNumStrFromStr(balanceType, balanceStr, 8);
                        type = "nep5";
                        simpleName = "SDS";
                    }

                    names.Add(assetName, "1");
                    JObject balance = new JObject();
                    balance.Add("name", simpleName);
                    balance.Add("type",type);
                    balance.Add("balance", balanceBigint);
                    ret.Add(balance);
                }
            }

            return ret;
        }

        private string getNep5Balance(string neoCliJsonRPCUrl,string NEP5scripthash, string NEP5address) {
            string balanceBigint = "0";
            try
            {
                byte[] NEP5addrHash = ThinNeo.Helper.GetPublicKeyHashFromAddress(NEP5address);
                string NEP5addrHashHex = ThinNeo.Helper.Bytes2HexString(NEP5addrHash.Reverse().ToArray());
                JObject NEP5balanceOfJ = ct.callContractForTest(neoCliJsonRPCUrl, new List<string> { NEP5scripthash }, new JArray() { JArray.Parse("['(str)balanceOf',['(hex)" + NEP5addrHashHex + "']]") });
                string balanceStr = (string)((JArray)NEP5balanceOfJ["stack"])[0]["value"];
                string balanceType = (string)((JArray)NEP5balanceOfJ["stack"])[0]["type"];

                balanceBigint = "0";

                if (balanceStr != string.Empty)
                {
                    balanceBigint = NEP5.getNumStrFromStr(balanceType, balanceStr, 8);
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            return balanceBigint;
        }

        internal JObject getHandOutRecord(string mongodbConnStr, string mongodbDatabase, string neoCliJsonRPCUrl, string asset) {
            JObject ret = new JObject();
            string findFliter = "{asset:'"+asset+"'}";
            JArray arrs = mh.GetData(mongodbConnStr, mongodbDatabase, "operatedLockAddr", findFliter);
            List<string> addrs = new List<string>();

            JArray addrList = new JArray();
            decimal total = new decimal(0.0);
            foreach (JObject vout in arrs)
            {
                string addr = (string)vout["addr"];
               
                if (!addrs.Contains(addr))
                {
                    JObject lockedOb = new JObject();
                    addrs.Add(addr);

                    decimal curr = new decimal(0.0);
                  
                    decimal locko1 = getLockedByType(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, asset, addr, LOCK_01);
                    decimal locko2 = getLockedByType(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, asset, addr, LOCK_02);
                    decimal locko3 = getLockedByType(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, asset, addr, LOCK_03);
                    decimal locko4 = getLockedByType(mongodbConnStr, mongodbDatabase, neoCliJsonRPCUrl, asset, addr, LOCK_04);
                    if (locko1.CompareTo(total) != 0)
                    {
                        total = total + locko1 * rate_01;
                        curr = curr + locko1 * rate_01;
                    }
                    if (locko2.CompareTo(total) != 0)
                    {
                        total = total + locko2 * rate_02;
                        curr = curr + locko2 * rate_02;
                    }
                    if (locko3.CompareTo(total) != 0)
                    {
                        total = total + locko3 * rate_03;
                        curr = curr + locko3 * rate_03;

                    }
                    if (locko4.CompareTo(total) != 0)
                    {
                        total = total + locko4 * rate_04;
                        curr = curr + locko4 * rate_04;
                    }

                    lockedOb.Add("addr",addr);
                    lockedOb.Add("lockedShare",curr);
                    addrList.Add(lockedOb);
                }
            }

            //进行批次存入
            string id = DateTime.Now.ToString("u");

            //批量处理地址份额存入数据库
            foreach (JObject curr in addrList)
            {
                string addr = (string)curr["addr"];
                decimal lockedShare = (decimal)curr["lockedShare"];
                var client = new MongoClient(mongodbConnStr);
                var database = client.GetDatabase(mongodbDatabase);
                BonusRecord re = new BonusRecord(id,addr,lockedShare, total, DateTime.Now);
                var collectionPro = database.GetCollection<BonusRecord>("BonusRecord");
                collectionPro.InsertOne(re);
            }
            ret.Add("result",id);
            return ret;
            
        }
    }
}
