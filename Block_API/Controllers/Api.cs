using NEO_Block_API.lib;
using NEO_Block_API.RPC;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

namespace NEO_Block_API.Controllers
{
    public class Api
    {
        private string netnode { get; set; }
        private string mongodbConnStr { get; set; }
        private string mongodbDatabase { get; set; }
        private string neoCliJsonRPCUrl { get; set; }

        httpHelper hh = new httpHelper();
        mongoHelper mh = new mongoHelper();
        Transaction tx = new Transaction();
        Contract ct = new Contract();
        Claim claim = new Claim();

        public Api(string node) {
            netnode = node;
            switch (netnode) {
                case "testnet":
                    mongodbConnStr = mh.mongodbConnStr_testnet;
                    mongodbDatabase = mh.mongodbDatabase_testnet;
                    neoCliJsonRPCUrl = mh.neoCliJsonRPCUrl_testnet;
                    break;
                case "mainnet":
                    mongodbConnStr = mh.mongodbConnStr_mainnet;
                    mongodbDatabase = mh.mongodbDatabase_mainnet;
                    neoCliJsonRPCUrl = mh.neoCliJsonRPCUrl_mainnet;
                    break;
                case "privatenet":
                    mongodbConnStr = mh.mongodbConnStr_privatenet;
                    mongodbDatabase = mh.mongodbDatabase_privatenet;
                    neoCliJsonRPCUrl = mh.neoCliJsonRPCUrl_privatenet;
                    break;
                case "swnet":
                    mongodbConnStr = mh.mongodbConnStr_swnet;
                    mongodbDatabase = mh.mongodbDatabase_swnet;
                    neoCliJsonRPCUrl = mh.neoCliJsonRPCUrl_swnet;
                    break;
            }
        }

        private JArray getJAbyKV(string key, object value)
        {
            return  new JArray
                        {
                            new JObject
                            {
                                {
                                    key,
                                    value.ToString()
                                }
                            }
                        };
        }

        private JArray getJAbyJ(JObject J)
        {
            return new JArray
                        {
                            J
                        };
        }

        public object getRes(JsonRPCrequest req,string reqAddr)
        {
            JArray result = new JArray();
            string resultStr = string.Empty;
            string findFliter = string.Empty;
            string sortStr = string.Empty;
            try
            {
                switch (req.method)
                {
                    case "getnodetype":
                        JArray JA = new JArray
                        {
                            new JObject {
                                { "nodeType",netnode }
                            }
                        };
                        result = JA;
                        break;
                    case "getcliversion":
                        result = getJAbyKV("cliversion", (string)JObject.Parse(hh.Post(neoCliJsonRPCUrl, "{'jsonrpc':'2.0','method':'getversion','params':[],'id':1}", System.Text.Encoding.UTF8, 1))["result"]["useragent"]);
                        break;
                    case "getclirawmempool":
                        JObject rawmempoolJ = new JObject();
                        rawmempoolJ.Add("clirawmempool", JObject.Parse(hh.Post(neoCliJsonRPCUrl, "{'jsonrpc':'2.0','method':'getrawmempool','params':[],'id':1}", System.Text.Encoding.UTF8, 1))["result"]);
                        result = getJAbyJ(rawmempoolJ);
                        break;
                    case "getdatablockheight":
                        result = mh.Getdatablockheight(mongodbConnStr, mongodbDatabase);
                        break;
                    case "getblockcount":
                        result = getJAbyKV("blockcount", (long)(mh.GetData(mongodbConnStr, mongodbDatabase, "system_counter", "{counter:'block'}")[0]["lastBlockindex"]) + 1);
                        break;
                    case "getsar4ccount":
                        findFliter = "{}";
                        if (req.@params.Count() == 1)
                        {
                            string status = req.@params[0].ToString();
                            findFliter = "{status:" + status + "}";
                        }else if (req.@params.Count() == 2) {
                            string status = req.@params[0].ToString();
                            string assetId = req.@params[1].ToString();
                            findFliter = "{status:" + status +",asset:'"+assetId+ "'}";
                        }

                        result = getJAbyKV("sar4ccount", mh.GetDataCount(mongodbConnStr, mongodbDatabase, "SAR4C",findFliter));
                        break;
                    case "getsar4bcount":
                        findFliter = "{}";
                        if (req.@params.Count() > 0)
                        {
                            string status = req.@params[0].ToString();
                            findFliter = "{status:" + status + "}";
                        }
                        result = getJAbyKV("sar4bcount", mh.GetDataCount(mongodbConnStr, mongodbDatabase, "SAR4B", findFliter));
                        break;
                   
                    case "getcliblockcount":
                        var resp = hh.Post(neoCliJsonRPCUrl, "{'jsonrpc':'2.0','method':'getblockcount','params':[],'id':1}", System.Text.Encoding.UTF8, 1);

                        string cliResultStr = (string)JObject.Parse(resp)["result"];
                        result = getJAbyKV("cliblockcount", cliResultStr);
                        break;
                    case "gettxcount":
                        //resultStr = "[{txcount:" + mh.GetDataCount(mongodbConnStr, mongodbDatabase, "tx") + "}]";
                        //result = getJAbyKV("txcount", mh.GetDataCount(mongodbConnStr, mongodbDatabase, "tx"));
                        findFliter = "{}";
                        if (req.@params.Count() > 0)
                        {
                            string type = req.@params[0].ToString();
                            if (type != null && type != string.Empty)
                            {
                                findFliter = "{type:\"" + type + "\"}";
                            }
                        }
                        result = getJAbyKV("txcount", mh.GetDataCount(mongodbConnStr, mongodbDatabase, "tx", findFliter));
                        break;
                    case "getaddrcount":
                        //resultStr = "[{addrcount:" + mh.GetDataCount(mongodbConnStr, mongodbDatabase, "address") + "}]";
                        result = getJAbyKV("addrcount", mh.GetDataCount(mongodbConnStr, mongodbDatabase, "address"));
                        break;
                    case "getblock":
                        findFliter = "{index:" + req.@params[0] + "}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "block", findFliter);
                        break;
                    case "getblocktime":
                        findFliter = "{index:" + req.@params[0] + "}";
                        var time = (Int32)mh.GetData(mongodbConnStr, mongodbDatabase, "block", findFliter)[0]["time"];
                        result = getJAbyKV("time", time);
                        break;
                    case "getblocks":
                        sortStr = "{index:-1}";
                        result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "block", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()));
                        break;
                    case "getrawtransaction":
                        findFliter = "{txid:'" + ((string)req.@params[0]).formatHexStr() + "'}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "tx", findFliter);
                        break;
                    case "getrawtransactions":
                        sortStr = "{blockindex:-1,txid:-1}";
                        findFliter = "{}";
                        if (req.@params.Count() > 2)
                        {
                            string txType = req.@params[2].ToString();

                            if (txType != null && txType != string.Empty)
                            { findFliter = "{type:'" + txType + "'}"; }
                        }
                        result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "tx", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()), findFliter);
                        break;
                    case "getaddrs":
                        sortStr = "{'lastuse.blockindex' : -1,'lastuse.txid' : -1}";
                        result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "address", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()));
                        break;
                    case "getaddr":
                        string addr = req.@params[0].ToString();
                        findFliter = "{addr:'" + addr + "'}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "address", findFliter);
                        break;
                    case "getaddresstxs":
                        string findBson = "{'addr':'" + req.@params[0].ToString() + "'}";
                        sortStr = "{'blockindex' : -1}";
                        result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "address_tx", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findBson);
                        break;
                    case "getasset":
                        findFliter = "{id:'" + ((string)req.@params[0]).formatHexStr() + "'}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "asset", findFliter);
                        break;
                    case "getallasset":
                        findFliter = "{}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "asset", findFliter);
                        break;
                    case "getfulllog":
                        findFliter = "{txid:'" + ((string)req.@params[0]).formatHexStr() + "'}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "fulllog", findFliter);
                        break;
                    case "getnotify":
                        findFliter = "{txid:'" + ((string)req.@params[0]).formatHexStr() + "'}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "notify", findFliter);
                        break;
                    case "getutxo":
                        if (req.@params.Count() == 1)
                        {
                            findFliter = "{addr:'" + req.@params[0] + "',used:''}";
                            result = mh.GetData(mongodbConnStr, mongodbDatabase, "utxo", findFliter);
                        }
                        else if (req.@params.Count() == 2)
                        {
                            if ((Int64)req.@params[1] == 1)
                            {
                                findFliter = "{addr:'" + req.@params[0] + "'}";
                            }
                            result = mh.GetData(mongodbConnStr, mongodbDatabase, "utxo", findFliter);
                        }
                        else if (req.@params.Count() == 3)
                        {
                            findFliter = "{addr:'" + req.@params[0] + "',used:''}";
                            sortStr = "{'createHeight':1,'txid':1,'n':1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "utxo", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                        }
                        else if (req.@params.Count() == 4)
                        {
                            if ((Int64)req.@params[1] == 1)
                            {
                                findFliter = "{addr:'" + req.@params[0] + "'}";
                            }
                            sortStr = "{'createHeight':1,'txid':1,'n':1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "utxo", sortStr, int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), findFliter);
                        }
                        break;
                    case "getutxocount":
                        addr = req.@params[0].ToString();
                        if (addr != null && addr != string.Empty)
                        {
                            findFliter = "{addr:\"" + addr + "\"}";
                        }
                        result = getJAbyKV("utxocount", mh.GetDataCount(mongodbConnStr, mongodbDatabase, "utxo", findFliter));
                        break;
                    case "getutxostopay":
                        string address = (string)req.@params[0];
                        string assetID = ((string)req.@params[1]).formatHexStr();
                        decimal amount = decimal.Parse(req.@params[2].ToString(),NumberStyles.Float);
                        bool isBigFirst = false; //默认先用小的。

                        if (req.@params.Count() == 4)
                        {
                            if ((Int64)req.@params[3] == 1)
                            {
                                isBigFirst = true;//加可选参数可以先用大的。
                            }
                        }

                        findFliter = "{addr:'" + address + "',used:''}";
                        JArray utxoJA = mh.GetData(mongodbConnStr, mongodbDatabase, "utxo", findFliter);

                        result = tx.getUtxo2Pay(utxoJA, address, assetID, amount, isBigFirst);
                        break;
                    case "getclaimgas":
                        JObject claimsJ = new JObject();
                        if (req.@params.Count() == 1)
                        {
                            claimsJ = claim.getClaimGas(mongodbConnStr, mongodbDatabase, req.@params[0].ToString());
                        };
                        if (req.@params.Count() == 2)
                        {
                            if ((Int64)req.@params[1] == 1)
                            {
                                claimsJ = claim.getClaimGas(mongodbConnStr, mongodbDatabase, req.@params[0].ToString(), false);
                            }
                        }
                        result = getJAbyJ(claimsJ);
                        break;
                    case "getclaimtxhex":
                        string addrClaim = (string)req.@params[0];
                        JObject claimgasJ = claim.getClaimGas(mongodbConnStr, mongodbDatabase, addrClaim);
                        if (claimgasJ["errorCode"] != null)
                        {
                            result = getJAbyJ(claimgasJ);
                        }
                        else
                        {
                            result = getJAbyKV("claimtxhex", tx.getClaimTxHex(addrClaim, claimgasJ));
                        }

                        break;
                    case "getbalance":
                        findFliter = "{addr:'" + req.@params[0] + "',used:''}";
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
                        result = balanceJA;
                        break;
                    case "getcontractscript":
                        findFliter = "{hash:'" + ((string)req.@params[0]).formatHexStr() + "'}";
                        result = mh.GetData(mh.mongodbConnStr_NeonOnline, mh.mongodbDatabase_NeonOnline, "contractWarehouse", findFliter);
                        break;
                    case "gettransfertxhex":
                        string addrOut = (string)req.@params[0];
                        findFliter = "{addr:'" + addrOut + "',used:''}";
                        JArray outputJA = mh.GetData(mongodbConnStr, mongodbDatabase, "utxo", findFliter);

                        result = getJAbyKV("transfertxhex", tx.getTransferTxHex(outputJA, (string)req.@params[0], (string)req.@params[1], (string)req.@params[2], decimal.Parse(req.@params[3].ToString(),NumberStyles.Float)));

                        //result = new JArray
                        //{
                        //    new JObject
                        //    {
                        //        {
                        //            "transfertxhex",
                        //            tx.getTransferTxHex(outputJA,(string)req.@params[0], (string)req.@params[1], (string)req.@params[2], decimal.Parse(req.@params[3].ToString()))
                        //        }
                        //    }
                        //};
                        break;
                    case "sendtxplussign":
                        result = getJAbyJ(tx.sendTxPlusSign(neoCliJsonRPCUrl, (string)req.@params[0], (string)req.@params[1], (string)req.@params[2]));
                        break;
                    case "verifytxsign":
                        result = getJAbyKV("sign", tx.verifyTxSign((string)req.@params[0], (string)req.@params[1]));
                        break;
                    case "sendrawtransaction":
                        result = getJAbyJ(tx.sendrawtransaction(neoCliJsonRPCUrl, (string)req.@params[0]));

                        //result = new JArray
                        //{
                        //    new JObject
                        //    {
                        //        {
                        //            "sendrawtransactionresult",
                        //            tx.sendrawtransaction(neoCliJsonRPCUrl,(string)req.@params[0])
                        //        }
                        //    }
                        //};
                        break;
                    case "getcontractstate":
                        result = getJAbyJ(ct.getContractState(neoCliJsonRPCUrl, (string)req.@params[0]));

                        break;
                    case "invokescript":
                        result = getJAbyJ(ct.invokeScript(neoCliJsonRPCUrl, (string)req.@params[0]));

                        break;
                    case "callcontractfortest":
                        result = getJAbyJ(ct.callContractForTest(neoCliJsonRPCUrl, new List<string> { (string)req.@params[0] }, new JArray() { (JArray)req.@params[1] }));

                        break;
                    case "callnep55common":
                        result = getJAbyJ(ct.callnep55common(neoCliJsonRPCUrl,new List<string> { (string)req.@params[0]},new JArray() { (JArray)req.@params[1]}));
                        break;
                    case "publishcontractfortest":
                        result = getJAbyJ(ct.publishContractForTest(neoCliJsonRPCUrl, (string)req.@params[0], (JObject)req.@params[1]));
                        break;
                    case "getinvoketxhex":
                        string addrPayFee = (string)req.@params[0];
                        findFliter = "{addr:'" + addrPayFee + "',used:''}";
                        JArray outputJAPayFee = mh.GetData(mongodbConnStr, mongodbDatabase, "utxo", findFliter);

                        string invokeScript = (string)req.@params[1];
                        decimal invokeScriptFee = decimal.Parse(req.@params[2].ToString(),NumberStyles.Float);

                        result = getJAbyKV("invoketxhex", tx.getInvokeTxHex(outputJAPayFee, addrPayFee, invokeScript, invokeScriptFee));
                        break;
                    case "getstorage":
                        result = getJAbyJ(ct.getStorage(neoCliJsonRPCUrl, (string)req.@params[0], (string)req.@params[1]));

                        break;
                    case "setcontractscript":
                        JObject J = JObject.Parse((string)req.@params[0]);
                        string hash = (string)J["hash"];
                        //string hash = (string)req.@params[0];
                        //J.Add("hash", hash);
                        //J.Add("avm", (string)req.@params[1]);
                        //J.Add("cs", (string)req.@params[2]);

                        //string mapStr = (string)req.@params[3];
                        //string abiStr = (string)req.@params[4];

                        //if (mapStr != null && mapStr != string.Empty)
                        //{
                        //    J.Add("map", JArray.Parse((string)req.@params[3]));
                        //}
                        //else
                        //{
                        //    J.Add("map", string.Empty);
                        //}

                        //if (abiStr != null && abiStr != string.Empty)
                        //{
                        //    J.Add("abi", JObject.Parse((string)req.@params[4]));
                        //}
                        //else
                        //{
                        //    J.Add("abi", string.Empty);
                        //}

                        J.Add("requestIP", reqAddr);

                        mh.InsertOneDataByCheckKey(mh.mongodbConnStr_NeonOnline, mh.mongodbDatabase_NeonOnline, "contractWarehouse", J, "hash", hash);
                        result = getJAbyKV("isSetSuccess", true);

                        //result = new JArray
                        //{
                        //    new JObject{
                        //        { "isSetSuccess",true }
                        //    }
                        //};

                        break;
                    case "getnep5balanceofaddress":
                        string NEP5scripthash = (string)req.@params[0];
                        string NEP5address = (string)req.@params[1];
                        byte[] NEP5addrHash = ThinNeo.Helper.GetPublicKeyHashFromAddress(NEP5address);
                        string NEP5addrHashHex = ThinNeo.Helper.Bytes2HexString(NEP5addrHash.Reverse().ToArray());
                        JObject NEP5balanceOfJ = ct.callContractForTest(neoCliJsonRPCUrl,new List<string>{ NEP5scripthash }, new JArray() { JArray.Parse("['(str)balanceOf',['(hex)" + NEP5addrHashHex + "']]") });
                        string balanceStr = (string)((JArray)NEP5balanceOfJ["stack"])[0]["value"];
                        string balanceType = (string)((JArray)NEP5balanceOfJ["stack"])[0]["type"];

                        string balanceBigint = "0";

                        if (balanceStr != string.Empty)
                        {
                            //获取NEP5资产信息，获取精度
                            NEP5.Asset NEP5asset = new NEP5.Asset(mongodbConnStr, mongodbDatabase, NEP5scripthash);

                            balanceBigint = NEP5.getNumStrFromStr(balanceType,balanceStr, NEP5asset.decimals);
                        }

                        result = getJAbyKV("nep5balance", balanceBigint);
                        break;
                    case "getallnep5assetofaddress":
                        string NEP5addr = (string)req.@params[0];
                        bool isNeedBalance = false;
                        if (req.@params.Count() > 1)
                        {
                            isNeedBalance = ((Int64)req.@params[1] == 1) ? true : false;
                        }
                        
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
                                    select new { assetid = tftG.Key};
                        var assetAdds = queryTo.ToList();

                        //如果需要余额，则通过cli RPC批量获取余额
                        List<NEP5.AssetBalanceOfAddr> addrAssetBalances = new List<NEP5.AssetBalanceOfAddr>();
                        if (isNeedBalance) {
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

                                abt.balance = NEP5.getNumStrFromStr(allBalanceType,allBalanceStr, NEP5asset.decimals);
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

                        ////按资产汇集支出的钱
                        //string findTransferFrom = "{ from:'" + NEP5addr + "'}";
                        //JArray transferFromJA = mh.GetData(mongodbConnStr, mongodbDatabase, "NEP5transfer", findTransferFrom);
                        //List<NEP5.Transfer> tffs = new List<NEP5.Transfer>();
                        //foreach (JObject tfJ in transferFromJA)
                        //{
                        //    tffs.Add(new NEP5.Transfer(tfJ));
                        //}
                        //var queryFrom = from tff in tffs
                        //                group tff by tff.asset into tffG
                        //            select new { assetid = tffG.Key, sumOfValue = tffG.Sum(m => m.value) };
                        //var assetRemoves = queryFrom.ToList();

                        ////以支出的钱扣减收到的钱得到余额
                        //JArray JAadds = JArray.FromObject(assetAdds);
                        //foreach (JObject Jadd in JAadds) {
                        //    foreach (var assetRemove in assetRemoves)
                        //    {
                        //        if ((string)Jadd["assetid"] == assetRemove.assetid)
                        //        {
                        //            Jadd["sumOfValue"] = (decimal)Jadd["sumOfValue"] - assetRemove.sumOfValue;
                        //            break;
                        //        }
                        //    }
                        //}
                        //var a = Newtonsoft.Json.JsonConvert.SerializeObject(JAadds);

                        //***********
                        //经简单测试，仅看transfer记录，所有to减去所有from并不一定等于合约查询得到的地址余额(可能有其他非标方法消耗了余额，尤其是测试网)，废弃这种方法，还是采用调用NEP5合约获取地址余额方法的方式
                        //这里给出所有该地址收到过的资产hash，可以配合其他接口获取资产信息和余额
                        //***********
                        if (!isNeedBalance)
                        {
                            result = JArray.FromObject(assetAdds);
                        }
                        else
                        {
                            result = JArray.FromObject(addrAssetBalances);
                        }                   

                        break;
                    case "getallnep55assetofaddress":
                        string NEP55addr = (string)req.@params[0];
                        string NEP55asset = (string)req.@params[1];
                          isNeedBalance = false;
                        if (req.@params.Count() > 2)
                        {
                            isNeedBalance = ((Int64)req.@params[2] == 1) ? true : false;
                        }

                        //按资产汇集收到的钱(按照name)
                         findTransferTo = "{ to:'" + NEP55addr + "',asset:'"+ NEP55asset + "'}";
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
                                queryParams.Add(JArray.Parse("['(str)balanceOf',['(str)" + abt.name +"','(hex)"+ NEP5allAssetOfAddrHashHex + "']]"));
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

                        break;
                    case "getnep5asset":
                        findFliter = "{assetid:'" + ((string)req.@params[0]).formatHexStr() + "'}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "NEP5asset", findFliter);
                        break;
                    case "getallnep5asset":
                        findFliter = "{}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "NEP5asset", findFliter);
                        break;
                    case "getnep5transferbytxid":
                        string txid = ((string)req.@params[0]).formatHexStr();
                        findFliter = "{txid:'" + txid + "'}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "NEP5transfer", findFliter);
                        break;
                    case "getnep5transferbyaddresscount":
                        findFliter = "{}";
                        string NEP5transferAddress = string.Empty;
                        if (req.@params.Count() == 1)
                        {
                            NEP5transferAddress = (string)req.@params[0];
                            findFliter = "{$or:[{'from':'" + NEP5transferAddress + "'},{'to':'" + NEP5transferAddress + "'}]}";
                        }
                        else if (req.@params.Count() == 2)
                        {
                            NEP5transferAddress = (string)req.@params[0];
                            string asset = (string)req.@params[1];
                            findFliter = "{$or:[{'from':'" + NEP5transferAddress + "'},{'to':'" + NEP5transferAddress + "'}]," + "asset:'" + asset + "'}";
                        }
                        result = getJAbyKV("transfercount", mh.GetDataCount(mongodbConnStr, mongodbDatabase, "NEP5transfer", findFliter));
                        break;
                    case "getnep5transferbyaddress":
                        sortStr = "{'blockindex':-1}";
                        findFliter = "{}";
                        if (req.@params.Count() == 3)
                        {
                            NEP5transferAddress = (string)req.@params[0];
                            findFliter = "{$or:[{'from':'" + NEP5transferAddress + "'},{'to':'" + NEP5transferAddress + "'}]}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "NEP5transfer", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                        }
                        else if (req.@params.Count() == 4)
                        {
                            NEP5transferAddress = (string)req.@params[0];
                            string asset = (string)req.@params[1];
                            findFliter = "{$or:[{'from':'" + NEP5transferAddress + "'},{'to':'" + NEP5transferAddress + "'}]," + "asset:'" + asset + "'}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "NEP5transfer", sortStr, int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), findFliter);
                        }
                        break;
                    case "getnep5transferbyaddressSimple":
                        sortStr = "{'blockindex':1,'txid':1,'n':1}";
                        NEP5transferAddress = (string)req.@params[0];
                        string NEP5transferAddressType = (string)req.@params[1];
                        findFliter = "{'" + NEP5transferAddressType + "':'" + NEP5transferAddress + "'}";
                        result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "NEP5transfer", sortStr, int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), findFliter);
                        break;
                    case "getnep5transfers":
                        sortStr = "{'blockindex':1,'txid':1,'n':1}";
                        result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "NEP5transfer", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()));
                        break;
                    case "getnep5transfersbyasset":
                        string str_asset = ((string)req.@params[0]).formatHexStr();
                        findFliter = "{asset:'" + str_asset + "'}";
                        //sortStr = "{'blockindex':1,'txid':1,'n':1}";
                        sortStr = "{}";
                        if (req.@params.Count() == 3)
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "NEP5transfer", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                        else
                            result = mh.GetData(mongodbConnStr, mongodbDatabase, "NEP5transfer", findFliter);
                        break;
                    //获取所有nep55资产
                    case "getnep55asset":
                        findFliter = "{}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "NEP55asset", findFliter);
                        break;
                    case "getnep55transfersbyname":
                        string nep55_name = "";
                 
                        if (req.@params.Count() == 3)
                        {
                            nep55_name = (string)req.@params[0];
                            findFliter = "{name:'" + nep55_name + "'}";
                            sortStr = "{}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "transferSAR", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);

                        }
                        else if (req.@params.Count() == 4) {
                            nep55_name = (string)req.@params[0];
                            addr = (string)req.@params[1];
                            findFliter = "{name:'" + nep55_name + "',from:'" + addr + "'}";
                            sortStr = "{}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "transferSAR", sortStr, int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), findFliter);
                        }
                        else
                            result = mh.GetData(mongodbConnStr, mongodbDatabase, "transferSAR", findFliter);
                        break;
                    case "getnep55transfersbyaddresscount":
                        addr = (string)req.@params[0];
                        findFliter = "{$or:[{'from':'" + addr + "'},{'to':'" + addr + "'}]}";

                        result = getJAbyKV("transfercount", mh.GetDataCount(mongodbConnStr, mongodbDatabase, "transferSAR", findFliter));
                        break;
                    case "getnep55transfersbyaddress":
                        addr = (string)req.@params[0];
                        findFliter = "{$or:[{'from':'" + addr + "'},{'to':'" + addr + "'}]}";
                        sortStr = "{'blockindex':-1}";
                        result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "transferSAR", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
    
                        break;
                    case "getSAR4BByName":
                        string nep55_asset = (string)req.@params[0];
                        nep55_name = (string)req.@params[1];
                        findFliter = "{asset:'"+ nep55_asset + "',name:'"+nep55_name+"',status:1}";
                        sortStr ="{}";
                        result = mh.GetData(mongodbConnStr,mongodbDatabase,"SAR4B",findFliter);

                        break;
                    //根据用户名获取所有代币名称
                    case "getnep55nameByaddr":
                        if (req.@params.Count() == 1) {
                            addr = (string)req.@params[0];
                            findFliter = "{to:'" + addr + "'}";
                            sortStr = "{}";
                            JArray transfers = mh.GetData(mongodbConnStr,mongodbDatabase, "transferSAR", findFliter);

                            List<string> list = new List<string>();
                            JObject nameResult = null;

                            foreach (JObject tfJ in transfers)
                            {
                                list.Add((string)tfJ["name"]);
                            }

                            List<string> names = list.Distinct().ToList();
                      
                            string strs = "";
                            if (names.Count > 0)
                            {
                                foreach (string name in names)
                                {
                                    strs = name + "," + strs;
                                }
                                strs = strs.Substring(0,strs.Length-1);
                            }

                            nameResult = new JObject();
                            nameResult["name"] = strs;
                            result = new JArray() { nameResult };
                        }
                        break;
                    case "getsarOperatedByaddr":
                        if (req.@params.Count() == 4)
                        {
                            string sarTxid = (string)req.@params[0];
                            addr = (string)req.@params[1];
                            findFliter = "{sarTxid:'" + sarTxid + "',addr:'" + addr + "'}";
                            sortStr = "{}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operatedSAR", sortStr, int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), findFliter);
                        }
                        else if (req.@params.Count() == 3)
                        {
                            addr = (string)req.@params[0];
                            findFliter = "{addr:'" + addr + "'}";
                            sortStr = "{}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operatedSAR", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                        }
                        else
                        {
                            result = mh.GetData(mongodbConnStr, mongodbDatabase, "operatedSAR", findFliter);
                        }
                        break;
                    case "getsar4COperatedByaddr":
                        if (req.@params.Count() == 4)
                        {
                            addr = (string)req.@params[0];
                            findFliter = "{addr:'" + addr + "',type:" + int.Parse(req.@params[1].ToString()) + "}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operatedSAR4C", sortStr, int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), findFliter);
                        }
                        else if (req.@params.Count() == 3)
                        {
                            addr = (string)req.@params[0];
                            findFliter = "{addr:'" + addr + "'}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operatedSAR4C", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                        }
                        else
                        {
                            findFliter = "{}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operatedSAR4C", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()), findFliter);
                        }
                        break;
                    case "getsar4COperatedBySAR":
                        if (req.@params.Count() == 3)
                        {
                            string sarTxid = (string)req.@params[0];
                            findFliter = "{sarTxid:'" + sarTxid + "'}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operatedSAR4C", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                        }
                        else
                        {
                            findFliter = "{}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operatedSAR4C", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()), findFliter);
                        }
                        break;
                    case "getOperated4CByaddr":
                        if (req.@params.Count() == 4)
                        {
                            addr = (string)req.@params[0];
                            findFliter = "{addr:'" + addr + "',type:" + int.Parse(req.@params[1].ToString()) + ",status:1}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operated4C", sortStr, int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), findFliter);
                        }
                        else if (req.@params.Count() == 3)
                        {
                            addr = (string)req.@params[0];
                            findFliter = "{addr:'" + addr + "',status:1}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operated4C", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                        }
                        else
                        {
                            findFliter = "{}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operated4C", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()), findFliter);
                        }
                        break;
                    case "getsarListByType":
                        findFliter = "{type:" + int.Parse(req.@params[0].ToString()) + "}";
                        sortStr = "{'blockindex':-1}";
                        result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operatedSAR", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                        break;
                    case "getsar4BListByType":
                        findFliter = "{}";
                        sortStr = "{'blockindex':-1}";

                        if (req.@params.Count() > 0)
                        {
                            string status = req.@params[0].ToString();

                            findFliter = "{status:" + status + "}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "SAR4B", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                        }
                        else
                        {
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "SAR4B", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()), findFliter);
                        }
                        break;
                    case "getsar4CListByType":
                        findFliter = "{}";
                        sortStr = "{'blockindex':-1}";

                        if (req.@params.Count() == 3)
                        {
                            string status = req.@params[0].ToString();

                            findFliter = "{status:" + status + "}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "SAR4C", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                        }
                        else if (req.@params.Count() == 4)
                        {
                            string status = req.@params[0].ToString();
                            string assetId = req.@params[1].ToString();

                            findFliter = "{status:" + status + ",asset:'"+assetId+"'}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "SAR4C", sortStr, int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), findFliter);
                        }
                        else {
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "SAR4C", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()), findFliter);
                        }
                        break;
                    case "getOracleOperated":
                        if (req.@params.Count() == 5)
                        {
                            assetID = (string)req.@params[0];
                            addr = (string)req.@params[1];

                            findFliter = "{asset:'" + assetID +"',addr:'"+ addr+"',type:" + int.Parse(req.@params[2].ToString()) + "}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operatedOracle", sortStr, int.Parse(req.@params[3].ToString()), int.Parse(req.@params[4].ToString()), findFliter);
                        }
                        else if (req.@params.Count() == 4)
                        {
                            assetID = (string)req.@params[0];
                            addr = (string)req.@params[1];

                            findFliter = "{asset:'" + assetID + "',addr:'" + addr + "'}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operatedOracle", sortStr, int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), findFliter);
                        }
                        else
                        {
                            findFliter = "{}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "operatedOracle", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()), findFliter);
                        }
                        break;
                    case "getnep5count":
                        findFliter = "{}";
                        if (req.@params.Count() == 2)
                        {
                            string key = (string)req.@params[0];
                            string value = (string)req.@params[1];
                            findFliter = "{\"" + key + "\":\"" + value + "\"}";
                        }
                        result = getJAbyKV("nep5count", mh.GetDataCount(mongodbConnStr, mongodbDatabase, "NEP5transfer", findFliter));
                        break;
                    case "getnep5transferbyblockindex":
                        Int64 blockindex = (Int64)req.@params[0];
                        findFliter = "{blockindex:" + blockindex + "}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "NEP5transfer", findFliter);
                        break;
                    case "getaddresstxbyblockindex":
                        blockindex = (Int64)req.@params[0];
                        findFliter = "{blockindex:" + blockindex + "}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "address_tx", findFliter);
                        break;
                    case "gettxinfo":
                        txid = ((string)req.@params[0]).formatHexStr();
                        findFliter = "{txid:'" + (txid).formatHexStr() + "'}";
                        JArray JATx = mh.GetData(mongodbConnStr, mongodbDatabase, "tx", findFliter);
                        JObject JOTx = (JObject)JATx[0];
                        var heightforblock = (int)JOTx["blockindex"];
                        var indexforblock = -1;
                        findFliter = "{index:" + heightforblock + "}";
                        result = (JArray)mh.GetData(mongodbConnStr, mongodbDatabase, "block", findFliter)[0]["tx"];
                        for (var i = 0; i < result.Count; i++)
                        {
                            JObject Jo = (JObject)result[i];
                            if (txid == (string)Jo["txid"])
                            {
                                indexforblock = i;
                            }
                        }
                        JObject JOresult = new JObject();
                        JOresult["heightforblock"] = heightforblock;
                        JOresult["indexforblock"] = indexforblock;
                        result = new JArray() { JOresult };
                        break;
                    case "uxtoinfo":
                        var starttxid = ((string)req.@params[0]).formatHexStr();
                        var voutN = (Int64)req.@params[1];

                        findFliter = "{txid:'" + (starttxid).formatHexStr() + "'}";
                        JATx = mh.GetData(mongodbConnStr, mongodbDatabase, "tx", findFliter);
                        JOTx = (JObject)JATx[0];
                        int starttxblockheight = (int)JOTx["blockindex"];
                        int starttxblockindex = -1;
                        findFliter = "{index:" + starttxblockheight + "}";
                        result = (JArray)mh.GetData(mongodbConnStr, mongodbDatabase, "block", findFliter)[0]["tx"];
                        for (var i = 0; i < result.Count; i++)
                        {
                            JObject Jo = (JObject)result[i];
                            if (starttxid == (string)Jo["txid"])
                            {
                                starttxblockindex = i;
                            }
                        }
                        //根据txid和n获取utxo信息
                        findFliter = "{txid:\"" + starttxid + "\",n:" + voutN + "}";
                        var endtxid = (string)mh.GetData(mongodbConnStr, mongodbDatabase, "utxo", findFliter)[0]["used"];
                        int endtxblockheight = -1;
                        int endtxblockindex = -1;
                        int vinputN = -1;
                        if (!string.IsNullOrEmpty(endtxid))
                        {
                            findFliter = "{txid:'" + (endtxid).formatHexStr() + "'}";
                            JATx = mh.GetData(mongodbConnStr, mongodbDatabase, "tx", findFliter);
                            JOTx = (JObject)JATx[0];
                            endtxblockheight = (int)JOTx["blockindex"];
                            JArray JAvin = (JArray)JOTx["vin"];
                            findFliter = "{index:" + endtxblockheight + "}";
                            result = (JArray)mh.GetData(mongodbConnStr, mongodbDatabase, "block", findFliter)[0]["tx"];
                            for (var i = 0; i < result.Count; i++)
                            {
                                JObject Jo = (JObject)result[i];
                                if (endtxid == (string)Jo["txid"])
                                {
                                    endtxblockindex = i;
                                }
                            }
                            for (var i = 0; i < JAvin.Count; i++)
                            {
                                JObject Jo = (JObject)JAvin[i];
                                if ((string)Jo["txid"] == starttxid && voutN == i)
                                {
                                    vinputN = i;
                                }
                            }

                        }
                        else
                        {
                        }

                        JOresult = new JObject();
                        JOresult["starttxid"] = starttxid;
                        JOresult["starttxblockheight"] = starttxblockheight;
                        JOresult["starttxblockindex"] = starttxblockindex;
                        JOresult["voutN"] = voutN;
                        JOresult["endtxid"] = endtxid;
                        JOresult["endtxblockheight"] = endtxblockheight;
                        JOresult["endtxblockindex"] = endtxblockindex;
                        JOresult["vinputN"] = vinputN;
                        result = new JArray() { JOresult };

                        break;
                    case "getTxByTxid":
                        findFliter = "{txid:'" + req.@params[0] + "'}";
                        result = mh.GetData(mongodbConnStr, mongodbDatabase, "tx", findFliter);
                        break;
                    case "getTx":
                        if (req.@params.Count() == 3)
                        {
                            string type = (string)req.@params[0];
                            findFliter = "{type:'" + type + "'}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "tx", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                        }
                        else {
                            findFliter = "{}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "tx", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()), findFliter);
                        }
                        break;
                    case "getTransByAddresscount":
                        if (req.@params.Count() == 1)
                        {
                            addr = (string)req.@params[0];
                            findFliter = "{addr:'" + addr + "'}";
                        }
                        else {
                            findFliter = "{}";
                        }
                        result = getJAbyKV("transcount", mh.GetDataCount(mongodbConnStr, mongodbDatabase, "address_tx", findFliter));
                        break;
                    case "getTransByAddress":
                        if (req.@params.Count() == 3)
                        {
                            addr = (string)req.@params[0];
                            findFliter = "{addr:'" + addr + "'}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "address_tx", sortStr, int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), findFliter);
                        }
                        else
                        {
                            findFliter = "{}";
                            sortStr = "{'blockindex':-1}";
                            result = mh.GetDataPages(mongodbConnStr, mongodbDatabase, "address_tx", sortStr, int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()), findFliter);
                        }
                        break;

                }
                if (result != null && result.Count > 0 && result[0]["errorCode"] != null)
                {
                    JsonPRCresponse_Error resE = new JsonPRCresponse_Error(req.id, (int)result[0]["errorCode"], (string)result[0]["errorMsg"], (string)result[0]["errorData"]);
                    return resE;
                }
                if (result.Count == 0)
                {
                    JsonPRCresponse_Error resE = new JsonPRCresponse_Error(req.id, -1, "No Data", "Data does not exist");

                    return resE;
                }
            }
            catch (Exception e)
            {
                JsonPRCresponse_Error resE = new JsonPRCresponse_Error(req.id, -100, "Parameter Error", e.Message);

                return resE;

            }

            JsonPRCresponse res = new JsonPRCresponse();
            res.jsonrpc = req.jsonrpc;
            res.id = req.id;
            res.result = result;

            return res;
        }
    }
}
